using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {

        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Clear specific cache key
        /// </summary>
        [HttpDelete("{key}")]
        public async Task<IActionResult> ClearCache(string key)
        {
            try
            {
                await _cacheService.RemoveAsync(key);
                return Ok(new { message = $"Cache cleared for key: {key}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for key: {Key}", key);
                return StatusCode(500, new { message = "Error clearing cache" });
            }
        }

        /// <summary>
        /// Clear cache pattern (admin only)
        /// </summary>
        [HttpDelete("pattern/{pattern}")]
        public async Task<IActionResult> ClearCachePattern(string pattern)
        {
            try
            {
                await _cacheService.RemovePatternAsync(pattern);
                return Ok(new { message = $"Cache cleared for pattern: {pattern}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache pattern: {Pattern}", pattern);
                return StatusCode(500, new { message = "Error clearing cache pattern" });
            }
        }

        /// <summary>
        /// Check if cache key exists
        /// </summary>
        [HttpGet("exists/{key}")]
        public async Task<IActionResult> CacheExists(string key)
        {
            try
            {
                var exists = await _cacheService.ExistsAsync(key);
                return Ok(new { key, exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return StatusCode(500, new { message = "Error checking cache" });
            }
        }
    
    }
}
