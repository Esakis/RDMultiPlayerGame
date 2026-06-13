using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLabyrinth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LabyrinthDice",
                table: "Kingdoms",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "LabyrinthExpeditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    GeneralId = table.Column<int>(type: "int", nullable: true),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PendingGold = table.Column<long>(type: "bigint", nullable: false),
                    PendingFood = table.Column<long>(type: "bigint", nullable: false),
                    PendingStone = table.Column<long>(type: "bigint", nullable: false),
                    PendingWeapons = table.Column<long>(type: "bigint", nullable: false),
                    PendingMana = table.Column<long>(type: "bigint", nullable: false),
                    PendingDice = table.Column<int>(type: "int", nullable: false),
                    LastEvent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabyrinthExpeditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabyrinthExpeditions_Generals_GeneralId",
                        column: x => x.GeneralId,
                        principalTable: "Generals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LabyrinthExpeditions_Kingdoms_KingdomId",
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
                value: new DateTime(2026, 6, 13, 7, 16, 43, 702, DateTimeKind.Utc).AddTicks(1266));

            migrationBuilder.CreateIndex(
                name: "IX_LabyrinthExpeditions_GeneralId",
                table: "LabyrinthExpeditions",
                column: "GeneralId");

            migrationBuilder.CreateIndex(
                name: "IX_LabyrinthExpeditions_KingdomId_Status",
                table: "LabyrinthExpeditions",
                columns: new[] { "KingdomId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabyrinthExpeditions");

            migrationBuilder.DropColumn(
                name: "LabyrinthDice",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 6, 35, 51, 15, DateTimeKind.Utc).AddTicks(8311));
        }
    }
}
