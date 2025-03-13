using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebApi.Extensions;
using BasicDotnet.WebApi.RateLimit;
using BasicDotnet.WebApi.Security.Filters;
using Microsoft.OpenApi.Models;

namespace BasicDotnet.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<PermissionAuthorizationFilter>();
        });

        // Register RateLimitRedisAdapter and define policies
        builder.Services.AddSingleton<RateLimitRedisAdapter>(sp =>
        {
            // Load rate-limiting configuration from appsettings.json
            var config = configuration.GetSection("RateLimiting");
            var redisConnectionString = config.GetValue<string>("Redis:ConnectionString")!;
            var redisInstanceName = config.GetValue<string>("Redis:InstanceName")!;
            var sensitiveLimit = config.GetValue<int>($"{RateLimitPolicies.Sensitive}:Limit");
            var sensitiveWindows = config.GetValue<TimeSpan>($"{RateLimitPolicies.Sensitive}:Window");
            var publicLimit = config.GetValue<int>($"{RateLimitPolicies.Public}:Limit");
            var publicWindows = config.GetValue<TimeSpan>($"{RateLimitPolicies.Public}:Window");
            var apiKeyHeader = config.GetValue<string>($"{RateLimitPolicies.ApiKey}:Header");
            var apiKeyLimit = config.GetValue<int>($"{RateLimitPolicies.ApiKey}:Limit");
            var apiKeyWindows = config.GetValue<TimeSpan>($"{RateLimitPolicies.ApiKey}:Window");

            var redisAdapter = new RateLimitRedisAdapter(redisConnectionString, redisInstanceName);

            // Add Policies
            redisAdapter.AddPolicy(RateLimitPolicies.Sensitive, sensitiveLimit, sensitiveWindows);
            redisAdapter.AddPolicy(RateLimitPolicies.Public, publicLimit, publicWindows);
            redisAdapter.AddPolicy(RateLimitPolicies.ApiKey, apiKeyLimit, apiKeyWindows);

            return redisAdapter;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        var jwtSetting = configuration.GetSection("Jwt").Get<JwtSetting>();
        if (jwtSetting == null)
        {
            throw new ArgumentNullException(nameof(jwtSetting));
        }

        builder.Services.AddJwtAuthentication(jwtSetting);
        builder.Services.AddAuthentication();

        builder.Services.AddApplicationExtension(jwtSetting);
        builder.Services.AddInfrastructureExtension(configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.ApplyMigrations();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.UseAuthorization();

        app.UseMiddleware<RateLimitMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
