#!/bin/bash

# Script to clear all data from all databases for fresh demo start

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "🧹 Clearing All Data for Fresh Demo"
echo "===================================="
echo ""

# PostgreSQL Range Partitioning
if docker ps | grep -q eqraatech-db-range; then
    echo -e "${BLUE}Clearing PostgreSQL Range Partitioning data...${NC}"
    docker exec eqraatech-db-range psql -U postgres -d eqraatech -c "DELETE FROM articles;" > /dev/null 2>&1
    echo -e "${GREEN}✅ Range partitioning cleared${NC}"
fi

# PostgreSQL List Partitioning
if docker ps | grep -q eqraatech-db-list; then
    echo -e "${BLUE}Clearing PostgreSQL List Partitioning data...${NC}"
    docker exec eqraatech-db-list psql -U postgres -d eqraatech -c "DELETE FROM articles;" > /dev/null 2>&1
    echo -e "${GREEN}✅ List partitioning cleared${NC}"
fi

# PostgreSQL Hash Partitioning
if docker ps | grep -q eqraatech-db-hash; then
    echo -e "${BLUE}Clearing PostgreSQL Hash Partitioning data...${NC}"
    docker exec eqraatech-db-hash psql -U postgres -d eqraatech -c "DELETE FROM articles;" > /dev/null 2>&1
    echo -e "${GREEN}✅ Hash partitioning cleared${NC}"
fi

# MongoDB Sharding
if docker ps | grep -q mongodb-router; then
    echo -e "${BLUE}Clearing MongoDB Sharding data...${NC}"
    docker exec mongodb-router mongosh eqraatech --quiet --eval "db.articles.deleteMany({});" > /dev/null 2>&1
    echo -e "${GREEN}✅ MongoDB cluster cleared${NC}"
fi

if docker ps | grep -q mongodb-shard1; then
    docker exec mongodb-shard1 mongosh --port 27018 eqraatech --quiet --eval "db.articles.deleteMany({});" > /dev/null 2>&1
    echo -e "${GREEN}✅ Shard 1 cleared${NC}"
fi

if docker ps | grep -q mongodb-shard2; then
    docker exec mongodb-shard2 mongosh --port 27018 eqraatech --quiet --eval "db.articles.deleteMany({});" > /dev/null 2>&1 || echo -e "${YELLOW}⚠️  Shard 2 may not be ready${NC}"
    echo -e "${GREEN}✅ Shard 2 cleared${NC}"
fi

echo ""
echo -e "${GREEN}✅ All data cleared! Ready for fresh demo.${NC}"
echo ""
echo "You can now:"
echo "  - Start inserting data step-by-step using queries from README.md"
echo "  - Show how data distributes across partitions/shard"

