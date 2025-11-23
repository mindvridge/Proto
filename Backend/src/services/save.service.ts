import crypto from 'crypto';
import { Database } from '../database/connection';
import { RedisClient } from '../database/redis';
import { logger } from '../utils/logger';
import { AppError, ConflictError, NotFoundError } from '../utils/errors';
import config from '../config';

export interface GameSaveData {
  version: number;
  checksum: string;
  player_data: any;
  stage_data?: any;
  currency_data?: any;
  inventory_data?: any;
  achievement_data?: any;
  play_time?: number;
  device_id?: string;
  platform?: string;
  save_time?: string;
  force_override?: boolean;
}

export interface BackupInfo {
  backup_id: string;
  save_time: string;
  version: number;
  level: number;
  stage: number;
}

class SaveService {
  /**
   * Upload game save to server
   */
  async uploadSave(userId: string, saveData: GameSaveData): Promise<{ success: boolean; version: number }> {
    // Validate checksum
    const calculatedChecksum = this.calculateChecksum(saveData);
    if (!saveData.force_override && saveData.checksum !== calculatedChecksum) {
      logger.warn(`Checksum mismatch for user ${userId}`);
      throw new AppError('데이터 무결성 검증에 실패했습니다.', 400, 'CHECKSUM_MISMATCH');
    }

    return await Database.transaction(async (client) => {
      // Check existing save
      const existingResult = await client.query(
        'SELECT version FROM game_saves WHERE user_id = $1 FOR UPDATE',
        [userId]
      );

      const existingVersion = existingResult.rows[0]?.version || 0;

      // Version conflict check
      if (!saveData.force_override && existingVersion >= saveData.version) {
        // Get server data for conflict resolution
        const serverData = await client.query(
          'SELECT * FROM game_saves WHERE user_id = $1',
          [userId]
        );

        throw new ConflictError('서버에 더 최신 버전이 있습니다.');
      }

      const newVersion = Math.max(existingVersion + 1, saveData.version);

      if (existingResult.rows.length > 0) {
        // Update existing save
        await client.query(
          `UPDATE game_saves SET
             version = $2,
             checksum = $3,
             player_data = $4,
             stage_data = $5,
             currency_data = $6,
             inventory_data = $7,
             achievement_data = $8,
             play_time = $9,
             device_id = $10,
             platform = $11
           WHERE user_id = $1`,
          [
            userId,
            newVersion,
            calculatedChecksum,
            JSON.stringify(saveData.player_data),
            saveData.stage_data ? JSON.stringify(saveData.stage_data) : null,
            saveData.currency_data ? JSON.stringify(saveData.currency_data) : null,
            saveData.inventory_data ? JSON.stringify(saveData.inventory_data) : null,
            saveData.achievement_data ? JSON.stringify(saveData.achievement_data) : null,
            saveData.play_time || 0,
            saveData.device_id || null,
            saveData.platform || null,
          ]
        );
      } else {
        // Create new save
        await client.query(
          `INSERT INTO game_saves
             (user_id, version, checksum, player_data, stage_data, currency_data, inventory_data, achievement_data, play_time, device_id, platform)
           VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)`,
          [
            userId,
            newVersion,
            calculatedChecksum,
            JSON.stringify(saveData.player_data),
            saveData.stage_data ? JSON.stringify(saveData.stage_data) : null,
            saveData.currency_data ? JSON.stringify(saveData.currency_data) : null,
            saveData.inventory_data ? JSON.stringify(saveData.inventory_data) : null,
            saveData.achievement_data ? JSON.stringify(saveData.achievement_data) : null,
            saveData.play_time || 0,
            saveData.device_id || null,
            saveData.platform || null,
          ]
        );
      }

      // Update user level from save data
      if (saveData.player_data?.level) {
        await client.query(
          'UPDATE users SET level = $1 WHERE id = $2',
          [saveData.player_data.level, userId]
        );
      }

      // Invalidate cache
      await RedisClient.del(`save:${userId}`);

      logger.info(`Save uploaded for user ${userId}, version ${newVersion}`);

      return { success: true, version: newVersion };
    });
  }

