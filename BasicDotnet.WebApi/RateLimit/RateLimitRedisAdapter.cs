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

    public RateLimitRedisAdapter(string connectionString, string instanceName, int db = -1)
    {
        var redis = ConnectionMultiplexer.Connect(connectionString);
        _redisDb = redis.GetDatabase(db);
        _policies = new Dictionary<string, RateLimitPolicyConfig>();
        _instanceName = instanceName;
    }

    public void AddPolicy(string policyName, int limit, TimeSpan window)
    {
        _policies[policyName] = new RateLimitPolicyConfig
        {
            Limit = limit,
            Window = window
        };
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
