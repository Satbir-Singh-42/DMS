using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DMS.Controllers;

[Authorize(Roles = DmsRoles.QA)]
public class UploadController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IPdfExtractionService _pdfService;
    private readonly IAuditService _auditService;
    private readonly DmsSettings _settings;

    public UploadController(IDocumentService documentService, IPdfExtractionService pdfService,
        IAuditService auditService, IOptions<DmsSettings> options)
    {
        _documentService = documentService;
        _pdfService = pdfService;
        _auditService = auditService;
        _settings = options.Value;
    }

    public IActionResult Index() => View(new UploadViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Extract(UploadViewModel model)
    {
        if (model.PdfFile == null || model.PdfFile.Length == 0)
        {
            ModelState.AddModelError("PdfFile", "Please select a PDF file.");
            return View("Index", model);
        }
        if (!model.PdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("PdfFile", "Only PDF files are allowed.");
            return View("Index", model);
        }

        using var stream = model.PdfFile.OpenReadStream();
        if (!_pdfService.ValidatePdf(stream, out var error))
        {
            ModelState.AddModelError("PdfFile", error);
            return View("Index", model);
        }

        stream.Position = 0;
        var metadata = _pdfService.ExtractFromFirstPage(stream);
        model.DocumentName = metadata.DocumentName ?? "";
        model.SerialNumber = metadata.SerialNumber ?? "";
        model.Plant = metadata.Plant ?? "";
        model.DocumentType = metadata.DocumentType;
        model.Unit = metadata.Unit;
        model.Category = metadata.Category;
        model.MaterialNumber = metadata.MaterialNumber;
        model.MaterialType = metadata.MaterialType;
        model.RevisionNumber = metadata.RevisionNumber ?? "";
        model.MetadataExtracted = true;

        foreach (var w in metadata.Warnings)
            ModelState.AddModelError("", w);

        if (!string.IsNullOrWhiteSpace(model.Plant) && !string.IsNullOrWhiteSpace(model.SerialNumber))
        {
            var (isDuplicate, existing) = await _documentService.CheckDuplicateAsync(model.Plant, model.SerialNumber);
            if (isDuplicate && existing != null)
            {
                model.IsRevision = true;
                model.ExistingDocumentID = existing.DocumentID;
                model.DuplicateWarning = $"Document with serial '{model.SerialNumber}' in plant '{model.Plant}' already exists. This will be added as a new revision.";
            }
        }
        if (!string.IsNullOrWhiteSpace(model.DocumentName))
        {
            var nameMatches = await _documentService.FindByNameAsync(model.DocumentName);
            if (nameMatches.Any())
                model.DuplicateWarning = (model.DuplicateWarning ?? "") + $" Warning: {nameMatches.Count} document(s) found with matching name.";
        }

        var tempPath = Path.Combine(Path.GetTempPath(), "DMS_Upload_" + Guid.NewGuid() + ".pdf");
        using (var fs = new FileStream(tempPath, FileMode.Create))
        {
            stream.Position = 0;
            await stream.CopyToAsync(fs);
        }
        TempData["TempFilePath"] = tempPath;
        return View("Verify", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(UploadViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.DocumentName) || string.IsNullOrWhiteSpace(model.SerialNumber) ||
            string.IsNullOrWhiteSpace(model.Plant) || string.IsNullOrWhiteSpace(model.RevisionNumber))
        {
            ModelState.AddModelError("", "Document Name, Serial Number, Plant, and Revision Number are required.");
            return View("Verify", model);
        }

        var tempPath = TempData["TempFilePath"]?.ToString();
        if (string.IsNullOrEmpty(tempPath) && model.PdfFile == null)
        {
            ModelState.AddModelError("", "PDF file was lost. Please upload again.");
            return View("Index", new UploadViewModel());
        }

        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        var storageBase = _settings.GetEffectiveStoragePath();
        var pendingPath = Path.Combine(storageBase, "Pending", model.Plant, model.SerialNumber);
        Directory.CreateDirectory(pendingPath);

        var fileName = $"{model.SerialNumber}_Rev{model.RevisionNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(pendingPath, fileName);

        if (!string.IsNullOrEmpty(tempPath) && System.IO.File.Exists(tempPath))
        {
            System.IO.File.Copy(tempPath, filePath, true);
            System.IO.File.Delete(tempPath);
        }
        else if (model.PdfFile != null)
        {
            using var fs = new FileStream(filePath, FileMode.Create);
            await model.PdfFile.CopyToAsync(fs);
        }

        string fileHash;
        using (var fs = System.IO.File.OpenRead(filePath))
            fileHash = _pdfService.ComputeFileHash(fs);

        try
        {
            if (model.IsRevision && model.ExistingDocumentID.HasValue)
            {
                var doc = await _documentService.GetDocumentAsync(model.ExistingDocumentID.Value);
                if (doc?.Revisions.Any(r => r.RevisionNumber == model.RevisionNumber) == true)
                {
                    ModelState.AddModelError("RevisionNumber", "This revision number already exists for this document.");
                    return View("Verify", model);
                }
                await _documentService.AddRevisionAsync(model.ExistingDocumentID.Value, model, userId, filePath, fileHash);
                await _auditService.LogAsync(userId, "RevisionUploaded", model.ExistingDocumentID.Value, null, GetIpAddress(),
                    $"Revision {model.RevisionNumber} uploaded");
            }
            else
            {
                var (document, _) = await _documentService.CreateDocumentWithRevisionAsync(model, userId, filePath, fileHash);
                await _auditService.LogAsync(userId, "DocumentCreated", document.DocumentID, null, GetIpAddress(),
                    $"Document {model.SerialNumber} created with revision {model.RevisionNumber}");
            }
            TempData["Success"] = "Document uploaded and submitted for review.";
            return RedirectToAction("Index", "Document");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving document: {ex.Message}");
            return View("Verify", model);
        }
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

