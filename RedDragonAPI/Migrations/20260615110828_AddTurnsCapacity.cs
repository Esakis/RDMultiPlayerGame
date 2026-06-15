using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnsCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TurnsCapacity",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Istniejące królestwa: przydział cyklu = aktualnie dostępne tury (licznik startuje od 0 zużytych)
            migrationBuilder.Sql("UPDATE [Kingdoms] SET [TurnsCapacity] = [TurnsAvailable];");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 15, 11, 8, 26, 763, DateTimeKind.Utc).AddTicks(847));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurnsCapacity",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 12, 59, 55, 603, DateTimeKind.Utc).AddTicks(6292));
        }
    }
}
