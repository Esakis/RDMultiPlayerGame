using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniwersytetyBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[] { 15, 0, 0, 1, "Uniwersytety", "Cechy", 0, 1, 200, 1, 0m, "Zwiększa produktywność naukowców (bonus rośnie z liczbą uniwersytetów)", "Uniwersytety", false, 0, 0.05m, null, null, 0, 0 });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 18, 43, 59, 856, DateTimeKind.Utc).AddTicks(5807));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 17, 53, 36, 242, DateTimeKind.Utc).AddTicks(5898));
        }
    }
}
