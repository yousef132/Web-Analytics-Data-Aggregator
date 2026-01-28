// MongoDB Sharding Initialization Script
// This script sets up a sharded cluster with sample data

// Enable sharding on the database
sh.enableSharding("eqraatech");

// Create a sharded collection with hashed shard key on _id
// This will distribute documents evenly across shards based on the hash of _id
// Using hashed sharding ensures even distribution even with small datasets
sh.shardCollection("eqraatech.articles", { "_id": "hashed" });

// Sample data will be inserted manually for demonstration
// See README.md for step-by-step INSERT queries

// Create indexes for better query performance
db.articles.createIndex({ "author": 1, "created_at": -1 });
db.articles.createIndex({ "category": 1 });
db.articles.createIndex({ "created_at": -1 });

print("✅ Sharding setup complete!");
print("📊 Collection 'articles' is now sharded on {_id: 'hashed'}");
print("💡 Hashed sharding ensures even distribution across shards");
print("📝 Ready for manual data insertion - see README.md for INSERT queries");

