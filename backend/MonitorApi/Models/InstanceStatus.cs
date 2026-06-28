namespace MonitorApi.Models;

public class InstanceStatus
{
    public string InstanceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "OK";
    public double? CpuPercent { get; set; }
    public double? MemoryPercent { get; set; }
    public double? DiskPercent { get; set; }
    public string CpuStatus { get; set; } = "OK";
    public string MemoryStatus { get; set; } = "OK";
    public string DiskStatus { get; set; } = "OK";
    public DateTime LastUpdated { get; set; }
    public string? AlertMessage { get; set; }
}
