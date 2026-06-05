using TravelService.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Data;

public class TravelDbContext : DbContext
{
    public TravelDbContext(DbContextOptions<TravelDbContext> options)
        : base(options)
    {
    }

    public DbSet<TravelPlan> TravelPlans => Set<TravelPlan>();
    public DbSet<Destination> Destinations => Set<Destination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TravelPlan>(entity =>
        {
            entity.ToTable("TravelPlans");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.PlannedBudget).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(p => p.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(p => p.UserId);
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.ToTable("Destinations");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Location).HasMaxLength(300).IsRequired();
            entity.HasOne(d => d.TravelPlan)
                .WithMany(p => p.Destinations)
                .HasForeignKey(d => d.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
