using BasicDotnet.App.Services;
using BasicDotnet.WebMvc.Constants;
using BasicDotnet.WebMvc.Services;

namespace BasicDotnet.WebMvc.Middleware;

public class JwtRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenService _tokenService;
    private readonly AuthCookieService _authCookieService;
    private readonly ILogger<JwtRefreshMiddleware> _logger;

    public JwtRefreshMiddleware(
        RequestDelegate next,
        TokenService tokenService,
        AuthCookieService authCookieService,
        ILogger<JwtRefreshMiddleware> logger)
    {
        _next = next;
        _tokenService = tokenService;
        _authCookieService = authCookieService;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var accessToken = context.Request.Cookies[AuthConstants.AccessToken];
        var refreshToken = context.Request.Cookies[AuthConstants.RefreshToken];

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogDebug("No valid tokens found in cookies.");
            await _next(context);
            return;
        }

        try
        {
            var tokenResponse = _tokenService.RegenerateToken(accessToken, refreshToken);
            if (tokenResponse == null)
            {
                _logger.LogWarning("Failed to refresh JWT: Invalid or expired tokens.");
                await _next(context);
                return;
            }

            _authCookieService.SetAuthCookies(context, tokenResponse);
            _logger.LogDebug("JWT refreshed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while refreshing JWT.");
        }

        await _next(context);
    }
}
