-- PostgreSQL List Partitioning Example
-- This demonstrates partitioning articles by author/category

-- Create parent table
CREATE TABLE IF NOT EXISTS articles (
    id SERIAL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    author VARCHAR(100) DEFAULT 'Anonymous',
    category VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id, category)
) PARTITION BY LIST (category);

-- Create partitions for different categories
CREATE TABLE IF NOT EXISTS articles_backend PARTITION OF articles
    FOR VALUES IN ('backend', 'api', 'server');

CREATE TABLE IF NOT EXISTS articles_frontend PARTITION OF articles
    FOR VALUES IN ('frontend', 'ui', 'react', 'vue');

CREATE TABLE IF NOT EXISTS articles_devops PARTITION OF articles
    FOR VALUES IN ('devops', 'docker', 'kubernetes', 'ci-cd');

CREATE TABLE IF NOT EXISTS articles_database PARTITION OF articles
    FOR VALUES IN ('database', 'sql', 'nosql', 'postgresql', 'mongodb');

CREATE TABLE IF NOT EXISTS articles_other PARTITION OF articles
    DEFAULT;

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_articles_backend_created ON articles_backend(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_frontend_created ON articles_frontend(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_devops_created ON articles_devops(created_at);
CREATE INDEX IF NOT EXISTS idx_articles_database_created ON articles_database(created_at);

-- Sample data will be inserted manually for demonstration
-- See README.md for INSERT queries to run step by step

