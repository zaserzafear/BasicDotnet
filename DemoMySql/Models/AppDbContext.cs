using Microsoft.EntityFrameworkCore;

namespace DemoMySql.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id); // PK

            entity.HasIndex(e => e.Username)
                  .IsUnique(); // UQ

            entity.Property(e => e.Username)
                  .IsRequired()
                  .HasMaxLength(100);
        });
    }

    public DbSet<Customer> Customers { get; set; }
}
