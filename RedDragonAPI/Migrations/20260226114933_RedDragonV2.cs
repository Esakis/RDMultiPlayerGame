using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class RedDragonV2 : Migration
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
                    CostBudulec = table.Column<int>(type: "int", nullable: false),
                    CostLand = table.Column<int>(type: "int", nullable: false),
                    BuildTime = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Col = table.Column<int>(type: "int", nullable: false),
                    BaseCost = table.Column<int>(type: "int", nullable: false),
                    RequiredBuildingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequiredTechnology = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSpecial = table.Column<bool>(type: "bit", nullable: false),
                    BonusTurnsPerDay = table.Column<int>(type: "int", nullable: false),
                    ProductionBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DefenseBonus = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PopulationCapacity = table.Column<int>(type: "int", nullable: false),
                    WorkshopCapacity = table.Column<int>(type: "int", nullable: false)
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
                    CostWeapons = table.Column<int>(type: "int", nullable: false),
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
                    IsMagicRace = table.Column<bool>(type: "bit", nullable: false),
                    Land = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false),
                    Food = table.Column<long>(type: "bigint", nullable: false),
                    Stone = table.Column<long>(type: "bigint", nullable: false),
                    Budulec = table.Column<long>(type: "bigint", nullable: false),
                    BudulecStored = table.Column<long>(type: "bigint", nullable: false),
                    Weapons = table.Column<long>(type: "bigint", nullable: false),
                    Mana = table.Column<long>(type: "bigint", nullable: false),
                    Population = table.Column<int>(type: "int", nullable: false),
                    Popularity = table.Column<int>(type: "int", nullable: false),
                    Wages = table.Column<int>(type: "int", nullable: false),
                    Education = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TurnsAvailable = table.Column<int>(type: "int", nullable: false),
                    TurnsPerDay = table.Column<int>(type: "int", nullable: false),
                    MaxTurns = table.Column<int>(type: "int", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    CurrentSpecialBuilding = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecialBuildingProgress = table.Column<int>(type: "int", nullable: false),
                    SpecialBuildingCost = table.Column<int>(type: "int", nullable: false),
                    CoalitionId = table.Column<int>(type: "int", nullable: true),
                    CoalitionRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EraId = table.Column<int>(type: "int", nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    ProtectionDaysLeft = table.Column<int>(type: "int", nullable: false),
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
                    NoviceCount = table.Column<int>(type: "int", nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    ProductionPerTurn = table.Column<long>(type: "bigint", nullable: false),
                    NovicePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
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
                columns: new[] { "Id", "BaseCost", "BonusTurnsPerDay", "BuildTime", "BuildingType", "Category", "Col", "CostBudulec", "CostGold", "CostLand", "DefenseBonus", "Description", "DisplayName", "IsSpecial", "PopulationCapacity", "ProductionBonus", "RequiredBuildingType", "RequiredTechnology", "Row", "WorkshopCapacity" },
                values: new object[,]
                {
                    { 1, 0, 0, 1, "Domy", "Gospodarcze", 0, 1, 100, 1, 0m, "Zwiększa limit ludności", "Domy", false, 100, 0m, null, null, 0, 0 },
                    { 2, 0, 0, 1, "WarsztatAlchemiczny", "Warsztaty", 0, 1, 100, 1, 0m, "Miejsce pracy alchemików (100 miejsc)", "Laboratorium alchemiczne", false, 0, 0m, null, null, 0, 100 },
                    { 3, 0, 0, 1, "Gospodarstwo", "Warsztaty", 0, 1, 100, 1, 0m, "Miejsce pracy chłopów (100 miejsc)", "Gospodarstwo", false, 0, 0m, null, null, 0, 100 },
                    { 4, 0, 0, 1, "LasyDruidow", "Warsztaty", 0, 1, 100, 1, 0m, "Miejsce pracy druidów (100 miejsc)", "Lasy Druidów", false, 0, 0m, null, null, 0, 100 },
                    { 5, 0, 0, 1, "ZakladyKamieniarskie", "Warsztaty", 0, 1, 100, 1, 0m, "Miejsce pracy kamieniarzy (100 miejsc)", "Zakłady Kamieniarskie", false, 0, 0m, null, null, 0, 100 },
                    { 6, 0, 0, 1, "WarsztatyMurarskie", "Warsztaty", 0, 1, 100, 1, 0m, "Miejsce pracy murarzy (100 miejsc)", "Warsztaty murarskie", false, 0, 0m, null, null, 0, 100 },
                    { 7, 0, 0, 1, "Zbrojownie", "Warsztaty", 0, 1, 100, 1, 0m, "Miejsce pracy płatnerzy (100 miejsc)", "Zbrojownie", false, 0, 0m, null, null, 0, 100 },
                    { 8, 0, 0, 1, "CechSlonca", "Cechy", 0, 1, 200, 1, 0m, "Bonus produkcji alchemików i chłopów", "Cech słońca", false, 0, 0.05m, null, null, 0, 0 },
                    { 9, 0, 0, 1, "CechZiemi", "Cechy", 0, 1, 200, 1, 0m, "Bonus produkcji druidów i kamieniarzy", "Cech ziemi", false, 0, 0.05m, null, null, 0, 0 },
                    { 10, 0, 0, 1, "CechGwiazd", "Cechy", 0, 1, 200, 1, 0m, "Bonus produkcji murarzy i płatnerzy", "Cech gwiazd", false, 0, 0.05m, null, null, 0, 0 },
                    { 11, 0, 0, 1, "Manufaktura", "Manufaktury", 0, 1, 300, 1, 0m, "Automatycznie produkuje surowce", "Manufaktura", false, 0, 0.10m, null, null, 0, 0 },
                    { 12, 0, 0, 1, "Szkoly", "Pozostale", 0, 1, 200, 1, 0m, "Przyspiesza szkolenie nowicjuszy", "Szkoły", false, 0, 0m, null, null, 0, 0 },
                    { 13, 0, 0, 1, "WiezeObronne", "Obrona", 0, 1, 300, 1, 0.03m, "Pomagają w obronie księstwa", "Wieże obronne", false, 0, 0m, null, null, 0, 0 },
                    { 14, 0, 0, 1, "KonstrukcjaMachin", "Wojskowe", 0, 1, 500, 1, 0m, "Budowa machin wojennych", "Konstrukcja machin bojowych", false, 0, 0m, null, null, 0, 0 },
                    { 101, 500, 0, 1, "ZajazdCzerwonego", "Specjalne", 1, 1, 0, 0, 0m, "Obniża wymaganą pensję do 42 dla 100% popularności", "Zajazd u Czerwonego Smoka", true, 0, 0m, null, null, 1, 0 },
                    { 102, 500, 0, 1, "Mlyn", "Specjalne", 2, 1, 0, 0, 0m, "Bonus do produkcji chłopów", "Młyn", true, 0, 0.10m, null, null, 1, 0 },
                    { 103, 500, 0, 1, "Ratusz", "Specjalne", 3, 1, 0, 0, 0m, "Dodatkowe złoto z podatków", "Ratusz", true, 0, 0m, null, null, 1, 0 },
                    { 104, 500, 0, 1, "KondensatorMagiczny", "Specjalne", 4, 1, 0, 0, 0m, "Bonus do produkcji many", "Kondensator magiczny", true, 0, 0.10m, null, null, 1, 0 },
                    { 105, 500, 0, 1, "SztabUderzeniowy", "Specjalne", 5, 1, 0, 0, 0m, "Bonus do siły ataku", "Sztab uderzeniowy", true, 0, 0m, null, null, 1, 0 },
                    { 106, 500, 0, 1, "Szaniec", "Specjalne", 6, 1, 0, 0, 0.05m, "Podstawowa obrona specjalna", "Szaniec", true, 0, 0m, null, null, 1, 0 },
                    { 201, 5000, 0, 2, "RezydencjaGenerala", "Specjalne", 1, 1, 0, 0, 0m, "Umożliwia posiadanie generała", "Rezydencja Generała", true, 0, 0m, "ZajazdCzerwonego", null, 2, 0 },
                    { 202, 5000, 0, 2, "KopalniaZlota", "Specjalne", 2, 1, 0, 0, 0m, "Szansa na znalezienie skarbu", "Kopalnia złota", true, 0, 0m, "Mlyn", null, 2, 0 },
                    { 203, 5000, 0, 2, "RenowacjaBroni", "Specjalne", 3, 1, 0, 0, 0m, "Zmniejsza zużycie broni", "Renowacja broni", true, 0, 0m, "Ratusz", null, 2, 0 },
                    { 204, 5000, 0, 2, "TajemnicaOdtworzenia", "Specjalne", 4, 1, 0, 0, 0m, "Regeneracja many", "Tajemnica Odtworzenia", true, 0, 0m, "KondensatorMagiczny", null, 2, 0 },
                    { 205, 5000, 0, 2, "Szpital", "Specjalne", 5, 1, 0, 0, 0m, "Leczenie rannych po walce", "Szpital", true, 0, 0m, "SztabUderzeniowy", null, 2, 0 },
                    { 206, 5000, 0, 2, "SmoczyMur", "Specjalne", 6, 1, 0, 0, 0.10m, "Bonus obrony", "Smoczy mur", true, 0, 0m, "Szaniec", null, 2, 0 },
                    { 301, 20000, 0, 3, "LazniaMiejska", "Specjalne", 1, 1, 0, 0, 0m, "Bonus do zaludnienia", "Łaźnia miejska", true, 500, 0m, "RezydencjaGenerala", null, 3, 0 },
                    { 302, 20000, 0, 3, "KlubOdkrywcow", "Specjalne", 2, 1, 0, 0, 0m, "Bonus do nauki", "Klub odkrywców", true, 0, 0m, "KopalniaZlota", null, 3, 0 },
                    { 303, 20000, 0, 3, "SwiatyniaAutora", "Specjalne", 3, 1, 0, 0, 0m, "Bonus do złota", "Świątynia bogactwa Autora", true, 0, 0m, "RenowacjaBroni", null, 3, 0 },
                    { 304, 20000, 0, 3, "SoczewkaMagiczna", "Specjalne", 4, 1, 0, 0, 0m, "Bonus do mocy czarów", "Soczewka magiczna", true, 0, 0m, "TajemnicaOdtworzenia", null, 3, 0 },
                    { 305, 20000, 0, 3, "OltarzInicjacji", "Specjalne", 5, 1, 0, 0, 0m, "Bonus do szkolenia wojsk", "Ołtarz Inicjacji", true, 0, 0m, "Szpital", null, 3, 0 },
                    { 306, 20000, 0, 3, "SmoczaBariera", "Specjalne", 6, 1, 0, 0, 0.15m, "Silna obrona magiczna", "Smocza bariera", true, 0, 0m, "SmoczyMur", null, 3, 0 },
                    { 401, 50000, 0, 4, "SystemJaskin", "Specjalne", 1, 1, 0, 0, 0m, "Ukrywa zasoby przed wrogami", "System jaskiń", true, 0, 0m, "LazniaMiejska", null, 4, 0 },
                    { 402, 50000, 0, 4, "SkrzyzowanieSzlakow", "Specjalne", 2, 1, 0, 0, 0m, "Bonus do handlu", "Skrzyżowanie szlaków handlowych", true, 0, 0m, "KlubOdkrywcow", null, 4, 0 },
                    { 403, 50000, 0, 4, "GildiaZlodziei", "Specjalne", 3, 1, 0, 0, 0m, "Odblokowanie akcji złodziejskich", "Gildia Złodziei", true, 0, 0m, "SwiatyniaAutora", null, 4, 0 },
                    { 404, 50000, 0, 4, "ScianyMagiczne", "Specjalne", 4, 1, 0, 0, 0m, "Obrona przed magią", "Ściany magiczne", true, 0, 0m, "SoczewkaMagiczna", null, 4, 0 },
                    { 405, 50000, 0, 4, "PlacDefilad", "Specjalne", 5, 1, 0, 0, 0m, "Bonus do morale armii", "Plac defilad", true, 0, 0m, "OltarzInicjacji", null, 4, 0 },
                    { 406, 50000, 0, 4, "Zamek", "Specjalne", 6, 1, 0, 0, 0.20m, "Potężna obrona", "Zamek", true, 0, 0m, "SmoczaBariera", null, 4, 0 },
                    { 501, 85000, 0, 5, "Akwedukt", "Specjalne", 1, 1, 0, 0, 0m, "Duży bonus zaludnienia", "Akwedukt", true, 1000, 0m, "SystemJaskin", null, 5, 0 },
                    { 502, 85000, 1, 5, "ZachodniaWiezaCzasu", "Specjalne", 2, 1, 0, 0, 0m, "+1 tura dziennie", "Zachodnia wieża czasu", true, 0, 0m, "SkrzyzowanieSzlakow", null, 5, 0 },
                    { 503, 85000, 0, 5, "Smokodrap", "Specjalne", 3, 1, 0, 0, 0m, "Przyciąga smoki", "Smokodrap", true, 0, 0m, "GildiaZlodziei", null, 5, 0 },
                    { 504, 85000, 0, 5, "LustroMagiczne", "Specjalne", 4, 1, 0, 0, 0m, "Odbija czary wroga", "Lustro magiczne", true, 0, 0m, "ScianyMagiczne", null, 5, 0 },
                    { 505, 85000, 0, 5, "AkademiaWojskowa", "Specjalne", 5, 1, 0, 0, 0m, "Bonus do siły armii", "Akademia wojskowa", true, 0, 0m, "PlacDefilad", null, 5, 0 },
                    { 506, 85000, 0, 5, "SiecFortec", "Specjalne", 6, 1, 0, 0, 0.25m, "Potężna obrona fortyfikacyjna", "Sieć wojennych fortec", true, 0, 0m, "Zamek", null, 5, 0 },
                    { 601, 110000, 0, 6, "Kanalizacja", "Specjalne", 1, 1, 0, 0, 0m, "Maksymalny bonus zaludnienia", "Kanalizacja", true, 2000, 0m, "Akwedukt", null, 6, 0 },
                    { 602, 110000, 1, 6, "WschodniaWiezaCzasu", "Specjalne", 2, 1, 0, 0, 0m, "+1 tura dziennie", "Wschodnia wieża czasu", true, 0, 0m, "ZachodniaWiezaCzasu", null, 6, 0 },
                    { 603, 110000, 0, 6, "Portal", "Specjalne", 3, 1, 0, 0, 0m, "Zaawansowane zdolności smoków", "Portal", true, 0, 0m, "Smokodrap", null, 6, 0 },
                    { 604, 110000, 0, 6, "PalacMagiczny", "Specjalne", 4, 1, 0, 0, 0m, "Najsilniejsza magia", "Pałac magiczny", true, 0, 0m, "LustroMagiczne", null, 6, 0 },
                    { 605, 110000, 0, 6, "KoszarySpecjalne", "Specjalne", 5, 1, 0, 0, 0m, "Elitarne jednostki wojskowe", "Koszary", true, 0, 0m, "AkademiaWojskowa", null, 6, 0 },
                    { 606, 110000, 0, 6, "PospoliteRuszenie", "Specjalne", 6, 1, 0, 0, 0.30m, "Ludność walczy w obronie", "Pospolite ruszenie", true, 0, 0m, "SiecFortec", null, 6, 0 },
                    { 701, 200000, 0, 7, "MinisterstwoSmokow", "Specjalne", 1, 1, 0, 0, 0m, "Pełna kontrola nad smokami", "Ministerstwo smoków", true, 0, 0m, "Portal", null, 7, 0 },
                    { 702, 200000, 0, 7, "SanktuariumBerserkerow", "Specjalne", 2, 1, 0, 0, 0m, "Najsilniejsze jednostki wojskowe", "Sanktuarium berserkerów", true, 0, 0m, "KoszarySpecjalne", null, 7, 0 },
                    { 703, 200000, 0, 7, "KlasztorMnichow", "Specjalne", 3, 1, 0, 0, 0.40m, "Ostateczna obrona", "Klasztor Smoczych Mnichów", true, 0, 0m, "PospoliteRuszenie", null, 7, 0 }
                });

            migrationBuilder.InsertData(
                table: "Eras",
                columns: new[] { "Id", "EndedAt", "IsActive", "Name", "StartedAt", "Theme", "WinningCoalitionId" },
                values: new object[] { 1, null, true, "Era Przebudzenia", new DateTime(2026, 2, 26, 11, 49, 32, 968, DateTimeKind.Utc).AddTicks(7116), "Pierwsza era nowego świata Red Dragon", null });

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
                    { 1, "Ekonomia", 3000, "Odblokowanie machin wojennych", "Konstrukcja maszyn drewnianych", "UnlockSiege", 1.0m, 1, null, null, 5, "KonstrukcjaMaszyn" },
                    { 2, "Nauka", 5000, "Bonus do efektywności naukowców", "Empiryzm", "ScienceBonus", 0.10m, 1, null, null, 8, "Empiryzm" },
                    { 3, "Czas", 20000, "+1 tura dziennie", "Zakrzywienie czasu", "BonusTurns", 1.0m, 1, null, null, 15, "ZakrzywCzasu" },
                    { 4, "Czas", 50000, "+1 dodatkowa tura dziennie", "Załamanie czasu", "BonusTurns", 1.0m, 2, null, "ZakrzywCzasu", 25, "ZalamCzasu" },
                    { 5, "Ziemia", 5000, "Tańsze kupowanie ziemi", "Rekultywacja", "LandCostReduction", 0.10m, 1, null, null, 8, "Rekultywacja" },
                    { 6, "Ziemia", 15000, "Jeszcze tańsza ziemia", "Osadnictwo", "LandCostReduction", 0.20m, 2, null, "Rekultywacja", 15, "Osadnictwo" },
                    { 7, "Ziemia", 30000, "Bonus do kamienia", "Górnictwo Odkrywkowe", "StoneBonus", 0.30m, 3, null, "Osadnictwo", 20, "GornictwoOdkrywkowe" },
                    { 8, "Smoki", 8000, "Podstawowa wiedza o smokach", "Smokoastronomia", "DragonKnowledge", 1.0m, 1, null, null, 10, "Smokoastronomia" },
                    { 9, "Smoki", 20000, "Znajomość anatomii smoków", "Smokoanatomia", "DragonKnowledge", 2.0m, 2, null, "Smokoastronomia", 18, "Smokoanatomia" },
                    { 10, "Smoki", 45000, "Pełna kontrola nad smokami", "Smokodynamika", "DragonKnowledge", 3.0m, 3, null, "Smokoanatomia", 25, "Smokodynamika" },
                    { 11, "Ekonomia", 5000, "Bonus do złota z kupców", "Rachunkowość", "MerchantBonus", 0.10m, 1, null, null, 8, "Rachunkowosc" },
                    { 12, "Ekonomia", 15000, "Większy bonus do handlu", "Buchalteria", "MerchantBonus", 0.20m, 2, null, "Rachunkowosc", 15, "Buchalteria" },
                    { 13, "Ekonomia", 35000, "Maksymalny bonus handlowy", "Księgowość", "MerchantBonus", 0.30m, 3, null, "Buchalteria", 22, "Ksiegowosc" },
                    { 14, "Wojsko", 8000, "Bonus do ataku", "Ostrzenie broni", "AttackBonus", 0.10m, 1, null, null, 10, "OstrzenieBroni" },
                    { 15, "Wojsko", 20000, "Większy bonus do ataku", "Naprawa broni", "AttackBonus", 0.20m, 2, null, "OstrzenieBroni", 18, "NaprawaBroni" },
                    { 16, "Wojsko", 45000, "Maksymalny bonus do ataku", "Przekuwanie broni", "AttackBonus", 0.30m, 3, null, "NaprawaBroni", 25, "PrzekuwanieBroni" },
                    { 17, "Nauka", 5000, "Bonus do produkcji", "Wynalazczość I", "ProductionBonus", 0.05m, 1, null, null, 8, "Wynalazki1" },
                    { 18, "Nauka", 12000, null, "Wynalazczość II", "ProductionBonus", 0.10m, 2, null, "Wynalazki1", 12, "Wynalazki2" },
                    { 19, "Nauka", 25000, null, "Wynalazczość III", "ProductionBonus", 0.15m, 3, null, "Wynalazki2", 18, "Wynalazki3" },
                    { 20, "Nauka", 45000, null, "Wynalazczość IV", "ProductionBonus", 0.20m, 4, null, "Wynalazki3", 25, "Wynalazki4" },
                    { 21, "Nauka", 80000, null, "Wynalazczość V", "ProductionBonus", 0.25m, 5, null, "Wynalazki4", 35, "Wynalazki5" },
                    { 22, "Budowa", 5000, "Tańsze budynki specjalne", "Architektura I", "SpecialBuildingCostReduction", 0.05m, 1, null, null, 8, "Architektura1" },
                    { 23, "Budowa", 12000, null, "Architektura II", "SpecialBuildingCostReduction", 0.10m, 2, null, "Architektura1", 12, "Architektura2" },
                    { 24, "Budowa", 25000, null, "Architektura III", "SpecialBuildingCostReduction", 0.15m, 3, null, "Architektura2", 18, "Architektura3" },
                    { 25, "Budowa", 45000, "Brak kosztu złota pod czarną magią + 50% taniej przyspieszanie", "Architektura IV", "SpecialBuildingCostReduction", 0.20m, 4, null, "Architektura3", 25, "Architektura4" },
                    { 26, "Budowa", 80000, null, "Architektura V", "SpecialBuildingCostReduction", 0.25m, 5, null, "Architektura4", 35, "Architektura5" },
                    { 27, "Budowa", 5000, "Tańsze budynki gospodarcze", "Inżynieria I", "EcoBuildingCostReduction", 0.05m, 1, null, null, 8, "Inzynieria1" },
                    { 28, "Budowa", 12000, null, "Inżynieria II", "EcoBuildingCostReduction", 0.10m, 2, null, "Inzynieria1", 12, "Inzynieria2" },
                    { 29, "Budowa", 25000, null, "Inżynieria III", "EcoBuildingCostReduction", 0.15m, 3, null, "Inzynieria2", 18, "Inzynieria3" },
                    { 30, "Budowa", 45000, null, "Inżynieria IV", "EcoBuildingCostReduction", 0.20m, 4, null, "Inzynieria3", 25, "Inzynieria4" },
                    { 31, "Budowa", 80000, null, "Inżynieria V", "EcoBuildingCostReduction", 0.25m, 5, null, "Inzynieria4", 35, "Inzynieria5" },
                    { 32, "Magia", 8000, "Bonus do mocy czarów", "Czarodziejstwo I", "SpellPower", 0.10m, 1, null, null, 10, "Czarodziejstwo1" },
                    { 33, "Magia", 18000, null, "Czarodziejstwo II", "SpellPower", 0.20m, 2, null, "Czarodziejstwo1", 16, "Czarodziejstwo2" },
                    { 34, "Magia", 35000, null, "Czarodziejstwo III", "SpellPower", 0.30m, 3, null, "Czarodziejstwo2", 22, "Czarodziejstwo3" },
                    { 35, "Magia", 55000, null, "Czarodziejstwo IV", "SpellPower", 0.40m, 4, null, "Czarodziejstwo3", 28, "Czarodziejstwo4" },
                    { 36, "Magia", 90000, null, "Czarodziejstwo V", "SpellPower", 0.50m, 5, null, "Czarodziejstwo4", 38, "Czarodziejstwo5" },
                    { 37, "Wojsko", 5000, "Szybsze szkolenie wojsk", "Trening I", "TrainingSpeed", 0.10m, 1, null, null, 8, "Trening1" },
                    { 38, "Wojsko", 12000, null, "Trening II", "TrainingSpeed", 0.20m, 2, null, "Trening1", 12, "Trening2" },
                    { 39, "Wojsko", 25000, null, "Trening III", "TrainingSpeed", 0.30m, 3, null, "Trening2", 18, "Trening3" },
                    { 40, "Wojsko", 45000, null, "Trening IV", "TrainingSpeed", 0.40m, 4, null, "Trening3", 25, "Trening4" },
                    { 41, "Wojsko", 80000, null, "Trening V", "TrainingSpeed", 0.50m, 5, null, "Trening4", 35, "Trening5" },
                    { 42, "Wojsko", 5000, "Tańsza rekrutacja", "Rekrutacja I", "RecruitCostReduction", 0.05m, 1, null, null, 8, "Rekrutacja1" },
                    { 43, "Wojsko", 12000, null, "Rekrutacja II", "RecruitCostReduction", 0.10m, 2, null, "Rekrutacja1", 12, "Rekrutacja2" },
                    { 44, "Wojsko", 25000, null, "Rekrutacja III", "RecruitCostReduction", 0.15m, 3, null, "Rekrutacja2", 18, "Rekrutacja3" },
                    { 45, "Wojsko", 45000, null, "Rekrutacja IV", "RecruitCostReduction", 0.20m, 4, null, "Rekrutacja3", 25, "Rekrutacja4" },
                    { 46, "Wojsko", 80000, null, "Rekrutacja V", "RecruitCostReduction", 0.25m, 5, null, "Rekrutacja4", 35, "Rekrutacja5" }
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
