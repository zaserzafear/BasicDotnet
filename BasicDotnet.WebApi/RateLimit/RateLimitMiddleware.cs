using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace BasicDotnet.WebApi.RateLimit;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitRedisAdapter _rateLimitAdapter;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(
        RequestDelegate next,
        RateLimitRedisAdapter rateLimitAdapter,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _rateLimitAdapter = rateLimitAdapter;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var policy = endpoint.Metadata.GetMetadata<RateLimitPolicy>();
        if (policy == null)
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

        if (await _rateLimitAdapter.IsLimitExceededAsync(key, policy.PolicyName))
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded.");
            return;
        }

        await _next(context);
    }

    private string? GetClientKey(HttpContext context)
    {
        // Identify by IP
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return ipAddress;
        }

        // Identify by JWT token
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader["Bearer ".Length..].Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken?.Subject;
        }

        return null;
    }
}
