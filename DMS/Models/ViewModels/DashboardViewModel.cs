namespace DMS.Models.ViewModels;

public class DashboardViewModel
{
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // QA counts
    public int PendingUploads { get; set; }
    public int PendingReviews { get; set; }
    public int PendingApprovals { get; set; }
    public int PendingAccessRequests { get; set; }
    public int PendingIssuance { get; set; }

    // Plant counts
    public int MyPendingRequests { get; set; }
    public int MyApprovedRequests { get; set; }

    // Admin counts
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalDocuments { get; set; }
}
