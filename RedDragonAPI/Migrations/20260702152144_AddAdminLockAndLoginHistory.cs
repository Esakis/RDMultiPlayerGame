using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminLockAndLoginHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminLocked",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "KingdomLogins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KingdomLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KingdomLogins_Kingdoms_KingdomId",
                        column: x => x.KingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 2, 15, 21, 40, 697, DateTimeKind.Utc).AddTicks(6741));

            migrationBuilder.CreateIndex(
                name: "IX_KingdomLogins_KingdomId",
                table: "KingdomLogins",
                column: "KingdomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KingdomLogins");

            migrationBuilder.DropColumn(
                name: "AdminLocked",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 2, 14, 58, 43, 599, DateTimeKind.Utc).AddTicks(1700));
        }
    }
}
