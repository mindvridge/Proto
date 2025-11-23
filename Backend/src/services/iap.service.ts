import axios from 'axios';
import crypto from 'crypto';
import { Database } from '../database/connection';
import { logger } from '../utils/logger';
import { AppError } from '../utils/errors';
import config from '../config';

export interface PurchaseValidationRequest {
  product_id: string;
  transaction_id: string;
  receipt: string;
  platform: 'google' | 'apple';
  user_id: string;
}

export interface PurchaseValidationResult {
  is_valid: boolean;
  error_code?: string;
  error_message?: string;
  order_id?: string;
}

class IAPService {
  /**
   * Validate purchase receipt
   */
  async validatePurchase(request: PurchaseValidationRequest): Promise<PurchaseValidationResult> {
    // Check for duplicate transaction
    const existing = await Database.query(
      'SELECT id, status FROM purchases WHERE transaction_id = $1',
      [request.transaction_id]
    );

    if (existing.rows.length > 0) {
      if (existing.rows[0].status === 'completed') {
        return {
          is_valid: false,
          error_code: 'DUPLICATE_TRANSACTION',
          error_message: '이미 처리된 거래입니다.',
        };
      }
    }

    // Store pending purchase
    const receiptHash = crypto.createHash('sha256').update(request.receipt).digest('hex');

    await Database.query(
      `INSERT INTO purchases (user_id, product_id, transaction_id, platform, receipt, receipt_hash, status)
       VALUES ($1, $2, $3, $4, $5, $6, 'pending')
       ON CONFLICT (transaction_id) DO UPDATE SET status = 'pending', updated_at = CURRENT_TIMESTAMP`,
      [request.user_id, request.product_id, request.transaction_id, request.platform, request.receipt, receiptHash]
    );

    let result: PurchaseValidationResult;

    try {
      if (request.platform === 'google') {
        result = await this.validateGooglePurchase(request);
      } else if (request.platform === 'apple') {
        result = await this.validateApplePurchase(request);
      } else {
        result = {
          is_valid: false,
          error_code: 'INVALID_PLATFORM',
          error_message: '지원하지 않는 플랫폼입니다.',
        };
      }

      // Update purchase status
      const status = result.is_valid ? 'completed' : 'failed';
      await Database.query(
        `UPDATE purchases SET
           is_valid = $2,
           validation_response = $3,
           validated_at = CURRENT_TIMESTAMP,
           status = $4
         WHERE transaction_id = $1`,
        [request.transaction_id, result.is_valid, JSON.stringify(result), status]
      );

      if (result.is_valid) {
        logger.info(`Purchase validated: user=${request.user_id}, product=${request.product_id}, transaction=${request.transaction_id}`);
      } else {
        logger.warn(`Purchase validation failed: user=${request.user_id}, product=${request.product_id}, error=${result.error_code}`);
      }

      return result;
    } catch (error) {
      logger.error('Purchase validation error:', error);

      // Update as failed
      await Database.query(
        `UPDATE purchases SET status = 'failed', validation_response = $2 WHERE transaction_id = $1`,
        [request.transaction_id, JSON.stringify({ error: (error as Error).message })]
      );

      return {
        is_valid: false,
        error_code: 'VALIDATION_ERROR',
        error_message: '결제 검증 중 오류가 발생했습니다.',
      };
    }
  }

  /**
   * Validate Google Play purchase
   */
  private async validateGooglePurchase(request: PurchaseValidationRequest): Promise<PurchaseValidationResult> {
    try {
      // Parse receipt
      const receiptData = JSON.parse(request.receipt);
      const { purchaseToken, packageName } = receiptData;

      if (!purchaseToken || !packageName) {
        return {
          is_valid: false,
          error_code: 'INVALID_RECEIPT',
          error_message: '유효하지 않은 영수증입니다.',
        };
      }

      // Verify package name
      if (packageName !== config.iap.google.packageName) {
        return {
          is_valid: false,
          error_code: 'PACKAGE_MISMATCH',
          error_message: '패키지 이름이 일치하지 않습니다.',
        };
      }

      // In production, use Google Play Developer API
      // This requires service account authentication
      // For now, we do basic validation

      // TODO: Implement actual Google Play verification
      // const auth = new google.auth.GoogleAuth({
      //   credentials: {
      //     client_email: config.iap.google.serviceAccountEmail,
      //     private_key: config.iap.google.serviceAccountKey,
      //   },
      //   scopes: ['https://www.googleapis.com/auth/androidpublisher'],
      // });

      // const androidpublisher = google.androidpublisher({ version: 'v3', auth });
      // const response = await androidpublisher.purchases.products.get({
      //   packageName: config.iap.google.packageName,
      //   productId: request.product_id,
      //   token: purchaseToken,
      // });

      // Simplified validation for development
      if (purchaseToken && request.transaction_id) {
        return {
          is_valid: true,
          order_id: request.transaction_id,
        };
      }

      return {
        is_valid: false,
        error_code: 'VERIFICATION_FAILED',
        error_message: 'Google Play 검증에 실패했습니다.',
      };
    } catch (error) {
      logger.error('Google purchase validation error:', error);
      return {
        is_valid: false,
        error_code: 'GOOGLE_API_ERROR',
        error_message: 'Google Play API 오류가 발생했습니다.',
      };
    }
  }

