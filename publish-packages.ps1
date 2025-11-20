#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Publishes all NuGet packages from bin/Release folders to the DS source.

.DESCRIPTION
    This script searches for all .nupkg files in bin/Release folders and publishes them
    using dotnet push with the DS source and az key.

.EXAMPLE
    .\publish-packages.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# Get the script directory (project root)
$projectRoot = $PSScriptRoot

Write-Host "Searching for NuGet packages in bin/Release folders..." -ForegroundColor Cyan

# Find all .nupkg files in bin/Release folders, excluding symbols packages
$allPackages = Get-ChildItem -Path $projectRoot -Filter "*.nupkg" -Recurse | 
    Where-Object { 
        $_.FullName -match "bin\\Release" -and 
        $_.Name -notlike "*.symbols.nupkg" 
    }

# Group by directory and select only the most recent package from each folder
$packages = $allPackages | 
    Group-Object -Property DirectoryName | 
    ForEach-Object { 
        $_.Group | Sort-Object -Property LastWriteTime -Descending | Select-Object -First 1 
    }

if ($packages.Count -eq 0) {
    Write-Host "No NuGet packages found in bin/Release folders." -ForegroundColor Yellow
    Write-Host "Make sure to build the solution in Release configuration first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Found $($packages.Count) package(s) to publish:" -ForegroundColor Green
foreach ($package in $packages) {
    Write-Host "  - $($package.Name)" -ForegroundColor Gray
}

Write-Host ""

# Publish each package
$successCount = 0
$failCount = 0

foreach ($package in $packages) {
    Write-Host "Publishing $($package.Name)..." -ForegroundColor Cyan
    
    try {
        # Execute dotnet push
        dotnet nuget push -s DS -k az $package.FullName 
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Successfully published $($package.Name)" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "  ✗ Failed to publish $($package.Name) (exit code: $LASTEXITCODE)" -ForegroundColor Red
            $failCount++
        }
    } catch {
        Write-Host "  ✗ Error publishing $($package.Name): $_" -ForegroundColor Red
        $failCount++
    }
    
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Publication Summary:" -ForegroundColor Cyan
Write-Host "  Success: $successCount" -ForegroundColor Green
Write-Host "  Failed:  $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Gray" })
Write-Host "========================================" -ForegroundColor Cyan

if ($failCount -gt 0) {
    exit 1
}

