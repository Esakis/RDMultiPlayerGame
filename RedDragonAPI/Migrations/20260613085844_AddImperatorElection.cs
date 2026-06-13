using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddImperatorElection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImperatorVoteForKingdomId",
                table: "Kingdoms",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 44, 354, DateTimeKind.Utc).AddTicks(8927));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImperatorVoteForKingdomId",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 8, 47, 55, 782, DateTimeKind.Utc).AddTicks(2183));
        }
    }
}
