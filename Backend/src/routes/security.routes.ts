import { Router, Request, Response, NextFunction } from 'express';
import { securityService } from '../services/security.service';
import { authenticate, optionalAuth } from '../middlewares/auth.middleware';
import { validate } from '../middlewares/validate.middleware';
import { AppError } from '../utils/errors';
import Joi from 'joi';

const router = Router();

// Validation schemas
const integrityCheckSchema = Joi.object({
  device_fingerprint: Joi.string().required(),
  session_token: Joi.string().required(),
  data_hash: Joi.string().required(),
  timestamp: Joi.string().required(),
  app_version: Joi.string().required(),
  platform: Joi.string().required(),
});

const deviceBindSchema = Joi.object({
  device_fingerprint: Joi.string().required(),
  device_id: Joi.string().required(),
  device_model: Joi.string().required(),
  os_version: Joi.string().required(),
});

const violationReportSchema = Joi.object({
  violation_type: Joi.string().valid(
    'memory_tampering', 'speed_hack', 'data_tampering', 'device_mismatch',
    'invalid_value', 'abnormal_progression', 'rooted_device', 'modified_apk'
  ).required(),
  details: Joi.string().required(),
  device_fingerprint: Joi.string().required(),
  session_token: Joi.string().required(),
  timestamp: Joi.string().required(),
  violation_count: Joi.number().integer().min(1).required(),
});

/**
 * POST /security/integrity-check
 * Verify game data integrity
 */
router.post('/integrity-check', authenticate, validate(integrityCheckSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const clientIp = req.ip || req.socket.remoteAddress || 'unknown';
    const result = await securityService.checkIntegrity(req.user.userId, req.body, clientIp);

    res.json({
      success: result.is_valid,
      is_valid: result.is_valid,
      error_code: result.error_code,
      error_message: result.error_message,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /security/validate-device
 * Validate device binding
 */
router.post('/validate-device', authenticate, validate(deviceBindSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const { device_fingerprint, device_id, device_model, os_version } = req.body;
    const result = await securityService.validateDevice(
      req.user.userId,
      device_fingerprint,
      device_id,
      device_model,
      os_version
    );

    res.json({
      success: result.is_valid,
      is_valid: result.is_valid,
      is_new_device: result.is_new_device,
      error_message: result.error_message,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /security/register-device
 * Register new device
 */
router.post('/register-device', authenticate, validate(deviceBindSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const { device_fingerprint, device_id, device_model, os_version } = req.body;

    // First validate
    const validation = await securityService.validateDevice(
      req.user.userId,
      device_fingerprint,
      device_id,
      device_model,
      os_version
    );

    if (!validation.is_valid) {
      res.status(400).json({
        success: false,
        message: validation.error_message,
      });
      return;
    }

    // Register
    await securityService.registerDevice(
      req.user.userId,
      device_fingerprint,
      device_id,
      device_model,
      os_version
    );

    res.json({
      success: true,
      message: '기기가 등록되었습니다.',
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /security/report-violation
 * Report security violation from client
 */
router.post('/report-violation', optionalAuth, validate(violationReportSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    const clientIp = req.ip || req.socket.remoteAddress || 'unknown';
    const userId = req.user?.userId || null;

    const result = await securityService.reportViolation(userId, req.body, clientIp);

    res.json({
      success: true,
      action: result.action,
      message: result.message,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /security/violations
 * Get violation history (admin or self)
 */
router.get('/violations', authenticate, async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const violations = await securityService.getViolationHistory(req.user.userId);

    res.json({
      success: true,
      violations,
    });
  } catch (error) {
    next(error);
  }
});

export default router;
