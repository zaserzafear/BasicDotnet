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

        if (permissionAttributes == null || !permissionAttributes.Any())
        {
            return; // No permission required
        }

        var roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Result = new ForbidResult();
            return;
        }

        foreach (var permissionAttribute in permissionAttributes)
        {
            var hasPermission = await _permissionRepository.HasPermissionAsync(int.Parse(roleClaim), permissionAttribute.PermissionName);

            if (hasPermission)
            {
                return; // User has permission
            }
        }

        context.Result = new ForbidResult(); // No matching permission found
    }
}
