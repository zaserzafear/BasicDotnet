using BasicDotnet.Infra.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BasicDotnet.Infra.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClientExtensions(this IServiceCollection services,
        HttpClientSettings httpClientSettings)
    {
        services.AddHttpClient();
        services.AddSingleton(httpClientSettings);
        services.AddSingleton<HttpClientService>();

        return services;
    }
}
