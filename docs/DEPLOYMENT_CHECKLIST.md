# Render Production Deployment Checklist — Phase 13

**Application:** SK Fabricator & Erector (ASP.NET Core Web API & Angular Frontend)

---

## Deployment Checklist Matrix

- [x] **Production Secrets Configuration**: Secrets set exclusively via Render Environment Variables (`Jwt__Key`, `ConnectionStrings__DefaultConnection`, `CloudinarySettings__CloudName`, `CloudinarySettings__ApiKey`, `CloudinarySettings__ApiSecret`, `SmtpSettings__Password`, `SeedUserPasswords__Admin`, `SeedUserPasswords__Manager`), keeping repository free of plaintext production secrets.
- [x] **Startup Fail-Fast Active**: Backend automatically validates `ValidateStartupSecurity` on boot in production environments. Any unconfigured `REPLACE_WITH_...` placeholder immediately aborts deployment startup with an exception.
- [x] **Production Database Connection**: SQLite / PostgreSQL production database connection configured with restricted access and rotated credentials.
- [x] **CORS Domain Restrictions**: Production `AllowedOrigins` explicitly configured to production domain (`https://skfabricator.onrender.com`), eliminating wildcard (`*`) origins.
- [x] **Swagger UI Disabled**: Swagger documentation endpoints (`/swagger`) gated behind `app.Environment.IsDevelopment()`, disabling API discovery in production.
- [x] **Rate Limiting & Proxy Headers**: `auth-fixed` (10 req/min) and `general-fixed` (300 req/min) rate limits active with `UseForwardedHeaders` configuring `X-Forwarded-For` and `X-Forwarded-Proto` for Render proxy compatibility.
- [x] **HttpOnly & Secure Cookies**: Refresh tokens issued as `HttpOnly`, `Secure`, `SameSite=Lax` cookies to prevent client XSS extraction.
- [x] **Rotated Admin/Manager Accounts**: Real production admin and manager user accounts initialized with unique, rotated passwords rather than default fallback values.
