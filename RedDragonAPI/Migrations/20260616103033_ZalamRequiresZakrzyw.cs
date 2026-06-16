using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class ZalamRequiresZakrzyw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 16, 10, 30, 32, 383, DateTimeKind.Utc).AddTicks(6800));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Level", "RequiredTech" },
                values: new object[] { "Dodaje jednorazowo dwukrotny dzienny limit tur (ok. 30, z Wieżami Czasu 34). Wymaga zakrzywienia czasu.", 2, "ZakrzywCzasu" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 16, 10, 27, 15, 11, DateTimeKind.Utc).AddTicks(9539));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Level", "RequiredTech" },
                values: new object[] { "Dodaje jednorazowo dwukrotny dzienny limit tur (ok. 30, z Wieżami Czasu 34). Do odkrycia w dowolnym momencie bez aktywności wojennej.", 1, null });
        }
    }
}
