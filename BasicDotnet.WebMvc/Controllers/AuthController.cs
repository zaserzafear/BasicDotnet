using BasicDotnet.App.Services;
using BasicDotnet.Infra.Services;
using BasicDotnet.WebMvc.Configurations;
using BasicDotnet.WebMvc.Constants;
using BasicDotnet.WebMvc.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasicDotnet.WebMvc.Controllers;

public class AuthController : BaseController
{
    private readonly string _registerApiEndpoint;
    private readonly string _loginApiEndpoint;

    private readonly HttpClientService _httpClientService;

    public AuthController(IOptions<ApiConfig> apiConfigOption, HttpClientService httpClientService) : base(apiConfigOption)
    {
        _registerApiEndpoint = $"{_baseApiUrlFrontend}/customer/register";
        _loginApiEndpoint = $"{_baseApiUrlBackend}/customer/login";
        _httpClientService = httpClientService;
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

    [HttpGet("login")]
    public IActionResult Login()
    {
        var model = new LoginViewModel
        {
            LoginApiEndpoint = Url.Action("Login", ControllerName)!
        };

        return View(model);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromForm] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Error"] = "Invalid input. Please check your credentials.";
            return View(model);
        }

        var result = await _httpClientService.PostAsync<TokenResponse>(AuthConstants.HttpClientName, _loginApiEndpoint, model.LoginDto);

        if (result?.Data is not { AccessToken: { Length: > 0 } })
        {
            ViewData["Error"] = result?.Message;
            return View(model);
        }

        SetAuthCookies(result.Data);
        return RedirectToAction("Me", ControllerName);
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return View();
    }

    private void SetAuthCookies(TokenResponse tokenResponse)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenResponse.RefreshTokenExpires
        };

        Response.Cookies.Append(AuthConstants.AccessToken, tokenResponse.AccessToken, cookieOptions);
        Response.Cookies.Append(AuthConstants.RefreshToken, tokenResponse.RefreshToken, cookieOptions);
    }
}
