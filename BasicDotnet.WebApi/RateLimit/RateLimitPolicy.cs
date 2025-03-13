namespace BasicDotnet.WebApi.RateLimit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitPolicy : Attribute
{
    public string PolicyName { get; }

    public RateLimitPolicy(string policyName)
    {
        PolicyName = policyName;
    }
}
