import { Request, Response, NextFunction } from 'express';
import config from '../config';

// 간단한 관리자 인증 미들웨어
// 프로덕션에서는 더 강력한 인증 시스템 권장

export interface AdminRequest extends Request {
  isAdmin?: boolean;
}

export const adminAuth = (req: AdminRequest, res: Response, next: NextFunction) => {
  const adminKey = req.headers['x-admin-key'] as string;
  const expectedKey = process.env.ADMIN_SECRET_KEY || 'admin_secret_change_me';

  if (!adminKey || adminKey !== expectedKey) {
    return res.status(401).json({
      success: false,
      error: 'Unauthorized: Invalid admin key',
    });
  }

  req.isAdmin = true;
  next();
};

// 기본 인증 (브라우저용)
export const basicAuth = (req: Request, res: Response, next: NextFunction) => {
  const authHeader = req.headers.authorization;

  if (!authHeader || !authHeader.startsWith('Basic ')) {
    res.setHeader('WWW-Authenticate', 'Basic realm="Admin Dashboard"');
    return res.status(401).send('Authentication required');
  }

  const base64Credentials = authHeader.split(' ')[1];
  const credentials = Buffer.from(base64Credentials, 'base64').toString('ascii');
  const [username, password] = credentials.split(':');

  const adminUsername = process.env.ADMIN_USERNAME || 'admin';
  const adminPassword = process.env.ADMIN_PASSWORD || 'admin123';

  if (username === adminUsername && password === adminPassword) {
    next();
  } else {
    res.setHeader('WWW-Authenticate', 'Basic realm="Admin Dashboard"');
    return res.status(401).send('Invalid credentials');
  }
};

export default adminAuth;
