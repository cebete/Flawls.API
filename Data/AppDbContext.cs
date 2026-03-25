using Flawls.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Flawls.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Variant> Variants => Set<Variant>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Variant>()
                .HasMany(v => v.StockMovements)
                .WithOne(m => m.Variant)
                .HasForeignKey(m => m.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Variant>()
                .HasIndex(v => v.BarcodeId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice).HasPrecision(10, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.SellingPrice).HasPrecision(10, 2);
        }
    }
}
