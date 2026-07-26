using Scalar.AspNetCore;

namespace SkFabricatorAndErector.Api.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Registers the built-in .NET 10 OpenAPI document generation service.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi("v1");
        return services;
    }

    /// <summary>
    /// Maps the OpenAPI JSON endpoint (/openapi/v1.json) and the Scalar UI (/scalar/v1).
    /// Call this alongside MapControllers(), not inside the middleware pipeline.
    /// </summary>
    public static WebApplication MapSwaggerDocumentation(this WebApplication app)
    {
        // Raw OpenAPI spec: GET /openapi/v1.json
        app.MapOpenApi().AllowAnonymous();

        // Scalar API Explorer UI: GET /scalar/v1
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("SK Fabricator & Erector API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("Bearer")
                .AddHttpAuthentication("Bearer", http =>
                {
                    http.Token = "your-jwt-token-here";
                });
        }).AllowAnonymous();

        return app;
    }
}
