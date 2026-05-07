# deploy.ps1 - Build CMZ and stage binaries for deployment.
#
# Usage:
#   .\deploy.ps1               # Debug build (default, fast)
#   .\deploy.ps1 -Release      # Release build (smaller, optimized)
#   .\deploy.ps1 -Clean        # Wipe bin/obj before building
#   .\deploy.ps1 -Pack         # Build, stage, AND repack into STFS LIVE container
#                              # (requires stfs_pack.exe in PATH or alongside this script)
#
# Outputs the staged package to .\deploy\ ready to FTP onto the RGH at
#   Hdd1:\Content\0000000000000000\584E07D1\

[CmdletBinding()]
param(
    [switch]$Release,
    [switch]$Clean,
    [switch]$Pack
)

$ErrorActionPreference = 'Stop'

$repoRoot   = $PSScriptRoot
$msbuild    = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
$solution   = Join-Path $repoRoot 'CastleMinerZ.sln'
$deployDir  = Join-Path $repoRoot 'deploy'
$retailDir  = Join-Path $repoRoot 'cmz_extracted'
$config     = if ($Release) { 'Release' } else { 'Debug' }
$platform   = 'Xbox 360'

$dnaOutput  = Join-Path $repoRoot "DNA Common\bin\$platform\$config\DNA.Common.dll"
$cmzOutput  = Join-Path $repoRoot "CastleMinerZ\bin\$platform\$config\CastleMinerZ.exe"

function Write-Step($msg) { Write-Host "`n[+] $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "    $msg"   -ForegroundColor Green }
function Write-Bad($msg)  { Write-Host "    $msg"   -ForegroundColor Red   }

# Sanity checks
if (-not (Test-Path $msbuild))   { Write-Bad "MSBuild not found at $msbuild";   exit 1 }
if (-not (Test-Path $solution))  { Write-Bad "Solution not found at $solution"; exit 1 }
if (-not (Test-Path $retailDir)) {
    Write-Bad "Retail extraction missing at $retailDir"
    Write-Bad "Run setup_assets.ps1 first to extract Content/ from your retail STFS"
    exit 1
}

if ($Clean) {
    Write-Step "Cleaning bin/obj"
    @(
        "DNA Common\bin", "DNA Common\obj",
        "CastleMinerZ\bin", "CastleMinerZ\obj"
    ) | ForEach-Object {
        $p = Join-Path $repoRoot $_
        if (Test-Path $p) {
            Remove-Item $p -Recurse -Force
            Write-Ok "removed $_"
        }
    }
}

Write-Step "Building $config|$platform"
$buildStart = Get-Date
& $msbuild $solution /p:Configuration=$config "/p:Platform=$platform" /v:minimal /nologo /m
if ($LASTEXITCODE -ne 0) {
    Write-Bad "Build FAILED (exit $LASTEXITCODE)"
    exit $LASTEXITCODE
}
$buildDuration = ((Get-Date) - $buildStart).TotalSeconds
Write-Ok ("build succeeded in {0:N1}s" -f $buildDuration)

Write-Step "Verifying build artifacts"
if (-not (Test-Path $dnaOutput)) { Write-Bad "Missing DNA.Common.dll at $dnaOutput"; exit 1 }
if (-not (Test-Path $cmzOutput)) { Write-Bad "Missing CastleMinerZ.exe at $cmzOutput"; exit 1 }
Write-Ok ("DNA.Common.dll   $((Get-Item $dnaOutput).Length) bytes")
Write-Ok ("CastleMinerZ.exe $((Get-Item $cmzOutput).Length) bytes")

if (-not (Test-Path $deployDir)) {
    Write-Step "Seeding deploy/ from cmz_extracted/ (first run)"
    Copy-Item -Path $retailDir -Destination $deployDir -Recurse
    Write-Ok ("seeded {0} files" -f (Get-ChildItem $deployDir -Recurse -File).Count)
} else {
    Write-Step "deploy/ already exists, refreshing binaries"
}

Write-Step "Updating binaries in deploy/"
# The deploy folder mirrors the cmz_extracted/ structure: dashboard files at
# root, game files inside 584E07D1/. Our built binaries replace the ones in
# 584E07D1/.
$gameDir = Join-Path $deployDir '584E07D1'
if (-not (Test-Path $gameDir)) {
    Write-Bad "deploy/584E07D1/ folder missing - did you extract Content/ correctly?"
    exit 1
}
Copy-Item $dnaOutput (Join-Path $gameDir 'DNA.Common.dll') -Force
Copy-Item $cmzOutput (Join-Path $gameDir 'CastleMinerZ.exe') -Force
Write-Ok "DNA.Common.dll   replaced"
Write-Ok "CastleMinerZ.exe replaced"

$deployBytes = (Get-ChildItem $deployDir -Recurse -File | Measure-Object Length -Sum).Sum
$deployFiles = (Get-ChildItem $deployDir -Recurse -File).Count
Write-Step "Deploy ready"
Write-Ok ("{0:N2} MB across {1} files" -f ($deployBytes / 1MB), $deployFiles)
Write-Ok "Path: $deployDir"

if ($Pack) {
    Write-Step "Repacking into STFS LIVE container"

    # Find stfs_pack.exe - check PATH first, then common locations
    $stfsPack = $null
    $candidates = @(
        "stfs_pack.exe",
        (Join-Path $repoRoot "stfs-cli\stfs_pack.exe"),
        (Join-Path $repoRoot "..\stfs-cli\stfs_pack.exe"),
        "C:\Users\$env:USERNAME\Documents\Programming\stfs-cli\stfs_pack.exe"
    )
    foreach ($c in $candidates) {
        $r = Get-Command $c -ErrorAction SilentlyContinue
        if ($r) { $stfsPack = $r.Source; break }
        if (Test-Path $c) { $stfsPack = (Resolve-Path $c).Path; break }
    }
    if (-not $stfsPack) {
        Write-Bad "stfs_pack.exe not found. Build it from the stfs-cli/ folder."
        exit 1
    }
    Write-Ok "Using $stfsPack"

    $stfsOut = Join-Path $repoRoot 'CMZModded.stfs'
    & $stfsPack $deployDir $stfsOut `
        --title-id 584E07D2 `
        --no-wrap `
        --display-name "CMZ Modded"

    if ($LASTEXITCODE -ne 0) {
        Write-Bad "stfs_pack failed (exit $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
    Write-Ok ("packed STFS at {0:N2} MB" -f ((Get-Item $stfsOut).Length / 1MB))
    Write-Ok "Path: $stfsOut"
    Write-Host ""
    Write-Host "Next: FTP CMZModded.stfs to your RGH at:" -ForegroundColor Yellow
    Write-Host "  Hdd1:\Content\0000000000000000\584E07D1\"
} else {
    Write-Host ""
    Write-Host "Tip: pass -Pack to also repack into an STFS LIVE container" -ForegroundColor DarkGray
    Write-Host "     (requires stfs_pack.exe built from the stfs-cli folder)" -ForegroundColor DarkGray
}
