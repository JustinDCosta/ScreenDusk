namespace ScreenDusk.App.Models;

public sealed class AppSettings
{
    public int DimLevelPercent { get; set; } = 35;

    public bool IsDimmingEnabled { get; set; } = true;

    public bool LaunchOnStartup { get; set; } = false;

    public bool MinimizeToTray { get; set; } = true;
}
