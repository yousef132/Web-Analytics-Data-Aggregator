-- PostgreSQL Range Partitioning Example
-- This demonstrates partitioning articles by date (created_at)

-- Create parent table
CREATE TABLE IF NOT EXISTS articles (
    id SERIAL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    author VARCHAR(100) DEFAULT 'Anonymous',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);

-- Create partitions for different time ranges
-- Q1 2025 (January - March)
CREATE TABLE IF NOT EXISTS articles_2025_q1 PARTITION OF articles
    FOR VALUES FROM ('2025-01-01') TO ('2025-04-01');

-- Q2 2025 (April - June)
CREATE TABLE IF NOT EXISTS articles_2025_q2 PARTITION OF articles
    FOR VALUES FROM ('2025-04-01') TO ('2025-07-01');

-- Q3 2025 (July - September)
CREATE TABLE IF NOT EXISTS articles_2025_q3 PARTITION OF articles
    FOR VALUES FROM ('2025-07-01') TO ('2025-10-01');

-- Q4 2025 (October - December)
CREATE TABLE IF NOT EXISTS articles_2025_q4 PARTITION OF articles
    FOR VALUES FROM ('2025-10-01') TO ('2026-01-01');

-- Default partition for future dates
CREATE TABLE IF NOT EXISTS articles_future PARTITION OF articles
    FOR VALUES FROM ('2026-01-01') TO (MAXVALUE);

-- Create indexes on partitions
CREATE INDEX IF NOT EXISTS idx_articles_2025_q1_created ON articles_2025_q1(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_2025_q2_created ON articles_2025_q2(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_2025_q3_created ON articles_2025_q3(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_2025_q4_created ON articles_2025_q4(created_at);

-- Sample data will be inserted manually for demonstration
-- See README.md for INSERT queries to run step by step

-- View to show partition information
CREATE OR REPLACE VIEW partition_info AS
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE tablename LIKE 'articles_%'
ORDER BY tablename;

