using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRacialMechanics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArcherCommandoTargetId",
                table: "Kingdoms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HodokvasActive",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HodokvasTurnsPlayed",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RearmE1Attack",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RearmE1Defense",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RearmE2Attack",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RearmE2Defense",
                table: "Kingdoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 7, 3, 36, 46, 379, DateTimeKind.Utc).AddTicks(4550));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArcherCommandoTargetId",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "HodokvasActive",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "HodokvasTurnsPlayed",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "RearmE1Attack",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "RearmE1Defense",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "RearmE2Attack",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "RearmE2Defense",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 7, 3, 25, 23, 116, DateTimeKind.Utc).AddTicks(8943));
        }
    }
}
