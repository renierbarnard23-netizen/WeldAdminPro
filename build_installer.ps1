# build_installer.ps1
# Usage: Open PowerShell in the project folder and run:
#   .\build_installer.ps1
# Requires Inno Setup installed (ISCC.exe available in PATH) or set $isccPath to full location.

$ErrorActionPreference = "Stop"

# If ISCC is not in PATH, set full path here, e.g.:
# $isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

$issFile = Join-Path (Get-Location) "weldadmin_installer.iss"

if (-not (Test-Path $issFile)) {
    Write-Error "Could not find $issFile. Make sure weldadmin_installer.iss is in the current folder."
    exit 1
}

Write-Host "Using Inno Setup compiler:" $isccPath
Write-Host "Compiling installer from:" $issFile

# call ISCC
& $isccPath $issFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "Installer built successfully."
    Write-Host "Output file will be in the current folder (check for WeldAdminPro_Installer_v1.0.0.exe)."
} else {
    Write-Error "ISCC returned exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
