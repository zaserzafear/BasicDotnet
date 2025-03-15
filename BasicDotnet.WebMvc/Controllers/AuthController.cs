using BasicDotnet.WebMvc.Configurations;
using BasicDotnet.WebMvc.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasicDotnet.WebMvc.Controllers;

public class AuthController : BaseController
{
    private readonly string _registerApiEndpoint;

    public AuthController(IOptions<ApiConfig> apiConfigOption) : base(apiConfigOption)
    {
        _registerApiEndpoint = $"{_baseApiUrl}/Customer/register";
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
