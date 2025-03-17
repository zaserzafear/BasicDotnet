using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BasicDotnet.Infra.Persistence;

internal class AppDbContext : DbContext
{
    public DbSet<SuperAdmin> SuperAdmins { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Role>().HasData(
            new Role { Id = (int)UserRoleEnum.SuperAdmin, Name = UserRoleEnum.SuperAdmin.GetName() },
            new Role { Id = (int)UserRoleEnum.Admin, Name = UserRoleEnum.Admin.GetName() },
            new Role { Id = (int)UserRoleEnum.Customer, Name = UserRoleEnum.Customer.GetName() }
        );

        builder.Entity<Permission>().HasData(
            new Permission { Id = (int)PermissionEnum.ViewAllCustomers, Name = PermissionEnum.ViewAllCustomers.GetName(), Description = "Permission to view all customers" },
            new Permission { Id = (int)PermissionEnum.ViewOwnUserId, Name = PermissionEnum.ViewOwnUserId.GetName(), Description = "Permission to view only own customer details" }
        );

        builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        builder.Entity<RolePermission>().HasData(
            // SuperAdmin has permission to view all customers and their own customer
            new RolePermission { RoleId = (int)UserRoleEnum.SuperAdmin, PermissionId = (int)PermissionEnum.ViewAllCustomers },  // SuperAdmin can view all customers
            new RolePermission { RoleId = (int)UserRoleEnum.SuperAdmin, PermissionId = (int)PermissionEnum.ViewOwnUserId },  // SuperAdmin can view own user_id

            // Admin can view all customers and their own customer
            new RolePermission { RoleId = (int)UserRoleEnum.Admin, PermissionId = (int)PermissionEnum.ViewAllCustomers },  // Admin can view all customers
            new RolePermission { RoleId = (int)UserRoleEnum.Admin, PermissionId = (int)PermissionEnum.ViewOwnUserId },  // Admin can view own user_id

            // Customer can only view their own customer
            new RolePermission { RoleId = (int)UserRoleEnum.Customer, PermissionId = (int)PermissionEnum.ViewOwnUserId }   // Customer can view their own user_id customer
        );
    }
}
