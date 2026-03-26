# ScreenDusk

ScreenDusk is a lightweight Windows desktop app that applies a software dimming overlay across one or more monitors.

It is designed for privacy-focused screen usage in public spaces and low-light comfort scenarios.

## Download Here

[Download Latest Release](https://github.com/JustinDCosta/ScreenDusk/releases/latest)

Download this file from the release assets:

- ScreenDusk-Windows-x64-Portable.zip

Then extract and run:

- ScreenDusk.exe

## Highlights

- Software dimming overlay (independent from hardware brightness controls)
- Real-time dimming slider (0% to 100%)
- Multi-monitor overlay support
- Click-through overlays (input is not blocked)
- Tray integration with minimize-to-tray behavior
- Optional launch at Windows startup
- Customizable global hotkeys (Ctrl + Alt + [selected key])
- Persistent settings under `%LOCALAPPDATA%/ScreenDusk/settings.json`

## Tech Stack

- .NET 8
- WPF (Windows desktop)
- Small Win32 interop layer for overlay and hotkeys

## Build

Prerequisites:

1. Windows 10/11
2. .NET 8 SDK

Commands:

```powershell
dotnet restore
dotnet build .\ScreenDusk.sln -c Release
dotnet run --project .\ScreenDusk.App\ScreenDusk.App.csproj
```

## Publish

Unsigned publish:

```powershell
.\publish.ps1
```

Local dev signing (for your own machine):

```powershell
.\publish.ps1 -SignLocal
```

Production signing with trusted certificate (.pfx):

```powershell
.\publish.ps1 -PfxPath "C:\certs\your-cert.pfx" -PfxPassword "<password>"
```

## SmartScreen Note (Important)

To distribute publicly without "unknown app" warnings, you need a trusted code-signing certificate and signing reputation.

- Local self-signed certificates only help on the local trusted machine.
- Public releases should be signed with a real CA-issued code-signing certificate.
- EV certificates generally establish trust faster for SmartScreen reputation.

## Release Process

1. Update version metadata in [ScreenDusk.App/ScreenDusk.App.csproj](ScreenDusk.App/ScreenDusk.App.csproj)
2. Update [CHANGELOG.md](CHANGELOG.md)
3. Commit and tag (example: `v3.0.0`)
4. Push tag to trigger GitHub release workflow in [.github/workflows/release.yml](.github/workflows/release.yml)

## Repository Structure

```text
ScreenDusk/
  .github/workflows/release.yml
  CHANGELOG.md
  LICENSE
  SECURITY.md
  publish.ps1
  ScreenDusk.sln
  ScreenDusk.App/
```

## Security

See [SECURITY.md](SECURITY.md) for reporting guidelines and security notes.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
