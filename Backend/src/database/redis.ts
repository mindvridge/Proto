import { createClient, RedisClientType } from 'redis';
import config from '../config';
import { logger } from '../utils/logger';

class Redis {
  private client: RedisClientType | null = null;

  async initialize(): Promise<void> {
    this.client = createClient({
      socket: {
        host: config.redis.host,
        port: config.redis.port,
      },
      password: config.redis.password || undefined,
    });

    this.client.on('error', (err) => {
      logger.error('Redis error:', err);
    });

    this.client.on('connect', () => {
      logger.info('Redis connected');
    });

    await this.client.connect();
  }

  async get(key: string): Promise<string | null> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.get(key);
  }

  async set(key: string, value: string, ttlSeconds?: number): Promise<void> {
    if (!this.client) throw new Error('Redis not initialized');
    if (ttlSeconds) {
      await this.client.setEx(key, ttlSeconds, value);
    } else {
      await this.client.set(key, value);
    }
  }

  async del(key: string): Promise<void> {
    if (!this.client) throw new Error('Redis not initialized');
    await this.client.del(key);
  }

  async exists(key: string): Promise<boolean> {
    if (!this.client) throw new Error('Redis not initialized');
    const result = await this.client.exists(key);
    return result === 1;
  }

  async incr(key: string): Promise<number> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.incr(key);
  }

  async expire(key: string, seconds: number): Promise<void> {
    if (!this.client) throw new Error('Redis not initialized');
    await this.client.expire(key, seconds);
  }

  // Sorted Set operations for rankings
  async zadd(key: string, score: number, member: string): Promise<void> {
    if (!this.client) throw new Error('Redis not initialized');
    await this.client.zAdd(key, { score, value: member });
  }

  async zrevrank(key: string, member: string): Promise<number | null> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.zRevRank(key, member);
  }

  async zscore(key: string, member: string): Promise<number | null> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.zScore(key, member);
  }

  async zrevrange(key: string, start: number, stop: number): Promise<string[]> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.zRange(key, start, stop, { REV: true });
  }

  async zrevrangeWithScores(key: string, start: number, stop: number): Promise<{ value: string; score: number }[]> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.zRangeWithScores(key, start, stop, { REV: true });
  }

  async zcard(key: string): Promise<number> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.zCard(key);
  }

  async zcount(key: string, min: number | string, max: number | string): Promise<number> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.zCount(key, min, max);
  }

  // Hash operations for caching
  async hset(key: string, field: string, value: string): Promise<void> {
    if (!this.client) throw new Error('Redis not initialized');
    await this.client.hSet(key, field, value);
  }

  async hget(key: string, field: string): Promise<string | undefined> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.hGet(key, field);
  }

  async hgetall(key: string): Promise<Record<string, string>> {
    if (!this.client) throw new Error('Redis not initialized');
    return this.client.hGetAll(key);
  }

  async hdel(key: string, field: string): Promise<void> {
    if (!this.client) throw new Error('Redis not initialized');
    await this.client.hDel(key, field);
  }

  async close(): Promise<void> {
    if (this.client) {
      await this.client.quit();
      logger.info('Redis connection closed');
    }
  }
}

export const RedisClient = new Redis();
export default RedisClient;
