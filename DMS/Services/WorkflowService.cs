using System.Security.Cryptography;
using DMS.Data;
using DMS.Models;
using DMS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DMS.Services;

public class WorkflowService : IWorkflowService
{
    private readonly DmsDbContext _db;
    private readonly DmsSettings _settings;

    public WorkflowService(DmsDbContext db, IOptions<DmsSettings> options)
    {
        _db = db;
        _settings = options.Value;
    }

    // ---- Review ----
    public async Task<List<RevisionItem>> GetPendingReviewsAsync()
    {
        return await _db.Revisions
            .Include(r => r.Document)
            .Include(r => r.Uploader)
            .Where(r => r.Status == RevisionStatus.Pending)
            .OrderBy(r => r.UploadedAt)
            .Select(r => new RevisionItem
            {
                RevisionID = r.RevisionID,
                DocumentID = r.DocumentID,
                DocumentName = r.Document!.DocumentName,
                SerialNumber = r.Document.SerialNumber,
                Plant = r.Document.Plant,
                RevisionNumber = r.RevisionNumber,
                Status = r.Status,
                UploadedByName = r.Uploader!.EmployeeName,
                UploadedAt = r.UploadedAt,
                FileName = r.FileName
            })
            .ToListAsync();
    }

    public async Task ReviewRevisionAsync(long revisionId, long userId, string decision, string? remarks)
    {
        var revision = await _db.Revisions.FindAsync(revisionId)
            ?? throw new InvalidOperationException("Revision not found.");

        if (revision.Status != RevisionStatus.Pending)
            throw new InvalidOperationException("Revision is not in Pending status.");

        revision.Status = decision == "Approved" ? RevisionStatus.Review : RevisionStatus.Rejected;

        var approval = new DmsApproval
        {
            DocumentID = revision.DocumentID,
            RevisionID = revisionId,
            WorkflowType = WorkflowTypes.Review,
            Decision = decision,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow,
            Remarks = remarks
        };
        _db.Approvals.Add(approval);
        await _db.SaveChangesAsync();
    }

    // ---- Approval ----
    public async Task<List<RevisionItem>> GetPendingApprovalsAsync()
    {
        return await _db.Revisions
            .Include(r => r.Document)
            .Include(r => r.Uploader)
            .Where(r => r.Status == RevisionStatus.Review)
            .OrderBy(r => r.UploadedAt)
            .Select(r => new RevisionItem
            {
                RevisionID = r.RevisionID,
                DocumentID = r.DocumentID,
                DocumentName = r.Document!.DocumentName,
                SerialNumber = r.Document.SerialNumber,
                Plant = r.Document.Plant,
                RevisionNumber = r.RevisionNumber,
                Status = r.Status,
                UploadedByName = r.Uploader!.EmployeeName,
                UploadedAt = r.UploadedAt,
                FileName = r.FileName
            })
            .ToListAsync();
    }

    public async Task ApproveRevisionAsync(long revisionId, long userId, string decision, string? remarks, DateTime reauthAt)
    {
        var revision = await _db.Revisions.FindAsync(revisionId)
            ?? throw new InvalidOperationException("Revision not found.");

        if (revision.Status != RevisionStatus.Review)
            throw new InvalidOperationException("Revision is not in Review status.");

        revision.Status = decision == "Approved" ? "Approved" : RevisionStatus.Rejected;
        if (decision == "Approved")
        {
            var storageBase = _settings.GetEffectiveStoragePath();
            await ArchiveObsoleteRevisionAsync(revision.DocumentID, revisionId, storageBase);

            revision.EffectiveAt = DateTime.UtcNow;
            revision.Status = RevisionStatus.Active;

            // Update document's current revision
            var document = await _db.Documents.FindAsync(revision.DocumentID);
            if (document != null)
            {
                document.CurrentRevisionID = revisionId;
            }
        }

        var approval = new DmsApproval
        {
            DocumentID = revision.DocumentID,
            RevisionID = revisionId,
            WorkflowType = WorkflowTypes.DocumentApproval,
            Decision = decision,
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow,
            ReauthenticatedAt = reauthAt,
            Remarks = remarks
        };
        _db.Approvals.Add(approval);
        await _db.SaveChangesAsync();
    }

