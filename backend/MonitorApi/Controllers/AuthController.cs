using Microsoft.AspNetCore.Mvc;
using MonitorApi.Services;

namespace MonitorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public AuthController(IJwtService jwt, IConfiguration config)
    {
        _jwt = jwt;
        _config = config;
    }

    public record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var adminUsername = _config["Auth:Admin:Username"] ?? "admin";
        var adminPassword = _config["Auth:Admin:Password"] ?? "admin";

        if (request.Username != adminUsername || request.Password != adminPassword)
            return Unauthorized(new { error = "Invalid username or password" });

        var token = _jwt.GenerateToken(request.Username, request.Username, "");

        return Ok(new
        {
            token,
            email = request.Username,
            name = request.Username,
            picture = ""
        });
    }
}
