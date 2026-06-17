using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAmbasadaBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[] { 705, 30000, 0, 3, "Ambasada", "Specjalne", 5, 1, 0, 0, 0m, "Zwiększa limit paktów obronnych o 1 (5 → 6)", "Ambasada", true, 0, 0m, null, null, 7, 0 });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 11, 5, 9, 510, DateTimeKind.Utc).AddTicks(8978));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 705);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 16, 12, 27, 43, 106, DateTimeKind.Utc).AddTicks(2920));
        }
    }
}
