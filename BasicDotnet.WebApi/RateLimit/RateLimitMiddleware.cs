using BasicDotnet.WebApi.RateLimit.Configurations;
using Microsoft.Extensions.Options;
using System.Net;

namespace BasicDotnet.WebApi.RateLimit;

/// <summary>
/// Middleware for enforcing rate limits using Redis as the backend.
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitRedisAdapter _rateLimitAdapter;
    private readonly RateLimitingOptions _rateLimitingOptions;
    private readonly ILogger<RateLimitMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitMiddleware"/> class.
    /// </summary>
    public RateLimitMiddleware(
        RequestDelegate next,
        RateLimitRedisAdapter rateLimitAdapter,
        IOptions<RateLimitingOptions> rateLimitingOptions,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _rateLimitingOptions = rateLimitingOptions.Value;
        _rateLimitAdapter = rateLimitAdapter;
        _logger = logger;
    }

    /// <summary>
    /// Core middleware logic to enforce rate limiting.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var policy = endpoint?.Metadata.GetMetadata<RateLimitPolicy>()
                     ?? context.GetRouteData()?.Values["controller"]?.GetType()?.GetCustomAttributes(true)
                          .OfType<RateLimitPolicy>().FirstOrDefault();

        // Skip rate limiting if no policy or explicitly set to "None"
        if (policy == null || policy.PolicyName == RateLimitPolicies.None)
        {
            await _next(context);
            return;
        }

        string? clientKey = GetClientKey(context);
        if (clientKey == null)
        {
            await _next(context);
            return;
        }

        var key = $"{policy.PolicyName}:{clientKey}";

        switch (policy.PolicyName)
        {
            case RateLimitPolicies.Sensitive:
                key = $"{policy.PolicyName}:{clientKey}";
                break;

            case RateLimitPolicies.Public:
                key = $"{policy.PolicyName}:{clientKey}";
                break;

            case RateLimitPolicies.ApiKey:
                var apiKey = context.Request.Headers[_rateLimitingOptions.ApiKey.Header].FirstOrDefault();
                if (string.IsNullOrEmpty(apiKey))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "API Key is required" });
                    return;
                }
                clientKey = apiKey; // ✅ Ensure the correct key is used
                key = $"{policy.PolicyName}:{clientKey}";
                break;

            default:
                _logger.LogWarning("Unknown rate limit policy: {PolicyName}", policy.PolicyName);
                break;
        }

        // Check rate limits in Redis
        if (await _rateLimitAdapter.IsLimitExceededAsync(key, policy.PolicyName))
        {
            var retryAfterSeconds = await _rateLimitAdapter.GetRetryAfterAsync(key, policy.PolicyName);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = $"{retryAfterSeconds} seconds",
            });

            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Generates a unique identifier for the client based on IP or JWT username.
    /// </summary>
    private string? GetClientKey(HttpContext context)
    {
        var ipAddress = GetClientIpAddress(context);
        var userName = context.User.Identity?.Name;
        return !string.IsNullOrEmpty(userName) ? userName : ipAddress;
    }

    /// <summary>
    /// Retrieves the client's IP address, giving priority to the "X-Forwarded-For" header.
    /// </summary>
    private string GetClientIpAddress(HttpContext httpContext)
    {
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var clientIp = forwardedFor.Split(',').FirstOrDefault()?.Trim();
            if (IPAddress.TryParse(clientIp, out var ipAddress))
            {
                return ipAddress.ToString();
            }
        }
        return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown client";
    }
}
