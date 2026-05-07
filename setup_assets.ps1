# setup_assets.ps1 - Extract Content/ from a retail CMZ STFS file.
#
# We can't redistribute CMZ's game assets, but you can extract them from your
# own retail STFS package using this script. After running, deploy.ps1 will
# work normally.
#
# Usage:
#   .\setup_assets.ps1 -StfsPath "C:\path\to\Castle Miner Z"
#
# Prerequisites:
#   - Python 3 in PATH

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$StfsPath
)

$ErrorActionPreference = 'Stop'

$here = $PSScriptRoot
$pyScript = Join-Path $here 'stfs_extract.py'
$outDir = Join-Path $here 'cmz_extracted'

if (-not (Test-Path $StfsPath)) {
    Write-Host "[!] STFS file not found: $StfsPath" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $pyScript)) {
    Write-Host "[!] stfs_extract.py missing - did the bundle extract correctly?" -ForegroundColor Red
    exit 1
}

# Verify Python is available
try {
    $null = & python --version 2>&1
} catch {
    Write-Host "[!] Python 3 not found in PATH. Install from python.org." -ForegroundColor Red
    exit 1
}

if (Test-Path $outDir) {
    Write-Host "[!] cmz_extracted/ already exists. Delete it to re-extract." -ForegroundColor Yellow
    exit 0
}

Write-Host "[+] Extracting $StfsPath to cmz_extracted/" -ForegroundColor Cyan
& python $pyScript $StfsPath $outDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "[!] Extraction failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "[+] Done. Next: run .\deploy.ps1" -ForegroundColor Green
