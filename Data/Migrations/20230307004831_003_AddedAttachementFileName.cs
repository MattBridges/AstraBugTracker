using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstraBugTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class _003_AddedAttachementFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TicketAttachment",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TicketAttachment");
        }
    }
}
