using System.Collections.Concurrent;
using MonitorApi.Models;

namespace MonitorApi.Services;

public class MetricStore : IMetricStore
{
    private readonly ConcurrentDictionary<string, List<MetricSnapshot>> _metrics = new();
    private readonly object _lock = new();

    public void AddMetric(MetricSnapshot snapshot)
    {
        var list = _metrics.GetOrAdd(snapshot.InstanceId, _ => new List<MetricSnapshot>());

        lock (_lock)
        {
            list.Add(snapshot);
            var cutoff = DateTime.UtcNow.AddMinutes(-60);
            list.RemoveAll(m => m.Timestamp < cutoff);
        }
    }

    public List<MetricSnapshot> GetMetrics(string instanceId, int minutes = 60)
    {
        if (!_metrics.TryGetValue(instanceId, out var list))
            return new List<MetricSnapshot>();

        lock (_lock)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-minutes);
            return list.Where(m => m.Timestamp >= cutoff).OrderBy(m => m.Timestamp).ToList();
        }
    }

    public MetricSnapshot? GetLatest(string instanceId)
    {
        if (!_metrics.TryGetValue(instanceId, out var list))
            return null;

        lock (_lock)
        {
            return list.OrderByDescending(m => m.Timestamp).FirstOrDefault();
        }
    }

    public List<string> GetAllInstanceIds()
    {
        return _metrics.Keys.ToList();
    }
}
