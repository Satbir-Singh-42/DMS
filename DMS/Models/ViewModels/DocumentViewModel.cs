using System.ComponentModel.DataAnnotations;

namespace DMS.Models.ViewModels;

public class DocumentSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? PlantFilter { get; set; }
    public string? TypeFilter { get; set; }
    public string? CategoryFilter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public List<DocumentListItem> Documents { get; set; } = new();
    public List<string> Plants { get; set; } = new();
    public List<string> Types { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}

public class DocumentListItem
{
    public long DocumentID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Plant { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public long? CurrentRevisionID { get; set; }
    public string? CurrentRevisionNumber { get; set; }
    public string? CurrentRevisionStatus { get; set; }
    public bool CanRequestAccess { get; set; }
}
