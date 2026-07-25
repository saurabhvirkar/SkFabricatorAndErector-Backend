# Dependency Rules

1. **Domain Project**:
   - MUST NOT depend on any other project.
   - MUST NOT depend on EF Core, ASP.NET Core MVC, or third-party infrastructure SDKs.

2. **Application Project**:
   - MAY depend on `Domain`.
   - Contains all Request/Response Contract DTOs in `SkFabricatorAndErector.Application.Contracts`.
   - MUST NOT depend on `Infrastructure` or `Api`.

3. **Infrastructure Project**:
   - MAY depend on `Application` and `Domain`.
   - MUST NOT depend on `Api`.

4. **Api Project**:
   - MAY depend on `Application` and `Infrastructure`.
   - Direct usage of `Infrastructure` classes inside Controllers is FORBIDDEN except in Dependency Injection configuration (`Program.cs`).
