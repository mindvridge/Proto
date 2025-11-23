import { Database } from '../database/connection';
import { RedisClient } from '../database/redis';
import { logger } from '../utils/logger';
import { AppError, NotFoundError } from '../utils/errors';

export type RankingType = 'total_power' | 'level' | 'highest_stage' | 'gold' | 'total_damage' | 'boss_kills' | 'play_time';
export type RankingPeriod = 'all_time' | 'weekly' | 'daily' | 'season';

export interface RankingEntry {
  rank: number;
  user_id: string;
  nickname: string;
  profile_image: string | null;
  score: number;
  level: number;
  guild_name: string | null;
  rank_change: number;
}

export interface RankingData {
  type: RankingType;
  period: RankingPeriod;
  entries: RankingEntry[];
  total_count: number;
  current_page: number;
  total_pages: number;
  last_updated: string;
}

export interface MyRankInfo {
  rank: number;
  score: number;
  percentile: number;
  rank_change: number;
  score_to_next: number;
}

class RankingService {
  private readonly REDIS_KEY_PREFIX = 'ranking:';
  private readonly PAGE_SIZE = 50;

  /**
   * Submit score to ranking
   */
  async submitScore(userId: string, rankingType: RankingType, score: number): Promise<{ new_rank: number; previous_rank: number; is_new_record: boolean }> {
    const redisKey = `${this.REDIS_KEY_PREFIX}${rankingType}:all_time`;

    // Get previous rank
    const previousRank = await RedisClient.zrevrank(redisKey, userId);
    const previousScore = await RedisClient.zscore(redisKey, userId);

    const isNewRecord = previousScore === null || score > previousScore;

    // Only update if new record
    if (isNewRecord) {
      // Update Redis
      await RedisClient.zadd(redisKey, score, userId);

      // Update PostgreSQL
      await Database.query(
        `INSERT INTO rankings (user_id, ranking_type, score, period)
         VALUES ($1, $2, $3, 'all_time')
         ON CONFLICT (user_id, ranking_type, period, period_start)
         DO UPDATE SET score = $3, previous_rank = rankings.rank, updated_at = CURRENT_TIMESTAMP`,
        [userId, rankingType, score]
      );
    }

    // Get new rank
    const newRank = await RedisClient.zrevrank(redisKey, userId);

    return {
      new_rank: newRank !== null ? newRank + 1 : -1,
      previous_rank: previousRank !== null ? previousRank + 1 : -1,
      is_new_record: isNewRecord,
    };
  }

  /**
   * Get ranking list
   */
  async getRanking(rankingType: RankingType, period: RankingPeriod = 'all_time', page: number = 0, pageSize: number = 50): Promise<RankingData> {
    const redisKey = `${this.REDIS_KEY_PREFIX}${rankingType}:${period}`;

    const start = page * pageSize;
    const stop = start + pageSize - 1;

    // Get from Redis
    const rankedUsers = await RedisClient.zrevrangeWithScores(redisKey, start, stop);
    const totalCount = await RedisClient.zcard(redisKey);

    // Get user details
    const entries: RankingEntry[] = [];

    for (let i = 0; i < rankedUsers.length; i++) {
      const { value: usersId, score } = rankedUsers[i];

      const userResult = await Database.query(
        'SELECT id, nickname, profile_image, level FROM users WHERE id = $1',
        [usersId]
      );

      if (userResult.rows.length > 0) {
        const user = userResult.rows[0];

        // Get previous rank from DB
        const rankResult = await Database.query(
          'SELECT previous_rank FROM rankings WHERE user_id = $1 AND ranking_type = $2 AND period = $3',
          [usersId, rankingType, period]
        );

        const previousRank = rankResult.rows[0]?.previous_rank || start + i + 1;
        const currentRank = start + i + 1;

        entries.push({
          rank: currentRank,
          user_id: user.id,
          nickname: user.nickname,
          profile_image: user.profile_image,
          score: Math.floor(score),
          level: user.level,
          guild_name: null, // TODO: Add guild support
          rank_change: previousRank - currentRank,
        });
      }
    }

    return {
      type: rankingType,
      period,
      entries,
      total_count: totalCount,
      current_page: page,
      total_pages: Math.ceil(totalCount / pageSize),
      last_updated: new Date().toISOString(),
    };
  }

  /**
   * Get my rank
   */
  async getMyRank(userId: string, rankingType: RankingType, period: RankingPeriod = 'all_time'): Promise<MyRankInfo> {
    const redisKey = `${this.REDIS_KEY_PREFIX}${rankingType}:${period}`;

    const rank = await RedisClient.zrevrank(redisKey, userId);
    const score = await RedisClient.zscore(redisKey, userId);
    const totalCount = await RedisClient.zcard(redisKey);

    if (rank === null || score === null) {
      return {
        rank: -1,
        score: 0,
        percentile: 100,
        rank_change: 0,
        score_to_next: 0,
      };
    }

    // Calculate percentile
    const percentile = totalCount > 0 ? Math.floor(((rank + 1) / totalCount) * 100) : 100;

    // Get previous rank
    const rankResult = await Database.query(
      'SELECT previous_rank FROM rankings WHERE user_id = $1 AND ranking_type = $2 AND period = $3',
      [userId, rankingType, period]
    );
    const previousRank = rankResult.rows[0]?.previous_rank || rank + 1;

    // Get score to next rank
    let scoreToNext = 0;
    if (rank > 0) {
      const higherRanked = await RedisClient.zrevrangeWithScores(redisKey, rank - 1, rank - 1);
      if (higherRanked.length > 0) {
        scoreToNext = Math.floor(higherRanked[0].score - score) + 1;
      }
    }

    return {
      rank: rank + 1,
      score: Math.floor(score),
      percentile,
      rank_change: previousRank - (rank + 1),
      score_to_next: scoreToNext,
    };
  }

