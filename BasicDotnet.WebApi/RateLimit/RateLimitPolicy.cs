namespace BasicDotnet.WebApi.RateLimit;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitPolicy : Attribute
{
    public string PolicyName { get; }

    public RateLimitPolicy(string policyName)
    {
        PolicyName = policyName;
    }
}
