using System.ComponentModel.DataAnnotations;

namespace DMS.Models.ViewModels;

public class PasswordVerifyViewModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string Action { get; set; } = string.Empty; // View, Approve, AccessApprove
    public long TargetId { get; set; } // RevisionID or RequestID
    public string? Remarks { get; set; }
    public string? Decision { get; set; }
}
