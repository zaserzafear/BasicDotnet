namespace BasicDotnet.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation property for the many-to-many relationship with Roles
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
