using BasicDotnet.App.Services;
using BasicDotnet.WebMvc.Constants;

namespace BasicDotnet.WebMvc.Services;

public class AuthCookieService
{
    public void SetAuthCookies(HttpContext context, TokenResponse tokenResponse)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenResponse.RefreshTokenExpires
        };

        context.Response.Cookies.Append(AuthConstants.AccessToken, tokenResponse.AccessToken, cookieOptions);
        context.Response.Cookies.Append(AuthConstants.RefreshToken, tokenResponse.RefreshToken, cookieOptions);
    }
}
