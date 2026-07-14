namespace APO_BOT.Models;

public sealed class OrderQuery
{
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class OrderSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string RequestedBy { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;
    public string OutputName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class OrderDetailDto : OrderSummaryDto
{
    public IReadOnlyList<OrderLineDto> Lines { get; init; } = [];
}

public sealed class OrderLineDto
{
    public string Id { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public int RequestedQuantity { get; init; }
    public int CompletedQuantity { get; init; }
    public string? Observation { get; init; }
    public bool IsCompleted => CompletedQuantity >= RequestedQuantity;
}
