using BasicDotnet.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
