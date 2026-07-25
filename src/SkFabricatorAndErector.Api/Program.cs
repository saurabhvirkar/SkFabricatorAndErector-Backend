using Microsoft.AspNetCore.HttpOverrides;
using SkFabricatorAndErector.Api.Extensions;
using SkFabricatorAndErector.Api.Filters;
using SkFabricatorAndErector.Api.Middleware;
using SkFabricatorAndErector.Application;
using SkFabricatorAndErector.Infrastructure;
using SkFabricatorAndErector.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Application Core & Infrastructure Registration ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// --- Presentation Layer Configuration ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy(builder.Configuration);

// Task 11 — Security hardening: rate limiting
builder.Services.AddRateLimitingPolicies();

var app = builder.Build();

// --- Database Initialization ---
await app.UseDatabaseInitialization(app.Services, builder.Configuration);

// --- Security Headers (before all other middleware) ---
app.UseMiddleware<SecurityHeadersMiddleware>();

// Explicitly handle OPTIONS requests for CORS preflight
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers.Origin.ToString();
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:4200", "https://skfabricatorui.onrender.com"];

        if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", new[] { origin });
            context.Response.Headers.Append("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept, Authorization" });
            context.Response.Headers.Append("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
            context.Response.Headers.Append("Access-Control-Allow-Credentials", new[] { "true" });
        }
        context.Response.StatusCode = 200;
        return;
    }
    await next();
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// HTTPS Redirection — enabled in production only (Render handles TLS termination)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCorsPolicy();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwaggerDocumentation();

// Rate limiting must be after routing
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Minimal health probe — used by Docker HEALTHCHECK and Render platform
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();

