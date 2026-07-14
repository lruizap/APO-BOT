using MudBlazor;

namespace APO_BOT.Theme;

public static class ApoBotTheme
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3CB2DD",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#E1806F",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#69C2A2",
            Background = "#ECECEC",
            Surface = "#FFFFFF",
            AppbarBackground = "#3CB2DD",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#1F2144",
            DrawerText = "#FFFFFF",
            TextPrimary = "#1F2144",
            TextSecondary = "#6E6F7A",
            ActionDefault = "#1F2144",
            ActionDisabled = "#A8A9B2",
            Divider = "#D9DADF",
            Error = "#D86467",
            Warning = "#F6B683",
            Success = "#69C2A2",
            Info = "#3CB2DD"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px"
        }
    };
}
