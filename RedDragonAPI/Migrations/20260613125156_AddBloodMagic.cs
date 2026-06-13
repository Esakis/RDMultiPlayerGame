using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodMagic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BloodElixirAttack",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BloodElixirBloodlust",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BloodElixirFocus",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BloodElixirThief",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "BloodPoints",
                table: "Kingdoms",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 12, 51, 55, 562, DateTimeKind.Utc).AddTicks(9325));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodElixirAttack",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "BloodElixirBloodlust",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "BloodElixirFocus",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "BloodElixirThief",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "BloodPoints",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 12, 49, 13, 299, DateTimeKind.Utc).AddTicks(3277));
        }
    }
}
