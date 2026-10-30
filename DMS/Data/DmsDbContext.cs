using DMS.Models;
using Microsoft.EntityFrameworkCore;

namespace DMS.Data;

public class DmsDbContext : DbContext
{
    public DmsDbContext(DbContextOptions<DmsDbContext> options) : base(options) { }

    public DbSet<DmsUser> Users => Set<DmsUser>();
    public DbSet<DmsDocument> Documents => Set<DmsDocument>();
    public DbSet<DmsRevision> Revisions => Set<DmsRevision>();
    public DbSet<DmsRequest> Requests => Set<DmsRequest>();
    public DbSet<DmsApproval> Approvals => Set<DmsApproval>();
    public DbSet<DmsAuditLog> AuditLogs => Set<DmsAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DMS_Users
        modelBuilder.Entity<DmsUser>(entity =>
        {
            entity.HasIndex(e => e.SalaryCode).IsUnique();
        });

        // DMS_Documents
        modelBuilder.Entity<DmsDocument>(entity =>
        {
            entity.HasIndex(e => new { e.Plant, e.SerialNumber }).IsUnique();
            
            entity.HasOne(e => e.CurrentRevision)
                  .WithMany()
                  .HasForeignKey(e => e.CurrentRevisionID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedDocuments)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // DMS_Revisions
        modelBuilder.Entity<DmsRevision>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentID, e.RevisionNumber }).IsUnique();
            
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Revisions)
                  .HasForeignKey(e => e.DocumentID)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Uploader)
                  .WithMany(u => u.UploadedRevisions)
                  .HasForeignKey(e => e.UploadedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // DMS_Requests
        modelBuilder.Entity<DmsRequest>(entity =>
        {
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Requests)
                  .HasForeignKey(e => e.DocumentID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Revision)
                  .WithMany(r => r.Requests)
                  .HasForeignKey(e => e.RevisionID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Requester)
                  .WithMany(u => u.Requests)
                  .HasForeignKey(e => e.RequestedBy)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Approver)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // DMS_Approvals
        modelBuilder.Entity<DmsApproval>(entity =>
        {
            entity.HasOne(e => e.Document)
                  .WithMany()
                  .HasForeignKey(e => e.DocumentID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Revision)
                  .WithMany(r => r.Approvals)
                  .HasForeignKey(e => e.RevisionID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Request)
                  .WithMany()
                  .HasForeignKey(e => e.RequestID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Performer)
                  .WithMany(u => u.Approvals)
                  .HasForeignKey(e => e.PerformedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // DMS_AuditLogs
        modelBuilder.Entity<DmsAuditLog>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserID)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.ActionAt);
            entity.HasIndex(e => e.ActionType);
        });

        // Admin user is seeded in Program.cs startup
    }
}
