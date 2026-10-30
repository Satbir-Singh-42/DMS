using DMS.Data;
using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS.Controllers;

[Authorize(Roles = DmsRoles.Admin)]
public class UserController : Controller
{
    private readonly DmsDbContext _db;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public UserController(DmsDbContext db, IAuthService authService, IAuditService auditService)
    {
        _db = db;
        _authService = authService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.OrderBy(u => u.EmployeeName).ToListAsync();
        return View(users);
    }

    public IActionResult Create() => View(new UserCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _db.Users.AnyAsync(u => u.SalaryCode == model.SalaryCode))
        {
            ModelState.AddModelError("SalaryCode", "This salary code already exists.");
            return View(model);
        }

        var user = new DmsUser
        {
            SalaryCode = model.SalaryCode,
            EmployeeName = model.EmployeeName,
            PasswordHash = _authService.HashPassword(model.Password),
            AccessRole = model.AccessRole,
            IsActive = model.IsActive
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync(GetCurrentUserId(), "UserCreated", null, null, GetIpAddress(),
            $"Created user {user.SalaryCode} with role {user.AccessRole}");
        TempData["Success"] = "User created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(long id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(new UserEditViewModel
        {
            UserID = user.UserID, SalaryCode = user.SalaryCode,
            EmployeeName = user.EmployeeName, AccessRole = user.AccessRole, IsActive = user.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FindAsync(model.UserID);
        if (user == null) return NotFound();

        if (await _db.Users.AnyAsync(u => u.SalaryCode == model.SalaryCode && u.UserID != model.UserID))
        {
            ModelState.AddModelError("SalaryCode", "This salary code already exists.");
            return View(model);
        }

        user.SalaryCode = model.SalaryCode;
        user.EmployeeName = model.EmployeeName;
        user.AccessRole = model.AccessRole;
        user.IsActive = model.IsActive;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
            user.PasswordHash = _authService.HashPassword(model.NewPassword);

        await _db.SaveChangesAsync();
        await _auditService.LogAsync(GetCurrentUserId(), "UserUpdated", null, null, GetIpAddress(), $"Updated user {user.SalaryCode}");
        TempData["Success"] = "User updated successfully.";
        return RedirectToAction("Index");
    }

    private long GetCurrentUserId() => long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

