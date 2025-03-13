using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebApi.Extensions;
using BasicDotnet.WebApi.Filters;
using BasicDotnet.WebApi.RateLimit;
using Microsoft.OpenApi.Models;

namespace BasicDotnet.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<PermissionAuthorizationFilter>();
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

        var configuration = builder.Configuration;

        var jwtSetting = configuration.GetSection("Jwt").Get<JwtSetting>();
        if (jwtSetting == null)
        {
            throw new ArgumentNullException(nameof(jwtSetting));
        }

        builder.Services.AddJwtAuthentication(jwtSetting);
        builder.Services.AddAuthentication();

        builder.Services.AddApplicationExtension(jwtSetting);
        builder.Services.AddInfrastructureExtension(configuration);

        //// Load rate-limiting configuration from appsettings.json
        //var config = configuration.GetSection("RateLimiting");

        //int rejectionStatusCode = config.GetValue<int>("RejectionStatusCode");
        //int ipLimit = config.GetValue<int>("IpLimit");
        //int ipWindowSeconds = config.GetValue<int>("IpWindowSeconds");
        //int bruteForceLimit = config.GetValue<int>("BruteForceLimit");
        //int bruteForceWindowSeconds = config.GetValue<int>("BruteForceWindowSeconds");
        //int userLimitAuth = config.GetValue<int>("UserLimitAuthenticated");
        //int userLimitUnauth = config.GetValue<int>("UserLimitUnauthenticated");
        //int userWindowAuthMinutes = config.GetValue<int>("UserWindowAuthenticatedMinutes");
        //int userWindowUnauthSeconds = config.GetValue<int>("UserWindowUnauthenticatedSeconds");

        // Register RateLimitRedisAdapter and define policies
        builder.Services.AddSingleton<RateLimitRedisAdapter>(sp =>
        {
            var redisAdapter = new RateLimitRedisAdapter("basicdotnet.redis:6379, password=basicdotnet", "BasicDotnet.WebApi");

            // Add Policies
            redisAdapter.AddPolicy("sensitive", 5, TimeSpan.FromMinutes(1));  // For login/register/OTP
            redisAdapter.AddPolicy("public", 1000, TimeSpan.FromMinutes(1));  // For general API
            redisAdapter.AddPolicy("apiKey", 50, TimeSpan.FromMinutes(5));    // For specific API keys

            return redisAdapter;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.ApplyMigrations();

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<RateLimitMiddleware>();

        app.UseAuthorization();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
