namespace RedDragonAPI.Models.DTOs;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static ServiceResult Ok(string? message = null) => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public new static ServiceResult<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

public class ResourcesStolen
{
    public long Gold { get; set; }
    public long Food { get; set; }
    public long Stone { get; set; }
    public long Weapons { get; set; }
}

public class TurnResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int TurnsRemaining { get; set; }
    public Dictionary<string, long> Deltas { get; set; } = new();
}

public class MilitaryAttackData
{
    public Dictionary<string, int> Units { get; set; } = new();
}

public class BattleResult
{
    public bool Success { get; set; }
    public bool AttackerWins { get; set; }
    public int LandCaptured { get; set; }
    public ResourcesStolen? ResourcesStolen { get; set; }
}
