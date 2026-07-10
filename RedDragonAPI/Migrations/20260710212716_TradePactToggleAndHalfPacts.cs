using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class TradePactToggleAndHalfPacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TradePactEnabled",
                table: "Kingdoms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TradePactSince",
                table: "Kingdoms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 21, 27, 16, 297, DateTimeKind.Utc).AddTicks(6163));

            // Konwersja danych: dotychczasowe pakty handlowe per-partner stają się
            // przełącznikiem udziału w handlu (włączony dla każdego, kto miał
            // jakikolwiek aktywny pakt handlowy), a rekordy Handlowy znikają.
            migrationBuilder.Sql(@"
                UPDATE Kingdoms SET TradePactEnabled = 1
                WHERE Id IN (
                    SELECT ProposerKingdomId FROM Pacts WHERE PactType = 'Handlowy' AND Status = 'Active'
                    UNION
                    SELECT TargetKingdomId FROM Pacts WHERE PactType = 'Handlowy' AND Status = 'Active'
                );
                DELETE FROM Pacts WHERE PactType = 'Handlowy';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TradePactEnabled",
                table: "Kingdoms");

            migrationBuilder.DropColumn(
                name: "TradePactSince",
                table: "Kingdoms");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 21, 16, 11, 636, DateTimeKind.Utc).AddTicks(8918));
        }
    }
}
