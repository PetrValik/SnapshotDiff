#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds release packages for SnapshotDiff locally.
.DESCRIPTION
    Builds Windows (MAUI) and Linux (Photino) packages.
    Outputs to ./dist/ folder.
.PARAMETER Version
    Version string, e.g. "1.2.0". Defaults to "0.0.0-local".
.PARAMETER Target
    Which targets to build: "all" (default), "windows", "linux".
.EXAMPLE
    .\build-release.ps1 -Version 1.0.0
    .\build-release.ps1 -Version 1.0.0 -Target windows
#>
param(
    [string]$Version = "0.0.0-local",
    [ValidateSet("all","windows","linux")]
    [string]$Target = "all"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$dist = Join-Path $root "dist"

Write-Host "=== SnapshotDiff Release Build ===" -ForegroundColor Cyan
Write-Host "Version : $Version"
Write-Host "Target  : $Target"
Write-Host "Output  : $dist"
Write-Host ""

# Clean dist
if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item $dist -ItemType Directory | Out-Null

function Build-Windows {
    Write-Host "--- Building Windows (MAUI) ---" -ForegroundColor Yellow

    $out = Join-Path $dist "windows"
    dotnet publish "$root/SnapshotDiff.MAUI/SnapshotDiff.MAUI.csproj" `
        -f net10.0-windows10.0.19041.0 `
        -c Release `
        -r win-x64 `
        --self-contained true `
        "/p:ApplicationVersion=$Version" `
        "/p:ApplicationDisplayVersion=$Version" `
        -o $out

    if ($LASTEXITCODE -ne 0) { throw "Windows build failed" }

    $zip = Join-Path $dist "SnapshotDiff-$Version-windows-x64.zip"
    Compress-Archive -Path "$out\*" -DestinationPath $zip -Force
    Remove-Item $out -Recurse -Force
    Write-Host "  -> $zip" -ForegroundColor Green
}

function Build-Linux {
    Write-Host "--- Building Linux (Photino) ---" -ForegroundColor Yellow

    $out = Join-Path $dist "linux"
    dotnet publish "$root/SnapshotDiff.Linux/SnapshotDiff.Linux.csproj" `
        -c Release `
        -r linux-x64 `
        --self-contained true `
        -o $out

    if ($LASTEXITCODE -ne 0) { throw "Linux build failed" }

    $tar = Join-Path $dist "SnapshotDiff-$Version-linux-x64.tar.gz"
    tar -czf $tar -C $out .
    Remove-Item $out -Recurse -Force
    Write-Host "  -> $tar" -ForegroundColor Green
}

# Run tests first
Write-Host "--- Running tests ---" -ForegroundColor Yellow
dotnet test "$root/SnapshotDiff.Tests/SnapshotDiff.Tests.csproj" -c Release --no-restore -v quiet
if ($LASTEXITCODE -ne 0) { throw "Tests failed — aborting release build" }
Write-Host "  All tests passed." -ForegroundColor Green
Write-Host ""

if ($Target -eq "all" -or $Target -eq "windows") { Build-Windows }
if ($Target -eq "all" -or $Target -eq "linux")   { Build-Linux }

Write-Host ""
Write-Host "=== Build complete ===" -ForegroundColor Cyan
Write-Host "Packages in: $dist"
Get-ChildItem $dist -Filter "SnapshotDiff-*"
