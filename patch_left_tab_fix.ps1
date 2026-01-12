# patch_left_tab_fix.ps1
# Usage: run this from the solution root (where the WeldAdminPro.UI folder is)
# Example: powershell -ExecutionPolicy Bypass -File .\patch_left_tab_fix.ps1

$path = ".\WeldAdminPro.UI\App.xaml"
if (-not (Test-Path $path)) {
    Write-Error "File not found: $path. Run this script from the solution root where WeldAdminPro.UI exists."
    exit 2
}

$bak  = "$path.bak.$((Get-Date).ToString('yyyyMMddHHmmss'))"
Copy-Item -LiteralPath $path -Destination $bak -Force

$content = Get-Content -LiteralPath $path -Raw -ErrorAction Stop

# Style + DataTemplate to force readable headers and provide a simple left-tab header template fallback
$style = @'
  <!-- Diagnostic / fix: force TabItem header readable and provide a safe header template -->
  <Style TargetType="TabItem">
    <Setter Property="Foreground" Value="Black"/>
    <Setter Property="Background" Value="Transparent"/>
  </Style>
  <DataTemplate x:Key="LeftTabHeaderTemplate">
    <TextBlock Text="{Binding}" Foreground="Black" FontWeight="Bold" TextTrimming="CharacterEllipsis"/>
  </DataTemplate>
'@

if ($content -match '<Application\.Resources[^>]*>') {
    # Insert style right after the opening <Application.Resources> tag
    $new = $content -replace '(<Application\.Resources[^>]*>)', "`$1`r`n$style"
} else {
    # No Application.Resources section found — create one just before the closing </Application> tag
    if ($content -match '(</Application>)') {
        $new = $content -replace '(</Application>)', "  <Application.Resources>`r`n$style  </Application.Resources>`r`n`$1"
    } else {
        Write-Error "Couldn't locate <Application.Resources> or </Application> in App.xaml. Backup created at: $bak"
        exit 3
    }
}

Set-Content -LiteralPath $path -Value $new -Encoding UTF8
Write-Host "Patched $path (backup: $bak)"
Write-Host "Rebuild and run the UI (dotnet build / run or run the EXE) and check the left tabs. If you still see the green block, open the live visual tree (Snoop or VS Live Visual Tree) and tell me what element is the header (TextBlock, Rectangle, Path)."
