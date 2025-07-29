# Deploy Facteur Packages Script
# This script builds the solution and deploys all packages to the NuGet source

param(
    [string]$NuGetSource = "DS",
    [string]$ApiKey = "az"
)

Write-Host "Starting Facteur package deployment..." -ForegroundColor Green

# Step 1: Navigate to the src directory
Write-Host "Navigating to src directory..." -ForegroundColor Yellow
Set-Location "src"

# Step 2: Build the solution in Release mode
Write-Host "Building solution in Release mode..." -ForegroundColor Yellow
dotnet build Facteur.sln -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Exiting..." -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green

# Step 3: Get all project directories that generate packages
$projectDirectories = @(
    "Facteur",
    "Facteur.Smtp", 
    "Facteur.SendGrid",
    "Facteur.MsGraph",
    "Facteur.Attachments.IO",
    "Facteur.Compilers.Scriban",
    "Facteur.Extensions.DependencyInjection",
    "Facteur.Resolvers.ViewModel",
    "Facteur.TemplateProviders.IO"
)

# Step 4: Deploy each package
foreach ($projectDir in $projectDirectories) {
    Write-Host "Deploying package from $projectDir..." -ForegroundColor Yellow
    
    # Get the version from the csproj file
    $csprojPath = Join-Path $projectDir "$projectDir.csproj"
    if (Test-Path $csprojPath) {
        $csprojContent = Get-Content $csprojPath -Raw
        if ($csprojContent -match '<Version>(.*?)</Version>') {
            $version = $matches[1]
            Write-Host "Found version $version in $projectDir.csproj" -ForegroundColor Cyan
        } else {
            Write-Host "No version found in $projectDir.csproj, skipping..." -ForegroundColor Yellow
            continue
        }
    } else {
        Write-Host "Project file not found: $csprojPath" -ForegroundColor Yellow
        continue
    }
    
    # Find the .nupkg file in the project's bin/Release directory
    $releasePath = Join-Path $projectDir "bin\Release"
    
    if (Test-Path $releasePath) {
        $nupkgFiles = Get-ChildItem -Path $releasePath -Filter "*.nupkg" -Recurse
        
        if ($nupkgFiles.Count -gt 0) {
            # Filter packages that match the version from csproj
            $matchingPackages = $nupkgFiles | Where-Object { $_.Name -like "*$version*" }
            
            if ($matchingPackages.Count -gt 0) {
                foreach ($nupkgFile in $matchingPackages) {
                    Write-Host "Pushing $($nupkgFile.Name) to $NuGetSource..." -ForegroundColor Cyan
                    
                    # Push the package
                    dotnet nuget push $nupkgFile.FullName -s $NuGetSource -k $ApiKey
                    
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "Successfully deployed $($nupkgFile.Name)" -ForegroundColor Green
                    } else {
                        Write-Host "Failed to deploy $($nupkgFile.Name)" -ForegroundColor Red
                    }
                }
            } else {
                Write-Host "No packages found matching version $version in $releasePath" -ForegroundColor Yellow
            }
        } else {
            Write-Host "No .nupkg files found in $releasePath" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Release directory not found: $releasePath" -ForegroundColor Yellow
    }
}

Write-Host "Package deployment completed!" -ForegroundColor Green 