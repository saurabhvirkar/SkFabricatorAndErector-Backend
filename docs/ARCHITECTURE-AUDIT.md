# Architecture Audit & Refactoring Assessment

## Executive Summary
This document provides an architectural audit for the **SK Fabricator & Erector** application suite (Backend: ASP.NET Core .NET 10 Clean Architecture; Frontend: Angular 21). The objective is to evaluate cloud portability, cloud provider decoupling ($0-cost stack targeting Oracle Cloud Always Free VM, Cloudflare Pages, Neon PostgreSQL, and Cloudinary), layer boundary isolation, and readiness for future migration to AWS or Azure.

---

## Current Architecture Assessment

```
                      +-----------------------------+
                      |      CLOUDFLARE DNS / CDN   |
                      +--------------+--------------+
                                     |
             +-----------------------+-----------------------+
             |                                               |
             v                                               v
+------------------------+                       +-----------------------+
|    CLOUDFLARE PAGES    |                       |    ORACLE CLOUD VM    |
|   Angular 21 Frontend  |                       | Always Free (Ubuntu)  |
|  (QA & Production)     |                       |  Docker + Nginx Proxy |
+------------------------+                       +-----------+-----------+
                                                             |
                                                             v
                                                 +-----------------------+
                                                 |   ASP.NET Core API    |
                                                 |     (.NET 10.0)       |
                                                 +-----------+-----------+
                                                             |
                                  +--------------------------+--------------------------+
                                  |                                                     |
                                  v                                                     v
                       +--------------------+                                +--------------------+
                       |   NEON POSTGRESQL  |                                |     CLOUDINARY     |
                       | Serverless Database|                                | Managed Image/File |
                       | (QA & Production)  |                                |  Storage Service   |
                       +--------------------+                                +--------------------+
```

### Clean Architecture Layer Boundaries
1. **Domain Layer (`SkFabricatorAndErector.Domain`)**:
   - Contains core entities, value objects, domain events, and repository interfaces.
   - **Status**: Pure C# logic with zero external infrastructure or framework dependencies.

2. **Application Layer (`SkFabricatorAndErector.Application`)**:
   - Contains CQRS handlers, application DTOs, interface abstractions (`IFileStorageService`, `IEmailService`, `IApplicationDbContext`).
   - **Status**: Clean decoupling intact. Interfaces allow seamless swapping of infrastructure providers.

3. **Infrastructure Layer (`SkFabricatorAndErector.Infrastructure`)**:
   - Contains EF Core DbContext implementations (Npgsql / SQLite), Cloudinary integration, MailKit SMTP, JWT Token Generation.
   - **Status**: Well-isolated. All provider-specific code is confined here.

4. **API Layer (`SkFabricatorAndErector.Api`)**:
   - Controller endpoints, middleware, dependency injection configuration, OpenAPI / Scalar UI.
   - **Status**: Clean. No direct provider locks.

---

## Key Refactoring & Portability Recommendations

1. **Storage Provider Decoupling**:
   - Ensure `IFileStorageService` strictly exposes domain-oriented operations (`UploadAsync`, `DeleteAsync`, `GetUrl`).
   - Implement `CloudinaryFileStorageService` in Infrastructure.
   - Future transition to `AwsS3FileStorageService` or `AzureBlobStorageService` requires only creating a new class implementing `IFileStorageService` and updating DI registration.

2. **Database Provider Abstraction**:
   - Application depends on EF Core `IApplicationDbContext` and standard SQL dialect supported by PostgreSQL.
   - Production database target: Neon PostgreSQL (serverless free tier). Development: SQLite / PostgreSQL.

3. **Zero-Lock VM Execution**:
   - Application runs in containerized Docker environment.
   - Host OS (Ubuntu on Oracle VM, AWS EC2, or Azure VM) acts strictly as a container runner.
