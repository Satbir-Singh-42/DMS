using DMS.Data;
using DMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DMS.Services;

public class AuthService : IAuthService
{
    private readonly DmsDbContext _db;
    private readonly DmsSettings _settings;
    private readonly PasswordHasher<DmsUser> _hasher = new();

    public AuthService(DmsDbContext db, IOptions<DmsSettings> options)
    {
        _db = db;
        _settings = options.Value;
    }

    public async Task<DmsUser?> AuthenticateAsync(string salaryCode, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.SalaryCode == salaryCode && u.IsActive);
        if (user == null) return null;
        if (IsLockedOut(user)) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            await RecordFailedLoginAsync(user.UserID);
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, password);
        }

        await ResetFailedLoginAsync(user.UserID);
        return user;
    }

    public async Task<bool> VerifyPasswordAsync(long userId, string password)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null || !user.IsActive) return false;
        if (IsLockedOut(user)) return false;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            await RecordFailedLoginAsync(userId);
            return false;
        }

        await ResetFailedLoginAsync(userId);
        return true;
    }

    public string HashPassword(string password)
    {
        var user = new DmsUser();
        return _hasher.HashPassword(user, password);
    }

    public async Task RecordFailedLoginAsync(long userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= _settings.MaxFailedLoginAttempts)
        {
            user.LockedUntil = DateTime.UtcNow.AddMinutes(_settings.LockoutMinutes);
        }
        await _db.SaveChangesAsync();
    }

    public async Task ResetFailedLoginAsync(long userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        await _db.SaveChangesAsync();
    }

    public bool IsLockedOut(DmsUser user)
    {
        return user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow;
    }
}
