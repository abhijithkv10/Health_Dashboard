using Microsoft.EntityFrameworkCore;
using MonitorApi.Models;

namespace MonitorApi.Data;

public class MonitorDbContext : DbContext
{
    public MonitorDbContext(DbContextOptions<MonitorDbContext> options) : base(options) { }

    public DbSet<MetricSnapshot> Metrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetricSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.InstanceId, e.Timestamp });
            entity.Property(e => e.InstanceId).HasMaxLength(64);
        });
    }
}