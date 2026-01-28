// MongoDB Sharding Demo Queries

// 1. Query all articles
print("\n=== All Articles ===");
db.articles.find().pretty();

// 2. Query by author (uses shard key - efficient)
print("\n=== Articles by Author (Shard Key) ===");
db.articles.find({ author: "أحمد محمد" }).pretty();

// 3. Query by category (may need to query multiple shards)
print("\n=== Articles by Category ===");
db.articles.find({ category: "scalability" }).pretty();

// 4. Show shard distribution
print("\n=== Shard Distribution ===");
db.articles.getShardDistribution();

// 5. Explain query plan
print("\n=== Query Plan (by author - uses shard key) ===");
db.articles.find({ author: "أحمد محمد" }).explain("executionStats");

// 6. Query plan without shard key (scatter-gather)
print("\n=== Query Plan (by category - no shard key) ===");
db.articles.find({ category: "scalability" }).explain("executionStats");

// 7. Count documents per shard
print("\n=== Document Count ===");
print("Total documents: " + db.articles.countDocuments());

// 8. Aggregation example
print("\n=== Articles by Category (Aggregation) ===");
db.articles.aggregate([
    { $group: { _id: "$category", count: { $sum: 1 }, avgViews: { $avg: "$views" } } },
    { $sort: { count: -1 } }
]).pretty();

// 9. Show chunk distribution
print("\n=== Chunk Information ===");
sh.status();

