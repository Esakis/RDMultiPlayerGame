using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class LabyrinthDoubleEntryTavernAndExplorer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 706);

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 101,
                column: "Description",
                value: "Obniża wymaganą pensję do 42 dla 100% popularności; pozwala wejść 2× do labiryntu na przeliczenie");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 17, 53, 36, 242, DateTimeKind.Utc).AddTicks(5898));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 101,
                column: "Description",
                value: "Obniża wymaganą pensję do 42 dla 100% popularności");

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
    }
}
