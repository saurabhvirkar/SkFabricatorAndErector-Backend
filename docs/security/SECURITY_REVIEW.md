# Security Review & Audit — SkFabricatorAndErector-Backend

**Review Date**: 2026-07-24
**Status**: ✅ Hardened & Verified

---

## Executive Summary

This document is the authoritative security audit for the **SK Fabricator & Erector** backend service. All items below represent _actual code changes applied_ — not aspirational goals. The application follows ASP.NET Core security best practices and Clean Architecture guidelines.

---

## 1. Authentication & JWT Token Management ✅

**File**: [`JwtTokenGenerator.cs`](../../src/SkFabricatorAndErector.Infrastructure/Authentication/JwtTokenGenerator.cs)
**File**: [`DependencyInjection.cs`](../../src/SkFabricatorAndErector.Infrastructure/DependencyInjection.cs)

| Control | Value |
|---|---|
| Algorithm | HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`) |
| Key source | `Jwt__Key` environment variable / appsettings |
| Token expiry | Configurable (default: 1 day) |
| **Clock Skew** | **`TimeSpan.Zero`** — no grace window after expiry |
| Issuer / Audience | Validated on every request |
| Lifetime | `ValidateLifetime = true` |

**Refresh Token Policy:**
- Generated with `RandomNumberGenerator.GetBytes(32)` — cryptographically secure
- Stored in `ApplicationUser.RefreshToken` with `RefreshTokenExpiryTime` (UTC)
- Rotated on every valid refresh request
- Expiry: 7 days (configurable via `Jwt__RefreshTokenExpireDays`)

---

## 2. Authorization & RBAC ✅

**Roles**: `Admin`, `Manager` (seeded via `SeedData`)

| Action | Access |
|---|---|
| Read catalog, photos, sliders | `[AllowAnonymous]` |
| Submit inquiry | `[AllowAnonymous]` |
| List / delete inquiries | `[Authorize(Roles = "Admin,Manager")]` |
| Create / update / delete catalog items | `[Authorize(Roles = "Admin,Manager")]` |
| Upload / delete photos | `[Authorize(Roles = "Admin,Manager")]` |

---

## 3. Rate Limiting ✅ (NEW — Task 11)

**File**: [`RateLimitingExtensions.cs`](../../src/SkFabricatorAndErector.Api/Extensions/RateLimitingExtensions.cs)

| Policy | Limit | Window | Applied To |
|---|---|---|---|
| `auth-fixed` | 10 req | 1 min / IP | `/api/account/login`, `/api/account/refresh-token` |
| `general-fixed` | 300 req | 1 min / IP | All other endpoints |

Rejection status: **HTTP 429 Too Many Requests**

---

## 4. Security Response Headers ✅ (NEW — Task 11)

**File**: [`SecurityHeadersMiddleware.cs`](../../src/SkFabricatorAndErector.Api/Middleware/SecurityHeadersMiddleware.cs)

| Header | Value | Protection |
|---|---|---|
| `X-Content-Type-Options` | `nosniff` | MIME sniffing |
| `X-Frame-Options` | `DENY` | Clickjacking |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Referrer leakage |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` | Feature abuse |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` | HTTPS-only (when TLS active) |
| `X-XSS-Protection` | `1; mode=block` | Legacy XSS filter |

---

## 5. Input Validation & Injection Defenses ✅

**File**: [`Validators/`](../../src/SkFabricatorAndErector.Application/Validators/)

- **FluentValidation** on 6 request types: Login, Inquiry, Project, OurService, TeamMember, ClientDetails
- **EF Core** parameterized LINQ — no raw SQL used anywhere
- **IFormFile uploads** routed through Cloudinary SDK + ImageSharp (no direct filesystem writes)
- **FluentValidation integrated** into ASP.NET model pipeline via `DependencyInjection.cs`

---

## 6. CORS & Preflight ✅

**File**: [`CorsExtensions.cs`](../../src/SkFabricatorAndErector.Api/Extensions/CorsExtensions.cs)

- Explicit origin whitelist loaded from `AllowedOrigins` in appsettings (overridable via env vars)
- `AllowCredentials()` — requires explicit origin, never `*`
- Custom OPTIONS handler in `Program.cs` ensures preflight response always returns headers
- Wildcard origin (`*`) is structurally forbidden by `AllowCredentials()` combination

---

## 7. HTTPS & Transport Security ✅ (NEW — Task 11)

**File**: [`Program.cs`](../../src/SkFabricatorAndErector.Api/Program.cs)

- `UseHttpsRedirection()` enabled in **non-development** environments
- `UseForwardedHeaders` configured for `X-Forwarded-For` and `X-Forwarded-Proto` (required for Render proxy)
- HSTS via `Strict-Transport-Security` header when connection is already HTTPS

---

## 8. Error Handling & Information Leakage ✅

**File**: [`ErrorHandlingMiddleware.cs`](../../src/SkFabricatorAndErector.Api/Middleware/ErrorHandlingMiddleware.cs)

- All unhandled exceptions caught globally
- Returns standardized `ApiResponse` JSON — no stack traces, no internal exception detail
- `NotFoundException` → 404, `BusinessRuleException` → 400, all others → 500
- Auth endpoints return generic `ApiResponse` wrapper — no enumeration hints

---

## 9. Secret Management ✅

All secrets are **placeholder-only** in source. Production deployment injects via environment variables:

| Secret | Env Var Key |
|---|---|
| JWT Signing Key | `Jwt__Key` |
| DB Connection String | `ConnectionStrings__DefaultConnection` |
| Cloudinary Cloud Name | `CloudinarySettings__CloudName` |
| Cloudinary API Key | `CloudinarySettings__ApiKey` |
| Cloudinary API Secret | `CloudinarySettings__ApiSecret` |
| SMTP Username | `SmtpSettings__Username` |
| SMTP Password | `SmtpSettings__Password` |

---

## 10. Known Vulnerabilities

| Package | CVE | Severity | Status |
|---|---|---|---|
| `MailKit` 4.14.0 | GHSA-9j88-vvj5-vhgr | Moderate | ⏳ Awaiting upstream patch from MailKit team |

---

## Recommendations — Production Checklist

- [ ] Set `Jwt__Key` to a 256-bit (32+ char) cryptographically random string
- [ ] Ensure `AllowedOrigins` only contains your actual production Angular frontend URL
- [ ] Upgrade `MailKit` once a patched version is released
- [ ] Enable database connection pooling limits if under high load
- [ ] Configure a WAF (Web Application Firewall) at the Render/cloud level for additional DDoS protection
