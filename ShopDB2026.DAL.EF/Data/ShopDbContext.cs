using ShopDB.DAL.EF.Entities;
using Microsoft.EntityFrameworkCore;

namespace ShopDB.DAL.EF.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductHistory> ProductHistories { get; set; } = null!;
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       
        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(150);
            entity.Property(p => p.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(p => p.SellingPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ProductHistory>()
            .Property(ph => ph.Comment)
            .HasMaxLength(255);

        
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.ClientSetNull;
        }
    }
}