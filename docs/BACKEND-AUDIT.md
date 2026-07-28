# Backend Architecture Audit & API Design Review

## Executive Summary
This document reviews the ASP.NET Core API codebase (`SkFabricatorAndErector-Backend`), API endpoints, security middleware, exception handling, and health check monitoring.

---

## Technical Baseline

- **Framework**: .NET 10.0 (`net10.0`) ASP.NET Core Web API
- **ORM**: Entity Framework Core 10.0
- **Authentication**: JWT Bearer Tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **API Documentation**: Microsoft.AspNetCore.OpenApi, Scalar OpenAPI UI
- **File Storage**: CloudinaryDotNet
- **Email Service**: MailKit / MimeKit

---

## Key Backend Enhancements & Best Practices

1. **Health Check Probes**:
   - Implement ASP.NET Core Health Checks exposing `/health`, `/health/live`, `/health/ready`.
   - `/health/live`: Liveness check (returns 200 OK if service process is running).
   - `/health/ready`: Readiness check (verifies Neon database connectivity and Cloudinary API status without exposing connection strings).

2. **Global Exception Handling & Problem Details**:
   - Standardize error responses using RFC 7807 `ProblemDetails`.
   - Prevent internal trace leakage in non-development environments.

3. **CORS & Rate Limiting Policy**:
   - Restrict CORS origins strictly per environment (`appsettings.QA.json` and `appsettings.Production.json`).
   - Implement IP rate limiting middleware to prevent abuse on public endpoints (e.g., authentication, file uploads).

4. **Cloud-Agnostic Storage Layer**:
   - Enforce `IFileStorageService` abstraction across domain & application services.
   - Offload all file uploads directly to Cloudinary (folders: `skfabricator/qa/` and `skfabricator/production/`).
