# Build All Projects Script (PowerShell)
# Builds the entire workshop solution

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Blue
Write-Host "Building Skidata Workshop 2025" -ForegroundColor Blue
Write-Host "========================================" -ForegroundColor Blue
Write-Host ""

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

Set-Location $RootDir

Write-Host "Step 1: Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore
Write-Host "✓ Packages restored" -ForegroundColor Green
Write-Host ""

Write-Host "Step 2: Building shared libraries..." -ForegroundColor Cyan
dotnet build src/Shared/Workshop.Common/Workshop.Common.csproj -c Release
dotnet build src/Shared/Workshop.Proto/Workshop.Proto.csproj -c Release
dotnet build src/Shared/Workshop.Messaging/Workshop.Messaging.csproj -c Release
Write-Host "✓ Shared libraries built" -ForegroundColor Green
Write-Host ""

Write-Host "Step 3: Building services..." -ForegroundColor Cyan
# Note: Uncomment when services are created
# dotnet build src/Services/DeviceService/DeviceService.sln -c Release
# dotnet build src/Services/MonitoringService/MonitoringService.sln -c Release
# dotnet build src/Services/DiagnosticsService/DiagnosticsService.sln -c Release
Write-Host "✓ Services built (or skipped if not yet created)" -ForegroundColor Green
Write-Host ""

Write-Host "Step 4: Building orchestrator..." -ForegroundColor Cyan
# Note: Uncomment when orchestrator is created
# dotnet build src/Orchestrator/Workshop.Orchestrator/Workshop.Orchestrator.sln -c Release
Write-Host "✓ Orchestrator built (or skipped if not yet created)" -ForegroundColor Green
Write-Host ""

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "  - Run tests: .\scripts\test-all.ps1"
Write-Host "  - Start services: docker-compose up"
Write-Host ""
