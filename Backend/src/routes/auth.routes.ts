import { Router } from 'express';
import { authController } from '../controllers/auth.controller';
import { authenticate } from '../middlewares/auth.middleware';
import { validate } from '../middlewares/validate.middleware';
import Joi from 'joi';

const router = Router();

// Validation schemas
const emailRegisterSchema = Joi.object({
  email: Joi.string().email().required().messages({
    'string.email': '유효한 이메일 주소를 입력해주세요.',
    'any.required': '이메일은 필수입니다.',
  }),
  password: Joi.string().min(6).max(100).required().messages({
    'string.min': '비밀번호는 최소 6자 이상이어야 합니다.',
    'any.required': '비밀번호는 필수입니다.',
  }),
  nickname: Joi.string().min(2).max(20).required().messages({
    'string.min': '닉네임은 최소 2자 이상이어야 합니다.',
    'string.max': '닉네임은 최대 20자까지 가능합니다.',
    'any.required': '닉네임은 필수입니다.',
  }),
});

const emailLoginSchema = Joi.object({
  email: Joi.string().email().required(),
  password: Joi.string().required(),
});

const socialLoginSchema = Joi.object({
  provider: Joi.string().valid('kakao', 'google', 'apple').required().messages({
    'any.only': '지원하지 않는 로그인 방식입니다.',
  }),
  access_token: Joi.string().required().messages({
    'any.required': '액세스 토큰이 필요합니다.',
  }),
});

const guestLoginSchema = Joi.object({
  device_id: Joi.string().required().messages({
    'any.required': '디바이스 ID가 필요합니다.',
  }),
  platform: Joi.string().valid('Android', 'IPhonePlayer', 'WindowsEditor', 'OSXEditor', 'WindowsPlayer', 'LinuxPlayer').required(),
  // 추가 기기 정보 (선택)
  device_fingerprint: Joi.string().optional(),
  device_model: Joi.string().optional(),
  os_version: Joi.string().optional(),
  app_version: Joi.string().optional(),
});

const refreshSchema = Joi.object({
  refresh_token: Joi.string().required(),
});

const nicknameSchema = Joi.object({
  nickname: Joi.string().min(2).max(20).required(),
});

// Routes
router.post('/register/email', validate(emailRegisterSchema), authController.registerEmail.bind(authController));
router.post('/login/email', validate(emailLoginSchema), authController.loginEmail.bind(authController));
router.post('/login/social', validate(socialLoginSchema), authController.loginSocial.bind(authController));
router.post('/login/guest', validate(guestLoginSchema), authController.loginGuest.bind(authController));
router.post('/refresh', validate(refreshSchema), authController.refreshToken.bind(authController));
router.post('/logout', authenticate, authController.logout.bind(authController));
router.get('/me', authenticate, authController.getMe.bind(authController));
router.put('/nickname', authenticate, validate(nicknameSchema), authController.updateNickname.bind(authController));
router.delete('/account', authenticate, authController.deleteAccount.bind(authController));

export default router;
