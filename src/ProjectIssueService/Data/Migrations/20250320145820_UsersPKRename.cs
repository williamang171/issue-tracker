using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectIssueService.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsersPKRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "Users",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "Guid");
        }
    }
}
