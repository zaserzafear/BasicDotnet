using BasicDotnet.App.Services;
using BasicDotnet.WebMvc.Constants;

namespace BasicDotnet.WebMvc.Middleware;

public class JwtRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenService _tokenService;
    private readonly ILogger<JwtRefreshMiddleware> _logger;

    public JwtRefreshMiddleware(
        RequestDelegate next,
        TokenService tokenService,
        ILogger<JwtRefreshMiddleware> logger)
    {
        _next = next;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // 1. Read tokens from cookies
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
            // 2. Regenerate the token using TokenService
            var tokenResponse = _tokenService.RegenerateToken(accessToken, refreshToken);
            if (tokenResponse == null)
            {
                _logger.LogWarning("Failed to refresh JWT: Invalid or expired tokens.");
                await _next(context);
                return;
            }

            // 3. Set new cookies with refreshed token
            SetAuthCookies(context, tokenResponse);
            _logger.LogDebug("JWT refreshed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while refreshing JWT.");
        }

        await _next(context);
    }

    private void SetAuthCookies(HttpContext context, TokenResponse tokenResponse)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenResponse.RefreshTokenExpires // Keep same expiration as refresh token
        };

        context.Response.Cookies.Append(AuthConstants.AccessToken, tokenResponse.AccessToken, cookieOptions);
        context.Response.Cookies.Append(AuthConstants.RefreshToken, tokenResponse.RefreshToken, cookieOptions);
    }
}
