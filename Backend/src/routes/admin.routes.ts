import { Router, Request, Response } from 'express';
import { adminService } from '../services/admin.service';
import { adminAuth, basicAuth } from '../middlewares/admin.middleware';
import { logger } from '../utils/logger';

const router = Router();

// API 인증은 X-Admin-Key 헤더 사용
// 대시보드 페이지는 Basic Auth 사용

// 대시보드 통계
router.get('/stats', adminAuth, async (req: Request, res: Response) => {
  try {
    const stats = await adminService.getDashboardStats();
    res.json({ success: true, data: stats });
  } catch (error) {
    logger.error('Failed to get dashboard stats:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 유저 목록
router.get('/users', adminAuth, async (req: Request, res: Response) => {
  try {
    const page = parseInt(req.query.page as string) || 1;
    const limit = parseInt(req.query.limit as string) || 20;
    const search = req.query.search as string;

    const result = await adminService.getUsers(page, limit, search);
    res.json({ success: true, data: result });
  } catch (error) {
    logger.error('Failed to get users:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 유저 상세
router.get('/users/:userId', adminAuth, async (req: Request, res: Response) => {
  try {
    const { userId } = req.params;
    const user = await adminService.getUserDetail(userId);

    if (!user) {
      return res.status(404).json({ success: false, error: 'User not found' });
    }

    res.json({ success: true, data: user });
  } catch (error) {
    logger.error('Failed to get user detail:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 유저 밴/언밴
router.post('/users/:userId/ban', adminAuth, async (req: Request, res: Response) => {
  try {
    const { userId } = req.params;
    const { banned } = req.body;

    const result = await adminService.setUserBan(userId, banned);

    if (!result) {
      return res.status(404).json({ success: false, error: 'User not found' });
    }

    res.json({ success: true, message: banned ? 'User banned' : 'User unbanned' });
  } catch (error) {
    logger.error('Failed to ban/unban user:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 최근 활동
router.get('/activities', adminAuth, async (req: Request, res: Response) => {
  try {
    const limit = parseInt(req.query.limit as string) || 50;
    const activities = await adminService.getRecentActivities(limit);
    res.json({ success: true, data: activities });
  } catch (error) {
    logger.error('Failed to get activities:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 랭킹 데이터
router.get('/rankings/:type', adminAuth, async (req: Request, res: Response) => {
  try {
    const { type } = req.params;
    const limit = parseInt(req.query.limit as string) || 100;
    const rankings = await adminService.getRankingData(type, limit);
    res.json({ success: true, data: rankings });
  } catch (error) {
    logger.error('Failed to get rankings:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 일별 통계
router.get('/stats/daily', adminAuth, async (req: Request, res: Response) => {
  try {
    const days = parseInt(req.query.days as string) || 30;
    const stats = await adminService.getDailyStats(days);
    res.json({ success: true, data: stats });
  } catch (error) {
    logger.error('Failed to get daily stats:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 구매 통계
router.get('/stats/purchases', adminAuth, async (req: Request, res: Response) => {
  try {
    const days = parseInt(req.query.days as string) || 30;
    const stats = await adminService.getPurchaseStats(days);
    res.json({ success: true, data: stats });
  } catch (error) {
    logger.error('Failed to get purchase stats:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

// 서버 상태
router.get('/server/status', adminAuth, async (req: Request, res: Response) => {
  try {
    const status = await adminService.getServerStatus();
    res.json({ success: true, data: status });
  } catch (error) {
    logger.error('Failed to get server status:', error);
    res.status(500).json({ success: false, error: 'Internal server error' });
  }
});

export default router;
