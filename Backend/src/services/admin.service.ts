import { Database } from '../database/connection';
import { RedisClient } from '../database/redis';
import { logger } from '../utils/logger';

export interface DashboardStats {
  totalUsers: number;
  activeUsersToday: number;
  newUsersToday: number;
  totalSaves: number;
  totalPurchases: number;
  totalRevenue: number;
  averagePlayTime: number;
}

export interface UserListItem {
  id: string;
  username: string;
  email: string | null;
  provider: string;
  created_at: Date;
  last_login: Date | null;
  total_playtime: number;
  purchase_count: number;
}

export interface RecentActivity {
  type: 'login' | 'save' | 'purchase' | 'register';
  user_id: string;
  username: string;
  details: string;
  timestamp: Date;
}

class AdminService {
  // 대시보드 통계
  async getDashboardStats(): Promise<DashboardStats> {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // 전체 유저 수
    const totalUsersResult = await Database.query<{ count: string }>(
      'SELECT COUNT(*) as count FROM users'
    );

    // 오늘 활성 유저 (로그인한 유저)
    const activeUsersTodayResult = await Database.query<{ count: string }>(
      'SELECT COUNT(*) as count FROM users WHERE last_login >= $1',
      [today]
    );

    // 오늘 신규 가입
    const newUsersTodayResult = await Database.query<{ count: string }>(
      'SELECT COUNT(*) as count FROM users WHERE created_at >= $1',
      [today]
    );

    // 전체 세이브 수
    const totalSavesResult = await Database.query<{ count: string }>(
      'SELECT COUNT(*) as count FROM game_saves'
    );

    // 전체 구매 수 및 매출
    const purchaseResult = await Database.query<{ count: string; revenue: string }>(
      `SELECT COUNT(*) as count, COALESCE(SUM(amount), 0) as revenue
       FROM purchase_history
       WHERE status = 'completed'`
    );

    return {
      totalUsers: parseInt(totalUsersResult.rows[0]?.count || '0'),
      activeUsersToday: parseInt(activeUsersTodayResult.rows[0]?.count || '0'),
      newUsersToday: parseInt(newUsersTodayResult.rows[0]?.count || '0'),
      totalSaves: parseInt(totalSavesResult.rows[0]?.count || '0'),
      totalPurchases: parseInt(purchaseResult.rows[0]?.count || '0'),
      totalRevenue: parseFloat(purchaseResult.rows[0]?.revenue || '0'),
      averagePlayTime: 0, // 추후 구현
    };
  }

  // 유저 목록
  async getUsers(page: number = 1, limit: number = 20, search?: string): Promise<{
    users: UserListItem[];
    total: number;
    page: number;
    totalPages: number;
  }> {
    const offset = (page - 1) * limit;
    let whereClause = '';
    const params: any[] = [];

    if (search) {
      whereClause = 'WHERE username ILIKE $1 OR email ILIKE $1';
      params.push(`%${search}%`);
    }

    // 전체 수
    const countResult = await Database.query<{ count: string }>(
      `SELECT COUNT(*) as count FROM users ${whereClause}`,
      params
    );
    const total = parseInt(countResult.rows[0]?.count || '0');

    // 유저 목록
    const usersResult = await Database.query<UserListItem>(
      `SELECT
        u.id, u.username, u.email, u.provider, u.created_at, u.last_login,
        COALESCE(s.save_count, 0) as total_playtime,
        COALESCE(p.purchase_count, 0) as purchase_count
      FROM users u
      LEFT JOIN (
        SELECT user_id, COUNT(*) as save_count FROM game_saves GROUP BY user_id
      ) s ON u.id = s.user_id
      LEFT JOIN (
        SELECT user_id, COUNT(*) as purchase_count FROM purchase_history WHERE status = 'completed' GROUP BY user_id
      ) p ON u.id = p.user_id
      ${whereClause}
      ORDER BY u.created_at DESC
      LIMIT $${params.length + 1} OFFSET $${params.length + 2}`,
      [...params, limit, offset]
    );

    return {
      users: usersResult.rows,
      total,
      page,
      totalPages: Math.ceil(total / limit),
    };
  }

  // 유저 상세 정보
  async getUserDetail(userId: string): Promise<any> {
    const userResult = await Database.query(
      `SELECT * FROM users WHERE id = $1`,
      [userId]
    );

    if (userResult.rows.length === 0) {
      return null;
    }

    const user = userResult.rows[0];

    // 세이브 데이터
    const savesResult = await Database.query(
      `SELECT slot, updated_at,
        (save_data->>'gold') as gold,
        (save_data->>'stage') as stage,
        (save_data->>'power') as power
       FROM game_saves WHERE user_id = $1 ORDER BY updated_at DESC`,
      [userId]
    );

    // 구매 내역
    const purchasesResult = await Database.query(
      `SELECT * FROM purchase_history WHERE user_id = $1 ORDER BY created_at DESC LIMIT 20`,
      [userId]
    );

    return {
      ...user,
      saves: savesResult.rows,
      purchases: purchasesResult.rows,
    };
  }

