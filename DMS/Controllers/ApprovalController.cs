using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers;

[Authorize(Roles = DmsRoles.QA)]
public class ApprovalController : Controller
{
    private readonly IWorkflowService _workflowService;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public ApprovalController(IWorkflowService workflowService, IAuthService authService, IAuditService auditService)
    {
        _workflowService = workflowService;
        _authService = authService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        var pending = await _workflowService.GetPendingApprovalsAsync();
        return View(new ReviewListViewModel { PendingRevisions = pending });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ApprovalActionViewModel model)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);

        if (!await _authService.VerifyPasswordAsync(userId, model.Password))
        {
            TempData["Error"] = "Password verification failed. Please try again.";
            return RedirectToAction("Index");
        }

        try
        {
            await _workflowService.ApproveRevisionAsync(model.RevisionID, userId, model.Decision, model.Remarks, DateTime.UtcNow);
            await _auditService.LogAsync(userId, $"DocumentApproval{model.Decision}", null, null, GetIpAddress(),
                $"Revision {model.RevisionID}: {model.Decision}");
            TempData["Success"] = $"Approval decision recorded: {model.Decision}";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

