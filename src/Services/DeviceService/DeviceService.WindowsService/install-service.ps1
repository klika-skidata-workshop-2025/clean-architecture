# Install DeviceService as a Windows Service
# Run this script as Administrator

$ServiceName = "DeviceService"
$DisplayName = "Device Service - Skidata Workshop"
$Description = "Manages devices (gates, lifts, counters, controls) for Skidata Workshop microservices demo"
$ExecutablePath = Join-Path $PSScriptRoot "DeviceService.WindowsService.exe"

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    exit 1
}

# Check if executable exists
if (-not (Test-Path $ExecutablePath)) {
    Write-Host "ERROR: Executable not found at: $ExecutablePath" -ForegroundColor Red
    Write-Host "Please build the project first: dotnet build -c Release" -ForegroundColor Yellow
    exit 1
}

# Stop and remove existing service if it exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Stopping existing service..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue

    Write-Host "Removing existing service..." -ForegroundColor Yellow
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
}

# Create the service
Write-Host "Installing service: $ServiceName" -ForegroundColor Cyan
sc.exe create $ServiceName binPath= $ExecutablePath start= auto DisplayName= $DisplayName

# Set description
sc.exe description $ServiceName $Description

# Start the service
Write-Host "Starting service..." -ForegroundColor Cyan
Start-Service -Name $ServiceName

# Show service status
Write-Host ""
Write-Host "Service installed successfully!" -ForegroundColor Green
Get-Service -Name $ServiceName | Format-Table -AutoSize

Write-Host ""
Write-Host "To uninstall the service, run: .\uninstall-service.ps1" -ForegroundColor Yellow
