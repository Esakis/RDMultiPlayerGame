using System.ComponentModel.DataAnnotations;

namespace RedDragonAPI.Models.DTOs;

/// <summary>Księstwo na liście konta wraz ze statusem opłaty.</summary>
public class AccountKingdomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public int Land { get; set; }
    public int Age { get; set; }

    /// <summary>Czy to księstwo jest aktualnie wybrane (grane).</summary>
    public bool IsActive { get; set; }

    public bool IsFree { get; set; }
    public bool IsPaid { get; set; }
    public bool IsImperial { get; set; }
    public bool IsSuspended { get; set; }

    /// <summary>Czy księstwo wymaga jeszcze opłacenia.</summary>
    public bool RequiresPayment { get; set; }

    public int DaysSinceCreation { get; set; }

    /// <summary>Dni do zawieszenia (null, gdy nie dotyczy).</summary>
    public int? DaysToSuspension { get; set; }

    /// <summary>Dni do trwałego usunięcia (null, gdy nie dotyczy).</summary>
    public int? DaysToDeletion { get; set; }

    /// <summary>Darmowe | Opłacone | Imperatorskie | Do opłaty | Zawieszone.</summary>
    public string Status { get; set; } = string.Empty;
}

public class CreateKingdomDto
{
    [Required, MinLength(3), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Race { get; set; } = "Człowiek";
}

public class PayForKingdomDto
{
    [Required]
    public int KingdomId { get; set; }

    /// <summary>BLIK | Karta | Przelew.</summary>
    [Required, MaxLength(50)]
    public string Method { get; set; } = string.Empty;
}

public class PaymentDto
{
    public int Id { get; set; }
    public int KingdomId { get; set; }
    public string KingdomName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Username { get; set; }
}

public class KingdomPriceDto
{
    public decimal Price { get; set; }
}
