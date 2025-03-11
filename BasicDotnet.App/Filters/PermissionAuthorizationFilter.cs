using BasicDotnet.App.Attributes;
using BasicDotnet.Infra.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BasicDotnet.App.Filters;

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

        var ownCustomerPermissionAttributes = endpoint?
            .Metadata
            .OfType<HasOwnCustomerPermissionAttribute>()
            .ToList();

        // If there are no permission attributes, continue with no permission check
        if (permissionAttributes == null && ownCustomerPermissionAttributes == null)
        {
            return;
        }

        // Fetch role claim from user
        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Result = new ForbidResult();
            return;
        }

        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Track if permission checks are successful
        bool hasPermission = false;

        // Check generic permissions first
        foreach (var permissionAttribute in permissionAttributes)
        {
            hasPermission = await _permissionRepository.HasPermissionAsync(int.Parse(roleClaim), permissionAttribute.PermissionName);

            if (hasPermission)
            {
                break; // If permission found, exit loop early
            }
        }

        if (!hasPermission)
        {
            context.Result = new ForbidResult(); // No matching permission found
            return;
        }

        // Check for 'ViewOwnCustomer' permission
        foreach (var ownCustomerPermissionAttribute in ownCustomerPermissionAttributes)
        {
            // Assuming the user_id is passed as part of the route
            var userIdFromRoute = context.RouteData.Values["user_id"]?.ToString();

            if (userIdFromRoute != currentUserId)
            {
                context.Result = new ForbidResult(); // Deny access if the user is trying to access someone else's data
                return;
            }
        }
    }
}
