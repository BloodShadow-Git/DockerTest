using Serilog;
using StackExchange.Redis;

namespace NetNotepad.ServiceBase
{
    public static class RedisClient
    {
        public static async Task<IDatabase> Connect(string hostVar = "REDISHOST", string portVar = "REDISPORT")
        {
            string RedisHost = Environment.GetEnvironmentVariable(hostVar) ?? throw new Exception("No Redis host string");
            string RedisPort = Environment.GetEnvironmentVariable(portVar) ?? throw new Exception("No Redis port string");
            Log.Information("Found redis host and port connect strings");
            var res = ConnectionMultiplexer.Connect($"{RedisHost}:{RedisPort}").GetDatabase();
            Log.Information("Successfully connected to {0}:{1}", RedisHost, RedisPort);
            return res;
        }
    }
}