  // 최근 활동
  async getRecentActivities(limit: number = 50): Promise<RecentActivity[]> {
    // 최근 로그인
    const loginResult = await Database.query<{ id: string; username: string; last_login: Date }>(
      `SELECT id, username, last_login FROM users
       WHERE last_login IS NOT NULL
       ORDER BY last_login DESC LIMIT $1`,
      [limit]
    );

    // 최근 세이브
    const saveResult = await Database.query<{ user_id: string; username: string; updated_at: Date; slot: number }>(
      `SELECT gs.user_id, u.username, gs.updated_at, gs.slot
       FROM game_saves gs
       JOIN users u ON gs.user_id = u.id
       ORDER BY gs.updated_at DESC LIMIT $1`,
      [limit]
    );

    // 최근 구매
    const purchaseResult = await Database.query<{ user_id: string; username: string; created_at: Date; product_id: string; amount: number }>(
      `SELECT ph.user_id, u.username, ph.created_at, ph.product_id, ph.amount
       FROM purchase_history ph
       JOIN users u ON ph.user_id = u.id
       WHERE ph.status = 'completed'
       ORDER BY ph.created_at DESC LIMIT $1`,
      [limit]
    );

    const activities: RecentActivity[] = [];

    loginResult.rows.forEach(row => {
      activities.push({
        type: 'login',
        user_id: row.id,
        username: row.username,
        details: '로그인',
        timestamp: row.last_login,
      });
    });

    saveResult.rows.forEach(row => {
      activities.push({
        type: 'save',
        user_id: row.user_id,
        username: row.username,
        details: `슬롯 ${row.slot} 저장`,
        timestamp: row.updated_at,
      });
    });

    purchaseResult.rows.forEach(row => {
      activities.push({
        type: 'purchase',
        user_id: row.user_id,
        username: row.username,
        details: `${row.product_id} 구매 ($${row.amount})`,
        timestamp: row.created_at,
      });
    });

    // 시간순 정렬
    activities.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());

    return activities.slice(0, limit);
  }

  // 랭킹 데이터
  async getRankingData(type: string, limit: number = 100): Promise<any[]> {
    const redis = RedisClient.getClient();
    const key = `ranking:${type}`;

    const rankings = await redis.zrevrange(key, 0, limit - 1, 'WITHSCORES');
    const result: any[] = [];

    for (let i = 0; i < rankings.length; i += 2) {
      const userId = rankings[i];
      const score = parseFloat(rankings[i + 1]);

      // 유저 정보 조회
      const userResult = await Database.query<{ username: string }>(
        'SELECT username FROM users WHERE id = $1',
        [userId]
      );

      result.push({
        rank: Math.floor(i / 2) + 1,
        user_id: userId,
        username: userResult.rows[0]?.username || 'Unknown',
        score,
      });
    }

    return result;
  }

  // 일별 통계
  async getDailyStats(days: number = 30): Promise<any[]> {
    const result = await Database.query(
      `SELECT
        DATE(created_at) as date,
        COUNT(*) as new_users
       FROM users
       WHERE created_at >= NOW() - INTERVAL '${days} days'
       GROUP BY DATE(created_at)
       ORDER BY date DESC`
    );

    return result.rows;
  }

  // 구매 통계
  async getPurchaseStats(days: number = 30): Promise<any[]> {
    const result = await Database.query(
      `SELECT
        DATE(created_at) as date,
        COUNT(*) as purchase_count,
        SUM(amount) as revenue
       FROM purchase_history
       WHERE status = 'completed' AND created_at >= NOW() - INTERVAL '${days} days'
       GROUP BY DATE(created_at)
       ORDER BY date DESC`
    );

    return result.rows;
  }

  // 유저 밴/언밴
  async setUserBan(userId: string, banned: boolean): Promise<boolean> {
    const result = await Database.query(
      'UPDATE users SET is_banned = $1 WHERE id = $2 RETURNING id',
      [banned, userId]
    );
    return result.rowCount > 0;
  }

  // 서버 상태
  async getServerStatus(): Promise<any> {
    const redis = RedisClient.getClient();

    // Redis 정보
    const redisInfo = await redis.info();

    // DB 연결 상태
    let dbStatus = 'connected';
    try {
      await Database.query('SELECT 1');
    } catch {
      dbStatus = 'disconnected';
    }

    return {
      database: dbStatus,
      redis: 'connected',
      uptime: process.uptime(),
      memory: process.memoryUsage(),
      nodeVersion: process.version,
    };
  }
}

export const adminService = new AdminService();
export default adminService;
