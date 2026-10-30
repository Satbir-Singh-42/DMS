using DMS.Models;
using DMS.Models.ViewModels;

namespace DMS.Services;

public interface IWorkflowService
{
    // Review
    Task<List<RevisionItem>> GetPendingReviewsAsync();
    Task ReviewRevisionAsync(long revisionId, long userId, string decision, string? remarks);

    // Approval
    Task<List<RevisionItem>> GetPendingApprovalsAsync();
    Task ApproveRevisionAsync(long revisionId, long userId, string decision, string? remarks, DateTime reauthAt);

    // Access Requests
    Task<DmsRequest> CreateAccessRequestAsync(long documentId, long userId, string? remarks);
    Task<List<AccessRequestItem>> GetPendingAccessRequestsAsync();
    Task<List<AccessRequestItem>> GetMyAccessRequestsAsync(long userId);
    Task ApproveAccessRequestAsync(long requestId, long qaUserId, string decision, string? remarks, DateTime reauthAt);

    // Viewing
    Task<(string? token, DateTime? expiresAt)> StartViewingSessionAsync(long requestId, long userId);
    Task<DmsRequest?> ValidateViewingSessionAsync(string tokenHash, long userId);

    // Issue
    Task<List<RevisionItem>> GetPendingIssuanceAsync();
    Task IssueRevisionAsync(long revisionId, long userId, string? remarks, DateTime reauthAt);

    // Obsolescence
    Task ArchiveObsoleteRevisionAsync(long documentId, long newRevisionId, string archiveBasePath);
}
