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
    public async Task<ActionResult<object>> Get(string instanceId, [FromQuery] int minutes = 60)
    {
        var metrics = await _store.GetMetricsAsync(instanceId, minutes);
        var latest = await _store.GetLatestAsync(instanceId);

        return new
        {
            InstanceId = instanceId,
            Latest = latest,
            History = metrics,
            Count = metrics.Count
        };
    }

    [HttpPost("push")]
    public async Task<IActionResult> Push([FromBody] PushMetricRequest request)
    {
        await _store.AddMetricAsync(new MetricSnapshot
        {
            InstanceId = request.InstanceId,
            Timestamp = DateTime.UtcNow,
            CpuPercent = request.CpuPercent,
            MemoryPercent = request.MemoryPercent,
            DiskPercent = request.DiskPercent
        });

        return Ok(new { received = true });
    }
}
