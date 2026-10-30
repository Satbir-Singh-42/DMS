using DMS.Models;
using DMS.Models.ViewModels;

namespace DMS.Services;

public interface IDocumentService
{
    Task<DocumentSearchViewModel> SearchDocumentsAsync(DocumentSearchViewModel search);
    Task<DmsDocument?> GetDocumentAsync(long documentId);
    Task<(DmsDocument document, DmsRevision revision)> CreateDocumentWithRevisionAsync(UploadViewModel model, long userId, string filePath, string fileHash);
    Task<DmsRevision> AddRevisionAsync(long documentId, UploadViewModel model, long userId, string filePath, string fileHash);
    Task<(bool isDuplicate, DmsDocument? existing)> CheckDuplicateAsync(string plant, string serialNumber);
    Task<List<DmsDocument>> FindByNameAsync(string documentName);
}
