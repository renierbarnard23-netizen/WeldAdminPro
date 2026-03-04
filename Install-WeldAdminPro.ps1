$installPath = "C:\Program Files\WeldAdminPro"
$dataPath = "$env:LOCALAPPDATA\WeldAdminPro"
$publishPath = ".\publish"

Write-Host "Installing WeldAdminPro..."

# Create install folder
New-Item -ItemType Directory -Force -Path $installPath

# Ensure data folder exists
if (!(Test-Path $dataPath)) {
    New-Item -ItemType Directory -Path $dataPath
}

# Copy program files
Copy-Item "$publishPath\*" $installPath -Recurse -Force

# Create desktop shortcut
$ws = New-Object -ComObject WScript.Shell
$shortcut = $ws.CreateShortcut("$env:PUBLIC\Desktop\WeldAdminPro.lnk")
$shortcut.TargetPath = "$installPath\WeldAdminPro.UI.exe"
$shortcut.Save()

Write-Host "Installation Complete"
Pause