using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class DracopediaDefenseCalibration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 106,
                column: "Description",
                value: "Obrona +5%, straty ludności broniącej mniejsze o 20%");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 205,
                column: "Description",
                value: "Straty wojska w obronie mniejsze o 25%");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 206,
                column: "Description",
                value: "Podnosi siłę obrony księstwa o 10%");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0.10m, "Druga linia umocnień — obrona +10%" });

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0.15m, "Obrona +15% (także w paktach), straty ludności cywilnej −10%" });

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0m, "Po przełamaniu obrony kolejne ataki zabierają 6/6/6/4,5/3/1,5% ziemi zamiast 10/10/8/6/4/2%" });

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 606,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0m, "Cała ludność cywilna i złodzieje bronią księstwa ze współczynnikiem 2 (Goblin 3, Olbrzym 2,5, Br-Oug 1,5, Gnom 1)" });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 17, 41, 59, 771, DateTimeKind.Utc).AddTicks(7388));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 106,
                column: "Description",
                value: "Podstawowa obrona specjalna");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 205,
                column: "Description",
                value: "Leczenie rannych po walce");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 206,
                column: "Description",
                value: "Bonus obrony");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0.15m, "Silna obrona magiczna" });

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0.20m, "Potężna obrona" });

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0.25m, "Potężna obrona fortyfikacyjna" });

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 606,
                columns: new[] { "DefenseBonus", "Description" },
                values: new object[] { 0.30m, "Ludność walczy w obronie" });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 7, 3, 36, 46, 379, DateTimeKind.Utc).AddTicks(4550));
        }
    }
}
