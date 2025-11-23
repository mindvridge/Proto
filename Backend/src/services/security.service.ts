import crypto from 'crypto';
import { Database } from '../database/connection';
import { RedisClient } from '../database/redis';
import { logger } from '../utils/logger';
import { AppError } from '../utils/errors';
import config from '../config';

export type ViolationType = 'memory_tampering' | 'speed_hack' | 'data_tampering' | 'device_mismatch' | 'invalid_value' | 'abnormal_progression' | 'rooted_device' | 'modified_apk';
export type SeverityLevel = 'low' | 'medium' | 'high' | 'critical';
export type ActionType = 'warn' | 'suspend' | 'ban';

export interface IntegrityCheckRequest {
  device_fingerprint: string;
  session_token: string;
  data_hash: string;
  timestamp: string;
  app_version: string;
  platform: string;
}

export interface ViolationReport {
  violation_type: ViolationType;
  details: string;
  device_fingerprint: string;
  session_token: string;
  timestamp: string;
  violation_count: number;
}

class SecurityService {
  private readonly VIOLATION_THRESHOLD = {
    low: 10,
    medium: 5,
    high: 2,
    critical: 1,
  };

  /**
   * Perform integrity check
   */
  async checkIntegrity(userId: string, request: IntegrityCheckRequest, clientIp: string): Promise<{ is_valid: boolean; error_code?: string; error_message?: string }> {
    // Verify timestamp (within 5 minutes)
    const requestTime = new Date(request.timestamp).getTime();
    const now = Date.now();
    const timeDiff = Math.abs(now - requestTime);

    if (timeDiff > 5 * 60 * 1000) {
      return {
        is_valid: false,
        error_code: 'TIMESTAMP_INVALID',
        error_message: '요청 시간이 유효하지 않습니다.',
      };
    }

    // Verify device fingerprint matches registered device
    const deviceResult = await Database.query(
      `SELECT device_fingerprint FROM user_devices
       WHERE user_id = $1 AND device_fingerprint = $2`,
      [userId, request.device_fingerprint]
    );

    if (deviceResult.rows.length === 0) {
      // New device - auto register if under limit
      const deviceCount = await Database.query(
        'SELECT COUNT(*) FROM user_devices WHERE user_id = $1',
        [userId]
      );

      if (parseInt(deviceCount.rows[0].count) >= 5) {
        await this.reportViolation(userId, {
          violation_type: 'device_mismatch',
          details: `Unknown device: ${request.device_fingerprint}`,
          device_fingerprint: request.device_fingerprint,
          session_token: request.session_token,
          timestamp: request.timestamp,
          violation_count: 1,
        }, clientIp);

        return {
          is_valid: false,
          error_code: 'DEVICE_LIMIT_EXCEEDED',
          error_message: '등록된 기기 수를 초과했습니다.',
        };
      }
    }

    // Verify data hash (compare with last saved data)
    const saveResult = await Database.query(
      'SELECT checksum FROM game_saves WHERE user_id = $1',
      [userId]
    );

    // If there's saved data and hash doesn't match, flag it
    // (This is informational - client may have newer unsaved data)
    if (saveResult.rows.length > 0) {
      const serverHash = saveResult.rows[0].checksum;
      // Log for monitoring but don't fail
      if (serverHash !== request.data_hash) {
        logger.debug(`Data hash mismatch for user ${userId}: client=${request.data_hash}, server=${serverHash}`);
      }
    }

    // Check for suspicious patterns in Redis
    const suspicionKey = `suspicion:${userId}`;
    const suspicionCount = await RedisClient.get(suspicionKey);

    if (suspicionCount && parseInt(suspicionCount) >= 10) {
      return {
        is_valid: false,
        error_code: 'SUSPICIOUS_ACTIVITY',
        error_message: '비정상적인 활동이 감지되었습니다.',
      };
    }

    return { is_valid: true };
  }

  /**
   * Validate device binding
   */
  async validateDevice(userId: string, deviceFingerprint: string, deviceId: string, deviceModel: string, osVersion: string): Promise<{ is_valid: boolean; is_new_device: boolean; error_message?: string }> {
    // Check existing device
    const existing = await Database.query(
      `SELECT id FROM user_devices WHERE user_id = $1 AND device_fingerprint = $2`,
      [userId, deviceFingerprint]
    );

    if (existing.rows.length > 0) {
      // Update last active
      await Database.query(
        `UPDATE user_devices SET last_active_at = CURRENT_TIMESTAMP, device_model = $3, os_version = $4
         WHERE user_id = $1 AND device_fingerprint = $2`,
        [userId, deviceFingerprint, deviceModel, osVersion]
      );

      return { is_valid: true, is_new_device: false };
    }

    // Check device limit
    const deviceCount = await Database.query(
      'SELECT COUNT(*) FROM user_devices WHERE user_id = $1',
      [userId]
    );

    if (parseInt(deviceCount.rows[0].count) >= 5) {
      return {
        is_valid: false,
        is_new_device: true,
        error_message: '등록 가능한 기기 수(5대)를 초과했습니다.',
      };
    }

    return { is_valid: true, is_new_device: true };
  }

