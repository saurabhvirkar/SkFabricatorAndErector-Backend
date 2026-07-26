namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface ISecurityAuditLogger
{
    void LogAuditEvent(string actor, string action, string entity, string entityId, string? ipAddress = null, bool isSuccess = true, string? details = null);
}