  /**
   * Validate Apple App Store purchase
   */
  private async validateApplePurchase(request: PurchaseValidationRequest): Promise<PurchaseValidationResult> {
    try {
      // Apple verification endpoint
      const verifyUrl = config.nodeEnv === 'production'
        ? 'https://buy.itunes.apple.com/verifyReceipt'
        : 'https://sandbox.itunes.apple.com/verifyReceipt';

      const response = await axios.post(verifyUrl, {
        'receipt-data': request.receipt,
        'password': config.iap.apple.sharedSecret,
        'exclude-old-transactions': true,
      });

      const { status, receipt } = response.data;

      // Apple status codes
      // 0: Valid
      // 21000-21010: Various errors
      if (status === 0) {
        // Verify bundle ID
        if (receipt.bundle_id !== config.iap.apple.bundleId) {
          return {
            is_valid: false,
            error_code: 'BUNDLE_MISMATCH',
            error_message: '번들 ID가 일치하지 않습니다.',
          };
        }

        // Find the matching in-app purchase
        const inAppPurchases = receipt.in_app || [];
        const matchingPurchase = inAppPurchases.find(
          (p: any) => p.product_id === request.product_id && p.transaction_id === request.transaction_id
        );

        if (matchingPurchase) {
          return {
            is_valid: true,
            order_id: matchingPurchase.original_transaction_id,
          };
        }

        return {
          is_valid: false,
          error_code: 'PURCHASE_NOT_FOUND',
          error_message: '구매 내역을 찾을 수 없습니다.',
        };
      } else if (status === 21007) {
        // Sandbox receipt on production - retry with sandbox
        return this.validateApplePurchaseSandbox(request);
      } else {
        return {
          is_valid: false,
          error_code: `APPLE_ERROR_${status}`,
          error_message: `Apple 검증 실패 (코드: ${status})`,
        };
      }
    } catch (error) {
      logger.error('Apple purchase validation error:', error);
      return {
        is_valid: false,
        error_code: 'APPLE_API_ERROR',
        error_message: 'Apple 검증 API 오류가 발생했습니다.',
      };
    }
  }

  /**
   * Validate Apple purchase against sandbox (for TestFlight/development)
   */
  private async validateApplePurchaseSandbox(request: PurchaseValidationRequest): Promise<PurchaseValidationResult> {
    try {
      const response = await axios.post('https://sandbox.itunes.apple.com/verifyReceipt', {
        'receipt-data': request.receipt,
        'password': config.iap.apple.sharedSecret,
        'exclude-old-transactions': true,
      });

      const { status, receipt } = response.data;

      if (status === 0 && receipt) {
        return {
          is_valid: true,
          order_id: request.transaction_id,
        };
      }

      return {
        is_valid: false,
        error_code: `APPLE_SANDBOX_ERROR_${status}`,
        error_message: `Apple Sandbox 검증 실패 (코드: ${status})`,
      };
    } catch (error) {
      return {
        is_valid: false,
        error_code: 'APPLE_SANDBOX_API_ERROR',
        error_message: 'Apple Sandbox API 오류가 발생했습니다.',
      };
    }
  }

  /**
   * Get purchase history for user
   */
  async getPurchaseHistory(userId: string): Promise<any[]> {
    const result = await Database.query(
      `SELECT product_id, transaction_id, platform, created_at, amount, currency, status
       FROM purchases
       WHERE user_id = $1 AND status = 'completed'
       ORDER BY created_at DESC
       LIMIT 100`,
      [userId]
    );

    return result.rows.map((row) => ({
      product_id: row.product_id,
      transaction_id: row.transaction_id,
      platform: row.platform,
      purchase_time: row.created_at?.toISOString(),
      amount: row.amount,
      currency: row.currency,
      status: row.status,
    }));
  }

  /**
   * Process refund (called by webhook)
   */
  async processRefund(transactionId: string, reason: string): Promise<void> {
    await Database.query(
      `UPDATE purchases SET status = 'refunded', validation_response = validation_response || $2
       WHERE transaction_id = $1`,
      [transactionId, JSON.stringify({ refund_reason: reason, refunded_at: new Date().toISOString() })]
    );

    logger.info(`Purchase refunded: ${transactionId}, reason: ${reason}`);

    // TODO: Revoke granted items/currency
  }
}

export const iapService = new IAPService();
export default iapService;
