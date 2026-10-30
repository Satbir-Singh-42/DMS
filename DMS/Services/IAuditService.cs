namespace DMS.Services;

public interface IAuditService
{
    Task LogAsync(long? userId, string actionType, long? documentId, long? requestId, string? ipAddress, string? details);
}
