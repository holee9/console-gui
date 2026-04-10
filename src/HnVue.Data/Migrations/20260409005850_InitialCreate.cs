using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HnVue.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    EntryId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    TimestampTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    TimestampOffsetMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CurrentHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.EntryId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DateOfBirth = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Sex = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    IsEmergency = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtOffsetMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "UpdateHistories",
                columns: table => new
                {
                    UpdateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FromVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ToVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    InstalledBy = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PackageHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateHistories", x => x.UpdateId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RoleValue = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAtTicks = table.Column<long>(type: "INTEGER", nullable: true),
                    LastLoginAtOffsetMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    QuickPinHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    QuickPinFailedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    QuickPinLockedUntilTicks = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Studies",
                columns: table => new
                {
                    StudyInstanceUid = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PatientId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    StudyDateTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    StudyDateOffsetMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AccessionNumber = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BodyPart = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studies", x => x.StudyInstanceUid);
                    table.ForeignKey(
                        name: "FK_Studies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoseRecords",
                columns: table => new
                {
                    DoseId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    StudyInstanceUid = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Dap = table.Column<double>(type: "REAL", nullable: false),
                    Ei = table.Column<double>(type: "REAL", nullable: false),
                    EffectiveDose = table.Column<double>(type: "REAL", nullable: false),
                    BodyPart = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RecordedAtTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    RecordedAtOffsetMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoseRecords", x => x.DoseId);
                    table.ForeignKey(
                        name: "FK_DoseRecords_Studies_StudyInstanceUid",
                        column: x => x.StudyInstanceUid,
                        principalTable: "Studies",
                        principalColumn: "StudyInstanceUid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    StudyInstanceUid = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    AcquiredAtTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    AcquiredAtOffsetMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_Images_Studies_StudyInstanceUid",
                        column: x => x.StudyInstanceUid,
                        principalTable: "Studies",
                        principalColumn: "StudyInstanceUid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TimestampTicks",
                table: "AuditLogs",
                column: "TimestampTicks");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoseRecords_StudyInstanceUid",
                table: "DoseRecords",
                column: "StudyInstanceUid");

            migrationBuilder.CreateIndex(
                name: "IX_Images_StudyInstanceUid",
                table: "Images",
                column: "StudyInstanceUid");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Name",
                table: "Patients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Studies_PatientId",
                table: "Studies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_UpdateHistories_FromVersion",
                table: "UpdateHistories",
                column: "FromVersion");

            migrationBuilder.CreateIndex(
                name: "IX_UpdateHistories_Timestamp",
                table: "UpdateHistories",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UpdateHistories_ToVersion",
                table: "UpdateHistories",
                column: "ToVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DoseRecords");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "UpdateHistories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Studies");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
