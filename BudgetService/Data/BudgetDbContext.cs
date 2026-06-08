using BudgetService.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetService.Data;

public class BudgetDbContext : DbContext
{
    public BudgetDbContext(DbContextOptions<BudgetDbContext> options)
        : base(options)
    {
    }

    public DbSet<TravelPlan> TravelPlans => Set<TravelPlan>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TravelPlan>(entity =>
        {
            entity.ToTable("TravelPlans");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.PlannedBudget).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.TravelPlan)
                .WithMany(p => p.Expenses)
                .HasForeignKey(e => e.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TravelPlanId);
        });
    }
}
