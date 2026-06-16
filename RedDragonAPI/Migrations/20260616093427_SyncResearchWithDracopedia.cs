using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncResearchWithDracopedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 16, 9, 34, 26, 924, DateTimeKind.Utc).AddTicks(1398));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Podnosi wydajność manufaktur i wytwarzania maszyn bojowych o 10%.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Zwiększa szansę na przełom o 10% oraz wartość przełomu o 10%.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Dodaje jednorazowo 10 tur w chwili odkrycia (działa tylko do 10. dnia wieku księstwa).", "StartTurns", 10m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "EffectType", "EffectValue", "Level", "RequiredTech" },
                values: new object[] { "Dodaje jednorazowo dwukrotny dzienny limit tur (ok. 30, z Wieżami Czasu 34). Do odkrycia w dowolnym momencie bez aktywności wojennej.", "StartTurns", 30m, 1, null });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CostGold", "CostScience", "Description", "EffectValue", "Level", "RequiredTech", "ResearchTime" },
                values: new object[] { 15000, 3000000L, "Drugi poziom Osadnictwa. Obniża koszt zagospodarowania pustkowi o 1/3.", 0.20m, 2, "Osadnictwo", 15 });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CostGold", "CostScience", "Description", "EffectValue", "Level", "RequiredTech", "ResearchTime" },
                values: new object[] { 5000, 300000L, "Obniża koszt zakupu ziemi.", 0.10m, 1, null, 8 });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectType", "EffectValue", "RequiredTech" },
                values: new object[] { 1000000L, "Zastępuje chaotyczne przychody z kopalni stabilnym urobkiem (21% złota produkowanego przez alchemików).", "Górnictwo odkrywkowe", "MineGold", 0.21m, "Rekultywacja" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "Description",
                value: "O 12% podnosi limit smoków bez smokodrapu (do 53), zwiększa szansę na smoka i ilość smoków z labiryntu, skuteczność generała i akcji złodziejskiej ZS.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Description",
                value: "O 32% podnosi limit smoków bez smokodrapu (do 58) i wzmacnia pozostałe efekty smocze. Wymaga smokoastronomii.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "O 40% podnosi limit smoków bez smokodrapu (do 60) i wzmacnia pozostałe efekty smocze. Wymaga smokoastronomii i smokoanatomii.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "Obniża podatek przy kupnie jedzenia o 3 punkty procentowe (również przy braku SSH).");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 12,
                column: "Description",
                value: "Obniża podatek przy kupnie jedzenia i kamienia o 7 punktów procentowych. Wymaga rachunkowości.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 13,
                column: "Description",
                value: "Obniża podatek przy kupnie jedzenia, kamienia i broni o 10 punktów procentowych. Wymaga rachunkowości i buchalterii.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Obniża koszt jednostek E2 w broni o 5 i podnosi odzysk broni z Renowacji Broni o 5%.", "WeaponCostReduction", 5m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Obniża koszt jednostek E2 w broni o 15 i podnosi odzysk broni z Renowacji Broni o 15%. Wymaga ostrzenia broni.", "WeaponCostReduction", 15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Obniża koszt jednostek E2 w broni o 20 i E1 o 5, podnosi odzysk broni z Renowacji Broni o 20%. Wymaga ostrzenia i naprawy broni.", "WeaponCostReduction", 20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { "Zwiększa maksymalny limit punktów nauki na turę do 35 000.", "Wynalazczość prymitywna", "ScienceCap", 35000m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { "Zwiększa maksymalny limit punktów nauki na turę do 50 000.", "Wynalazczość podstawowa", "ScienceCap", 50000m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { "Zwiększa maksymalny limit punktów nauki na turę do 100 000.", "Wynalazczość rozwinięta", "ScienceCap", 100000m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { "Zwiększa maksymalny limit punktów nauki na turę do 125 000.", "Wynalazczość zaawansowana", "ScienceCap", 125000m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { "Zwiększa maksymalny limit punktów nauki na turę do 150 000.", "Wynalazczość nowoczesna", "ScienceCap", 150000m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę budynków specjalnych o 4,5%.", "Architektura prymitywna", 0.045m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę budynków specjalnych o 9%.", "Architektura podstawowa", 0.09m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Zmniejsza cenę budynków specjalnych o 15%.", "Architektura rozwinięta" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Budynki 3. rzędu nie kosztują złota niezależnie od czarnej magii; przyspieszanie budowy dodatkowo tańsze o 50%.", "Architektura zaawansowana", 0.15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Budynki 6. i 7. rzędu budują się o turę szybciej.", "Architektura nowoczesna", 0.15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę zabudowań w złocie o 8%.", "Inżynieria prymitywna", 0.08m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę zabudowań w złocie o 16%.", "Inżynieria podstawowa", 0.16m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę zabudowań w złocie o 24%.", "Inżynieria rozwinięta", 0.24m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectValue" },
                values: new object[] { 12000000L, "Murarze zużywają 10% mniej kamienia.", "Inżynieria zaawansowana", 0.24m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectValue" },
                values: new object[] { 12000000L, "Wyburzanie budynków zwraca 80% budulca.", "Inżynieria nowoczesna", 0.24m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 4,5%.", "Czarodziejstwo prymitywne", 0.045m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 9%.", "Czarodziejstwo podstawowe", 0.09m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 15%.", "Czarodziejstwo rozwinięte", 0.15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 21%.", "Czarodziejstwo zaawansowane", 0.21m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 30%.", "Czarodziejstwo nowoczesne", 0.30m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Przyspiesza szkolenie wojska (poziom 1 z 5).", "Trening prymitywny" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Przyspiesza szkolenie wojska (poziom 2 z 5).", "Trening podstawowy" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Przyspiesza szkolenie wojska (poziom 3 z 5).", "Trening rozwinięty" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Przyspiesza szkolenie wojska (poziom 4 z 5).", "Trening zaawansowany" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Przyspiesza szkolenie wojska (poziom 5 z 5).", "Trening nowoczesny" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Zmniejsza cenę złodziei o 5%.", "Rekrutacja prymitywna" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Zmniejsza cenę złodziei o 10%.", "Rekrutacja podstawowa" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Zmniejsza cenę złodziei o 20%.", "Rekrutacja rozwinięta", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "CostScience", "Description", "DisplayName" },
                values: new object[] { 12000000L, "Umożliwia kradzież zapasów z karawan.", "Rekrutacja zaawansowana" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectValue" },
                values: new object[] { 12000000L, "Ranni generałowie zachowują poziom równy lvl/3.", "Rekrutacja nowoczesna", 0.20m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 15, 13, 12, 44, 985, DateTimeKind.Utc).AddTicks(8587));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Odblokowanie machin wojennych");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Bonus do efektywności naukowców");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "+1 tura dziennie", "BonusTurns", 1.0m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "EffectType", "EffectValue", "Level", "RequiredTech" },
                values: new object[] { "+1 dodatkowa tura dziennie", "BonusTurns", 1.0m, 2, "ZakrzywCzasu" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CostGold", "CostScience", "Description", "EffectValue", "Level", "RequiredTech", "ResearchTime" },
                values: new object[] { 5000, 300000L, "Tańsze kupowanie ziemi", 0.10m, 1, null, 8 });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CostGold", "CostScience", "Description", "EffectValue", "Level", "RequiredTech", "ResearchTime" },
                values: new object[] { 15000, 3000000L, "Jeszcze tańsza ziemia", 0.20m, 2, "Rekultywacja", 15 });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectType", "EffectValue", "RequiredTech" },
                values: new object[] { 6000000L, "Bonus do kamienia", "Górnictwo Odkrywkowe", "StoneBonus", 0.30m, "Osadnictwo" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "Description",
                value: "Podstawowa wiedza o smokach");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Description",
                value: "Znajomość anatomii smoków");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Description",
                value: "Pełna kontrola nad smokami");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 11,
                column: "Description",
                value: "Bonus do złota z kupców");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 12,
                column: "Description",
                value: "Większy bonus do handlu");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 13,
                column: "Description",
                value: "Maksymalny bonus handlowy");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Bonus do ataku", "AttackBonus", 0.10m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Większy bonus do ataku", "AttackBonus", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Description", "EffectType", "EffectValue" },
                values: new object[] { "Maksymalny bonus do ataku", "AttackBonus", 0.30m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { "Bonus do produkcji i wyższy limit SP/turę", "Wynalazczość I", "ProductionBonus", 0.05m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { null, "Wynalazczość II", "ProductionBonus", 0.10m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { null, "Wynalazczość III", "ProductionBonus", 0.15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { null, "Wynalazczość IV", "ProductionBonus", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Description", "DisplayName", "EffectType", "EffectValue" },
                values: new object[] { null, "Wynalazczość V", "ProductionBonus", 0.25m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Tańsze budynki specjalne", "Architektura I", 0.05m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Architektura II", 0.10m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { null, "Architektura III" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Brak kosztu złota pod czarną magią + 50% taniej przyspieszanie", "Architektura IV", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Architektura V", 0.25m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Tańsze budynki gospodarcze", "Inżynieria I", 0.05m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Inżynieria II", 0.10m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Inżynieria III", 0.15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectValue" },
                values: new object[] { 15000000L, null, "Inżynieria IV", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectValue" },
                values: new object[] { 21000000L, null, "Inżynieria V", 0.25m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { "Tańsze rzucanie zaklęć", "Czarodziejstwo I", 0.10m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Czarodziejstwo II", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Czarodziejstwo III", 0.30m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Czarodziejstwo IV", 0.40m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Czarodziejstwo V", 0.50m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Szybsze szkolenie wojsk", "Trening I" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { null, "Trening II" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { null, "Trening III" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { null, "Trening IV" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { null, "Trening V" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { "Tańsza rekrutacja złodziei", "Rekrutacja I" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Description", "DisplayName" },
                values: new object[] { null, "Rekrutacja II" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Description", "DisplayName", "EffectValue" },
                values: new object[] { null, "Rekrutacja III", 0.15m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "CostScience", "Description", "DisplayName" },
                values: new object[] { 15000000L, null, "Rekrutacja IV" });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "CostScience", "Description", "DisplayName", "EffectValue" },
                values: new object[] { 21000000L, null, "Rekrutacja V", 0.25m });
        }
    }
}
