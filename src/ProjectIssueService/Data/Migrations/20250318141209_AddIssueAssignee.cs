using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectIssueService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Assignee",
                table: "Issues",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assignee",
                table: "Issues");
        }
    }
}
