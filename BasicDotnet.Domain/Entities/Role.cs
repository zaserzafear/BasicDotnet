﻿namespace BasicDotnet.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property for the many-to-many relationship with Permissions
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
