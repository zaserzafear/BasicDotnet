using BasicDotnet.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicDotnet.Infra.Repositories;

internal class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int roleId, string permissionName)
    {
        return await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permissionName);
    }
}
