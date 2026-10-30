namespace DMS.Models.ViewModels;

public class ReviewListViewModel
{
    public List<RevisionItem> PendingRevisions { get; set; } = new();
}

public class RevisionItem
{
    public long RevisionID { get; set; }
    public long DocumentID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Plant { get; set; } = string.Empty;
    public string RevisionNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string FileName { get; set; } = string.Empty;
}

public class ReviewActionViewModel
{
    public long RevisionID { get; set; }
    public string Decision { get; set; } = string.Empty; // Approved, Rejected, Returned
    public string? Remarks { get; set; }
}

public class ApprovalActionViewModel
{
    public long RevisionID { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Password { get; set; } = string.Empty; // QA re-auth
}
