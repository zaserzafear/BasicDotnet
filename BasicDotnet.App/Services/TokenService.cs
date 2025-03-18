using BasicDotnet.App.Configurations;
using BasicDotnet.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BasicDotnet.App.Services;

public class TokenService
{
    private readonly JwtSetting _jwtSetting;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(JwtSetting jwtSetting)
    {
        _jwtSetting = jwtSetting;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public TokenResponse GenerateToken(UserBase user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()),
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

            var allowedClaimTypes = new[]
            {
                ClaimTypes.Name,
                ClaimTypes.Role,
                JwtRegisteredClaimNames.Jti
            };

            var claims = principal.Claims
                .Where(c => allowedClaimTypes.Contains(c.Type))
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
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSetting.ExpiredMinute);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSetting.RefreshTokenExpiredDays);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessTokenExpiration,
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
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

        return _tokenHandler.ValidateToken(token, validationParams, out _);
    }
}
