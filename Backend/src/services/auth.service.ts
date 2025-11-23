import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import axios from 'axios';
import { OAuth2Client } from 'google-auth-library';
import { v4 as uuidv4 } from 'uuid';
import { Database } from '../database/connection';
import { RedisClient } from '../database/redis';
import config from '../config';
import { logger } from '../utils/logger';
import { AppError } from '../utils/errors';

export interface UserPayload {
  userId: string;
  email?: string;
  nickname: string;
}

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface UserInfo {
  user_id: string;
  email: string | null;
  nickname: string;
  profile_image: string | null;
  level: number;
  created_at: string;
  last_login: string | null;
}

export interface GuestDeviceInfo {
  device_fingerprint?: string;
  device_model?: string;
  os_version?: string;
  app_version?: string;
}

class AuthService {
  private googleClient: OAuth2Client;

  constructor() {
    this.googleClient = new OAuth2Client(config.google.clientId);
  }

  // ========================
  // Email Authentication
  // ========================

  async registerWithEmail(email: string, password: string, nickname: string): Promise<{ user: UserInfo; tokens: TokenPair }> {
    // Check if email exists
    const existingUser = await Database.query(
      'SELECT id FROM users WHERE email = $1',
      [email]
    );

    if (existingUser.rows.length > 0) {
      throw new AppError('이미 사용 중인 이메일입니다.', 409);
    }

    // Check nickname
    const existingNickname = await Database.query(
      'SELECT id FROM users WHERE nickname = $1',
      [nickname]
    );

    if (existingNickname.rows.length > 0) {
      throw new AppError('이미 사용 중인 닉네임입니다.', 409);
    }

    // Hash password
    const passwordHash = await bcrypt.hash(password, 12);

    // Create user
    const result = await Database.query(
      `INSERT INTO users (email, password_hash, nickname)
       VALUES ($1, $2, $3)
       RETURNING id, email, nickname, profile_image, level, created_at, last_login_at`,
      [email, passwordHash, nickname]
    );

    const user = this.formatUserInfo(result.rows[0]);
    const tokens = await this.generateTokens(user);

    return { user, tokens };
  }

  async loginWithEmail(email: string, password: string): Promise<{ user: UserInfo; tokens: TokenPair }> {
    const result = await Database.query(
      `SELECT id, email, password_hash, nickname, profile_image, level, created_at, last_login_at,
              is_active, is_banned, ban_reason, ban_until
       FROM users WHERE email = $1`,
      [email]
    );

    if (result.rows.length === 0) {
      throw new AppError('이메일 또는 비밀번호가 올바르지 않습니다.', 401);
    }

    const dbUser = result.rows[0];

    // Check ban status
    this.checkBanStatus(dbUser);

    // Verify password
    const isValidPassword = await bcrypt.compare(password, dbUser.password_hash);
    if (!isValidPassword) {
      throw new AppError('이메일 또는 비밀번호가 올바르지 않습니다.', 401);
    }

    // Update last login
    await Database.query(
      'UPDATE users SET last_login_at = CURRENT_TIMESTAMP WHERE id = $1',
      [dbUser.id]
    );

    const user = this.formatUserInfo(dbUser);
    const tokens = await this.generateTokens(user);

    return { user, tokens };
  }

  // ========================
  // Social Authentication
  // ========================

