-- Demo Queries for PostgreSQL Partitioning

-- 1. Query all articles (PostgreSQL automatically queries relevant partitions)
SELECT * FROM articles ORDER BY created_at DESC;

-- 2. Query with partition pruning (only Q1 partition will be scanned)
SELECT * FROM articles 
WHERE created_at >= '2025-01-01' AND created_at < '2025-04-01';

-- 3. Count articles per partition
SELECT 
    CASE 
        WHEN tablename = 'articles_2025_q1' THEN 'Q1 2025'
        WHEN tablename = 'articles_2025_q2' THEN 'Q2 2025'
        WHEN tablename = 'articles_2025_q3' THEN 'Q3 2025'
        WHEN tablename = 'articles_2025_q4' THEN 'Q4 2025'
        ELSE tablename
    END AS partition_name,
    (SELECT COUNT(*) FROM articles WHERE created_at >= 
        CASE 
            WHEN tablename = 'articles_2025_q1' THEN '2025-01-01'
            WHEN tablename = 'articles_2025_q2' THEN '2025-04-01'
            WHEN tablename = 'articles_2025_q3' THEN '2025-07-01'
            WHEN tablename = 'articles_2025_q4' THEN '2025-10-01'
            ELSE '1900-01-01'
        END
        AND created_at < 
        CASE 
            WHEN tablename = 'articles_2025_q1' THEN '2025-04-01'
            WHEN tablename = 'articles_2025_q2' THEN '2025-07-01'
            WHEN tablename = 'articles_2025_q3' THEN '2025-10-01'
            WHEN tablename = 'articles_2025_q4' THEN '2026-01-01'
            ELSE '2100-01-01'
        END
    ) AS row_count
FROM pg_tables
WHERE schemaname = 'public' AND tablename LIKE 'articles_%'
ORDER BY tablename;

-- 4. Show partition sizes
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size,
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) AS table_size,
    pg_size_pretty(pg_indexes_size(schemaname||'.'||tablename)) AS indexes_size
FROM pg_tables
WHERE schemaname = 'public' AND tablename LIKE 'articles_%'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- 5. Explain plan showing partition pruning
EXPLAIN (ANALYZE, BUFFERS) 
SELECT * FROM articles 
WHERE created_at >= '2025-01-01' AND created_at < '2025-04-01';

-- 6. For list partitioning - query by category
-- SELECT * FROM articles WHERE category = 'backend';

-- 7. For hash partitioning - show distribution
-- SELECT 
--     CASE id % 4
--         WHEN 0 THEN 'Partition 0'
--         WHEN 1 THEN 'Partition 1'
--         WHEN 2 THEN 'Partition 2'
--         WHEN 3 THEN 'Partition 3'
--     END AS partition,
--     COUNT(*) as count
-- FROM articles
-- GROUP BY id % 4
-- ORDER BY partition;

