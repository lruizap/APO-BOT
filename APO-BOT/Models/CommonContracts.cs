namespace APO_BOT.Models;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class ApiProblem
{
    public string? Type { get; init; }
    public string? Title { get; init; }
    public int? Status { get; init; }
    public string? Detail { get; init; }
    public string? Code { get; init; }
    public IReadOnlyDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}

public enum PriorityMode
{
    Dispense,
    Load,
    Optimize
}

public enum DispensationPriority
{
    High,
    Medium,
    Low
}

public enum MachineAction
{
    Pause,
    PowerOff
}

public enum LoadCommand
{
    Start,
    Pause,
    Stop,
    StartShelf,
    PauseShelf,
    StopShelf
}

public enum HistoryType
{
    Dispensation,
    Load,
    Capacity,
    Article
}

public enum StatisticsPeriod
{
    Days,
    Week,
    Month
}
