using Microsoft.EntityFrameworkCore;
using MonitorApi.Models;

namespace MonitorApi.Data;

public class MonitorDbContext : DbContext
{
    public MonitorDbContext(DbContextOptions<MonitorDbContext> options) : base(options) { }

    public DbSet<MetricSnapshot> Metrics { get; set; }
    public DbSet<InstanceConfig> Instances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetricSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.InstanceId, e.Timestamp });
            entity.Property(e => e.InstanceId).HasMaxLength(64);
        });

        modelBuilder.Entity<InstanceConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InstanceId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Region).HasMaxLength(32);
            entity.HasIndex(e => e.InstanceId).IsUnique();
        });
    }
}
