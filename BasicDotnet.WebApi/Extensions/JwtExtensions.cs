using BasicDotnet.App.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BasicDotnet.WebApi.Extensions;

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

        // Add Authentication and configure JwtBearer
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Check if the environment is not development
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSetting.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSetting.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSetting.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
