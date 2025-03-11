using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebApi.Extensions;
using BasicDotnet.WebApi.Filters;
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
            options.Filters.Add<OwnUserIdAuthorizationFilter>();
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


        app.MapControllers();

        app.Run();
    }
}
