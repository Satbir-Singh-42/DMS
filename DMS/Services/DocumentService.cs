using DMS.Data;
using DMS.Models;
using DMS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services;

public class DocumentService : IDocumentService
{
    private readonly DmsDbContext _db;

    public DocumentService(DmsDbContext db)
    {
        _db = db;
    }

    public async Task<DocumentSearchViewModel> SearchDocumentsAsync(DocumentSearchViewModel search)
    {
        var query = _db.Documents
            .Include(d => d.CurrentRevision)
            .Where(d => d.CurrentRevision == null || d.CurrentRevision.Status != "Obsolete");

        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(d =>
                d.DocumentName.ToLower().Contains(term) ||
                d.SerialNumber.ToLower().Contains(term) ||
                (d.MaterialNumber != null && d.MaterialNumber.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(search.PlantFilter))
            query = query.Where(d => d.Plant == search.PlantFilter);

        if (!string.IsNullOrWhiteSpace(search.TypeFilter))
            query = query.Where(d => d.DocumentType == search.TypeFilter);

        if (!string.IsNullOrWhiteSpace(search.CategoryFilter))
            query = query.Where(d => d.Category == search.CategoryFilter);

        search.TotalCount = await query.CountAsync();

        search.Documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(d => new DocumentListItem
            {
                DocumentID = d.DocumentID,
                DocumentName = d.DocumentName,
                SerialNumber = d.SerialNumber,
                Plant = d.Plant,
                DocumentType = d.DocumentType,
                Unit = d.Unit,
                Category = d.Category,
                CurrentRevisionID = d.CurrentRevisionID,
                CurrentRevisionNumber = d.CurrentRevision != null ? d.CurrentRevision.RevisionNumber : null,
                CurrentRevisionStatus = d.CurrentRevision != null ? d.CurrentRevision.Status : null,
            })
            .ToListAsync();

        search.Plants = await _db.Documents.Select(d => d.Plant).Distinct().OrderBy(p => p).ToListAsync();
        search.Types = await _db.Documents.Where(d => d.DocumentType != null).Select(d => d.DocumentType!).Distinct().OrderBy(t => t).ToListAsync();
        search.Categories = await _db.Documents.Where(d => d.Category != null).Select(d => d.Category!).Distinct().OrderBy(c => c).ToListAsync();

        return search;
    }

    public async Task<DmsDocument?> GetDocumentAsync(long documentId)
    {
        return await _db.Documents
            .Include(d => d.CurrentRevision)
            .Include(d => d.Revisions.Where(r => r.Status != "Obsolete"))
            .FirstOrDefaultAsync(d => d.DocumentID == documentId);
    }

    public async Task<(DmsDocument document, DmsRevision revision)> CreateDocumentWithRevisionAsync(
        UploadViewModel model, long userId, string filePath, string fileHash)
    {
        var document = new DmsDocument
        {
            DocumentName = model.DocumentName,
            SerialNumber = model.SerialNumber,
            Plant = model.Plant,
            DocumentType = model.DocumentType,
            Unit = model.Unit,
            Category = model.Category,
            MaterialNumber = model.MaterialNumber,
            MaterialType = model.MaterialType,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        var revision = new DmsRevision
        {
            DocumentID = document.DocumentID,
            RevisionNumber = model.RevisionNumber,
            FileName = model.PdfFile!.FileName,
            FileLocation = filePath,
            FileHash = fileHash,
            Status = "Pending",
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };
        _db.Revisions.Add(revision);
        await _db.SaveChangesAsync();

        return (document, revision);
    }

    public async Task<DmsRevision> AddRevisionAsync(long documentId, UploadViewModel model, long userId, string filePath, string fileHash)
    {
        var revision = new DmsRevision
        {
            DocumentID = documentId,
            RevisionNumber = model.RevisionNumber,
            FileName = model.PdfFile!.FileName,
            FileLocation = filePath,
            FileHash = fileHash,
            Status = "Pending",
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };
        _db.Revisions.Add(revision);
        await _db.SaveChangesAsync();
        return revision;
    }

    public async Task<(bool isDuplicate, DmsDocument? existing)> CheckDuplicateAsync(string plant, string serialNumber)
    {
        var existing = await _db.Documents
            .FirstOrDefaultAsync(d => d.Plant == plant && d.SerialNumber == serialNumber);
        return (existing != null, existing);
    }

    public async Task<List<DmsDocument>> FindByNameAsync(string documentName)
    {
        return await _db.Documents
            .Where(d => d.DocumentName == documentName)
            .ToListAsync();
    }
}
