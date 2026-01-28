-- PostgreSQL Hash Partitioning Example
-- This demonstrates partitioning articles by hash of ID for even distribution

-- Create parent table
CREATE TABLE IF NOT EXISTS articles (
    id SERIAL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    author VARCHAR(100) DEFAULT 'Anonymous',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id)
) PARTITION BY HASH (id);

-- Create 4 hash partitions for even distribution
CREATE TABLE IF NOT EXISTS articles_hash_0 PARTITION OF articles
    FOR VALUES WITH (MODULUS 4, REMAINDER 0);

CREATE TABLE IF NOT EXISTS articles_hash_1 PARTITION OF articles
    FOR VALUES WITH (MODULUS 4, REMAINDER 1);

CREATE TABLE IF NOT EXISTS articles_hash_2 PARTITION OF articles
    FOR VALUES WITH (MODULUS 4, REMAINDER 2);

CREATE TABLE IF NOT EXISTS articles_hash_3 PARTITION OF articles
    FOR VALUES WITH (MODULUS 4, REMAINDER 3);

-- Create indexes on each partition
CREATE INDEX IF NOT EXISTS idx_articles_hash_0_created ON articles_hash_0(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_hash_1_created ON articles_hash_1(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_hash_2_created ON articles_hash_2(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_hash_3_created ON articles_hash_3(created_at);

-- Sample data will be inserted manually for demonstration
-- See README.md for INSERT queries to run step by step

