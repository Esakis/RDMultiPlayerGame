using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn7SpecialBuildings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[,]
                {
                    { 107, 500, 0, 1, "Komando", "Specjalne", 7, 1, 0, 0, 0m, "Oddziały szybkiego reagowania — straty cywilów przy wrogich atakach mniejsze o 20%", "Komando", true, 0, 0m, null, null, 1, 0 },
                    { 207, 5000, 0, 2, "NawiedzonyLas", "Specjalne", 7, 1, 0, 0, 0m, "Klątwa na przygranicznych lasach — 5% armii inwazyjnej ucieka przed walką", "Nawiedzony las", true, 0, 0m, "Komando", null, 2, 0 },
                    { 307, 50000, 0, 4, "AmbulatoriumPolowe", "Specjalne", 7, 1, 0, 0, 0m, "Mobilny lazaret — straty własnej armii w ataku mniejsze o 50%", "Ambulatorium polowe", true, 0, 0m, "NawiedzonyLas", null, 3, 0 },
                    { 407, 50000, 0, 4, "Zamtuz", "Specjalne", 7, 1, 0, 0, 0m, "+25% przyrostu nowych poddanych (nie zwiększa pojemności księstwa)", "Zamtuz pod Smoczym Ogonem", true, 0, 0m, "AmbulatoriumPolowe", null, 4, 0 },
                    { 607, 85000, 0, 5, "Palac", "Specjalne", 7, 1, 0, 0, 0m, "+1 ataku dla elity 2. stopnia; ranni generałowie wracają do sił 2× szybciej", "Pałac", true, 0, 0m, "PortTowarowy", null, 6, 0 }
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 6, 22, 45, 6, 470, DateTimeKind.Utc).AddTicks(5386));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 307);

            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 407);

            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 607);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 2, 15, 21, 40, 697, DateTimeKind.Utc).AddTicks(6741));
        }
    }
}
