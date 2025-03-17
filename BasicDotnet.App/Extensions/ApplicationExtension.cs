using BasicDotnet.App.Configurations;
using BasicDotnet.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BasicDotnet.App.Extensions;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplicationExtension(this IServiceCollection services, JwtSetting jwtSetting)
    {
        services.AddScoped<AuthService>();

        return services;
    }
}
