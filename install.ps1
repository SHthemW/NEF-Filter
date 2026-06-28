$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $projectRoot "NEF-Filter.csproj"
$publishDirectory = Join-Path $projectRoot "dist\publish"
$installDirectory = Join-Path $env:LOCALAPPDATA "neff"
$launcherPath = Join-Path $installDirectory "neff.cmd"
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")

Write-Host "Publishing application..."
dotnet publish $projectFile -c Release -o $publishDirectory | Out-Null

New-Item -ItemType Directory -Force -Path $installDirectory | Out-Null
Copy-Item (Join-Path $publishDirectory "*") $installDirectory -Recurse -Force

$launcherContent = @"
@echo off
"%~dp0NEF-Filter.exe" %*
"@

Set-Content -Path $launcherPath -Value $launcherContent -Encoding ASCII

$pathEntries = @()
if (-not [string]::IsNullOrWhiteSpace($userPath)) {
    $pathEntries = $userPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)
}

if ($pathEntries -notcontains $installDirectory) {
    $updatedPath = if ([string]::IsNullOrWhiteSpace($userPath)) {
        $installDirectory
    }
    else {
        "$userPath;$installDirectory"
    }

    [Environment]::SetEnvironmentVariable("Path", $updatedPath, "User")
    Write-Host "Added $installDirectory to the current user's PATH."
}
else {
    Write-Host "PATH already contains $installDirectory."
}

Write-Host ""
Write-Host "Installation complete."
Write-Host "Open a new terminal and run:"
Write-Host "  neff <folder>"
