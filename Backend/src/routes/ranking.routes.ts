import { Router, Request, Response, NextFunction } from 'express';
import { rankingService, RankingType, RankingPeriod } from '../services/ranking.service';
import { authenticate, optionalAuth } from '../middlewares/auth.middleware';
import { validate, validateQuery } from '../middlewares/validate.middleware';
import { AppError } from '../utils/errors';
import Joi from 'joi';

const router = Router();

// Validation schemas
const submitSchema = Joi.object({
  ranking_type: Joi.string().valid('total_power', 'level', 'highest_stage', 'gold', 'total_damage', 'boss_kills', 'play_time').required(),
  score: Joi.number().integer().min(0).required(),
  timestamp: Joi.string().optional(),
});

const querySchema = Joi.object({
  page: Joi.number().integer().min(0).default(0),
  size: Joi.number().integer().min(1).max(100).default(50),
});

const rangeSchema = Joi.object({
  range: Joi.number().integer().min(1).max(50).default(5),
});

/**
 * GET /ranking/:type
 * Get ranking list
 */
router.get('/:type', optionalAuth, validateQuery(querySchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    const rankingType = req.params.type as RankingType;
    const page = Number(req.query.page) || 0;
    const size = Number(req.query.size) || 50;

    const ranking = await rankingService.getRanking(rankingType, 'all_time', page, size);

    res.json({
      success: true,
      ...ranking,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /ranking/:type/weekly
 * Get weekly ranking
 */
router.get('/:type/weekly', optionalAuth, validateQuery(querySchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    const rankingType = req.params.type as RankingType;
    const page = Number(req.query.page) || 0;
    const size = Number(req.query.size) || 50;

    const ranking = await rankingService.getRanking(rankingType, 'weekly', page, size);

    res.json({
      success: true,
      ...ranking,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /ranking/:type/me
 * Get my rank
 */
router.get('/:type/me', authenticate, async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const rankingType = req.params.type as RankingType;
    const myRank = await rankingService.getMyRank(req.user.userId, rankingType);

    res.json({
      success: true,
      ...myRank,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /ranking/:type/around
 * Get ranking around me
 */
router.get('/:type/around', authenticate, validateQuery(rangeSchema), async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const rankingType = req.params.type as RankingType;
    const range = Number(req.query.range) || 5;

    const entries = await rankingService.getRankingAroundMe(req.user.userId, rankingType, range);

    res.json({
      success: true,
      entries,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /ranking/:type/submit
 * Submit score
 */
router.post('/:type/submit', authenticate, async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const rankingType = req.params.type as RankingType;
    const { score } = req.body;

    if (typeof score !== 'number' || score < 0) {
      throw new AppError('유효한 점수를 입력해주세요.', 400);
    }

    const result = await rankingService.submitScore(req.user.userId, rankingType, score);

    res.json({
      success: true,
      ...result,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * GET /ranking/:type/rewards
 * Get reward tiers
 */
router.get('/:type/rewards', async (req: Request, res: Response, next: NextFunction) => {
  try {
    const rankingType = req.params.type as RankingType;
    const rewards = await rankingService.getRewardTiers(rankingType);

    res.json({
      success: true,
      rewards,
    });
  } catch (error) {
    next(error);
  }
});

/**
 * POST /ranking/:type/claim
 * Claim ranking reward
 */
router.post('/:type/claim', authenticate, async (req: Request, res: Response, next: NextFunction) => {
  try {
    if (!req.user) {
      throw new AppError('인증이 필요합니다.', 401);
    }

    const rankingType = req.params.type as RankingType;
    const result = await rankingService.claimReward(req.user.userId, rankingType, 'all_time');

    res.json({
      success: true,
      ...result,
    });
  } catch (error) {
    next(error);
  }
});

export default router;
