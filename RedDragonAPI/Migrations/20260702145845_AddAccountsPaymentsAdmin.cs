using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsPaymentsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveKingdomId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Kingdoms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    KingdomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 2, 14, 58, 43, 599, DateTimeKind.Utc).AddTicks(1700));

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            // Backfill istniejących kont: rola gracza, aktywne księstwo = pierwsze
            // księstwo konta, pierwsze księstwo każdego konta jest darmowe.
            migrationBuilder.Sql(@"
UPDATE Users SET Role = 'Player' WHERE Role = '' OR Role IS NULL;

UPDATE u SET u.ActiveKingdomId = k.Id
FROM Users u
CROSS APPLY (SELECT TOP 1 Id FROM Kingdoms WHERE UserId = u.Id ORDER BY Id) k
WHERE u.ActiveKingdomId IS NULL;

WITH firstK AS (SELECT Id, ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY Id) rn FROM Kingdoms)
UPDATE k SET k.IsFree = 1 FROM Kingdoms k JOIN firstK f ON f.Id = k.Id WHERE f.rn = 1;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSettings");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "ActiveKingdomId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 1, 22, 31, 26, 227, DateTimeKind.Utc).AddTicks(2617));
        }
    }
}
