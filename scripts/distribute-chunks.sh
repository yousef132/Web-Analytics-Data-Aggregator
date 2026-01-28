#!/bin/bash

# Script to manually split and distribute chunks across shards for demo purposes
# This is useful when you have small datasets that all go to one shard initially

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "🔀 Distributing Chunks Across Shards"
echo "===================================="
echo ""

# Step 1: Split chunks at author boundaries using splitFind
echo -e "${BLUE}Step 1: Splitting chunks at author boundaries...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
var authors = db.articles.distinct('author').sort();
print('Found ' + authors.length + ' unique authors');
for (var i = 1; i < authors.length; i++) {
    try {
        sh.splitFind('eqraatech.articles', {author: authors[i]});
        print('✅ Split at: ' + authors[i]);
    } catch(e) {
        // Chunk might already be split or too small
    }
}
var chunks = db.getSiblingDB('config').chunks.find({ns: 'eqraatech.articles'}).toArray();
print('Total chunks after splitting: ' + chunks.length);
" 2>&1 | grep -v "^$"

# Step 2: Move some chunks to Shard 2
echo ""
echo -e "${BLUE}Step 2: Moving chunks to Shard 2...${NC}"
docker exec mongodb-router mongosh eqraatech --quiet --eval "
var chunks = db.getSiblingDB('config').chunks.find({ns: 'eqraatech.articles', shard: 'shard1ReplSet'}).toArray();
var chunksToMove = Math.floor(chunks.length / 2);
print('Moving ' + chunksToMove + ' of ' + chunks.length + ' chunks to Shard 2...');
for (var i = 0; i < chunksToMove && i < chunks.length; i++) {
    try {
        sh.moveChunk('eqraatech.articles', chunks[i].min, 'shard2ReplSet');
        print('✅ Moved chunk ' + (i+1));
    } catch(e) {
        print('⚠️  Error: ' + e.message);
    }
}
" 2>&1 | grep -v "^$"

# Step 3: Show final distribution
echo ""
echo -e "${GREEN}✅ Distribution complete!${NC}"
echo ""
echo "Final distribution:"
./scripts/view-shard-distribution.sh 2>&1

