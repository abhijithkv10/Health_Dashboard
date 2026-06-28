using Microsoft.AspNetCore.Mvc;
using MonitorApi.Models;
using MonitorApi.Services;

namespace MonitorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricStore _store;

    public MetricsController(IMetricStore store)
    {
        _store = store;
    }

    [HttpGet("{instanceId}")]
    public ActionResult<object> Get(string instanceId, [FromQuery] int minutes = 60)
    {
        var metrics = _store.GetMetrics(instanceId, minutes);
        var latest = _store.GetLatest(instanceId);

        return new
        {
            InstanceId = instanceId,
            Latest = latest,
            History = metrics,
            Count = metrics.Count
        };
    }

    [HttpPost("push")]
    public IActionResult Push([FromBody] PushMetricRequest request)
    {
        var latest = _store.GetLatest(request.InstanceId);
        _store.AddMetric(new MetricSnapshot
        {
            InstanceId = request.InstanceId,
            Timestamp = DateTime.UtcNow,
            CpuPercent = latest?.CpuPercent,
            MemoryPercent = request.MemoryPercent,
            DiskPercent = request.DiskPercent
        });

        return Ok(new { received = true });
    }
}
