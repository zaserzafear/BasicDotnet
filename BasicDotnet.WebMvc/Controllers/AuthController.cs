using BasicDotnet.WebMvc.Configurations;
using BasicDotnet.WebMvc.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasicDotnet.WebMvc.Controllers;

public class AuthController : BaseController
{
    public AuthController(IOptions<ApiConfig> apiConfigOption) : base(apiConfigOption)
    {
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        var model = new RegisterViewModel
        {
            RegisterApiEndpoint = _registerApiEndpoint
        };

        return View(model);
    }
}
