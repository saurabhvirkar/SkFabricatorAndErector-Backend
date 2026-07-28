# --- Stage 1: Build & Publish ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/SkFabricatorAndErector.Api/SkFabricatorAndErector.Api.csproj", "src/SkFabricatorAndErector.Api/"]
COPY ["src/SkFabricatorAndErector.Application/SkFabricatorAndErector.Application.csproj", "src/SkFabricatorAndErector.Application/"]
COPY ["src/SkFabricatorAndErector.Domain/SkFabricatorAndErector.Domain.csproj", "src/SkFabricatorAndErector.Domain/"]
COPY ["src/SkFabricatorAndErector.Infrastructure/SkFabricatorAndErector.Infrastructure.csproj", "src/SkFabricatorAndErector.Infrastructure/"]

RUN dotnet restore "src/SkFabricatorAndErector.Api/SkFabricatorAndErector.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/SkFabricatorAndErector.Api"
RUN dotnet build "SkFabricatorAndErector.Api.csproj" -c Release -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "SkFabricatorAndErector.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Stage 2: Runtime Image ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Security: Create non-root user
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SkFabricatorAndErector.Api.dll"]
