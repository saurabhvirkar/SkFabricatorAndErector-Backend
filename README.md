# SK Fabricator & Erector — Backend API Service

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Build](https://github.com/your-org/SkFabricatorAndErector-Backend/actions/workflows/ci.yml/badge.svg)](https://github.com/your-org/SkFabricatorAndErector-Backend/actions/workflows/ci.yml)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen.svg)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

An enterprise-grade ASP.NET Core 8 Web API built for **SK Fabricator & Erector**. Replaces the legacy monolithic API with Clean Architecture, explicit domain boundaries, FluentValidation, EF Core dual-provider persistence (SQLite / PostgreSQL), security hardening, and containerized deployment to Render.com.

---

## 🏛️ Architecture

```
┌──────────────────────────────────────────────┐
│              API Layer (Controllers,          │
│              Middleware, Filters, Swagger)     │
└───────────────────────┬──────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────┐
│           Application Layer                  │
│    (Use Cases · Interfaces · DTOs ·          │
│     Contracts · FluentValidation)            │
└──────────┬───────────────────────▲───────────┘
           │                       │
           ▼                       │
┌─────────────────┐    ┌───────────┴──────────┐
│  Domain Layer   │◄───│  Infrastructure Layer│
│  (Entities,     │    │  (EF Core · Identity │
│   no deps)      │    │   JWT · Cloudinary   │
│                 │    │   MailKit)           │
└─────────────────┘    └──────────────────────┘
```

| Project | Responsibility |
|---|---|
| `SkFabricatorAndErector.Domain` | 8 pure entity classes — zero framework dependencies |
| `SkFabricatorAndErector.Application` | Service interfaces, use-case logic, DTOs, FluentValidation |
| `SkFabricatorAndErector.Infrastructure` | EF Core, Identity, JWT, Cloudinary, MailKit |
| `SkFabricatorAndErector.Api` | Controllers, middleware pipeline, CORS, Swagger |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(optional)*

### Local Development

```bash
# Clone
git clone <repo-url>
cd SkFabricatorAndErector-Backend

# Build
dotnet build SkFabricatorAndErector.slnx

# Test
dotnet test SkFabricatorAndErector.slnx

# Run (SQLite, no external services needed)
dotnet run --project src/SkFabricatorAndErector.Api
```

- Swagger UI: `http://localhost:5000/swagger`
- Health probe: `http://localhost:5000/health`

### Docker (API + PostgreSQL)

```bash
docker-compose up --build
```

- API: `http://localhost:8080`
- Health: `http://localhost:8080/health`

---

## 🧪 Testing

37 automated tests — 100% pass rate.

| Suite | Count | What's tested |
|---|---|---|
| Unit | 22 | Services, validators, JWT token generation |
| Integration | 15 | Controllers, route attributes, HTTP verbs, response shapes |

```bash
dotnet test SkFabricatorAndErector.slnx --logger "console;verbosity=minimal"
```

---

## 🌐 API Endpoints

| Module | Public | Protected (Admin / Manager) |
|---|---|---|
| **Account** | `POST /api/account/login` · `POST /api/account/refresh-token` | — |
| **Inquiries** | `POST /api/inquiry` | `GET /api/inquiry` · `GET /api/inquiry/{id}` · `DELETE /api/inquiry/{id}` |
| **Photos** | `GET /api/photos` · `GET /api/photos/about-slider` | `POST /api/photos` · `DELETE /api/photos/{id}` · `DELETE /api/photos/about-slider/{id}` |
| **Home Slider** | `GET /api/homeslider` | `POST /api/homeslider` · `DELETE /api/homeslider/{id}` |
| **Projects** | `GET /api/project` · `GET /api/project/{id}` · `GET /api/project/category/{c}` | `POST` · `PUT /api/project/{id}` · `DELETE /api/project/{id}` |
| **Our Services** | `GET /api/ourservices` · `GET /api/ourservices/{id}` | `POST` · `PUT /api/ourservices/{id}` · `DELETE /api/ourservices/{id}` |
| **Team Members** | `GET /api/teammembers` · `GET /api/teammembers/{id}` | `POST` · `PUT /api/teammembers/{id}` · `DELETE /api/teammembers/{id}` |
| **Client Details** | `GET /api/clientdetails` · `GET /api/clientdetails/{id}` | `POST` · `PUT /api/clientdetails/{id}` · `DELETE /api/clientdetails/{id}` |
| **Health** | `GET /health` | — |

---

## 🔒 Security (Task 11 Hardening)

| Control | Detail |
|---|---|
| **JWT** | HMAC-SHA256 · `ClockSkew = Zero` · 1-day access / 7-day refresh rotation |
| **Rate Limiting** | 10 req/min on auth endpoints · 429 Too Many Requests on breach |
| **Security Headers** | `X-Content-Type-Options` · `X-Frame-Options: DENY` · `Referrer-Policy` · `HSTS` |
| **CORS** | Explicit origin whitelist · `AllowCredentials()` — no wildcard |
| **RBAC** | `Admin` / `Manager` roles via ASP.NET Core Identity |
| **FluentValidation** | 6 validator classes on all mutating request DTOs |
| **Error Handling** | Global middleware — no stack traces in production |
| **Secrets** | All secrets via environment variables — zero hardcoded values in source |

See [`docs/security/SECURITY_REVIEW.md`](docs/security/SECURITY_REVIEW.md) for the full audit.

---

## 🐳 Deployment

See [`docs/deployment/DEPLOYMENT.md`](docs/deployment/DEPLOYMENT.md) for the full guide.

**Quick Render deploy:**
1. Connect repo → Render auto-detects `render.yaml` + `Dockerfile.api`
2. Set secret env vars in Render Dashboard
3. Copy the Render Deploy Hook URL → add as `RENDER_DEPLOY_HOOK_URL` GitHub Secret
4. Every push to `main` → CI tests + auto-deploy via `cd.yml`

---

## 📁 Documentation

| Document | Path |
|---|---|
| Architecture overview | [`docs/architecture/ARCHITECTURE.md`](docs/architecture/ARCHITECTURE.md) |
| Dependency rules | [`docs/architecture/DEPENDENCY_RULES.md`](docs/architecture/DEPENDENCY_RULES.md) |
| Security audit | [`docs/security/SECURITY_REVIEW.md`](docs/security/SECURITY_REVIEW.md) |
| API compatibility report | [`docs/verification/API_COMPATIBILITY_REPORT.md`](docs/verification/API_COMPATIBILITY_REPORT.md) |
| Deployment guide | [`docs/deployment/DEPLOYMENT.md`](docs/deployment/DEPLOYMENT.md) |

---

## 📄 License
MIT License
