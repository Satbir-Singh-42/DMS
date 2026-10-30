using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers;

[Authorize(Roles = DmsRoles.QA)]
public class IssueController : Controller
{
    private readonly IWorkflowService _workflowService;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public IssueController(IWorkflowService workflowService, IAuthService authService, IAuditService auditService)
    {
        _workflowService = workflowService;
        _authService = authService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        var pending = await _workflowService.GetPendingIssuanceAsync();
        return View(new ReviewListViewModel { PendingRevisions = pending });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(long revisionId, string password, string? remarks)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        if (!await _authService.VerifyPasswordAsync(userId, password))
        {
            TempData["Error"] = "Password verification failed.";
            return RedirectToAction("Index");
        }
        try
        {
            await _workflowService.IssueRevisionAsync(revisionId, userId, remarks, DateTime.UtcNow);
            await _auditService.LogAsync(userId, "DocumentIssued", null, null, GetIpAddress(), $"Revision {revisionId} issued");
            TempData["Success"] = "Document issued successfully.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

