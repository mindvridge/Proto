-- Hidden Growth Backend Database Schema
-- PostgreSQL 14+

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =====================================================
-- USERS & AUTHENTICATION
-- =====================================================

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE,
    password_hash VARCHAR(255),
    nickname VARCHAR(50) NOT NULL,
    profile_image VARCHAR(500),
    level INTEGER DEFAULT 1,

    -- OAuth identifiers
    kakao_id VARCHAR(100) UNIQUE,
    google_id VARCHAR(100) UNIQUE,
    apple_id VARCHAR(100) UNIQUE,

    -- Guest/Device
    device_id VARCHAR(255),

    -- Status
    is_active BOOLEAN DEFAULT TRUE,
    is_banned BOOLEAN DEFAULT FALSE,
    ban_reason VARCHAR(500),
    ban_until TIMESTAMP,

    -- VIP
    is_vip BOOLEAN DEFAULT FALSE,
    vip_until TIMESTAMP,

    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP,

    -- Indexes
    CONSTRAINT email_or_oauth CHECK (
        email IS NOT NULL OR
        kakao_id IS NOT NULL OR
        google_id IS NOT NULL OR
        apple_id IS NOT NULL OR
        device_id IS NOT NULL
    )
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_kakao_id ON users(kakao_id);
CREATE INDEX idx_users_google_id ON users(google_id);
CREATE INDEX idx_users_device_id ON users(device_id);
CREATE INDEX idx_users_nickname ON users(nickname);

-- Refresh tokens table
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL,
    device_info VARCHAR(500),
    ip_address VARCHAR(45),
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP
);

CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_hash ON refresh_tokens(token_hash);

-- User devices table (for multi-device management)
CREATE TABLE user_devices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    device_fingerprint VARCHAR(255) NOT NULL,
    device_id VARCHAR(255),
    device_model VARCHAR(100),
    os_version VARCHAR(50),
    app_version VARCHAR(20),
    is_primary BOOLEAN DEFAULT FALSE,
    last_active_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(user_id, device_fingerprint)
);

CREATE INDEX idx_user_devices_user ON user_devices(user_id);

-- =====================================================
-- GAME SAVE DATA
-- =====================================================

-- Game saves table
CREATE TABLE game_saves (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Version control
    version INTEGER NOT NULL DEFAULT 1,
    checksum VARCHAR(64) NOT NULL,

    -- Player data (JSON for flexibility)
    player_data JSONB NOT NULL,
    stage_data JSONB,
    currency_data JSONB,
    inventory_data JSONB,
    achievement_data JSONB,

    -- Metadata
    play_time INTEGER DEFAULT 0,  -- seconds
    device_id VARCHAR(255),
    platform VARCHAR(20),

    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(user_id)  -- One active save per user
);

CREATE INDEX idx_game_saves_user ON game_saves(user_id);

-- Save backups table
CREATE TABLE save_backups (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    save_id UUID REFERENCES game_saves(id) ON DELETE SET NULL,

    version INTEGER NOT NULL,
    checksum VARCHAR(64) NOT NULL,

    player_data JSONB NOT NULL,
    stage_data JSONB,
    currency_data JSONB,

    backup_reason VARCHAR(50),  -- 'auto', 'manual', 'before_restore'
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_save_backups_user ON save_backups(user_id);
CREATE INDEX idx_save_backups_created ON save_backups(created_at);

-- =====================================================
-- RANKINGS
-- =====================================================

-- Rankings table (persistent storage, Redis for real-time)
CREATE TABLE rankings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ranking_type VARCHAR(50) NOT NULL,  -- 'total_power', 'level', 'highest_stage', etc.

    score BIGINT NOT NULL DEFAULT 0,
    rank INTEGER,
    previous_rank INTEGER,

    -- Period
    period VARCHAR(20) DEFAULT 'all_time',  -- 'all_time', 'weekly', 'daily', 'season'
    period_start DATE,
    period_end DATE,

    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(user_id, ranking_type, period, period_start)
);

CREATE INDEX idx_rankings_type_score ON rankings(ranking_type, score DESC);
CREATE INDEX idx_rankings_user ON rankings(user_id);
CREATE INDEX idx_rankings_period ON rankings(period, period_start);

-- Ranking rewards history
CREATE TABLE ranking_rewards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ranking_type VARCHAR(50) NOT NULL,
    period VARCHAR(20) NOT NULL,
    period_start DATE NOT NULL,

    final_rank INTEGER NOT NULL,
    reward_type VARCHAR(50) NOT NULL,
    reward_amount INTEGER NOT NULL,

    claimed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_ranking_rewards_user ON ranking_rewards(user_id);

-- =====================================================
-- IN-APP PURCHASES
-- =====================================================

