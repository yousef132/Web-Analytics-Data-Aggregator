#!/bin/bash

# MongoDB Sharding Setup Script
# This script initializes the MongoDB sharded cluster

set -e

echo "🍃 MongoDB Sharding Setup"
echo "========================"
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Wait for services to be ready
echo -e "${BLUE}⏳ Waiting for MongoDB services to start...${NC}"
echo "Checking container status..."
for i in {1..30}; do
    if docker ps | grep -q mongodb-config && \
       docker ps | grep -q mongodb-shard1 && \
       docker ps | grep -q mongodb-shard2; then
        echo -e "${GREEN}✅ All MongoDB containers are running${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}❌ Some MongoDB containers failed to start${NC}"
        docker ps | grep mongodb
        exit 1
    fi
    sleep 2
done
sleep 5

# Initialize Config Server Replica Set
echo -e "${BLUE}🔧 Initializing Config Server Replica Set...${NC}"
# Wait for config server to be ready
for i in {1..20}; do
    if docker exec mongodb-config mongosh --port 27019 --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
        break
    fi
    sleep 1
done

docker exec mongodb-config mongosh --port 27019 --eval "
    try {
        var status = rs.status();
        if (status.members && status.members[0].stateStr === 'PRIMARY') {
            print('ℹ️  Config Server replica set already initialized with PRIMARY');
        } else {
            print('⏳ Config Server replica set initialized, waiting for PRIMARY...');
        }
    } catch(e) {
        if (e.message.includes('no replset config') || e.message.includes('not yet initialized')) {
            var result = rs.initiate({
                _id: 'configReplSet',
                configsvr: true,
                members: [{ _id: 0, host: 'mongodb-config:27019' }]
            });
            print('✅ Config Server replica set initialization started');
        } else {
            print('⚠️  Error: ' + e.message);
        }
    }
" 2>&1

# Wait for config server to have PRIMARY
echo -e "${BLUE}⏳ Waiting for Config Server PRIMARY...${NC}"
for i in {1..30}; do
    PRIMARY=$(docker exec mongodb-config mongosh --port 27019 --quiet --eval "try { var s = rs.status(); s.members[0].stateStr; } catch(e) { 'NOT_READY'; }" 2>/dev/null | tr -d '\r\n')
    if [ "$PRIMARY" = "PRIMARY" ]; then
        echo -e "${GREEN}✅ Config Server PRIMARY is ready${NC}"
        break
    fi
    sleep 2
done

sleep 10

# Initialize Shard 1 Replica Set
echo -e "${BLUE}🔧 Initializing Shard 1 Replica Set...${NC}"
# Wait for shard1 to be ready
for i in {1..20}; do
    if docker exec mongodb-shard1 mongosh --port 27018 --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
        break
    fi
    sleep 1
done

docker exec mongodb-shard1 mongosh --port 27018 --eval "
    try {
        var status = rs.status();
        if (status.members && status.members[0].stateStr === 'PRIMARY') {
            print('ℹ️  Shard 1 replica set already initialized with PRIMARY');
        } else {
            print('⏳ Shard 1 replica set initialized, waiting for PRIMARY...');
        }
    } catch(e) {
        if (e.message.includes('no replset config') || e.message.includes('not yet initialized')) {
            var result = rs.initiate({
                _id: 'shard1ReplSet',
                members: [{ _id: 0, host: 'mongodb-shard1:27018' }]
            });
            print('✅ Shard 1 replica set initialization started');
        } else {
            print('⚠️  Error: ' + e.message);
        }
    }
" 2>&1

# Wait for shard1 to have PRIMARY
echo -e "${BLUE}⏳ Waiting for Shard 1 PRIMARY...${NC}"
for i in {1..30}; do
    PRIMARY=$(docker exec mongodb-shard1 mongosh --port 27018 --quiet --eval "try { var s = rs.status(); s.members[0].stateStr; } catch(e) { 'NOT_READY'; }" 2>/dev/null | tr -d '\r\n')
    if [ "$PRIMARY" = "PRIMARY" ]; then
        echo -e "${GREEN}✅ Shard 1 PRIMARY is ready${NC}"
        break
    fi
    sleep 2
done

sleep 10

# Initialize Shard 2 Replica Set
echo -e "${BLUE}🔧 Initializing Shard 2 Replica Set...${NC}"
# Wait a bit longer for shard2 to be fully ready (shardsvr can take time)
echo -e "${BLUE}⏳ Waiting for Shard 2 to be ready...${NC}"
sleep 10

# Try to initialize - shard2 listens on port 27018
# Note: Even if connection fails here, we'll continue - replica set might initialize later
docker exec mongodb-shard2 mongosh --port 27018 --eval "
    try {
        var status = rs.status();
        if (status.members && status.members[0].stateStr === 'PRIMARY') {
            print('✅ Shard 2 replica set already initialized with PRIMARY');
        } else {
            print('⏳ Shard 2 replica set initialized, waiting for PRIMARY...');
        }
    } catch(e) {
        if (e.message.includes('no replset config') || e.message.includes('not yet initialized')) {
            try {
                rs.initiate({
                    _id: 'shard2ReplSet',
                    members: [{ _id: 0, host: 'mongodb-shard2:27018' }]
                });
                print('✅ Shard 2 replica set initialization started');
            } catch(initErr) {
                print('⚠️  Init error (may retry): ' + initErr.message);
            }
        } else if (e.message.includes('ECONNREFUSED') || e.message.includes('connect')) {
            print('⚠️  Connection issue (MongoDB may still be starting): ' + e.message);
            print('ℹ️  Will continue - replica set may initialize automatically');
        } else {
            print('⚠️  Error: ' + e.message);
        }
    }
" 2>&1 || echo -e "${YELLOW}⚠️  Shard 2 initialization attempt completed (may need more time)${NC}"

