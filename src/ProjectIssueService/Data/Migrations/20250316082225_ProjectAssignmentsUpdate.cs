using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectIssueService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProjectAssignmentsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "ProjectAssignments",
                newName: "UserName");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectAssignments_ProjectId_Username",
                table: "ProjectAssignments",
                newName: "IX_ProjectAssignments_ProjectId_UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "ProjectAssignments",
                newName: "Username");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectAssignments_ProjectId_UserName",
                table: "ProjectAssignments",
                newName: "IX_ProjectAssignments_ProjectId_Username");
        }
    }
}
