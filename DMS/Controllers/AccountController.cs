using System.Security.Claims;
using DMS.Models;
using DMS.Models.ViewModels;
using DMS.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DMS.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;
    private readonly DmsSettings _settings;

    public AccountController(IAuthService authService, IAuditService auditService, IOptions<DmsSettings> options)
    {
        _authService = authService;
        _auditService = auditService;
        _settings = options.Value;
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _authService.AuthenticateAsync(model.SalaryCode, model.Password);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid salary code or password, or account is locked.");
            await _auditService.LogAsync(null, "LoginFailed", null, null, GetIpAddress(), $"SalaryCode: {model.SalaryCode}");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(DmsClaimTypes.UserId, user.UserID.ToString()),
            new(DmsClaimTypes.SalaryCode, user.SalaryCode),
            new(ClaimTypes.Name, user.EmployeeName),
            new(ClaimTypes.Role, user.AccessRole)
        };

        var identity = new ClaimsIdentity(claims, DmsAuthDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(DmsAuthDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(_settings.CookieExpireHours > 0 ? _settings.CookieExpireHours : 8)
        });

        await _auditService.LogAsync(user.UserID, "Login", null, null, GetIpAddress(), null);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        await HttpContext.SignOutAsync(DmsAuthDefaults.AuthenticationScheme);
        await _auditService.LogAsync(userId, "Logout", null, null, GetIpAddress(), null);
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public IActionResult AccessDenied()
    {
        TempData["Error"] = "Access Denied: You do not have permission to access that resource.";
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return RedirectToAction("Login");
    }

    private long? GetCurrentUserId()
    {
        var claim = User.FindFirst(DmsClaimTypes.UserId);
        return claim != null ? long.Parse(claim.Value) : null;
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

