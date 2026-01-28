#!/bin/bash

# Script to redistribute data across shards
# This is useful when data was inserted before all shards were added

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "🔄 Redistributing Data Across Shards"
echo "===================================="
echo ""
echo "⚠️  Note: This script will DELETE all existing data and re-insert it."
echo "   For step-by-step insertion demonstrations, use the INSERT queries in README.md instead."
echo ""

# Check if containers are running
if ! docker ps | grep -q mongodb-router; then
    echo -e "${YELLOW}⚠️  MongoDB cluster is not running${NC}"
    exit 1
fi

echo -e "${BLUE}Step 1: Checking current distribution...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
print('Current documents: ' + db.articles.countDocuments());
var dist = db.articles.getShardDistribution();
print('Distribution:');
print(JSON.stringify(dist, null, 2));
"

echo ""
echo -e "${BLUE}Step 2: Deleting existing data...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
var count = db.articles.countDocuments();
db.articles.deleteMany({});
print('✅ Deleted ' + count + ' documents');
"

echo ""
echo -e "${BLUE}Step 3: Re-inserting data (will be distributed across available shards)...${NC}"
docker exec mongodb-router mongosh eqraatech --eval "
db.articles.insertMany([
    {
        title: 'مقدمة في قابلية التوسع',
        content: 'قابلية التوسع هي قدرة النظام على التعامل مع زيادة الحمل.',
        author: 'أحمد محمد',
        category: 'scalability',
        created_at: new Date('2025-01-15'),
        views: 1500
    },
    {
        title: 'التوسع العمودي',
        content: 'التوسع العمودي يعني زيادة موارد الخادم الحالي.',
        author: 'سارة علي',
        category: 'scalability',
        created_at: new Date('2025-02-20'),
        views: 1200
    },
    {
        title: 'التوسع الأفقي',
        content: 'التوسع الأفقي يعني إضافة المزيد من الخوادم.',
        author: 'محمد خالد',
        category: 'scalability',
        created_at: new Date('2025-05-10'),
        views: 1800
    },
    {
        title: 'Database Sharding',
        content: 'Sharding distributes data across multiple servers.',
        author: 'أحمد محمد',
        category: 'database',
        created_at: new Date('2025-03-05'),
        views: 2500
    },
    {
        title: 'قاعدة البيانات والتوسع',
        content: 'قاعدة البيانات هي نقطة الاختناق الشائعة.',
        author: 'فاطمة أحمد',
        category: 'database',
        created_at: new Date('2025-08-25'),
        views: 2100
    },
    {
        title: 'موازنة الأحمال',
        content: 'موازن الأحمال يوزع الطلبات على عدة خوادم.',
        author: 'خالد حسن',
        category: 'load-balancing',
        created_at: new Date('2025-11-12'),
        views: 900
    }
]);
print('✅ Inserted ' + db.articles.countDocuments() + ' documents');
"

echo ""
echo -e "${BLUE}Step 4: Splitting chunks and moving to Shard 2...${NC}"
docker exec mongodb-router mongosh eqraatech --eval "
// Split chunks at author boundaries
var authors = db.articles.distinct('author').sort();
print('Splitting at ' + authors.length + ' author boundaries...');
authors.forEach(function(author, index) {
    if (index > 0) {
        try {
            sh.splitAt('eqraatech.articles', {author: author, _id: MinKey()});
        } catch(e) {
            // Ignore errors if split already exists
        }
    }
});

// Move some chunks to Shard 2
print('\\nMoving chunks to Shard 2...');
// Get distinct authors to move some to shard2
var authors = db.articles.distinct('author').sort();
var authorsToMove = Math.floor(authors.length / 2);
if (authorsToMove === 0) authorsToMove = 1;

print('Moving chunks for ' + authorsToMove + ' authors to Shard 2...');
for (var i = 0; i < authorsToMove && i < authors.length; i++) {
    var author = authors[i];
    try {
        sh.moveChunk('eqraatech.articles', {author: author, _id: MinKey()}, 'shard2ReplSet');
        print('✅ Moved chunk for author: ' + author);
    } catch(e) {
        // If chunk doesn't exist at that point, try the next author
        if (!e.message.includes('no chunk found')) {
            print('⚠️  ' + author + ': ' + e.message);
        }
    }
}
"

echo ""
echo -e "${BLUE}Step 5: Waiting for migrations to complete...${NC}"
sleep 5

echo ""
echo -e "${BLUE}Step 6: Checking new distribution...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
var dist = db.articles.getShardDistribution();
print('New distribution:');
print(JSON.stringify(dist, null, 2));
"

echo ""
echo -e "${GREEN}✅ Data redistribution complete!${NC}"
echo ""
echo "Run ./scripts/view-shard-distribution.sh to see the distribution"

