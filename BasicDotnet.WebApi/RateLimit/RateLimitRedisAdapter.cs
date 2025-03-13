using StackExchange.Redis;

namespace BasicDotnet.WebApi.RateLimit;

public class RateLimitPolicyConfig
{
    public int Limit { get; set; }
    public TimeSpan Window { get; set; }
}

public class RateLimitRedisAdapter
{
    private readonly IDatabase _redisDb;
    private readonly Dictionary<string, RateLimitPolicyConfig> _policies;
    private readonly string _instanceName;

    public RateLimitRedisAdapter(string connectionString, string instanceName)
    {
        var redis = ConnectionMultiplexer.Connect(connectionString);
        _redisDb = redis.GetDatabase();
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

        key = $"{_instanceName}_{key}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Store timestamp in sorted set
        await _redisDb.SortedSetAddAsync(key, now.ToString(), now);

        // Remove expired entries outside the sliding window
        await _redisDb.SortedSetRemoveRangeByScoreAsync(key, 0, now - (long)policy.Window.TotalSeconds);

        // Count remaining timestamps
        var count = await _redisDb.SortedSetLengthAsync(key);

        // Set expiration to ensure cleanup
        await _redisDb.KeyExpireAsync(key, policy.Window);

        return count > policy.Limit;
    }
}
