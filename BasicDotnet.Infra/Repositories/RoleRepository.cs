using BasicDotnet.Domain.Entities;
using BasicDotnet.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicDotnet.Infra.Repositories;

internal class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles
            .Where(r => r.Id == roleId)
            .FirstOrDefaultAsync();
    }
}
