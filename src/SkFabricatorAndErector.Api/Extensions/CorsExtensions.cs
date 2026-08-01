namespace SkFabricatorAndErector.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:4200", "https://skfabricator.onrender.com"];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()          // Covers X-Correlation-ID, Authorization, Content-Type, etc.
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .WithExposedHeaders(       // Allow Angular to read these response headers
                          "X-Correlation-ID",
                          "Content-Disposition"  // Needed for file downloads
                      );
            });
        });
        return services;
    }

    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors();
        return app;
    }
}
