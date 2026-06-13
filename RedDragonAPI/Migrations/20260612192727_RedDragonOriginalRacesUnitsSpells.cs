using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class RedDragonOriginalRacesUnitsSpells : Migration
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
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.AddColumn<bool>(
                name: "IsLimited",
                table: "SpellDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RequiredBooks",
                table: "SpellDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequiredRace",
                table: "SpellDefinitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RaceDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameCz = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EaseRating = table.Column<int>(type: "int", nullable: false),
                    MagicRating = table.Column<int>(type: "int", nullable: false),
                    ThievesRating = table.Column<int>(type: "int", nullable: false),
                    DefenseRating = table.Column<int>(type: "int", nullable: false),
                    EconomyRating = table.Column<int>(type: "int", nullable: false),
                    AttackRating = table.Column<int>(type: "int", nullable: false),
                    MagicBooks = table.Column<int>(type: "int", nullable: false),
                    TurnsPerDay = table.Column<int>(type: "int", nullable: false),
                    GeneralsLimit = table.Column<int>(type: "int", nullable: false),
                    LimitedSpellsPerRecalc = table.Column<int>(type: "int", nullable: false),
                    HouseCapacityBase = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PopPerAcreBase = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    WaterworksHouseBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BurrowsHouseBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SewersHouseBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AqueductAcreBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FoodPerPop = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PopGrowthModifier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusFarmers = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusStonemasons = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusMasons = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusMerchants = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusAlchemists = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusArmorers = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusDruids = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusMages = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BonusScientists = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    E1Attack = table.Column<int>(type: "int", nullable: false),
                    E1Defense = table.Column<int>(type: "int", nullable: false),
                    E2Attack = table.Column<int>(type: "int", nullable: false),
                    E2Defense = table.Column<int>(type: "int", nullable: false),
                    MachineAttack = table.Column<int>(type: "int", nullable: false),
                    ThiefPowerModifier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ThiefCostModifier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MilitaryLossModifier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ResearchModifier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SpecialTraits = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceDefinitions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 12, 19, 27, 26, 988, DateTimeKind.Utc).AddTicks(2432));

            migrationBuilder.InsertData(
                table: "RaceDefinitions",
                columns: new[] { "Id", "AqueductAcreBonus", "AttackRating", "BonusAlchemists", "BonusArmorers", "BonusDruids", "BonusFarmers", "BonusMages", "BonusMasons", "BonusMerchants", "BonusScientists", "BonusStonemasons", "BurrowsHouseBonus", "DefenseRating", "Description", "E1Attack", "E1Defense", "E2Attack", "E2Defense", "EaseRating", "EconomyRating", "FoodPerPop", "GeneralsLimit", "HouseCapacityBase", "LimitedSpellsPerRecalc", "MachineAttack", "MagicBooks", "MagicRating", "MilitaryLossModifier", "Name", "NameCz", "PopGrowthModifier", "PopPerAcreBase", "ResearchModifier", "SewersHouseBonus", "SpecialTraits", "ThiefCostModifier", "ThiefPowerModifier", "ThievesRating", "TurnsPerDay", "WaterworksHouseBonus" },
                values: new object[,]
                {
                    { 1, 1m, 65, 0.10m, 0m, 0m, 0m, 0m, 0m, 0.20m, 0m, 0m, 1m, 60, "Dzięki wysokiej liczebności naszej populacji potrafimy uczynić wszystko, co ci przyjdzie do głowy! Umiemy świetnie czarować, jesteśmy dobrymi złodziejami, nie rozczarujemy Cię również w armii. Jesteśmy wszechstronną rasą.", 3, 3, 7, 7, 90, 65, 1m, 8, 5m, 2, 5, 2, 85, 0m, "Człowiek", "Člověk", 0m, 3m, 0.10m, 1.5m, "Generałowie zdobywają doświadczenie o 20% szybciej; budynki infrastrukturalne o 10% tańsze (złoto i budulec); mechanika Nauka stosowana (szkoła złodziejska/magiczna/wojskowa).", 0m, -0.05m, 90, 15, 0.5m },
                    { 2, 0.5m, 60, 0m, 0.20m, 0.20m, 0m, 0.30m, -0.10m, 0m, 0m, -0.10m, 1m, 70, "Najchętniej spędzamy czas w lasach, których potrafimy bardzo skutecznie bronić, a w razie potrzeby przeprowadzić z nich również kontratak. Potrafimy wpływać na świat dzięki wielu zaklęciom.", 4, 6, 8, 11, 60, 80, 1m, 6, 3m, 2, 5, 3, 90, 0m, "Elf", "Elf", 0m, 3m, 0m, 1.5m, "Straszny lasek odstrasza 10% armii inwazyjnej; Pałac magiczny: wzrost kosztu zaklęć tylko 9%; biała magia o 25% tańsza; 1,5× łupy z labiryntu; E1/E2 mają siłę magiczną 0,5/1,0; mechanika Komando łuczników (+20% obrony sojusznika, -20% własnej).", 0m, 0m, 75, 15, 0.5m },
                    { 3, 0.5m, 80, 0m, 0.30m, -0.30m, 0m, -0.20m, 0.20m, 0m, 0m, 0.20m, 1m, 50, "Nasze rześkie jednostki nadają się jak do obrony, tak do ataku. Nie znamy się na magii, ale twarde życie w górach zahartowało naszą armię. Obróbka kamienia zapewnia nam dobrobyt i sławę najlepszych budowniczych.", 5, 6, 10, 11, 100, 85, 1m, 6, 3m, 1, 5, 1, 60, -0.25m, "Krasnolud", "Trpaslík", 0m, 3m, 0m, 1.5m, "O 25% niższe straty wojskowe; zabijają o 20% więcej smoków; budynki specjalne o 10% tańsze; przechodzi o jedno limitowane zaklęcie mniej; pakty złodziejskie -10% skuteczności; mechanika Dodatkowe uzbrojenie (do +2 atak/obrona elit za broń).", 0m, -0.15m, 65, 15, 0.5m },
                    { 4, 0.5m, 40, 0m, 0m, 0m, 0.30m, -0.20m, 0m, 0.10m, 0m, 0m, 1m, 50, "Szukasz zręcznego złodzieja? Nie ma lepszych rabusiów od tych naszych. Nikt nie dorównuje ich zdolnościom. Agresorów potrafimy zaskoczyć upartą obroną.", 2, 4, 4, 10, 80, 70, 1m, 6, 3m, 2, 5, 1, 60, 0m, "Hobbit", "Hobit", 0m, 3.5m, 0m, 1.5m, "Złodzieje o 25% tańsi i silniejsi; Zniszczenie zapasów działa na nich w 50%; obniżki popularności (rewolta, Smoczy Oddech, ataki) o połowę słabsze; odporni na Zły humor; mniejsze straty ziemi (pierwszy atak 9% zamiast 11%); mechanika Hodokvas.", -0.25m, 0.25m, 100, 15, 0.5m },
                    { 5, 0.5m, 90, 0m, 0m, 0.20m, -0.25m, 0.30m, 0m, 0m, 0m, 0m, 1m, 90, "Wojna, śmierć i cierpienie! Obrona nie należy do naszych silnych stron, ale zatrzymanie hord żywych trupów jest praktycznie niemożliwe. Znamy również wiele mocnych zaklęć. Naszą specjalnością jest zasypywanie wrogów klęskami żywiołowymi.", 2, 1, 6, 3, 90, 65, 1m, 6, 3m, 2, 4, 3, 90, 0m, "Nekromant", "Nekromant", 0m, 3m, 0m, 1.5m, "Armia nie je i nie pobiera żołdu, nie umiera w czasie głodu; odporny na Zarazę, Kastrację i Płodność; Zaraza/Szarańcza/Kastracja/Zły humor o połowę tańsze; mechanika Nekromancja (armia wyczarowywana przez magów z ciał, Cmentarze, zaklęcie Ofiarowanie).", 0m, -0.50m, 70, 15, 0.5m },
                    { 6, 1.5m, 35, 0m, 0m, 0.10m, -0.30m, 0.40m, -0.10m, 0m, 0m, -0.10m, 1m, 90, "Jesteśmy najlepszymi magami, o jakich możesz śnić. Całe nasze życie poświęciliśmy magii. Nikt inny nam w niej nie dorównuje — jedynym prawdziwym wyzwaniem jest dla nas walczyć z innymi dżinami.", 2, 2, 4, 6, 50, 45, 1m, 6, 2m, 2, 5, 5, 100, 0m, "Dżin", "Džin", 0m, 2m, 0.20m, 2m, "Mana nie znika po turze (każdy dżin przechowa 1 manę); Pałac magiczny: wzrost kosztu zaklęć 6%, pakty magiczne +5% skuteczności; Padłe legiony 3× skuteczniejsze; zaklęcia Metamagii (Wzmocniona/Przyspieszona magia).", 0m, -0.15m, 65, 15, 1m },
                    { 7, 0.5m, 95, -0.30m, -0.20m, -0.50m, -0.30m, -0.50m, -0.20m, 0m, -0.30m, -0.20m, 0.5m, 50, "Jesteśmy rasą agresywną! Mamy silne jednostki ataku i złodziei, nad obroną się zbytecznie nie zastanawiamy. Jesteśmy najlepsi w budowaniu bardzo skutecznych narzędzi wojennych.", 2, 0, 6, 3, 80, 50, 1m, 6, 3m, 2, 5, 0, 65, 0m, "Goblin", "Skřet", 0.25m, 7m, -0.20m, 1m, "+2 tury dziennie (17), Wieża Czasu daje +2 tury; wieże obronne mieszczą 10 hoplitów (obrona 6) i 10 machin (obrona 100); każda jednostka utrzyma 2 machiny; mechanika Goblińska inżynieria (machiny z E1 +50% siły, z E2 obniżają obronę celu).", 0m, -0.20m, 80, 17, 0.5m },
                    { 8, 0.5m, 50, 0m, 0m, 0m, 0.50m, 0m, 0m, 0m, 0.20m, 0m, 1m, 100, "Nasza prastara rasa przerzedziła się w ciągu wieków, ale dysponuje najsilniejszymi jednostkami obrony. Nie ma rasy, która by nam dorównywała w obronie naszych i zaprzyjaźnionych księstw.", 2, 7, 5, 19, 50, 100, 1m, 6, 2m, 3, 5, 2, 60, -0.50m, "Ent", "Ent", 0m, 2m, 0m, 1.5m, "O 50% niższe straty wojskowe; -2 tury dziennie (13); limitowane zaklęcia przechodzą 3× za przeliczenie; Ognisty deszcz i Smoczy Oddech zadają im 2× straty; sady owocowe mieszczą 100 E2; mechanika Gniew Enta (+100% ataku i burzenia po stratach).", 0m, -0.25m, 50, 13, 0.5m },
                    { 9, 0.5m, 90, 0.20m, 0m, 0m, 0m, 0.20m, 0m, 0m, 0m, 0m, 1m, 80, "Jednostki wojskowe są naszą najsilniejszą stroną. Jak tylko posmakują krwi wrogów, zmieniają się w żądne krwi bestie nie do zatrzymania. Także nasi złodzieje są uważani za jednych z najlepszych w tej profesji.", 4, 2, 10, 5, 60, 40, 1m, 8, 3m, 2, 5, 3, 85, 0m, "Wampir", "Vampýr", 0m, 3m, 0m, 1.5m, "Armia nie je (nie umiera w głodzie); odporny na Zarazę; Głupota/Somnambulizm/Ospałość o połowę tańsze; 25% upitych żołnierzy wroga umiera; mechanika Krwawa magia (punkty krwi za zabitych wrogów odblokowują eliksiry: złodziei +5%/lvl, ataku +7%/lvl, magów +3%/lvl, strat +12,5%/lvl).", 0m, 0.10m, 90, 15, 0.5m },
                    { 10, 0.5m, 100, 0m, 0m, 0m, 0m, -0.15m, 0.30m, 0m, -0.15m, 0.30m, 0.5m, 70, "Jedynym, co nas interesuje, jest walka! Nie jest nas, co prawda, wielu, ale nasze jednostki są najsilniejsze ze wszystkich. Wybierz nas, a zmiażdżymy każdego, kto stanie na naszej drodze!", 6, 6, 16, 10, 70, 60, 2m, 6, 3m, 4, 6, 0, 55, 0m, "Olbrzym", "Obr", 0m, 2.5m, 0m, 1m, "Jedzenie 2/mieszkańca; limitowane zaklęcia działają na nich do 4× za przeliczenie; +25% burzenia machin; E1 burzy 0,1, E2 burzy 0,5 (nie blokują wież); mechanika Szamanizm (totemy: Grabieży / Smokobójstwa / Niszczycielstwa).", 0m, -0.25m, 55, 15, 0.5m }
                });

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
                    { 51, "Pozostałe", "Usuwa zaklęcie z własnego księstwa", "Zdjęcie zaklęcia", "Dispel", false, 100, 1, 1, null, "ZdjecieZaklecia", "Self" },
                    { 60, "Rasowe", "Metamagia Dżina: zaklęcia +10% siły, +25% ceny", "Wzmocniona magia", "Metamagic", false, 210, 2, 0, "Dżin", "WzmocnionaMagia", "Self" },
                    { 61, "Rasowe", "Metamagia Dżina: zaklęcia −10% ceny, −25% siły", "Przyspieszona magia", "Metamagic", false, 210, 2, 0, "Dżin", "PrzyspieszonaMagia", "Self" },
                    { 62, "Rasowe", "Szamanizm Olbrzyma: ładuje wybrany totem (koszt totemu: obszar×20)", "Wezwanie totemu", "TotemCharge", false, 380, 2, 0, "Olbrzym", "WezwanieTotemu", "Self" },
                    { 63, "Rasowe", "Nekromancja: −10% populacji/turę, polegli stają się ciałami", "Ofiarowanie", "Sacrifice", false, 210, 2, 0, "Nekromant", "Ofiarowanie", "Self" },
                    { 64, "Rasowe", "Nekromancja: wskrzesza E2 (10% wolnych magów, 1 ciało/jednostkę)", "Przywołaj elitę 2. stopnia", "SummonE2", false, 1000, 3, 0, "Nekromant", "PrzywolajE2", "Self" },
                    { 65, "Rasowe", "Nekromancja: wskrzesza E1 (26% wolnych magów, 1/6 ciała)", "Przywołaj elitę 1. stopnia", "SummonE1", false, 1000, 3, 0, "Nekromant", "PrzywolajE1", "Self" },
                    { 66, "Rasowe", "Nekromancja: wskrzesza hoplitów (50% wolnych magów, 1/6 ciała)", "Przywołaj hoplitów", "SummonHoplites", false, 1000, 3, 0, "Nekromant", "PrzywolajHoplitow", "Self" },
                    { 67, "Rasowe", "Nekromancja: wskrzesza złodziei (50% wolnych magów, 1/2 ciała)", "Przywołaj złodziei", "SummonThieves", false, 1000, 3, 0, "Nekromant", "PrzywolajZlodziei", "Self" }
                });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "ObserwacjaKsiestwa", "Wywiad: stan wojsk, zasobów i budynków wroga (bez expów dla generała)", "Obserwacja księstwa", "Spy", 1.00m, 20 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "KradziezZapasow", "Kradnie złoto i surowce wroga (bez expów dla generała)", "Kradzież zapasów", "StealSupplies", 0.90m, 50 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "PodzeganieDoRewolty", "Obniża popularność wroga (Hobbit: efekt połowiczny)", "Podżeganie do rewolty", "Revolt", 0.80m, 60 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "ThievesRequired" },
                values: new object[] { "BurzenieBudynkow", "Niszczy infrastrukturę wroga", "Burzenie budynków", "DemolishBuildings", 100 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "WojnaGangow", "Zabija złodziei wroga", "Wojna gangów", "ThiefWar", 0.80m, 80 });

            migrationBuilder.InsertData(
                table: "ThiefActionDefinitions",
                columns: new[] { "Id", "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[,]
                {
                    { 6, "WymordowanieMagow", "Zabija magów wroga", "Wymordowanie magów", "KillMages", 0.60m, 120 },
                    { 7, "ZabijanieLudnosci", "Morduje cywilów wroga", "Zabijanie ludności", "KillPeople", 0.70m, 100 },
                    { 8, "UpijanieArmii", "Upija żołnierzy wroga — nie bronią w następnym przeliczeniu (Wampir: 25% upitych umiera)", "Upijanie armii", "DrunkArmy", 0.70m, 90 },
                    { 9, "ZabojstwoGenerala", "Próba zamachu na generała wroga", "Zabójstwo generała", "KillGeneral", 0.30m, 200 },
                    { 10, "PorwanieGenerala", "Próba porwania generała wroga (można negocjować okup)", "Porwanie generała", "KidnapGeneral", 0.25m, 200 }
                });

            migrationBuilder.InsertData(
                table: "UnitDefinitions",
                columns: new[] { "Id", "AttackPower", "CostFood", "CostGold", "CostWeapons", "DefensePower", "Description", "DisplayName", "Race", "RequiredBuilding", "RequiredTech", "TrainingTime", "UnitType", "Upkeep" },
                values: new object[,]
                {
                    { 11, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Człowiek", "", null, 1, "Czlowiek_Hoplita", 0 },
                    { 12, 3, 0, 400, 4, 3, "Elita 1. stopnia", "Rycerz", "Człowiek", "OltarzInicjacji", null, 1, "Czlowiek_Rycerz", 0 },
                    { 13, 7, 0, 1200, 80, 7, "Elita 2. stopnia", "Paladyn", "Człowiek", "KoszarySpecjalne", null, 1, "Czlowiek_Paladyn", 0 },
                    { 14, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Człowiek", "KonstrukcjaMachin", null, 1, "Czlowiek_Machina", 0 },
                    { 15, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Człowiek", "GildiaZlodziei", null, 1, "Czlowiek_Zlodziej", 0 },
                    { 16, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Człowiek", "Smokodrap", null, 1, "Czlowiek_Smok", 0 },
                    { 21, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Elf", "", null, 1, "Elf_Hoplita", 0 },
                    { 22, 4, 0, 700, 20, 6, "Elita 1. stopnia", "Łucznik", "Elf", "OltarzInicjacji", null, 1, "Elf_Lucznik", 0 },
                    { 23, 8, 0, 1900, 200, 11, "Elita 2. stopnia", "Leśna Zjawa", "Elf", "KoszarySpecjalne", null, 1, "Elf_LesnaZjawa", 0 },
                    { 24, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Elf", "KonstrukcjaMachin", null, 1, "Elf_Machina", 0 },
                    { 25, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Elf", "GildiaZlodziei", null, 1, "Elf_Zlodziej", 0 },
                    { 26, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Elf", "Smokodrap", null, 1, "Elf_Smok", 0 },
                    { 31, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Krasnolud", "", null, 1, "Krasnolud_Hoplita", 0 },
                    { 32, 5, 0, 1000, 15, 6, "Elita 1. stopnia", "Ciężkozbrojny", "Krasnolud", "OltarzInicjacji", null, 1, "Krasnolud_Ciezkozbrojny", 0 },
                    { 33, 10, 0, 1800, 120, 11, "Elita 2. stopnia", "Berserker", "Krasnolud", "KoszarySpecjalne", null, 1, "Krasnolud_Berserker", 0 },
                    { 34, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Krasnolud", "KonstrukcjaMachin", null, 1, "Krasnolud_Machina", 0 },
                    { 35, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Krasnolud", "GildiaZlodziei", null, 1, "Krasnolud_Zlodziej", 0 },
                    { 36, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Krasnolud", "Smokodrap", null, 1, "Krasnolud_Smok", 0 },
                    { 41, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Hobbit", "", null, 1, "Hobbit_Hoplita", 0 },
                    { 42, 2, 0, 500, 20, 4, "Elita 1. stopnia", "Błotostęp", "Hobbit", "OltarzInicjacji", null, 1, "Hobbit_Blotostep", 0 },
                    { 43, 4, 0, 1200, 120, 10, "Elita 2. stopnia", "Nornik", "Hobbit", "KoszarySpecjalne", null, 1, "Hobbit_Nornik", 0 },
                    { 44, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Hobbit", "KonstrukcjaMachin", null, 1, "Hobbit_Machina", 0 },
                    { 45, 0, 0, 900, 0, 0, "Armia podziemia — duma Hobbitów", "Złodziej", "Hobbit", "GildiaZlodziei", null, 1, "Hobbit_Zlodziej", 0 },
                    { 46, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Hobbit", "Smokodrap", null, 1, "Hobbit_Smok", 0 },
                    { 51, 1, 0, 200, 2, 1, "Podstawowy żołnierz (nie je, bez żołdu)", "Hoplita", "Nekromant", "", null, 1, "Nekromant_Hoplita", 0 },
                    { 52, 2, 0, 700, 20, 1, "Elita 1. stopnia", "Szkielet", "Nekromant", "OltarzInicjacji", null, 1, "Nekromant_Szkielet", 0 },
                    { 53, 6, 0, 1900, 200, 3, "Elita 2. stopnia", "Ghul", "Nekromant", "KoszarySpecjalne", null, 1, "Nekromant_Ghul", 0 },
                    { 54, 4, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Nekromant", "KonstrukcjaMachin", null, 1, "Nekromant_Machina", 0 },
                    { 55, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Nekromant", "GildiaZlodziei", null, 1, "Nekromant_Zlodziej", 0 },
                    { 56, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Nekromant", "Smokodrap", null, 1, "Nekromant_Smok", 0 },
                    { 61, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Dżin", "", null, 1, "Dzin_Hoplita", 0 },
                    { 62, 2, 0, 600, 20, 2, "Elita 1. stopnia", "Al'Ahvar", "Dżin", "OltarzInicjacji", null, 1, "Dzin_AlAhvar", 0 },
                    { 63, 4, 0, 1400, 120, 6, "Elita 2. stopnia", "Dżin'Beam", "Dżin", "KoszarySpecjalne", null, 1, "Dzin_DzinBeam", 0 },
                    { 64, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Dżin", "KonstrukcjaMachin", null, 1, "Dzin_Machina", 0 },
                    { 65, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Dżin", "GildiaZlodziei", null, 1, "Dzin_Zlodziej", 0 },
                    { 66, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Dżin", "Smokodrap", null, 1, "Dzin_Smok", 0 },
                    { 71, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Goblin", "", null, 1, "Goblin_Hoplita", 0 },
                    { 72, 2, 0, 700, 20, 0, "Elita 1. stopnia", "Wilczy Jeździec", "Goblin", "OltarzInicjacji", null, 1, "Goblin_WilczyJezdziec", 0 },
                    { 73, 6, 0, 2000, 200, 3, "Elita 2. stopnia", "Skurut Hai", "Goblin", "KoszarySpecjalne", null, 1, "Goblin_SkurutHai", 0 },
                    { 74, 5, 0, 800, 50, 0, "Burzy budynki; Gobliny używają jej też w obronie", "Machina wojenna", "Goblin", "KonstrukcjaMachin", null, 1, "Goblin_Machina", 0 },
                    { 75, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Goblin", "GildiaZlodziei", null, 1, "Goblin_Zlodziej", 0 },
                    { 76, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Goblin", "Smokodrap", null, 1, "Goblin_Smok", 0 },
                    { 81, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Ent", "", null, 1, "Ent_Hoplita", 0 },
                    { 82, 2, 0, 900, 20, 7, "Elita 1. stopnia", "Konar", "Ent", "OltarzInicjacji", null, 1, "Ent_Konar", 0 },
                    { 83, 5, 0, 2400, 200, 19, "Elita 2. stopnia — najtwardszy obrońca w grze", "Drzewiec", "Ent", "KoszarySpecjalne", null, 1, "Ent_Drzewiec", 0 },
                    { 84, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Ent", "KonstrukcjaMachin", null, 1, "Ent_Machina", 0 },
                    { 85, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Ent", "GildiaZlodziei", null, 1, "Ent_Zlodziej", 0 },
                    { 86, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Ent", "Smokodrap", null, 1, "Ent_Smok", 0 },
                    { 91, 1, 0, 200, 2, 1, "Podstawowy żołnierz (nie je)", "Hoplita", "Wampir", "", null, 1, "Wampir_Hoplita", 0 },
                    { 92, 4, 0, 700, 20, 2, "Elita 1. stopnia", "Upiór", "Wampir", "OltarzInicjacji", null, 1, "Wampir_Upior", 0 },
                    { 93, 10, 0, 1800, 160, 5, "Elita 2. stopnia", "Nosferatu", "Wampir", "KoszarySpecjalne", null, 1, "Wampir_Nosferatu", 0 },
                    { 94, 5, 0, 800, 50, 0, "Burzy budynki wroga", "Machina wojenna", "Wampir", "KonstrukcjaMachin", null, 1, "Wampir_Machina", 0 },
                    { 95, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Wampir", "GildiaZlodziei", null, 1, "Wampir_Zlodziej", 0 },
                    { 96, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Wampir", "Smokodrap", null, 1, "Wampir_Smok", 0 },
                    { 101, 1, 0, 200, 2, 1, "Podstawowy żołnierz", "Hoplita", "Olbrzym", "", null, 1, "Olbrzym_Hoplita", 0 },
                    { 102, 6, 0, 1200, 40, 6, "Elita 1. stopnia (burzy 0,1 budynku)", "Głazomiot", "Olbrzym", "OltarzInicjacji", null, 1, "Olbrzym_Glazomiot", 0 },
                    { 103, 16, 0, 3200, 320, 10, "Elita 2. stopnia — najsilniejszy atak w grze (burzy 0,5 budynku)", "Niszczyciel", "Olbrzym", "KoszarySpecjalne", null, 1, "Olbrzym_Niszczyciel", 0 },
                    { 104, 6, 0, 800, 50, 0, "Burzy budynki wroga (+25% u Olbrzymów)", "Machina wojenna", "Olbrzym", "KonstrukcjaMachin", null, 1, "Olbrzym_Machina", 0 },
                    { 105, 0, 0, 1200, 0, 0, "Armia podziemia", "Złodziej", "Olbrzym", "GildiaZlodziei", null, 1, "Olbrzym_Zlodziej", 0 },
                    { 106, 100, 0, 0, 0, 100, "Potężna bestia — wzmacnia armię", "Smok", "Olbrzym", "Smokodrap", null, 1, "Olbrzym_Smok", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceDefinitions");

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

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "SpellDefinitions",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 86);

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

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "UnitDefinitions",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DropColumn(
                name: "IsLimited",
                table: "SpellDefinitions");

            migrationBuilder.DropColumn(
                name: "RequiredBooks",
                table: "SpellDefinitions");

            migrationBuilder.DropColumn(
                name: "RequiredRace",
                table: "SpellDefinitions");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 2, 26, 15, 26, 52, 626, DateTimeKind.Utc).AddTicks(3307));

            migrationBuilder.InsertData(
                table: "SpellDefinitions",
                columns: new[] { "Id", "Category", "Description", "DisplayName", "EffectType", "ManaCost", "PowerLevel", "SpellType", "TargetType" },
                values: new object[,]
                {
                    { 1, "White", "Leczy rany żołnierzy po walce", "Światło Uzdrowienia", "Buff", 100, 5, "HealingLight", "Self" },
                    { 2, "White", "+15% obrony na 5 tur", "Aura Ochronna", "Buff", 200, 10, "ProtectiveAura", "Self" },
                    { 3, "White", "+25% produkcji na 3 tury", "Błogosławieństwo Produkcji", "Buff", 150, 8, "ProductionBlessing", "Self" },
                    { 4, "Destructive", "Zadaje obrażenia armii wroga", "Kula Ognia", "Damage", 300, 15, "Fireball", "Enemy" },
                    { 5, "Destructive", "Niszczy budynki wroga", "Trzęsienie Ziemi", "Damage", 500, 20, "Earthquake", "Enemy" },
                    { 6, "Black", "Zmniejsza populację wroga", "Zaraza", "Debuff", 400, 18, "Plague", "Enemy" },
                    { 7, "Black", "-20% produkcji wroga na 5 tur", "Klątwa", "Debuff", 250, 12, "Curse", "Enemy" }
                });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "StealGold", "Kradnie złoto z wrogiego skarbca", "Kradzież Złota", "StealGold", 0.60m, 50 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "StealResources", "Kradnie surowce wroga", "Kradzież Surowców", "StealResources", 0.50m, 75 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "Sabotage", "Niszczy budynki wroga", "Sabotaż", "Sabotage", 0.40m, 100 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "ThievesRequired" },
                values: new object[] { "Spy", "Zbiera informacje o wrogu", "Szpiegostwo", "Spy", 30 });

            migrationBuilder.UpdateData(
                table: "ThiefActionDefinitions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[] { "Assassination", "Zabija magów lub naukowców wroga", "Zamach", "Sabotage", 0.30m, 150 });

            migrationBuilder.InsertData(
                table: "UnitDefinitions",
                columns: new[] { "Id", "AttackPower", "CostFood", "CostGold", "CostWeapons", "DefensePower", "Description", "DisplayName", "Race", "RequiredBuilding", "RequiredTech", "TrainingTime", "UnitType", "Upkeep" },
                values: new object[,]
                {
                    { 1, 10, 10, 50, 1, 12, "Podstawowe jednostki piechoty", "Piechota", "Ludzie", "KonstrukcjaMachin", null, 1, "Ludzie_Piechota", 2 },
                    { 2, 15, 10, 80, 2, 6, "Jednostki dystansowe", "Łucznik", "Ludzie", "KonstrukcjaMachin", null, 1, "Ludzie_Lucznik", 3 },
                    { 3, 25, 20, 200, 3, 20, "Szybkie i silne jednostki konne", "Kawaleria", "Ludzie", "KonstrukcjaMachin", null, 2, "Ludzie_Kawaleria", 5 },
                    { 4, 40, 30, 500, 5, 35, "Elitarne jednostki wojskowe", "Rycerz", "Ludzie", "AkademiaWojskowa", null, 3, "Ludzie_Rycerz", 10 },
                    { 5, 60, 0, 1000, 10, 5, "Potężna machina oblężnicza", "Machina wojenna", "Ludzie", "KonstrukcjaMachin", null, 5, "Ludzie_Machina", 15 },
                    { 6, 12, 10, 60, 1, 15, "Silna piechota krasnoludów", "Wojownik krasnoludzki", "Krasnoludy", "KonstrukcjaMachin", null, 1, "Krasnoludy_Piechota", 2 },
                    { 7, 18, 10, 90, 2, 8, "Ciężka broń dystansowa", "Kusznik krasnoludzki", "Krasnoludy", "KonstrukcjaMachin", null, 1, "Krasnoludy_Lucznik", 3 },
                    { 8, 11, 8, 55, 1, 10, "Zwinny wojownik elfów", "Strażnik elfów", "Elfy", "KonstrukcjaMachin", null, 1, "Elfy_Piechota", 2 },
                    { 9, 20, 8, 75, 1, 5, "Mistrzowscy łucznicy", "Łucznik elfów", "Elfy", "KonstrukcjaMachin", null, 1, "Elfy_Lucznik", 3 },
                    { 10, 15, 12, 40, 1, 8, "Dziki wojownik orków", "Berserker orków", "Orkowie", "KonstrukcjaMachin", null, 1, "Orkowie_Piechota", 3 },
                    { 11, 6, 5, 25, 1, 5, "Tania i szybka jednostka", "Gobliński łobuz", "Gobliny", "KonstrukcjaMachin", null, 1, "Gobliny_Piechota", 1 }
                });
        }
    }
}
