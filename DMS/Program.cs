using DMS.Data;
using DMS.Models;
using DMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ── Bind strongly-typed configuration ──────────────────────────────────
builder.Services.Configure<DmsSettings>(builder.Configuration.GetSection(DmsSettings.SectionName));
var dmsSettings = builder.Configuration.GetSection(DmsSettings.SectionName).Get<DmsSettings>() ?? new DmsSettings();

// ── Database ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<DmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DmsConnection")));

// ── Application services ──────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPdfExtractionService, PdfExtractionService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// ── Authentication (cookie-based, salary-code login) ──────────────────
builder.Services.AddAuthentication(DmsAuthDefaults.AuthenticationScheme)
    .AddCookie(DmsAuthDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = DmsAuthDefaults.AuthCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(dmsSettings.CookieExpireHours > 0 ? dmsSettings.CookieExpireHours : 8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ── Session (viewer tokens & temporary upload staging) ─────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(dmsSettings.ViewingSessionMinutes > 0 ? dmsSettings.ViewingSessionMinutes : 30);
    options.Cookie.Name = DmsAuthDefaults.SessionCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ── Health checks for production monitoring ───────────────────────────
builder.Services.AddHealthChecks();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// ── Apply migrations & seed initial admin ──────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DmsDbContext>();
    db.Database.Migrate();

    var settingsOptions = scope.ServiceProvider.GetRequiredService<IOptions<DmsSettings>>().Value;
    var adminCfg = settingsOptions.InitialAdmin;

    if (!db.Users.Any(u => u.SalaryCode == adminCfg.SalaryCode))
    {
        var hasher = new PasswordHasher<DmsUser>();
        var admin = new DmsUser
        {
            SalaryCode = adminCfg.SalaryCode,
            EmployeeName = adminCfg.EmployeeName,
            AccessRole = DmsRoles.Admin,
            IsActive = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, adminCfg.DefaultPassword);
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Security response headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
