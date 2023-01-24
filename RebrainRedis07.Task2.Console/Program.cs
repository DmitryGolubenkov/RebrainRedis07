using StackExchange.Redis;

var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
var db = redis.GetDatabase();
var server = redis.GetServer("localhost:6379");

await foreach (var key in server.KeysAsync(pattern: $"data:*"))
{
    var length = db.StringLengthAsync(key);
    var file = key.ToString().Substring(5);
    Console.WriteLine($"{length} {file}");
}