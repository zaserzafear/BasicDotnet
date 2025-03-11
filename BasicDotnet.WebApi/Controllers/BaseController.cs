using BasicDotnet.Domain.Enums;
using BasicDotnet.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicDotnet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BaseController : ControllerBase
{
    protected string RequestId => HttpContext.TraceIdentifier;

    protected IActionResult Success<T>(T data, string message = "Request successful")
    {
        return Ok(ApiResult<T>.SuccessResult(data, RequestId, message));
    }

    protected IActionResult Error(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResult<object>.ErrorResult(message, RequestId));
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

    protected UserRole? GetCurrentRoleId()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (Enum.TryParse(roleClaim, out UserRole role))
        {
            return role;
        }

        return null;
    }
}
