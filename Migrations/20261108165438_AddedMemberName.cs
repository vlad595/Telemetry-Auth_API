using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry_Auth_API.Migrations
{
    /// <inheritdoc />
    public partial class AddedMemberName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "WorkspaceMember",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "WorkspaceMember");
        }
    }
}
