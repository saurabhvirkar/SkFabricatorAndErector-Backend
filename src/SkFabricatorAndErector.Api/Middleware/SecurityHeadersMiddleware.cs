namespace SkFabricatorAndErector.Api.Middleware;

/// <summary>
/// Adds standard security HTTP response headers to every response.
/// Protects against common web vulnerabilities (clickjacking, MIME sniffing, etc.)
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // Prevent MIME type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Prevent clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Control referrer information
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Disable browser features not needed by this API
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            // Only send over HTTPS (1 year, include subdomains)
            if (context.Request.IsHttps)
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // Basic CSP — APIs typically don't serve HTML but this prevents misuse
            headers["X-XSS-Protection"] = "1; mode=block";

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
