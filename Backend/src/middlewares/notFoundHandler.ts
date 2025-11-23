import { Request, Response, NextFunction } from 'express';

export const notFoundHandler = (
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  res.status(404).json({
    success: false,
    code: 'NOT_FOUND',
    message: `경로를 찾을 수 없습니다: ${req.method} ${req.path}`,
  });
};

export default notFoundHandler;
