using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DMS.Controllers;

[Authorize]
public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IWorkflowService _workflowService;
    private readonly IAuditService _auditService;
    private readonly DmsSettings _settings;

    public DocumentController(IDocumentService documentService, IWorkflowService workflowService,
        IAuditService auditService, IOptions<DmsSettings> options)
    {
        _documentService = documentService;
        _workflowService = workflowService;
        _auditService = auditService;
        _settings = options.Value;
    }

    public async Task<IActionResult> Index(DocumentSearchViewModel search)
    {
        if (search.PageSize <= 0)
            search.PageSize = _settings.DefaultPageSize > 0 ? _settings.DefaultPageSize : 20;

        var result = await _documentService.SearchDocumentsAsync(search);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == DmsRoles.Plant)
        {
            foreach (var doc in result.Documents)
                doc.CanRequestAccess = doc.CurrentRevisionStatus == RevisionStatus.Active;
        }
        return View(result);
    }

    [Authorize(Roles = DmsRoles.Plant)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestAccess(RequestAccessViewModel model)
    {
        try
        {
            var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
            var request = await _workflowService.CreateAccessRequestAsync(model.DocumentID, userId, model.Remarks);
            await _auditService.LogAsync(userId, "AccessRequested", model.DocumentID, request.RequestID, GetIpAddress(), null);
            TempData["Success"] = "Access request submitted successfully.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

