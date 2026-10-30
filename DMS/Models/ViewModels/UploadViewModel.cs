using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DMS.Models.ViewModels;

public class UploadViewModel
{
    [Required]
    [Display(Name = "PDF Document")]
    public IFormFile? PdfFile { get; set; }

    // Extracted / editable fields
    [Required, MaxLength(255)]
    [Display(Name = "Document Name")]
    public string DocumentName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Plant { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Document Type")]
    public string? DocumentType { get; set; }

    [MaxLength(100)]
    public string? Unit { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    [Display(Name = "Material Number")]
    public string? MaterialNumber { get; set; }

    [MaxLength(100)]
    [Display(Name = "Material Type")]
    public string? MaterialType { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Revision Number")]
    public string RevisionNumber { get; set; } = string.Empty;

    // Flags
    public bool IsRevision { get; set; }
    public long? ExistingDocumentID { get; set; }
    public string? DuplicateWarning { get; set; }
    public bool MetadataExtracted { get; set; }
}
