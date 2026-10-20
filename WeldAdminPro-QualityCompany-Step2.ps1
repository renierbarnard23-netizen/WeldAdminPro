Set-Location "C:\Users\Renie\Source\Repos\WeldAdminPro"
$ErrorActionPreference = "Stop"

$root = (Get-Location).Path
$outFile = Join-Path $env:TEMP "WeldAdminPro-QualityCompany-Step2.txt"

$files = @(
    "WeldAdminPro.Web\Components\Shared\Quality\PqrEditorDialog.razor",
    "WeldAdminPro.Web\Components\Shared\Quality\WpsEditorDialog.razor",
    "WeldAdminPro.Web\Components\Shared\Quality\WpqrEditor.razor",
    "WeldAdminPro.Web\Services\Quality\PqrApplicationService.cs",
    "WeldAdminPro.Web\Services\Quality\WpsApplicationService.cs",
    "WeldAdminPro.Web\Services\Quality\WpqrApplicationService.cs",
    "WeldAdminPro.Core\Models\Company.cs",
    "WeldAdminPro.Data\Repositories\CompanyRepository.cs"
)

$report = [System.Collections.Generic.List[string]]::new()
$report.Add("WELDADMIN PRO QUALITY COMPANY OWNERSHIP - STEP 2")
$report.Add("READ-ONLY SOURCE INSPECTION. NO SOURCE FILES MODIFIED.")
$report.Add("Repository: $root")
$report.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")

foreach ($relativePath in $files) {
    $fullPath = Join-Path $root $relativePath
    $report.Add("")
    $report.Add(("=" * 100))
    $report.Add("FILE: $relativePath")
    $report.Add(("=" * 100))

    if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
        $lines = Get-Content -LiteralPath $fullPath
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $report.Add(("{0,5}: {1}" -f ($i + 1), $lines[$i]))
        }
    } else {
        $report.Add("FILE NOT FOUND")
    }
}

$report | Set-Content -LiteralPath $outFile -Encoding UTF8
Write-Host "SOURCE INSPECTION COMPLETE" -ForegroundColor Green
Write-Host "Report: $outFile" -ForegroundColor Cyan
Get-Item -LiteralPath $outFile | Select-Object FullName, Length, LastWriteTime
notepad.exe $outFile
