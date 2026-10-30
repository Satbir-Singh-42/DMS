using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models;

[Table("DMS_AuditLogs")]
public class DmsAuditLog
{
    [Key]
    public long AuditID { get; set; }

    public long? UserID { get; set; }

    [Required, MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    public long? DocumentID { get; set; }

    public long? RequestID { get; set; }

    public DateTime ActionAt { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? IPAddress { get; set; }

    public string? Details { get; set; } // NVARCHAR(MAX)

    // Navigation
    [ForeignKey("UserID")]
    public DmsUser? User { get; set; }
}
