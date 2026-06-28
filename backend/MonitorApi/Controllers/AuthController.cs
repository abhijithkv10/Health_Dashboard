using Google.Apis.Auth;
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

    public record GoogleLoginRequest(string IdToken);

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Auth:Google:ClientId"] ?? "" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

            var allowedDomain = _config["Auth:Google:AllowedDomain"];
            if (!string.IsNullOrEmpty(allowedDomain))
            {
                var emailDomain = payload.Email?.Split('@').LastOrDefault();
                if (emailDomain == null || !emailDomain.Equals(allowedDomain, StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { error = $"Email must be from @{allowedDomain} domain" });
            }

            var token = _jwt.GenerateToken(
                payload.Email ?? "unknown",
                payload.Name ?? "unknown",
                payload.Picture ?? ""
            );

            return Ok(new
            {
                token,
                email = payload.Email,
                name = payload.Name,
                picture = payload.Picture
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { error = "Invalid Google token" });
        }
    }
}