  async loginWithSocial(provider: string, accessToken: string): Promise<{ user: UserInfo; tokens: TokenPair }> {
    let socialId: string;
    let email: string | null = null;
    let nickname: string;
    let profileImage: string | null = null;

    switch (provider.toLowerCase()) {
      case 'kakao':
        const kakaoUser = await this.verifyKakaoToken(accessToken);
        socialId = kakaoUser.id;
        email = kakaoUser.email;
        nickname = kakaoUser.nickname;
        profileImage = kakaoUser.profileImage;
        break;

      case 'google':
        const googleUser = await this.verifyGoogleToken(accessToken);
        socialId = googleUser.id;
        email = googleUser.email;
        nickname = googleUser.nickname;
        profileImage = googleUser.profileImage;
        break;

      default:
        throw new AppError('지원하지 않는 로그인 방식입니다.', 400);
    }

    // Find or create user
    const columnName = `${provider.toLowerCase()}_id`;
    let result = await Database.query(
      `SELECT id, email, nickname, profile_image, level, created_at, last_login_at,
              is_active, is_banned, ban_reason, ban_until
       FROM users WHERE ${columnName} = $1`,
      [socialId]
    );

    let dbUser;

    if (result.rows.length === 0) {
      // Create new user
      const uniqueNickname = await this.generateUniqueNickname(nickname);

      result = await Database.query(
        `INSERT INTO users (${columnName}, email, nickname, profile_image)
         VALUES ($1, $2, $3, $4)
         RETURNING id, email, nickname, profile_image, level, created_at, last_login_at`,
        [socialId, email, uniqueNickname, profileImage]
      );

      dbUser = result.rows[0];
    } else {
      dbUser = result.rows[0];

      // Check ban status
      this.checkBanStatus(dbUser);

      // Update last login
      await Database.query(
        'UPDATE users SET last_login_at = CURRENT_TIMESTAMP WHERE id = $1',
        [dbUser.id]
      );
    }

    const user = this.formatUserInfo(dbUser);
    const tokens = await this.generateTokens(user);

    return { user, tokens };
  }

  // ========================
  // Guest Authentication
  // ========================

  async loginAsGuest(deviceId: string, platform: string, deviceInfo?: GuestDeviceInfo): Promise<{ user: UserInfo; tokens: TokenPair; isNewUser: boolean }> {
    let result = await Database.query(
      `SELECT id, email, nickname, profile_image, level, created_at, last_login_at,
              is_active, is_banned, ban_reason, ban_until
       FROM users WHERE device_id = $1`,
      [deviceId]
    );

    let dbUser;
    let isNewUser = false;

    if (result.rows.length === 0) {
      // 자동 회원가입 - 새 게스트 유저 생성
      isNewUser = true;
      const guestNickname = `영웅${Math.floor(Math.random() * 900000) + 100000}`;

      result = await Database.query(
        `INSERT INTO users (device_id, nickname)
         VALUES ($1, $2)
         RETURNING id, email, nickname, profile_image, level, created_at, last_login_at`,
        [deviceId, guestNickname]
      );

      dbUser = result.rows[0];

      // 기기 정보 저장
      if (deviceInfo) {
        await this.registerGuestDevice(dbUser.id, deviceId, deviceInfo);
      }

      logger.info(`New guest user created: ${dbUser.id}, device: ${deviceId}`);
    } else {
      dbUser = result.rows[0];

      // Check ban status
      this.checkBanStatus(dbUser);

      // Update last login
      await Database.query(
        'UPDATE users SET last_login_at = CURRENT_TIMESTAMP WHERE id = $1',
        [dbUser.id]
      );

      // 기기 정보 업데이트
      if (deviceInfo) {
        await this.updateGuestDevice(dbUser.id, deviceId, deviceInfo);
      }
    }

    const user = this.formatUserInfo(dbUser);
    const tokens = await this.generateTokens(user);

    return { user, tokens, isNewUser };
  }

  /**
   * 게스트 기기 정보 등록
   */
  private async registerGuestDevice(userId: string, deviceId: string, info: GuestDeviceInfo): Promise<void> {
    try {
      await Database.query(
        `INSERT INTO user_devices (user_id, device_fingerprint, device_id, device_model, os_version, app_version, is_primary, last_active_at)
         VALUES ($1, $2, $3, $4, $5, $6, TRUE, CURRENT_TIMESTAMP)
         ON CONFLICT (user_id, device_fingerprint) DO NOTHING`,
        [userId, info.device_fingerprint || deviceId, deviceId, info.device_model, info.os_version, info.app_version]
      );
    } catch (error) {
      logger.error('Failed to register guest device:', error);
    }
  }

  /**
   * 게스트 기기 정보 업데이트
   */
  private async updateGuestDevice(userId: string, deviceId: string, info: GuestDeviceInfo): Promise<void> {
    try {
      await Database.query(
        `UPDATE user_devices SET
           device_model = COALESCE($3, device_model),
           os_version = COALESCE($4, os_version),
           app_version = COALESCE($5, app_version),
           last_active_at = CURRENT_TIMESTAMP
         WHERE user_id = $1 AND device_id = $2`,
        [userId, deviceId, info.device_model, info.os_version, info.app_version]
      );
    } catch (error) {
      logger.error('Failed to update guest device:', error);
    }
  }

