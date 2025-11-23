import { Request, Response, NextFunction } from 'express';
import { AppError, ValidationError } from '../utils/errors';
import { logger } from '../utils/logger';
import config from '../config';

export const errorHandler = (
  err: Error,
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  // Log error
  logger.error('Error:', {
    message: err.message,
    stack: err.stack,
    path: req.path,
    method: req.method,
    ip: req.ip,
  });

  // Handle known errors
  if (err instanceof AppError) {
    const response: any = {
      success: false,
      code: err.code,
      message: err.message,
    };

    // Add validation details if present
    if (err instanceof ValidationError && err.details) {
      response.details = err.details;
    }

    // Add stack trace in development
    if (config.nodeEnv === 'development') {
      response.stack = err.stack;
    }

    res.status(err.statusCode).json(response);
    return;
  }

  // Handle JWT errors
  if (err.name === 'JsonWebTokenError') {
    res.status(401).json({
      success: false,
      code: 'INVALID_TOKEN',
      message: '유효하지 않은 토큰입니다.',
    });
    return;
  }

  if (err.name === 'TokenExpiredError') {
    res.status(401).json({
      success: false,
      code: 'TOKEN_EXPIRED',
      message: '토큰이 만료되었습니다.',
    });
    return;
  }

  // Handle Joi validation errors
  if (err.name === 'ValidationError') {
    res.status(422).json({
      success: false,
      code: 'VALIDATION_ERROR',
      message: err.message,
    });
    return;
  }

  // Handle database errors
  if ((err as any).code === '23505') {
    // Unique violation
    res.status(409).json({
      success: false,
      code: 'DUPLICATE_ENTRY',
      message: '이미 존재하는 데이터입니다.',
    });
    return;
  }

  if ((err as any).code === '23503') {
    // Foreign key violation
    res.status(400).json({
      success: false,
      code: 'INVALID_REFERENCE',
      message: '참조하는 데이터가 존재하지 않습니다.',
    });
    return;
  }

  // Handle unknown errors
  const response: any = {
    success: false,
    code: 'INTERNAL_ERROR',
    message: config.nodeEnv === 'production'
      ? '서버 오류가 발생했습니다.'
      : err.message,
  };

  if (config.nodeEnv === 'development') {
    response.stack = err.stack;
  }

  res.status(500).json(response);
};

export default errorHandler;
