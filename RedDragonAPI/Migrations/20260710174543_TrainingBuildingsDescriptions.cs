using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class TrainingBuildingsDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 505,
                column: "Description",
                value: "+5 p.p. awansu E1→E2 na turę (Olbrzym 6, Goblin 4,5); podwaja też szansę przyjścia generała");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 605,
                column: "Description",
                value: "Umożliwia rekrutację E2; przy włączonym szkoleniu +10 p.p. awansu hoplitów do E1 na turę");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 17, 45, 43, 16, DateTimeKind.Utc).AddTicks(6088));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 505,
                column: "Description",
                value: "Bonus do siły armii");

            migrationBuilder.UpdateData(
                table: "BuildingDefinitions",
                keyColumn: "Id",
                keyValue: 605,
                column: "Description",
                value: "Elitarne jednostki wojskowe");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 17, 41, 59, 771, DateTimeKind.Utc).AddTicks(7388));
        }
    }
}