  // ========================
  // Token Management
  // ========================

  async generateTokens(user: UserInfo): Promise<TokenPair> {
    const payload: UserPayload = {
      userId: user.user_id,
      email: user.email || undefined,
      nickname: user.nickname,
    };

    const accessToken = jwt.sign(payload, config.jwt.secret, {
      expiresIn: config.jwt.expiresIn,
    });

    const refreshToken = jwt.sign(
      { userId: user.user_id, type: 'refresh' },
      config.jwt.refreshSecret,
      { expiresIn: config.jwt.refreshExpiresIn }
    );

    // Store refresh token hash in database
    const tokenHash = await bcrypt.hash(refreshToken, 10);
    const expiresAt = new Date();
    expiresAt.setDate(expiresAt.getDate() + 30);

    await Database.query(
      `INSERT INTO refresh_tokens (user_id, token_hash, expires_at)
       VALUES ($1, $2, $3)`,
      [user.user_id, tokenHash, expiresAt]
    );

    return {
      accessToken,
      refreshToken,
      expiresIn: 7 * 24 * 60 * 60, // 7 days in seconds
    };
  }

  async refreshAccessToken(refreshToken: string): Promise<TokenPair> {
    try {
      const decoded = jwt.verify(refreshToken, config.jwt.refreshSecret) as { userId: string };

      // Find valid refresh token
      const result = await Database.query(
        `SELECT rt.*, u.email, u.nickname, u.profile_image, u.level, u.created_at, u.last_login_at
         FROM refresh_tokens rt
         JOIN users u ON u.id = rt.user_id
         WHERE rt.user_id = $1 AND rt.revoked_at IS NULL AND rt.expires_at > CURRENT_TIMESTAMP
         ORDER BY rt.created_at DESC
         LIMIT 1`,
        [decoded.userId]
      );

      if (result.rows.length === 0) {
        throw new AppError('유효하지 않은 토큰입니다.', 401);
      }

      const dbUser = result.rows[0];
      const user = this.formatUserInfo(dbUser);

      // Revoke old refresh token
      await Database.query(
        'UPDATE refresh_tokens SET revoked_at = CURRENT_TIMESTAMP WHERE user_id = $1 AND revoked_at IS NULL',
        [decoded.userId]
      );

      // Generate new tokens
      return this.generateTokens(user);
    } catch (error) {
      if (error instanceof AppError) throw error;
      throw new AppError('토큰 갱신에 실패했습니다.', 401);
    }
  }

  async verifyAccessToken(token: string): Promise<UserPayload> {
    try {
      const decoded = jwt.verify(token, config.jwt.secret) as UserPayload;

      // Check if token is blacklisted
      const isBlacklisted = await RedisClient.exists(`blacklist:${token}`);
      if (isBlacklisted) {
        throw new AppError('토큰이 무효화되었습니다.', 401);
      }

      return decoded;
    } catch (error) {
      if (error instanceof AppError) throw error;
      throw new AppError('유효하지 않은 토큰입니다.', 401);
    }
  }

  async logout(userId: string, token: string): Promise<void> {
    // Revoke all refresh tokens
    await Database.query(
      'UPDATE refresh_tokens SET revoked_at = CURRENT_TIMESTAMP WHERE user_id = $1 AND revoked_at IS NULL',
      [userId]
    );

    // Blacklist current access token (TTL: token remaining lifetime)
    await RedisClient.set(`blacklist:${token}`, '1', 7 * 24 * 60 * 60);
  }

  // ========================
  // User Management
  // ========================

  async getUserById(userId: string): Promise<UserInfo | null> {
    const result = await Database.query(
      `SELECT id, email, nickname, profile_image, level, created_at, last_login_at
       FROM users WHERE id = $1`,
      [userId]
    );

    if (result.rows.length === 0) return null;
    return this.formatUserInfo(result.rows[0]);
  }

