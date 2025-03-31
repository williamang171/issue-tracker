using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectIssueService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Attachments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Attachments");
        }
    }
}
