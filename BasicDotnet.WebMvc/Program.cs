using BasicDotnet.App.Configurations;
using BasicDotnet.App.Extensions;
using BasicDotnet.Infra.Extensions;
using BasicDotnet.WebMvc.Configurations;
using BasicDotnet.WebMvc.Middleware;
using BasicDotnet.WebMvc.Services;

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

builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("ApiConfig"));

var jwtSetting = configuration.GetSection("Jwt").Get<JwtSetting>();
if (jwtSetting == null)
{
    throw new ArgumentNullException(nameof(jwtSetting));
}

builder.Services.AddJwtAuthentication(jwtSetting);
builder.Services.AddAuthentication();
builder.Services.AddSingleton<AuthCookieService>();

builder.Services.AddHttpClientExtensions();

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
