namespace APO_BOT.Models;

public sealed class SystemPreferencesDto
{
    public string PharmacyName { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public bool AlertSoundEnabled { get; set; }
    public int ScreenBrightness { get; set; }
    public int RefreshIntervalSeconds { get; set; }
}

public sealed class UpdateSystemPreferencesRequest
{
    public string PharmacyName { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public bool AlertSoundEnabled { get; init; }
    public int ScreenBrightness { get; init; }
    public int RefreshIntervalSeconds { get; init; }
}
