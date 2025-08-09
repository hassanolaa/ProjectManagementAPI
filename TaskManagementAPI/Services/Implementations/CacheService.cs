using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TaskManagementAPI.Services.Interfaces;
using System.Text.Json;

namespace TaskManagementAPI.Services.Implementations
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IDistributedCache distributedCache,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedValue = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedValue))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.SetAbsoluteExpiration(expiration.Value);
                }
                else
                {
                    // Default expiration of 1 hour
                    options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                }

                var serializedValue = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, serializedValue, options);

                _logger.LogInformation("Cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogInformation("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());

                var keys = server.Keys(pattern: pattern);

                foreach (var key in keys)
                {
                    await database.KeyDeleteAsync(key);
                }

                _logger.LogInformation("Cache pattern removed: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var cachedValue = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task RefreshAsync(string key)
        {
            try
            {
                await _distributedCache.RefreshAsync(key);
                _logger.LogInformation("Cache refreshed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
            }
        }
    }
}
