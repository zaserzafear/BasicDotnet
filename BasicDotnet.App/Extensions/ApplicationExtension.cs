using BasicDotnet.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicDotnet.App.Extensions;

public static class ApplicationExtension
{
    public static IServiceCollection AddApplicationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthService>();

        return services;
    }
}
