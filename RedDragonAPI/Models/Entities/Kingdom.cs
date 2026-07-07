using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

[Table("Kingdoms")]
public class Kingdom
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Race { get; set; } = "Człowiek";

    public bool IsMagicRace { get; set; } = true;

    // Próg ziemi, po którym księstwo traci status nowicjusza („parasol" ochronny spada).
    public const int NoviceLandCap = 30000;

    // Zasoby (Red Dragon faithful)
    public int Land { get; set; } = 1000;
    public long Gold { get; set; } = 50000;
    public long Food { get; set; } = 10000;
    public long Stone { get; set; } = 2000;
    public long Budulec { get; set; } = 0;
    public long BudulecStored { get; set; } = 0;
    public long Weapons { get; set; } = 0;
    public long Mana { get; set; } = 0;

    // Punkty akcji labiryntu wykorzystane w bieżącym przeliczeniu (docs/MECHANIKA.md §13).
    // Budżet na przeliczenie: 2 pkt (4 z Sanktuarium Stwórcy); skarb kosztuje 2 pkt,
    // akcja generała (szukanie / zmiana zdolności) 1 pkt. Zerowane w przeliczeniu dziennym.
    public int LabyrinthActionsUsed { get; set; } = 0;

    // Ciała na cmentarzu — surowiec Nekromancji (docs/MECHANIKA.md §2.2)
    public long Bodies { get; set; } = 0;

    /// <summary>Metamagia Dżina (docs/MECHANIKA.md §2.2): None | Strengthened | Accelerated.</summary>
    [MaxLength(20)]
    public string MetamagicMode { get; set; } = "None";

    /// <summary>Gniew Enta (docs/MECHANIKA.md §2.2): +100% ataku po stratach, do najbliższego przeliczenia.</summary>
    public bool EntWrathActive { get; set; } = false;

    /// <summary>
    /// Upojenie armii (akcja złodziejska, docs/MECHANIKA.md §10): % armii upitej —
    /// obniża obronę o tyle procent; zerowane w DailyResetService po wykonaniu ataków.
    /// </summary>
    public int DrunkArmyPct { get; set; } = 0;

    /// <summary>
    /// Goblińska inżynieria (docs/MECHANIKA.md §2.2): machiny Goblina wysłane z E2
    /// obniżają obronę celu o 20% swojej siły przy kolejnych atakach w tym samym
    /// przeliczeniu; zerowane w DailyResetService po fazie wojskowej.
    /// </summary>
    public long SiegeDefensePenalty { get; set; } = 0;

    // Dozbrojenie Krasnoluda (blog 31. wieku): do 2 punktów +1 atak/obrona dla E1/E2
    // za broń (1. punkt = ziemia×50, 2. = ziemia×100); reset po przeliczeniu.
    public int RearmE1Attack { get; set; } = 0;
    public int RearmE1Defense { get; set; } = 0;
    public int RearmE2Attack { get; set; } = 0;
    public int RearmE2Defense { get; set; } = 0;

    /// <summary>
    /// Komando łuczników Elfa (blog 31. wieku): księstwo z paktem wojskowym wspierane
    /// łucznikami (+20% jego obrony, −20% obrony Elfa); reset po przeliczeniu.
    /// </summary>
    public int? ArcherCommandoTargetId { get; set; }

    // Hodokvas Hobbita (blog 31. wieku): uczta od popularności ≥80 (+20 od razu,
    // jedzenie 5/os., szkolenie ×0,6, przyrost ×1,5); koniec ręczny po ≥4 turach.
    public bool HodokvasActive { get; set; } = false;
    public int HodokvasTurnsPlayed { get; set; } = 0;

    /// <summary>
    /// Auto-rzucanie (docs/GAME_DESIGN.md, krok 24 tury): zaklęcie pozytywne rzucane
    /// automatycznie na siebie po każdym przeliczeniu (kosztuje turę i manę). Null = wyłączone.
    /// </summary>
    [MaxLength(100)]
    public string? AutoCastSpellType { get; set; }

    // Szamanizm Olbrzyma (docs/MECHANIKA.md §2.2) — 3 totemy ładowane maną (poziom 0–10)
    public int TotemPlunder { get; set; } = 0;       // +5%/lvl zrabowanych surowców
    public int TotemDragonSlay { get; set; } = 0;    // zabija %/lvl smoków wroga
    public int TotemDestruction { get; set; } = 0;   // +5%/lvl zdobytej ziemi (burzenie)

    /// <summary>Nauka stosowana Człowieka (docs/MECHANIKA.md §2.2): None | Thief | Magic | Military (+10% w danej dziedzinie).</summary>
    [MaxLength(20)]
    public string AppliedScienceSchool { get; set; } = "None";

    // Ludność
    public int Population { get; set; } = 1000;
    public int Popularity { get; set; } = 100;
    public int Wages { get; set; } = 50;

    // Edukacja (from Naukowcy, max 15%) — legacy mnożnik produkcji
    [Column(TypeName = "decimal(5,2)")]
    public decimal Education { get; set; } = 0;

    // Badania: Punkty Nauki (docs/MECHANIKA.md §13). SciencePoints to nadprodukcja
    // odłożona „w zapasie", CurrentResearchTech to aktualnie rozwijana dziedzina.
    public long SciencePoints { get; set; } = 0;

    [MaxLength(100)]
    public string? CurrentResearchTech { get; set; }

    // Tury
    public int TurnsAvailable { get; set; } = 15;
    public int TurnsPerDay { get; set; } = 15;
    public int MaxTurns { get; set; } = 49;
    public int TurnNumber { get; set; } = 0;

    /// <summary>
    /// Przydział tur w bieżącym cyklu (po ostatnim resecie dziennym) — mianownik licznika 0→max.
    /// Wykorzystane tury = TurnsCapacity - TurnsAvailable (licznik rośnie w miarę grania).
    /// </summary>
    public int TurnsCapacity { get; set; } = 15;

    // Wiek księstwa (day count)
    public int Age { get; set; } = 0;

    // Budynek specjalny w budowie
    [MaxLength(100)]
    public string? CurrentSpecialBuilding { get; set; }
    public int SpecialBuildingProgress { get; set; } = 0;
    public int SpecialBuildingCost { get; set; } = 0;

    // Koalicja
    public int? CoalitionId { get; set; }

    [MaxLength(50)]
    public string? CoalitionRole { get; set; }

    /// <summary>Na kogo to księstwo głosuje w wyborach Imperatora koalicji (docs/MECHANIKA.md §12).</summary>
    public int? ImperatorVoteForKingdomId { get; set; }

    // Era
    public int EraId { get; set; }

    // Ochrona poczatkowa (status nowicjusza)
    public bool IsProtected { get; set; } = true;
    public int ProtectionDaysLeft { get; set; } = 5;

    // Mrożenie (zmrazení) — zawieszenie gubernatu na czas nieobecności
    public bool IsFrozen { get; set; } = false;
    public DateTime? FrozenAt { get; set; }

    // ── Opłata za księstwo ─────────────────────────────────────────────
    // Pierwsze księstwo konta jest darmowe (IsFree), księstwo imperatorskie
    // (CoalitionRole == "Imperator") jest zawsze zwolnione z opłaty.
    // Płatne księstwo trzeba opłacić do PaymentDeadlineDays dnia od założenia —
    // po tym terminie zostaje zawieszone (IsSuspended, nie da się go wybrać),
    // a po DeletionDays dniach bez opłaty jest trwale usuwane.
    public const int PaymentDeadlineDays = 20;
    public const int DeletionDays = 30;

    /// <summary>Księstwo darmowe (pierwsze na koncie) — nie wymaga opłaty.</summary>
    public bool IsFree { get; set; } = false;

    /// <summary>Czy opłata za księstwo została wniesiona.</summary>
    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }

    /// <summary>Zawieszone za brak opłaty — niedostępne do wyboru i gry.</summary>
    public bool IsSuspended { get; set; } = false;

    /// <summary>
    /// Ręczna blokada nałożona przez super admina. Gdy aktywna, IsSuspended też
    /// jest ustawiane (wspólna ścieżka egzekwowania) i opłacenie nie odblokowuje.
    /// </summary>
    public bool AdminLocked { get; set; } = false;

    /// <summary>Zwolnione z opłaty: darmowe, opłacone albo imperatorskie.</summary>
    [NotMapped]
    public bool IsPaymentExempt => IsFree || IsPaid || CoalitionRole == "Imperator";

    /// <summary>Ile pełnych dni istnieje księstwo.</summary>
    [NotMapped]
    public int DaysSinceCreation => (int)(DateTime.UtcNow - CreatedAt).TotalDays;

    /// <summary>Czy termin płatności minął (kandydat do zawieszenia).</summary>
    [NotMapped]
    public bool IsPaymentOverdue => !IsPaymentExempt && DaysSinceCreation >= PaymentDeadlineDays;

    /// <summary>Czy księstwo kwalifikuje się do usunięcia (30 dni bez opłaty).</summary>
    [NotMapped]
    public bool IsPaymentDeletable => !IsPaymentExempt && DaysSinceCreation >= DeletionDays;

    // Szkolenie wojska — automatyczny awans jednostek co turę (Dracopedia/Trening).
    // Żołnierze: E0→E1 (wymaga Ołtarza Inicjacji); Elita: E1→E2 (wymaga Koszar Specjalnych).
    public bool TrainSoldiers { get; set; } = false;
    public bool TrainElite { get; set; } = false;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Coalition? Coalition { get; set; }
    public Era Era { get; set; } = null!;
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<MilitaryUnit> MilitaryUnits { get; set; } = new List<MilitaryUnit>();
    public ICollection<Profession> Professions { get; set; } = new List<Profession>();
    public ICollection<Research> Researches { get; set; } = new List<Research>();
    public ICollection<ActiveSpell> ActiveSpells { get; set; } = new List<ActiveSpell>();
}
