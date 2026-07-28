# Database Audit & Neon PostgreSQL Strategy

## Overview
This document evaluates the database architecture, schema migrations, and deployment protocol using Neon PostgreSQL Free Tier.

---

## Current Database Configuration Audit

- **Development Provider**: SQLite (`skfabricator.db`) / PostgreSQL
- **Production Provider Target**: Neon Serverless PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **ORM / Migration Tool**: Entity Framework Core 10.0 (`Microsoft.EntityFrameworkCore`)

---

## Neon PostgreSQL Free Tier Architecture

```
                       +-------------------------------+
                       |       NEON POSTGRESQL         |
                       |    (Serverless Free Tier)     |
                       +---------------+---------------+
                                       |
                +----------------------+----------------------+
                |                                             |
                v                                             v
     +--------------------+                        +--------------------+
     |    QA DATABASE     |                        | PRODUCTION DATABASE|
     | skfabricator_qa    |                        | skfabricator_prod  |
     +--------------------+                        +--------------------+
```

### Environment Isolation Rules
1. **Strict Database Separation**: QA and Production MUST use distinct database instances or isolated projects on Neon. QA code must never connect to the Production database.
2. **Connection Pooling**: Serverless PostgreSQL requires pooled connection strings (`-pooler` endpoint on Neon) for optimal serverless connection handling.
3. **SSL / TLS Required**: All connections to Neon must enforce `SslMode=Require`.

---

## EF Core Migration Strategy & Pipeline Integration

```
  Developer Code Change
           │
           ▼
  EF Core Migration Generated (`dotnet ef migrations add <Name>`)
           │
           ▼
  PR Verification & Dry-Run Validation (`dotnet ef migrations script`)
           │
           ▼
  Deployment to QA -> Migration Applied Automatically (`Database.MigrateAsync()`)
           │
           ▼
  QA Integration & E2E Testing Verification
           │
           ▼
  Manual Production Approval Gate
           │
           ▼
  Deployment to Production -> Automated Database Backup Check & Migration Execution
```

---

## Backup & Recovery Protocols

1. **Automated Schema & Data Backups**:
   - Utilize Neon point-in-time recovery / branching capabilities.
   - Run daily `pg_dump` backup scripts stored securely outside the VM.
2. **Migration Rollback Plan**:
   - Every EF Core migration must support idempotent applying and backward compatibility.
   - Database schema changes (e.g., column additions) should be non-breaking to allow concurrent zero-downtime application deployments.
