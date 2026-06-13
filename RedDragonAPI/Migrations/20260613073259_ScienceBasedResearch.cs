using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class ScienceBasedResearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CostScience",
                table: "TechnologyDefinitions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "InvestedScience",
                table: "Research",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CurrentResearchTech",
                table: "Kingdoms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SciencePoints",
                table: "Kingdoms",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 7, 32, 58, 862, DateTimeKind.Utc).AddTicks(2481));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CostScience",
                value: 2000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CostScience",
                value: 5000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CostScience",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CostScience",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CostScience",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CostScience",
                value: 6000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CostScience",
                value: 2000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CostScience",
                value: 4000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CostScience",
                value: 2000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CostScience",
                value: 4000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CostScience",
                value: 6000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CostScience",
                value: 2000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CostScience",
                value: 4000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CostScience",
                value: 6000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CostScience", "Description" },
                values: new object[] { 300000L, "Bonus do produkcji i wyższy limit SP/turę" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CostScience",
                value: 9000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CostScience",
                value: 15000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CostScience",
                value: 21000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CostScience",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CostScience",
                value: 9000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CostScience",
                value: 15000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CostScience",
                value: 21000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CostScience",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CostScience",
                value: 9000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CostScience",
                value: 15000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CostScience",
                value: 21000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "CostScience", "Description" },
                values: new object[] { 300000L, "Tańsze rzucanie zaklęć" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CostScience",
                value: 9000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CostScience",
                value: 15000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CostScience",
                value: 21000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CostScience",
                value: 300000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CostScience",
                value: 9000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CostScience",
                value: 15000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CostScience",
                value: 21000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "CostScience", "Description" },
                values: new object[] { 300000L, "Tańsza rekrutacja złodziei" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CostScience",
                value: 3000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CostScience",
                value: 9000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CostScience",
                value: 15000000L);

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CostScience",
                value: 21000000L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostScience",
                table: "TechnologyDefinitions");

            migrationBuilder.DropColumn(
                name: "InvestedScience",
                table: "Research");

            migrationBuilder.DropColumn(
                name: "CurrentResearchTech",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "SciencePoints",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 7, 16, 43, 702, DateTimeKind.Utc).AddTicks(1266));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 17,
                column: "Description",
                value: "Bonus do produkcji");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 32,
                column: "Description",
                value: "Bonus do mocy czarów");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 42,
                column: "Description",
                value: "Tańsza rekrutacja");
        }
    }
}
