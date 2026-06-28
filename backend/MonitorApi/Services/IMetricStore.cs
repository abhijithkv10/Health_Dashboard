using MonitorApi.Models;

namespace MonitorApi.Services;

public interface IMetricStore
{
    Task AddMetricAsync(MetricSnapshot snapshot);
    Task<List<MetricSnapshot>> GetMetricsAsync(string instanceId, int minutes = 60);
    Task<MetricSnapshot?> GetLatestAsync(string instanceId);
    Task<List<string>> GetAllInstanceIdsAsync();
}
