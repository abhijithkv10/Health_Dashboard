using Microsoft.EntityFrameworkCore;
using MonitorApi.Data;
using MonitorApi.Models;

namespace MonitorApi.Services;

public class InstanceService : IInstanceService
{
    private readonly IDbContextFactory<MonitorDbContext> _contextFactory;

    public InstanceService(IDbContextFactory<MonitorDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<InstanceConfig>> GetAllAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Instances.OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<InstanceConfig?> GetByInstanceIdAsync(string instanceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Instances.FirstOrDefaultAsync(i => i.InstanceId == instanceId);
    }

    public async Task<InstanceConfig> AddAsync(InstanceConfig instance)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Instances.Add(instance);
        await db.SaveChangesAsync();
        return instance;
    }

    public async Task<InstanceConfig?> UpdateAsync(int id, InstanceConfig updated)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.Instances.FindAsync(id);
        if (existing == null) return null;

        existing.InstanceId = updated.InstanceId;
        existing.Name = updated.Name;
        existing.Region = updated.Region;
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.Instances.FindAsync(id);
        if (existing == null) return false;

        db.Instances.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
