using Microsoft.AspNetCore.Mvc;
using MonitorApi.Models;
using MonitorApi.Services;

namespace MonitorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstancesController : ControllerBase
{
    private readonly IMetricStore _store;
    private readonly IConfiguration _config;

    public InstancesController(IMetricStore store, IConfiguration config)
    {
        _store = store;
        _config = config;
    }

    [HttpGet]
    public ActionResult<List<InstanceStatus>> GetAll()
    {
        var configuredInstances = _config.GetSection("Instances").Get<List<InstanceConfig>>() ?? new();
        var result = new List<InstanceStatus>();

        foreach (var instance in configuredInstances)
        {
            var latest = _store.GetLatest(instance.Id);
            var status = EvaluateStatus(instance, latest);
            result.Add(status);
        }

        var unconfigured = _store.GetAllInstanceIds()
            .Where(id => !configuredInstances.Any(c => c.Id == id))
            .ToList();

        foreach (var id in unconfigured)
        {
            var latest = _store.GetLatest(id);
            result.Add(EvaluateStatus(new InstanceConfig { Id = id, Name = id }, latest));
        }

        return result;
    }

    private static InstanceStatus EvaluateStatus(InstanceConfig config, MetricSnapshot? latest)
    {
        var status = new InstanceStatus
        {
            InstanceId = config.Id,
            Name = config.Name,
            LastUpdated = latest?.Timestamp ?? DateTime.MinValue
        };

        if (latest == null) return status;

        status.CpuPercent = latest.CpuPercent;
        status.MemoryPercent = latest.MemoryPercent;
        status.DiskPercent = latest.DiskPercent;

        var alerts = new List<string>();

        // CPU evaluation (checked against latest - AlertService handles duration)
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
