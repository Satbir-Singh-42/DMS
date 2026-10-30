using DMS.Data;
using DMS.Models;
using DMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly DmsDbContext _db;

    public DashboardController(DmsDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)!.Value;

        var vm = new DashboardViewModel
        {
            EmployeeName = name,
            Role = role
        };

        if (role == DmsRoles.QA)
        {
            vm.PendingReviews = await _db.Revisions.CountAsync(r => r.Status == RevisionStatus.Pending);
            vm.PendingApprovals = await _db.Revisions.CountAsync(r => r.Status == RevisionStatus.Review);
            vm.PendingAccessRequests = await _db.Requests.CountAsync(r => r.Status == RequestStatus.Pending);
            vm.PendingIssuance = await _db.Revisions
                .Where(r => r.Status == RevisionStatus.Active)
                .Where(r => !_db.Approvals.Any(a => a.RevisionID == r.RevisionID && a.WorkflowType == WorkflowTypes.Issue))
                .CountAsync();
        }
        else if (role == DmsRoles.Plant)
        {
            vm.MyPendingRequests = await _db.Requests.CountAsync(r => r.RequestedBy == userId && r.Status == RequestStatus.Pending);
            vm.MyApprovedRequests = await _db.Requests.CountAsync(r => r.RequestedBy == userId && r.Status == RequestStatus.Approved);
        }
        else if (role == DmsRoles.Admin)
        {
            vm.TotalUsers = await _db.Users.CountAsync();
            vm.ActiveUsers = await _db.Users.CountAsync(u => u.IsActive);
            vm.TotalDocuments = await _db.Documents.CountAsync();
        }

        return View(vm);
    }
}

