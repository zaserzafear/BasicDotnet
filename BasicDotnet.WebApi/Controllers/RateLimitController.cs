using BasicDotnet.WebApi.RateLimit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[RateLimitPolicy(RateLimitPolicies.None)]
[AllowAnonymous]
public class RateLimitController : BaseController
{
    private static readonly Random _random = new();

    [HttpGet("none")]
    [RateLimitPolicy(RateLimitPolicies.None)]
    public IActionResult NoneEndpoint()
    {
        return Ok(new { message = "Success: No rate limit applied to this endpoint." });
    }

    [HttpGet("public")]
    [RateLimitPolicy(RateLimitPolicies.Public)]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "Success: This endpoint is rate-limited under the 'Public' policy.", randomValue = _random.Next(1, 100) });
    }

    [HttpPost("sensitive")]
    [RateLimitPolicy(RateLimitPolicies.Sensitive)]
    public IActionResult SensitiveEndpoint()
    {
        return Ok(new { message = "Success: This endpoint is rate-limited under the 'Sensitive' policy.", generatedOtp = _random.Next(100000, 999999) });
    }

    [HttpGet("apikey")]
    [RateLimitPolicy(RateLimitPolicies.ApiKey)]
    public IActionResult ApiKeyEndpoint()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKey) || string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized(new { error = "Access Denied: A valid API Key is required for this endpoint." });
        }

        return Ok(new { message = "Success: Valid API Key provided. This endpoint is rate-limited under the 'ApiKey' policy.", providedApiKey = apiKey.ToString() });
    }
}
