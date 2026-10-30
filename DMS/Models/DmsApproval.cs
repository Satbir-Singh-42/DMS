using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

[Table("DMS_Approvals")]
public class DmsApproval
{
    [Key]
    public long ApprovalID { get; set; }

    public long DocumentID { get; set; }

    public long RevisionID { get; set; }

    public long? RequestID { get; set; }

    [Required, MaxLength(30)]
    public string WorkflowType { get; set; } = string.Empty; // Review, DocumentApproval, AccessApproval, Issue

    [Required, MaxLength(20)]
    public string Decision { get; set; } = string.Empty; // Approved, Rejected, Returned, Issued

    public long PerformedBy { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReauthenticatedAt { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey("DocumentID")]
    public DmsDocument? Document { get; set; }

    [ForeignKey("RevisionID")]
    public DmsRevision? Revision { get; set; }

    [ForeignKey("RequestID")]
    public DmsRequest? Request { get; set; }

    [ForeignKey("PerformedBy")]
    public DmsUser? Performer { get; set; }
}
