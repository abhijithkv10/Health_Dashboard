using Microsoft.EntityFrameworkCore;
using MonitorApi.Data;
using MonitorApi.Models;

namespace MonitorApi.Services;

public class MetricStore : IMetricStore
{
    private readonly IDbContextFactory<MonitorDbContext> _contextFactory;

    public MetricStore(IDbContextFactory<MonitorDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddMetricAsync(MetricSnapshot snapshot)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Metrics.Add(snapshot);
        await db.SaveChangesAsync();
    }

    public async Task<List<MetricSnapshot>> GetMetricsAsync(string instanceId, int minutes = 60)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var cutoff = DateTime.UtcNow.AddMinutes(-minutes);
        return await db.Metrics
            .Where(m => m.InstanceId == instanceId && m.Timestamp >= cutoff)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<MetricSnapshot?> GetLatestAsync(string instanceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Metrics
            .Where(m => m.InstanceId == instanceId)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> GetAllInstanceIdsAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Metrics
            .Select(m => m.InstanceId)
            .Distinct()
            .ToListAsync();
    }
}