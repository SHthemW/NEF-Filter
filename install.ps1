$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $scriptRoot "NEF-Filter.csproj"
$publishDirectory = Join-Path $scriptRoot "dist\publish"
$installDirectory = Join-Path $env:LOCALAPPDATA "neff"
$launcherPath = Join-Path $installDirectory "neff.cmd"
$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
$sourceDirectory = $scriptRoot

if (Test-Path $projectFile) {
    Write-Host "正在发布程序..."
    dotnet publish $projectFile -c Release -o $publishDirectory | Out-Null
    $sourceDirectory = $publishDirectory
}
elseif (-not (Test-Path (Join-Path $scriptRoot "NEF-Filter.exe"))) {
    throw "未找到可安装的程序文件喵。请在源码仓库或发布包目录中运行此脚本。"
}
else {
    Write-Host "正在从发布包安装..."
}

New-Item -ItemType Directory -Force -Path $installDirectory | Out-Null
Copy-Item (Join-Path $sourceDirectory "*") $installDirectory -Recurse -Force

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
    Write-Host "已将 $installDirectory 添加到当前用户 PATH。"
}
else {
    Write-Host "PATH 中已包含 $installDirectory。"
}

Write-Host ""
Write-Host "安装完成。"
Write-Host "打开新的终端后可直接运行:"
Write-Host "  neff <folder>"
