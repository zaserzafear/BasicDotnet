using BasicDotnet.WebApi.RateLimit.Configurations;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace BasicDotnet.WebApi.RateLimit;

public class RateLimitRedisAdapter
{
    private readonly IDatabase _redisDb;
    private readonly Dictionary<string, RateLimitPolicyConfig> _policies;
    private readonly string _instanceName;

    private const string LuaScript = @"
        local key = KEYS[1]
        local now = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])
        local limit = tonumber(ARGV[3])

        -- Remove outdated entries
        redis.call('ZREMRANGEBYSCORE', key, 0, now - window)

        -- Get the current count
        local count = redis.call('ZCARD', key)

        if count >= limit then
            return 1 -- Rate limit exceeded
        end

        -- Add new timestamp
        redis.call('ZADD', key, now, now)

        -- Set expiration (only update if not already set)
        local ttl = redis.call('TTL', key)
        if ttl < window then
            redis.call('EXPIRE', key, window)
        end

        return 0 -- Allowed
    ";

    public RateLimitRedisAdapter(IOptions<RateLimitingOptions> rateLimitingOptions)
    {
        var options = rateLimitingOptions.Value;

        var redis = ConnectionMultiplexer.Connect(options.Redis.ConnectionString);
        _redisDb = redis.GetDatabase();
        _instanceName = options.Redis.InstanceName;
        _policies = new Dictionary<string, RateLimitPolicyConfig>();

        // Load policies from configuration
        if (options.Sensitive != null)
            _policies[RateLimitPolicies.Sensitive] = new RateLimitPolicyConfig { Limit = options.Sensitive.Limit, Window = options.Sensitive.Window };

        if (options.Public != null)
            _policies[RateLimitPolicies.Public] = new RateLimitPolicyConfig { Limit = options.Public.Limit, Window = options.Public.Window };

        if (options.ApiKey != null)
            _policies[RateLimitPolicies.ApiKey] = new RateLimitPolicyConfig { Limit = options.ApiKey.Limit, Window = options.ApiKey.Window };
    }

    public async Task<bool> IsLimitExceededAsync(string key, string policyName)
    {
        if (!_policies.TryGetValue(policyName, out var policy))
        {
            throw new InvalidOperationException($"Rate limit policy '{policyName}' not found.");
        }

        key = $"{_instanceName}{key}";
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = (long)await _redisDb.ScriptEvaluateAsync(
            LuaScript,
            new RedisKey[] { key },
            new RedisValue[] { now, (long)policy.Window.TotalMilliseconds, policy.Limit }
        );

        return result == 1;
    }

    public async Task<int> GetRetryAfterAsync(string key, string policyName)
    {
        if (!_policies.TryGetValue(policyName, out var policy))
        {
            throw new InvalidOperationException($"Rate limit policy '{policyName}' not found.");
        }

        var ttl = await _redisDb.KeyTimeToLiveAsync(key);
        return ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int)policy.Window.TotalSeconds;
    }
}
