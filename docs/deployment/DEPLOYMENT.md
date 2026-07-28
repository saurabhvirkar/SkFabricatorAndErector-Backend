# Deployment Guide — SkFabricatorAndErector-Backend

**Last Updated**: 2026-07-24
**Target**: Render.com (Docker Web Service) + PostgreSQL database

---

## Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 8.0+ | Build & test locally |
| Docker Desktop | 24+ | Local containerized run |
| Git | Any | Source control |
| Render account | — | Production hosting |

---

## 1. Local Development (SQLite)

The fastest way to get running locally — no external database needed.

```bash
# 1. Clone
git clone <repo-url>
cd SkFabricatorAndErector-Backend

# 2. Build
dotnet build SkFabricatorAndErector.slnx

# 3. Run tests
dotnet test SkFabricatorAndErector.slnx

# 4. Run API (uses SQLite automatically in Development)
dotnet run --project src/SkFabricatorAndErector.Api
```

Swagger UI: **http://localhost:5000/swagger**
Health probe: **http://localhost:5000/health**

> The `appsettings.Development.json` overrides are picked up automatically when running in Development mode.

---

## 2. Local Docker (API + PostgreSQL)

Runs the full production-like stack locally with a real PostgreSQL container.

```bash
# Copy and fill in secrets
cp docker-compose.yml docker-compose.local.yml
# Edit docker-compose.local.yml and replace all REPLACE_WITH_* values

# Start all services
docker-compose -f docker-compose.local.yml up --build

# Stop
docker-compose down
```

API: **http://localhost:8080**
Health: **http://localhost:8080/health**
Swagger: **http://localhost:8080/swagger**

---

## 3. Production Deployment on Render.com

### Step 1: Create a PostgreSQL database on Render

1. Render Dashboard → **New** → **PostgreSQL**
2. Name: `skfabricator-db`
3. Plan: Free
4. Copy the **Internal Database URL** — you'll use this as `ConnectionStrings__DefaultConnection`

### Step 2: Create the Web Service

1. Render Dashboard → **New** → **Web Service**
2. Connect your GitHub repository
3. Render auto-detects `render.yaml` and sets:
   - **Environment**: Docker
   - **Dockerfile path**: `./Dockerfile.api`
   - **Port**: 8080

### Step 3: Set Secret Environment Variables

In Render Dashboard → Web Service → **Environment**, add these **secret** values:

| Key | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL Internal URL from Step 1 |
| `Jwt__Issuer` | Your API's public URL (e.g. `https://sk-fabricator-api.onrender.com`) |
| `Jwt__Audience` | Your Angular frontend URL (e.g. `https://skfabricator.onrender.com`) |
| `Jwt__Key` | Random 32+ character secret key |
| `CloudinarySettings__CloudName` | From Cloudinary Dashboard |
| `CloudinarySettings__ApiKey` | From Cloudinary Dashboard |
| `CloudinarySettings__ApiSecret` | From Cloudinary Dashboard |
| `SmtpSettings__Host` | SMTP host (e.g. `smtp.gmail.com`) |
| `SmtpSettings__Username` | SMTP username / email |
| `SmtpSettings__Password` | SMTP app password |
| `SmtpSettings__ToEmail` | Recipient for inquiry notifications |
| `AllowedOrigins__0` | Angular frontend URL |

> **Never commit secrets to source control.** All secrets are injected at runtime via environment variables only.

### Step 4: First Deploy — Database Migration

On first deploy, the API automatically runs:
- **PostgreSQL**: `context.Database.MigrateAsync()` (applies EF Core migrations)
- **Seed data**: Creates default `Admin` and `Manager` roles and accounts

> **Seed credentials** are controlled by `SeedUserPasswords` config keys (set in Render Dashboard):
> - `SeedUserPasswords__AdminPassword`
> - `SeedUserPasswords__ManagerPassword`

### Step 5: Verify Deployment

```bash
# Health probe (should return 200 OK)
curl https://sk-fabricator-api.onrender.com/health

# Swagger UI
open https://sk-fabricator-api.onrender.com/swagger
```

---

## 4. CI/CD Pipeline (GitHub Actions)

Two workflows are configured in `.github/workflows/`:

| Workflow | File | Trigger | Actions |
|---|---|---|---|
| **CI** | `ci.yml` | Push / PR to `main`, `develop` | Restore → Build → Unit Tests → Integration Tests |
| **CD** | `cd.yml` | Push to `main` | Build → All Tests → Trigger Render Deploy Hook |

### Configuring the CD Deploy Hook

1. Render Dashboard → Web Service → **Settings** → **Deploy Hook** → Copy URL
2. GitHub Repository → **Settings** → **Secrets and variables** → **Actions**
3. Add secret: `RENDER_DEPLOY_HOOK_URL` = the copied Render URL

Every merge to `main` will then automatically:
- Build and test the solution
- Trigger a zero-downtime Render deploy

---

## 5. File Reference

| File | Purpose |
|---|---|
| [`Dockerfile.api`](../../Dockerfile.api) | Multi-stage Docker build; non-root user; HEALTHCHECK |
| [`docker-compose.yml`](../../docker-compose.yml) | Local dev stack: API + PostgreSQL |
| [`render.yaml`](../../render.yaml) | Render Infrastructure-as-Code |
| [`.dockerignore`](../../.dockerignore) | Excludes tests/docs from Docker build context |
| [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) | GitHub Actions CI |
| [`.github/workflows/cd.yml`](../../.github/workflows/cd.yml) | GitHub Actions CD → Render |