    // ---- Access Requests ----
    public async Task<DmsRequest> CreateAccessRequestAsync(long documentId, long userId, string? remarks)
    {
        var document = await _db.Documents.Include(d => d.CurrentRevision)
            .FirstOrDefaultAsync(d => d.DocumentID == documentId)
            ?? throw new InvalidOperationException("Document not found.");

        if (document.CurrentRevisionID == null || document.CurrentRevision?.Status != RevisionStatus.Active)
            throw new InvalidOperationException("Document has no active revision.");

        // Check for existing pending request
        var existingPending = await _db.Requests
            .AnyAsync(r => r.DocumentID == documentId && r.RequestedBy == userId
                && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Approved));
        if (existingPending)
            throw new InvalidOperationException("You already have a pending or approved request for this document.");

        var request = new DmsRequest
        {
            DocumentID = documentId,
            RevisionID = document.CurrentRevisionID.Value,
            RequestedBy = userId,
            RequestedAt = DateTime.UtcNow,
            Status = RequestStatus.Pending,
            Remarks = remarks
        };
        _db.Requests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<List<AccessRequestItem>> GetPendingAccessRequestsAsync()
    {
        return await _db.Requests
            .Include(r => r.Document)
            .Include(r => r.Revision)
            .Include(r => r.Requester)
            .Where(r => r.Status == RequestStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .Select(r => new AccessRequestItem
            {
                RequestID = r.RequestID,
                DocumentID = r.DocumentID,
                DocumentName = r.Document!.DocumentName,
                SerialNumber = r.Document.SerialNumber,
                Plant = r.Document.Plant,
                RevisionNumber = r.Revision!.RevisionNumber,
                Status = r.Status,
                RequestedByName = r.Requester!.EmployeeName,
                RequestedAt = r.RequestedAt,
                Remarks = r.Remarks
            })
            .ToListAsync();
    }

    public async Task<List<AccessRequestItem>> GetMyAccessRequestsAsync(long userId)
    {
        return await _db.Requests
            .Include(r => r.Document)
            .Include(r => r.Revision)
            .Where(r => r.RequestedBy == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new AccessRequestItem
            {
                RequestID = r.RequestID,
                DocumentID = r.DocumentID,
                DocumentName = r.Document!.DocumentName,
                SerialNumber = r.Document.SerialNumber,
                Plant = r.Document.Plant,
                RevisionNumber = r.Revision!.RevisionNumber,
                Status = r.Status,
                RequestedByName = "",
                RequestedAt = r.RequestedAt,
                Remarks = r.Remarks,
                ViewExpiresAt = r.ViewExpiresAt
            })
            .ToListAsync();
    }

    public async Task ApproveAccessRequestAsync(long requestId, long qaUserId, string decision, string? remarks, DateTime reauthAt)
    {
        var request = await _db.Requests.FindAsync(requestId)
            ?? throw new InvalidOperationException("Request not found.");

        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Request is not pending.");

        request.Status = decision == "Approved" ? RequestStatus.Approved : RequestStatus.Rejected;
        request.ApprovedBy = qaUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.Remarks = remarks;

        var approval = new DmsApproval
        {
            DocumentID = request.DocumentID,
            RevisionID = request.RevisionID,
            RequestID = requestId,
            WorkflowType = WorkflowTypes.AccessApproval,
            Decision = decision,
            PerformedBy = qaUserId,
            PerformedAt = DateTime.UtcNow,
            ReauthenticatedAt = reauthAt,
            Remarks = remarks
        };
        _db.Approvals.Add(approval);
        await _db.SaveChangesAsync();
    }

    // ---- Viewing ----
    public async Task<(string? token, DateTime? expiresAt)> StartViewingSessionAsync(long requestId, long userId)
    {
        var request = await _db.Requests.FindAsync(requestId);
        if (request == null || request.RequestedBy != userId || request.Status != RequestStatus.Approved)
            return (null, null);

        // Check if revision is still active
        var revision = await _db.Revisions.FindAsync(request.RevisionID);
        if (revision == null || revision.Status != RevisionStatus.Active)
            return (null, null);

        // Generate secure token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes);
        var tokenHash = ComputeTokenHash(token);

        request.Status = RequestStatus.Viewing;
        request.ViewTokenHash = tokenHash;
        request.ViewStartedAt = DateTime.UtcNow;
        request.ViewExpiresAt = DateTime.UtcNow.AddMinutes(_settings.ViewingSessionMinutes);
        await _db.SaveChangesAsync();

        return (token, request.ViewExpiresAt);
    }

    public async Task<DmsRequest?> ValidateViewingSessionAsync(string tokenHash, long userId)
    {
        var request = await _db.Requests
            .Include(r => r.Revision)
            .FirstOrDefaultAsync(r =>
                r.ViewTokenHash == tokenHash &&
                r.RequestedBy == userId &&
                r.Status == RequestStatus.Viewing &&
                r.ViewExpiresAt > DateTime.UtcNow);

        if (request == null) return null;

        // Also verify revision is still active
        if (request.Revision?.Status != RevisionStatus.Active)
        {
            request.Status = RequestStatus.Expired;
            await _db.SaveChangesAsync();
            return null;
        }

        return request;
    }

    // ---- Issue ----
    public async Task<List<RevisionItem>> GetPendingIssuanceAsync()
    {
        return await _db.Revisions
            .Include(r => r.Document)
            .Include(r => r.Uploader)
            .Where(r => r.Status == RevisionStatus.Active)
            .Where(r => !_db.Approvals.Any(a => a.RevisionID == r.RevisionID && a.WorkflowType == WorkflowTypes.Issue))
            .OrderBy(r => r.EffectiveAt)
            .Select(r => new RevisionItem
            {
                RevisionID = r.RevisionID,
                DocumentID = r.DocumentID,
                DocumentName = r.Document!.DocumentName,
                SerialNumber = r.Document.SerialNumber,
                Plant = r.Document.Plant,
                RevisionNumber = r.RevisionNumber,
                Status = r.Status,
                UploadedByName = r.Uploader!.EmployeeName,
                UploadedAt = r.UploadedAt,
                FileName = r.FileName
            })
            .ToListAsync();
    }

    public async Task IssueRevisionAsync(long revisionId, long userId, string? remarks, DateTime reauthAt)
    {
        var revision = await _db.Revisions.FindAsync(revisionId)
            ?? throw new InvalidOperationException("Revision not found.");

        if (revision.Status != RevisionStatus.Active)
            throw new InvalidOperationException("Only active revisions can be issued.");

        var approval = new DmsApproval
        {
            DocumentID = revision.DocumentID,
            RevisionID = revisionId,
            WorkflowType = WorkflowTypes.Issue,
            Decision = "Issued",
            PerformedBy = userId,
            PerformedAt = DateTime.UtcNow,
            ReauthenticatedAt = reauthAt,
            Remarks = remarks
        };
        _db.Approvals.Add(approval);
        await _db.SaveChangesAsync();
    }

    // ---- Obsolescence ----
    public async Task ArchiveObsoleteRevisionAsync(long documentId, long newRevisionId, string archiveBasePath)
    {
        var document = await _db.Documents
            .Include(d => d.CurrentRevision)
            .FirstOrDefaultAsync(d => d.DocumentID == documentId);

        if (document?.CurrentRevisionID == null || document.CurrentRevisionID == newRevisionId)
            return;

        var oldRevision = await _db.Revisions.FindAsync(document.CurrentRevisionID);
        if (oldRevision == null || oldRevision.Status == RevisionStatus.Obsolete)
            return;

        // Archive path: Obsolete/Plant/SerialNumber/Revision/
        var archivePath = Path.Combine(archiveBasePath, "Obsolete",
            document.Plant, document.SerialNumber, oldRevision.RevisionNumber);
        Directory.CreateDirectory(archivePath);

        var sourceFile = oldRevision.FileLocation;
        if (File.Exists(sourceFile))
        {
            var destFile = Path.Combine(archivePath, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destFile, overwrite: true);

            // Verify checksum
            using var stream = File.OpenRead(destFile);
            using var sha256 = SHA256.Create();
            var hash = Convert.ToHexStringLower(sha256.ComputeHash(stream));
            if (hash != oldRevision.FileHash)
                throw new InvalidOperationException("Archive checksum mismatch — aborting obsolescence.");
        }

        // Invalidate grants for old revision
        var activeRequests = await _db.Requests
            .Where(r => r.RevisionID == oldRevision.RevisionID
                && (r.Status == RequestStatus.Approved || r.Status == RequestStatus.Viewing))
            .ToListAsync();
        foreach (var req in activeRequests)
        {
            req.Status = RequestStatus.Expired;
        }

        oldRevision.Status = RevisionStatus.Obsolete;
        oldRevision.ObsoleteAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static string ComputeTokenHash(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
