param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$SignLocal,
    [string]$PfxPath,
    [string]$PfxPassword,
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"

Write-Host "Restoring packages..."
dotnet restore

Write-Host "Publishing ScreenDusk ($Configuration)..."
dotnet publish .\ScreenDusk.App\ScreenDusk.App.csproj -c $Configuration -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true

$cert = $null

if (-not [string]::IsNullOrWhiteSpace($PfxPath))
{
    if (-not (Test-Path $PfxPath))
    {
        throw "PFX file not found: $PfxPath"
    }

    if ([string]::IsNullOrWhiteSpace($PfxPassword))
    {
        throw "PfxPassword is required when PfxPath is provided."
    }

    Write-Host "Importing code-signing certificate from PFX..."
    $securePassword = ConvertTo-SecureString $PfxPassword -AsPlainText -Force
    $imported = Import-PfxCertificate -FilePath $PfxPath -CertStoreLocation "Cert:\CurrentUser\My" -Password $securePassword
    $cert = $imported | Sort-Object NotAfter -Descending | Select-Object -First 1
}

if ($SignLocal)
{
    $subject = "CN=ScreenDusk Local Dev"
    $localCert = Get-ChildItem Cert:\CurrentUser\My |
        Where-Object { $_.Subject -eq $subject -and $_.HasPrivateKey } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if ($null -eq $localCert)
    {
        Write-Host "Creating local code-signing certificate..."
        $localCert = New-SelfSignedCertificate -Type CodeSigningCert -Subject $subject -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(3)

        $cerPath = Join-Path $env:TEMP "ScreenDuskLocalDev.cer"
        Export-Certificate -Cert $localCert -FilePath $cerPath | Out-Null
        Import-Certificate -FilePath $cerPath -CertStoreLocation "Cert:\CurrentUser\TrustedPublisher" | Out-Null
        Import-Certificate -FilePath $cerPath -CertStoreLocation "Cert:\CurrentUser\Root" | Out-Null
    }

    if ($null -eq $cert)
    {
        $cert = $localCert
    }
}

if ($null -ne $cert)
{
    Write-Host "Signing executables..."

    $targets = @(
        ".\ScreenDusk.App\bin\$Configuration\net8.0-windows\ScreenDusk.exe",
        ".\ScreenDusk.App\bin\$Configuration\net8.0-windows\win-x64\ScreenDusk.exe",
        ".\ScreenDusk.App\bin\$Configuration\net8.0-windows\win-x64\publish\ScreenDusk.exe"
    )

    foreach ($target in $targets)
    {
        if (Test-Path $target)
        {
            Unblock-File -Path $target -ErrorAction SilentlyContinue

            if ([string]::IsNullOrWhiteSpace($TimestampUrl))
            {
                Set-AuthenticodeSignature -FilePath $target -Certificate $cert | Out-Null
            }
            else
            {
                Set-AuthenticodeSignature -FilePath $target -Certificate $cert -TimestampServer $TimestampUrl | Out-Null
            }

            Write-Host "Signed: $target"
        }
    }

    Write-Host "Signing complete. Thumbprint: $($cert.Thumbprint)"
}

Write-Host "Publish complete. Output folder:"
Write-Host ".\ScreenDusk.App\bin\$Configuration\net8.0-windows\win-x64\publish"
