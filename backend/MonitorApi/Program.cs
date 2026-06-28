using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSingleton<ICloudWatchService, CloudWatchService>();
builder.Services.AddSingleton<IAlertService, AlertService>();
builder.Services.AddHostedService<MetricPollingService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("PushEndpoint", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

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
app.UseRateLimiter();
app.MapControllers();
app.Run();
