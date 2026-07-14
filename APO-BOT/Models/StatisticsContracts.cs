namespace APO_BOT.Models;

public sealed class HistoryQuery
{
    public HistoryType Type { get; set; }
    public StatisticsPeriod Period { get; set; } = StatisticsPeriod.Days;
    public string? ProductId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class HistoryResultDto
{
    public PagedResult<HistoryEntryDto> Successful { get; init; } = new();
    public PagedResult<HistoryEntryDto> Rejected { get; init; } = new();
}

public sealed class HistoryEntryDto
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public int? Shelf { get; init; }
    public int? PositionX { get; init; }
    public int? PositionY { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public string Batch { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public DateTimeOffset? OccurredAt { get; init; }
    public string? OutputName { get; init; }
    public string? RejectionCause { get; init; }
}

public sealed class CapacityStatisticsDto
{
    public IReadOnlyList<ChartPointDto> Throughput { get; init; } = [];
    public IReadOnlyList<ChartPointDto> Capacity { get; init; } = [];
}

public sealed class ChartPointDto
{
    public string Label { get; init; } = string.Empty;
    public decimal PrimaryValue { get; init; }
    public decimal SecondaryValue { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
}
