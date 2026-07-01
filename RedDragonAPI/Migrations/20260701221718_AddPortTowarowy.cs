using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPortTowarowy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[] { 507, 85000, 0, 5, "PortTowarowy", "Specjalne", 7, 1, 0, 0, 0m, "400–600 tys. złota na turę, w czasie wojny podwójnie", "Port towarowy", true, 0, 0m, "SkrzyzowanieSzlakow", null, 5, 0 });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 1, 22, 17, 17, 317, DateTimeKind.Utc).AddTicks(4270));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 507);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 1, 22, 13, 54, 917, DateTimeKind.Utc).AddTicks(9604));
        }
    }
}
