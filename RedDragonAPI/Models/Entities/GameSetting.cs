using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDragonAPI.Models.Entities;

/// <summary>
/// Globalne ustawienia gry edytowalne przez super admina (klucz → wartość),
/// np. KingdomPrice — opłata za założenie płatnego księstwa (domyślnie 30 zł).
/// </summary>
[Table("GameSettings")]
public class GameSetting
{
    public const string KingdomPriceKey = "KingdomPrice";
    public const decimal DefaultKingdomPrice = 30m;

    [Key, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Value { get; set; } = string.Empty;
}
