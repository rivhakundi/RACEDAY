$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$api = Join-Path $root "Raceday part 2"
$mvc = Join-Path $root "RaceDay.Mvc"

if (-not (Test-Path (Join-Path $api "Raceday part 2.csproj"))) {
    throw "API project not found at: $api"
}
if (-not (Test-Path (Join-Path $mvc "RaceDay.Mvc.csproj"))) {
    throw "MVC project not found at: $mvc"
}

Start-Process -FilePath "powershell.exe" -WorkingDirectory $api -ArgumentList @(
    "-NoExit", "-Command", "dotnet run --launch-profile http"
)
Start-Sleep -Seconds 2
Start-Process -FilePath "powershell.exe" -WorkingDirectory $mvc -ArgumentList @(
    "-NoExit", "-Command", "dotnet run --launch-profile http"
)

Write-Host "Started RaceDay API and MVC in separate terminal windows." -ForegroundColor Green
Write-Host "API Swagger: http://localhost:5099/swagger"
Write-Host "MVC website: http://localhost:5080"
