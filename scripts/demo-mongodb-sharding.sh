#!/bin/bash

# Demo script for MongoDB Sharding
# This script demonstrates sharding concepts

set -e

echo "🍃 MongoDB Sharding Demo"
echo "======================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

MONGO_URI="mongodb://localhost:27020"

echo -e "${BLUE}📊 Connecting to MongoDB Router (mongos)${NC}"
echo "URI: $MONGO_URI"
echo ""

# Wait for MongoDB to be ready
echo "⏳ Waiting for MongoDB to be ready..."
until docker exec mongodb-router mongosh --eval "db.adminCommand('ping')" > /dev/null 2>&1; do
    sleep 2
done
echo -e "${GREEN}✅ MongoDB is ready${NC}"
echo ""

# Initialize sharding (if not already done)
echo -e "${BLUE}🔧 Setting up sharding...${NC}"
docker exec mongodb-router mongosh eqraatech --eval "
    try {
        sh.enableSharding('eqraatech');
        print('✅ Sharding enabled on eqraatech database');
    } catch(e) {
        print('ℹ️  Sharding already enabled or error: ' + e);
    }
    
    try {
        sh.shardCollection('eqraatech.articles', { _id: 'hashed' });
        print('✅ Collection sharded on {_id: "hashed"}');
    } catch(e) {
        print('ℹ️  Collection already sharded or error: ' + e);
    }
"
echo ""

# Show shard status
echo -e "${BLUE}📈 Shard Status${NC}"
docker exec mongodb-router mongosh --eval "sh.status()"
echo ""

# Query examples
echo -e "${BLUE}📊 Query Examples${NC}"
echo "----------------------------------------"

echo -e "${YELLOW}1. All Articles:${NC}"
docker exec mongodb-router mongosh eqraatech --eval "db.articles.find().pretty()"
echo ""

echo -e "${YELLOW}2. Articles by Author (Uses Shard Key - Efficient):${NC}"
docker exec mongodb-router mongosh eqraatech --eval "db.articles.find({ author: 'أحمد محمد' }).pretty()"
echo ""

echo -e "${YELLOW}3. Articles by Category (No Shard Key - Scatter-Gather):${NC}"
docker exec mongodb-router mongosh eqraatech --eval "db.articles.find({ category: 'scalability' }).pretty()"
echo ""

echo -e "${YELLOW}4. Query Plan (with shard key):${NC}"
docker exec mongodb-router mongosh eqraatech --eval "db.articles.find({ author: 'أحمد محمد' }).explain('executionStats').executionStats"
echo ""

echo -e "${YELLOW}5. Document Distribution:${NC}"
docker exec mongodb-router mongosh eqraatech --eval "
    db.articles.aggregate([
        { \$group: { _id: '\$author', count: { \$sum: 1 } } },
        { \$sort: { count: -1 } }
    ]).pretty()
"
echo ""

echo -e "${GREEN}✅ Demo complete!${NC}"

