using BasicDotnet.Infra.Persistence;
using BasicDotnet.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasicDotnet.Infra.Extensions;

public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructureExtension(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
