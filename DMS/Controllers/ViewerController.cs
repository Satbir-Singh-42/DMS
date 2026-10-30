using System.Security.Cryptography;
using DMS.Data;
using DMS.Models;
using DMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DMS.Controllers;

[Authorize]
public class ViewerController : Controller
{
    private readonly IWorkflowService _workflowService;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;
    private readonly DmsDbContext _db;
    private readonly DmsSettings _settings;

    public ViewerController(IWorkflowService workflowService, IAuthService authService,
        IAuditService auditService, DmsDbContext db, IOptions<DmsSettings> options)
    {
        _workflowService = workflowService;
        _authService = authService;
        _auditService = auditService;
        _db = db;
        _settings = options.Value;
    }

    [Authorize(Roles = DmsRoles.Plant)]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Start(long requestId)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        var (token, expiresAt) = await _workflowService.StartViewingSessionAsync(requestId, userId);
        if (token == null)
        {
            TempData["Error"] = "Unable to start viewing session. Request may be expired or already used.";
            return RedirectToAction("Index", "AccessRequest");
        }

        var tokenHash = ComputeTokenHash(token);
        HttpContext.Session.SetString("ViewToken", tokenHash);
        HttpContext.Session.SetString("ViewRequestId", requestId.ToString());
        await _auditService.LogAsync(userId, "ViewingStarted", null, requestId, GetIpAddress(), null);

        ViewBag.RequestId = requestId;
        ViewBag.ExpiresAt = expiresAt;
        ViewBag.IsQa = false;
        return View("View");
    }

    [Authorize(Roles = DmsRoles.QA)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> QaView(long revisionId, string password)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        if (!await _authService.VerifyPasswordAsync(userId, password))
        {
            TempData["Error"] = "Password verification failed.";
            return RedirectToAction("Index", "Document");
        }

        var revision = await _db.Revisions.Include(r => r.Document).FirstOrDefaultAsync(r => r.RevisionID == revisionId);
        if (revision == null || revision.Status == RevisionStatus.Obsolete)
        {
            TempData["Error"] = "Document not found or is obsolete.";
            return RedirectToAction("Index", "Document");
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes);
        HttpContext.Session.SetString("QaViewToken", ComputeTokenHash(token));
        HttpContext.Session.SetString("QaViewRevisionId", revisionId.ToString());
        HttpContext.Session.SetString("QaViewExpiry", DateTime.UtcNow.AddMinutes(_settings.ViewingSessionMinutes).ToString("O"));

        await _auditService.LogAsync(userId, "QAViewing", revision.DocumentID, null, GetIpAddress(),
            $"QA viewing revision {revision.RevisionNumber}");

        ViewBag.RevisionId = revisionId;
        ViewBag.IsQa = true;
        return View("View");
    }

    [HttpGet]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ServePdf(long? requestId, long? revisionId)
    {
        var userId = long.Parse(User.FindFirst(DmsClaimTypes.UserId)!.Value);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        string? filePath = null;

        if (role == DmsRoles.Plant && requestId.HasValue)
        {
            var tokenHash = HttpContext.Session.GetString("ViewToken");
            if (string.IsNullOrEmpty(tokenHash)) return Forbid();
            var request = await _workflowService.ValidateViewingSessionAsync(tokenHash, userId);
            if (request?.Revision == null) return Forbid();
            filePath = request.Revision.FileLocation;
        }
        else if (role == DmsRoles.QA && revisionId.HasValue)
        {
            var qaToken = HttpContext.Session.GetString("QaViewToken");
            var qaRevId = HttpContext.Session.GetString("QaViewRevisionId");
            var qaExpiry = HttpContext.Session.GetString("QaViewExpiry");
            if (string.IsNullOrEmpty(qaToken) || qaRevId != revisionId.ToString()) return Forbid();
            if (DateTime.TryParse(qaExpiry, out var expiry) && expiry < DateTime.UtcNow) return Forbid();
            var revision = await _db.Revisions.FindAsync(revisionId.Value);
            if (revision == null || revision.Status == RevisionStatus.Obsolete) return Forbid();
            filePath = revision.FileLocation;
        }
        else return Forbid();

        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return NotFound("PDF file not found.");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        Response.Headers.Append("Content-Disposition", "inline; filename=document.pdf");
        Response.Headers.Append("X-Content-Type-Options", "nosniff");
        Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
        return File(bytes, "application/pdf");
    }

    private static string ComputeTokenHash(string token)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token)));
    }

    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

