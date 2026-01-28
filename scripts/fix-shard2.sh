#!/bin/bash

# Helper script to manually initialize Shard 2 if it failed during setup

set -e

echo "🔧 Fixing Shard 2 Replica Set"
echo "=============================="
echo ""

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Check if container is running
if ! docker ps | grep -q mongodb-shard2; then
    echo -e "${RED}❌ mongodb-shard2 container is not running${NC}"
    echo "Start it with: docker-compose -f docker-compose.mongodb-sharding.yml up -d mongodb-shard2"
    exit 1
fi

echo -e "${BLUE}⏳ Waiting for MongoDB to be ready...${NC}"
for i in {1..30}; do
    if docker exec mongodb-shard2 mongosh --port 27018 --quiet --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ MongoDB is ready${NC}"
        break
    fi
    sleep 1
done

echo -e "${BLUE}🔧 Initializing replica set...${NC}"
docker exec mongodb-shard2 mongosh --port 27018 --eval "
    try {
        var status = rs.status();
        if (status.members && status.members[0].stateStr === 'PRIMARY') {
            print('✅ Replica set already initialized with PRIMARY');
        } else {
            print('⚠️  Replica set exists but no PRIMARY found');
            print('   Current status: ' + JSON.stringify(status.members[0].stateStr));
        }
    } catch(e) {
        if (e.message.includes('no replset config') || e.message.includes('not yet initialized')) {
            print('⏳ Initializing replica set...');
            try {
                var result = rs.initiate({
                    _id: 'shard2ReplSet',
                    members: [{ _id: 0, host: 'mongodb-shard2:27018' }]
                });
                print('✅ Replica set initialization started');
                print('⏳ Waiting for PRIMARY election (this may take 10-30 seconds)...');
            } catch(initErr) {
                print('❌ Error initializing: ' + initErr.message);
            }
        } else {
            print('⚠️  Error: ' + e.message);
        }
    }
"

echo ""
echo -e "${BLUE}⏳ Waiting for PRIMARY election...${NC}"
for i in {1..30}; do
    PRIMARY=$(docker exec mongodb-shard2 mongosh --port 27018 --quiet --eval "try { var s = rs.status(); s.members[0].stateStr; } catch(e) { 'NOT_READY'; }" 2>/dev/null | tail -1 | tr -d '\r\n' || echo "NOT_READY")
    if [ "$PRIMARY" = "PRIMARY" ]; then
        echo -e "${GREEN}✅ PRIMARY is ready!${NC}"
        echo ""
        echo "You can now add Shard 2 to the cluster:"
        echo "  docker exec mongodb-router mongosh --eval 'sh.addShard(\"shard2ReplSet/mongodb-shard2:27018\")'"
        exit 0
    fi
    if [ $((i % 5)) -eq 0 ]; then
        echo "  Still waiting... ($i/30)"
    fi
    sleep 2
done

echo -e "${YELLOW}⚠️  PRIMARY not ready after 60 seconds${NC}"
echo "Check the logs: docker logs mongodb-shard2"
echo "Or check status: docker exec mongodb-shard2 mongosh --port 27018 --eval 'rs.status()'"

