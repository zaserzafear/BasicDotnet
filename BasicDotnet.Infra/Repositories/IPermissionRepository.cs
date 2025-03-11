namespace BasicDotnet.Infra.Repositories;

public interface IPermissionRepository
{
    Task<bool> HasPermissionAsync(int roleId, string permissionName);
}
