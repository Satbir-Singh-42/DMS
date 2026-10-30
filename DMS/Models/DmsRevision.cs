using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

[Table("DMS_Revisions")]
public class DmsRevision
{
    [Key]
    public long RevisionID { get; set; }

    public long DocumentID { get; set; }

    [Required, MaxLength(30)]
    public string RevisionNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string FileLocation { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? FileHash { get; set; } // SHA-256

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Review, Approved, Active, Obsolete, Rejected

    public long UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveAt { get; set; }

    public DateTime? ObsoleteAt { get; set; }

    // Navigation
    [ForeignKey("DocumentID")]
    public DmsDocument? Document { get; set; }

    [ForeignKey("UploadedBy")]
    public DmsUser? Uploader { get; set; }

    public ICollection<DmsRequest> Requests { get; set; } = new List<DmsRequest>();
    public ICollection<DmsApproval> Approvals { get; set; } = new List<DmsApproval>();
}
