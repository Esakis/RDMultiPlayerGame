using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class RewriteLabyrinthOriginal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabyrinthExpeditions");

            migrationBuilder.DropColumn(
                name: "LabyrinthDice",
                table: "Kingdoms");

            migrationBuilder.AddColumn<int>(
                name: "LabyrinthActionsUsed",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[] { 706, 200000, 0, 7, "SanktuariumStworcy", "Specjalne", 6, 1, 0, 0, 0m, "Podwaja budżet akcji w labiryncie (2 skarby lub 4 akcje generała na przeliczenie)", "Sanktuarium Stwórcy", true, 0, 0m, "PospoliteRuszenie", null, 7, 0 });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 17, 38, 18, 91, DateTimeKind.Utc).AddTicks(6298));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 706);

            migrationBuilder.DropColumn(
                name: "LabyrinthActionsUsed",
                table: "Kingdoms");

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
                    GeneralId = table.Column<int>(type: "int", nullable: true),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false),
                    LastEvent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PendingDice = table.Column<int>(type: "int", nullable: false),
                    PendingFood = table.Column<long>(type: "bigint", nullable: false),
                    PendingGold = table.Column<long>(type: "bigint", nullable: false),
                    PendingMana = table.Column<long>(type: "bigint", nullable: false),
                    PendingStone = table.Column<long>(type: "bigint", nullable: false),
                    PendingWeapons = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabyrinthExpeditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabyrinthExpeditions_Generals_GeneralId",
                        column: x => x.GeneralId,
                        principalTable: "Generals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                value: new DateTime(2026, 6, 17, 11, 5, 9, 510, DateTimeKind.Utc).AddTicks(8978));

            migrationBuilder.CreateIndex(
                name: "IX_LabyrinthExpeditions_GeneralId",
                table: "LabyrinthExpeditions",
                column: "GeneralId");

            migrationBuilder.CreateIndex(
                name: "IX_LabyrinthExpeditions_KingdomId_Status",
                table: "LabyrinthExpeditions",
                columns: new[] { "KingdomId", "Status" });
        }
    }
}
