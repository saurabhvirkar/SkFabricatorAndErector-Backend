using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Services;

namespace SkFabricatorAndErector.Infrastructure.Logging;

public class SecurityAuditLogger(ILogger<SecurityAuditLogger> logger, IHttpContextAccessor httpContextAccessor) : ISecurityAuditLogger
{
    private readonly ILogger<SecurityAuditLogger> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public void LogAuditEvent(string actor, string action, string entity, string entityId, string? ipAddress = null, bool isSuccess = true, string? details = null)
    {
        var resolvedIp = string.IsNullOrWhiteSpace(ipAddress)
            ? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "UNKNOWN_IP"
            : ipAddress;

        var status = isSuccess ? "SUCCESS" : "FAILURE";

        _logger.LogInformation(
            "[SECURITY AUDIT] Status: {Status} | Actor: {Actor} | Action: {Action} | Entity: {Entity} | EntityId: {EntityId} | IP: {IP} | Details: {Details} | Timestamp: {Timestamp}",
            status, actor, action, entity, entityId, resolvedIp, details ?? "N/A", DateTime.UtcNow);
    }
}
