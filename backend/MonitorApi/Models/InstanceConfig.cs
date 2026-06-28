namespace MonitorApi.Models;

public class InstanceConfig
{
    public int Id { get; set; }
    public string InstanceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
}
