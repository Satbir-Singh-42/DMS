using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

[Table("DMS_Users")]
public class DmsUser
{
    [Key]
    public long UserID { get; set; }

    [Required, MaxLength(50)]
    public string SalaryCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string AccessRole { get; set; } = string.Empty; // Admin, QA, Plant

    public bool IsActive { get; set; } = true;

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockedUntil { get; set; }

    // Navigation
    public ICollection<DmsDocument> CreatedDocuments { get; set; } = new List<DmsDocument>();
    public ICollection<DmsRevision> UploadedRevisions { get; set; } = new List<DmsRevision>();
    public ICollection<DmsRequest> Requests { get; set; } = new List<DmsRequest>();
    public ICollection<DmsApproval> Approvals { get; set; } = new List<DmsApproval>();
    public ICollection<DmsAuditLog> AuditLogs { get; set; } = new List<DmsAuditLog>();
}