  /**
   * Register new device
   */
  async registerDevice(userId: string, deviceFingerprint: string, deviceId: string, deviceModel: string, osVersion: string): Promise<void> {
    await Database.query(
      `INSERT INTO user_devices (user_id, device_fingerprint, device_id, device_model, os_version, last_active_at)
       VALUES ($1, $2, $3, $4, $5, CURRENT_TIMESTAMP)
       ON CONFLICT (user_id, device_fingerprint) DO UPDATE SET
         device_model = $4, os_version = $5, last_active_at = CURRENT_TIMESTAMP`,
      [userId, deviceFingerprint, deviceId, deviceModel, osVersion]
    );

    logger.info(`Device registered: user=${userId}, device=${deviceFingerprint}`);
  }

  /**
   * Report security violation
   */
  async reportViolation(userId: string | null, report: ViolationReport, clientIp: string): Promise<{ action: ActionType; message: string }> {
    const severity = this.getSeverity(report.violation_type);

    // Store violation
    await Database.query(
      `INSERT INTO security_violations
         (user_id, violation_type, details, severity, device_fingerprint, ip_address, session_token)
       VALUES ($1, $2, $3, $4, $5, $6, $7)`,
      [userId, report.violation_type, report.details, severity, report.device_fingerprint, clientIp, report.session_token]
    );

    // Track in Redis for rate limiting
    if (userId) {
      const violationKey = `violations:${userId}:${report.violation_type}`;
      await RedisClient.incr(violationKey);
      await RedisClient.expire(violationKey, 24 * 60 * 60); // 24 hours

      const totalKey = `violations:${userId}:total`;
      const totalCount = await RedisClient.incr(totalKey);
      await RedisClient.expire(totalKey, 24 * 60 * 60);

      // Determine action
      const threshold = this.VIOLATION_THRESHOLD[severity];
      const typeCount = parseInt(await RedisClient.get(violationKey) || '0');

      if (typeCount >= threshold || totalCount >= 20) {
        // Ban user
        await this.banUser(userId, `자동 밴: ${report.violation_type} 위반 ${typeCount}회`);
        return { action: 'ban', message: '계정이 정지되었습니다.' };
      } else if (typeCount >= Math.ceil(threshold / 2)) {
        // Suspend temporarily
        return { action: 'suspend', message: '일시적으로 이용이 제한됩니다.' };
      }
    }

    logger.warn(`Security violation: type=${report.violation_type}, severity=${severity}, user=${userId}, ip=${clientIp}`);

    return { action: 'warn', message: '경고: 비정상적인 활동이 감지되었습니다.' };
  }

  /**
   * Get severity for violation type
   */
  private getSeverity(violationType: ViolationType): SeverityLevel {
    const severityMap: Record<ViolationType, SeverityLevel> = {
      memory_tampering: 'high',
      speed_hack: 'high',
      data_tampering: 'critical',
      device_mismatch: 'low',
      invalid_value: 'medium',
      abnormal_progression: 'medium',
      rooted_device: 'low',
      modified_apk: 'critical',
    };

    return severityMap[violationType] || 'medium';
  }

  /**
   * Ban user
   */
  async banUser(userId: string, reason: string, until?: Date): Promise<void> {
    await Database.query(
      `UPDATE users SET is_banned = TRUE, ban_reason = $2, ban_until = $3 WHERE id = $1`,
      [userId, reason, until || null]
    );

    // Invalidate all tokens
    await Database.query(
      'UPDATE refresh_tokens SET revoked_at = CURRENT_TIMESTAMP WHERE user_id = $1',
      [userId]
    );

    // Mark in Redis for quick check
    await RedisClient.set(`banned:${userId}`, '1', until ? Math.floor((until.getTime() - Date.now()) / 1000) : undefined);

    logger.warn(`User banned: ${userId}, reason: ${reason}`);
  }

  /**
   * Verify request signature (for signed API requests)
   */
  verifyRequestSignature(endpoint: string, body: string, timestamp: string, signature: string, deviceFingerprint: string): boolean {
    const dataToSign = `${endpoint}|${body}|${timestamp}|${deviceFingerprint}`;
    const expectedSignature = crypto
      .createHmac('sha256', config.security.hmacSecret)
      .update(dataToSign)
      .digest('base64');

    return crypto.timingSafeEqual(
      Buffer.from(signature),
      Buffer.from(expectedSignature)
    );
  }

  /**
   * Check if user is banned
   */
  async isUserBanned(userId: string): Promise<boolean> {
    // Quick check in Redis
    const redisBanned = await RedisClient.exists(`banned:${userId}`);
    if (redisBanned) return true;

    // Check database
    const result = await Database.query(
      `SELECT is_banned, ban_until FROM users WHERE id = $1`,
      [userId]
    );

    if (result.rows.length === 0) return false;

    const { is_banned, ban_until } = result.rows[0];

    if (is_banned) {
      // Check if ban has expired
      if (ban_until && new Date(ban_until) < new Date()) {
        // Unban
        await Database.query(
          'UPDATE users SET is_banned = FALSE, ban_reason = NULL, ban_until = NULL WHERE id = $1',
          [userId]
        );
        return false;
      }
      return true;
    }

    return false;
  }

  /**
   * Get violation history for user
   */
  async getViolationHistory(userId: string): Promise<any[]> {
    const result = await Database.query(
      `SELECT violation_type, details, severity, action_taken, created_at
       FROM security_violations
       WHERE user_id = $1
       ORDER BY created_at DESC
       LIMIT 50`,
      [userId]
    );

    return result.rows;
  }
}

export const securityService = new SecurityService();
export default securityService;
