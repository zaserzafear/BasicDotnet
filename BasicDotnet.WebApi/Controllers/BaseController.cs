using BasicDotnet.Domain.Enums;
using BasicDotnet.WebApi.Helpers;
using BasicDotnet.WebApi.RateLimit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicDotnet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[RateLimitPolicy(RateLimitPolicies.Public)]
[Authorize]
public class BaseController : ControllerBase
{
    protected string RequestId => HttpContext.TraceIdentifier;

    protected IActionResult Success<T>(T data, string message = "Request successful", int statusCode = 200)
    {
        return ResponseHelper.Success(RequestId, data, message, statusCode);
    }

    protected IActionResult Error(string message, int statusCode = 400)
    {
        return ResponseHelper.Error(RequestId, message, statusCode);
    }

    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    protected UserRoleEnum? GetCurrentRoleId()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (Enum.TryParse(roleClaim, out UserRoleEnum role))
        {
            return role;
        }

        return null;
    }
}
