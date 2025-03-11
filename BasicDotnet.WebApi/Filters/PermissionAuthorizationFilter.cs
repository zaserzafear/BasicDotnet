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

        var ownUserIdPermissionAttributes = endpoint?
            .Metadata
            .OfType<HasOwnUserIdPermissionAttribute>()
            .ToList();

        // If there are no permission attributes, continue with no permission check
        if (permissionAttributes!.Count == 0 && ownUserIdPermissionAttributes!.Count == 0)
        {
            return;
        }

        string RequestId = _httpContextAccessor!.HttpContext!.TraceIdentifier;

        // Fetch role claim from user
        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Result = ResponseHelper.Error(RequestId, "Forbidden: No role claim found", 403);
            return;
        }

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            context.Result = ResponseHelper.Error(RequestId, "Forbidden: No unique_name claim found", 403);
            return;
        }

        // Track if permission checks are successful
        bool hasPermission = false;

        // Check generic permissions first
        foreach (var permissionAttribute in permissionAttributes!)
        {
            hasPermission = await _permissionRepository.HasPermissionAsync(int.Parse(roleClaim), permissionAttribute.PermissionName);

            if (hasPermission)
            {
                break; // If permission found, exit loop early
            }
        }

        if (!hasPermission)
        {
            context.Result = ResponseHelper.Error(RequestId, "Forbidden: User does not have permission", 403);
            return;
        }

        foreach (var ownCustomerPermissionAttribute in ownUserIdPermissionAttributes!)
        {
            // Assuming the user_id is passed as part of the route
            var userIdFromRoute = context.RouteData.Values["user_id"]?.ToString();

            if (userIdFromRoute != currentUserId)
            {
                context.Result = ResponseHelper.Error(RequestId, "Access denied: You are not authorized.", 403);
                return;
            }
        }
    }
}
