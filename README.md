# ScreenDusk

ScreenDusk is a modern, lightweight Windows desktop app that dims your display using a software overlay, letting you go darker than your monitor's built-in brightness controls.

## Why This Tech Stack

This project uses **C# + WPF (.NET 8)** because it is:

- Native on Windows, with low overhead compared to browser-based stacks.
- Great for custom transparent windows and always-on-top overlays.
- Easy to maintain for beginner-to-intermediate developers.

## Features

- Software-based dimming overlay (not hardware brightness).
- Real-time dimming slider (0% to 100%) with instant updates.
- Optional enable/disable dimming toggle.
- Always-on-top, click-through overlay that does not block input.
- Multi-monitor support (one overlay per connected screen).
- System tray integration with reopen/exit actions.
- Minimize-to-tray behavior.
- Startup on boot option.
- Global hotkeys:
  - `Ctrl + Alt + Up`: increase dimming
  - `Ctrl + Alt + Down`: decrease dimming
  - `Ctrl + Alt + D`: toggle dimming
- Persistent user settings (stored in `%LOCALAPPDATA%/ScreenDusk/settings.json`).

## Folder Structure

```text
ScreenDusk/
  ScreenDusk.sln
  .gitignore
  README.md
  ScreenDusk.App/
    ScreenDusk.App.csproj
    App.xaml
    App.xaml.cs
    MainWindow.xaml
    MainWindow.xaml.cs
    Models/
      AppSettings.cs
    Infrastructure/
      NativeMethods.cs
    Services/
      DimmingOverlayManager.cs
      GlobalHotkeyService.cs
      OverlayWindow.cs
      SettingsService.cs
      StartupService.cs
      TrayService.cs
    Themes/
      Colors.xaml
    Resources/
```

## Build and Run

### Prerequisites

1. Windows 10/11
2. .NET 8 SDK
3. Visual Studio 2022 (or VS Code with C# tooling)

### Steps

1. Open a terminal in the project root.
2. Restore packages:
   ```powershell
   dotnet restore
   ```
3. Build:
   ```powershell
   dotnet build -c Release
   ```
4. Run:
   ```powershell
   dotnet run --project .\ScreenDusk.App\ScreenDusk.App.csproj
   ```

## Usage

1. Launch ScreenDusk.
2. Drag the slider to set dimming intensity.
3. Toggle **Enable dimming** to quickly turn the overlay on/off.
4. Optionally enable startup and tray behavior.
5. Use global hotkeys for quick adjustments.

## How Dimming Works

ScreenDusk creates a borderless, transparent-enabled black window per monitor, sized exactly to each display.

- The overlay windows are always on top.
- Win32 extended styles make them **click-through** (`WS_EX_TRANSPARENT`) and hidden from Alt+Tab (`WS_EX_TOOLWINDOW`).
- Dimming is controlled by setting each overlay window's opacity in real time.

Because this is a software overlay, it works independently from monitor hardware brightness controls.

## Security Notes

- No external network calls.
- No hardcoded secrets or credentials.
- Uses only local settings storage and standard Windows APIs.

## Screenshots

> Placeholder: add screenshots here.

- Main window UI
- Tray icon and context menu
- Multi-monitor dimming example

## License

Choose a license before publishing publicly (for example, MIT).
