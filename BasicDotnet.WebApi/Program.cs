using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebApi.RateLimit;
using BasicDotnet.WebApi.RateLimit.Configurations;
using BasicDotnet.WebApi.Security.Filters;
using BasicDotnet.WebMvc.Configurations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace BasicDotnet.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null;
        });
        builder.Services.AddRouting(options => options.LowercaseUrls = true);
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<PermissionAuthorizationFilter>();
        });

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            if (allowedOrigins.Length > 0 && allowedOrigins[0] == "*")
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            }
            else
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            }
        });

        // Register RateLimitRedisAdapter and define policies
        builder.Services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));
        builder.Services.AddSingleton<RateLimitRedisAdapter>();

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

        var jwtSetting = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSetting == null)
        {
            throw new ArgumentNullException(nameof(jwtSetting));
        }

        builder.Services.AddApplicationExtension(jwtSetting);
        builder.Services.AddJwtAuthentication(jwtSetting);
        builder.Services.AddAuthentication();

        builder.Services.AddInfrastructureExtension(configuration);

        var dataProtectionConfig = builder.Configuration.GetSection("DataProtection").Get<DataProtectionRedisConfig>();
        if (dataProtectionConfig == null || string.IsNullOrEmpty(dataProtectionConfig.RedisConnection))
        {
            throw new Exception("Data Protection configuration is missing or invalid.");
        }
        var dataProtectionRedis = ConnectionMultiplexer.Connect(dataProtectionConfig.RedisConnection);
        builder.Services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(() => dataProtectionRedis.GetDatabase(dataProtectionConfig.DatabaseId), dataProtectionConfig.KeyPrefix);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.ApplyMigrations();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<RateLimitMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
