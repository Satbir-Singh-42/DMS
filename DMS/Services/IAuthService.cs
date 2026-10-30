using DMS.Models;

namespace DMS.Services;

public interface IAuthService
{
    Task<DmsUser?> AuthenticateAsync(string salaryCode, string password);
    Task<bool> VerifyPasswordAsync(long userId, string password);
    string HashPassword(string password);
    Task RecordFailedLoginAsync(long userId);
    Task ResetFailedLoginAsync(long userId);
    bool IsLockedOut(DmsUser user);
}
