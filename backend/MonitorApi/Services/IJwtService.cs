namespace MonitorApi.Services;

public interface IJwtService
{
    string GenerateToken(string email, string name, string picture);
}