# Wait for shard2 to have PRIMARY (with more patience)
echo -e "${BLUE}⏳ Waiting for Shard 2 PRIMARY (this may take up to 60 seconds)...${NC}"
PRIMARY="NOT_READY"
for i in {1..30}; do
    PRIMARY=$(docker exec mongodb-shard2 mongosh --port 27018 --quiet --eval "try { var s = rs.status(); s.members[0].stateStr; } catch(e) { 'NOT_READY'; }" 2>/dev/null | tail -1 | tr -d '\r\n' || echo "NOT_READY")
    if [ "$PRIMARY" = "PRIMARY" ]; then
        echo -e "${GREEN}✅ Shard 2 PRIMARY is ready${NC}"
        break
    fi
    if [ $((i % 5)) -eq 0 ]; then
        echo "  Still waiting... ($i/30)"
    fi
    sleep 2
done

if [ "$PRIMARY" != "PRIMARY" ]; then
    echo -e "${YELLOW}⚠️  Shard 2 PRIMARY not ready yet, but continuing with setup...${NC}"
    echo "  The replica set may still be initializing in the background"
    echo "  You can check status later with: docker exec mongodb-shard2 mongosh --eval 'rs.status()'"
fi

echo -e "${BLUE}⏳ All replica sets should be ready now...${NC}"
sleep 5

# Wait for mongos to be ready
echo -e "${BLUE}⏳ Waiting for mongos router to be ready...${NC}"
MAX_RETRIES=60
RETRY_COUNT=0
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    if docker exec mongodb-router mongosh --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
        echo ""
        echo -e "${GREEN}✅ MongoDB Router (mongos) is ready${NC}"
        break
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    if [ $((RETRY_COUNT % 5)) -eq 0 ]; then
        echo -n "."
    fi
    sleep 2
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
    echo ""
    echo -e "${RED}❌ MongoDB Router did not become ready in time${NC}"
    echo "Checking container logs..."
    docker logs mongodb-router --tail 20
    exit 1
fi

# Add shards to cluster
echo -e "${BLUE}🔧 Adding shards to cluster...${NC}"
docker exec mongodb-router mongosh --eval "
    try {
        sh.addShard('shard1ReplSet/mongodb-shard1:27018');
        print('✅ Shard 1 added to cluster');
    } catch(e) {
        if (e.message.includes('already exists')) {
            print('ℹ️  Shard 1 already added');
        } else {
            print('⚠️  Error adding Shard 1: ' + e.message);
        }
    }
    
    // Check if shard2 is ready before adding
    print('⏳ Checking Shard 2 status...');
    try {
        sh.addShard('shard2ReplSet/mongodb-shard2:27018');
        print('✅ Shard 2 added to cluster');
    } catch(e) {
        if (e.message.includes('already exists')) {
            print('ℹ️  Shard 2 already added');
        } else if (e.message.includes('Could not find host matching read preference') || 
                   e.message.includes('primary')) {
            print('⚠️  Shard 2 replica set not ready (no PRIMARY yet)');
            print('ℹ️  Shard 2 will be skipped for now');
            print('ℹ️  You can add it later manually when ready:');
            print('   docker exec mongodb-router mongosh --eval \"sh.addShard(\\\"shard2ReplSet/mongodb-shard2:27018\\\")\"');
        } else {
            print('⚠️  Error adding Shard 2: ' + e.message);
            print('ℹ️  You can try adding it manually later');
        }
    }
"

# Enable sharding on database
echo -e "${BLUE}🔧 Enabling sharding on eqraatech database...${NC}"
docker exec mongodb-router mongosh --eval "
    try {
        sh.enableSharding('eqraatech');
        print('✅ Sharding enabled on eqraatech database');
    } catch(e) {
        print('ℹ️  Sharding may already be enabled: ' + e.message);
    }
"

# Shard the articles collection
echo -e "${BLUE}🔧 Sharding articles collection...${NC}"
docker exec mongodb-router mongosh eqraatech --eval "
    try {
        sh.shardCollection('eqraatech.articles', { _id: 'hashed' });
        print('✅ Collection sharded on {_id: "hashed"}');
    } catch(e) {
        if (e.message.includes('already sharded')) {
            print('ℹ️  Collection already sharded');
        } else {
            print('⚠️  Error sharding collection: ' + e.message);
        }
    }
"

# Note: Sample data should be inserted manually for demonstration
# See README.md for step-by-step INSERT queries
echo -e "${BLUE}ℹ️  Sample data insertion skipped${NC}"
echo -e "${YELLOW}   Insert data manually using queries from README.md${NC}"
echo -e "${YELLOW}   This allows you to demonstrate step-by-step how data distributes${NC}"

# Show status
echo -e "${BLUE}📊 Cluster Status:${NC}"
docker exec mongodb-router mongosh --eval "sh.status()"

echo ""
echo -e "${GREEN}✅ MongoDB Sharding Setup Complete!${NC}"
echo ""
echo "Note: If Shard 2 was not added, you can add it manually later:"
echo "  1. Wait for Shard 2 PRIMARY: docker exec mongodb-shard2 mongosh --eval 'rs.status()'"
echo "  2. When PRIMARY is ready, add it: docker exec mongodb-router mongosh --eval 'sh.addShard(\"shard2ReplSet/mongodb-shard2:27017\")'"
echo ""
echo "You can now:"
echo "  - Connect to mongos: docker exec -it mongodb-router mongosh"
echo "  - Run demo: ./scripts/demo-mongodb-sharding.sh"
echo "  - Query data: docker exec mongodb-router mongosh eqraatech --eval \"db.articles.find().pretty()\""
echo "  - Check shard status: docker exec mongodb-router mongosh --eval 'sh.status()'"

