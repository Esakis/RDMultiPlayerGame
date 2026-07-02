using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Helpers;

/// <summary>Reguły opłat za księstwa (docs: pierwsze darmowe, imperatorskie zawsze darmowe, 20/30 dni).</summary>
public static class PaymentRules
{
    public static async Task<decimal> GetKingdomPriceAsync(ApplicationDbContext context)
    {
        var setting = await context.GameSettings
            .FirstOrDefaultAsync(s => s.Key == GameSetting.KingdomPriceKey);
        return setting != null && decimal.TryParse(setting.Value, out var price)
            ? price
            : GameSetting.DefaultKingdomPrice;
    }

    public static AccountKingdomDto ToAccountDto(Kingdom k, int? activeKingdomId)
    {
        bool imperial = k.CoalitionRole == "Imperator";
        bool exempt = k.IsFree || k.IsPaid || imperial;
        int days = k.DaysSinceCreation;

        string status = k.AdminLocked ? "Zablokowane"
            : imperial ? "Imperatorskie"
            : k.IsFree ? "Darmowe"
            : k.IsPaid ? "Opłacone"
            : (k.IsSuspended || days >= Kingdom.PaymentDeadlineDays) ? "Zawieszone"
            : "Do opłaty";

        return new AccountKingdomDto
        {
            Id = k.Id,
            Name = k.Name,
            Race = k.Race,
            Land = k.Land,
            Age = k.Age,
            IsActive = k.Id == activeKingdomId,
            IsFree = k.IsFree,
            IsPaid = k.IsPaid,
            IsImperial = imperial,
            IsSuspended = k.IsSuspended || k.AdminLocked || (!exempt && days >= Kingdom.PaymentDeadlineDays),
            RequiresPayment = !exempt,
            DaysSinceCreation = days,
            DaysToSuspension = exempt || days >= Kingdom.PaymentDeadlineDays
                ? null : Kingdom.PaymentDeadlineDays - days,
            DaysToDeletion = exempt ? null : Math.Max(0, Kingdom.DeletionDays - days),
            Status = status
        };
    }
}
