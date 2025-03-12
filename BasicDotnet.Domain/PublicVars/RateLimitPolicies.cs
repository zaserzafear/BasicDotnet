namespace BasicDotnet.Domain.PublicVars;

public static class RateLimitPolicies
{
    public const string IpRateLimit = "rate_limit_ip";
    public const string BruteForceProtection = "rate_limit_brute_force";
    public const string UserRateLimit = "rate_limit_user";
}
