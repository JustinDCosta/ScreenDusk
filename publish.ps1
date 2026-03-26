param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "Restoring packages..."
dotnet restore

Write-Host "Publishing ScreenDusk ($Configuration)..."
dotnet publish .\ScreenDusk.App\ScreenDusk.App.csproj -c $Configuration -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true

Write-Host "Publish complete. Output folder:"
Write-Host ".\ScreenDusk.App\bin\$Configuration\net8.0-windows\win-x64\publish"
