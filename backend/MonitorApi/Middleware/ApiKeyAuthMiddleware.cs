namespace MonitorApi.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/metrics/push"))
        {
            var configuredKey = context.RequestServices
                .GetRequiredService<IConfiguration>()["Auth:ApiKey"];

            if (string.IsNullOrEmpty(configuredKey))
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("API key not configured");
                return;
            }

            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var providedKey)
                || providedKey != configuredKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }
}