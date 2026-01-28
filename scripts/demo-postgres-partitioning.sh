#!/bin/bash

# Demo script for PostgreSQL Partitioning
# This script demonstrates different partitioning strategies

set -e

echo "🗄️  PostgreSQL Partitioning Demo"
echo "=================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to run queries using docker exec
run_query() {
    local container_name=$1
    local query=$2
    local description=$3
    
    echo -e "${BLUE}📊 $description${NC}"
    echo "----------------------------------------"
    docker exec $container_name psql -U postgres -d eqraatech -c "$query"
    echo ""
}

# Check which demo to run
DEMO_TYPE=${1:-range}

case $DEMO_TYPE in
    range)
        CONTAINER="eqraatech-db-range"
        echo -e "${GREEN}Running Range Partitioning Demo${NC}"
        echo "Container: $CONTAINER"
        echo ""
        
        # Check if container is running
        if ! docker ps | grep -q $CONTAINER; then
            echo -e "${RED}❌ Container $CONTAINER is not running!${NC}"
            echo "Please start it with: docker-compose -f docker-compose.postgres-range.yml up -d"
            exit 1
        fi
        
        run_query $CONTAINER "SELECT * FROM articles ORDER BY created_at;" "All Articles (Range Partitioned)"
        
        run_query $CONTAINER "SELECT * FROM articles WHERE created_at >= '2025-01-01' AND created_at < '2025-04-01';" "Q1 2025 Articles (Partition Pruning)"
        
        run_query $CONTAINER "SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size FROM pg_tables WHERE schemaname = 'public' AND tablename LIKE 'articles_%' ORDER BY tablename;" "Partition Sizes"
        
        run_query $CONTAINER "EXPLAIN (ANALYZE, BUFFERS) SELECT * FROM articles WHERE created_at >= '2025-01-01' AND created_at < '2025-04-01';" "Query Plan (Shows Partition Pruning)"
        ;;
        
    list)
        CONTAINER="eqraatech-db-list"
        echo -e "${GREEN}Running List Partitioning Demo${NC}"
        echo "Container: $CONTAINER"
        echo ""
        
        # Check if container is running
        if ! docker ps | grep -q $CONTAINER; then
            echo -e "${RED}❌ Container $CONTAINER is not running!${NC}"
            echo "Please start it with: docker-compose -f docker-compose.postgres-list.yml up -d"
            exit 1
        fi
        
        run_query $CONTAINER "SELECT * FROM articles ORDER BY category, created_at;" "All Articles (List Partitioned by Category)"
        
        run_query $CONTAINER "SELECT * FROM articles WHERE category = 'backend';" "Backend Articles (Single Partition)"
        
        run_query $CONTAINER "SELECT category, COUNT(*) as count FROM articles GROUP BY category ORDER BY count DESC;" "Articles per Category"
        
        run_query $CONTAINER "SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size FROM pg_tables WHERE schemaname = 'public' AND tablename LIKE 'articles_%' ORDER BY tablename;" "Partition Sizes"
        ;;
        
    hash)
        CONTAINER="eqraatech-db-hash"
        echo -e "${GREEN}Running Hash Partitioning Demo${NC}"
        echo "Container: $CONTAINER"
        echo ""
        
        # Check if container is running
        if ! docker ps | grep -q $CONTAINER; then
            echo -e "${RED}❌ Container $CONTAINER is not running!${NC}"
            echo "Please start it with: docker-compose -f docker-compose.postgres-hash.yml up -d"
            exit 1
        fi
        
        run_query $CONTAINER "SELECT * FROM articles ORDER BY id;" "All Articles (Hash Partitioned)"
        
        run_query $CONTAINER "SELECT CASE id % 4 WHEN 0 THEN 'Partition 0' WHEN 1 THEN 'Partition 1' WHEN 2 THEN 'Partition 2' WHEN 3 THEN 'Partition 3' END AS partition, COUNT(*) as count FROM articles GROUP BY id % 4 ORDER BY partition;" "Distribution Across Partitions"
        
        run_query $CONTAINER "SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size FROM pg_tables WHERE schemaname = 'public' AND tablename LIKE 'articles_hash_%' ORDER BY tablename;" "Partition Sizes"
        
        run_query $CONTAINER "EXPLAIN (ANALYZE) SELECT * FROM articles WHERE id = 1;" "Query Plan (Shows Hash Partition Routing)"
        ;;
        
    *)
        echo "Usage: $0 [range|list|hash]"
        exit 1
        ;;
esac

echo -e "${GREEN}✅ Demo complete!${NC}"

