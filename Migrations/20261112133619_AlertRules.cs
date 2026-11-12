using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry_Auth_API.Migrations
{
    /// <inheritdoc />
    public partial class AlertRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramChatId = table.Column<string>(type: "text", nullable: true),
                    Rules = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "AlertRules" ("Id", "UserId", "TelegramChatId", "Rules")
                SELECT gen_random_uuid(), "Id", "TelegramChatId", ARRAY[]::text[]
                FROM "Users"
                WHERE "TelegramChatId" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_UserId",
                table: "AlertRules",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramChatId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Users" AS u
                SET "TelegramChatId" = a."TelegramChatId"
                FROM "AlertRules" AS a
                WHERE a."UserId" = u."Id";
                """);

            migrationBuilder.DropTable(
                name: "AlertRules");
        }
    }
}
