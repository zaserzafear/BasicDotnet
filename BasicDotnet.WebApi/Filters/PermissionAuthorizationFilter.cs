using BasicDotnet.Domain.Enums;
using BasicDotnet.Infra.Repositories;
using BasicDotnet.WebApi.Attributes;
using BasicDotnet.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BasicDotnet.WebApi.Filters;

public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationFilter(
        IPermissionRepository permissionRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _permissionRepository = permissionRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var permissionAttributes = endpoint?
            .Metadata
            .OfType<HasPermissionAttribute>()
            .ToList();

        if (permissionAttributes is null || permissionAttributes.Count == 0)
        {
            return;
        }

        string requestId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;
        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Result = ResponseHelper.Error(requestId, "Forbidden: No role claim found", 403);
            return;
        }

        bool hasPermission = false;
        foreach (var permissionAttribute in permissionAttributes)
        {
            var permissionName = permissionAttribute.Permission.GetName();
            hasPermission = await _permissionRepository.HasPermissionAsync(int.Parse(roleClaim), permissionName);

            if (hasPermission)
            {
                if (permissionAttribute.Permission == PermissionEnum.ViewOwnUserId && !HasAccessToOwnUser(context))
                {
                    context.Result = ResponseHelper.Error(requestId, "Access denied: You are not authorized.", 403);
                    return;
                }
                break;
            }
        }

        if (!hasPermission)
        {
            context.Result = ResponseHelper.Error(requestId, "Forbidden: User does not have permission", 403);
            return;
        }
    }

    private bool HasAccessToOwnUser(AuthorizationFilterContext context)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return false;
        }

        var userIdFromRoute = context.RouteData.Values["user_id"]?.ToString();
        return userIdFromRoute == currentUserId;
    }
}
