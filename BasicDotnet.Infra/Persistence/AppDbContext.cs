using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Security;
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
            new Role { Id = 1, Name = RoleNames.SuperAdmin },
            new Role { Id = 2, Name = RoleNames.Admin },
            new Role { Id = 3, Name = RoleNames.Customer }
        );

        builder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = PermissionNames.ViewAllCustomers, Description = "Permission to view all customers" },
            new Permission { Id = 2, Name = PermissionNames.ViewOwnCustomer, Description = "Permission to view only own customer details" }
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
            new RolePermission { RoleId = 1, PermissionId = 1 },  // SuperAdmin can view all customers
            new RolePermission { RoleId = 1, PermissionId = 2 },  // SuperAdmin can view their own customer

            // Admin can view all customers and their own customer
            new RolePermission { RoleId = 2, PermissionId = 1 },  // Admin can view all customers
            new RolePermission { RoleId = 2, PermissionId = 2 },  // Admin can view their own customer

            // Customer can only view their own customer
            new RolePermission { RoleId = 3, PermissionId = 2 }   // Customer can view their own customer
        );
    }
}