  async updateNickname(userId: string, nickname: string): Promise<UserInfo> {
    // Check nickname availability
    const existing = await Database.query(
      'SELECT id FROM users WHERE nickname = $1 AND id != $2',
      [nickname, userId]
    );

    if (existing.rows.length > 0) {
      throw new AppError('이미 사용 중인 닉네임입니다.', 409);
    }

    const result = await Database.query(
      `UPDATE users SET nickname = $1
       WHERE id = $2
       RETURNING id, email, nickname, profile_image, level, created_at, last_login_at`,
      [nickname, userId]
    );

    return this.formatUserInfo(result.rows[0]);
  }

  async deleteAccount(userId: string): Promise<void> {
    await Database.transaction(async (client) => {
      // Soft delete - mark as inactive
      await client.query(
        'UPDATE users SET is_active = FALSE, email = NULL, nickname = $1 WHERE id = $2',
        [`deleted_${userId.substring(0, 8)}`, userId]
      );

      // Revoke all tokens
      await client.query(
        'UPDATE refresh_tokens SET revoked_at = CURRENT_TIMESTAMP WHERE user_id = $1',
        [userId]
      );
    });
  }

  // ========================
  // OAuth Verification
  // ========================

  private async verifyKakaoToken(accessToken: string): Promise<{ id: string; email: string | null; nickname: string; profileImage: string | null }> {
    try {
      const response = await axios.get('https://kapi.kakao.com/v2/user/me', {
        headers: { Authorization: `Bearer ${accessToken}` }
      });

      const data = response.data;
      return {
        id: data.id.toString(),
        email: data.kakao_account?.email || null,
        nickname: data.properties?.nickname || data.kakao_account?.profile?.nickname || `카카오유저${data.id}`,
        profileImage: data.properties?.profile_image || data.kakao_account?.profile?.profile_image_url || null,
      };
    } catch (error) {
      logger.error('Kakao token verification failed:', error);
      throw new AppError('카카오 인증에 실패했습니다.', 401);
    }
  }

  private async verifyGoogleToken(idToken: string): Promise<{ id: string; email: string | null; nickname: string; profileImage: string | null }> {
    try {
      const ticket = await this.googleClient.verifyIdToken({
        idToken,
        audience: config.google.clientId,
      });

      const payload = ticket.getPayload();
      if (!payload) {
        throw new AppError('구글 인증에 실패했습니다.', 401);
      }

      return {
        id: payload.sub,
        email: payload.email || null,
        nickname: payload.name || `구글유저${payload.sub.substring(0, 8)}`,
        profileImage: payload.picture || null,
      };
    } catch (error) {
      logger.error('Google token verification failed:', error);
      throw new AppError('구글 인증에 실패했습니다.', 401);
    }
  }

  // ========================
  // Helper Methods
  // ========================

  private checkBanStatus(user: any): void {
    if (!user.is_active) {
      throw new AppError('비활성화된 계정입니다.', 403);
    }

    if (user.is_banned) {
      if (user.ban_until && new Date(user.ban_until) > new Date()) {
        throw new AppError(`계정이 정지되었습니다. (${user.ban_reason || '사유 없음'}) 해제일: ${user.ban_until}`, 403);
      } else if (!user.ban_until) {
        throw new AppError(`계정이 영구 정지되었습니다. (${user.ban_reason || '사유 없음'})`, 403);
      }
    }
  }

  private formatUserInfo(dbUser: any): UserInfo {
    return {
      user_id: dbUser.id,
      email: dbUser.email,
      nickname: dbUser.nickname,
      profile_image: dbUser.profile_image,
      level: dbUser.level,
      created_at: dbUser.created_at?.toISOString() || new Date().toISOString(),
      last_login: dbUser.last_login_at?.toISOString() || null,
    };
  }

  private async generateUniqueNickname(baseName: string): Promise<string> {
    let nickname = baseName;
    let counter = 1;

    while (true) {
      const existing = await Database.query(
        'SELECT id FROM users WHERE nickname = $1',
        [nickname]
      );

      if (existing.rows.length === 0) {
        return nickname;
      }

      nickname = `${baseName}${counter}`;
      counter++;

      if (counter > 1000) {
        nickname = `${baseName}${Math.floor(Math.random() * 100000)}`;
        break;
      }
    }

    return nickname;
  }
}

export const authService = new AuthService();
export default authService;
