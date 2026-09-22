using Microsoft.EntityFrameworkCore;
using VanFlowAPI.Models;

namespace VanFlowAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<VanOrder> VanOrders => Set<VanOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VanOrder>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.Property(order => order.CustomerName).HasMaxLength(120).IsRequired();
            entity.Property(order => order.VehicleModel).HasMaxLength(120).IsRequired();
            entity.Property(order => order.BuildNumber).HasMaxLength(40).IsRequired();
            entity.Property(order => order.ErpStatus).HasMaxLength(80).IsRequired();
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(order => order.BuildNumber).IsUnique();
        });
    }
}
