namespace BasicDotnet.WebApi.RateLimit;

public class RateLimitPolicyConfig
{
    public int Limit { get; set; }
    public TimeSpan Window { get; set; }
}
