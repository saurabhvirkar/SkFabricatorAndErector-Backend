using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SkFabricatorAndErector.Api.Extensions;
using SkFabricatorAndErector.Api.Filters;
using SkFabricatorAndErector.Api.Middleware;
using SkFabricatorAndErector.Application;
using SkFabricatorAndErector.Application.Extensions;
using SkFabricatorAndErector.Infrastructure;
using SkFabricatorAndErector.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Startup Security Validation ---
builder.Configuration.ValidateStartupSecurity(builder.Environment);

// --- Application Core & Infrastructure Registration ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// --- Presentation Layer Configuration ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy(builder.Configuration);

// Task 11 — Security hardening: rate limiting
builder.Services.AddRateLimitingPolicies();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

var app = builder.Build();

// --- Database Initialization ---
await app.UseDatabaseInitialization(app.Services, builder.Configuration);

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// HTTPS Redirection — enabled in production only (Nginx handles TLS termination)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// CORS must be first so preflight OPTIONS requests are handled before any other middleware.
// CorsExtensions uses AllowAnyHeader() which covers X-Correlation-ID and all custom headers.
app.UseCorsPolicy();

// --- Correlation ID & Security Headers ---
// These run AFTER CORS so they don't interfere with preflight responses.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseMiddleware<ErrorHandlingMiddleware>();

// Rate limiting must be after routing
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health probes — used by Docker HEALTHCHECK, Nginx, and UptimeRobot
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow })).AllowAnonymous();
app.MapGet("/health/live", () => Results.Ok(new { status = "Alive", timestamp = DateTime.UtcNow })).AllowAnonymous();
app.MapGet("/health/ready", () => Results.Ok(new { status = "Ready", timestamp = DateTime.UtcNow })).AllowAnonymous();
app.MapGet("/health/db-status", (IServiceProvider sp) =>
{
    try
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SkFabricatorAndErector.Infrastructure.Persistence.ApplicationDbContext>();
        var connStr = db.Database.GetConnectionString();
        var connHost = connStr != null ? (connStr.Contains('@') ? connStr.Split('@')[1].Split('/')[0] : connStr) : "Unknown";
        var userCount = db.Users.Count();
        var users = db.Users.Select(u => new { u.Email, u.UserName, u.Role, u.EmailConfirmed }).ToList();
        return Results.Ok(new {
            InitLog = SkFabricatorAndErector.Infrastructure.Persistence.DatabaseExtensions.LastInitLog,
            Provider = db.Database.ProviderName,
            ConnHostSnippet = connHost,
            UserCount = userCount,
            Users = users
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new {
            InitLog = SkFabricatorAndErector.Infrastructure.Persistence.DatabaseExtensions.LastInitLog,
            Error = ex.Message,
            StackTrace = ex.StackTrace
        });
    }
}).AllowAnonymous();

// Scalar API Explorer — development only
// OpenAPI JSON : /openapi/v1.json
// Scalar UI    : /scalar/v1
if (app.Environment.IsDevelopment())
{
    app.MapSwaggerDocumentation();
}

app.Run();
