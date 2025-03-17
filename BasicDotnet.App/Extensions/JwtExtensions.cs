using BasicDotnet.App.Configurations;
using BasicDotnet.App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BasicDotnet.App.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSetting jwtSetting)
    {
        // Ensure JwtSettings are properly configured
        if (string.IsNullOrEmpty(jwtSetting.SecretKey) ||
            string.IsNullOrEmpty(jwtSetting.Issuer) ||
            string.IsNullOrEmpty(jwtSetting.Audience))
        {
            throw new InvalidOperationException("JWT configuration settings are missing.");
        }

        services.AddSingleton(jwtSetting);

        // Add Authentication and configure JwtBearer
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SecretKey)), // Consistent encoding
                ValidateIssuer = true,
                ValidIssuer = jwtSetting.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSetting.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No clock skew for strict expiration time
            };
        });

        services.AddSingleton<TokenService>();
        return services;
    }
}
