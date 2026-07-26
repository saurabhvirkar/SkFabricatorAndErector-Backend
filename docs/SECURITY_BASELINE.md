# Security Baseline Documentation — Phase 0

**Date:** July 26, 2026  
**Repository:** `SkFabricatorAndErector` (Backend & Frontend)

---

## 1. Confirmed Security Baseline (Existing Verified Protections)

- **CORS Configuration**: Explicit allowed-origins list configured (`AllowedOrigins` in `appsettings.json`), no wildcard (`*`).
- **Rate Limiting**: `auth-fixed` (10 requests/min for login & refresh token endpoints) and `general-fixed` (300 requests/min).
- **JWT Bearer Validation**: Validates issuer, audience, token lifetime, signing key, with zero clock skew.
- **Middleware Protections**: Global exception handling middleware (`ErrorHandlingMiddleware`) and security headers middleware (`SecurityHeadersMiddleware`) registered in request pipeline.
- **Role Guards**: `[Authorize(Roles = "Admin,Manager")]` applied to write operations across resource controllers.

---

## 2. Baseline Audit Findings Matrix (To Be Remediated)

| # | Finding | Target Component / File | Severity | Remediation Phase |
|---|---|---|---|---|
| 1 | Plaintext Secrets Committed in Development Config | `SkFabricatorAndErector.Api/appsettings.Development.json` | Critical | Phase 2 |
| 2 | Hardcoded Fallback Seed Passwords (`Admin@123`, `Manager@123`) | `SkFabricatorAndErector.Infrastructure/Persistence/SeedData.cs` | Critical | Phase 3 |
| 3 | Dual Role System (`ApplicationUser.Role` property vs Identity Roles) | `SkFabricatorAndErector.Domain/Entities/ApplicationUser.cs` | High | Phase 3 |
| 4 | Unhashed Refresh Tokens Stored at Rest | `ApplicationUser.RefreshToken` column | High | Phase 4 |
| 5 | Tokens Stored in Browser `localStorage` | `src/core/auth/auth.service.ts` | High | Phase 5 |
| 6 | Long Access Token Lifetime (1 Day) | `appsettings.json` → `Jwt:ExpireDays` | Medium | Phase 4 |
| 7 | Unconditional Swagger UI Enabling | `Program.cs` → `app.UseSwaggerDocumentation()` | Medium | Phase 9 |
| 8 | Missing File Upload Magic-Byte & Extension Validation | `CloudinaryPhotoService.cs` & Controllers | Medium | Phase 8 |
| 9 | Missing Refresh Token Rotation & Reuse Detection | `AuthenticationService.cs` refresh flow | Medium | Phase 4 |

---

## 3. Route Inventories

### Backend API Endpoints:
- `POST /api/account/login` (Anonymous, Rate-limited)
- `POST /api/account/refresh-token` (Anonymous, Rate-limited)
- `GET, POST, PUT, DELETE /api/clientdetails` (Write endpoints guarded by `[Authorize(Roles = "Admin,Manager")]`)
- `GET, POST, DELETE /api/homeslider` (Write endpoints guarded by `[Authorize(Roles = "Admin,Manager")]`)
- `POST /api/inquiry` (Anonymous), `GET, GET/{id}, DELETE /api/inquiry` (Guarded by `[Authorize(Roles = "Admin,Manager")]`)
- `GET, POST, PUT, DELETE /api/ourservices` (Write endpoints guarded by `[Authorize(Roles = "Admin,Manager")]`)
- `GET, GET/about-slider, POST, DELETE /api/photos` (Write endpoints guarded by `[Authorize(Roles = "Admin,Manager")]`)
- `GET, POST, PUT, DELETE /api/projects` (Write endpoints guarded by `[Authorize(Roles = "Admin,Manager")]`)
- `GET, POST, PUT, DELETE /api/teammembers` (Write endpoints guarded by `[Authorize(Roles = "Admin,Manager")]`)

### Frontend Angular Routes:
- `/` (Home)
- `/about` (About Us)
- `/projects` (Projects Showcase)
- `/team` (Team Members)
- `/gallery` (Gallery)
- `/our-services` (Services Catalog)
- `/contact-us` (Contact Form)
- `/clients` (Client Partners)
- `/login` (Admin Login)
- `/ops/adminportal` (Admin Layout Shell — Guarded by `authGuard` for `Admin`, `Manager` roles)
  - `/dashboard`, `/projects`, `/services`, `/team`, `/gallery`, `/clients`, `/sliders`, `/inquiries`, `/users`

---

## 4. Initial Test Suite Baseline

- **Backend Unit & Integration Tests (`dotnet test`)**: **37 / 37 PASSED**
- **Frontend Karma Test Suite (`ng test`)**: **46 / 46 PASSED**
