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

    public TokenService(JwtSetting jwtSetting)
    {
        _jwtSetting = jwtSetting;
    }

    public TokenResponse GenerateToken(UserBase user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = Guid.NewGuid().ToString("N"); // Generate a strong refresh token (can improve if needed)

        // Store refresh token securely in the database with user reference if required

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            AccessTokenExpires = accessTokenExpiration,
            RefreshToken = refreshToken,
            RefreshTokenExpires = refreshTokenExpiration
        };
    }

    public TokenResponse? RegenerateToken(string accessToken, string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);

        try
        {
            // Validate the old token (ignoring expiration)
            var principal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSetting.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSetting.Audience,
                ValidateLifetime = false, // Ignore expiration
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                return null;
            }

            // Extract claims
            var claims = principal.Claims.Where(c => c.Type != JwtRegisteredClaimNames.Aud).ToList();

            // Extend expiration
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

            var newToken = tokenHandler.CreateToken(tokenDescriptor);

            return new TokenResponse
            {
                AccessToken = tokenHandler.WriteToken(newToken),
                AccessTokenExpires = accessTokenExpiration,
                RefreshToken = refreshToken,
                RefreshTokenExpires = refreshTokenExpiration
            };
        }
        catch
        {
            return null;
        }
    }
}
