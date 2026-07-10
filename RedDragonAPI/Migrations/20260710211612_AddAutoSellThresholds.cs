using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoSellThresholds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AutoSellFoodAbove",
                table: "Kingdoms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AutoSellManaAbove",
                table: "Kingdoms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AutoSellStoneAbove",
                table: "Kingdoms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AutoSellWeaponsAbove",
                table: "Kingdoms",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 21, 16, 11, 636, DateTimeKind.Utc).AddTicks(8918));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSellFoodAbove",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "AutoSellManaAbove",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "AutoSellStoneAbove",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "AutoSellWeaponsAbove",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 17, 56, 52, 21, DateTimeKind.Utc).AddTicks(4915));
        }
    }
}
