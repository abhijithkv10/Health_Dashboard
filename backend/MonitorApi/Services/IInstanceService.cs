using MonitorApi.Models;

namespace MonitorApi.Services;

public interface IInstanceService
{
    Task<List<InstanceConfig>> GetAllAsync();
    Task<InstanceConfig?> GetByInstanceIdAsync(string instanceId);
    Task<InstanceConfig> AddAsync(InstanceConfig instance);
    Task<InstanceConfig?> UpdateAsync(int id, InstanceConfig instance);
    Task<bool> DeleteAsync(int id);
}
