# Uninstall DeviceService Windows Service
# Run this script as Administrator

$ServiceName = "DeviceService"

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    exit 1
}

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $existingService) {
    Write-Host "Service '$ServiceName' is not installed." -ForegroundColor Yellow
    exit 0
}

# Stop the service
Write-Host "Stopping service: $ServiceName" -ForegroundColor Yellow
Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Remove the service
Write-Host "Removing service: $ServiceName" -ForegroundColor Yellow
sc.exe delete $ServiceName

Write-Host ""
Write-Host "Service uninstalled successfully!" -ForegroundColor Green
