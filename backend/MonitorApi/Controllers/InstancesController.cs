using Microsoft.AspNetCore.Mvc;
using MonitorApi.Models;
using MonitorApi.Services;

namespace MonitorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstancesController : ControllerBase
{
    private readonly IMetricStore _store;
    private readonly IInstanceService _instances;

    public InstancesController(IMetricStore store, IInstanceService instances)
    {
        _store = store;
        _instances = instances;
    }

    [HttpGet]
    public async Task<ActionResult<List<InstanceStatus>>> GetAll()
    {
        var configuredInstances = await _instances.GetAllAsync();
        var result = new List<InstanceStatus>();

        foreach (var instance in configuredInstances)
        {
            var latest = await _store.GetLatestAsync(instance.InstanceId);
            var status = EvaluateStatus(instance, latest);
            result.Add(status);
        }

        var knownIds = configuredInstances.Select(c => c.InstanceId).ToHashSet();
        var unconfiguredIds = await _store.GetAllInstanceIdsAsync();

        foreach (var id in unconfiguredIds.Where(id => !knownIds.Contains(id)))
        {
            var latest = await _store.GetLatestAsync(id);
            result.Add(EvaluateStatus(new InstanceConfig
            {
                InstanceId = id,
                Name = id,
                Region = "us-east-1"
            }, latest));
        }

        return result;
    }

    private static InstanceStatus EvaluateStatus(InstanceConfig config, MetricSnapshot? latest)
    {
        var status = new InstanceStatus
        {
            InstanceId = config.InstanceId,
            Name = config.Name,
            LastUpdated = latest?.Timestamp ?? DateTime.MinValue
        };

        if (latest == null) return status;

        status.CpuPercent = latest.CpuPercent;
        status.MemoryPercent = latest.MemoryPercent;
        status.DiskPercent = latest.DiskPercent;

        var alerts = new List<string>();

        if (latest.CpuPercent > 80)
        {
            status.CpuStatus = "Critical";
            alerts.Add($"CPU: {latest.CpuPercent}%");
        }
        else if (latest.CpuPercent > 60)
        {
            status.CpuStatus = "Warning";
        }

        if (latest.MemoryPercent > 80)
        {
            status.MemoryStatus = "Critical";
            alerts.Add($"Memory: {latest.MemoryPercent}%");
        }
        else if (latest.MemoryPercent > 70)
        {
            status.MemoryStatus = "Warning";
        }

        if (latest.DiskPercent > 90)
        {
            status.DiskStatus = "Critical";
            alerts.Add($"Disk: {latest.DiskPercent}%");
        }
        else if (latest.DiskPercent > 80)
        {
            status.DiskStatus = "Warning";
        }

        if (alerts.Count > 0)
        {
            status.Status = "Critical";
            status.AlertMessage = string.Join(" | ", alerts);
        }
        else if (status.CpuStatus == "Warning" || status.MemoryStatus == "Warning" || status.DiskStatus == "Warning")
        {
            status.Status = "Warning";
        }

        return status;
    }
}
