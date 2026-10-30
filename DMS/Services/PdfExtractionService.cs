using System.Security.Cryptography;
using System.Text.RegularExpressions;
using DMS.Models;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

namespace DMS.Services;

public class PdfExtractionService : IPdfExtractionService
{
    private readonly DmsSettings _settings;

    public PdfExtractionService(IOptions<DmsSettings> options)
    {
        _settings = options.Value;
    }

    public ExtractedMetadata ExtractFromFirstPage(Stream pdfStream)
    {
        var metadata = new ExtractedMetadata();

        try
        {
            using var document = PdfDocument.Open(pdfStream);
            if (document.NumberOfPages == 0)
            {
                metadata.Warnings.Add("PDF has no pages.");
                return metadata;
            }

            var page = document.GetPage(1);
            var text = page.Text;
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                           .Select(l => l.Trim())
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .ToList();

            var aliases = _settings.ExtractionAliases != null && _settings.ExtractionAliases.Count > 0
                ? _settings.ExtractionAliases
                : new Dictionary<string, string[]>
                {
                    ["Plant"] = new[] { "PLANT CODE", "PLANT" },
                    ["SerialNumber"] = new[] { "SERIAL NO.", "SERIAL NO", "DOCUMENT NO.", "DOCUMENT NO", "DOC NO.", "DOC NO" },
                    ["DocumentName"] = new[] { "DOCUMENT NAME", "TITLE" },
                    ["DocumentType"] = new[] { "DOCUMENT TYPE", "TYPE" },
                    ["Unit"] = new[] { "UNIT NO.", "UNIT NO", "UNIT" },
                    ["Category"] = new[] { "CATEGORY" },
                    ["MaterialNumber"] = new[] { "MATERIAL NO.", "MATERIAL NUMBER", "MATERIAL NO" },
                    ["MaterialType"] = new[] { "MATERIAL TYPE" },
                    ["RevisionNumber"] = new[] { "REVISION", "REV. NO.", "REV NO.", "REV. NO", "REV NO" }
                };

            foreach (var field in aliases)
            {
                var value = FindFieldValue(lines, text, field.Value, aliases);
                if (value != null)
                {
                    typeof(ExtractedMetadata).GetProperty(field.Key)?.SetValue(metadata, value);
                }
                else
                {
                    metadata.Warnings.Add($"Could not extract '{field.Key}' — please enter manually.");
                }
            }
        }
        catch (Exception ex)
        {
            metadata.Warnings.Add($"PDF parsing error: {ex.Message}");
        }

        return metadata;
    }

    private string? FindFieldValue(List<string> lines, string fullText, string[] labels, Dictionary<string, string[]> allAliases)
    {
        foreach (var label in labels)
        {
            var pattern = $@"{Regex.Escape(label)}\s*[:\-]?\s*(.+)";
            var match = Regex.Match(fullText, pattern, RegexOptions.IgnoreCase);
            if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
            {
                var val = match.Groups[1].Value.Trim();
                var endIdx = val.IndexOfAny(new[] { '\n', '\r' });
                if (endIdx > 0) val = val[..endIdx].Trim();
                if (!string.IsNullOrWhiteSpace(val) && val.Length <= 255)
                    return val;
            }

            for (int i = 0; i < lines.Count - 1; i++)
            {
                if (lines[i].Contains(label, StringComparison.OrdinalIgnoreCase))
                {
                    var nextLine = lines[i + 1].Trim();
                    if (!string.IsNullOrWhiteSpace(nextLine) && nextLine.Length <= 255)
                    {
                        bool isLabel = allAliases.Values.Any(lbls => lbls.Any(l => nextLine.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
                        if (!isLabel) return nextLine;
                    }
                }
            }
        }
        return null;
    }

    public bool ValidatePdf(Stream pdfStream, out string error)
    {
        error = string.Empty;

        if (pdfStream.Length == 0)
        {
            error = "File is empty.";
            return false;
        }

        if (pdfStream.Length > _settings.MaxPdfSizeBytes)
        {
            var maxMb = _settings.MaxPdfSizeBytes / (1024 * 1024);
            error = $"File exceeds maximum allowed size of {maxMb} MB.";
            return false;
        }

        pdfStream.Position = 0;
        var header = new byte[5];
        pdfStream.ReadExactly(header, 0, 5);
        pdfStream.Position = 0;

        if (System.Text.Encoding.ASCII.GetString(header) != "%PDF-")
        {
            error = "File is not a valid PDF.";
            return false;
        }

        try
        {
            using var doc = PdfDocument.Open(pdfStream);
            if (doc.NumberOfPages == 0)
            {
                error = "PDF contains no pages.";
                return false;
            }
        }
        catch
        {
            error = "PDF is corrupted or cannot be read.";
            return false;
        }

        pdfStream.Position = 0;
        return true;
    }

    public string ComputeFileHash(Stream fileStream)
    {
        fileStream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileStream);
        fileStream.Position = 0;
        return Convert.ToHexStringLower(hashBytes);
    }
}
