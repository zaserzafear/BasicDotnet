namespace BasicDotnet.WebApi.RateLimit;

public static class RateLimitPolicies
{
    public const string None = "None";           // No rate limiting applied
    public const string Sensitive = "Sensitive"; // For login, register, OTP
    public const string Public = "Public";       // For general API usage
    public const string ApiKey = "ApiKey";       // For API keys
}
