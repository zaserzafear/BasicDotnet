using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebMvc.Configurations;
using BasicDotnet.WebMvc.Middleware;
using BasicDotnet.WebMvc.Services;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllersWithViews();

builder.Services.AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"));

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.ConsentCookie.Expiration = TimeSpan.FromDays(7);
    options.ConsentCookieValue = "true";
});

builder.Services.Configure<ApiConfig>(configuration.GetSection("ApiConfig"));

var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new ArgumentNullException(nameof(jwtSettings));
}

builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddAuthentication();
builder.Services.AddSingleton<AuthCookieService>();

var HttpClientSettings = configuration.GetSection("HttpClientSettings").Get<HttpClientSettings>();
if (HttpClientSettings == null)
{
    throw new ArgumentNullException(nameof(HttpClientSettings));
}
builder.Services.AddHttpClientExtensions(HttpClientSettings);

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.MapReverseProxy();

app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<JwtRefreshMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
