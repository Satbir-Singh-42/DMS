namespace DMS.Services;

public class ExtractedMetadata
{
    public string? DocumentName { get; set; }
    public string? SerialNumber { get; set; }
    public string? Plant { get; set; }
    public string? DocumentType { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string? MaterialNumber { get; set; }
    public string? MaterialType { get; set; }
    public string? RevisionNumber { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public interface IPdfExtractionService
{
    ExtractedMetadata ExtractFromFirstPage(Stream pdfStream);
    bool ValidatePdf(Stream pdfStream, out string error);
    string ComputeFileHash(Stream fileStream);
}
