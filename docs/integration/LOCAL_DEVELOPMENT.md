# End-to-End Local Development & Integration Guide

This guide documents how to run **SkFabricatorAndErector-Backend** and **SkFabricatorAndErector-Frontend** together locally.

---

## 🏛️ Local Runtime Architecture

```
Browser
   │
   │ http://localhost:4200 (Angular UI)
   ▼
Frontend Dev Server
   │
   │ /api HTTP Proxy -> http://localhost:5229
   ▼
SkFabricatorAndErector.Api (ASP.NET Core 8)
   │
   ▼
PostgreSQL / SQLite Database
```

---

## 📍 Configured Local Ports

| Service | Protocol / Path | Local URL |
|---|---|---|
| **Frontend UI** | HTTP | `http://localhost:4200` |
| **Backend API (HTTP)** | HTTP | `http://localhost:5229` |
| **Backend API (HTTPS)** | HTTPS | `https://localhost:7163` |
| **Swagger UI** | HTTP/HTTPS | `http://localhost:5229/swagger` |
| **Health Probe** | HTTP/HTTPS | `http://localhost:5229/health` |

---

## 🛠️ Step-by-Step Local Run Instructions

### 1. Terminal 1: Start Backend API

```bash
cd SkFabricatorAndErector-Backend

# Verify build and test suite
dotnet restore
dotnet build
dotnet test SkFabricatorAndErector.slnx

# Launch local backend API with hot reload
dotnet watch run --project src/SkFabricatorAndErector.Api/SkFabricatorAndErector.Api.csproj
```

**Verification**:
- Health probe: `GET http://localhost:5229/health` → `{"status":"Healthy"}`
- Swagger documentation: Open `http://localhost:5229/swagger`

---

### 2. Terminal 2: Start Frontend Application

```bash
cd SkFabricatorAndErector-Frontend

# Install dependencies (first time only)
npm install

# Run karma unit test suite (40 tests)
npm test -- --watch=false --browsers=ChromeHeadless

# Launch local Angular development server
npm start
```

**Verification**:
- Open browser: `http://localhost:4200`
- Open Browser DevTools Network tab: Verify requests to `/api/*` are proxied to `http://localhost:5229/api/*`.
