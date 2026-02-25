using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Kingdom> Kingdoms { get; set; }
    public DbSet<Era> Eras { get; set; }
    public DbSet<Coalition> Coalitions { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<BuildingDefinition> BuildingDefinitions { get; set; }
    public DbSet<MilitaryUnit> MilitaryUnits { get; set; }
    public DbSet<UnitDefinition> UnitDefinitions { get; set; }
    public DbSet<Profession> Professions { get; set; }
    public DbSet<Research> Researches { get; set; }
    public DbSet<TechnologyDefinition> TechnologyDefinitions { get; set; }
    public DbSet<QueuedAction> QueuedActions { get; set; }
    public DbSet<BattleReport> BattleReports { get; set; }
    public DbSet<SpellDefinition> SpellDefinitions { get; set; }
    public DbSet<ActiveSpell> ActiveSpells { get; set; }
    public DbSet<ThiefActionDefinition> ThiefActionDefinitions { get; set; }
    public DbSet<Pantheon> Pantheons { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRelationships(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureUniqueConstraints(modelBuilder);
        SeedEras(modelBuilder);
        SeedBuildingDefinitions(modelBuilder);
        SeedUnitDefinitions(modelBuilder);
        SeedTechnologyDefinitions(modelBuilder);
        SeedSpellDefinitions(modelBuilder);
        SeedThiefActionDefinitions(modelBuilder);
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // User -> Kingdoms
        modelBuilder.Entity<Kingdom>()
            .HasOne(k => k.User)
            .WithMany(u => u.Kingdoms)
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kingdom -> Era
        modelBuilder.Entity<Kingdom>()
            .HasOne(k => k.Era)
            .WithMany(e => e.Kingdoms)
            .HasForeignKey(k => k.EraId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kingdom -> Coalition
        modelBuilder.Entity<Kingdom>()
            .HasOne(k => k.Coalition)
            .WithMany(c => c.Members)
            .HasForeignKey(k => k.CoalitionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Coalition -> Era
        modelBuilder.Entity<Coalition>()
            .HasOne(c => c.Era)
            .WithMany(e => e.Coalitions)
            .HasForeignKey(c => c.EraId)
            .OnDelete(DeleteBehavior.Restrict);

        // Coalition -> Leader
        modelBuilder.Entity<Coalition>()
            .HasOne(c => c.Leader)
            .WithMany()
            .HasForeignKey(c => c.LeaderKingdomId)
            .OnDelete(DeleteBehavior.SetNull);

        // Building -> Kingdom
        modelBuilder.Entity<Building>()
            .HasOne(b => b.Kingdom)
            .WithMany(k => k.Buildings)
            .HasForeignKey(b => b.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Building -> BuildingDefinition
        modelBuilder.Entity<Building>()
            .HasOne(b => b.Definition)
            .WithMany()
            .HasForeignKey(b => b.BuildingType)
            .HasPrincipalKey(bd => bd.BuildingType)
            .OnDelete(DeleteBehavior.Restrict);

        // MilitaryUnit -> Kingdom
        modelBuilder.Entity<MilitaryUnit>()
            .HasOne(m => m.Kingdom)
            .WithMany(k => k.MilitaryUnits)
            .HasForeignKey(m => m.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // MilitaryUnit -> UnitDefinition
        modelBuilder.Entity<MilitaryUnit>()
            .HasOne(m => m.Definition)
            .WithMany()
            .HasForeignKey(m => m.UnitType)
            .HasPrincipalKey(ud => ud.UnitType)
            .OnDelete(DeleteBehavior.Restrict);

        // Profession -> Kingdom
        modelBuilder.Entity<Profession>()
            .HasOne(p => p.Kingdom)
            .WithMany(k => k.Professions)
            .HasForeignKey(p => p.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Research -> Kingdom
        modelBuilder.Entity<Research>()
            .HasOne(r => r.Kingdom)
            .WithMany(k => k.Researches)
            .HasForeignKey(r => r.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Research -> TechnologyDefinition
        modelBuilder.Entity<Research>()
            .HasOne(r => r.Tech)
            .WithMany()
            .HasForeignKey(r => r.TechType)
            .HasPrincipalKey(td => td.TechType)
            .OnDelete(DeleteBehavior.Restrict);

        // ActiveSpell -> Kingdom
        modelBuilder.Entity<ActiveSpell>()
            .HasOne(a => a.Kingdom)
            .WithMany(k => k.ActiveSpells)
            .HasForeignKey(a => a.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // ActiveSpell -> SpellDefinition
        modelBuilder.Entity<ActiveSpell>()
            .HasOne(a => a.Spell)
            .WithMany()
            .HasForeignKey(a => a.SpellType)
            .HasPrincipalKey(sd => sd.SpellType)
            .OnDelete(DeleteBehavior.Restrict);

        // QueuedAction -> Kingdom
        modelBuilder.Entity<QueuedAction>()
            .HasOne(q => q.Kingdom)
            .WithMany()
            .HasForeignKey(q => q.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // QueuedAction -> TargetKingdom
        modelBuilder.Entity<QueuedAction>()
            .HasOne(q => q.TargetKingdom)
            .WithMany()
            .HasForeignKey(q => q.TargetKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        // BattleReport -> AttackerKingdom
        modelBuilder.Entity<BattleReport>()
            .HasOne(b => b.AttackerKingdom)
            .WithMany()
            .HasForeignKey(b => b.AttackerKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        // BattleReport -> DefenderKingdom
        modelBuilder.Entity<BattleReport>()
            .HasOne(b => b.DefenderKingdom)
            .WithMany()
            .HasForeignKey(b => b.DefenderKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message -> Sender/Receiver
        modelBuilder.Entity<Message>()
            .HasOne(m => m.SenderKingdom)
            .WithMany()
            .HasForeignKey(m => m.SenderKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ReceiverKingdom)
            .WithMany()
            .HasForeignKey(m => m.ReceiverKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Pantheon
        modelBuilder.Entity<Pantheon>()
            .HasOne(p => p.Era)
            .WithMany()
            .HasForeignKey(p => p.EraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pantheon>()
            .HasOne(p => p.Coalition)
            .WithMany()
            .HasForeignKey(p => p.CoalitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Era -> WinningCoalition
        modelBuilder.Entity<Era>()
            .HasOne(e => e.WinningCoalition)
            .WithMany()
            .HasForeignKey(e => e.WinningCoalitionId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kingdom>().HasIndex(k => k.UserId);
        modelBuilder.Entity<Kingdom>().HasIndex(k => k.CoalitionId);
        modelBuilder.Entity<Kingdom>().HasIndex(k => k.EraId);
        modelBuilder.Entity<QueuedAction>().HasIndex(q => new { q.ScheduledFor, q.Status });
        modelBuilder.Entity<QueuedAction>().HasIndex(q => q.KingdomId);
        modelBuilder.Entity<Building>().HasIndex(b => b.KingdomId);
        modelBuilder.Entity<MilitaryUnit>().HasIndex(m => m.KingdomId);
        modelBuilder.Entity<BattleReport>().HasIndex(b => b.AttackerKingdomId);
        modelBuilder.Entity<BattleReport>().HasIndex(b => b.DefenderKingdomId);
    }

    private void ConfigureUniqueConstraints(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<BuildingDefinition>().HasIndex(b => b.BuildingType).IsUnique();
        modelBuilder.Entity<TechnologyDefinition>().HasIndex(t => t.TechType).IsUnique();
        modelBuilder.Entity<SpellDefinition>().HasIndex(s => s.SpellType).IsUnique();
        modelBuilder.Entity<ThiefActionDefinition>().HasIndex(t => t.ActionType).IsUnique();
        modelBuilder.Entity<Profession>().HasIndex(p => new { p.KingdomId, p.ProfessionType }).IsUnique();
        modelBuilder.Entity<Research>().HasIndex(r => new { r.KingdomId, r.TechType }).IsUnique();
        modelBuilder.Entity<UnitDefinition>().HasIndex(u => new { u.UnitType, u.Race }).IsUnique();
        modelBuilder.Entity<Pantheon>().HasIndex(p => new { p.EraId, p.CoalitionId }).IsUnique();
    }

    private void SeedEras(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Era>().HasData(
            new Era
            {
                Id = 1,
                Name = "Era Przebudzenia",
                Theme = "Pierwsza era nowego świata Red Dragon",
                StartedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }

    private void SeedBuildingDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuildingDefinition>().HasData(
            // MIESZKALNE
            new BuildingDefinition
            {
                Id = 1, BuildingType = "House", Category = "Residential",
                DisplayName = "Dom",
                Description = "Zwiększa limit ludności o 100",
                CostWood = 50, CostStone = 20, CostLand = 2,
                BuildTime = 1, PopulationCapacity = 100
            },
            // EKONOMICZNE
            new BuildingDefinition
            {
                Id = 2, BuildingType = "Farm", Category = "Economic",
                DisplayName = "Farma",
                Description = "Produkuje 500 żywności na turę",
                CostWood = 40, CostGold = 10, CostLand = 3,
                BuildTime = 1
            },
            new BuildingDefinition
            {
                Id = 3, BuildingType = "Sawmill", Category = "Economic",
                DisplayName = "Tartak",
                Description = "Produkuje 300 drewna na turę",
                CostWood = 60, CostGold = 30, CostLand = 2,
                BuildTime = 2
            },
            new BuildingDefinition
            {
                Id = 4, BuildingType = "Quarry", Category = "Economic",
                DisplayName = "Kamieniołom",
                Description = "Wydobywa kamień (wymaga kamieniarzy)",
                CostWood = 50, CostGold = 50, CostLand = 3,
                BuildTime = 2
            },
            new BuildingDefinition
            {
                Id = 5, BuildingType = "Mine", Category = "Economic",
                DisplayName = "Kopalnia",
                Description = "Produkuje 200 żelaza na turę",
                CostWood = 100, CostStone = 50, CostGold = 100, CostLand = 4,
                BuildTime = 3
            },
            new BuildingDefinition
            {
                Id = 6, BuildingType = "Forge", Category = "Economic",
                DisplayName = "Kuźnia",
                Description = "Przekształca żelazo w broń",
                CostWood = 80, CostStone = 100, CostGold = 150, CostLand = 2,
                BuildTime = 3
            },
            new BuildingDefinition
            {
                Id = 7, BuildingType = "Workshop", Category = "Economic",
                DisplayName = "Warsztat",
                Description = "Bonus +10% do produkcji surowców",
                CostWood = 50, CostStone = 30, CostGold = 50, CostLand = 2,
                BuildTime = 2,
                ProductionBonus = 0.10m
            },
            new BuildingDefinition
            {
                Id = 8, BuildingType = "Manufactory", Category = "Economic",
                DisplayName = "Manufaktura",
                Description = "Bonus +20% do produkcji zaawansowanej",
                CostWood = 150, CostStone = 200, CostGold = 500, CostLand = 4,
                BuildTime = 5,
                RequiredBuildingType = "Workshop",
                ProductionBonus = 0.20m
            },
            // NAUKOWE
            new BuildingDefinition
            {
                Id = 9, BuildingType = "University", Category = "Scientific",
                DisplayName = "Uniwersytet",
                Description = "Umożliwia badania technologiczne",
                CostWood = 200, CostStone = 300, CostGold = 1000, CostLand = 5,
                BuildTime = 5
            },
            // WOJSKOWE
            new BuildingDefinition
            {
                Id = 10, BuildingType = "Barracks", Category = "Military",
                DisplayName = "Koszary",
                Description = "Szkolenie piechoty",
                CostWood = 150, CostStone = 150, CostGold = 300, CostLand = 3,
                BuildTime = 3
            },
            new BuildingDefinition
            {
                Id = 11, BuildingType = "ArcheryRange", Category = "Military",
                DisplayName = "Strzelnica",
                Description = "Szkolenie łuczników",
                CostWood = 120, CostStone = 80, CostGold = 250, CostLand = 3,
                BuildTime = 3
            },
            new BuildingDefinition
            {
                Id = 12, BuildingType = "Stable", Category = "Military",
                DisplayName = "Stajnia",
                Description = "Szkolenie kawalerii",
                CostWood = 200, CostStone = 150, CostGold = 500, CostLand = 4,
                BuildTime = 4
            },
            // OBRONNE
            new BuildingDefinition
            {
                Id = 13, BuildingType = "Wall", Category = "Defensive",
                DisplayName = "Mur",
                Description = "Bonus obronny +5% per poziom (max 10)",
                CostStone = 100, CostGold = 200, CostLand = 1,
                BuildTime = 2,
                DefenseBonus = 0.05m
            },
            new BuildingDefinition
            {
                Id = 14, BuildingType = "GuardTower", Category = "Defensive",
                DisplayName = "Wieża Strażnicza",
                Description = "Bonus obronny +3%, wykrywa złodziei +10%",
                CostStone = 150, CostGold = 300, CostLand = 2,
                BuildTime = 3,
                DefenseBonus = 0.03m
            },
            new BuildingDefinition
            {
                Id = 15, BuildingType = "EnchantmentPoint", Category = "Defensive",
                DisplayName = "Punkt Zaklinania (PZ)",
                Description = "Nie może być zniszczony magią ani złodziejami",
                CostStone = 500, CostGold = 1000, CostMana = 500, CostLand = 3,
                BuildTime = 10
            },
            // SPECJALNE
            new BuildingDefinition
            {
                Id = 16, BuildingType = "MerchantGuild", Category = "Special",
                DisplayName = "Gildia Kupców",
                Description = "Bonus +50% złota od kupców, +1 tura/dzień",
                CostStone = 5000, CostGold = 10000, CostLand = 5,
                BuildTime = 10, IsSpecial = true,
                BonusTurnsPerDay = 1, ProductionBonus = 0.50m
            },
            new BuildingDefinition
            {
                Id = 17, BuildingType = "MagicLibrary", Category = "Special",
                DisplayName = "Biblioteka Magiczna",
                Description = "Bonus +50% produkcji many, +1 tura/dzień",
                CostStone = 5000, CostGold = 10000, CostMana = 2000, CostLand = 5,
                BuildTime = 10, IsSpecial = true,
                BonusTurnsPerDay = 1, ProductionBonus = 0.50m
            },
            new BuildingDefinition
            {
                Id = 18, BuildingType = "MilitaryAcademy", Category = "Special",
                DisplayName = "Akademia Wojskowa",
                Description = "Bonus +20% siły armii, +1 tura/dzień",
                CostStone = 5000, CostGold = 10000, CostIron = 2000, CostLand = 5,
                BuildTime = 10, IsSpecial = true,
                BonusTurnsPerDay = 1
            },
            new BuildingDefinition
            {
                Id = 19, BuildingType = "ThievesGuild", Category = "Special",
                DisplayName = "Gildia Złodziei",
                Description = "Bonus +30% skuteczności złodziei, +1 tura/dzień",
                CostStone = 5000, CostGold = 10000, CostLand = 5,
                BuildTime = 10, IsSpecial = true,
                BonusTurnsPerDay = 1
            },
            new BuildingDefinition
            {
                Id = 20, BuildingType = "Citadel", Category = "Special",
                DisplayName = "Cytadela",
                Description = "Bonus +50% obrony, +2 tury/dzień",
                CostStone = 10000, CostGold = 15000, CostIron = 3000, CostLand = 8,
                BuildTime = 15, IsSpecial = true,
                BonusTurnsPerDay = 2, DefenseBonus = 0.50m
            }
        );
    }

    private void SeedUnitDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UnitDefinition>().HasData(
            new UnitDefinition
            {
                Id = 1, UnitType = "Infantry", Race = "Human",
                DisplayName = "Piechota",
                Description = "Podstawowe jednostki piechoty",
                CostGold = 50, CostIron = 20, CostFood = 10,
                AttackPower = 10, DefensePower = 12,
                Upkeep = 2, RequiredBuilding = "Barracks",
                TrainingTime = 1
            },
            new UnitDefinition
            {
                Id = 2, UnitType = "Archer", Race = "Human",
                DisplayName = "Łucznik",
                Description = "Jednostki dystansowe",
                CostGold = 80, CostIron = 15, CostWood = 30, CostFood = 10,
                AttackPower = 15, DefensePower = 6,
                Upkeep = 3, RequiredBuilding = "ArcheryRange",
                TrainingTime = 1
            },
            new UnitDefinition
            {
                Id = 3, UnitType = "Cavalry", Race = "Human",
                DisplayName = "Kawaleria",
                Description = "Szybkie i silne jednostki konne",
                CostGold = 200, CostIron = 40, CostFood = 20,
                AttackPower = 25, DefensePower = 20,
                Upkeep = 5, RequiredBuilding = "Stable",
                TrainingTime = 2
            },
            new UnitDefinition
            {
                Id = 4, UnitType = "SiegeEngine", Race = "Human",
                DisplayName = "Machina Oblężnicza",
                Description = "Bonus +100% vs budynki",
                CostGold = 500, CostIron = 100, CostWood = 200,
                AttackPower = 50, DefensePower = 5,
                Upkeep = 10, RequiredBuilding = "Forge",
                TrainingTime = 3
            },
            new UnitDefinition
            {
                Id = 5, UnitType = "Knight", Race = "Human",
                DisplayName = "Rycerz",
                Description = "Elitarne jednostki wojskowe",
                CostGold = 1000, CostIron = 200, CostFood = 50,
                AttackPower = 50, DefensePower = 45,
                Upkeep = 15, RequiredBuilding = "MilitaryAcademy",
                RequiredTech = "WeaponryIII",
                TrainingTime = 5
            }
        );
    }

    private void SeedTechnologyDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TechnologyDefinition>().HasData(
            // EKONOMIA
            new TechnologyDefinition { Id = 1, TechType = "AgricultureI", Category = "Economy", DisplayName = "Rolnictwo I", Description = "+20% produkcji żywności", CostGold = 5000, ResearchTime = 10, EffectType = "FoodProduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 2, TechType = "AgricultureII", Category = "Economy", DisplayName = "Rolnictwo II", Description = "+40% produkcji żywności", CostGold = 15000, ResearchTime = 20, RequiredTech = "AgricultureI", EffectType = "FoodProduction", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 3, TechType = "AgricultureIII", Category = "Economy", DisplayName = "Rolnictwo III", Description = "+60% produkcji żywności", CostGold = 40000, ResearchTime = 30, RequiredTech = "AgricultureII", EffectType = "FoodProduction", EffectValue = 0.60m },
            new TechnologyDefinition { Id = 4, TechType = "MiningI", Category = "Economy", DisplayName = "Górnictwo I", Description = "+20% produkcji kamienia i żelaza", CostGold = 5000, ResearchTime = 10, EffectType = "MiningProduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 5, TechType = "MiningII", Category = "Economy", DisplayName = "Górnictwo II", Description = "+40% produkcji kamienia i żelaza", CostGold = 15000, ResearchTime = 20, RequiredTech = "MiningI", EffectType = "MiningProduction", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 6, TechType = "MiningIII", Category = "Economy", DisplayName = "Górnictwo III", Description = "+60% produkcji kamienia i żelaza", CostGold = 40000, ResearchTime = 30, RequiredTech = "MiningII", EffectType = "MiningProduction", EffectValue = 0.60m },
            // WOJSKO
            new TechnologyDefinition { Id = 7, TechType = "WeaponryI", Category = "Military", DisplayName = "Broń I", Description = "+15% siły ataku", CostGold = 10000, ResearchTime = 15, EffectType = "AttackBonus", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 8, TechType = "WeaponryII", Category = "Military", DisplayName = "Broń II", Description = "+30% siły ataku", CostGold = 25000, ResearchTime = 25, RequiredTech = "WeaponryI", EffectType = "AttackBonus", EffectValue = 0.30m },
            new TechnologyDefinition { Id = 9, TechType = "WeaponryIII", Category = "Military", DisplayName = "Broń III", Description = "+50% siły ataku", CostGold = 50000, ResearchTime = 35, RequiredTech = "WeaponryII", EffectType = "AttackBonus", EffectValue = 0.50m },
            new TechnologyDefinition { Id = 10, TechType = "ArmorI", Category = "Military", DisplayName = "Zbroja I", Description = "+15% obrony", CostGold = 10000, ResearchTime = 15, EffectType = "DefenseBonus", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 11, TechType = "ArmorII", Category = "Military", DisplayName = "Zbroja II", Description = "+30% obrony", CostGold = 25000, ResearchTime = 25, RequiredTech = "ArmorI", EffectType = "DefenseBonus", EffectValue = 0.30m },
            new TechnologyDefinition { Id = 12, TechType = "ArmorIII", Category = "Military", DisplayName = "Zbroja III", Description = "+50% obrony", CostGold = 50000, ResearchTime = 35, RequiredTech = "ArmorII", EffectType = "DefenseBonus", EffectValue = 0.50m },
            // MAGIA
            new TechnologyDefinition { Id = 13, TechType = "WhiteMagicI", Category = "Magic", DisplayName = "Biała Magia I", Description = "+20% efektywności bufów", CostGold = 8000, ResearchTime = 12, EffectType = "BuffPower", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 14, TechType = "WhiteMagicII", Category = "Magic", DisplayName = "Biała Magia II", Description = "+40% efektywności bufów", CostGold = 20000, ResearchTime = 22, RequiredTech = "WhiteMagicI", EffectType = "BuffPower", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 15, TechType = "DestructiveMagicI", Category = "Magic", DisplayName = "Magia Niszcząca I", Description = "+15% siły czarów bojowych", CostGold = 12000, ResearchTime = 18, EffectType = "SpellPower", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 16, TechType = "DestructiveMagicII", Category = "Magic", DisplayName = "Magia Niszcząca II", Description = "+30% siły czarów bojowych", CostGold = 30000, ResearchTime = 28, RequiredTech = "DestructiveMagicI", EffectType = "SpellPower", EffectValue = 0.30m },
            new TechnologyDefinition { Id = 17, TechType = "DestructiveMagicIII", Category = "Magic", DisplayName = "Magia Niszcząca III", Description = "+50% siły czarów bojowych", CostGold = 60000, ResearchTime = 38, RequiredTech = "DestructiveMagicII", EffectType = "SpellPower", EffectValue = 0.50m },
            // ZŁODZIEJE
            new TechnologyDefinition { Id = 18, TechType = "StealthI", Category = "Thieves", DisplayName = "Skradanie I", Description = "+20% skuteczności kradzieży", CostGold = 7000, ResearchTime = 10, EffectType = "ThiefSuccess", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 19, TechType = "StealthII", Category = "Thieves", DisplayName = "Skradanie II", Description = "+40% skuteczności kradzieży", CostGold = 18000, ResearchTime = 20, RequiredTech = "StealthI", EffectType = "ThiefSuccess", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 20, TechType = "Espionage", Category = "Thieves", DisplayName = "Szpiegostwo", Description = "Umożliwia szpiegowanie wrogów", CostGold = 15000, ResearchTime = 15, RequiredTech = "StealthI", EffectType = "UnlockEspionage", EffectValue = 1.0m }
        );
    }

    private void SeedSpellDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpellDefinition>().HasData(
            new SpellDefinition { Id = 1, SpellType = "HealingLight", Category = "White", DisplayName = "Światło Uzdrowienia", Description = "Leczy rany żołnierzy po walce", ManaCost = 100, PowerLevel = 5, EffectType = "Buff", TargetType = "Self" },
            new SpellDefinition { Id = 2, SpellType = "ProtectiveAura", Category = "White", DisplayName = "Aura Ochronna", Description = "+15% obrony na 5 tur", ManaCost = 200, PowerLevel = 10, EffectType = "Buff", TargetType = "Self" },
            new SpellDefinition { Id = 3, SpellType = "ProductionBlessing", Category = "White", DisplayName = "Błogosławieństwo Produkcji", Description = "+25% produkcji na 3 tury", ManaCost = 150, PowerLevel = 8, EffectType = "Buff", TargetType = "Self" },
            new SpellDefinition { Id = 4, SpellType = "Fireball", Category = "Destructive", DisplayName = "Kula Ognia", Description = "Zadaje obrażenia armii wroga", ManaCost = 300, PowerLevel = 15, EffectType = "Damage", TargetType = "Enemy" },
            new SpellDefinition { Id = 5, SpellType = "Earthquake", Category = "Destructive", DisplayName = "Trzęsienie Ziemi", Description = "Niszczy budynki wroga", ManaCost = 500, PowerLevel = 20, EffectType = "Damage", TargetType = "Enemy" },
            new SpellDefinition { Id = 6, SpellType = "Plague", Category = "Black", DisplayName = "Zaraza", Description = "Zmniejsza populację wroga", ManaCost = 400, PowerLevel = 18, EffectType = "Debuff", TargetType = "Enemy" },
            new SpellDefinition { Id = 7, SpellType = "Curse", Category = "Black", DisplayName = "Klątwa", Description = "-20% produkcji wroga na 5 tur", ManaCost = 250, PowerLevel = 12, EffectType = "Debuff", TargetType = "Enemy" }
        );
    }

    private void SeedThiefActionDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ThiefActionDefinition>().HasData(
            new ThiefActionDefinition { Id = 1, ActionType = "StealGold", DisplayName = "Kradzież Złota", Description = "Kradnie złoto z wrogiego skarbca", ThievesRequired = 50, SuccessBaseRate = 0.60m, EffectType = "StealGold" },
            new ThiefActionDefinition { Id = 2, ActionType = "StealResources", DisplayName = "Kradzież Surowców", Description = "Kradnie surowce wroga", ThievesRequired = 75, SuccessBaseRate = 0.50m, EffectType = "StealResources" },
            new ThiefActionDefinition { Id = 3, ActionType = "Sabotage", DisplayName = "Sabotaż", Description = "Niszczy budynki wroga", ThievesRequired = 100, SuccessBaseRate = 0.40m, EffectType = "Sabotage" },
            new ThiefActionDefinition { Id = 4, ActionType = "Spy", DisplayName = "Szpiegostwo", Description = "Zbiera informacje o wrogu", ThievesRequired = 30, SuccessBaseRate = 0.70m, EffectType = "Spy" },
            new ThiefActionDefinition { Id = 5, ActionType = "Assassination", DisplayName = "Zamach", Description = "Zabija magów lub naukowców wroga", ThievesRequired = 150, SuccessBaseRate = 0.30m, EffectType = "Sabotage" }
        );
    }
}
