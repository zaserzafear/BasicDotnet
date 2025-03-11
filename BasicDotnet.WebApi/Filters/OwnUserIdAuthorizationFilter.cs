using BasicDotnet.WebApi.Attributes;
using BasicDotnet.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BasicDotnet.WebApi.Filters;

public class OwnUserIdAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OwnUserIdAuthorizationFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var ownUserIdPermissionAttributes = endpoint?
            .Metadata
            .OfType<HasOwnUserIdPermissionAttribute>()
            .ToList();

        if (ownUserIdPermissionAttributes is null || ownUserIdPermissionAttributes.Count == 0)
        {
            return Task.CompletedTask;
        }

        string requestId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            context.Result = ResponseHelper.Error(requestId, "Forbidden: No unique_name claim found", 403);
            return Task.CompletedTask;
        }

        var userIdFromRoute = context.RouteData.Values["user_id"]?.ToString();
        if (userIdFromRoute != currentUserId)
        {
            context.Result = ResponseHelper.Error(requestId, "Access denied: You are not authorized.", 403);
        }

        return Task.CompletedTask;
    }
}
