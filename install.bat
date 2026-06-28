@echo off
setlocal EnableExtensions
set "ERROR_STAGE="
set "ERROR_CODE="
set "ERROR_MESSAGE="

set "SCRIPT_DIR=%~dp0"
set "PROJECT_FILE=%SCRIPT_DIR%NEF-Filter.csproj"
set "PUBLISH_DIR=%SCRIPT_DIR%dist\publish"
set "INSTALL_DIR=%LOCALAPPDATA%\neff"
set "LAUNCHER_PATH=%INSTALL_DIR%\neff.cmd"
set "LAUNCHER_TEMP=%INSTALL_DIR%\neff.cmd.tmp"

echo Installing NEF-Filter...

if exist "%PROJECT_FILE%" (
  echo Building release package...
  dotnet publish "%PROJECT_FILE%" -c Release -o "%PUBLISH_DIR%"
  if errorlevel 1 (
    set "ERROR_STAGE=dotnet publish"
    set "ERROR_CODE=%ERRORLEVEL%"
    set "ERROR_MESSAGE=Failed to publish the application."
    goto :fail
  )
  set "SOURCE_DIR=%PUBLISH_DIR%"
) else (
  if exist "%SCRIPT_DIR%NEF-Filter.exe" (
    echo Installing from published package...
    set "SOURCE_DIR=%SCRIPT_DIR%"
  ) else (
    set "ERROR_STAGE=source detection"
    set "ERROR_CODE=1"
    set "ERROR_MESSAGE=Could not find NEF-Filter.csproj or NEF-Filter.exe."
    goto :fail
  )
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "try {" ^
  "  $source = '%SOURCE_DIR%';" ^
  "  $destination = '%INSTALL_DIR%';" ^
  "  New-Item -ItemType Directory -Force -Path $destination | Out-Null;" ^
  "  Copy-Item -Path (Join-Path $source '*') -Destination $destination -Recurse -Force -ErrorAction Stop;" ^
  "} catch {" ^
  "  Write-Host ('Copy failed: ' + $_.Exception.Message);" ^
  "  exit 1" ^
  "}"
if errorlevel 1 (
  set "ERROR_STAGE=copy"
  set "ERROR_CODE=%ERRORLEVEL%"
  set "ERROR_MESSAGE=Failed to copy program files."
  goto :fail
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$content = '@echo off`r`n\"%~dp0NEF-Filter.exe\" %*`r`n';" ^
  "[System.IO.File]::WriteAllText('%LAUNCHER_TEMP%', $content, [System.Text.Encoding]::ASCII)"
if errorlevel 1 (
  set "ERROR_STAGE=launcher creation"
  set "ERROR_CODE=%ERRORLEVEL%"
  set "ERROR_MESSAGE=Failed to create the launcher script."
  goto :fail
)

move /Y "%LAUNCHER_TEMP%" "%LAUNCHER_PATH%" >nul
if errorlevel 1 (
  set "ERROR_STAGE=launcher move"
  set "ERROR_CODE=%ERRORLEVEL%"
  set "ERROR_MESSAGE=Failed to place the launcher script."
  goto :fail
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$dir = '%INSTALL_DIR%'.TrimEnd('\');" ^
  "$current = [Environment]::GetEnvironmentVariable('Path', 'User');" ^
  "$entries = @();" ^
  "if (-not [string]::IsNullOrWhiteSpace($current)) { $entries = $current -split ';' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } }" ^
  "$normalized = $entries | ForEach-Object { $_.TrimEnd('\') };" ^
  "if ($normalized -notcontains $dir) {" ^
  "  $updated = if ([string]::IsNullOrWhiteSpace($current)) { $dir } else { $current.TrimEnd(';') + ';' + $dir };" ^
  "  [Environment]::SetEnvironmentVariable('Path', $updated, 'User')" ^
  "}"
if errorlevel 1 (
  set "ERROR_STAGE=PATH update"
  set "ERROR_CODE=%ERRORLEVEL%"
  set "ERROR_MESSAGE=Failed to update the user PATH."
  goto :fail
)

echo Installed to %INSTALL_DIR%.
echo Added the install directory to the current user's PATH when needed.
echo Open a new terminal and run:
echo   neff ^<path^>
goto :done

:fail
echo Installation failed.
if not "%ERROR_STAGE%"=="" echo Stage: %ERROR_STAGE%
if not "%ERROR_CODE%"=="" echo Error code: %ERROR_CODE%
if not "%ERROR_MESSAGE%"=="" echo Reason: %ERROR_MESSAGE%
pause
exit /b 1

:done
echo Installation complete.
pause
exit /b 0
