import { Router, Request, Response, NextFunction } from 'express';
import { saveService } from '../services/save.service';
import { authenticate } from '../middlewares/auth.middleware';
import { validate } from '../middlewares/validate.middleware';
import { AppError } from '../utils/errors';
import Joi from 'joi';

const router = Router();

// All routes require authentication
router.use(authenticate);

// Validation schemas
const uploadSchema = Joi.object({
  version: Joi.number().integer().min(1).required(),
  checksum: Joi.string().required(),
  player_data: Joi.object().required(),
  stage_data: Joi.object().optional(),
  currency_data: Joi.alternatives().try(Joi.object(), Joi.array()).optional(),
  inventory_data: Joi.object().optional(),
  achievement_data: Joi.object().optional(),
  play_time: Joi.number().integer().min(0).optional(),
  device_id: Joi.string().optional(),
  platform: Joi.string().optional(),
  force_override: Joi.boolean().optional(),
});

/**
 * POST /save/upload
 * Upload game save
 */
router.post('/upload', validate(uploadSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const result = await saveService.uploadSave(req.user.userId, req.body);

    res.json({
      success: true,
      version: result.version,
    });
  } catch (error) {
    // Handle conflict error specially
    if (error instanceof AppError && error.statusCode === 409) {
      const serverData = await saveService.downloadSave(req.user!.userId);
      res.status(409).json({
        success: false,
        code: 'CONFLICT',
        message: error.message,
        server_data: serverData,
      });
      return;
    }
    next(error);
  }
});

/**
 * GET /save/download
 * Download game save
 */
router.get('/download', async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const saveData = await saveService.downloadSave(req.user.userId);

    if (!saveData) {
      res.status(404).json({
        success: false,
        code: 'NOT_FOUND',
        message: '저장된 데이터가 없습니다.',
      });
      return;
    }

    res.json({
      success: true,
      ...saveData,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /save/backups
 * Get backup list
 */
router.get('/backups', async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const backups = await saveService.getBackups(req.user.userId);

    res.json({
      success: true,
      backups,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /save/restore/:backupId
 * Restore from backup
 */
router.post('/restore/:backupId', async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    await saveService.restoreFromBackup(req.user.userId, req.params.backupId);

    res.json({
      success: true,
      message: '복원이 완료되었습니다.',
    });
  } catch (error) {
    next(error);
  }
});

/**
 * DELETE /save/delete
 * Delete all save data
 */
router.delete('/delete', async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    await saveService.deleteSave(req.user.userId);

    res.json({
      success: true,
      message: '저장 데이터가 삭제되었습니다.',
    });
  } catch (error) {
    next(error);
  }
});

export default router;
