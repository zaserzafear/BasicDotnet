using DemoMySql.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoMySql.Extensions;

public static class DatabaseExtensions
{
    public static void ApplyMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}
