namespace SkFabricatorAndErector.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[CorrelationIdHeaderName] = correlationId.ToString();
        context.Response.Headers[CorrelationIdHeaderName] = correlationId.ToString();

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId.ToString() }))
        {
            await _next(context);
        }
    }
}
