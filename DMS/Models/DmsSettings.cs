namespace DMS.Models;

public class DmsSettings
{
    public const string SectionName = "DMS";

    public string StoragePath { get; set; } = string.Empty;
    public long MaxPdfSizeBytes { get; set; } = 104857600; // 100 MB
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public int ViewingSessionMinutes { get; set; } = 30;
    public int CookieExpireHours { get; set; } = 8;
    public int DefaultPageSize { get; set; } = 20;

    public string GetEffectiveStoragePath()
    {
        return !string.IsNullOrWhiteSpace(StoragePath)
            ? StoragePath
            : Path.Combine(Directory.GetCurrentDirectory(), DmsAuthDefaults.DefaultStorageDirectory);
    }

    public InitialAdminSettings InitialAdmin { get; set; } = new();
    public Dictionary<string, string[]> ExtractionAliases { get; set; } = new()
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
}

public class InitialAdminSettings
{
    public string SalaryCode { get; set; } = "ADMIN001";
    public string EmployeeName { get; set; } = "System Administrator";
    public string DefaultPassword { get; set; } = "Admin@123";
}

