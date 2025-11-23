import dotenv from 'dotenv';
dotenv.config();

// Parse DATABASE_URL if provided (Railway, Heroku, etc.)
function parseDatabaseUrl(): { host: string; port: number; name: string; user: string; password: string } | null {
  const url = process.env.DATABASE_URL;
  if (!url) return null;

  try {
    const parsed = new URL(url);
    return {
      host: parsed.hostname,
      port: parseInt(parsed.port || '5432'),
      name: parsed.pathname.slice(1), // Remove leading '/'
      user: parsed.username,
      password: parsed.password,
    };
  } catch {
    console.error('Failed to parse DATABASE_URL');
    return null;
  }
}

// Parse REDIS_URL if provided (Railway, Heroku, etc.)
function parseRedisUrl(): { host: string; port: number; password: string | undefined } | null {
  const url = process.env.REDIS_URL;
  if (!url) return null;

  try {
    const parsed = new URL(url);
    return {
      host: parsed.hostname,
      port: parseInt(parsed.port || '6379'),
      password: parsed.password || undefined,
    };
  } catch {
    console.error('Failed to parse REDIS_URL');
    return null;
  }
}

const dbConfig = parseDatabaseUrl();
const redisConfig = parseRedisUrl();

export const config = {
  // Server
  nodeEnv: process.env.NODE_ENV || 'development',
  port: parseInt(process.env.PORT || '3000'),
  apiVersion: process.env.API_VERSION || 'v1',

  // Database (DATABASE_URL takes priority)
  database: {
    host: dbConfig?.host || process.env.DB_HOST || 'localhost',
    port: dbConfig?.port || parseInt(process.env.DB_PORT || '5432'),
    name: dbConfig?.name || process.env.DB_NAME || 'hidden_growth',
    user: dbConfig?.user || process.env.DB_USER || 'postgres',
    password: dbConfig?.password || process.env.DB_PASSWORD || '',
  },

  // Redis (REDIS_URL takes priority)
  redis: {
    host: redisConfig?.host || process.env.REDIS_HOST || 'localhost',
    port: redisConfig?.port || parseInt(process.env.REDIS_PORT || '6379'),
    password: redisConfig?.password || process.env.REDIS_PASSWORD || undefined,
  },

  // JWT
  jwt: {
    secret: process.env.JWT_SECRET || 'default_secret_change_me',
    expiresIn: process.env.JWT_EXPIRES_IN || '7d',
    refreshSecret: process.env.JWT_REFRESH_SECRET || 'default_refresh_secret',
    refreshExpiresIn: process.env.JWT_REFRESH_EXPIRES_IN || '30d',
  },

  // OAuth - Kakao
  kakao: {
    restApiKey: process.env.KAKAO_REST_API_KEY || '',
    clientSecret: process.env.KAKAO_CLIENT_SECRET || '',
  },

  // OAuth - Google
  google: {
    clientId: process.env.GOOGLE_CLIENT_ID || '',
    clientSecret: process.env.GOOGLE_CLIENT_SECRET || '',
  },

  // IAP
  iap: {
    google: {
      serviceAccountEmail: process.env.GOOGLE_PLAY_SERVICE_ACCOUNT_EMAIL || '',
      serviceAccountKey: process.env.GOOGLE_PLAY_SERVICE_ACCOUNT_KEY || '',
      packageName: process.env.GOOGLE_PLAY_PACKAGE_NAME || '',
    },
    apple: {
      sharedSecret: process.env.APPLE_SHARED_SECRET || '',
      bundleId: process.env.APPLE_BUNDLE_ID || '',
    },
  },

  // Security
  security: {
    encryptionKey: process.env.ENCRYPTION_KEY || 'default_32_char_encryption_key!!',
    hmacSecret: process.env.HMAC_SECRET || 'default_hmac_secret',
  },

  // Rate Limiting
  rateLimit: {
    windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS || '900000'),
    maxRequests: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS || '100'),
  },

  // Logging
  logLevel: process.env.LOG_LEVEL || 'debug',
};

export default config;
