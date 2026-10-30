namespace DMS.Models.ViewModels;

public class AccessRequestListViewModel
{
    // For Plant user
    public List<AccessRequestItem> MyRequests { get; set; } = new();
    // For QA
    public List<AccessRequestItem> PendingRequests { get; set; } = new();
}

public class AccessRequestItem
{
    public long RequestID { get; set; }
    public long DocumentID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Plant { get; set; } = string.Empty;
    public string RevisionNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string? Remarks { get; set; }
    public DateTime? ViewExpiresAt { get; set; }
}

public class AccessApprovalViewModel
{
    public long RequestID { get; set; }
    public string Decision { get; set; } = string.Empty; // Approved, Rejected
    public string? Remarks { get; set; }
    public string Password { get; set; } = string.Empty; // QA re-auth
}

public class RequestAccessViewModel
{
    public long DocumentID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
