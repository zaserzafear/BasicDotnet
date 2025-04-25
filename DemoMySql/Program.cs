
using DemoMySql.Extensions;
using DemoMySql.Models;
using DemoMySql.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace DemoMySql
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();
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

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            if (jwtSetting == null)
            {
                throw new ArgumentNullException(nameof(jwtSetting));
            }
            // Ensure JwtSettings are properly configured
            if (string.IsNullOrEmpty(jwtSetting.SecretKey) ||
                string.IsNullOrEmpty(jwtSetting.Issuer) ||
                string.IsNullOrEmpty(jwtSetting.Audience))
            {
                throw new InvalidOperationException("JWT configuration settings are missing.");
            }

            builder.Services.AddSingleton(jwtSetting);

            // Add Authentication and configure JwtBearer
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SecretKey)), // Consistent encoding
                    ValidateIssuer = true,
                    ValidIssuer = jwtSetting.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSetting.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // No clock skew for strict expiration time
                };
            });

            builder.Services.AddSingleton<TokenService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                scope.ServiceProvider.ApplyMigrations();

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