  /**
   * Get ranking around user
   */
  async getRankingAroundMe(userId: string, rankingType: RankingType, range: number = 5): Promise<RankingEntry[]> {
    const redisKey = `${this.REDIS_KEY_PREFIX}${rankingType}:all_time`;

    const myRank = await RedisClient.zrevrank(redisKey, userId);

    if (myRank === null) {
      return [];
    }

    const start = Math.max(0, myRank - range);
    const stop = myRank + range;

    const rankedUsers = await RedisClient.zrevrangeWithScores(redisKey, start, stop);

    const entries: RankingEntry[] = [];

    for (let i = 0; i < rankedUsers.length; i++) {
      const { value: usersId, score } = rankedUsers[i];

      const userResult = await Database.query(
        'SELECT id, nickname, profile_image, level FROM users WHERE id = $1',
        [usersId]
      );

      if (userResult.rows.length > 0) {
        const user = userResult.rows[0];

        entries.push({
          rank: start + i + 1,
          user_id: user.id,
          nickname: user.nickname,
          profile_image: user.profile_image,
          score: Math.floor(score),
          level: user.level,
          guild_name: null,
          rank_change: 0,
        });
      }
    }

    return entries;
  }

  /**
   * Get ranking rewards info
   */
  async getRewardTiers(rankingType: RankingType): Promise<any[]> {
    const result = await Database.query(
      `SELECT min_rank, max_rank, reward_type, reward_amount, description
       FROM ranking_reward_tiers
       WHERE ranking_type = $1
       ORDER BY min_rank`,
      [rankingType]
    );

    return result.rows;
  }

  /**
   * Claim ranking reward
   */
  async claimReward(userId: string, rankingType: RankingType, period: RankingPeriod): Promise<{ final_rank: number; reward_type: string; reward_amount: number }> {
    // Check if already claimed
    const claimed = await Database.query(
      `SELECT id FROM ranking_rewards
       WHERE user_id = $1 AND ranking_type = $2 AND period = $3 AND period_start = (
         SELECT MAX(period_start) FROM ranking_rewards WHERE ranking_type = $2 AND period = $3
       )`,
      [userId, rankingType, period]
    );

    if (claimed.rows.length > 0) {
      throw new AppError('이미 보상을 수령했습니다.', 400);
    }

    // Get user's rank
    const myRank = await this.getMyRank(userId, rankingType, period);

    if (myRank.rank === -1) {
      throw new AppError('랭킹에 참여하지 않았습니다.', 400);
    }

    // Find reward tier
    const tierResult = await Database.query(
      `SELECT reward_type, reward_amount FROM ranking_reward_tiers
       WHERE ranking_type = $1 AND $2 BETWEEN min_rank AND max_rank`,
      [rankingType, myRank.rank]
    );

    if (tierResult.rows.length === 0) {
      throw new AppError('받을 수 있는 보상이 없습니다.', 400);
    }

    const reward = tierResult.rows[0];

    // Record claim
    await Database.query(
      `INSERT INTO ranking_rewards (user_id, ranking_type, period, period_start, final_rank, reward_type, reward_amount)
       VALUES ($1, $2, $3, CURRENT_DATE, $4, $5, $6)`,
      [userId, rankingType, period, myRank.rank, reward.reward_type, reward.reward_amount]
    );

    // TODO: Actually grant reward (integrate with currency system)

    logger.info(`Ranking reward claimed: user=${userId}, type=${rankingType}, rank=${myRank.rank}, reward=${reward.reward_type} x${reward.reward_amount}`);

    return {
      final_rank: myRank.rank,
      reward_type: reward.reward_type,
      reward_amount: reward.reward_amount,
    };
  }

  /**
   * Sync Redis ranking from PostgreSQL (for initialization/recovery)
   */
  async syncRankingToRedis(rankingType: RankingType, period: RankingPeriod = 'all_time'): Promise<void> {
    const redisKey = `${this.REDIS_KEY_PREFIX}${rankingType}:${period}`;

    const result = await Database.query(
      `SELECT user_id, score FROM rankings
       WHERE ranking_type = $1 AND period = $2
       ORDER BY score DESC`,
      [rankingType, period]
    );

    for (const row of result.rows) {
      await RedisClient.zadd(redisKey, row.score, row.user_id);
    }

    logger.info(`Synced ${result.rows.length} entries to Redis for ${rankingType}:${period}`);
  }
}

export const rankingService = new RankingService();
export default rankingService;
