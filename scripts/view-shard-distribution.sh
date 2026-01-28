#!/bin/bash

# Script to view data distribution across MongoDB shards
# This helps visualize how sharding distributes data

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "📊 MongoDB Shard Distribution Viewer"
echo "===================================="
echo ""

# Check if containers are running
if ! docker ps | grep -q mongodb-shard1 || ! docker ps | grep -q mongodb-shard2; then
    echo -e "${YELLOW}⚠️  MongoDB shards are not running${NC}"
    echo "Start them with: docker-compose -f docker-compose.mongodb-sharding.yml up -d"
    exit 1
fi

echo -e "${BLUE}=== Shard 1 ===${NC}"
docker exec mongodb-shard1 mongosh --port 27018 eqraatech --quiet --eval "
print('Total documents: ' + db.articles.countDocuments());
if (db.articles.countDocuments() > 0) {
    print('\\nAuthors and their articles:');
    db.articles.aggregate([
        { \$group: { 
            _id: '\$author', 
            count: { \$sum: 1 },
            titles: { \$push: '\$title' }
        }},
        { \$sort: { _id: 1 } }
    ]).forEach(function(author) {
        print('  ' + author._id + ': ' + author.count + ' article(s)');
        author.titles.forEach(function(title) {
            print('    - ' + title);
        });
    });
} else {
    print('No documents in this shard');
}
"

echo ""
echo -e "${BLUE}=== Shard 2 ===${NC}"
docker exec mongodb-shard2 mongosh --port 27018 eqraatech --quiet --eval "
try {
    print('Total documents: ' + db.articles.countDocuments());
    if (db.articles.countDocuments() > 0) {
        print('\\nAuthors and their articles:');
        db.articles.aggregate([
            { \$group: { 
                _id: '\$author', 
                count: { \$sum: 1 },
                titles: { \$push: '\$title' }
            }},
            { \$sort: { _id: 1 } }
        ]).forEach(function(author) {
            print('  ' + author._id + ': ' + author.count + ' article(s)');
            author.titles.forEach(function(title) {
                print('    - ' + title);
            });
        });
    } else {
        print('No documents in this shard');
    }
} catch(e) {
    if (e.message.includes('not primary') || e.message.includes('not in primary')) {
        print('⚠️  Shard 2 replica set not ready (not PRIMARY)');
    } else {
        print('⚠️  Error: ' + e.message);
    }
}
"

echo ""
echo -e "${BLUE}=== Summary ===${NC}"
SHARD1_COUNT=$(docker exec mongodb-shard1 mongosh --port 27018 eqraatech --quiet --eval "db.articles.countDocuments()" 2>/dev/null | tail -1 | tr -d '\r\n' || echo "0")
SHARD2_COUNT=$(docker exec mongodb-shard2 mongosh --port 27018 eqraatech --quiet --eval "db.articles.countDocuments()" 2>/dev/null | tail -1 | tr -d '\r\n' || echo "0")
TOTAL=$((SHARD1_COUNT + SHARD2_COUNT))

echo "Shard 1: $SHARD1_COUNT documents"
echo "Shard 2: $SHARD2_COUNT documents"
echo "Total: $TOTAL documents"

if [ "$TOTAL" -gt 0 ]; then
    SHARD1_PCT=$((SHARD1_COUNT * 100 / TOTAL))
    SHARD2_PCT=$((SHARD2_COUNT * 100 / TOTAL))
    echo ""
    echo "Distribution:"
    echo "  Shard 1: ${SHARD1_PCT}%"
    echo "  Shard 2: ${SHARD2_PCT}%"
fi

echo ""
echo -e "${GREEN}✅ Distribution view complete${NC}"
echo ""
echo "💡 Tip: Data is distributed based on the hashed shard key {_id: \"hashed\"}"
echo "   Each document's _id is hashed to determine which shard it belongs to,"
echo "   ensuring roughly even distribution across all shards."

