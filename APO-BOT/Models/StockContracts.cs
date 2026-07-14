namespace APO_BOT.Models;

public sealed class StockQuery
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? HasStock { get; set; }
    public int? MinimumStock { get; set; }
    public string? Observation { get; set; }
    public string SortBy { get; set; } = "description";
    public bool Descending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class ProductSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int Stock { get; init; }
    public string? Observation { get; init; }
    public bool CanOrder { get; init; }
}

public sealed class ProductDetailDto
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Supplier { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public bool IsSnsBillable { get; init; }
    public string Situation { get; init; } = string.Empty;
    public string NationalCode { get; init; } = string.Empty;
    public string Ean13 { get; init; } = string.Empty;
    public int Stock { get; init; }
}

public sealed class StockUnitDto
{
    public string Id { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public int Shelf { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public DateTimeOffset? LoadedAt { get; init; }
    public string Batch { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
}

public sealed class OutputDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Available { get; init; }
}

public sealed class CreateDispensationRequest
{
    public string ProductId { get; init; } = string.Empty;
    public string OutputId { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public DispensationPriority Priority { get; init; }
    public IReadOnlyList<string> UnitIds { get; init; } = [];
}

public sealed class DispensationResultDto
{
    public string Id { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
