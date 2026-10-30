using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

[Table("DMS_Documents")]
public class DmsDocument
{
    [Key]
    public long DocumentID { get; set; }

    [Required, MaxLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Plant { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DocumentType { get; set; }

    [MaxLength(100)]
    public string? Unit { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? MaterialNumber { get; set; }

    [MaxLength(100)]
    public string? MaterialType { get; set; }

    public long? CurrentRevisionID { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("CurrentRevisionID")]
    public DmsRevision? CurrentRevision { get; set; }

    [ForeignKey("CreatedBy")]
    public DmsUser? Creator { get; set; }

    public ICollection<DmsRevision> Revisions { get; set; } = new List<DmsRevision>();
    public ICollection<DmsRequest> Requests { get; set; } = new List<DmsRequest>();
}
