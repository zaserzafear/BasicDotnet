using System.Net;

namespace BasicDotnet.WebApi.RateLimit;

/// <summary>
/// Middleware for enforcing rate limits using Redis as the backend.
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitRedisAdapter _rateLimitAdapter;
    private readonly ILogger<RateLimitMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitMiddleware"/> class.
    /// </summary>
    public RateLimitMiddleware(
        RequestDelegate next,
        RateLimitRedisAdapter rateLimitAdapter,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _rateLimitAdapter = rateLimitAdapter;
        _logger = logger;
    }

    /// <summary>
    /// Core middleware logic to enforce rate limiting.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if endpoint metadata contains a rate limit policy.
        var endpoint = context.GetEndpoint();
        var policy = endpoint?.Metadata.GetMetadata<RateLimitPolicy>()
                     ?? context.GetRouteData()?.Values["controller"]?.GetType()?.GetCustomAttributes(true)
                          .OfType<RateLimitPolicy>().FirstOrDefault();

        if (policy == null)
        {
            await _next(context); // Continue if no policy is defined.
            return;
        }

        // Generate a unique client identifier (IP or username) for rate limiting.
        string? clientKey = GetClientKey(context);
        if (clientKey == null)
        {
            await _next(context); // Continue if no valid client key is found.
            return;
        }

        // Construct the rate limit key for Redis storage.
        var key = $"{policy.PolicyName}:{clientKey}";

        // Check if the request exceeds the allowed limit.
        if (await _rateLimitAdapter.IsLimitExceededAsync(key, policy.PolicyName))
        {
            var retryAfterSeconds = await _rateLimitAdapter.GetRetryAfterAsync(key, policy.PolicyName);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Rate limit exceeded",
                retryAfter = $"{retryAfterSeconds} seconds",
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Proceed to the next middleware in the pipeline.
        await _next(context);
    }

    /// <summary>
    /// Generates a unique identifier for the client based on IP or JWT username.
    /// </summary>
    private string? GetClientKey(HttpContext context)
    {
        // Extract the client's IP address as a fallback identifier.
        var ipAddress = GetClientIpAddress(context);

        // Extract the username if authenticated via JWT.
        var userName = context.User.Identity?.Name;
        bool isAuthenticated = !string.IsNullOrEmpty(userName);

        // Use the username if authenticated; otherwise, fallback to IP address.
        return isAuthenticated ? userName : ipAddress;
    }

    /// <summary>
    /// Retrieves the client's IP address, giving priority to the "X-Forwarded-For" header.
    /// </summary>
    private string GetClientIpAddress(HttpContext httpContext)
    {
        // Check for the "X-Forwarded-For" header, which may contain multiple IPs.
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var clientIp = forwardedFor.Split(',').FirstOrDefault()?.Trim();

            if (IPAddress.TryParse(clientIp, out var ipAddress))
            {
                return ipAddress.ToString();
            }
        }

        // Fallback to the remote IP address in case "X-Forwarded-For" is not present.
        return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown client";
    }
}
