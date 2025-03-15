using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : ControllerBase
{
    [HttpGet("GetHeaders")]
    public IActionResult GetHeaders()
    {
        var headers = HttpContext.Request.Headers
            .Select(h => $"{h.Key}: {h.Value}")
            .ToList();

        return Ok(headers);
    }
}
