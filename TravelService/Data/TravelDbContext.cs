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
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();
    public DbSet<Expense> Expenses => Set<Expense>();

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

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activities");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Location).HasMaxLength(300);
            entity.Property(a => a.Status).HasMaxLength(20).IsRequired();
            entity.Property(a => a.EstimatedCost).HasColumnType("decimal(18,2)");
            entity.HasOne(a => a.TravelPlan)
                .WithMany(p => p.Activities)
                .HasForeignKey(a => a.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Destination)
                .WithMany()
                .HasForeignKey(a => a.DestinationId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(a => new { a.TravelPlanId, a.ActivityDate });
        });

        modelBuilder.Entity<ShareLink>(entity =>
        {
            entity.ToTable("ShareLinks");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Token).HasMaxLength(100).IsRequired();
            entity.Property(s => s.AccessType).HasMaxLength(10).IsRequired();
            entity.Property(s => s.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(s => s.ExpiresAt).HasColumnType("datetime2(0)");
            entity.HasIndex(s => s.Token).IsUnique();
            entity.HasOne(s => s.TravelPlan)
                .WithMany(p => p.ShareLinks)
                .HasForeignKey(s => s.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.TravelPlan)
                .WithMany()
                .HasForeignKey(e => e.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TravelPlanId);
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.ToTable("ChecklistItems");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).HasMaxLength(300).IsRequired();
            entity.HasOne(c => c.TravelPlan)
                .WithMany(p => p.ChecklistItems)
                .HasForeignKey(c => c.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
