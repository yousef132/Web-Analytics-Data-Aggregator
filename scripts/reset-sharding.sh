#!/bin/bash

# Script to reset MongoDB sharding with hashed shard key
# Use this if the collection was sharded with the old key

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo "🔄 Resetting MongoDB Sharding with Hashed Shard Key"
echo "=================================================="
echo ""

# Check if MongoDB router is running
if ! docker ps | grep -q mongodb-router; then
    echo -e "${RED}❌ MongoDB router is not running${NC}"
    echo "Please start MongoDB cluster first:"
    echo "  docker-compose -f docker-compose.mongodb-sharding.yml up -d"
    exit 1
fi

# Drop the collection
echo -e "${BLUE}Step 1: Dropping existing collection...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
try {
    db.articles.drop();
    print('✅ Collection dropped');
} catch(e) {
    if (e.message.includes('ns not found')) {
        print('ℹ️  Collection does not exist (already dropped)');
    } else {
        print('⚠️  Error: ' + e.message);
    }
}
" 2>&1 | grep -v "^$"

# Re-shard with hashed key
echo ""
echo -e "${BLUE}Step 2: Re-sharding collection with hashed shard key...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
try {
    sh.shardCollection('eqraatech.articles', {_id: 'hashed'});
    print('✅ Collection sharded with {_id: \"hashed\"}');
} catch(e) {
    if (e.message.includes('already sharded')) {
        print('ℹ️  Collection already sharded');
    } else {
        print('⚠️  Error: ' + e.message);
        exit(1);
    }
}
" 2>&1 | grep -v "^$"

# Verify sharding
echo ""
echo -e "${BLUE}Step 3: Verifying sharding configuration...${NC}"
docker exec mongodb-router mongosh --quiet --eval "
var status = sh.status(true);
var collection = status.databases.find(function(db) {
    return db.database && db.database._id === 'eqraatech';
});
if (collection && collection.collections && collection.collections['eqraatech.articles']) {
    var shardKey = collection.collections['eqraatech.articles'].shardKey;
    var chunks = collection.collections['eqraatech.articles'].chunkMetadata;
    print('✅ Shard key: ' + JSON.stringify(shardKey));
    if (chunks) {
        chunks.forEach(function(chunk) {
            print('   Chunks on ' + chunk.shard + ': ' + chunk.nChunks);
        });
    }
} else {
    print('⚠️  Could not verify sharding configuration');
}
" 2>&1 | grep -v "^$"

echo ""
echo -e "${GREEN}✅ Sharding reset complete!${NC}"
echo ""
echo "The collection is now sharded with {_id: \"hashed\"}"
echo "When you insert documents, they will distribute evenly across shards."
echo ""
echo "Test it:"
echo "  docker exec mongodb-router mongosh eqraatech --eval \"db.articles.insertOne({title: 'Test', author: 'Test Author', content: 'Test content'})\""
echo "  ./scripts/view-shard-distribution.sh"

