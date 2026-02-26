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
    public DbSet<ForumPost> ForumPosts { get; set; }

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

        // ForumPost -> AuthorKingdom
        modelBuilder.Entity<ForumPost>()
            .HasOne(f => f.AuthorKingdom)
            .WithMany()
            .HasForeignKey(f => f.AuthorKingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // ForumPost -> Coalition
        modelBuilder.Entity<ForumPost>()
            .HasOne(f => f.Coalition)
            .WithMany()
            .HasForeignKey(f => f.CoalitionId)
            .OnDelete(DeleteBehavior.SetNull);

        // ForumPost -> ParentPost (self-referencing)
        modelBuilder.Entity<ForumPost>()
            .HasOne(f => f.ParentPost)
            .WithMany(f => f.Replies)
            .HasForeignKey(f => f.ParentPostId)
            .OnDelete(DeleteBehavior.Restrict);
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
            // === BUDYNKI GOSPODARCZE (economic) ===
            new BuildingDefinition { Id = 1, BuildingType = "Domy", Category = "Gospodarcze", DisplayName = "Domy", Description = "Zwiększa limit ludności", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, PopulationCapacity = 100 },
            new BuildingDefinition { Id = 2, BuildingType = "WarsztatAlchemiczny", Category = "Warsztaty", DisplayName = "Laboratorium alchemiczne", Description = "Miejsce pracy alchemików (100 miejsc)", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, WorkshopCapacity = 100 },
            new BuildingDefinition { Id = 3, BuildingType = "Gospodarstwo", Category = "Warsztaty", DisplayName = "Gospodarstwo", Description = "Miejsce pracy chłopów (100 miejsc)", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, WorkshopCapacity = 100 },
            new BuildingDefinition { Id = 4, BuildingType = "LasyDruidow", Category = "Warsztaty", DisplayName = "Lasy Druidów", Description = "Miejsce pracy druidów (100 miejsc)", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, WorkshopCapacity = 100 },
            new BuildingDefinition { Id = 5, BuildingType = "ZakladyKamieniarskie", Category = "Warsztaty", DisplayName = "Zakłady Kamieniarskie", Description = "Miejsce pracy kamieniarzy (100 miejsc)", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, WorkshopCapacity = 100 },
            new BuildingDefinition { Id = 6, BuildingType = "WarsztatyMurarskie", Category = "Warsztaty", DisplayName = "Warsztaty murarskie", Description = "Miejsce pracy murarzy (100 miejsc)", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, WorkshopCapacity = 100 },
            new BuildingDefinition { Id = 7, BuildingType = "Zbrojownie", Category = "Warsztaty", DisplayName = "Zbrojownie", Description = "Miejsce pracy płatnerzy (100 miejsc)", CostGold = 100, CostBudulec = 1, CostLand = 1, BuildTime = 1, WorkshopCapacity = 100 },
            new BuildingDefinition { Id = 8, BuildingType = "CechSlonca", Category = "Cechy", DisplayName = "Cech słońca", Description = "Bonus produkcji alchemików i chłopów", CostGold = 200, CostBudulec = 1, CostLand = 1, BuildTime = 1, ProductionBonus = 0.05m },
            new BuildingDefinition { Id = 9, BuildingType = "CechZiemi", Category = "Cechy", DisplayName = "Cech ziemi", Description = "Bonus produkcji druidów i kamieniarzy", CostGold = 200, CostBudulec = 1, CostLand = 1, BuildTime = 1, ProductionBonus = 0.05m },
            new BuildingDefinition { Id = 10, BuildingType = "CechGwiazd", Category = "Cechy", DisplayName = "Cech gwiazd", Description = "Bonus produkcji murarzy i płatnerzy", CostGold = 200, CostBudulec = 1, CostLand = 1, BuildTime = 1, ProductionBonus = 0.05m },
            new BuildingDefinition { Id = 11, BuildingType = "Manufaktura", Category = "Manufaktury", DisplayName = "Manufaktura", Description = "Automatycznie produkuje surowce", CostGold = 300, CostBudulec = 1, CostLand = 1, BuildTime = 1, ProductionBonus = 0.10m },
            new BuildingDefinition { Id = 12, BuildingType = "Szkoly", Category = "Pozostale", DisplayName = "Szkoły", Description = "Przyspiesza szkolenie nowicjuszy", CostGold = 200, CostBudulec = 1, CostLand = 1, BuildTime = 1 },
            new BuildingDefinition { Id = 13, BuildingType = "WiezeObronne", Category = "Obrona", DisplayName = "Wieże obronne", Description = "Pomagają w obronie księstwa", CostGold = 300, CostBudulec = 1, CostLand = 1, BuildTime = 1, DefenseBonus = 0.03m },
            new BuildingDefinition { Id = 14, BuildingType = "KonstrukcjaMachin", Category = "Wojskowe", DisplayName = "Konstrukcja machin bojowych", Description = "Budowa machin wojennych", CostGold = 500, CostBudulec = 1, CostLand = 1, BuildTime = 1 },
            // === BUDYNKI SPECJALNE - Rząd 1 (koszt bazowy 500, 1 tura) ===
            new BuildingDefinition { Id = 101, BuildingType = "ZajazdCzerwonego", Category = "Specjalne", DisplayName = "Zajazd u Czerwonego Smoka", Description = "Obniża wymaganą pensję do 42 dla 100% popularności", IsSpecial = true, Row = 1, Col = 1, BaseCost = 500, BuildTime = 1, CostLand = 0 },
            new BuildingDefinition { Id = 102, BuildingType = "Mlyn", Category = "Specjalne", DisplayName = "Młyn", Description = "Bonus do produkcji chłopów", IsSpecial = true, Row = 1, Col = 2, BaseCost = 500, BuildTime = 1, CostLand = 0, ProductionBonus = 0.10m },
            new BuildingDefinition { Id = 103, BuildingType = "Ratusz", Category = "Specjalne", DisplayName = "Ratusz", Description = "Dodatkowe złoto z podatków", IsSpecial = true, Row = 1, Col = 3, BaseCost = 500, BuildTime = 1, CostLand = 0 },
            new BuildingDefinition { Id = 104, BuildingType = "KondensatorMagiczny", Category = "Specjalne", DisplayName = "Kondensator magiczny", Description = "Bonus do produkcji many", IsSpecial = true, Row = 1, Col = 4, BaseCost = 500, BuildTime = 1, CostLand = 0, ProductionBonus = 0.10m },
            new BuildingDefinition { Id = 105, BuildingType = "SztabUderzeniowy", Category = "Specjalne", DisplayName = "Sztab uderzeniowy", Description = "Bonus do siły ataku", IsSpecial = true, Row = 1, Col = 5, BaseCost = 500, BuildTime = 1, CostLand = 0 },
            new BuildingDefinition { Id = 106, BuildingType = "Szaniec", Category = "Specjalne", DisplayName = "Szaniec", Description = "Podstawowa obrona specjalna", IsSpecial = true, Row = 1, Col = 6, BaseCost = 500, BuildTime = 1, CostLand = 0, DefenseBonus = 0.05m },
            // === BUDYNKI SPECJALNE - Rząd 2 (koszt bazowy 5000, 2 tury) ===
            new BuildingDefinition { Id = 201, BuildingType = "RezydencjaGenerala", Category = "Specjalne", DisplayName = "Rezydencja Generała", Description = "Umożliwia posiadanie generała", IsSpecial = true, Row = 2, Col = 1, BaseCost = 5000, BuildTime = 2, CostLand = 0, RequiredBuildingType = "ZajazdCzerwonego" },
            new BuildingDefinition { Id = 202, BuildingType = "KopalniaZlota", Category = "Specjalne", DisplayName = "Kopalnia złota", Description = "Szansa na znalezienie skarbu", IsSpecial = true, Row = 2, Col = 2, BaseCost = 5000, BuildTime = 2, CostLand = 0, RequiredBuildingType = "Mlyn" },
            new BuildingDefinition { Id = 203, BuildingType = "RenowacjaBroni", Category = "Specjalne", DisplayName = "Renowacja broni", Description = "Zmniejsza zużycie broni", IsSpecial = true, Row = 2, Col = 3, BaseCost = 5000, BuildTime = 2, CostLand = 0, RequiredBuildingType = "Ratusz" },
            new BuildingDefinition { Id = 204, BuildingType = "TajemnicaOdtworzenia", Category = "Specjalne", DisplayName = "Tajemnica Odtworzenia", Description = "Regeneracja many", IsSpecial = true, Row = 2, Col = 4, BaseCost = 5000, BuildTime = 2, CostLand = 0, RequiredBuildingType = "KondensatorMagiczny" },
            new BuildingDefinition { Id = 205, BuildingType = "Szpital", Category = "Specjalne", DisplayName = "Szpital", Description = "Leczenie rannych po walce", IsSpecial = true, Row = 2, Col = 5, BaseCost = 5000, BuildTime = 2, CostLand = 0, RequiredBuildingType = "SztabUderzeniowy" },
            new BuildingDefinition { Id = 206, BuildingType = "SmoczyMur", Category = "Specjalne", DisplayName = "Smoczy mur", Description = "Bonus obrony", IsSpecial = true, Row = 2, Col = 6, BaseCost = 5000, BuildTime = 2, CostLand = 0, RequiredBuildingType = "Szaniec", DefenseBonus = 0.10m },
            // === BUDYNKI SPECJALNE - Rząd 3 (koszt bazowy 20000, 3 tury) ===
            new BuildingDefinition { Id = 301, BuildingType = "LazniaMiejska", Category = "Specjalne", DisplayName = "Łaźnia miejska", Description = "Bonus do zaludnienia", IsSpecial = true, Row = 3, Col = 1, BaseCost = 20000, BuildTime = 3, CostLand = 0, RequiredBuildingType = "RezydencjaGenerala", PopulationCapacity = 500 },
            new BuildingDefinition { Id = 302, BuildingType = "KlubOdkrywcow", Category = "Specjalne", DisplayName = "Klub odkrywców", Description = "Bonus do nauki", IsSpecial = true, Row = 3, Col = 2, BaseCost = 20000, BuildTime = 3, CostLand = 0, RequiredBuildingType = "KopalniaZlota" },
            new BuildingDefinition { Id = 303, BuildingType = "SwiatyniaAutora", Category = "Specjalne", DisplayName = "Świątynia bogactwa Autora", Description = "Bonus do złota", IsSpecial = true, Row = 3, Col = 3, BaseCost = 20000, BuildTime = 3, CostLand = 0, RequiredBuildingType = "RenowacjaBroni" },
            new BuildingDefinition { Id = 304, BuildingType = "SoczewkaMagiczna", Category = "Specjalne", DisplayName = "Soczewka magiczna", Description = "Bonus do mocy czarów", IsSpecial = true, Row = 3, Col = 4, BaseCost = 20000, BuildTime = 3, CostLand = 0, RequiredBuildingType = "TajemnicaOdtworzenia" },
            new BuildingDefinition { Id = 305, BuildingType = "OltarzInicjacji", Category = "Specjalne", DisplayName = "Ołtarz Inicjacji", Description = "Bonus do szkolenia wojsk", IsSpecial = true, Row = 3, Col = 5, BaseCost = 20000, BuildTime = 3, CostLand = 0, RequiredBuildingType = "Szpital" },
            new BuildingDefinition { Id = 306, BuildingType = "SmoczaBariera", Category = "Specjalne", DisplayName = "Smocza bariera", Description = "Silna obrona magiczna", IsSpecial = true, Row = 3, Col = 6, BaseCost = 20000, BuildTime = 3, CostLand = 0, RequiredBuildingType = "SmoczyMur", DefenseBonus = 0.15m },
            // === BUDYNKI SPECJALNE - Rząd 4 (koszt bazowy 50000, 4 tury) ===
            new BuildingDefinition { Id = 401, BuildingType = "SystemJaskin", Category = "Specjalne", DisplayName = "System jaskiń", Description = "Ukrywa zasoby przed wrogami", IsSpecial = true, Row = 4, Col = 1, BaseCost = 50000, BuildTime = 4, CostLand = 0, RequiredBuildingType = "LazniaMiejska" },
            new BuildingDefinition { Id = 402, BuildingType = "SkrzyzowanieSzlakow", Category = "Specjalne", DisplayName = "Skrzyżowanie szlaków handlowych", Description = "Bonus do handlu", IsSpecial = true, Row = 4, Col = 2, BaseCost = 50000, BuildTime = 4, CostLand = 0, RequiredBuildingType = "KlubOdkrywcow" },
            new BuildingDefinition { Id = 403, BuildingType = "GildiaZlodziei", Category = "Specjalne", DisplayName = "Gildia Złodziei", Description = "Odblokowanie akcji złodziejskich", IsSpecial = true, Row = 4, Col = 3, BaseCost = 50000, BuildTime = 4, CostLand = 0, RequiredBuildingType = "SwiatyniaAutora" },
            new BuildingDefinition { Id = 404, BuildingType = "ScianyMagiczne", Category = "Specjalne", DisplayName = "Ściany magiczne", Description = "Obrona przed magią", IsSpecial = true, Row = 4, Col = 4, BaseCost = 50000, BuildTime = 4, CostLand = 0, RequiredBuildingType = "SoczewkaMagiczna" },
            new BuildingDefinition { Id = 405, BuildingType = "PlacDefilad", Category = "Specjalne", DisplayName = "Plac defilad", Description = "Bonus do morale armii", IsSpecial = true, Row = 4, Col = 5, BaseCost = 50000, BuildTime = 4, CostLand = 0, RequiredBuildingType = "OltarzInicjacji" },
            new BuildingDefinition { Id = 406, BuildingType = "Zamek", Category = "Specjalne", DisplayName = "Zamek", Description = "Potężna obrona", IsSpecial = true, Row = 4, Col = 6, BaseCost = 50000, BuildTime = 4, CostLand = 0, RequiredBuildingType = "SmoczaBariera", DefenseBonus = 0.20m },
            // === BUDYNKI SPECJALNE - Rząd 5 (koszt bazowy 85000, 5 tur) ===
            new BuildingDefinition { Id = 501, BuildingType = "Akwedukt", Category = "Specjalne", DisplayName = "Akwedukt", Description = "Duży bonus zaludnienia", IsSpecial = true, Row = 5, Col = 1, BaseCost = 85000, BuildTime = 5, CostLand = 0, RequiredBuildingType = "SystemJaskin", PopulationCapacity = 1000 },
            new BuildingDefinition { Id = 502, BuildingType = "ZachodniaWiezaCzasu", Category = "Specjalne", DisplayName = "Zachodnia wieża czasu", Description = "+1 tura dziennie", IsSpecial = true, Row = 5, Col = 2, BaseCost = 85000, BuildTime = 5, CostLand = 0, RequiredBuildingType = "SkrzyzowanieSzlakow", BonusTurnsPerDay = 1 },
            new BuildingDefinition { Id = 503, BuildingType = "Smokodrap", Category = "Specjalne", DisplayName = "Smokodrap", Description = "Przyciąga smoki", IsSpecial = true, Row = 5, Col = 3, BaseCost = 85000, BuildTime = 5, CostLand = 0, RequiredBuildingType = "GildiaZlodziei" },
            new BuildingDefinition { Id = 504, BuildingType = "LustroMagiczne", Category = "Specjalne", DisplayName = "Lustro magiczne", Description = "Odbija czary wroga", IsSpecial = true, Row = 5, Col = 4, BaseCost = 85000, BuildTime = 5, CostLand = 0, RequiredBuildingType = "ScianyMagiczne" },
            new BuildingDefinition { Id = 505, BuildingType = "AkademiaWojskowa", Category = "Specjalne", DisplayName = "Akademia wojskowa", Description = "Bonus do siły armii", IsSpecial = true, Row = 5, Col = 5, BaseCost = 85000, BuildTime = 5, CostLand = 0, RequiredBuildingType = "PlacDefilad" },
            new BuildingDefinition { Id = 506, BuildingType = "SiecFortec", Category = "Specjalne", DisplayName = "Sieć wojennych fortec", Description = "Potężna obrona fortyfikacyjna", IsSpecial = true, Row = 5, Col = 6, BaseCost = 85000, BuildTime = 5, CostLand = 0, RequiredBuildingType = "Zamek", DefenseBonus = 0.25m },
            // === BUDYNKI SPECJALNE - Rząd 6 (koszt bazowy 110000, 6 tur) ===
            new BuildingDefinition { Id = 601, BuildingType = "Kanalizacja", Category = "Specjalne", DisplayName = "Kanalizacja", Description = "Maksymalny bonus zaludnienia", IsSpecial = true, Row = 6, Col = 1, BaseCost = 110000, BuildTime = 6, CostLand = 0, RequiredBuildingType = "Akwedukt", PopulationCapacity = 2000 },
            new BuildingDefinition { Id = 602, BuildingType = "WschodniaWiezaCzasu", Category = "Specjalne", DisplayName = "Wschodnia wieża czasu", Description = "+1 tura dziennie", IsSpecial = true, Row = 6, Col = 2, BaseCost = 110000, BuildTime = 6, CostLand = 0, RequiredBuildingType = "ZachodniaWiezaCzasu", BonusTurnsPerDay = 1 },
            new BuildingDefinition { Id = 603, BuildingType = "Portal", Category = "Specjalne", DisplayName = "Portal", Description = "Zaawansowane zdolności smoków", IsSpecial = true, Row = 6, Col = 3, BaseCost = 110000, BuildTime = 6, CostLand = 0, RequiredBuildingType = "Smokodrap" },
            new BuildingDefinition { Id = 604, BuildingType = "PalacMagiczny", Category = "Specjalne", DisplayName = "Pałac magiczny", Description = "Najsilniejsza magia", IsSpecial = true, Row = 6, Col = 4, BaseCost = 110000, BuildTime = 6, CostLand = 0, RequiredBuildingType = "LustroMagiczne" },
            new BuildingDefinition { Id = 605, BuildingType = "KoszarySpecjalne", Category = "Specjalne", DisplayName = "Koszary", Description = "Elitarne jednostki wojskowe", IsSpecial = true, Row = 6, Col = 5, BaseCost = 110000, BuildTime = 6, CostLand = 0, RequiredBuildingType = "AkademiaWojskowa" },
            new BuildingDefinition { Id = 606, BuildingType = "PospoliteRuszenie", Category = "Specjalne", DisplayName = "Pospolite ruszenie", Description = "Ludność walczy w obronie", IsSpecial = true, Row = 6, Col = 6, BaseCost = 110000, BuildTime = 6, CostLand = 0, RequiredBuildingType = "SiecFortec", DefenseBonus = 0.30m },
            // === BUDYNKI SPECJALNE - Rząd 7 (koszt bazowy 200000, 7 tur) ===
            new BuildingDefinition { Id = 701, BuildingType = "MinisterstwoSmokow", Category = "Specjalne", DisplayName = "Ministerstwo smoków", Description = "Pełna kontrola nad smokami", IsSpecial = true, Row = 7, Col = 1, BaseCost = 200000, BuildTime = 7, CostLand = 0, RequiredBuildingType = "Portal" },
            new BuildingDefinition { Id = 702, BuildingType = "SanktuariumBerserkerow", Category = "Specjalne", DisplayName = "Sanktuarium berserkerów", Description = "Najsilniejsze jednostki wojskowe", IsSpecial = true, Row = 7, Col = 2, BaseCost = 200000, BuildTime = 7, CostLand = 0, RequiredBuildingType = "KoszarySpecjalne" },
            new BuildingDefinition { Id = 703, BuildingType = "KlasztorMnichow", Category = "Specjalne", DisplayName = "Klasztor Smoczych Mnichów", Description = "Ostateczna obrona", IsSpecial = true, Row = 7, Col = 3, BaseCost = 200000, BuildTime = 7, CostLand = 0, RequiredBuildingType = "PospoliteRuszenie", DefenseBonus = 0.40m }
        );
    }

    private void SeedUnitDefinitions(ModelBuilder modelBuilder)
    {
        // UnitType must be globally unique (used as FK principal key)
        modelBuilder.Entity<UnitDefinition>().HasData(
            // Ludzie
            new UnitDefinition { Id = 1, UnitType = "Ludzie_Piechota", Race = "Ludzie", DisplayName = "Piechota", Description = "Podstawowe jednostki piechoty", CostGold = 50, CostWeapons = 1, CostFood = 10, AttackPower = 10, DefensePower = 12, Upkeep = 2, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 2, UnitType = "Ludzie_Lucznik", Race = "Ludzie", DisplayName = "Łucznik", Description = "Jednostki dystansowe", CostGold = 80, CostWeapons = 2, CostFood = 10, AttackPower = 15, DefensePower = 6, Upkeep = 3, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 3, UnitType = "Ludzie_Kawaleria", Race = "Ludzie", DisplayName = "Kawaleria", Description = "Szybkie i silne jednostki konne", CostGold = 200, CostWeapons = 3, CostFood = 20, AttackPower = 25, DefensePower = 20, Upkeep = 5, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 2 },
            new UnitDefinition { Id = 4, UnitType = "Ludzie_Rycerz", Race = "Ludzie", DisplayName = "Rycerz", Description = "Elitarne jednostki wojskowe", CostGold = 500, CostWeapons = 5, CostFood = 30, AttackPower = 40, DefensePower = 35, Upkeep = 10, RequiredBuilding = "AkademiaWojskowa", TrainingTime = 3 },
            new UnitDefinition { Id = 5, UnitType = "Ludzie_Machina", Race = "Ludzie", DisplayName = "Machina wojenna", Description = "Potężna machina oblężnicza", CostGold = 1000, CostWeapons = 10, CostFood = 0, AttackPower = 60, DefensePower = 5, Upkeep = 15, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 5 },
            // Krasnoludy
            new UnitDefinition { Id = 6, UnitType = "Krasnoludy_Piechota", Race = "Krasnoludy", DisplayName = "Wojownik krasnoludzki", Description = "Silna piechota krasnoludów", CostGold = 60, CostWeapons = 1, CostFood = 10, AttackPower = 12, DefensePower = 15, Upkeep = 2, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 7, UnitType = "Krasnoludy_Lucznik", Race = "Krasnoludy", DisplayName = "Kusznik krasnoludzki", Description = "Ciężka broń dystansowa", CostGold = 90, CostWeapons = 2, CostFood = 10, AttackPower = 18, DefensePower = 8, Upkeep = 3, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            // Elfy
            new UnitDefinition { Id = 8, UnitType = "Elfy_Piechota", Race = "Elfy", DisplayName = "Strażnik elfów", Description = "Zwinny wojownik elfów", CostGold = 55, CostWeapons = 1, CostFood = 8, AttackPower = 11, DefensePower = 10, Upkeep = 2, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 9, UnitType = "Elfy_Lucznik", Race = "Elfy", DisplayName = "Łucznik elfów", Description = "Mistrzowscy łucznicy", CostGold = 75, CostWeapons = 1, CostFood = 8, AttackPower = 20, DefensePower = 5, Upkeep = 3, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            // Orkowie
            new UnitDefinition { Id = 10, UnitType = "Orkowie_Piechota", Race = "Orkowie", DisplayName = "Berserker orków", Description = "Dziki wojownik orków", CostGold = 40, CostWeapons = 1, CostFood = 12, AttackPower = 15, DefensePower = 8, Upkeep = 3, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            // Gobliny
            new UnitDefinition { Id = 11, UnitType = "Gobliny_Piechota", Race = "Gobliny", DisplayName = "Gobliński łobuz", Description = "Tania i szybka jednostka", CostGold = 25, CostWeapons = 1, CostFood = 5, AttackPower = 6, DefensePower = 5, Upkeep = 1, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 }
        );
    }

    private void SeedTechnologyDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TechnologyDefinition>().HasData(
            // === 1-LEVEL (standalone) ===
            new TechnologyDefinition { Id = 1, TechType = "KonstrukcjaMaszyn", Category = "Ekonomia", DisplayName = "Konstrukcja maszyn drewnianych", Description = "Odblokowanie machin wojennych", Level = 1, CostGold = 3000, ResearchTime = 5, EffectType = "UnlockSiege", EffectValue = 1.0m },
            new TechnologyDefinition { Id = 2, TechType = "Empiryzm", Category = "Nauka", DisplayName = "Empiryzm", Description = "Bonus do efektywności naukowców", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "ScienceBonus", EffectValue = 0.10m },
            // === 2-LEVEL CHAIN: Czas ===
            new TechnologyDefinition { Id = 3, TechType = "ZakrzywCzasu", Category = "Czas", DisplayName = "Zakrzywienie czasu", Description = "+1 tura dziennie", Level = 1, CostGold = 20000, ResearchTime = 15, EffectType = "BonusTurns", EffectValue = 1.0m },
            new TechnologyDefinition { Id = 4, TechType = "ZalamCzasu", Category = "Czas", DisplayName = "Załamanie czasu", Description = "+1 dodatkowa tura dziennie", Level = 2, CostGold = 50000, ResearchTime = 25, RequiredTech = "ZakrzywCzasu", EffectType = "BonusTurns", EffectValue = 1.0m },
            // === 3-LEVEL CHAINS ===
            // Rekultywacja
            new TechnologyDefinition { Id = 5, TechType = "Rekultywacja", Category = "Ziemia", DisplayName = "Rekultywacja", Description = "Tańsze kupowanie ziemi", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "LandCostReduction", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 6, TechType = "Osadnictwo", Category = "Ziemia", DisplayName = "Osadnictwo", Description = "Jeszcze tańsza ziemia", Level = 2, CostGold = 15000, ResearchTime = 15, RequiredTech = "Rekultywacja", EffectType = "LandCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 7, TechType = "GornictwoOdkrywkowe", Category = "Ziemia", DisplayName = "Górnictwo Odkrywkowe", Description = "Bonus do kamienia", Level = 3, CostGold = 30000, ResearchTime = 20, RequiredTech = "Osadnictwo", EffectType = "StoneBonus", EffectValue = 0.30m },
            // Smokoastronomia
            new TechnologyDefinition { Id = 8, TechType = "Smokoastronomia", Category = "Smoki", DisplayName = "Smokoastronomia", Description = "Podstawowa wiedza o smokach", Level = 1, CostGold = 8000, ResearchTime = 10, EffectType = "DragonKnowledge", EffectValue = 1.0m },
            new TechnologyDefinition { Id = 9, TechType = "Smokoanatomia", Category = "Smoki", DisplayName = "Smokoanatomia", Description = "Znajomość anatomii smoków", Level = 2, CostGold = 20000, ResearchTime = 18, RequiredTech = "Smokoastronomia", EffectType = "DragonKnowledge", EffectValue = 2.0m },
            new TechnologyDefinition { Id = 10, TechType = "Smokodynamika", Category = "Smoki", DisplayName = "Smokodynamika", Description = "Pełna kontrola nad smokami", Level = 3, CostGold = 45000, ResearchTime = 25, RequiredTech = "Smokoanatomia", EffectType = "DragonKnowledge", EffectValue = 3.0m },
            // Rachunkowość
            new TechnologyDefinition { Id = 11, TechType = "Rachunkowosc", Category = "Ekonomia", DisplayName = "Rachunkowość", Description = "Bonus do złota z kupców", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "MerchantBonus", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 12, TechType = "Buchalteria", Category = "Ekonomia", DisplayName = "Buchalteria", Description = "Większy bonus do handlu", Level = 2, CostGold = 15000, ResearchTime = 15, RequiredTech = "Rachunkowosc", EffectType = "MerchantBonus", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 13, TechType = "Ksiegowosc", Category = "Ekonomia", DisplayName = "Księgowość", Description = "Maksymalny bonus handlowy", Level = 3, CostGold = 35000, ResearchTime = 22, RequiredTech = "Buchalteria", EffectType = "MerchantBonus", EffectValue = 0.30m },
            // Ostrzenie broni
            new TechnologyDefinition { Id = 14, TechType = "OstrzenieBroni", Category = "Wojsko", DisplayName = "Ostrzenie broni", Description = "Bonus do ataku", Level = 1, CostGold = 8000, ResearchTime = 10, EffectType = "AttackBonus", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 15, TechType = "NaprawaBroni", Category = "Wojsko", DisplayName = "Naprawa broni", Description = "Większy bonus do ataku", Level = 2, CostGold = 20000, ResearchTime = 18, RequiredTech = "OstrzenieBroni", EffectType = "AttackBonus", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 16, TechType = "PrzekuwanieBroni", Category = "Wojsko", DisplayName = "Przekuwanie broni", Description = "Maksymalny bonus do ataku", Level = 3, CostGold = 45000, ResearchTime = 25, RequiredTech = "NaprawaBroni", EffectType = "AttackBonus", EffectValue = 0.30m },
            // === 5-LEVEL CHAINS ===
            // Wynalazczość
            new TechnologyDefinition { Id = 17, TechType = "Wynalazki1", Category = "Nauka", DisplayName = "Wynalazczość I", Description = "Bonus do produkcji", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "ProductionBonus", EffectValue = 0.05m },
            new TechnologyDefinition { Id = 18, TechType = "Wynalazki2", Category = "Nauka", DisplayName = "Wynalazczość II", Level = 2, CostGold = 12000, ResearchTime = 12, RequiredTech = "Wynalazki1", EffectType = "ProductionBonus", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 19, TechType = "Wynalazki3", Category = "Nauka", DisplayName = "Wynalazczość III", Level = 3, CostGold = 25000, ResearchTime = 18, RequiredTech = "Wynalazki2", EffectType = "ProductionBonus", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 20, TechType = "Wynalazki4", Category = "Nauka", DisplayName = "Wynalazczość IV", Level = 4, CostGold = 45000, ResearchTime = 25, RequiredTech = "Wynalazki3", EffectType = "ProductionBonus", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 21, TechType = "Wynalazki5", Category = "Nauka", DisplayName = "Wynalazczość V", Level = 5, CostGold = 80000, ResearchTime = 35, RequiredTech = "Wynalazki4", EffectType = "ProductionBonus", EffectValue = 0.25m },
            // Architektura (reduces special building costs)
            new TechnologyDefinition { Id = 22, TechType = "Architektura1", Category = "Budowa", DisplayName = "Architektura I", Description = "Tańsze budynki specjalne", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "SpecialBuildingCostReduction", EffectValue = 0.05m },
            new TechnologyDefinition { Id = 23, TechType = "Architektura2", Category = "Budowa", DisplayName = "Architektura II", Level = 2, CostGold = 12000, ResearchTime = 12, RequiredTech = "Architektura1", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 24, TechType = "Architektura3", Category = "Budowa", DisplayName = "Architektura III", Level = 3, CostGold = 25000, ResearchTime = 18, RequiredTech = "Architektura2", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 25, TechType = "Architektura4", Category = "Budowa", DisplayName = "Architektura IV", Description = "Brak kosztu złota pod czarną magią + 50% taniej przyspieszanie", Level = 4, CostGold = 45000, ResearchTime = 25, RequiredTech = "Architektura3", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 26, TechType = "Architektura5", Category = "Budowa", DisplayName = "Architektura V", Level = 5, CostGold = 80000, ResearchTime = 35, RequiredTech = "Architektura4", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.25m },
            // Inżynieria (reduces economic building gold cost)
            new TechnologyDefinition { Id = 27, TechType = "Inzynieria1", Category = "Budowa", DisplayName = "Inżynieria I", Description = "Tańsze budynki gospodarcze", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "EcoBuildingCostReduction", EffectValue = 0.05m },
            new TechnologyDefinition { Id = 28, TechType = "Inzynieria2", Category = "Budowa", DisplayName = "Inżynieria II", Level = 2, CostGold = 12000, ResearchTime = 12, RequiredTech = "Inzynieria1", EffectType = "EcoBuildingCostReduction", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 29, TechType = "Inzynieria3", Category = "Budowa", DisplayName = "Inżynieria III", Level = 3, CostGold = 25000, ResearchTime = 18, RequiredTech = "Inzynieria2", EffectType = "EcoBuildingCostReduction", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 30, TechType = "Inzynieria4", Category = "Budowa", DisplayName = "Inżynieria IV", Level = 4, CostGold = 45000, ResearchTime = 25, RequiredTech = "Inzynieria3", EffectType = "EcoBuildingCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 31, TechType = "Inzynieria5", Category = "Budowa", DisplayName = "Inżynieria V", Level = 5, CostGold = 80000, ResearchTime = 35, RequiredTech = "Inzynieria4", EffectType = "EcoBuildingCostReduction", EffectValue = 0.25m },
            // Czarodziejstwo
            new TechnologyDefinition { Id = 32, TechType = "Czarodziejstwo1", Category = "Magia", DisplayName = "Czarodziejstwo I", Description = "Bonus do mocy czarów", Level = 1, CostGold = 8000, ResearchTime = 10, EffectType = "SpellPower", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 33, TechType = "Czarodziejstwo2", Category = "Magia", DisplayName = "Czarodziejstwo II", Level = 2, CostGold = 18000, ResearchTime = 16, RequiredTech = "Czarodziejstwo1", EffectType = "SpellPower", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 34, TechType = "Czarodziejstwo3", Category = "Magia", DisplayName = "Czarodziejstwo III", Level = 3, CostGold = 35000, ResearchTime = 22, RequiredTech = "Czarodziejstwo2", EffectType = "SpellPower", EffectValue = 0.30m },
            new TechnologyDefinition { Id = 35, TechType = "Czarodziejstwo4", Category = "Magia", DisplayName = "Czarodziejstwo IV", Level = 4, CostGold = 55000, ResearchTime = 28, RequiredTech = "Czarodziejstwo3", EffectType = "SpellPower", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 36, TechType = "Czarodziejstwo5", Category = "Magia", DisplayName = "Czarodziejstwo V", Level = 5, CostGold = 90000, ResearchTime = 38, RequiredTech = "Czarodziejstwo4", EffectType = "SpellPower", EffectValue = 0.50m },
            // Trening
            new TechnologyDefinition { Id = 37, TechType = "Trening1", Category = "Wojsko", DisplayName = "Trening I", Description = "Szybsze szkolenie wojsk", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "TrainingSpeed", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 38, TechType = "Trening2", Category = "Wojsko", DisplayName = "Trening II", Level = 2, CostGold = 12000, ResearchTime = 12, RequiredTech = "Trening1", EffectType = "TrainingSpeed", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 39, TechType = "Trening3", Category = "Wojsko", DisplayName = "Trening III", Level = 3, CostGold = 25000, ResearchTime = 18, RequiredTech = "Trening2", EffectType = "TrainingSpeed", EffectValue = 0.30m },
            new TechnologyDefinition { Id = 40, TechType = "Trening4", Category = "Wojsko", DisplayName = "Trening IV", Level = 4, CostGold = 45000, ResearchTime = 25, RequiredTech = "Trening3", EffectType = "TrainingSpeed", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 41, TechType = "Trening5", Category = "Wojsko", DisplayName = "Trening V", Level = 5, CostGold = 80000, ResearchTime = 35, RequiredTech = "Trening4", EffectType = "TrainingSpeed", EffectValue = 0.50m },
            // Rekrutacja
            new TechnologyDefinition { Id = 42, TechType = "Rekrutacja1", Category = "Wojsko", DisplayName = "Rekrutacja I", Description = "Tańsza rekrutacja", Level = 1, CostGold = 5000, ResearchTime = 8, EffectType = "RecruitCostReduction", EffectValue = 0.05m },
            new TechnologyDefinition { Id = 43, TechType = "Rekrutacja2", Category = "Wojsko", DisplayName = "Rekrutacja II", Level = 2, CostGold = 12000, ResearchTime = 12, RequiredTech = "Rekrutacja1", EffectType = "RecruitCostReduction", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 44, TechType = "Rekrutacja3", Category = "Wojsko", DisplayName = "Rekrutacja III", Level = 3, CostGold = 25000, ResearchTime = 18, RequiredTech = "Rekrutacja2", EffectType = "RecruitCostReduction", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 45, TechType = "Rekrutacja4", Category = "Wojsko", DisplayName = "Rekrutacja IV", Level = 4, CostGold = 45000, ResearchTime = 25, RequiredTech = "Rekrutacja3", EffectType = "RecruitCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 46, TechType = "Rekrutacja5", Category = "Wojsko", DisplayName = "Rekrutacja V", Level = 5, CostGold = 80000, ResearchTime = 35, RequiredTech = "Rekrutacja4", EffectType = "RecruitCostReduction", EffectValue = 0.25m }
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