-- Purchases table
CREATE TABLE purchases (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    -- Transaction info
    product_id VARCHAR(100) NOT NULL,
    transaction_id VARCHAR(255) NOT NULL UNIQUE,
    platform VARCHAR(20) NOT NULL,  -- 'google', 'apple'

    -- Receipt
    receipt TEXT NOT NULL,
    receipt_hash VARCHAR(64),

    -- Validation
    is_valid BOOLEAN DEFAULT FALSE,
    validation_response JSONB,
    validated_at TIMESTAMP,

    -- Amount
    amount DECIMAL(10, 2),
    currency VARCHAR(3),

    -- Status
    status VARCHAR(20) DEFAULT 'pending',  -- 'pending', 'completed', 'refunded', 'failed'

    -- Timestamps
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_purchases_user ON purchases(user_id);
CREATE INDEX idx_purchases_transaction ON purchases(transaction_id);
CREATE INDEX idx_purchases_status ON purchases(status);

-- =====================================================
-- SECURITY & MONITORING
-- =====================================================

-- Security violations table
CREATE TABLE security_violations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,

    violation_type VARCHAR(50) NOT NULL,
    details TEXT,
    severity VARCHAR(20) DEFAULT 'low',  -- 'low', 'medium', 'high', 'critical'

    device_fingerprint VARCHAR(255),
    ip_address VARCHAR(45),
    session_token VARCHAR(255),

    action_taken VARCHAR(50),  -- 'warn', 'suspend', 'ban'

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_security_violations_user ON security_violations(user_id);
CREATE INDEX idx_security_violations_type ON security_violations(violation_type);
CREATE INDEX idx_security_violations_created ON security_violations(created_at);

-- Login history table
CREATE TABLE login_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,

    login_type VARCHAR(20) NOT NULL,  -- 'email', 'kakao', 'google', 'guest'
    ip_address VARCHAR(45),
    device_info VARCHAR(500),

    success BOOLEAN NOT NULL,
    failure_reason VARCHAR(100),

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_login_history_user ON login_history(user_id);
CREATE INDEX idx_login_history_created ON login_history(created_at);

-- =====================================================
-- FUNCTIONS & TRIGGERS
-- =====================================================

-- Update timestamp function
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to tables
CREATE TRIGGER update_users_timestamp
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_game_saves_timestamp
    BEFORE UPDATE ON game_saves
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

CREATE TRIGGER update_purchases_timestamp
    BEFORE UPDATE ON purchases
    FOR EACH ROW EXECUTE FUNCTION update_updated_at();

-- Auto-create backup before save update
CREATE OR REPLACE FUNCTION create_save_backup()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.version IS DISTINCT FROM NEW.version THEN
        INSERT INTO save_backups (user_id, save_id, version, checksum, player_data, stage_data, currency_data, backup_reason)
        VALUES (OLD.user_id, OLD.id, OLD.version, OLD.checksum, OLD.player_data, OLD.stage_data, OLD.currency_data, 'auto');

        -- Keep only last 5 backups per user
        DELETE FROM save_backups
        WHERE user_id = OLD.user_id
        AND id NOT IN (
            SELECT id FROM save_backups
            WHERE user_id = OLD.user_id
            ORDER BY created_at DESC
            LIMIT 5
        );
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER auto_backup_save
    BEFORE UPDATE ON game_saves
    FOR EACH ROW EXECUTE FUNCTION create_save_backup();

-- =====================================================
-- INITIAL DATA
-- =====================================================

-- Ranking reward tiers
CREATE TABLE ranking_reward_tiers (
    id SERIAL PRIMARY KEY,
    ranking_type VARCHAR(50) NOT NULL,
    min_rank INTEGER NOT NULL,
    max_rank INTEGER NOT NULL,
    reward_type VARCHAR(50) NOT NULL,
    reward_amount INTEGER NOT NULL,
    description VARCHAR(200)
);

INSERT INTO ranking_reward_tiers (ranking_type, min_rank, max_rank, reward_type, reward_amount, description) VALUES
('total_power', 1, 1, 'diamond', 1000, '1위 보상'),
('total_power', 2, 3, 'diamond', 500, '2-3위 보상'),
('total_power', 4, 10, 'diamond', 300, '4-10위 보상'),
('total_power', 11, 50, 'diamond', 100, '11-50위 보상'),
('total_power', 51, 100, 'diamond', 50, '51-100위 보상'),
('highest_stage', 1, 1, 'diamond', 800, '1위 보상'),
('highest_stage', 2, 3, 'diamond', 400, '2-3위 보상'),
('highest_stage', 4, 10, 'diamond', 200, '4-10위 보상'),
('level', 1, 1, 'diamond', 500, '1위 보상'),
('level', 2, 10, 'diamond', 200, '2-10위 보상');
