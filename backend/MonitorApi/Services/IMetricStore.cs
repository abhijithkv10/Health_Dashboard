using MonitorApi.Models;

namespace MonitorApi.Services;

public interface IMetricStore
{
    void AddMetric(MetricSnapshot snapshot);
    List<MetricSnapshot> GetMetrics(string instanceId, int minutes = 60);
    MetricSnapshot? GetLatest(string instanceId);
    List<string> GetAllInstanceIds();
}
