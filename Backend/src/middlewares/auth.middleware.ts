import { Request, Response, NextFunction } from 'express';
import { authService, UserPayload } from '../services/auth.service';
import { AppError, AuthenticationError } from '../utils/errors';

// Extend Express Request to include user
declare global {
  namespace Express {
    interface Request {
      user?: UserPayload;
      token?: string;
    }
  }
}

/**
 * Authentication middleware - requires valid JWT
 */
export const authenticate = async (
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    const authHeader = req.headers.authorization;

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      throw new AuthenticationError('인증 토큰이 필요합니다.');
    }

    const token = authHeader.substring(7);
    const payload = await authService.verifyAccessToken(token);

    req.user = payload;
    req.token = token;

    next();
  } catch (error) {
    if (error instanceof AppError) {
      res.status(error.statusCode).json({
        success: false,
        code: error.code,
        message: error.message,
      });
    } else {
      res.status(401).json({
        success: false,
        code: 'AUTHENTICATION_ERROR',
        message: '인증에 실패했습니다.',
      });
    }
  }
};

/**
 * Optional authentication - attaches user if token present
 */
export const optionalAuth = async (
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    const authHeader = req.headers.authorization;

    if (authHeader && authHeader.startsWith('Bearer ')) {
      const token = authHeader.substring(7);
      const payload = await authService.verifyAccessToken(token);
      req.user = payload;
      req.token = token;
    }

    next();
  } catch (error) {
    // Ignore auth errors for optional auth
    next();
  }
};

export default authenticate;
