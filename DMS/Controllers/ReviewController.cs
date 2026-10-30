using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers;

[Authorize(Roles = DmsRoles.QA)]
public class ReviewController : Controller
{
    private readonly IWorkflowService _workflowService;
    private readonly IAuditService _auditService;

    public ReviewController(IWorkflowService workflowService, IAuditService auditService)
    {
        _workflowService = workflowService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        var pending = await _workflowService.GetPendingReviewsAsync();
        return View(new ReviewListViewModel { PendingRevisions = pending });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ReviewActionViewModel model)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        try
        {
            await _workflowService.ReviewRevisionAsync(model.RevisionID, userId, model.Decision, model.Remarks);
            await _auditService.LogAsync(userId, $"Review{model.Decision}", null, null, GetIpAddress(),
                $"Revision {model.RevisionID}: {model.Decision}");
            TempData["Success"] = $"Review decision recorded: {model.Decision}";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

