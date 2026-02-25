using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostGold = table.Column<int>(type: "int", nullable: false),
                    CostWood = table.Column<int>(type: "int", nullable: false),
                    CostStone = table.Column<int>(type: "int", nullable: false),
                    CostIron = table.Column<int>(type: "int", nullable: false),
                    CostMana = table.Column<int>(type: "int", nullable: false),
                    CostLand = table.Column<int>(type: "int", nullable: false),
                    BuildTime = table.Column<int>(type: "int", nullable: false),
                    RequiredBuildingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequiredTechnology = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSpecial = table.Column<bool>(type: "bit", nullable: false),
                    BonusTurnsPerDay = table.Column<int>(type: "int", nullable: false),
                    ProductionBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DefenseBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PopulationCapacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_BuildingDefinitions_BuildingType", x => x.BuildingType);
                });

            migrationBuilder.CreateTable(
                name: "SpellDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpellType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManaCost = table.Column<int>(type: "int", nullable: false),
                    PowerLevel = table.Column<int>(type: "int", nullable: false),
                    EffectType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_SpellDefinitions_SpellType", x => x.SpellType);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TechType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CostGold = table.Column<int>(type: "int", nullable: false),
                    ResearchTime = table.Column<int>(type: "int", nullable: false),
                    RequiredTech = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequiredBuilding = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EffectType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EffectValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_TechnologyDefinitions_TechType", x => x.TechType);
                });

            migrationBuilder.CreateTable(
                name: "ThiefActionDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThievesRequired = table.Column<int>(type: "int", nullable: false),
                    SuccessBaseRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThiefActionDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Race = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostGold = table.Column<int>(type: "int", nullable: false),
                    CostIron = table.Column<int>(type: "int", nullable: false),
                    CostWood = table.Column<int>(type: "int", nullable: false),
                    CostFood = table.Column<int>(type: "int", nullable: false),
                    AttackPower = table.Column<int>(type: "int", nullable: false),
                    DefensePower = table.Column<int>(type: "int", nullable: false),
                    Upkeep = table.Column<int>(type: "int", nullable: false),
                    RequiredBuilding = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredTech = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrainingTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_UnitDefinitions_UnitType", x => x.UnitType);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveSpells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    SpellType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Power = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CastAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveSpells_SpellDefinitions_SpellType",
                        column: x => x.SpellType,
                        principalTable: "SpellDefinitions",
                        principalColumn: "SpellType",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BattleReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttackerKingdomId = table.Column<int>(type: "int", nullable: false),
                    DefenderKingdomId = table.Column<int>(type: "int", nullable: false),
                    BattleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttackerLosses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefenderLosses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourcesStolen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LandCaptured = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    BuildingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsUnderConstruction = table.Column<bool>(type: "bit", nullable: false),
                    ConstructionCompletesAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buildings_BuildingDefinitions_BuildingType",
                        column: x => x.BuildingType,
                        principalTable: "BuildingDefinitions",
                        principalColumn: "BuildingType",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Coalitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LeaderKingdomId = table.Column<int>(type: "int", nullable: true),
                    EraId = table.Column<int>(type: "int", nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    PSOProgress = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coalitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Eras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WinningCoalitionId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eras_Coalitions_WinningCoalitionId",
                        column: x => x.WinningCoalitionId,
                        principalTable: "Coalitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Kingdoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Race = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Land = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false),
                    Food = table.Column<long>(type: "bigint", nullable: false),
                    Wood = table.Column<long>(type: "bigint", nullable: false),
                    Stone = table.Column<long>(type: "bigint", nullable: false),
                    Iron = table.Column<long>(type: "bigint", nullable: false),
                    Mana = table.Column<long>(type: "bigint", nullable: false),
                    Population = table.Column<int>(type: "int", nullable: false),
                    PopulationGrowthRate = table.Column<int>(type: "int", nullable: false),
                    TurnsAvailable = table.Column<int>(type: "int", nullable: false),
                    TurnsPerDay = table.Column<int>(type: "int", nullable: false),
                    MaxTurns = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    CoalitionId = table.Column<int>(type: "int", nullable: true),
                    CoalitionRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EraId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActive = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kingdoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kingdoms_Coalitions_CoalitionId",
                        column: x => x.CoalitionId,
                        principalTable: "Coalitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Kingdoms_Eras_EraId",
                        column: x => x.EraId,
                        principalTable: "Eras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kingdoms_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pantheon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EraId = table.Column<int>(type: "int", nullable: false),
                    CoalitionId = table.Column<int>(type: "int", nullable: false),
                    VictoryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pantheon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pantheon_Coalitions_CoalitionId",
                        column: x => x.CoalitionId,
                        principalTable: "Coalitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pantheon_Eras_EraId",
                        column: x => x.EraId,
                        principalTable: "Eras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderKingdomId = table.Column<int>(type: "int", nullable: false),
                    ReceiverKingdomId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Kingdoms_ReceiverKingdomId",
                        column: x => x.ReceiverKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Kingdoms_SenderKingdomId",
                        column: x => x.SenderKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilitaryUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    InTraining = table.Column<int>(type: "int", nullable: false),
                    TrainingCompletesAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilitaryUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilitaryUnits_Kingdoms_KingdomId",
                        column: x => x.KingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MilitaryUnits_UnitDefinitions_UnitType",
                        column: x => x.UnitType,
                        principalTable: "UnitDefinitions",
                        principalColumn: "UnitType",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Professions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    ProfessionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkerCount = table.Column<int>(type: "int", nullable: false),
                    ProductionPerTurn = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Professions_Kingdoms_KingdomId",
                        column: x => x.KingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueuedActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    TargetKingdomId = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuedActions_Kingdoms_KingdomId",
                        column: x => x.KingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueuedActions_Kingdoms_TargetKingdomId",
                        column: x => x.TargetKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Research",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    TechType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsInProgress = table.Column<bool>(type: "bit", nullable: false),
                    CompletesAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Research", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Research_Kingdoms_KingdomId",
                        column: x => x.KingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Research_TechnologyDefinitions_TechType",
                        column: x => x.TechType,
                        principalTable: "TechnologyDefinitions",
                        principalColumn: "TechType",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "BuildingDefinitions",
                columns: new[] { "Id", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "CostGold", "CostIron", "CostLand", "CostMana", "CostStone", "CostWood", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology" },
                values: new object[,]
                {
                    { 1, 0, 1, "House", "Residential", 0, 0, 2, 0, 20, 50, 0m, "Zwiększa limit ludności o 100", "Dom", false, 100, 0m, null, null },
                    { 2, 0, 1, "Farm", "Economic", 10, 0, 3, 0, 0, 40, 0m, "Produkuje 500 żywności na turę", "Farma", false, 0, 0m, null, null },
                    { 3, 0, 2, "Sawmill", "Economic", 30, 0, 2, 0, 0, 60, 0m, "Produkuje 300 drewna na turę", "Tartak", false, 0, 0m, null, null },
                    { 4, 0, 2, "Quarry", "Economic", 50, 0, 3, 0, 0, 50, 0m, "Wydobywa kamień (wymaga kamieniarzy)", "Kamieniołom", false, 0, 0m, null, null },
                    { 5, 0, 3, "Mine", "Economic", 100, 0, 4, 0, 50, 100, 0m, "Produkuje 200 żelaza na turę", "Kopalnia", false, 0, 0m, null, null },
                    { 6, 0, 3, "Forge", "Economic", 150, 0, 2, 0, 100, 80, 0m, "Przekształca żelazo w broń", "Kuźnia", false, 0, 0m, null, null },
                    { 7, 0, 2, "Workshop", "Economic", 50, 0, 2, 0, 30, 50, 0m, "Bonus +10% do produkcji surowców", "Warsztat", false, 0, 0.10m, null, null },
                    { 8, 0, 5, "Manufactory", "Economic", 500, 0, 4, 0, 200, 150, 0m, "Bonus +20% do produkcji zaawansowanej", "Manufaktura", false, 0, 0.20m, "Workshop", null },
                    { 9, 0, 5, "University", "Scientific", 1000, 0, 5, 0, 300, 200, 0m, "Umożliwia badania technologiczne", "Uniwersytet", false, 0, 0m, null, null },
                    { 10, 0, 3, "Barracks", "Military", 300, 0, 3, 0, 150, 150, 0m, "Szkolenie piechoty", "Koszary", false, 0, 0m, null, null },
                    { 11, 0, 3, "ArcheryRange", "Military", 250, 0, 3, 0, 80, 120, 0m, "Szkolenie łuczników", "Strzelnica", false, 0, 0m, null, null },
                    { 12, 0, 4, "Stable", "Military", 500, 0, 4, 0, 150, 200, 0m, "Szkolenie kawalerii", "Stajnia", false, 0, 0m, null, null },
                    { 13, 0, 2, "Wall", "Defensive", 200, 0, 1, 0, 100, 0, 0.05m, "Bonus obronny +5% per poziom (max 10)", "Mur", false, 0, 0m, null, null },
                    { 14, 0, 3, "GuardTower", "Defensive", 300, 0, 2, 0, 150, 0, 0.03m, "Bonus obronny +3%, wykrywa złodziei +10%", "Wieża Strażnicza", false, 0, 0m, null, null },
                    { 15, 0, 10, "EnchantmentPoint", "Defensive", 1000, 0, 3, 500, 500, 0, 0m, "Nie może być zniszczony magią ani złodziejami", "Punkt Zaklinania (PZ)", false, 0, 0m, null, null },
                    { 16, 1, 10, "MerchantGuild", "Special", 10000, 0, 5, 0, 5000, 0, 0m, "Bonus +50% złota od kupców, +1 tura/dzień", "Gildia Kupców", true, 0, 0.50m, null, null },
                    { 17, 1, 10, "MagicLibrary", "Special", 10000, 0, 5, 2000, 5000, 0, 0m, "Bonus +50% produkcji many, +1 tura/dzień", "Biblioteka Magiczna", true, 0, 0.50m, null, null },
                    { 18, 1, 10, "MilitaryAcademy", "Special", 10000, 2000, 5, 0, 5000, 0, 0m, "Bonus +20% siły armii, +1 tura/dzień", "Akademia Wojskowa", true, 0, 0m, null, null },
                    { 19, 1, 10, "ThievesGuild", "Special", 10000, 0, 5, 0, 5000, 0, 0m, "Bonus +30% skuteczności złodziei, +1 tura/dzień", "Gildia Złodziei", true, 0, 0m, null, null },
                    { 20, 2, 15, "Citadel", "Special", 15000, 3000, 8, 0, 10000, 0, 0.50m, "Bonus +50% obrony, +2 tury/dzień", "Cytadela", true, 0, 0m, null, null }
                });

            migrationBuilder.InsertData(
                table: "Eras",
                columns: new[] { "Id", "EndedAt", "IsActive", "Name", "StartedAt", "Theme", "WinningCoalitionId" },
                values: new object[] { 1, null, true, "Era Przebudzenia", new DateTime(2026, 2, 25, 13, 41, 17, 327, DateTimeKind.Utc).AddTicks(3060), "Pierwsza era nowego świata Red Dragon", null });

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

            migrationBuilder.InsertData(
                table: "TechnologyDefinitions",
                columns: new[] { "Id", "Category", "CostGold", "Description", "DisplayName", "EffectType", "EffectValue", "Level", "RequiredBuilding", "RequiredTech", "ResearchTime", "TechType" },
                values: new object[,]
                {
                    { 1, "Economy", 5000, "+20% produkcji żywności", "Rolnictwo I", "FoodProduction", 0.20m, 1, null, null, 10, "AgricultureI" },
                    { 2, "Economy", 15000, "+40% produkcji żywności", "Rolnictwo II", "FoodProduction", 0.40m, 1, null, "AgricultureI", 20, "AgricultureII" },
                    { 3, "Economy", 40000, "+60% produkcji żywności", "Rolnictwo III", "FoodProduction", 0.60m, 1, null, "AgricultureII", 30, "AgricultureIII" },
                    { 4, "Economy", 5000, "+20% produkcji kamienia i żelaza", "Górnictwo I", "MiningProduction", 0.20m, 1, null, null, 10, "MiningI" },
                    { 5, "Economy", 15000, "+40% produkcji kamienia i żelaza", "Górnictwo II", "MiningProduction", 0.40m, 1, null, "MiningI", 20, "MiningII" },
                    { 6, "Economy", 40000, "+60% produkcji kamienia i żelaza", "Górnictwo III", "MiningProduction", 0.60m, 1, null, "MiningII", 30, "MiningIII" },
                    { 7, "Military", 10000, "+15% siły ataku", "Broń I", "AttackBonus", 0.15m, 1, null, null, 15, "WeaponryI" },
                    { 8, "Military", 25000, "+30% siły ataku", "Broń II", "AttackBonus", 0.30m, 1, null, "WeaponryI", 25, "WeaponryII" },
                    { 9, "Military", 50000, "+50% siły ataku", "Broń III", "AttackBonus", 0.50m, 1, null, "WeaponryII", 35, "WeaponryIII" },
                    { 10, "Military", 10000, "+15% obrony", "Zbroja I", "DefenseBonus", 0.15m, 1, null, null, 15, "ArmorI" },
                    { 11, "Military", 25000, "+30% obrony", "Zbroja II", "DefenseBonus", 0.30m, 1, null, "ArmorI", 25, "ArmorII" },
                    { 12, "Military", 50000, "+50% obrony", "Zbroja III", "DefenseBonus", 0.50m, 1, null, "ArmorII", 35, "ArmorIII" },
                    { 13, "Magic", 8000, "+20% efektywności bufów", "Biała Magia I", "BuffPower", 0.20m, 1, null, null, 12, "WhiteMagicI" },
                    { 14, "Magic", 20000, "+40% efektywności bufów", "Biała Magia II", "BuffPower", 0.40m, 1, null, "WhiteMagicI", 22, "WhiteMagicII" },
                    { 15, "Magic", 12000, "+15% siły czarów bojowych", "Magia Niszcząca I", "SpellPower", 0.15m, 1, null, null, 18, "DestructiveMagicI" },
                    { 16, "Magic", 30000, "+30% siły czarów bojowych", "Magia Niszcząca II", "SpellPower", 0.30m, 1, null, "DestructiveMagicI", 28, "DestructiveMagicII" },
                    { 17, "Magic", 60000, "+50% siły czarów bojowych", "Magia Niszcząca III", "SpellPower", 0.50m, 1, null, "DestructiveMagicII", 38, "DestructiveMagicIII" },
                    { 18, "Thieves", 7000, "+20% skuteczności kradzieży", "Skradanie I", "ThiefSuccess", 0.20m, 1, null, null, 10, "StealthI" },
                    { 19, "Thieves", 18000, "+40% skuteczności kradzieży", "Skradanie II", "ThiefSuccess", 0.40m, 1, null, "StealthI", 20, "StealthII" },
                    { 20, "Thieves", 15000, "Umożliwia szpiegowanie wrogów", "Szpiegostwo", "UnlockEspionage", 1.0m, 1, null, "StealthI", 15, "Espionage" }
                });

            migrationBuilder.InsertData(
                table: "ThiefActionDefinitions",
                columns: new[] { "Id", "ActionType", "Description", "DisplayName", "EffectType", "SuccessBaseRate", "ThievesRequired" },
                values: new object[,]
                {
                    { 1, "StealGold", "Kradnie złoto z wrogiego skarbca", "Kradzież Złota", "StealGold", 0.60m, 50 },
                    { 2, "StealResources", "Kradnie surowce wroga", "Kradzież Surowców", "StealResources", 0.50m, 75 },
                    { 3, "Sabotage", "Niszczy budynki wroga", "Sabotaż", "Sabotage", 0.40m, 100 },
                    { 4, "Spy", "Zbiera informacje o wrogu", "Szpiegostwo", "Spy", 0.70m, 30 },
                    { 5, "Assassination", "Zabija magów lub naukowców wroga", "Zamach", "Sabotage", 0.30m, 150 }
                });

            migrationBuilder.InsertData(
                table: "UnitDefinitions",
                columns: new[] { "Id", "AttackPower", "CostFood", "CostGold", "CostIron", "CostWood", "DefensePower", "Description", "DisplayName", "Race", "RequiredBuilding", "RequiredTech", "TrainingTime", "UnitType", "Upkeep" },
                values: new object[,]
                {
                    { 1, 10, 10, 50, 20, 0, 12, "Podstawowe jednostki piechoty", "Piechota", "Human", "Barracks", null, 1, "Infantry", 2 },
                    { 2, 15, 10, 80, 15, 30, 6, "Jednostki dystansowe", "Łucznik", "Human", "ArcheryRange", null, 1, "Archer", 3 },
                    { 3, 25, 20, 200, 40, 0, 20, "Szybkie i silne jednostki konne", "Kawaleria", "Human", "Stable", null, 2, "Cavalry", 5 },
                    { 4, 50, 0, 500, 100, 200, 5, "Bonus +100% vs budynki", "Machina Oblężnicza", "Human", "Forge", null, 3, "SiegeEngine", 10 },
                    { 5, 50, 50, 1000, 200, 0, 45, "Elitarne jednostki wojskowe", "Rycerz", "Human", "MilitaryAcademy", "WeaponryIII", 5, "Knight", 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveSpells_KingdomId",
                table: "ActiveSpells",
                column: "KingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveSpells_SpellType",
                table: "ActiveSpells",
                column: "SpellType");

            migrationBuilder.CreateIndex(
                name: "IX_BattleReports_AttackerKingdomId",
                table: "BattleReports",
                column: "AttackerKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleReports_DefenderKingdomId",
                table: "BattleReports",
                column: "DefenderKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDefinitions_BuildingType",
                table: "BuildingDefinitions",
                column: "BuildingType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_BuildingType",
                table: "Buildings",
                column: "BuildingType");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_KingdomId",
                table: "Buildings",
                column: "KingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Coalitions_EraId",
                table: "Coalitions",
                column: "EraId");

            migrationBuilder.CreateIndex(
                name: "IX_Coalitions_LeaderKingdomId",
                table: "Coalitions",
                column: "LeaderKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Eras_WinningCoalitionId",
                table: "Eras",
                column: "WinningCoalitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Kingdoms_CoalitionId",
                table: "Kingdoms",
                column: "CoalitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Kingdoms_EraId",
                table: "Kingdoms",
                column: "EraId");

            migrationBuilder.CreateIndex(
                name: "IX_Kingdoms_UserId",
                table: "Kingdoms",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverKingdomId",
                table: "Messages",
                column: "ReceiverKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderKingdomId",
                table: "Messages",
                column: "SenderKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_MilitaryUnits_KingdomId",
                table: "MilitaryUnits",
                column: "KingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_MilitaryUnits_UnitType",
                table: "MilitaryUnits",
                column: "UnitType");

            migrationBuilder.CreateIndex(
                name: "IX_Pantheon_CoalitionId",
                table: "Pantheon",
                column: "CoalitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pantheon_EraId_CoalitionId",
                table: "Pantheon",
                columns: new[] { "EraId", "CoalitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Professions_KingdomId_ProfessionType",
                table: "Professions",
                columns: new[] { "KingdomId", "ProfessionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueuedActions_KingdomId",
                table: "QueuedActions",
                column: "KingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedActions_ScheduledFor_Status",
                table: "QueuedActions",
                columns: new[] { "ScheduledFor", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedActions_TargetKingdomId",
                table: "QueuedActions",
                column: "TargetKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Research_KingdomId_TechType",
                table: "Research",
                columns: new[] { "KingdomId", "TechType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Research_TechType",
                table: "Research",
                column: "TechType");

            migrationBuilder.CreateIndex(
                name: "IX_SpellDefinitions_SpellType",
                table: "SpellDefinitions",
                column: "SpellType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyDefinitions_TechType",
                table: "TechnologyDefinitions",
                column: "TechType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThiefActionDefinitions_ActionType",
                table: "ThiefActionDefinitions",
                column: "ActionType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitDefinitions_UnitType_Race",
                table: "UnitDefinitions",
                columns: new[] { "UnitType", "Race" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveSpells_Kingdoms_KingdomId",
                table: "ActiveSpells",
                column: "KingdomId",
                principalTable: "Kingdoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BattleReports_Kingdoms_AttackerKingdomId",
                table: "BattleReports",
                column: "AttackerKingdomId",
                principalTable: "Kingdoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BattleReports_Kingdoms_DefenderKingdomId",
                table: "BattleReports",
                column: "DefenderKingdomId",
                principalTable: "Kingdoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Kingdoms_KingdomId",
                table: "Buildings",
                column: "KingdomId",
                principalTable: "Kingdoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Coalitions_Eras_EraId",
                table: "Coalitions",
                column: "EraId",
                principalTable: "Eras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Coalitions_Kingdoms_LeaderKingdomId",
                table: "Coalitions",
                column: "LeaderKingdomId",
                principalTable: "Kingdoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coalitions_Kingdoms_LeaderKingdomId",
                table: "Coalitions");

            migrationBuilder.DropForeignKey(
                name: "FK_Coalitions_Eras_EraId",
                table: "Coalitions");

            migrationBuilder.DropTable(
                name: "ActiveSpells");

            migrationBuilder.DropTable(
                name: "BattleReports");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "MilitaryUnits");

            migrationBuilder.DropTable(
                name: "Pantheon");

            migrationBuilder.DropTable(
                name: "Professions");

            migrationBuilder.DropTable(
                name: "QueuedActions");

            migrationBuilder.DropTable(
                name: "Research");

            migrationBuilder.DropTable(
                name: "ThiefActionDefinitions");

            migrationBuilder.DropTable(
                name: "SpellDefinitions");

            migrationBuilder.DropTable(
                name: "BuildingDefinitions");

            migrationBuilder.DropTable(
                name: "UnitDefinitions");

            migrationBuilder.DropTable(
                name: "TechnologyDefinitions");

            migrationBuilder.DropTable(
                name: "Kingdoms");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Eras");

            migrationBuilder.DropTable(
                name: "Coalitions");
        }
    }
}
