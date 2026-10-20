$ErrorActionPreference = 'Stop'
$RootPath = 'C:\Users\Renie\Source\Repos\WeldAdminPro'
$RepositoryPath = Join-Path $RootPath 'WeldAdminPro.Data\Repositories\WelderQualificationRepository.cs'
$SolutionPath = Join-Path $RootPath 'WeldAdminPro.sln'
if (!(Test-Path -LiteralPath $RepositoryPath)) { throw "Repository file not found: $RepositoryPath" }
if (!(Test-Path -LiteralPath $SolutionPath)) { throw "Solution file not found: $SolutionPath" }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupDir = Join-Path $RootPath "Temp\WelderQualification_IsActive_Final_$stamp"
$backup = Join-Path $backupDir 'WelderQualificationRepository.cs'
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
Copy-Item -LiteralPath $RepositoryPath -Destination $backup -Force
$text = Get-Content -LiteralPath $RepositoryPath -Raw
$original = $text
function Replace-Required([string]$name,[string]$pattern,[string]$replacement,[int]$expected=1) {
  $count = ([regex]::Matches($script:text,$pattern)).Count
  if ($count -ne $expected) { throw "$name - expected $expected match(es), found $count." }
  $script:text = [regex]::Replace($script:text,$pattern,$replacement)
  Write-Host "Applied: $name" -ForegroundColor Green
}
$nl = [Environment]::NewLine
Replace-Required 'INSERT columns' '(?m)(RenewalDate,\s*\r?\n\s*ExpiryDate)(\s*\r?\n\s*\))' ('$1,'+$nl+'                    IsActive$2')
Replace-Required 'INSERT values' '(?m)(\$renewalDate,\s*\r?\n\s*\$expDate)(\s*\r?\n\s*\))' ('$1,'+$nl+'                    $isActive$2')
Replace-Required 'INSERT parameter' '(?m)(cmd\.Parameters\.AddWithValue\("\$expDate"[^\r\n]+\);)' ('$1'+$nl+'            cmd.Parameters.AddWithValue("$isActive", wq.IsActive ? 1 : 0);')
Replace-Required 'qualification mappings' '(?m)(CompanyId\s*=\s*\r?\n\s*reader\["CompanyId"\]\s*==\s*DBNull\.Value\s*\r?\n\s*\?\s*null\s*\r?\n\s*:\s*Guid\.Parse\(reader\["CompanyId"\]\.ToString\(\)!\),)' ('$1'+$nl+$nl+'                    IsActive ='+$nl+'                        reader["IsActive"] != DBNull.Value &&'+$nl+'                        Convert.ToInt32(reader["IsActive"]) == 1,') 2
Replace-Required 'UPDATE statement' '(?m)(ExpiryDate\s*=\s*\$expiryDate)(\s*\r?\n\s*WHERE\s+Id\s*=\s*\$id)' ('$1,'+$nl+'                IsActive = $isActive$2')
Replace-Required 'UPDATE parameter' '(?m)(cmd\.Parameters\.AddWithValue\("\$expiryDate"[^\r\n]+\);)' ('$1'+$nl+'            cmd.Parameters.AddWithValue("$isActive", wq.IsActive ? 1 : 0);')
Replace-Required 'HasValidQualification filter' '(?m)(x\.WelderNumber\s*==\s*welderNumber\s*&&\s*\r?\n\s*)(x\.ExpiryDate\s*>=\s*DateTime\.Today)' ('$1x.IsActive &&'+$nl+'                    $2')
Replace-Required 'HasValidProcessQualification filter' '(?m)(x\.WelderNumber\s*==\s*welderNumber\s*&&\s*\r?\n\s*x\.Process\s*==\s*process\s*&&\s*\r?\n\s*)(x\.ExpiryDate\s*>=\s*DateTime\.Today)' ('$1x.IsActive &&'+$nl+'                $2')
if ($text -eq $original) { throw 'No changes detected.' }
$mappingCount = ([regex]::Matches($text, 'reader\["IsActive"\]')).Count
Write-Host "Diagnostic: IsActive mapping count = $mappingCount" -ForegroundColor Yellow

$mappingAssignmentCount = ([regex]::Matches($text, '(?m)^\s*IsActive\s*=\s*$')).Count

if ($mappingAssignmentCount -ne 2) {
    throw "Validation failed: expected 2 IsActive mapping assignments, found $mappingAssignmentCount."
}
if ($text -notmatch 'IsActive\s*=\s*\$isActive') { throw 'Validation failed: UPDATE IsActive assignment missing.' }
Set-Content -LiteralPath $RepositoryPath -Value $text -Encoding UTF8
Write-Host "Source updated. Backup: $backup" -ForegroundColor Green
$build = & dotnet build $SolutionPath --no-restore 2>&1
$code = $LASTEXITCODE
$build | ForEach-Object { Write-Host $_ }
if ($code -ne 0) { Copy-Item -LiteralPath $backup -Destination $RepositoryPath -Force; throw "Build failed ($code). Backup restored." }
Write-Host 'PATCH AND BUILD COMPLETED SUCCESSFULLY' -ForegroundColor Green
Read-Host 'Press ENTER to close'




