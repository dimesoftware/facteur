# Sample usage: .\publish.ps1 -apiKey "YOUR_NUGET_API_KEY" -version "1.0.0"
param(
    [string]$apiKey,
    [string]$version = "1.0.0"  # Default version; replace if needed
)

# Define the NuGet source
$nugetSource = "https://api.nuget.org/v3/index.json"

# Check if apiKey is provided
if (-not $apiKey) {
    Write-Host "API key is required. Please provide it as a parameter."
    exit 1
}

# Set the path to one directory up and into the 'src' folder
$searchPath = (Resolve-Path "..\src").Path

# Find all .nupkg files in the src directory and its subdirectories
Get-ChildItem -Path $searchPath -Recurse -Filter *.nupkg | Where-Object { $_.FullName -notmatch '\\bin\\Debug\\' } | ForEach-Object {
    # Extract the version from the .nupkg file name
    $nupkgPath = $_.FullName
    $nupkgFileName = $_.Name
            
    # Check if the .nupkg file contains the specified version
    if ($nupkgFileName -match "(\d+\.\d+\.\d+-[a-zA-Z0-9.-]+)\.nupkg$" -and $matches[1] -eq $version) {
        Write-Host "Pushing $nupkgPath"
        dotnet nuget push $nupkgPath --source $nugetSource --api-key $apiKey
    }
}