  /**
   * Download game save from server
   */
  async downloadSave(userId: string): Promise<GameSaveData | null> {
    // Try cache first
    const cached = await RedisClient.get(`save:${userId}`);
    if (cached) {
      return JSON.parse(cached);
    }

    const result = await Database.query(
      `SELECT version, checksum, player_data, stage_data, currency_data,
              inventory_data, achievement_data, play_time, device_id, platform, updated_at
       FROM game_saves WHERE user_id = $1`,
      [userId]
    );

    if (result.rows.length === 0) {
      return null;
    }

    const row = result.rows[0];
    const saveData: GameSaveData = {
      version: row.version,
      checksum: row.checksum,
      player_data: row.player_data,
      stage_data: row.stage_data,
      currency_data: row.currency_data,
      inventory_data: row.inventory_data,
      achievement_data: row.achievement_data,
      play_time: row.play_time,
      device_id: row.device_id,
      platform: row.platform,
      save_time: row.updated_at?.toISOString(),
    };

    // Cache for 5 minutes
    await RedisClient.set(`save:${userId}`, JSON.stringify(saveData), 300);

    return saveData;
  }

  /**
   * Get backup list
   */
  async getBackups(userId: string): Promise<BackupInfo[]> {
    const result = await Database.query(
      `SELECT id, version, created_at, player_data
       FROM save_backups
       WHERE user_id = $1
       ORDER BY created_at DESC
       LIMIT 10`,
      [userId]
    );

    return result.rows.map((row) => ({
      backup_id: row.id,
      save_time: row.created_at?.toISOString(),
      version: row.version,
      level: row.player_data?.level || 0,
      stage: row.player_data?.stage_data?.current_stage || 0,
    }));
  }

  /**
   * Restore from backup
   */
  async restoreFromBackup(userId: string, backupId: string): Promise<void> {
    const backup = await Database.query(
      `SELECT * FROM save_backups WHERE id = $1 AND user_id = $2`,
      [backupId, userId]
    );

    if (backup.rows.length === 0) {
      throw new NotFoundError('백업');
    }

    const backupData = backup.rows[0];

    // Create backup of current save before restore
    await Database.query(
      `INSERT INTO save_backups (user_id, save_id, version, checksum, player_data, stage_data, currency_data, backup_reason)
       SELECT user_id, id, version, checksum, player_data, stage_data, currency_data, 'before_restore'
       FROM game_saves WHERE user_id = $1`,
      [userId]
    );

    // Restore
    await Database.query(
      `UPDATE game_saves SET
         version = version + 1,
         checksum = $2,
         player_data = $3,
         stage_data = $4,
         currency_data = $5
       WHERE user_id = $1`,
      [userId, backupData.checksum, backupData.player_data, backupData.stage_data, backupData.currency_data]
    );

    // Invalidate cache
    await RedisClient.del(`save:${userId}`);

    logger.info(`Save restored from backup ${backupId} for user ${userId}`);
  }

  /**
   * Delete save data
   */
  async deleteSave(userId: string): Promise<void> {
    await Database.transaction(async (client) => {
      await client.query('DELETE FROM save_backups WHERE user_id = $1', [userId]);
      await client.query('DELETE FROM game_saves WHERE user_id = $1', [userId]);
    });

    await RedisClient.del(`save:${userId}`);

    logger.info(`Save deleted for user ${userId}`);
  }

  /**
   * Calculate checksum for save data
   */
  private calculateChecksum(saveData: GameSaveData): string {
    const dataToHash = `${saveData.player_data?.level || 0}_${saveData.version}_${JSON.stringify(saveData.player_data)}`;

    return crypto
      .createHash('sha256')
      .update(dataToHash)
      .digest('hex');
  }
}

export const saveService = new SaveService();
export default saveService;
