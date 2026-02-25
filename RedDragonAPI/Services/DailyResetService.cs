using Microsoft.EntityFrameworkCore;
using RedDragonAPI.Data;
using RedDragonAPI.Models.Entities;

namespace RedDragonAPI.Services;

public class DailyResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyResetService> _logger;

    public DailyResetService(IServiceProvider serviceProvider, ILogger<DailyResetService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            DateTime nextRun;

            if (now.Hour >= 5)
            {
                nextRun = DateTime.Today.AddDays(1).AddHours(5);
            }
            else
            {
                nextRun = DateTime.Today.AddHours(5);
            }

            var delay = nextRun - now;
            _logger.LogInformation("Następne przeliczenie o: {NextRun}. Czekam {Hours:F1} godzin.", nextRun, delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await PerformDailyReset();
        }
    }

    private async Task PerformDailyReset()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var battleService = scope.ServiceProvider.GetRequiredService<IBattleService>();

        _logger.LogInformation("Rozpoczynam przeliczenie codzienne...");

        try
        {
            // 1. Wykonaj zakolejkowane ataki
            var pendingActions = await context.QueuedActions
                .Where(a => a.Status == "Pending" && a.ScheduledFor <= DateTime.UtcNow)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            foreach (var action in pendingActions)
            {
                try
                {
                    switch (action.ActionType)
                    {
                        case "MilitaryAttack":
                            await battleService.ExecuteMilitaryAttackAsync(action);
                            break;
                        case "ThiefAction":
                            await battleService.ExecuteThiefActionAsync(action);
                            break;
                        case "Spell":
                            await battleService.ExecuteSpellAsync(action);
                            break;
                        case "Construction":
                            await CompleteConstruction(action, context);
                            break;
                    }

                    action.Status = "Executed";
                    action.ExecutedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd przy wykonywaniu akcji {ActionId}", action.Id);
                    action.Status = "Failed";
                }
            }

            await context.SaveChangesAsync();

            // 2. Generuj zasoby dla wszystkich księstw
            var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();
            await resourceService.GenerateResourcesForAllAsync();

            // 3. Odnów tury
            var kingdoms = await context.Kingdoms
                .Include(k => k.Buildings).ThenInclude(b => b.Definition)
                .Where(k => k.Era.IsActive)
                .ToListAsync();

            foreach (var kingdom in kingdoms)
            {
                int bonusTurns = kingdom.Buildings
                    .Where(b => !b.IsUnderConstruction && b.Quantity > 0 && b.Definition != null)
                    .Sum(b => b.Definition.BonusTurnsPerDay * b.Quantity);

                int totalTurnsToAdd = kingdom.TurnsPerDay + bonusTurns;

                kingdom.TurnsAvailable = Math.Min(
                    kingdom.TurnsAvailable + totalTurnsToAdd,
                    kingdom.MaxTurns);
            }

            // 4. Zakończ szkolenie jednostek
            var trainingUnits = await context.MilitaryUnits
                .Where(m => m.InTraining > 0 && m.TrainingCompletesAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var unit in trainingUnits)
            {
                unit.Quantity += unit.InTraining;
                unit.InTraining = 0;
                unit.TrainingCompletesAt = null;
            }

            // 5. Usuń wygasłe czary
            var expiredSpells = await context.ActiveSpells
                .Where(s => s.ExpiresAt.HasValue && s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            context.ActiveSpells.RemoveRange(expiredSpells);

            // 6. Zakończ badania
            var completedResearch = await context.Researches
                .Where(r => r.IsInProgress && r.CompletesAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var research in completedResearch)
            {
                research.IsInProgress = false;
                research.IsCompleted = true;
                research.CompletedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Przeliczenie zakończone pomyślnie.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas przeliczenia!");
        }
    }

    private async Task CompleteConstruction(QueuedAction action, ApplicationDbContext context)
    {
        var data = System.Text.Json.JsonSerializer.Deserialize<ConstructionData>(action.ActionData!);
        if (data == null) return;

        var building = await context.Buildings
            .FirstOrDefaultAsync(b => b.KingdomId == action.KingdomId && b.BuildingType == data.BuildingType);

        if (building != null)
        {
            building.Quantity += data.Quantity;
            building.IsUnderConstruction = false;
            building.ConstructionCompletesAt = null;
        }
    }

    private class ConstructionData
    {
        public string BuildingType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
