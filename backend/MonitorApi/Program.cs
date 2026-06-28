using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonitorApi.Data;
using MonitorApi.Middleware;
using MonitorApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=aws_monitor;Username=postgres;Password=postgres";

builder.Services.AddDbContextFactory<MonitorDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IMetricStore, MetricStore>();
builder.Services.AddSingleton<IInstanceService, InstanceService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IAlertService, AlertService>();
builder.Services.AddHostedService<MetricPollingService>();

var jwtSecret = builder.Configuration["Auth:JwtSecret"] ?? "CHANGE_ME_TO_A_RANDOM_SECRET_32CHARS!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "aws-monitor",
            ValidAudience = "aws-monitor",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MonitorDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/api/health");
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
