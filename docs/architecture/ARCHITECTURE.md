# Clean Architecture Design & Principles

## Overview
This repository contains the backend service for SK Fabricator & Erector, restructured according to Microsoft's Clean Architecture recommendations.

## Layers

```
Domain  <--  Application (includes DTOs/Contracts)  <--  Infrastructure
                         ^
                         |
                        API
```

1. **Domain**: Zero external dependencies (except core Identity stores abstractions). Core business concepts and entities.
2. **Application**: Business workflows, use cases, API DTO request/response contracts, and interface definitions.
3. **Infrastructure**: Implementations of storage, email, identity, media services, and database access.
4. **Api**: Presentation, ASP.NET Core controllers, middleware, and dependency injection wiring.
