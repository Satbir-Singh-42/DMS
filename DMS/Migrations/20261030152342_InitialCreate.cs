using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DMS_Users",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AccessRole = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMS_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "DMS_AuditLogs",
                columns: table => new
                {
                    AuditID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentID = table.Column<long>(type: "bigint", nullable: true),
                    RequestID = table.Column<long>(type: "bigint", nullable: true),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMS_AuditLogs", x => x.AuditID);
                    table.ForeignKey(
                        name: "FK_DMS_AuditLogs_DMS_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "DMS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "DMS_Approvals",
                columns: table => new
                {
                    ApprovalID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentID = table.Column<long>(type: "bigint", nullable: false),
                    RevisionID = table.Column<long>(type: "bigint", nullable: false),
                    RequestID = table.Column<long>(type: "bigint", nullable: true),
                    WorkflowType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PerformedBy = table.Column<long>(type: "bigint", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReauthenticatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMS_Approvals", x => x.ApprovalID);
                    table.ForeignKey(
                        name: "FK_DMS_Approvals_DMS_Users_PerformedBy",
                        column: x => x.PerformedBy,
                        principalTable: "DMS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "DMS_Documents",
                columns: table => new
                {
                    DocumentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Plant = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaterialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaterialType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentRevisionID = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMS_Documents", x => x.DocumentID);
                    table.ForeignKey(
                        name: "FK_DMS_Documents_DMS_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "DMS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "DMS_Revisions",
                columns: table => new
                {
                    RevisionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentID = table.Column<long>(type: "bigint", nullable: false),
                    RevisionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileLocation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadedBy = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObsoleteAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMS_Revisions", x => x.RevisionID);
                    table.ForeignKey(
                        name: "FK_DMS_Revisions_DMS_Documents_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "DMS_Documents",
                        principalColumn: "DocumentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DMS_Revisions_DMS_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "DMS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "DMS_Requests",
                columns: table => new
                {
                    RequestID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentID = table.Column<long>(type: "bigint", nullable: false),
                    RevisionID = table.Column<long>(type: "bigint", nullable: false),
                    RequestedBy = table.Column<long>(type: "bigint", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<long>(type: "bigint", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ViewStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMS_Requests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_DMS_Requests_DMS_Documents_DocumentID",
                        column: x => x.DocumentID,
                        principalTable: "DMS_Documents",
                        principalColumn: "DocumentID");
                    table.ForeignKey(
                        name: "FK_DMS_Requests_DMS_Revisions_RevisionID",
                        column: x => x.RevisionID,
                        principalTable: "DMS_Revisions",
                        principalColumn: "RevisionID");
                    table.ForeignKey(
                        name: "FK_DMS_Requests_DMS_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "DMS_Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_DMS_Requests_DMS_Users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "DMS_Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Approvals_DocumentID",
                table: "DMS_Approvals",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Approvals_PerformedBy",
                table: "DMS_Approvals",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Approvals_RequestID",
                table: "DMS_Approvals",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Approvals_RevisionID",
                table: "DMS_Approvals",
                column: "RevisionID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_AuditLogs_ActionAt",
                table: "DMS_AuditLogs",
                column: "ActionAt");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_AuditLogs_ActionType",
                table: "DMS_AuditLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_AuditLogs_UserID",
                table: "DMS_AuditLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Documents_CreatedBy",
                table: "DMS_Documents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Documents_CurrentRevisionID",
                table: "DMS_Documents",
                column: "CurrentRevisionID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Documents_Plant_SerialNumber",
                table: "DMS_Documents",
                columns: new[] { "Plant", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Requests_ApprovedBy",
                table: "DMS_Requests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Requests_DocumentID",
                table: "DMS_Requests",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Requests_RequestedBy",
                table: "DMS_Requests",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Requests_RevisionID",
                table: "DMS_Requests",
                column: "RevisionID");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Revisions_DocumentID_RevisionNumber",
                table: "DMS_Revisions",
                columns: new[] { "DocumentID", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Revisions_UploadedBy",
                table: "DMS_Revisions",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DMS_Users_SalaryCode",
                table: "DMS_Users",
                column: "SalaryCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DMS_Approvals_DMS_Documents_DocumentID",
                table: "DMS_Approvals",
                column: "DocumentID",
                principalTable: "DMS_Documents",
                principalColumn: "DocumentID");

            migrationBuilder.AddForeignKey(
                name: "FK_DMS_Approvals_DMS_Requests_RequestID",
                table: "DMS_Approvals",
                column: "RequestID",
                principalTable: "DMS_Requests",
                principalColumn: "RequestID");

            migrationBuilder.AddForeignKey(
                name: "FK_DMS_Approvals_DMS_Revisions_RevisionID",
                table: "DMS_Approvals",
                column: "RevisionID",
                principalTable: "DMS_Revisions",
                principalColumn: "RevisionID");

            migrationBuilder.AddForeignKey(
                name: "FK_DMS_Documents_DMS_Revisions_CurrentRevisionID",
                table: "DMS_Documents",
                column: "CurrentRevisionID",
                principalTable: "DMS_Revisions",
                principalColumn: "RevisionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DMS_Revisions_DMS_Documents_DocumentID",
                table: "DMS_Revisions");

            migrationBuilder.DropTable(
                name: "DMS_Approvals");

            migrationBuilder.DropTable(
                name: "DMS_AuditLogs");

            migrationBuilder.DropTable(
                name: "DMS_Requests");

            migrationBuilder.DropTable(
                name: "DMS_Documents");

            migrationBuilder.DropTable(
                name: "DMS_Revisions");

            migrationBuilder.DropTable(
                name: "DMS_Users");
        }
    }
}
