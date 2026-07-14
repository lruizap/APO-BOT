namespace APO_BOT.DemoApi.Data;

public sealed class SystemSettingEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class ProductEntity
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsSnsBillable { get; set; }
    public string Situation { get; set; } = string.Empty;
    public string NationalCode { get; set; } = string.Empty;
    public string Ean13 { get; set; } = string.Empty;
    public string? Observation { get; set; }
    public bool CanOrder { get; set; }
}

public sealed class StockUnitEntity
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int Shelf { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public DateTime? LoadedAtUtc { get; set; }
    public string Batch { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}

public sealed class OutputEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Available { get; set; }
}

public sealed class MovementEntity
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

public sealed class DispensationEntity
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string OutputId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class AlertEntity
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool Resolved { get; set; }
}

public sealed class CameraEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public bool Available { get; set; }
}

public sealed class OrderEntity
{
    public string Id { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string OutputId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class OrderLineEntity
{
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int CompletedQuantity { get; set; }
    public string? Observation { get; set; }
}

public sealed class LoadWindowEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Available { get; set; }
}

public sealed class LoadShelfEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Available { get; set; }
}

public sealed class LoadSessionEntity
{
    public string Id { get; set; } = string.Empty;
    public string DeliveryNote { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string DeliveryNoteCode { get; set; } = string.Empty;
    public string WindowId { get; set; } = string.Empty;
    public string ShelfId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class LoadItemEntity
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int? Shelf { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string Batch { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Rejected { get; set; }
    public string? RejectionCause { get; set; }
}

public sealed class HistoryRecordEntity
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string UnitId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int? Shelf { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string Batch { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string? OutputName { get; set; }
    public bool Rejected { get; set; }
    public string? RejectionCause { get; set; }
}

public sealed class ChartPointEntity
{
    public int Id { get; set; }
    public string Series { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double PrimaryValue { get; set; }
    public double SecondaryValue { get; set; }
    public DateTime TimestampUtc { get; set; }
    public int SortOrder { get; set; }
}

public sealed class MachineCommandEntity
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
