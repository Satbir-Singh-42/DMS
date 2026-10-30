using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

[Table("DMS_Requests")]
public class DmsRequest
{
    [Key]
    public long RequestID { get; set; }

    public long DocumentID { get; set; }

    public long RevisionID { get; set; }

    public long RequestedBy { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Viewing, Consumed, Expired

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(128)]
    public string? ViewTokenHash { get; set; }

    public DateTime? ViewStartedAt { get; set; }

    public DateTime? ViewExpiresAt { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey("DocumentID")]
    public DmsDocument? Document { get; set; }

    [ForeignKey("RevisionID")]
    public DmsRevision? Revision { get; set; }

    [ForeignKey("RequestedBy")]
    public DmsUser? Requester { get; set; }

    [ForeignKey("ApprovedBy")]
    public DmsUser? Approver { get; set; }
}
