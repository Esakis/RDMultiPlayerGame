namespace RedDragonAPI.Models.DTOs;

public class LabyrinthGeneralDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string PrimaryTrait { get; set; } = string.Empty;
    public string SecondaryTrait { get; set; } = string.Empty;
}

/// <summary>Typ skarbu możliwy do wyniesienia z labiryntu (kosztuje 2 pkt akcji).</summary>
public class LabyrinthTreasureDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Czy przy braniu tego skarbu generał może zostać ranny/zginąć.</summary>
    public bool RiskyForGeneral { get; set; }
}

public class LabyrinthStatusDto
{
    /// <summary>Pozostałe punkty akcji w bieżącym przeliczeniu.</summary>
    public int ActionPoints { get; set; }
    /// <summary>Maks. punktów akcji na przeliczenie (2, lub 4 z Sanktuarium Stwórcy).</summary>
    public int MaxActionPoints { get; set; }
    public int TreasureCost { get; set; }
    public int GeneralActionCost { get; set; }
    /// <summary>Czy księstwo ma Zajazd u Czerwonego Smoka (×2 wejścia do labiryntu).</summary>
    public bool HasDoubleEntry { get; set; }

    /// <summary>Tury wykorzystane w tym przeliczeniu (skarb dostępny dopiero po 5.).</summary>
    public int TurnsUsedThisRecount { get; set; }
    public int TurnsRequiredForTreasure { get; set; }
    public bool CanTakeTreasure { get; set; }

    /// <summary>Siła zaklęcia „Szczęście" (% farta w labiryncie, max 49).</summary>
    public int FortuneLevel { get; set; }

    /// <summary>Generałowie gotowi do wejścia (w domu, zdrowi).</summary>
    public List<LabyrinthGeneralDto> AvailableGenerals { get; set; } = new();

    public List<LabyrinthTreasureDto> Treasures { get; set; } = new();

    public string? LastEvent { get; set; }
}

public class TakeTreasureDto
{
    public int GeneralId { get; set; }
    public string TreasureType { get; set; } = string.Empty;
}

public class GeneralActionDto
{
    public int GeneralId { get; set; }
}
