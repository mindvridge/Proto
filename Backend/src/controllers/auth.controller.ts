import { Request, Response, NextFunction } from 'express';
import { authService } from '../services/auth.service';
import { AppError } from '../utils/errors';
import { logger } from '../utils/logger';
import { Database } from '../database/connection';

class AuthController {
  /**
   * POST /auth/register/email
   * Email registration
   */
  async registerEmail(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { email, password, nickname } = req.body;

      const result = await authService.registerWithEmail(email, password, nickname);

      // Log registration
      await this.logLogin(result.user.user_id, 'email', req, true);

      res.status(201).json({
        success: true,
        access_token: result.tokens.accessToken,
        refresh_token: result.tokens.refreshToken,
        expires_in: result.tokens.expiresIn,
        user: result.user,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * POST /auth/login/email
   * Email login
   */
  async loginEmail(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { email, password } = req.body;

      const result = await authService.loginWithEmail(email, password);

      // Log login
      await this.logLogin(result.user.user_id, 'email', req, true);

      res.json({
        success: true,
        access_token: result.tokens.accessToken,
        refresh_token: result.tokens.refreshToken,
        expires_in: result.tokens.expiresIn,
        user: result.user,
      });
    } catch (error) {
      // Log failed login
      if (error instanceof AppError) {
        logger.warn(`Failed login attempt for email: ${req.body.email}`);
      }
      next(error);
    }
  }

  /**
   * POST /auth/login/social
   * Social login (Kakao, Google)
   */
  async loginSocial(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { provider, access_token } = req.body;

      const result = await authService.loginWithSocial(provider, access_token);

      // Log login
      await this.logLogin(result.user.user_id, provider, req, true);

      res.json({
        success: true,
        access_token: result.tokens.accessToken,
        refresh_token: result.tokens.refreshToken,
        expires_in: result.tokens.expiresIn,
        user: result.user,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * POST /auth/login/guest
   * Guest login
   */
  async loginGuest(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { device_id, platform } = req.body;

      const result = await authService.loginAsGuest(device_id, platform);

      // Log login
      await this.logLogin(result.user.user_id, 'guest', req, true);

      res.json({
        success: true,
        access_token: result.tokens.accessToken,
        refresh_token: result.tokens.refreshToken,
        expires_in: result.tokens.expiresIn,
        user: result.user,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * POST /auth/refresh
   * Refresh access token
   */
  async refreshToken(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { refresh_token } = req.body;

      if (!refresh_token) {
        throw new AppError('리프레시 토큰이 필요합니다.', 400);
      }

      const tokens = await authService.refreshAccessToken(refresh_token);

      res.json({
        success: true,
        access_token: tokens.accessToken,
        refresh_token: tokens.refreshToken,
        expires_in: tokens.expiresIn,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * POST /auth/logout
   * Logout (revoke tokens)
   */
  async logout(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      if (req.user && req.token) {
        await authService.logout(req.user.userId, req.token);
      }

      res.json({
        success: true,
        message: '로그아웃되었습니다.',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * GET /auth/me
   * Get current user info
   */
  async getMe(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) {
        throw new AppError('인증이 필요합니다.', 401);
      }

      const user = await authService.getUserById(req.user.userId);

      if (!user) {
        throw new AppError('사용자를 찾을 수 없습니다.', 404);
      }

      res.json({
        success: true,
        ...user,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * PUT /auth/nickname
   * Update nickname
   */
  async updateNickname(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) {
        throw new AppError('인증이 필요합니다.', 401);
      }

      const { nickname } = req.body;

      if (!nickname || nickname.length < 2 || nickname.length > 20) {
        throw new AppError('닉네임은 2~20자 사이여야 합니다.', 400);
      }

      const user = await authService.updateNickname(req.user.userId, nickname);

      res.json({
        success: true,
        ...user,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * DELETE /auth/account
   * Delete account
   */
  async deleteAccount(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.user) {
        throw new AppError('인증이 필요합니다.', 401);
      }

      await authService.deleteAccount(req.user.userId);

      res.json({
        success: true,
        message: '계정이 삭제되었습니다.',
      });
    } catch (error) {
      next(error);
    }
  }

  // ========================
  // Helper Methods
  // ========================

  private async logLogin(
    userId: string,
    loginType: string,
    req: Request,
    success: boolean,
    failureReason?: string
  ): Promise<void> {
    try {
      const ip = req.ip || req.socket.remoteAddress || 'unknown';
      const deviceInfo = req.headers['user-agent'] || 'unknown';

      await Database.query(
        `INSERT INTO login_history (user_id, login_type, ip_address, device_info, success, failure_reason)
         VALUES ($1, $2, $3, $4, $5, $6)`,
        [userId, loginType, ip, deviceInfo.substring(0, 500), success, failureReason]
      );
    } catch (error) {
      logger.error('Failed to log login:', error);
    }
  }
}

export const authController = new AuthController();
export default authController;
