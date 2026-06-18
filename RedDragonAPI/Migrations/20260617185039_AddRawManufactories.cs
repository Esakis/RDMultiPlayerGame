using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRawManufactories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[,]
                {
                    { 16, 0, 0, 1, "Kamieniolom", "Manufaktury", 0, 1, 300, 1, 0m, "Automatycznie wydobywa kamień bez pracowników", "Kamieniołom", false, 0, 0m, null, null, 0, 0 },
                    { 17, 0, 0, 1, "KopalniaDiamentow", "Manufaktury", 0, 1, 300, 1, 0m, "Automatycznie wydobywa złoto bez pracowników", "Diamentowa kopalnia", false, 0, 0m, null, null, 0, 0 },
                    { 18, 0, 0, 1, "ManoweJeziorko", "Manufaktury", 0, 1, 300, 1, 0m, "Automatycznie produkuje manę bez pracowników", "Manowe jeziorko", false, 0, 0m, null, null, 0, 0 }
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 18, 50, 38, 990, DateTimeKind.Utc).AddTicks(9695));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 17, 18, 43, 59, 856, DateTimeKind.Utc).AddTicks(5807));
        }
    }
}
