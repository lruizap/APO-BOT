namespace APO_BOT.Models;

public sealed class LoadConfigurationDto
{
    public IReadOnlyList<LoadWindowDto> Windows { get; init; } = [];
    public IReadOnlyList<LoadShelfDto> Shelves { get; init; } = [];
}

public sealed class LoadWindowDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Available { get; init; }
}

public sealed class LoadShelfDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool Available { get; init; }
}

public sealed class StartLoadRequest
{
    public string DeliveryNote { get; init; } = string.Empty;
    public string Supplier { get; init; } = string.Empty;
    public string DeliveryNoteCode { get; init; } = string.Empty;
    public string WindowId { get; init; } = string.Empty;
    public string ShelfId { get; init; } = string.Empty;
}

public sealed class LoadCommandRequest
{
    public LoadCommand Command { get; init; }
    public string? ShelfId { get; init; }
}

public sealed class LoadSessionDto
{
    public string Id { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<LoadItemDto> LoadedItems { get; init; } = [];
    public IReadOnlyList<LoadItemDto> RejectedItems { get; init; } = [];
}

public sealed class LoadItemDto
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
    public string Status { get; init; } = string.Empty;
    public string? RejectionCause { get; init; }
}
