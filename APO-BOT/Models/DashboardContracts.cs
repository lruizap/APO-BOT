namespace APO_BOT.Models;

public sealed class SystemContextDto
{
    public string PharmacyName { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public DateTimeOffset ServerTime { get; init; }
    public bool LightOn { get; init; }
    public int AlertCount { get; init; }
}

public sealed class DashboardDto
{
    public StorageCapacityDto Capacity { get; init; } = new();
    public IReadOnlyList<MovementDto> RecentEntries { get; init; } = [];
    public IReadOnlyList<DispensationSummaryDto> RecentDispensations { get; init; } = [];
    public IReadOnlyList<AlertDto> Alerts { get; init; } = [];
    public PriorityMode PriorityMode { get; init; }
    public IReadOnlyList<CameraFeedDto> Cameras { get; init; } = [];
}

public sealed class StorageCapacityDto
{
    public decimal Percentage { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? StatusDetail { get; init; }
}

public sealed class MovementDto
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

public sealed class DispensationSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public DateTimeOffset DispensedAt { get; init; }
    public string OutputName { get; init; } = string.Empty;
}

public sealed class AlertDto
{
    public string Id { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string Severity { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public bool Resolved { get; init; }
}

public sealed class CameraFeedDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string StreamUrl { get; init; } = string.Empty;
    public bool Available { get; init; }
}

public sealed class SetLightRequest
{
    public bool Enabled { get; init; }
}

public sealed class SetPriorityModeRequest
{
    public PriorityMode Mode { get; init; }
}

public sealed class MachineActionRequest
{
    public MachineAction Action { get; init; }
}
