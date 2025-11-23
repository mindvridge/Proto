import { Router, Request, Response, NextFunction } from 'express';
import { iapService } from '../services/iap.service';
import { authenticate } from '../middlewares/auth.middleware';
import { validate } from '../middlewares/validate.middleware';
import { AppError } from '../utils/errors';
import Joi from 'joi';

const router = Router();

// Validation schemas
const validateSchema = Joi.object({
  product_id: Joi.string().required(),
  transaction_id: Joi.string().required(),
  receipt: Joi.string().required(),
  platform: Joi.string().valid('google', 'apple').required(),
  user_id: Joi.string().optional(),
});

/**
 * POST /iap/validate
 * Validate purchase receipt
 */
router.post('/validate', authenticate, validate(validateSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const request = {
      ...req.body,
      user_id: req.user.userId,
    };

    const result = await iapService.validatePurchase(request);

    res.json({
      success: result.is_valid,
      is_valid: result.is_valid,
      error_code: result.error_code,
      error_message: result.error_message,
      order_id: result.order_id,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /iap/history
 * Get purchase history
 */
router.get('/history', authenticate, async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const history = await iapService.getPurchaseHistory(req.user.userId);

    res.json({
      success: true,
      items: history,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /iap/webhook/google
 * Google Play webhook (Real-time Developer Notifications)
 */
router.post('/webhook/google', async (req: Request, res: Response, next: NextFunction) => {
  try {
    // TODO: Verify Google Cloud Pub/Sub signature
    const { message } = req.body;

    if (message && message.data) {
      const data = JSON.parse(Buffer.from(message.data, 'base64').toString());

      // Handle notification types
      if (data.voidedPurchaseNotification) {
        // Purchase was voided (refund)
        await iapService.processRefund(
          data.voidedPurchaseNotification.orderId,
          'Google Play refund'
        );
      }
    }

    res.status(200).send('OK');
  } catch (error) {
    next(error);
  }
});

/**
 * POST /iap/webhook/apple
 * Apple App Store Server Notifications
 */
router.post('/webhook/apple', async (req: Request, res: Response, next: NextFunction) => {
  try {
    // TODO: Verify Apple signature
    const { notification_type, unified_receipt } = req.body;

    if (notification_type === 'REFUND' && unified_receipt) {
      const latestReceipt = unified_receipt.latest_receipt_info?.[0];
      if (latestReceipt) {
        await iapService.processRefund(
          latestReceipt.transaction_id,
          'Apple App Store refund'
        );
      }
    }

    res.status(200).send('OK');
  } catch (error) {
    next(error);
  }
});

export default router;
