$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProject = Join-Path $root "Raceday part 2.csproj"
$mvc = Join-Path $root "RaceDay.Mvc"
$mvcProject = Join-Path $mvc "RaceDay.Mvc.csproj"

if (-not (Test-Path $apiProject)) {
    throw "API project not found at: $apiProject"
}
if (-not (Test-Path $mvcProject)) {
    throw "MVC project not found at: $mvcProject"
}

Write-Host "Starting RaceDay API on http://localhost:5099 ..." -ForegroundColor Cyan
Start-Process -FilePath "powershell.exe" -WorkingDirectory $root -ArgumentList @(
    "-NoExit", "-Command", "dotnet run --project `"$apiProject`" --launch-profile http"
)

Start-Sleep -Seconds 3

Write-Host "Starting RaceDay MVC on http://localhost:5080 ..." -ForegroundColor Cyan
Start-Process -FilePath "powershell.exe" -WorkingDirectory $mvc -ArgumentList @(
    "-NoExit", "-Command", "dotnet run --project `"$mvcProject`" --launch-profile http"
)

Write-Host "API Swagger: http://localhost:5099/swagger" -ForegroundColor Green
Write-Host "MVC website: http://localhost:5080" -ForegroundColor Green
Write-Host "Keep both terminal windows open. Stop each with Ctrl+C." -ForegroundColor Yellow
