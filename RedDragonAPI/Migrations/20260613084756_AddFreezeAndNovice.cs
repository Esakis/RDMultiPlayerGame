using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFreezeAndNovice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FrozenAt",
                table: "Kingdoms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 8, 47, 55, 782, DateTimeKind.Utc).AddTicks(2183));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrozenAt",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 8, 41, 8, 656, DateTimeKind.Utc).AddTicks(3831));
        }
    }
}
