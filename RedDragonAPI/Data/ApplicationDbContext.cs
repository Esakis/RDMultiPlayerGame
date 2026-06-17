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
    public DbSet<RaceDefinition> RaceDefinitions { get; set; }
    public DbSet<General> Generals { get; set; }
    public DbSet<Pact> Pacts { get; set; }
    public DbSet<MarketOrder> MarketOrders { get; set; }
    public DbSet<War> Wars { get; set; }
    public DbSet<MarketTransaction> MarketTransactions { get; set; }
    public DbSet<KingdomEvent> KingdomEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRelationships(modelBuilder);
        ConfigureIndexes(modelBuilder);
        ConfigureUniqueConstraints(modelBuilder);
        SeedEras(modelBuilder);
        SeedRaceDefinitions(modelBuilder);
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

        // KingdomEvent -> Kingdom
        modelBuilder.Entity<KingdomEvent>()
            .HasOne(e => e.Kingdom)
            .WithMany()
            .HasForeignKey(e => e.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

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

        // General -> Kingdom
        modelBuilder.Entity<General>()
            .HasOne(g => g.Kingdom)
            .WithMany()
            .HasForeignKey(g => g.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pact -> Kingdoms
        modelBuilder.Entity<Pact>()
            .HasOne(p => p.ProposerKingdom)
            .WithMany()
            .HasForeignKey(p => p.ProposerKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pact>()
            .HasOne(p => p.TargetKingdom)
            .WithMany()
            .HasForeignKey(p => p.TargetKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        // MarketOrder -> Kingdom
        modelBuilder.Entity<MarketOrder>()
            .HasOne(o => o.Kingdom)
            .WithMany()
            .HasForeignKey(o => o.KingdomId)
            .OnDelete(DeleteBehavior.Cascade);

        // MarketTransaction -> Buyer / Seller
        modelBuilder.Entity<MarketTransaction>()
            .HasOne(t => t.BuyerKingdom)
            .WithMany()
            .HasForeignKey(t => t.BuyerKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MarketTransaction>()
            .HasOne(t => t.SellerKingdom)
            .WithMany()
            .HasForeignKey(t => t.SellerKingdomId)
            .OnDelete(DeleteBehavior.Restrict);

        // War -> Era / Coalitions
        modelBuilder.Entity<War>()
            .HasOne(w => w.Era)
            .WithMany()
            .HasForeignKey(w => w.EraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<War>()
            .HasOne(w => w.DeclaringCoalition)
            .WithMany()
            .HasForeignKey(w => w.DeclaringCoalitionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<War>()
            .HasOne(w => w.TargetCoalition)
            .WithMany()
            .HasForeignKey(w => w.TargetCoalitionId)
            .OnDelete(DeleteBehavior.Restrict);

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
        modelBuilder.Entity<MarketOrder>().HasIndex(o => new { o.Status, o.Resource });
        modelBuilder.Entity<MarketOrder>().HasIndex(o => o.KingdomId);
        modelBuilder.Entity<War>().HasIndex(w => new { w.EraId, w.Status });
        modelBuilder.Entity<MarketTransaction>().HasIndex(t => t.BuyerKingdomId);
        modelBuilder.Entity<MarketTransaction>().HasIndex(t => t.SellerKingdomId);
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

    /// <summary>
    /// 10 ras oryginalnego Red Dragon. Charakterystyki z oficjalnej strony reddragon.cz,
    /// statystyki jednostek i bonusy profesji z rebalansu „31. wieku" (oficjalny blog, 01.2016).
    /// Źródła: docs/MECHANIKA.md, docs/zrodla/.
    /// </summary>
    private void SeedRaceDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RaceDefinition>().HasData(
            new RaceDefinition
            {
                Id = 1, Name = "Człowiek", NameCz = "Člověk",
                Description = "Dzięki wysokiej liczebności naszej populacji potrafimy uczynić wszystko, co ci przyjdzie do głowy! Umiemy świetnie czarować, jesteśmy dobrymi złodziejami, nie rozczarujemy Cię również w armii. Jesteśmy wszechstronną rasą.",
                EaseRating = 90, MagicRating = 85, ThievesRating = 90, DefenseRating = 60, EconomyRating = 65, AttackRating = 65,
                MagicBooks = 3, TurnsPerDay = 15, GeneralsLimit = 8, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 5, PopPerAcreBase = 3, AqueductAcreBonus = 1m, FoodPerPop = 1,
                BonusAlchemists = 0.10m, BonusMerchants = 0.20m,
                E1Attack = 3, E1Defense = 3, E2Attack = 7, E2Defense = 7, MachineAttack = 5,
                ThiefPowerModifier = -0.05m, ResearchModifier = 0.10m,
                SpecialTraits = "Generałowie zdobywają doświadczenie o 20% szybciej; budynki infrastrukturalne o 10% tańsze (złoto i budulec); mechanika Nauka stosowana (szkoła złodziejska/magiczna/wojskowa)."
            },
            new RaceDefinition
            {
                Id = 2, Name = "Elf", NameCz = "Elf",
                Description = "Najchętniej spędzamy czas w lasach, których potrafimy bardzo skutecznie bronić, a w razie potrzeby przeprowadzić z nich również kontratak. Potrafimy wpływać na świat dzięki wielu zaklęciom.",
                EaseRating = 60, MagicRating = 90, ThievesRating = 75, DefenseRating = 70, EconomyRating = 80, AttackRating = 60,
                MagicBooks = 4, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 3, PopPerAcreBase = 3, FoodPerPop = 1,
                BonusArmorers = 0.20m, BonusDruids = 0.20m, BonusMages = 0.30m, BonusStonemasons = -0.10m, BonusMasons = -0.10m,
                E1Attack = 4, E1Defense = 6, E2Attack = 8, E2Defense = 11, MachineAttack = 5,
                SpecialTraits = "Straszny lasek odstrasza 10% armii inwazyjnej; Pałac magiczny: wzrost kosztu zaklęć tylko 9%; biała magia o 25% tańsza; 1,5× łupy z labiryntu; E1/E2 mają siłę magiczną 0,5/1,0; mechanika Komando łuczników (+20% obrony sojusznika, -20% własnej)."
            },
            new RaceDefinition
            {
                Id = 3, Name = "Krasnolud", NameCz = "Trpaslík",
                Description = "Nasze rześkie jednostki nadają się jak do obrony, tak do ataku. Nie znamy się na magii, ale twarde życie w górach zahartowało naszą armię. Obróbka kamienia zapewnia nam dobrobyt i sławę najlepszych budowniczych.",
                EaseRating = 100, MagicRating = 60, ThievesRating = 65, DefenseRating = 50, EconomyRating = 85, AttackRating = 80,
                MagicBooks = 1, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 4,
                HouseCapacityBase = 3, PopPerAcreBase = 3, FoodPerPop = 1,
                BonusStonemasons = 0.20m, BonusArmorers = 0.30m, BonusMasons = 0.20m, BonusDruids = -0.30m, BonusMages = -0.20m,
                E1Attack = 5, E1Defense = 6, E2Attack = 10, E2Defense = 11, MachineAttack = 5,
                ThiefPowerModifier = -0.15m, MilitaryLossModifier = -0.25m,
                SpecialTraits = "O 25% niższe straty wojskowe; zabijają o 20% więcej smoków; budynki specjalne o 10% tańsze; przechodzi o jedno limitowane zaklęcie mniej; pakty złodziejskie -10% skuteczności; mechanika Dodatkowe uzbrojenie (do +2 atak/obrona elit za broń)."
            },
            new RaceDefinition
            {
                Id = 4, Name = "Hobbit", NameCz = "Hobit",
                Description = "Szukasz zręcznego złodzieja? Nie ma lepszych rabusiów od tych naszych. Nikt nie dorównuje ich zdolnościom. Agresorów potrafimy zaskoczyć upartą obroną.",
                EaseRating = 80, MagicRating = 60, ThievesRating = 100, DefenseRating = 50, EconomyRating = 70, AttackRating = 40,
                MagicBooks = 1, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 3, PopPerAcreBase = 3.5m, FoodPerPop = 1,
                BonusFarmers = 0.30m, BonusMerchants = 0.10m, BonusMages = -0.20m,
                E1Attack = 2, E1Defense = 4, E2Attack = 4, E2Defense = 10, MachineAttack = 5,
                ThiefPowerModifier = 0.25m, ThiefCostModifier = -0.25m,
                SpecialTraits = "Złodzieje o 25% tańsi i silniejsi; Zniszczenie zapasów działa na nich w 50%; obniżki popularności (rewolta, Smoczy Oddech, ataki) o połowę słabsze; odporni na Zły humor; mniejsze straty ziemi (pierwszy atak 9% zamiast 11%); mechanika Hodokvas."
            },
            new RaceDefinition
            {
                Id = 5, Name = "Nekromant", NameCz = "Nekromant",
                Description = "Wojna, śmierć i cierpienie! Obrona nie należy do naszych silnych stron, ale zatrzymanie hord żywych trupów jest praktycznie niemożliwe. Znamy również wiele mocnych zaklęć. Naszą specjalnością jest zasypywanie wrogów klęskami żywiołowymi.",
                EaseRating = 90, MagicRating = 90, ThievesRating = 70, DefenseRating = 90, EconomyRating = 65, AttackRating = 90,
                MagicBooks = 4, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 3, PopPerAcreBase = 3, FoodPerPop = 1,
                BonusDruids = 0.20m, BonusMages = 0.30m, BonusFarmers = -0.25m,
                E1Attack = 2, E1Defense = 1, E2Attack = 6, E2Defense = 3, MachineAttack = 4,
                ThiefPowerModifier = -0.50m,
                SpecialTraits = "Armia nie je i nie pobiera żołdu, nie umiera w czasie głodu; odporny na Zarazę, Kastrację i Płodność; Zaraza/Szarańcza/Kastracja/Zły humor o połowę tańsze; mechanika Nekromancja (armia wyczarowywana przez magów z ciał, Cmentarze, zaklęcie Ofiarowanie)."
            },
            new RaceDefinition
            {
                Id = 6, Name = "Dżin", NameCz = "Džin",
                Description = "Jesteśmy najlepszymi magami, o jakich możesz śnić. Całe nasze życie poświęciliśmy magii. Nikt inny nam w niej nie dorównuje — jedynym prawdziwym wyzwaniem jest dla nas walczyć z innymi dżinami.",
                EaseRating = 50, MagicRating = 100, ThievesRating = 65, DefenseRating = 90, EconomyRating = 45, AttackRating = 35,
                MagicBooks = 5, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 2, PopPerAcreBase = 2, WaterworksHouseBonus = 1m, SewersHouseBonus = 2m, AqueductAcreBonus = 1.5m, FoodPerPop = 1,
                BonusMages = 0.40m, BonusDruids = 0.10m, BonusFarmers = -0.30m, BonusStonemasons = -0.10m, BonusMasons = -0.10m,
                E1Attack = 2, E1Defense = 2, E2Attack = 4, E2Defense = 6, MachineAttack = 5,
                ThiefPowerModifier = -0.15m, ResearchModifier = 0.20m,
                SpecialTraits = "Mana nie znika po turze (każdy dżin przechowa 1 manę); Pałac magiczny: wzrost kosztu zaklęć 6%, pakty magiczne +5% skuteczności; Padłe legiony 3× skuteczniejsze; zaklęcia Metamagii (Wzmocniona/Przyspieszona magia)."
            },
            new RaceDefinition
            {
                Id = 7, Name = "Goblin", NameCz = "Skřet",
                Description = "Jesteśmy rasą agresywną! Mamy silne jednostki ataku i złodziei, nad obroną się zbytecznie nie zastanawiamy. Jesteśmy najlepsi w budowaniu bardzo skutecznych narzędzi wojennych.",
                EaseRating = 80, MagicRating = 65, ThievesRating = 80, DefenseRating = 50, EconomyRating = 50, AttackRating = 95,
                MagicBooks = 1, TurnsPerDay = 17, GeneralsLimit = 6, LimitedSpellsPerRecalc = 3,
                HouseCapacityBase = 3, PopPerAcreBase = 7, BurrowsHouseBonus = 0.5m, SewersHouseBonus = 1m, FoodPerPop = 1,
                PopGrowthModifier = 0.25m,
                BonusStonemasons = -0.20m, BonusMasons = -0.20m, BonusArmorers = -0.20m,
                BonusAlchemists = -0.30m, BonusFarmers = -0.30m, BonusScientists = -0.30m,
                BonusMages = -0.50m, BonusDruids = -0.50m,
                E1Attack = 2, E1Defense = 0, E2Attack = 6, E2Defense = 3, MachineAttack = 5,
                ThiefPowerModifier = -0.20m, ResearchModifier = -0.20m,
                SpecialTraits = "+2 tury dziennie (17), Wieża Czasu daje +2 tury; wieże obronne mieszczą 10 hoplitów (obrona 6) i 10 machin (obrona 100); każda jednostka utrzyma 2 machiny; mechanika Goblińska inżynieria (machiny z E1 +50% siły, z E2 obniżają obronę celu)."
            },
            new RaceDefinition
            {
                Id = 8, Name = "Ent", NameCz = "Ent",
                Description = "Nasza prastara rasa przerzedziła się w ciągu wieków, ale dysponuje najsilniejszymi jednostkami obrony. Nie ma rasy, która by nam dorównywała w obronie naszych i zaprzyjaźnionych księstw.",
                EaseRating = 50, MagicRating = 60, ThievesRating = 50, DefenseRating = 100, EconomyRating = 100, AttackRating = 50,
                MagicBooks = 2, TurnsPerDay = 13, GeneralsLimit = 6, LimitedSpellsPerRecalc = 6,
                HouseCapacityBase = 2, PopPerAcreBase = 2, FoodPerPop = 1,
                BonusFarmers = 0.50m, BonusScientists = 0.20m,
                E1Attack = 2, E1Defense = 7, E2Attack = 5, E2Defense = 19, MachineAttack = 5,
                ThiefPowerModifier = -0.25m, MilitaryLossModifier = -0.50m,
                SpecialTraits = "O 50% niższe straty wojskowe; -2 tury dziennie (13); limitowane zaklęcia przechodzą 3× za przeliczenie; Ognisty deszcz i Smoczy Oddech zadają im 2× straty; sady owocowe mieszczą 100 E2; mechanika Gniew Enta (+100% ataku i burzenia po stratach)."
            },
            new RaceDefinition
            {
                Id = 10, Name = "Olbrzym", NameCz = "Obr",
                Description = "Jedynym, co nas interesuje, jest walka! Nie jest nas, co prawda, wielu, ale nasze jednostki są najsilniejsze ze wszystkich. Wybierz nas, a zmiażdżymy każdego, kto stanie na naszej drodze!",
                EaseRating = 70, MagicRating = 55, ThievesRating = 55, DefenseRating = 70, EconomyRating = 60, AttackRating = 100,
                MagicBooks = 1, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 3, PopPerAcreBase = 2.5m, BurrowsHouseBonus = 0.5m, SewersHouseBonus = 1m, FoodPerPop = 2,
                BonusStonemasons = 0.30m, BonusMasons = 0.30m, BonusMages = -0.15m, BonusScientists = -0.15m,
                E1Attack = 6, E1Defense = 6, E2Attack = 16, E2Defense = 10, MachineAttack = 6,
                ThiefPowerModifier = -0.25m,
                SpecialTraits = "Jedzenie 2/mieszkańca (PL: 1,5); limitowane zaklęcia działają na nich do 4× za przeliczenie; +25% burzenia machin; E1 burzy 0,1, E2 burzy 0,5 (nie blokują wież); odporny na Zarazę (PL); nie może mieć złodziei — Gildia Wojowników zamiast Gildii Złodziei (+1 atak/+2 obrona E2); 8 generałów (PL); mechanika Szamanizm (totemy: Grabieży / Smokobójstwa / Niszczycielstwa)."
            },
            // === Rasy polskiego serwera reddragon.pl (manual/2, 2007) ===
            new RaceDefinition
            {
                Id = 11, Name = "Gnom", NameCz = "Tryton (trytoni.php)",
                Description = "Gnomy słyną z alchemii — zamiast krwi w żyłach płynie im złoto. Ich saperzy potrafią wysadzić w powietrze całe oddziały, za to machin wojennych nie używają wcale. Po wybudowaniu Łaźni i Systemu nor ich domki robią się zadziwiająco pojemne.",
                EaseRating = 70, MagicRating = 70, ThievesRating = 75, DefenseRating = 60, EconomyRating = 75, AttackRating = 55,
                MagicBooks = 3, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 3, PopPerAcreBase = 3, WaterworksHouseBonus = 1m, BurrowsHouseBonus = 2m, FoodPerPop = 1,
                BonusAlchemists = 0.10m, BonusMasons = -0.05m, BonusStonemasons = -0.05m,
                E1Attack = 1, E1Defense = 5, E2Attack = 8, E2Defense = 7, MachineAttack = 0,
                SpecialTraits = "Nie używa machin wojennych (odporny na Chochliki); złodziej kosztuje 1500 złota; drożenie zaklęć +11% (zamiast 10%); Łaźnia +1 do domu, System nor +2; wyszkolone E1 dają dodatkowo 0,5 obrony złodziejskiej; saperzy: dodatkowi zabici = liczba saperów/3 (max 150%)."
            },
            new RaceDefinition
            {
                Id = 12, Name = "Br-Oug", NameCz = "Br-Oug",
                Description = "Prastara rasa o ogromnej płodności — na jednym akrze gnieździ się ich więcej niż przedstawicieli jakiejkolwiek innej rasy. Ich machiny wojenne sieją postrach (8 ataku), ale budowle stawiają niechętnie i drogo.",
                EaseRating = 50, MagicRating = 60, ThievesRating = 55, DefenseRating = 60, EconomyRating = 45, AttackRating = 80,
                MagicBooks = 3, TurnsPerDay = 15, GeneralsLimit = 6, LimitedSpellsPerRecalc = 5,
                HouseCapacityBase = 1, PopPerAcreBase = 7, AqueductAcreBonus = 2.5m, FoodPerPop = 2,
                BonusFarmers = -0.20m, BonusStonemasons = -0.20m, BonusMasons = -0.25m, BonusAlchemists = -0.25m,
                BonusArmorers = -0.20m, BonusDruids = -0.20m, BonusMages = -0.30m,
                E1Attack = 2, E1Defense = 2, E2Attack = 5, E2Defense = 6, MachineAttack = 8,
                ThiefPowerModifier = -0.20m,
                SpecialTraits = "+4 mieszkańców/akr (dom mieści tylko 1); Akwedukt daje +2,5/akr; je 2 jedzenia/mieszkańca; budynki o 50% droższe, ale podwójny limit infrapunktów; machiny 8 ataku (z E1: 6), z hoplitami burzą o 40% słabiej; wieże obronne słabsze o 33% (blokują 10 machin, niszczą 2); Zdjęcie zaklęcia o 50% droższe; domobrana broni z siłą 1,5."
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
            new BuildingDefinition { Id = 101, BuildingType = "ZajazdCzerwonego", Category = "Specjalne", DisplayName = "Zajazd u Czerwonego Smoka", Description = "Obniża wymaganą pensję do 42 dla 100% popularności; pozwala wejść 2× do labiryntu na przeliczenie", IsSpecial = true, Row = 1, Col = 1, BaseCost = 500, BuildTime = 1, CostLand = 0 },
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
            new BuildingDefinition { Id = 703, BuildingType = "KlasztorMnichow", Category = "Specjalne", DisplayName = "Klasztor Smoczych Mnichów", Description = "Ostateczna obrona", IsSpecial = true, Row = 7, Col = 3, BaseCost = 200000, BuildTime = 7, CostLand = 0, RequiredBuildingType = "PospoliteRuszenie", DefenseBonus = 0.40m },
            new BuildingDefinition { Id = 704, BuildingType = "PalacZmian", Category = "Specjalne", DisplayName = "Pałac Zmian", Description = "Umożliwia zmianę rasy w trakcie ery", IsSpecial = true, Row = 7, Col = 4, BaseCost = 150000, BuildTime = 7, CostLand = 0 },
            new BuildingDefinition { Id = 705, BuildingType = "Ambasada", Category = "Specjalne", DisplayName = "Ambasada", Description = "Zwiększa limit paktów obronnych o 1 (5 → 6)", IsSpecial = true, Row = 7, Col = 5, BaseCost = 30000, BuildTime = 3, CostLand = 0 }
        );
    }

    /// <summary>
    /// Jednostki wg oryginału: każda rasa ma Hoplitę (1/1), Elitę 1 i 2 stopnia,
    /// Machinę wojenną, Złodzieja i Smoka. Statystyki atak/obrona z rebalansu
    /// „31. wieku"; nazwy i koszty elit wg manuala RDx2 (urza.cz) — nazwy jednostek
    /// Olbrzyma rekonstruowane (brak źródła). Hoplici szkolą się
    /// z bezrobotnych; elity powstają przez przelew (hoplita→E1→E2).
    /// </summary>
    private void SeedUnitDefinitions(ModelBuilder modelBuilder)
    {
        // UnitType must be globally unique (used as FK principal key)
        modelBuilder.Entity<UnitDefinition>().HasData(
            // === Człowiek ===
            new UnitDefinition { Id = 11, UnitType = "Czlowiek_Hoplita", Race = "Człowiek", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 12, UnitType = "Czlowiek_Rycerz", Race = "Człowiek", DisplayName = "Rycerz", Description = "Elita 1. stopnia", CostGold = 400, CostWeapons = 4, AttackPower = 3, DefensePower = 3, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 13, UnitType = "Czlowiek_Paladyn", Race = "Człowiek", DisplayName = "Paladyn", Description = "Elita 2. stopnia", CostGold = 1200, CostWeapons = 80, AttackPower = 7, DefensePower = 7, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 14, UnitType = "Czlowiek_Machina", Race = "Człowiek", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 15, UnitType = "Czlowiek_Zlodziej", Race = "Człowiek", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 16, UnitType = "Czlowiek_Smok", Race = "Człowiek", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Elf ===
            new UnitDefinition { Id = 21, UnitType = "Elf_Hoplita", Race = "Elf", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 22, UnitType = "Elf_Lucznik", Race = "Elf", DisplayName = "Łucznik", Description = "Elita 1. stopnia", CostGold = 700, CostWeapons = 20, AttackPower = 4, DefensePower = 6, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 23, UnitType = "Elf_LesnaZjawa", Race = "Elf", DisplayName = "Leśna Zjawa", Description = "Elita 2. stopnia", CostGold = 1900, CostWeapons = 200, AttackPower = 8, DefensePower = 11, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 24, UnitType = "Elf_Machina", Race = "Elf", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 25, UnitType = "Elf_Zlodziej", Race = "Elf", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 26, UnitType = "Elf_Smok", Race = "Elf", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Krasnolud ===
            new UnitDefinition { Id = 31, UnitType = "Krasnolud_Hoplita", Race = "Krasnolud", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 32, UnitType = "Krasnolud_Ciezkozbrojny", Race = "Krasnolud", DisplayName = "Ciężkozbrojny", Description = "Elita 1. stopnia", CostGold = 1000, CostWeapons = 15, AttackPower = 5, DefensePower = 6, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 33, UnitType = "Krasnolud_Berserker", Race = "Krasnolud", DisplayName = "Berserker", Description = "Elita 2. stopnia", CostGold = 1800, CostWeapons = 120, AttackPower = 10, DefensePower = 11, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 34, UnitType = "Krasnolud_Machina", Race = "Krasnolud", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 35, UnitType = "Krasnolud_Zlodziej", Race = "Krasnolud", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 36, UnitType = "Krasnolud_Smok", Race = "Krasnolud", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Hobbit ===
            new UnitDefinition { Id = 41, UnitType = "Hobbit_Hoplita", Race = "Hobbit", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 42, UnitType = "Hobbit_Blotostep", Race = "Hobbit", DisplayName = "Błotostęp", Description = "Elita 1. stopnia", CostGold = 500, CostWeapons = 20, AttackPower = 2, DefensePower = 4, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 43, UnitType = "Hobbit_Nornik", Race = "Hobbit", DisplayName = "Nornik", Description = "Elita 2. stopnia", CostGold = 1200, CostWeapons = 120, AttackPower = 4, DefensePower = 10, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 44, UnitType = "Hobbit_Machina", Race = "Hobbit", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 45, UnitType = "Hobbit_Zlodziej", Race = "Hobbit", DisplayName = "Złodziej", Description = "Armia podziemia — duma Hobbitów", CostGold = 900, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 46, UnitType = "Hobbit_Smok", Race = "Hobbit", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Nekromant ===
            new UnitDefinition { Id = 51, UnitType = "Nekromant_Hoplita", Race = "Nekromant", DisplayName = "Hoplita", Description = "Podstawowy żołnierz (nie je, bez żołdu)", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 52, UnitType = "Nekromant_Szkielet", Race = "Nekromant", DisplayName = "Szkielet", Description = "Elita 1. stopnia", CostGold = 700, CostWeapons = 20, AttackPower = 2, DefensePower = 1, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 53, UnitType = "Nekromant_Ghul", Race = "Nekromant", DisplayName = "Ghul", Description = "Elita 2. stopnia", CostGold = 1900, CostWeapons = 200, AttackPower = 6, DefensePower = 3, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 54, UnitType = "Nekromant_Machina", Race = "Nekromant", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 4, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 55, UnitType = "Nekromant_Zlodziej", Race = "Nekromant", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 56, UnitType = "Nekromant_Smok", Race = "Nekromant", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Dżin ===
            new UnitDefinition { Id = 61, UnitType = "Dzin_Hoplita", Race = "Dżin", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 62, UnitType = "Dzin_AlAhvar", Race = "Dżin", DisplayName = "Al'Ahvar", Description = "Elita 1. stopnia", CostGold = 600, CostWeapons = 20, AttackPower = 2, DefensePower = 2, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 63, UnitType = "Dzin_DzinBeam", Race = "Dżin", DisplayName = "Dżin'Beam", Description = "Elita 2. stopnia", CostGold = 1400, CostWeapons = 120, AttackPower = 4, DefensePower = 6, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 64, UnitType = "Dzin_Machina", Race = "Dżin", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 65, UnitType = "Dzin_Zlodziej", Race = "Dżin", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 66, UnitType = "Dzin_Smok", Race = "Dżin", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Goblin ===
            new UnitDefinition { Id = 71, UnitType = "Goblin_Hoplita", Race = "Goblin", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 72, UnitType = "Goblin_WilczyJezdziec", Race = "Goblin", DisplayName = "Wilczy Jeździec", Description = "Elita 1. stopnia", CostGold = 700, CostWeapons = 20, AttackPower = 2, DefensePower = 0, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 73, UnitType = "Goblin_SkurutHai", Race = "Goblin", DisplayName = "Skurut Hai", Description = "Elita 2. stopnia", CostGold = 2000, CostWeapons = 200, AttackPower = 6, DefensePower = 3, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 74, UnitType = "Goblin_Machina", Race = "Goblin", DisplayName = "Machina wojenna", Description = "Burzy budynki; Gobliny używają jej też w obronie", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 75, UnitType = "Goblin_Zlodziej", Race = "Goblin", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 76, UnitType = "Goblin_Smok", Race = "Goblin", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Ent ===
            new UnitDefinition { Id = 81, UnitType = "Ent_Hoplita", Race = "Ent", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 82, UnitType = "Ent_Konar", Race = "Ent", DisplayName = "Konar", Description = "Elita 1. stopnia", CostGold = 900, CostWeapons = 20, AttackPower = 2, DefensePower = 7, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 83, UnitType = "Ent_Drzewiec", Race = "Ent", DisplayName = "Drzewiec", Description = "Elita 2. stopnia — najtwardszy obrońca w grze", CostGold = 2400, CostWeapons = 200, AttackPower = 5, DefensePower = 19, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 84, UnitType = "Ent_Machina", Race = "Ent", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga", CostGold = 800, CostWeapons = 50, AttackPower = 5, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 85, UnitType = "Ent_Zlodziej", Race = "Ent", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 86, UnitType = "Ent_Smok", Race = "Ent", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Olbrzym ===
            new UnitDefinition { Id = 101, UnitType = "Olbrzym_Hoplita", Race = "Olbrzym", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 102, UnitType = "Olbrzym_Glazomiot", Race = "Olbrzym", DisplayName = "Głazomiot", Description = "Elita 1. stopnia (burzy 0,1 budynku)", CostGold = 1200, CostWeapons = 40, AttackPower = 6, DefensePower = 6, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 103, UnitType = "Olbrzym_Niszczyciel", Race = "Olbrzym", DisplayName = "Niszczyciel", Description = "Elita 2. stopnia — najsilniejszy atak w grze (burzy 0,5 budynku)", CostGold = 3200, CostWeapons = 320, AttackPower = 16, DefensePower = 10, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 104, UnitType = "Olbrzym_Machina", Race = "Olbrzym", DisplayName = "Machina wojenna", Description = "Burzy budynki wroga (+25% u Olbrzymów)", CostGold = 800, CostWeapons = 50, AttackPower = 6, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 105, UnitType = "Olbrzym_Zlodziej", Race = "Olbrzym", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 106, UnitType = "Olbrzym_Smok", Race = "Olbrzym", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Gnom (rasa polskiego serwera; manual/2/trytoni.php) ===
            new UnitDefinition { Id = 111, UnitType = "Gnom_Hoplita", Race = "Gnom", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 112, UnitType = "Gnom_NocnyStraznik", Race = "Gnom", DisplayName = "Nocny Strażnik", Description = "Elita 1. stopnia (+0,5 obrony złodziejskiej)", CostGold = 600, CostWeapons = 20, AttackPower = 1, DefensePower = 5, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 113, UnitType = "Gnom_Saper", Race = "Gnom", DisplayName = "Saper", Description = "Elita 2. stopnia — wysadza wrogów (dodatkowi zabici = saperzy/3)", CostGold = 1600, CostWeapons = 140, AttackPower = 8, DefensePower = 7, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 115, UnitType = "Gnom_Zlodziej", Race = "Gnom", DisplayName = "Złodziej", Description = "Armia podziemia (Gnom: 1500 złota)", CostGold = 1500, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 116, UnitType = "Gnom_Smok", Race = "Gnom", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 },
            // === Br-Oug (rasa polskiego serwera; manual/2/broug.php) ===
            new UnitDefinition { Id = 121, UnitType = "BrOug_Hoplita", Race = "Br-Oug", DisplayName = "Hoplita", Description = "Podstawowy żołnierz", CostGold = 200, CostWeapons = 2, AttackPower = 1, DefensePower = 1, RequiredBuilding = "", TrainingTime = 1 },
            new UnitDefinition { Id = 122, UnitType = "BrOug_KroDraag", Race = "Br-Oug", DisplayName = "Kro-Draag", Description = "Elita 1. stopnia", CostGold = 500, CostWeapons = 20, AttackPower = 2, DefensePower = 2, RequiredBuilding = "OltarzInicjacji", TrainingTime = 1 },
            new UnitDefinition { Id = 123, UnitType = "BrOug_TerAark", Race = "Br-Oug", DisplayName = "Ter-Aark", Description = "Elita 2. stopnia", CostGold = 1300, CostWeapons = 110, AttackPower = 5, DefensePower = 6, RequiredBuilding = "KoszarySpecjalne", TrainingTime = 1 },
            new UnitDefinition { Id = 124, UnitType = "BrOug_Machina", Race = "Br-Oug", DisplayName = "Machina wojenna", Description = "Najsilniejsze machiny w grze (8 ataku; z E1: 6)", CostGold = 800, CostWeapons = 50, AttackPower = 8, DefensePower = 0, RequiredBuilding = "KonstrukcjaMachin", TrainingTime = 1 },
            new UnitDefinition { Id = 125, UnitType = "BrOug_Zlodziej", Race = "Br-Oug", DisplayName = "Złodziej", Description = "Armia podziemia", CostGold = 1200, CostWeapons = 0, AttackPower = 0, DefensePower = 0, RequiredBuilding = "GildiaZlodziei", TrainingTime = 1 },
            new UnitDefinition { Id = 126, UnitType = "BrOug_Smok", Race = "Br-Oug", DisplayName = "Smok", Description = "Potężna bestia — wzmacnia armię", CostGold = 0, CostWeapons = 0, AttackPower = 100, DefensePower = 100, RequiredBuilding = "Smokodrap", TrainingTime = 1 }
        );
    }

    private void SeedTechnologyDefinitions(ModelBuilder modelBuilder)
    {
        // CostScience: progi Punktów Nauki wg manuala (docs/zrodla/manual-pl/vyzkum.txt).
        // Dziedziny 5-poziomowe (Wynalazczość/Architektura/Inżynieria/Czarodziejstwo/
        // Trening/Rekrutacja): 0,3M / 3M / 9M / 15M / 21M. Łańcuchy poboczne wg
        // odpowiedników z manuala (2M/3M/4M itd.).
        // Dane (opis efektu + koszt SP + zależności) zsynchronizowane z Dracopedią
        // (docs/zrodla/dracopedia/*, Wayback dracopedia.pl). Poziomy łańcuchów 5-stopniowych:
        // Prymitywny 0,3M / Podstawowy 3M / Rozwinięty 9M / Zaawansowany / Nowoczesny
        // (Architektura, Czarodziejstwo, Trening: 15M/21M; Inżynieria, Rekrutacja: 12M/12M).
        modelBuilder.Entity<TechnologyDefinition>().HasData(
            // === 1-LEVEL (standalone) ===
            new TechnologyDefinition { Id = 1, TechType = "KonstrukcjaMaszyn", Category = "Ekonomia", DisplayName = "Konstrukcja maszyn drewnianych", Description = "Podnosi wydajność manufaktur i wytwarzania maszyn bojowych o 10%.", Level = 1, CostGold = 3000, ResearchTime = 5, CostScience = 2_000_000, EffectType = "UnlockSiege", EffectValue = 1.0m },
            new TechnologyDefinition { Id = 2, TechType = "Empiryzm", Category = "Nauka", DisplayName = "Empiryzm", Description = "Zwiększa szansę na przełom o 10% oraz wartość przełomu o 10%.", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 5_000_000, EffectType = "ScienceBonus", EffectValue = 0.10m },
            // === CZAS (jednorazowe tury w momencie odkrycia) ===
            new TechnologyDefinition { Id = 3, TechType = "ZakrzywCzasu", Category = "Czas", DisplayName = "Zakrzywienie czasu", Description = "Dodaje jednorazowo 10 tur w chwili odkrycia (działa tylko do 10. dnia wieku księstwa).", Level = 1, CostGold = 20000, ResearchTime = 15, CostScience = 300_000, EffectType = "StartTurns", EffectValue = 10m },
            new TechnologyDefinition { Id = 4, TechType = "ZalamCzasu", Category = "Czas", DisplayName = "Załamanie czasu", Description = "Dodaje jednorazowo dwukrotny dzienny limit tur (ok. 30, z Wieżami Czasu 34). Wymaga zakrzywienia czasu.", Level = 2, CostGold = 50000, ResearchTime = 25, CostScience = 300_000, RequiredTech = "ZakrzywCzasu", EffectType = "StartTurns", EffectValue = 30m },
            // === ZIEMIA: Osadnictwo → Rekultywacja → Górnictwo odkrywkowe ===
            new TechnologyDefinition { Id = 6, TechType = "Osadnictwo", Category = "Ziemia", DisplayName = "Osadnictwo", Description = "Obniża koszt zakupu ziemi.", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 300_000, EffectType = "LandCostReduction", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 5, TechType = "Rekultywacja", Category = "Ziemia", DisplayName = "Rekultywacja", Description = "Drugi poziom Osadnictwa. Obniża koszt zagospodarowania pustkowi o 1/3.", Level = 2, CostGold = 15000, ResearchTime = 15, CostScience = 3_000_000, RequiredTech = "Osadnictwo", EffectType = "LandCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 7, TechType = "GornictwoOdkrywkowe", Category = "Ziemia", DisplayName = "Górnictwo odkrywkowe", Description = "Zastępuje chaotyczne przychody z kopalni stabilnym urobkiem (21% złota produkowanego przez alchemików).", Level = 3, CostGold = 30000, ResearchTime = 20, CostScience = 1_000_000, RequiredTech = "Rekultywacja", EffectType = "MineGold", EffectValue = 0.21m },
            // === SMOKI (limit smoków bez smokodrapu: +12/32/40%) ===
            new TechnologyDefinition { Id = 8, TechType = "Smokoastronomia", Category = "Smoki", DisplayName = "Smokoastronomia", Description = "O 12% podnosi limit smoków bez smokodrapu (do 53), zwiększa szansę na smoka i ilość smoków z labiryntu, skuteczność generała i akcji złodziejskiej ZS.", Level = 1, CostGold = 8000, ResearchTime = 10, CostScience = 2_000_000, EffectType = "DragonKnowledge", EffectValue = 1.0m },
            new TechnologyDefinition { Id = 9, TechType = "Smokoanatomia", Category = "Smoki", DisplayName = "Smokoanatomia", Description = "O 32% podnosi limit smoków bez smokodrapu (do 58) i wzmacnia pozostałe efekty smocze. Wymaga smokoastronomii.", Level = 2, CostGold = 20000, ResearchTime = 18, CostScience = 3_000_000, RequiredTech = "Smokoastronomia", EffectType = "DragonKnowledge", EffectValue = 2.0m },
            new TechnologyDefinition { Id = 10, TechType = "Smokodynamika", Category = "Smoki", DisplayName = "Smokodynamika", Description = "O 40% podnosi limit smoków bez smokodrapu (do 60) i wzmacnia pozostałe efekty smocze. Wymaga smokoastronomii i smokoanatomii.", Level = 3, CostGold = 45000, ResearchTime = 25, CostScience = 4_000_000, RequiredTech = "Smokoanatomia", EffectType = "DragonKnowledge", EffectValue = 3.0m },
            // === EKONOMIA: Rachunkowość → Buchalteria → Księgowość (zniżki podatku przy kupnie) ===
            new TechnologyDefinition { Id = 11, TechType = "Rachunkowosc", Category = "Ekonomia", DisplayName = "Rachunkowość", Description = "Obniża podatek przy kupnie jedzenia o 3 punkty procentowe (również przy braku SSH).", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 2_000_000, EffectType = "MerchantBonus", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 12, TechType = "Buchalteria", Category = "Ekonomia", DisplayName = "Buchalteria", Description = "Obniża podatek przy kupnie jedzenia i kamienia o 7 punktów procentowych. Wymaga rachunkowości.", Level = 2, CostGold = 15000, ResearchTime = 15, CostScience = 4_000_000, RequiredTech = "Rachunkowosc", EffectType = "MerchantBonus", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 13, TechType = "Ksiegowosc", Category = "Ekonomia", DisplayName = "Księgowość", Description = "Obniża podatek przy kupnie jedzenia, kamienia i broni o 10 punktów procentowych. Wymaga rachunkowości i buchalterii.", Level = 3, CostGold = 35000, ResearchTime = 22, CostScience = 6_000_000, RequiredTech = "Buchalteria", EffectType = "MerchantBonus", EffectValue = 0.30m },
            // === WOJSKO — broń: Ostrzenie → Naprawa → Przekuwanie ===
            new TechnologyDefinition { Id = 14, TechType = "OstrzenieBroni", Category = "Wojsko", DisplayName = "Ostrzenie broni", Description = "Obniża koszt jednostek E2 w broni o 5 i podnosi odzysk broni z Renowacji Broni o 5%.", Level = 1, CostGold = 8000, ResearchTime = 10, CostScience = 2_000_000, EffectType = "WeaponCostReduction", EffectValue = 5m },
            new TechnologyDefinition { Id = 15, TechType = "NaprawaBroni", Category = "Wojsko", DisplayName = "Naprawa broni", Description = "Obniża koszt jednostek E2 w broni o 15 i podnosi odzysk broni z Renowacji Broni o 15%. Wymaga ostrzenia broni.", Level = 2, CostGold = 20000, ResearchTime = 18, CostScience = 4_000_000, RequiredTech = "OstrzenieBroni", EffectType = "WeaponCostReduction", EffectValue = 15m },
            new TechnologyDefinition { Id = 16, TechType = "PrzekuwanieBroni", Category = "Wojsko", DisplayName = "Przekuwanie broni", Description = "Obniża koszt jednostek E2 w broni o 20 i E1 o 5, podnosi odzysk broni z Renowacji Broni o 20%. Wymaga ostrzenia i naprawy broni.", Level = 3, CostGold = 45000, ResearchTime = 25, CostScience = 6_000_000, RequiredTech = "NaprawaBroni", EffectType = "WeaponCostReduction", EffectValue = 20m },
            // === 5-LEVEL CHAINS ===
            // Wynalazczość — podnosi maksymalny limit punktów nauki na turę (poziom = liczba odkryć)
            new TechnologyDefinition { Id = 17, TechType = "Wynalazki1", Category = "Nauka", DisplayName = "Wynalazczość prymitywna", Description = "Zwiększa maksymalny limit punktów nauki na turę do 35 000.", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 300_000, EffectType = "ScienceCap", EffectValue = 35000m },
            new TechnologyDefinition { Id = 18, TechType = "Wynalazki2", Category = "Nauka", DisplayName = "Wynalazczość podstawowa", Description = "Zwiększa maksymalny limit punktów nauki na turę do 50 000.", Level = 2, CostGold = 12000, ResearchTime = 12, CostScience = 3_000_000, RequiredTech = "Wynalazki1", EffectType = "ScienceCap", EffectValue = 50000m },
            new TechnologyDefinition { Id = 19, TechType = "Wynalazki3", Category = "Nauka", DisplayName = "Wynalazczość rozwinięta", Description = "Zwiększa maksymalny limit punktów nauki na turę do 100 000.", Level = 3, CostGold = 25000, ResearchTime = 18, CostScience = 9_000_000, RequiredTech = "Wynalazki2", EffectType = "ScienceCap", EffectValue = 100000m },
            new TechnologyDefinition { Id = 20, TechType = "Wynalazki4", Category = "Nauka", DisplayName = "Wynalazczość zaawansowana", Description = "Zwiększa maksymalny limit punktów nauki na turę do 125 000.", Level = 4, CostGold = 45000, ResearchTime = 25, CostScience = 15_000_000, RequiredTech = "Wynalazki3", EffectType = "ScienceCap", EffectValue = 125000m },
            new TechnologyDefinition { Id = 21, TechType = "Wynalazki5", Category = "Nauka", DisplayName = "Wynalazczość nowoczesna", Description = "Zwiększa maksymalny limit punktów nauki na turę do 150 000.", Level = 5, CostGold = 80000, ResearchTime = 35, CostScience = 21_000_000, RequiredTech = "Wynalazki4", EffectType = "ScienceCap", EffectValue = 150000m },
            // Architektura — zmniejsza cenę budynków specjalnych (4,5/9/15%); poz. 4–5: efekty specjalne
            new TechnologyDefinition { Id = 22, TechType = "Architektura1", Category = "Budowa", DisplayName = "Architektura prymitywna", Description = "Zmniejsza cenę budynków specjalnych o 4,5%.", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 300_000, EffectType = "SpecialBuildingCostReduction", EffectValue = 0.045m },
            new TechnologyDefinition { Id = 23, TechType = "Architektura2", Category = "Budowa", DisplayName = "Architektura podstawowa", Description = "Zmniejsza cenę budynków specjalnych o 9%.", Level = 2, CostGold = 12000, ResearchTime = 12, CostScience = 3_000_000, RequiredTech = "Architektura1", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.09m },
            new TechnologyDefinition { Id = 24, TechType = "Architektura3", Category = "Budowa", DisplayName = "Architektura rozwinięta", Description = "Zmniejsza cenę budynków specjalnych o 15%.", Level = 3, CostGold = 25000, ResearchTime = 18, CostScience = 9_000_000, RequiredTech = "Architektura2", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 25, TechType = "Architektura4", Category = "Budowa", DisplayName = "Architektura zaawansowana", Description = "Budynki 3. rzędu nie kosztują złota niezależnie od czarnej magii; przyspieszanie budowy dodatkowo tańsze o 50%.", Level = 4, CostGold = 45000, ResearchTime = 25, CostScience = 15_000_000, RequiredTech = "Architektura3", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 26, TechType = "Architektura5", Category = "Budowa", DisplayName = "Architektura nowoczesna", Description = "Budynki 6. i 7. rzędu budują się o turę szybciej.", Level = 5, CostGold = 80000, ResearchTime = 35, CostScience = 21_000_000, RequiredTech = "Architektura4", EffectType = "SpecialBuildingCostReduction", EffectValue = 0.15m },
            // Inżynieria — zmniejsza cenę zabudowań w złocie (8/16/24%); poz. 4–5: kamień/odzysk
            new TechnologyDefinition { Id = 27, TechType = "Inzynieria1", Category = "Budowa", DisplayName = "Inżynieria prymitywna", Description = "Zmniejsza cenę zabudowań w złocie o 8%.", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 300_000, EffectType = "EcoBuildingCostReduction", EffectValue = 0.08m },
            new TechnologyDefinition { Id = 28, TechType = "Inzynieria2", Category = "Budowa", DisplayName = "Inżynieria podstawowa", Description = "Zmniejsza cenę zabudowań w złocie o 16%.", Level = 2, CostGold = 12000, ResearchTime = 12, CostScience = 3_000_000, RequiredTech = "Inzynieria1", EffectType = "EcoBuildingCostReduction", EffectValue = 0.16m },
            new TechnologyDefinition { Id = 29, TechType = "Inzynieria3", Category = "Budowa", DisplayName = "Inżynieria rozwinięta", Description = "Zmniejsza cenę zabudowań w złocie o 24%.", Level = 3, CostGold = 25000, ResearchTime = 18, CostScience = 9_000_000, RequiredTech = "Inzynieria2", EffectType = "EcoBuildingCostReduction", EffectValue = 0.24m },
            new TechnologyDefinition { Id = 30, TechType = "Inzynieria4", Category = "Budowa", DisplayName = "Inżynieria zaawansowana", Description = "Murarze zużywają 10% mniej kamienia.", Level = 4, CostGold = 45000, ResearchTime = 25, CostScience = 12_000_000, RequiredTech = "Inzynieria3", EffectType = "EcoBuildingCostReduction", EffectValue = 0.24m },
            new TechnologyDefinition { Id = 31, TechType = "Inzynieria5", Category = "Budowa", DisplayName = "Inżynieria nowoczesna", Description = "Wyburzanie budynków zwraca 80% budulca.", Level = 5, CostGold = 80000, ResearchTime = 35, CostScience = 12_000_000, RequiredTech = "Inzynieria4", EffectType = "EcoBuildingCostReduction", EffectValue = 0.24m },
            // Czarodziejstwo — zmniejsza cenę czarów (4,5/9/15/21/30%)
            new TechnologyDefinition { Id = 32, TechType = "Czarodziejstwo1", Category = "Magia", DisplayName = "Czarodziejstwo prymitywne", Description = "Zmniejsza cenę czarów o 4,5%.", Level = 1, CostGold = 8000, ResearchTime = 10, CostScience = 300_000, EffectType = "SpellPower", EffectValue = 0.045m },
            new TechnologyDefinition { Id = 33, TechType = "Czarodziejstwo2", Category = "Magia", DisplayName = "Czarodziejstwo podstawowe", Description = "Zmniejsza cenę czarów o 9%.", Level = 2, CostGold = 18000, ResearchTime = 16, CostScience = 3_000_000, RequiredTech = "Czarodziejstwo1", EffectType = "SpellPower", EffectValue = 0.09m },
            new TechnologyDefinition { Id = 34, TechType = "Czarodziejstwo3", Category = "Magia", DisplayName = "Czarodziejstwo rozwinięte", Description = "Zmniejsza cenę czarów o 15%.", Level = 3, CostGold = 35000, ResearchTime = 22, CostScience = 9_000_000, RequiredTech = "Czarodziejstwo2", EffectType = "SpellPower", EffectValue = 0.15m },
            new TechnologyDefinition { Id = 35, TechType = "Czarodziejstwo4", Category = "Magia", DisplayName = "Czarodziejstwo zaawansowane", Description = "Zmniejsza cenę czarów o 21%.", Level = 4, CostGold = 55000, ResearchTime = 28, CostScience = 15_000_000, RequiredTech = "Czarodziejstwo3", EffectType = "SpellPower", EffectValue = 0.21m },
            new TechnologyDefinition { Id = 36, TechType = "Czarodziejstwo5", Category = "Magia", DisplayName = "Czarodziejstwo nowoczesne", Description = "Zmniejsza cenę czarów o 30%.", Level = 5, CostGold = 90000, ResearchTime = 38, CostScience = 21_000_000, RequiredTech = "Czarodziejstwo4", EffectType = "SpellPower", EffectValue = 0.30m },
            // Trening — przyspiesza szkolenie wojska
            new TechnologyDefinition { Id = 37, TechType = "Trening1", Category = "Wojsko", DisplayName = "Trening prymitywny", Description = "Przyspiesza szkolenie wojska (poziom 1 z 5).", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 300_000, EffectType = "TrainingSpeed", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 38, TechType = "Trening2", Category = "Wojsko", DisplayName = "Trening podstawowy", Description = "Przyspiesza szkolenie wojska (poziom 2 z 5).", Level = 2, CostGold = 12000, ResearchTime = 12, CostScience = 3_000_000, RequiredTech = "Trening1", EffectType = "TrainingSpeed", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 39, TechType = "Trening3", Category = "Wojsko", DisplayName = "Trening rozwinięty", Description = "Przyspiesza szkolenie wojska (poziom 3 z 5).", Level = 3, CostGold = 25000, ResearchTime = 18, CostScience = 9_000_000, RequiredTech = "Trening2", EffectType = "TrainingSpeed", EffectValue = 0.30m },
            new TechnologyDefinition { Id = 40, TechType = "Trening4", Category = "Wojsko", DisplayName = "Trening zaawansowany", Description = "Przyspiesza szkolenie wojska (poziom 4 z 5).", Level = 4, CostGold = 45000, ResearchTime = 25, CostScience = 15_000_000, RequiredTech = "Trening3", EffectType = "TrainingSpeed", EffectValue = 0.40m },
            new TechnologyDefinition { Id = 41, TechType = "Trening5", Category = "Wojsko", DisplayName = "Trening nowoczesny", Description = "Przyspiesza szkolenie wojska (poziom 5 z 5).", Level = 5, CostGold = 80000, ResearchTime = 35, CostScience = 21_000_000, RequiredTech = "Trening4", EffectType = "TrainingSpeed", EffectValue = 0.50m },
            // Rekrutacja — zmniejsza cenę złodziei (5/10/20%); poz. 4–5: efekty specjalne
            new TechnologyDefinition { Id = 42, TechType = "Rekrutacja1", Category = "Wojsko", DisplayName = "Rekrutacja prymitywna", Description = "Zmniejsza cenę złodziei o 5%.", Level = 1, CostGold = 5000, ResearchTime = 8, CostScience = 300_000, EffectType = "RecruitCostReduction", EffectValue = 0.05m },
            new TechnologyDefinition { Id = 43, TechType = "Rekrutacja2", Category = "Wojsko", DisplayName = "Rekrutacja podstawowa", Description = "Zmniejsza cenę złodziei o 10%.", Level = 2, CostGold = 12000, ResearchTime = 12, CostScience = 3_000_000, RequiredTech = "Rekrutacja1", EffectType = "RecruitCostReduction", EffectValue = 0.10m },
            new TechnologyDefinition { Id = 44, TechType = "Rekrutacja3", Category = "Wojsko", DisplayName = "Rekrutacja rozwinięta", Description = "Zmniejsza cenę złodziei o 20%.", Level = 3, CostGold = 25000, ResearchTime = 18, CostScience = 9_000_000, RequiredTech = "Rekrutacja2", EffectType = "RecruitCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 45, TechType = "Rekrutacja4", Category = "Wojsko", DisplayName = "Rekrutacja zaawansowana", Description = "Umożliwia kradzież zapasów z karawan.", Level = 4, CostGold = 45000, ResearchTime = 25, CostScience = 12_000_000, RequiredTech = "Rekrutacja3", EffectType = "RecruitCostReduction", EffectValue = 0.20m },
            new TechnologyDefinition { Id = 46, TechType = "Rekrutacja5", Category = "Wojsko", DisplayName = "Rekrutacja nowoczesna", Description = "Ranni generałowie zachowują poziom równy lvl/3.", Level = 5, CostGold = 80000, ResearchTime = 35, CostScience = 12_000_000, RequiredTech = "Rekrutacja4", EffectType = "RecruitCostReduction", EffectValue = 0.20m }
        );
    }

    /// <summary>
    /// Zaklęcia wg oryginalnego Red Dragon — autentyczna lista i CENY BAZOWE
    /// (przy 100 akrach) z polskiego manuala reddragon.pl/manual/2/magie.php
    /// (docs/zrodla/manual-pl/magie.txt). Drożenie: +10% za zaklęcie
    /// (Dżin 9% z Pałacem magicznym, Gnom 11%); po turze poziom drożyzny
    /// spada do (poziom/2)+45, min 100%. Siła zaklęcia: 80–120% siły magów
    /// (+20% z Soczewką magiczną). Długoterminowe tracą ~50% siły na turę.
    /// RequiredBooks = numer księgi (0 = podstawowe; 1 Mocy, 2 Ziemi, 3 Ognia,
    /// 4 Wiatru, 5 Mistyki) — uproszczenie oryginalnego wyboru ksiąg.
    /// </summary>
    private void SeedSpellDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpellDefinition>().HasData(
            // === PODSTAWOWE (dostępne dla każdej rasy magicznej) ===
            new SpellDefinition { Id = 1, SpellType = "SokoleOko", Category = "Pozostałe", DisplayName = "Sokole Oko", Description = "Pokazuje podstawowe informacje o wrogim księstwie (E2, E1, hoplici, złodzieje, magowie, machiny). Rzucone minimalną siłą służy jako sonda obrony magicznej.", ManaCost = 20, PowerLevel = 1, EffectType = "EagleEye", TargetType = "Enemy", RequiredBooks = 0 },
            new SpellDefinition { Id = 2, SpellType = "DobryHumor", Category = "Biała", DisplayName = "Dobry humor", Description = "+1 popularności co turę (jak Zajazd u Czerwonego Smoka)", ManaCost = 125, PowerLevel = 1, EffectType = "PopularityBuff", TargetType = "Self", RequiredBooks = 0 },
            new SpellDefinition { Id = 3, SpellType = "ZdjecieZaklecia", Category = "Pozostałe", DisplayName = "Zdjęcie zaklęcia", Description = "Osłabia wybrane zaklęcie o podwojoną siłę Twoich magów (min. 20% siły zaklęcia). Tylko na własne księstwo. Br-Oug: o 50% droższe.", ManaCost = 125, PowerLevel = 1, EffectType = "Dispel", TargetType = "Self", RequiredBooks = 0 },
            new SpellDefinition { Id = 4, SpellType = "Pracowitosc", Category = "Biała", DisplayName = "Pracowitość", Description = "Zwiększa wydajność niemagicznych profesji do +49%", ManaCost = 340, PowerLevel = 2, EffectType = "ProductionBuff", TargetType = "Self", RequiredBooks = 0 },
            new SpellDefinition { Id = 5, SpellType = "Mannamorfoza", Category = "Pozostałe", DisplayName = "Mannamorfoza", Description = "Zamienia manę w złoto — 200 sztuk złota za 1 manę", ManaCost = 85, PowerLevel = 1, EffectType = "Mannamorphosis", TargetType = "Self", RequiredBooks = 0 },
            // === KSIĘGA 1: MOCY ===
            new SpellDefinition { Id = 10, SpellType = "TarczaAntymagiczna", Category = "Tarcze", DisplayName = "Tarcza antymagiczna", Description = "Zwiększa obronę magiczną do +24% (nie działa przez pakty)", ManaCost = 210, PowerLevel = 2, EffectType = "AntimagicShield", TargetType = "Self", RequiredBooks = 1 },
            new SpellDefinition { Id = 11, SpellType = "TarczaWojenna", Category = "Tarcze", DisplayName = "Tarcza wojenna", Description = "Zwiększa obronę wojskową do +24% (tylko obrona własnych jednostek)", ManaCost = 380, PowerLevel = 2, EffectType = "WarShield", TargetType = "Self", RequiredBooks = 1 },
            new SpellDefinition { Id = 12, SpellType = "Szczescie", Category = "Biała", DisplayName = "Szczęście", Description = "+10% przyrostu; zwiększa szansę na smoka, złoto z kopalni, przyjście generała, odbicie zaklęć i fart w labiryncie (max 49%)", ManaCost = 210, PowerLevel = 2, EffectType = "LuckBuff", TargetType = "Self", RequiredBooks = 1 },
            new SpellDefinition { Id = 13, SpellType = "ZwierciadloMagiczne", Category = "Tarcze", DisplayName = "Zwierciadło magiczne", Description = "Do 24% szansy na odbicie nieudanych zaklęć wroga (+20% siły odbicia z Soczewką magiczną)", ManaCost = 680, PowerLevel = 3, EffectType = "MagicShield", TargetType = "Self", RequiredBooks = 1 },
            new SpellDefinition { Id = 14, SpellType = "PadleLegiony", Category = "Tarcze", DisplayName = "Padłe legiony", Description = "Duchy poległych bronią księstwa: obrona = min(siła zaklęcia, liczba magów). Zdejmowane tylko Klątwą Padłych Legionów.", ManaCost = 425, PowerLevel = 3, EffectType = "LegionShield", TargetType = "Self", RequiredBooks = 1 },
            // === KSIĘGA 2: ZIEMI ===
            new SpellDefinition { Id = 20, SpellType = "Plodnosc", Category = "Biała", DisplayName = "Płodność", Description = "Przyrost ludności +30%", ManaCost = 210, PowerLevel = 2, EffectType = "GrowthBuff", TargetType = "Self", RequiredBooks = 2 },
            new SpellDefinition { Id = 21, SpellType = "TrzesienieZiemi", Category = "Niszcząca", DisplayName = "Trzęsienie Ziemi", Description = "Burzy 1–2% budynków infrastruktury, 50%·x szansy na budynek specjalny. Limit 5 na cel (Krasnolud 4, Goblin 3).", ManaCost = 190, PowerLevel = 3, EffectType = "BuildingDamage", TargetType = "Enemy", IsLimited = true, RequiredBooks = 2 },
            new SpellDefinition { Id = 22, SpellType = "Szarancza", Category = "Czarna", DisplayName = "Szarańcza", Description = "Ludność potrzebuje do +300% więcej jedzenia, niszczy 9% zapasów (armie Nekromanty nie jedzą)", ManaCost = 125, PowerLevel = 2, EffectType = "FoodDamage", TargetType = "Enemy", RequiredBooks = 2 },
            new SpellDefinition { Id = 23, SpellType = "Zaraza", Category = "Czarna", DisplayName = "Zaraza", Description = "Co turę umiera do 3% ludności (Olbrzym odporny)", ManaCost = 275, PowerLevel = 3, EffectType = "PopulationDamage", TargetType = "Enemy", RequiredBooks = 2 },
            new SpellDefinition { Id = 24, SpellType = "KlatwaPadlychLegionow", Category = "Pozostałe", DisplayName = "Klątwa Padłych Legionów", Description = "Odsyła duchy poległych do grobów — osłabia Padłe legiony o siłę tego zaklęcia", ManaCost = 100, PowerLevel = 2, EffectType = "DoomLegions", TargetType = "Enemy", RequiredBooks = 2 },
            // === KSIĘGA 3: OGNIA ===
            new SpellDefinition { Id = 30, SpellType = "ZlyHumor", Category = "Czarna", DisplayName = "Zły humor", Description = "−1 popularności wroga co turę (Hobbit odporny)", ManaCost = 65, PowerLevel = 1, EffectType = "PopularityDebuff", TargetType = "Enemy", RequiredBooks = 3 },
            new SpellDefinition { Id = 31, SpellType = "Slabosc", Category = "Czarna", DisplayName = "Słabość", Description = "Obniża obronę wojskową wroga do −24% (tylko obronę własnych jednostek celu)", ManaCost = 85, PowerLevel = 2, EffectType = "DefenseDebuff", TargetType = "Enemy", RequiredBooks = 3 },
            new SpellDefinition { Id = 32, SpellType = "OgnistyDeszcz", Category = "Niszcząca", DisplayName = "Ognisty Deszcz", Description = "Zabija 2–4% mieszkańców (armia + profesje). Limit 5 na cel (Krasnolud 4, Goblin 3).", ManaCost = 340, PowerLevel = 3, EffectType = "ArmyDamage", TargetType = "Enemy", IsLimited = true, RequiredBooks = 3 },
            new SpellDefinition { Id = 33, SpellType = "Pech", Category = "Czarna", DisplayName = "Pech", Description = "−10% przyrostu ludności i mniej szczęścia w zdarzeniach losowych", ManaCost = 65, PowerLevel = 1, EffectType = "GrowthDebuff", TargetType = "Enemy", RequiredBooks = 3 },
            new SpellDefinition { Id = 34, SpellType = "PrzywolanieSmoka", Category = "Przywołania", DisplayName = "Przywołanie Smoka", Description = "Wabi Czerwonego Smoka do armii; koszt zależy od liczby smoków: ×(D²·0,0001+0,2)·(max(50,D)/100)²", ManaCost = 500, PowerLevel = 4, EffectType = "SummonDragon", TargetType = "Self", RequiredBooks = 3 },
            // === KSIĘGA 4: WIATRU ===
            new SpellDefinition { Id = 40, SpellType = "ZniszczenieZapasow", Category = "Czarna", DisplayName = "Zniszczenie zapasów", Description = "Niszczy 20% zasobów wroga (Elf rzuca 10% słabiej, Dżin 10% silniej)", ManaCost = 125, PowerLevel = 2, EffectType = "SupplyDamage", TargetType = "Enemy", RequiredBooks = 4 },
            new SpellDefinition { Id = 41, SpellType = "Huragan", Category = "Niszcząca", DisplayName = "Huragan", Description = "Zabija 4% ludzi w profesjach (nie rusza armii i złodziei). Limit 7 na cel (Krasnolud 6, Goblin 5).", ManaCost = 255, PowerLevel = 3, EffectType = "WorkerDamage", TargetType = "Enemy", IsLimited = true, RequiredBooks = 4 },
            new SpellDefinition { Id = 42, SpellType = "SpopielenieZlodziei", Category = "Niszcząca", DisplayName = "Spopielenie złodziei", Description = "Spala 5–10% złodziei wroga. Limit 7 na cel (Krasnolud 6; Goblin całkowicie odporny).", ManaCost = 210, PowerLevel = 3, EffectType = "ThiefDamage", TargetType = "Enemy", IsLimited = true, RequiredBooks = 4 },
            new SpellDefinition { Id = 43, SpellType = "Chochliki", Category = "Czarna", DisplayName = "Chochliki", Description = "Co turę niszczą część machin wojennych (Gnom odporny — nie używa machin)", ManaCost = 125, PowerLevel = 2, EffectType = "MachineDamage", TargetType = "Enemy", RequiredBooks = 4 },
            new SpellDefinition { Id = 44, SpellType = "SmoczyOddech", Category = "Niszcząca", DisplayName = "Smoczy Oddech", Description = "Najpotężniejsze zaklęcie: burzy 1–2% budynków, zabija 3–5% armii i 5–10% ludności, 50% szansy na budynek specjalny. Wymaga Pałacu Magicznego. Limit 5 (Krasnolud 4, Goblin 3).", ManaCost = 1500, PowerLevel = 5, EffectType = "DragonBreath", TargetType = "Enemy", IsLimited = true, RequiredBooks = 4 },
            // === KSIĘGA 5: MISTYKI ===
            new SpellDefinition { Id = 50, SpellType = "Somnambulizm", Category = "Czarna", DisplayName = "Somnambulizm", Description = "Obniża wydajność niemagicznych profesji wroga do −50%", ManaCost = 105, PowerLevel = 2, EffectType = "ProductionDebuff", TargetType = "Enemy", RequiredBooks = 5 },
            new SpellDefinition { Id = 51, SpellType = "Glupota", Category = "Czarna", DisplayName = "Głupota", Description = "Obniża wydajność magów i druidów o 25% oraz obronę magiczną celu i jego paktów", ManaCost = 85, PowerLevel = 2, EffectType = "StupidityDebuff", TargetType = "Enemy", RequiredBooks = 5 },
            new SpellDefinition { Id = 52, SpellType = "FluidMagiczny", Category = "Biała", DisplayName = "Fluid magiczny", Description = "Zwiększa wydajność magicznych profesji do +49%", ManaCost = 210, PowerLevel = 2, EffectType = "MagicBuff", TargetType = "Self", RequiredBooks = 5 },
            new SpellDefinition { Id = 53, SpellType = "Kastracja", Category = "Czarna", DisplayName = "Kastracja", Description = "Przyrost ludności wroga −50% (Nekromant odporny)", ManaCost = 85, PowerLevel = 2, EffectType = "GrowthDebuff", TargetType = "Enemy", RequiredBooks = 5 },
            // === RASOWE ===
            new SpellDefinition { Id = 60, SpellType = "WzmocnionaMagia", Category = "Rasowe", DisplayName = "Wzmocniona magia", Description = "Metamagia Dżina: zaklęcia +10% siły, +25% ceny", ManaCost = 210, PowerLevel = 2, EffectType = "Metamagic", TargetType = "Self", RequiredRace = "Dżin", RequiredBooks = 0 },
            new SpellDefinition { Id = 61, SpellType = "PrzyspieszonaMagia", Category = "Rasowe", DisplayName = "Przyspieszona magia", Description = "Metamagia Dżina: zaklęcia −10% ceny, −25% siły", ManaCost = 210, PowerLevel = 2, EffectType = "Metamagic", TargetType = "Self", RequiredRace = "Dżin", RequiredBooks = 0 },
            new SpellDefinition { Id = 62, SpellType = "WezwanieTotemu", Category = "Rasowe", DisplayName = "Wezwanie totemu", Description = "Szamanizm Olbrzyma: ładuje wybrany totem (koszt totemu: obszar×20)", ManaCost = 380, PowerLevel = 2, EffectType = "TotemCharge", TargetType = "Self", RequiredRace = "Olbrzym", RequiredBooks = 0 },
            new SpellDefinition { Id = 63, SpellType = "Ofiarowanie", Category = "Rasowe", DisplayName = "Ofiarowanie", Description = "Nekromancja: −10% populacji/turę, polegli stają się ciałami", ManaCost = 210, PowerLevel = 2, EffectType = "Sacrifice", TargetType = "Self", RequiredRace = "Nekromant", RequiredBooks = 0 },
            new SpellDefinition { Id = 64, SpellType = "PrzywolajE2", Category = "Rasowe", DisplayName = "Przywołaj elitę 2. stopnia", Description = "Nekromancja: wskrzesza E2 (10% wolnych magów, 1 ciało/jednostkę)", ManaCost = 1000, PowerLevel = 3, EffectType = "SummonE2", TargetType = "Self", RequiredRace = "Nekromant", RequiredBooks = 0 },
            new SpellDefinition { Id = 65, SpellType = "PrzywolajE1", Category = "Rasowe", DisplayName = "Przywołaj elitę 1. stopnia", Description = "Nekromancja: wskrzesza E1 (26% wolnych magów, 1/6 ciała)", ManaCost = 1000, PowerLevel = 3, EffectType = "SummonE1", TargetType = "Self", RequiredRace = "Nekromant", RequiredBooks = 0 },
            new SpellDefinition { Id = 66, SpellType = "PrzywolajHoplitow", Category = "Rasowe", DisplayName = "Przywołaj hoplitów", Description = "Nekromancja: wskrzesza hoplitów (50% wolnych magów, 1/6 ciała)", ManaCost = 1000, PowerLevel = 3, EffectType = "SummonHoplites", TargetType = "Self", RequiredRace = "Nekromant", RequiredBooks = 0 },
            new SpellDefinition { Id = 67, SpellType = "PrzywolajZlodziei", Category = "Rasowe", DisplayName = "Przywołaj złodziei", Description = "Nekromancja: wskrzesza złodziei (50% wolnych magów, 1/2 ciała)", ManaCost = 1000, PowerLevel = 3, EffectType = "SummonThieves", TargetType = "Self", RequiredRace = "Nekromant", RequiredBooks = 0 }
        );
    }

    /// <summary>
    /// Akcje złodziejskie wg oryginału. Szansa powodzenia zależy od stosunku sił
    /// złodziejskich atak/obrona (wzory w docs/MECHANIKA.md §10), nie od stałej bazy —
    /// SuccessBaseRate pełni rolę pomocniczego mnożnika trudności akcji.
    /// </summary>
    private void SeedThiefActionDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ThiefActionDefinition>().HasData(
            new ThiefActionDefinition { Id = 1, ActionType = "ObserwacjaKsiestwa", DisplayName = "Obserwacja księstwa", Description = "Wywiad: stan wojsk, zasobów i budynków wroga (bez expów dla generała)", ThievesRequired = 20, SuccessBaseRate = 1.00m, EffectType = "Spy" },
            new ThiefActionDefinition { Id = 2, ActionType = "KradziezZapasow", DisplayName = "Kradzież zapasów", Description = "Kradnie złoto i surowce wroga (bez expów dla generała)", ThievesRequired = 50, SuccessBaseRate = 0.90m, EffectType = "StealSupplies" },
            new ThiefActionDefinition { Id = 3, ActionType = "PodzeganieDoRewolty", DisplayName = "Podżeganie do rewolty", Description = "Obniża popularność wroga (Hobbit: efekt połowiczny)", ThievesRequired = 60, SuccessBaseRate = 0.80m, EffectType = "Revolt" },
            new ThiefActionDefinition { Id = 4, ActionType = "BurzenieBudynkow", DisplayName = "Burzenie budynków", Description = "Niszczy infrastrukturę wroga", ThievesRequired = 100, SuccessBaseRate = 0.70m, EffectType = "DemolishBuildings" },
            new ThiefActionDefinition { Id = 5, ActionType = "WojnaGangow", DisplayName = "Wojna gangów", Description = "Zabija złodziei wroga", ThievesRequired = 80, SuccessBaseRate = 0.80m, EffectType = "ThiefWar" },
            new ThiefActionDefinition { Id = 6, ActionType = "WymordowanieMagow", DisplayName = "Wymordowanie magów", Description = "Zabija magów wroga", ThievesRequired = 120, SuccessBaseRate = 0.60m, EffectType = "KillMages" },
            new ThiefActionDefinition { Id = 7, ActionType = "ZabijanieLudnosci", DisplayName = "Zabijanie ludności", Description = "Morduje cywilów wroga", ThievesRequired = 100, SuccessBaseRate = 0.70m, EffectType = "KillPeople" },
            new ThiefActionDefinition { Id = 8, ActionType = "UpijanieArmii", DisplayName = "Upijanie armii", Description = "Upija żołnierzy wroga — nie bronią w następnym przeliczeniu", ThievesRequired = 90, SuccessBaseRate = 0.70m, EffectType = "DrunkArmy" },
            new ThiefActionDefinition { Id = 9, ActionType = "ZabojstwoGenerala", DisplayName = "Zabójstwo generała", Description = "Próba zamachu na generała wroga", ThievesRequired = 200, SuccessBaseRate = 0.30m, EffectType = "KillGeneral" },
            new ThiefActionDefinition { Id = 10, ActionType = "PorwanieGenerala", DisplayName = "Porwanie generała", Description = "Próba porwania generała wroga (można negocjować okup)", ThievesRequired = 200, SuccessBaseRate = 0.25m, EffectType = "KidnapGeneral" }
        );
    }
}
