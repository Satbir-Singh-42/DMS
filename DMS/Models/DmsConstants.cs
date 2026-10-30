namespace DMS.Models;

public static class DmsRoles
{
    public const string Admin = "Admin";
    public const string QA = "QA";
    public const string Plant = "Plant";

    public const string AdminOrQA = "Admin,QA";
    public const string AllRoles = "Admin,QA,Plant";
}

public static class RevisionStatus
{
    public const string Pending = "Pending";
    public const string Review = "Review";
    public const string Active = "Active";
    public const string Rejected = "Rejected";
    public const string Obsolete = "Obsolete";
}

public static class RequestStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Viewing = "Viewing";
    public const string Consumed = "Consumed";
    public const string Expired = "Expired";
}

public static class WorkflowTypes
{
    public const string Review = "Review";
    public const string DocumentApproval = "DocumentApproval";
    public const string AccessApproval = "AccessApproval";
    public const string Issue = "Issue";
}

public static class DmsClaimTypes
{
    public const string UserId = "UserID";
    public const string SalaryCode = "SalaryCode";
}

public static class DmsAuthDefaults
{
    public const string AuthenticationScheme = "DmsCookieAuth";
    public const string AuthCookieName = "DMS.Auth";
    public const string SessionCookieName = "DMS.Session";
    public const string DefaultStorageDirectory = "DMS_Storage";
}

