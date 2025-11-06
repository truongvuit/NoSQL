using Infrastructure.Configuration;
using System.Text.Json;
using StackExchange.Redis;
using Core.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connection;
        private readonly string _instanceName;

        public RedisCacheService(IOptions<RedisSettings> settings)
        {
            _connection = ConnectionMultiplexer.Connect(settings.Value.ConnectionString);
            _database = _connection.GetDatabase();
            _instanceName = settings.Value.InstanceName;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(GetKey(key));
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serialized = JsonSerializer.Serialize(value);
            return await _database.StringSetAsync(GetKey(key), serialized, expiry);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(GetKey(key));
        }

        public async Task<long> RemoveByPatternAsync(string pattern)
        {
            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var fullPattern = $"{_instanceName}:{pattern}";
            var keys = server.Keys(pattern: fullPattern).ToArray();
            
            if (keys.Length == 0)
                return 0;

            return await _database.KeyDeleteAsync(keys);
        }

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            return await _database.StringIncrementAsync(GetKey(key), value);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(GetKey(key));
        }

        private string GetKey(string key)
        {
            return $"{_instanceName}:{key}";
        }
    }
}
