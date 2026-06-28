using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MonitorApi.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string email, string name, string picture)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Auth:JwtSecret"] ?? "CHANGE_ME_TO_A_RANDOM_SECRET_32CHARS!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim("picture", picture),
        };

        var token = new JwtSecurityToken(
            issuer: "aws-monitor",
            audience: "aws-monitor",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
