namespace APO_BOT.Infrastructure.Api;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 30;
}
