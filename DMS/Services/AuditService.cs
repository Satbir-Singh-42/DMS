using DMS.Data;
using DMS.Models;

namespace DMS.Services;

public class AuditService : IAuditService
{
    private readonly DmsDbContext _db;

    public AuditService(DmsDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(long? userId, string actionType, long? documentId, long? requestId, string? ipAddress, string? details)
    {
        var log = new DmsAuditLog
        {
            UserID = userId,
            ActionType = actionType,
            DocumentID = documentId,
            RequestID = requestId,
            ActionAt = DateTime.UtcNow,
            IPAddress = ipAddress?.Length > 45 ? ipAddress[..45] : ipAddress,
            Details = details
        };
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
