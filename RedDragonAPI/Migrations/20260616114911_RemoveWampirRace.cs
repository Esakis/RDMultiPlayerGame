using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWampirRace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Zabezpieczenie istniejących danych: rasa Wampir znika z gry.
            // Ewentualne księstwa-Wampiry przechodzą na Człowieka, a ich jednostki
            // Wampira są kasowane (FK MilitaryUnits→UnitDefinitions ma OnDelete: Restrict).
            migrationBuilder.Sql("DELETE FROM MilitaryUnits WHERE UnitType LIKE 'Wampir_%';");
            migrationBuilder.Sql("UPDATE Kingdoms SET Race = N'Człowiek' WHERE Race = N'Wampir';");

            migrationBuilder.DeleteData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 96);

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
                value: new DateTime(2026, 6, 16, 11, 49, 10, 861, DateTimeKind.Utc).AddTicks(4412));

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "Description",
                value: "Upija żołnierzy wroga — nie bronią w następnym przeliczeniu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                value: new DateTime(2026, 6, 16, 10, 30, 32, 383, DateTimeKind.Utc).AddTicks(6800));

            migrationBuilder.InsertData(
                table: "RaceDefinitions",
                columns: new[] { "Id", "AqueductAcreBonus", "AttackRating", "BonusAlchemists", "BonusArmorers", "BonusDruids", "BonusFarmers", "BonusMages", "BonusMasons", "BonusMerchants", "BonusScientists", "BonusStonemasons", "BurrowsHouseBonus", "DefenseRating", "Description", "E1Attack", "E1Defense", "E2Attack", "E2Defense", "EaseRating", "EconomyRating", "FoodPerPop", "GeneralsLimit", "HouseCapacityBase", "LimitedSpellsPerRecalc", "MachineAttack", "MagicBooks", "MagicRating", "MilitaryLossModifier", "Name", "NameCz", "PopGrowthModifier", "PopPerAcreBase", "ResearchModifier", "SewersHouseBonus", "SpecialTraits", "ThiefCostModifier", "ThiefPowerModifier", "ThievesRating", "TurnsPerDay", "WaterworksHouseBonus" },
                values: new object[] { 9, 0.5m, 90, 0.20m, 0m, 0m, 0m, 0.20m, 0m, 0m, 0m, 0m, 1m, 80, "Jednostki wojskowe są naszą najsilniejszą stroną. Jak tylko posmakują krwi wrogów, zmieniają się w żądne krwi bestie nie do zatrzymania. Także nasi złodzieje są uważani za jednych z najlepszych w tej profesji.", 4, 2, 10, 5, 60, 40, 1m, 8, 3m, 5, 5, 3, 85, 0m, "Wampir", "Vampýr", 0m, 3m, 0m, 1.5m, "Armia nie je (nie umiera w głodzie); odporny na Zarazę; Głupota/Somnambulizm/Ospałość o połowę tańsze; 25% upitych żołnierzy wroga umiera; mechanika Krwawa magia (punkty krwi za zabitych wrogów odblokowują eliksiry: złodziei +5%/lvl, ataku +7%/lvl, magów +3%/lvl, strat +12,5%/lvl).", 0m, 0.10m, 90, 15, 0.5m });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "Description",
                value: "Upija żołnierzy wroga — nie bronią w następnym przeliczeniu (Wampir: 25% upitych umiera)");

            migrationBuilder.InsertData(
                table: "UnitDefinitions",
                columns: new[] { "Id", "AttackPower", "CostFood", "CostGold", "CostWeapons", "DefensePower", "Description", "DisplayName", "Race", "RequiredBuilding", "RequiredTech", "TrainingTime", "UnitType", "Upkeep" },
                values: new object[,]
                {
                    { 91, 1, 0, 200, 2, 1, "Podstawowy żołnierz (nie je)", "Hoplita", "Wampir", "", null, 1, "Wampir_Hoplita", 0 },
                    { 92, 4, 0, 700, 20, 2, "Elita 1. stopnia", "Upiór", "Wampir", "OltarzInicjacji", null, 1, "Wampir_Upior", 0 },
                    { 93, 10, 0, 1800, 160, 5, "Elita 2. stopnia", "Nosferatu", "Wampir", "KoszarySpecjalne", null, 1, "Wampir_Nosferatu", 0 },
                    { 94, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Wampir", "KonstrukcjaMachin", null, 1, "Wampir_Machina", 0 },
                    { 95, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Wampir", "GildiaZlodziei", null, 1, "Wampir_Zlodziej", 0 },
                    { 96, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Wampir", "Smokodrap", null, 1, "Wampir_Smok", 0 }
                });
        }
    }
}
