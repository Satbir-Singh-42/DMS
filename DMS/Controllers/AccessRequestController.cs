using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers;

[Authorize]
public class AccessRequestController : Controller
{
    private readonly IWorkflowService _workflowService;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AccessRequestController(IWorkflowService workflowService, IAuthService authService, IAuditService auditService)
    {
        _workflowService = workflowService;
        _authService = authService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        var vm = new AccessRequestListViewModel();

        if (role == DmsRoles.QA)
            vm.PendingRequests = await _workflowService.GetPendingAccessRequestsAsync();
        if (role == DmsRoles.Plant)
            vm.MyRequests = await _workflowService.GetMyAccessRequestsAsync(userId);

        return View(vm);
    }

    [HttpPost, Authorize(Roles = DmsRoles.QA), ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(AccessApprovalViewModel model)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);

        if (!await _authService.VerifyPasswordAsync(userId, model.Password))
        {
            TempData["Error"] = "Password verification failed.";
            return RedirectToAction("Index");
        }

        try
        {
            await _workflowService.ApproveAccessRequestAsync(model.RequestID, userId, model.Decision, model.Remarks, DateTime.UtcNow);
            await _auditService.LogAsync(userId, $"AccessApproval{model.Decision}", null, model.RequestID, GetIpAddress(),
                $"Request {model.RequestID}: {model.Decision}");
            TempData["Success"] = $"Access request {model.Decision.ToLower()}.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

