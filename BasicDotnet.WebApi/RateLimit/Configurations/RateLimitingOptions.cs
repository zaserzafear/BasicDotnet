namespace BasicDotnet.WebApi.RateLimit.Configurations;

public class RateLimitingOptions
{
    public RedisOptions Redis { get; set; } = new();
    public RateLimitPolicyOptions Sensitive { get; set; } = new();
    public RateLimitPolicyOptions Public { get; set; } = new();
    public ApiKeyRateLimitOptions ApiKey { get; set; } = new();
}

public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
}

public class RateLimitPolicyOptions
{
    public int Limit { get; set; }
    public TimeSpan Window { get; set; }
}

public class ApiKeyRateLimitOptions : RateLimitPolicyOptions
{
    public string Header { get; set; } = string.Empty;
}

