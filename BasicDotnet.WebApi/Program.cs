using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Domain.PublicVars;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebApi.Extensions;
using BasicDotnet.WebApi.Filters;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

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

        builder.Services.AddRateLimiter(options =>
        {
            // Load rate-limiting configuration from appsettings.json
            var config = configuration.GetSection("RateLimiting");

            int rejectionStatusCode = config.GetValue<int>("RejectionStatusCode");
            int ipLimit = config.GetValue<int>("IpLimit");
            int ipWindowSeconds = config.GetValue<int>("IpWindowSeconds");
            int bruteForceLimit = config.GetValue<int>("BruteForceLimit");
            int bruteForceWindowSeconds = config.GetValue<int>("BruteForceWindowSeconds");
            int userLimitAuth = config.GetValue<int>("UserLimitAuthenticated");
            int userLimitUnauth = config.GetValue<int>("UserLimitUnauthenticated");
            int userWindowAuthMinutes = config.GetValue<int>("UserWindowAuthenticatedMinutes");
            int userWindowUnauthSeconds = config.GetValue<int>("UserWindowUnauthenticatedSeconds");

            // Set the HTTP response status code when rate limits are exceeded
            options.RejectionStatusCode = rejectionStatusCode;

            // Function to determine the client's IP address
            string GetClientIpAddress(HttpContext httpContext)
            {
                // Extract the first IP from X-Forwarded-For header (if present)
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    var clientIp = forwardedFor.Split(',').FirstOrDefault()?.Trim();

                    if (System.Net.IPAddress.TryParse(clientIp, out var ipAddress))
                    {
                        return ipAddress.ToString();
                    }
                }

                // Fallback to the direct remote IP address
                return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown client";
            }

            // 1️. Rate Limiting by IP Address
            //    - Limits total requests from a single IP within a fixed time window.
            //    - Helps prevent excessive API usage from a single client.
            options.AddPolicy(RateLimitPolicies.IpRateLimit, httpContext =>
            {
                var ipAddress = GetClientIpAddress(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = ipLimit, // Max requests allowed per time window
                        Window = TimeSpan.FromSeconds(ipWindowSeconds) // Time window duration
                    });
            });

            // 2️. Rate Limiting for Brute Force Protection
            //    - Applies stricter rate limits on sensitive endpoints (e.g., login, register, OTP requests).
            //    - Helps mitigate brute force attacks by slowing down repeated authentication attempts.
            options.AddPolicy(RateLimitPolicies.BruteForceProtection, httpContext =>
            {
                var ipAddress = GetClientIpAddress(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = bruteForceLimit, // Max failed attempts before blocking
                        Window = TimeSpan.FromSeconds(bruteForceWindowSeconds) // Block duration
                    });
            });

            // 3️. Rate Limiting by Authenticated User
            //    - Applies different rate limits based on whether the request is from an authenticated user.
            //    - If authenticated, rate limit is per user identity.
            //    - If unauthenticated, fallback to limiting by IP address.
            options.AddPolicy(RateLimitPolicies.UserRateLimit, httpContext =>
            {
                var userName = httpContext.User.Identity?.Name;
                bool isAuthenticated = !string.IsNullOrEmpty(userName);

                // Use username as the rate-limiting key if authenticated, otherwise fallback to IP address
                var partitionKey = isAuthenticated ? userName : GetClientIpAddress(httpContext);

                // Define different rate limits based on authentication status
                int permitLimit = isAuthenticated ? userLimitAuth : userLimitUnauth;
                TimeSpan window = isAuthenticated ? TimeSpan.FromMinutes(userWindowAuthMinutes) : TimeSpan.FromSeconds(userWindowUnauthSeconds);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit, // Max requests allowed per user/IP
                        Window = window // Time window duration
                    });
            });
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

        app.UseRateLimiter();

        app.UseAuthorization();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
