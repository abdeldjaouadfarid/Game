# Build & run HERETICIDE on Windows (desktop).
#   ./run.ps1          normal play
#   ./run.ps1 -Demo    attract/auto-play mode (the marine plays itself)
param([switch]$Demo)

$ErrorActionPreference = 'Stop'

function Find-Dotnet {
    $cands = @(
        "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe",
        "C:\Users\fabde\AppData\Local\Microsoft\dotnet\dotnet.exe",
        "$env:ProgramFiles\dotnet\dotnet.exe"
    )
    foreach ($c in $cands) { if ($c -and (Test-Path $c)) { return $c } }
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    return $null
}

$dotnet = Find-Dotnet
if (-not $dotnet) {
    Write-Error "Could not find dotnet.exe (expected at C:\Users\fabde\AppData\Local\Microsoft\dotnet\dotnet.exe)."
    exit 1
}

$env:DOTNET_ROOT = Split-Path $dotnet
if (-not ($env:Path -like "*$($env:DOTNET_ROOT)*")) { $env:Path = "$($env:DOTNET_ROOT);$env:Path" }
if ($Demo) { $env:HERETICIDE_AUTOPLAY = '1' }

Write-Host "Launching with: $dotnet"
& $dotnet run -c Release --project (Join-Path $PSScriptRoot 'Hereticide.csproj')
