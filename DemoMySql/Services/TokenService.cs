using DemoMySql.Cls;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoMySql.Services;

public class TokenService
{
    private readonly JwtSettings _jwtSetting;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly string[] AllowedClaimTypes = new[]
    {
        ClaimTypes.NameIdentifier,
        ClaimTypes.Name,
        ClaimTypes.Role,
        JwtRegisteredClaimNames.Jti
    };

    public TokenService(JwtSettings jwtSetting, IHttpContextAccessor httpContextAccessor)
    {
        _jwtSetting = jwtSetting;
        _httpContextAccessor = httpContextAccessor;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public TokenResponse GenerateToken(string userId, string username, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims);
    }

    public TokenResponse? RegenerateToken(string accessToken, string refreshToken)
    {
        try
        {
            var principal = ValidateToken(accessToken, ignoreExpiration: true);
            if (principal == null) return null;

            var claims = principal.Claims
                .Where(c => AllowedClaimTypes.Contains(c.Type))
                .ToList();

            var tokenResponse = CreateToken(claims);
            tokenResponse.RefreshToken = refreshToken;
            return tokenResponse;
        }
        catch
        {
            return null;
        }
    }

    private TokenResponse CreateToken(IEnumerable<Claim> claims)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);
        var now = DateTime.UtcNow;
        var accessTokenExpiration = now.AddMinutes(_jwtSetting.ExpiredMinute);
        var refreshTokenExpiration = now.AddDays(_jwtSetting.RefreshTokenExpiredDays);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessTokenExpiration,
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            NotBefore = now,
            IssuedAt = now,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var accessToken = _tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = Guid.NewGuid().ToString("N");

        return new TokenResponse
        {
            AccessToken = _tokenHandler.WriteToken(accessToken),
            AccessTokenExpires = accessTokenExpiration,
            RefreshToken = refreshToken,
            RefreshTokenExpires = refreshTokenExpiration
        };
    }

    private ClaimsPrincipal? ValidateToken(string token, bool ignoreExpiration = false)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _jwtSetting.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSetting.Audience,
            ValidateLifetime = !ignoreExpiration,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            return _tokenHandler.ValidateToken(token, validationParams, out _);
        }
        catch
        {
            return null;
        }
    }

    public string? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
