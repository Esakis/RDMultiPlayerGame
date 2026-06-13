using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class PolishManualSpellsAndRaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 12, 20, 24, 13, 343, DateTimeKind.Utc).AddTicks(2100));

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 5, 3 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 5, 4 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                column: "LimitedSpellsPerRecalc",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                column: "LimitedSpellsPerRecalc",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 5, 4 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 6,
                column: "LimitedSpellsPerRecalc",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 3, 1 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "LimitedSpellsPerRecalc",
                value: 6);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 9,
                column: "LimitedSpellsPerRecalc",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks", "SpecialTraits" },
                values: new object[] { 5, 1, "Jedzenie 2/mieszkańca (PL: 1,5); limitowane zaklęcia działają na nich do 4× za przeliczenie; +25% burzenia machin; E1 burzy 0,1, E2 burzy 0,5 (nie blokują wież); odporny na Zarazę (PL); nie może mieć złodziei — Gildia Wojowników zamiast Gildii Złodziei (+1 atak/+2 obrona E2); 8 generałów (PL); mechanika Szamanizm (totemy: Grabieży / Smokobójstwa / Niszczycielstwa)." });

            migrationBuilder.InsertData(
                table: "RaceDefinitions",
                columns: new[] { "Id", "AqueductAcreBonus", "AttackRating", "BonusAlchemists", "BonusArmorers", "BonusDruids", "BonusFarmers", "BonusMages", "BonusMasons", "BonusMerchants", "BonusScientists", "BonusStonemasons", "BurrowsHouseBonus", "DefenseRating", "Description", "E1Attack", "E1Defense", "E2Attack", "E2Defense", "EaseRating", "EconomyRating", "FoodPerPop", "GeneralsLimit", "HouseCapacityBase", "LimitedSpellsPerRecalc", "MachineAttack", "MagicBooks", "MagicRating", "MilitaryLossModifier", "Name", "NameCz", "PopGrowthModifier", "PopPerAcreBase", "ResearchModifier", "SewersHouseBonus", "SpecialTraits", "ThiefCostModifier", "ThiefPowerModifier", "ThievesRating", "TurnsPerDay", "WaterworksHouseBonus" },
                values: new object[,]
                {
                    { 11, 0.5m, 55, 0.10m, 0m, 0m, 0m, 0m, -0.05m, 0m, 0m, -0.05m, 2m, 60, "Gnomy słyną z alchemii — zamiast krwi w żyłach płynie im złoto. Ich saperzy potrafią wysadzić w powietrze całe oddziały, za to machin wojennych nie używają wcale. Po wybudowaniu Łaźni i Systemu nor ich domki robią się zadziwiająco pojemne.", 1, 5, 8, 7, 70, 75, 1m, 6, 3m, 5, 0, 3, 70, 0m, "Gnom", "Tryton (trytoni.php)", 0m, 3m, 0m, 1.5m, "Nie używa machin wojennych (odporny na Chochliki); złodziej kosztuje 1500 złota; drożenie zaklęć +11% (zamiast 10%); Łaźnia +1 do domu, System nor +2; wyszkolone E1 dają dodatkowo 0,5 obrony złodziejskiej; saperzy: dodatkowi zabici = liczba saperów/3 (max 150%).", 0m, 0m, 75, 15, 1m },
                    { 12, 2.5m, 80, -0.25m, -0.20m, -0.20m, -0.20m, -0.30m, -0.25m, 0m, 0m, -0.20m, 1m, 60, "Prastara rasa o ogromnej płodności — na jednym akrze gnieździ się ich więcej niż przedstawicieli jakiejkolwiek innej rasy. Ich machiny wojenne sieją postrach (8 ataku), ale budowle stawiają niechętnie i drogo.", 2, 2, 5, 6, 50, 45, 2m, 6, 1m, 5, 8, 3, 60, 0m, "Br-Oug", "Br-Oug", 0m, 7m, 0m, 1.5m, "+4 mieszkańców/akr (dom mieści tylko 1); Akwedukt daje +2,5/akr; je 2 jedzenia/mieszkańca; budynki o 50% droższe, ale podwójny limit infrapunktów; machiny 8 ataku (z E1: 6), z hoplitami burzą o 40% słabiej; wieże obronne słabsze o 33% (blokują 10 machin, niszczą 2); Zdjęcie zaklęcia o 50% droższe; domobrana broni z siłą 1,5.", 0m, -0.20m, 55, 15, 0.5m }
                });

            migrationBuilder.InsertData(
                table: "SpellDefinitions",
                columns: new[] { "Id", "Category", "Description", "DisplayName", "EffectType", "IsLimited", "ManaCost", "PowerLevel", "RequiredBooks", "RequiredRace", "SpellType", "TargetType" },
                values: new object[,]
                {
                    { 1, "Pozostałe", "Pokazuje podstawowe informacje o wrogim księstwie (E2, E1, hoplici, złodzieje, magowie, machiny). Rzucone minimalną siłą służy jako sonda obrony magicznej.", "Sokole Oko", "EagleEye", false, 20, 1, 0, null, "SokoleOko", "Enemy" },
                    { 2, "Biała", "+1 popularności co turę (jak Zajazd u Czerwonego Smoka)", "Dobry humor", "PopularityBuff", false, 125, 1, 0, null, "DobryHumor", "Self" },
                    { 3, "Pozostałe", "Osłabia wybrane zaklęcie o podwojoną siłę Twoich magów (min. 20% siły zaklęcia). Tylko na własne księstwo. Br-Oug: o 50% droższe.", "Zdjęcie zaklęcia", "Dispel", false, 125, 1, 0, null, "ZdjecieZaklecia", "Self" },
                    { 4, "Biała", "Zwiększa wydajność niemagicznych profesji do +49%", "Pracowitość", "ProductionBuff", false, 340, 2, 0, null, "Pracowitosc", "Self" },
                    { 5, "Pozostałe", "Zamienia manę w złoto — 200 sztuk złota za 1 manę", "Mannamorfoza", "Mannamorphosis", false, 85, 1, 0, null, "Mannamorfoza", "Self" },
                    { 10, "Tarcze", "Zwiększa obronę magiczną do +24% (nie działa przez pakty)", "Tarcza antymagiczna", "AntimagicShield", false, 210, 2, 1, null, "TarczaAntymagiczna", "Self" },
                    { 11, "Tarcze", "Zwiększa obronę wojskową do +24% (tylko obrona własnych jednostek)", "Tarcza wojenna", "WarShield", false, 380, 2, 1, null, "TarczaWojenna", "Self" },
                    { 12, "Biała", "+10% przyrostu; zwiększa szansę na smoka, złoto z kopalni, przyjście generała, odbicie zaklęć i fart w labiryncie (max 49%)", "Szczęście", "LuckBuff", false, 210, 2, 1, null, "Szczescie", "Self" },
                    { 13, "Tarcze", "Do 24% szansy na odbicie nieudanych zaklęć wroga (+20% siły odbicia z Soczewką magiczną)", "Zwierciadło magiczne", "MagicShield", false, 680, 3, 1, null, "ZwierciadloMagiczne", "Self" },
                    { 14, "Tarcze", "Duchy poległych bronią księstwa: obrona = min(siła zaklęcia, liczba magów). Zdejmowane tylko Klątwą Padłych Legionów.", "Padłe legiony", "LegionShield", false, 425, 3, 1, null, "PadleLegiony", "Self" },
                    { 20, "Biała", "Przyrost ludności +30%", "Płodność", "GrowthBuff", false, 210, 2, 2, null, "Plodnosc", "Self" },
                    { 21, "Niszcząca", "Burzy 1–2% budynków infrastruktury, 50%·x szansy na budynek specjalny. Limit 5 na cel (Krasnolud 4, Goblin 3).", "Trzęsienie Ziemi", "BuildingDamage", true, 190, 3, 2, null, "TrzesienieZiemi", "Enemy" },
                    { 22, "Czarna", "Ludność potrzebuje do +300% więcej jedzenia, niszczy 9% zapasów (armie Nekromanty nie jedzą)", "Szarańcza", "FoodDamage", false, 125, 2, 2, null, "Szarancza", "Enemy" },
                    { 23, "Czarna", "Co turę umiera do 3% ludności (Olbrzym odporny)", "Zaraza", "PopulationDamage", false, 275, 3, 2, null, "Zaraza", "Enemy" },
                    { 24, "Pozostałe", "Odsyła duchy poległych do grobów — osłabia Padłe legiony o siłę tego zaklęcia", "Klątwa Padłych Legionów", "DoomLegions", false, 100, 2, 2, null, "KlatwaPadlychLegionow", "Enemy" },
                    { 30, "Czarna", "−1 popularności wroga co turę (Hobbit odporny)", "Zły humor", "PopularityDebuff", false, 65, 1, 3, null, "ZlyHumor", "Enemy" },
                    { 31, "Czarna", "Obniża obronę wojskową wroga do −24% (tylko obronę własnych jednostek celu)", "Słabość", "DefenseDebuff", false, 85, 2, 3, null, "Slabosc", "Enemy" },
                    { 32, "Niszcząca", "Zabija 2–4% mieszkańców (armia + profesje). Limit 5 na cel (Krasnolud 4, Goblin 3).", "Ognisty Deszcz", "ArmyDamage", true, 340, 3, 3, null, "OgnistyDeszcz", "Enemy" },
                    { 33, "Czarna", "−10% przyrostu ludności i mniej szczęścia w zdarzeniach losowych", "Pech", "GrowthDebuff", false, 65, 1, 3, null, "Pech", "Enemy" },
                    { 34, "Przywołania", "Wabi Czerwonego Smoka do armii; koszt zależy od liczby smoków: ×(D²·0,0001+0,2)·(max(50,D)/100)²", "Przywołanie Smoka", "SummonDragon", false, 500, 4, 3, null, "PrzywolanieSmoka", "Self" },
                    { 40, "Czarna", "Niszczy 20% zasobów wroga (Elf rzuca 10% słabiej, Dżin 10% silniej)", "Zniszczenie zapasów", "SupplyDamage", false, 125, 2, 4, null, "ZniszczenieZapasow", "Enemy" },
                    { 41, "Niszcząca", "Zabija 4% ludzi w profesjach (nie rusza armii i złodziei). Limit 7 na cel (Krasnolud 6, Goblin 5).", "Huragan", "WorkerDamage", true, 255, 3, 4, null, "Huragan", "Enemy" },
                    { 42, "Niszcząca", "Spala 5–10% złodziei wroga. Limit 7 na cel (Krasnolud 6; Goblin całkowicie odporny).", "Spopielenie złodziei", "ThiefDamage", true, 210, 3, 4, null, "SpopielenieZlodziei", "Enemy" },
                    { 43, "Czarna", "Co turę niszczą część machin wojennych (Gnom odporny — nie używa machin)", "Chochliki", "MachineDamage", false, 125, 2, 4, null, "Chochliki", "Enemy" },
                    { 44, "Niszcząca", "Najpotężniejsze zaklęcie: burzy 1–2% budynków, zabija 3–5% armii i 5–10% ludności, 50% szansy na budynek specjalny. Wymaga Pałacu Magicznego. Limit 5 (Krasnolud 4, Goblin 3).", "Smoczy Oddech", "DragonBreath", true, 1500, 5, 4, null, "SmoczyOddech", "Enemy" },
                    { 50, "Czarna", "Obniża wydajność niemagicznych profesji wroga do −50%", "Somnambulizm", "ProductionDebuff", false, 105, 2, 5, null, "Somnambulizm", "Enemy" },
                    { 51, "Czarna", "Obniża wydajność magów i druidów o 25% oraz obronę magiczną celu i jego paktów", "Głupota", "StupidityDebuff", false, 85, 2, 5, null, "Glupota", "Enemy" },
                    { 52, "Biała", "Zwiększa wydajność magicznych profesji do +49%", "Fluid magiczny", "MagicBuff", false, 210, 2, 5, null, "FluidMagiczny", "Self" },
                    { 53, "Czarna", "Przyrost ludności wroga −50% (Nekromant odporny)", "Kastracja", "GrowthDebuff", false, 85, 2, 5, null, "Kastracja", "Enemy" }
                });

            migrationBuilder.InsertData(
                table: "UnitDefinitions",
                columns: new[] { "Id", "AttackPower", "CostFood", "CostGold", "CostWeapons", "DefensePower", "Description", "DisplayName", "Race", "RequiredBuilding", "RequiredTech", "TrainingTime", "UnitType", "Upkeep" },
                values: new object[,]
                {
                    { 111, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Gnom", "", null, 1, "Gnom_Hoplita", 0 },
                    { 112, 1, 0, 600, 20, 5, "Elita 1. stopnia (+0,5 obrony złodziejskiej)", "Nocny Strażnik", "Gnom", "OltarzInicjacji", null, 1, "Gnom_NocnyStraznik", 0 },
                    { 113, 8, 0, 1600, 140, 7, "Elita 2. stopnia — wysadza wrogów (dodatkowi zabici = saperzy/3)", "Saper", "Gnom", "KoszarySpecjalne", null, 1, "Gnom_Saper", 0 },
                    { 115, 0, 0, 1500, 0, 0, "Armia podziemia (Gnom: 1500 złota)", "Złodziej", "Gnom", "GildiaZlodziei", null, 1, "Gnom_Zlodziej", 0 },
                    { 116, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Gnom", "Smokodrap", null, 1, "Gnom_Smok", 0 },
                    { 121, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Br-Oug", "", null, 1, "BrOug_Hoplita", 0 },
                    { 122, 2, 0, 500, 20, 2, "Elita 1. stopnia", "Kro-Draag", "Br-Oug", "OltarzInicjacji", null, 1, "BrOug_KroDraag", 0 },
                    { 123, 5, 0, 1300, 110, 6, "Elita 2. stopnia", "Ter-Aark", "Br-Oug", "KoszarySpecjalne", null, 1, "BrOug_TerAark", 0 },
                    { 124, 8, 0, 800, 50, 0, "Najsilniejsze machiny w grze (8 ataku; z E1: 6)", "Machina wojenna", "Br-Oug", "KonstrukcjaMachin", null, 1, "BrOug_Machina", 0 },
                    { 125, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Br-Oug", "GildiaZlodziei", null, 1, "BrOug_Zlodziej", 0 },
                    { 126, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Br-Oug", "Smokodrap", null, 1, "BrOug_Smok", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 12, 19, 50, 38, 924, DateTimeKind.Utc).AddTicks(3851));

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 2, 2 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 2, 3 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                column: "LimitedSpellsPerRecalc",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                column: "LimitedSpellsPerRecalc",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 2, 3 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 6,
                column: "LimitedSpellsPerRecalc",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks" },
                values: new object[] { 2, 0 });

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 8,
                column: "LimitedSpellsPerRecalc",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 9,
                column: "LimitedSpellsPerRecalc",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RaceDefinitions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "LimitedSpellsPerRecalc", "MagicBooks", "SpecialTraits" },
                values: new object[] { 4, 0, "Jedzenie 2/mieszkańca; limitowane zaklęcia działają na nich do 4× za przeliczenie; +25% burzenia machin; E1 burzy 0,1, E2 burzy 0,5 (nie blokują wież); mechanika Szamanizm (totemy: Grabieży / Smokobójstwa / Niszczycielstwa)." });

            migrationBuilder.InsertData(
                table: "SpellDefinitions",
                columns: new[] { "Id", "Category", "Description", "DisplayName", "EffectType", "IsLimited", "ManaCost", "PowerLevel", "RequiredBooks", "RequiredRace", "SpellType", "TargetType" },
                values: new object[,]
                {
                    { 1, "Biała", "Zwiększa produktywność profesji (% siły zaklęcia)", "Pracowitość", "ProductionBuff", false, 80, 1, 1, null, "Pracowitosc", "Self" },
                    { 2, "Biała", "Zwiększa siłę magów i druidów", "Fluid magiczny", "MagicBuff", false, 90, 1, 2, null, "FluidMagiczny", "Self" },
                    { 3, "Biała", "Przyrost ludności ×1,3", "Płodność", "GrowthBuff", false, 70, 1, 1, null, "Plodnosc", "Self" },
                    { 4, "Biała", "Przyrost ludności ×1,1 i drobne bonusy losowe", "Szczęście", "LuckBuff", false, 60, 1, 1, null, "Szczescie", "Self" },
                    { 5, "Biała", "+1 popularności co turę", "Dobry humor", "PopularityBuff", false, 60, 1, 1, null, "DobryHumor", "Self" },
                    { 10, "Tarcze", "Odbija wrogie zaklęcia (Ent: −25% many)", "Zwierciadło magiczne", "MagicShield", false, 150, 1, 2, null, "ZwierciadloMagiczne", "Self" },
                    { 11, "Tarcze", "Magiczna obrona: min(siła, liczba magów) dodana do obrony (Dżin ×3)", "Padłe legiony", "LegionShield", false, 180, 1, 3, null, "PadleLegiony", "Self" },
                    { 12, "Tarcze", "+% obrony wojskowej", "Tarcza wojenna", "WarShield", false, 160, 1, 2, null, "TarczaWojenna", "Self" },
                    { 13, "Tarcze", "Zwiększa obronę przed magią", "Tarcza antymagiczna", "AntimagicShield", false, 140, 1, 2, null, "TarczaAntymagiczna", "Self" },
                    { 14, "Tarcze", "Chronią budynki przed zniszczeniem", "Mury magiczne", "BuildingShield", false, 170, 1, 3, null, "MuryMagiczne", "Self" },
                    { 20, "Czarna", "Przyrost ludności wroga ×0,9", "Pech", "GrowthDebuff", false, 70, 1, 1, null, "Pech", "Enemy" },
                    { 21, "Czarna", "−2 popularności wroga co turę (Hobbit odporny)", "Zły humor", "PopularityDebuff", false, 70, 1, 1, null, "ZlyHumor", "Enemy" },
                    { 22, "Czarna", "Zabija ludność wroga (Nekromant/Wampir odporni)", "Zaraza", "PopulationDamage", false, 120, 2, 2, null, "Zaraza", "Enemy" },
                    { 23, "Czarna", "Pożera zapasy jedzenia wroga", "Szarańcza", "FoodDamage", false, 110, 2, 2, null, "Szarancza", "Enemy" },
                    { 24, "Czarna", "Usypia magów i druidów wroga (−% siły)", "Somnambulizm", "MagicDebuff", false, 100, 2, 2, null, "Somnambulizm", "Enemy" },
                    { 25, "Czarna", "Spowalnia profesje wroga", "Ospałość", "ProductionDebuff", false, 90, 1, 1, null, "Ospalosc", "Enemy" },
                    { 26, "Czarna", "Osłabia magów wroga", "Głupota", "StupidityDebuff", false, 100, 2, 2, null, "Glupota", "Enemy" },
                    { 27, "Czarna", "−% obrony wojskowej wroga (Ent odporny)", "Słabość", "DefenseDebuff", false, 130, 2, 3, null, "Slabosc", "Enemy" },
                    { 28, "Czarna", "Przyrost ludności wroga ×0,5 (Nekromant odporny)", "Kastracja", "GrowthDebuff", false, 120, 2, 2, null, "Kastracja", "Enemy" },
                    { 29, "Czarna", "Niszczy zapasy wroga (Hobbit 50%; Elf: −50% many)", "Zniszczenie zapasów", "SupplyDamage", false, 110, 2, 2, null, "ZniszczenieZapasow", "Enemy" },
                    { 40, "Niszcząca", "Zabija ludność, burzy budynki i obniża popularność (Krasnolud: 75% szkód)", "Smoczy Oddech", "DragonBreath", true, 250, 3, 3, null, "SmoczyOddech", "Enemy" },
                    { 41, "Niszcząca", "Burzy budynki infrastrukturalne (Krasnolud: 75% szkód)", "Trzęsienie Ziemi", "BuildingDamage", true, 240, 3, 3, null, "TrzesienieZiemi", "Enemy" },
                    { 42, "Niszcząca", "Zabija armię wroga (Elf: 50% strat; Ent: 200%)", "Ognisty Deszcz", "ArmyDamage", true, 230, 3, 3, null, "OgnistyDeszcz", "Enemy" },
                    { 43, "Niszcząca", "Niszczy budynki i zabija ludność", "Huragan", "Hurricane", true, 220, 3, 4, null, "Huragan", "Enemy" },
                    { 44, "Niszcząca", "Zabija złodziei wroga", "Spopielenie złodziei", "ThiefDamage", true, 200, 3, 3, null, "SpopielenieZlodziei", "Enemy" },
                    { 45, "Niszcząca", "Zalewa ziemie — niszczy budynki i zapasy", "Powódź", "Flood", true, 210, 3, 4, null, "Powodz", "Enemy" },
                    { 50, "Przywołania", "Przywołuje smoka; koszt rośnie z liczbą smoków: ×(D²·0,0001+0,2)·(max(50,D)/100)²", "Przywołanie Smoka", "SummonDragon", false, 500, 4, 4, null, "PrzywolanieSmoka", "Self" },
                    { 51, "Pozostałe", "Usuwa zaklęcie z własnego księstwa", "Zdjęcie zaklęcia", "Dispel", false, 100, 1, 1, null, "ZdjecieZaklecia", "Self" }
                });
        }
    }
}
