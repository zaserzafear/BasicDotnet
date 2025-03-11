using BasicDotnet.Domain.Entities;

namespace BasicDotnet.Infra.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleByIdAsync(int roleId);
}
