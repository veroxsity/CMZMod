# patch_trial_mode.ps1 - Replace all Guide.IsTrialMode references with literal false
#
# Why: On RGH consoles using Bad Update + XBGuard, the LIVE license check
# can fail and Guide.IsTrialMode returns true even though the user owns the
# game. This locks the modded build into demo mode (300m travel cap, fixed
# seed, no guns/survival/creative). Since modded builds are by definition
# running on RGH, there's no legitimate reason to respect the trial check.
#
# This script edits the source files in-place. Run it once, commit the diff,
# build normally with deploy.ps1.
#
# Usage:
#   .\patch_trial_mode.ps1            # apply patches, with confirmation
#   .\patch_trial_mode.ps1 -Force     # apply without confirmation
#   .\patch_trial_mode.ps1 -DryRun    # show what would change, don't write

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$repoRoot = $PSScriptRoot

$files = @(
    'DNA Common\DNA\DNAGame.cs',
    'CastleMinerZ\DNA.CastleMinerZ.UI\MainMenu.cs',
    'CastleMinerZ\DNA.CastleMinerZ.UI\CraftingUIScreen.cs',
    'CastleMinerZ\DNA.CastleMinerZ.UI\InGameHUD.cs',
    'CastleMinerZ\DNA.CastleMinerZ.UI\InGameMenu.cs',
    'CastleMinerZ\DNA.CastleMinerZ.UI\GameModeMenu.cs',
    'CastleMinerZ\DNA.CastleMinerZ\FrontEndScreen.cs',
    'CastleMinerZ\DNA.CastleMinerZ\CastleMinerZGame.cs',
    'CastleMinerZ\DNA.CastleMinerZ\WorldInfo.cs'
)

$pattern     = 'Guide\.IsTrialMode'
$replacement = 'false /* Guide.IsTrialMode patched out for RGH */'

Write-Host "`n[+] Trial-mode patch script" -ForegroundColor Cyan
Write-Host "    Replacing 'Guide.IsTrialMode' with 'false' in $($files.Count) files`n"

if (-not $Force -and -not $DryRun) {
    $confirm = Read-Host "Continue? (y/N)"
    if ($confirm -ne 'y' -and $confirm -ne 'Y') {
        Write-Host "[!] Aborted" -ForegroundColor Yellow
        exit 0
    }
}

$totalReplacements = 0
$filesChanged = 0

foreach ($rel in $files) {
    $full = Join-Path $repoRoot $rel
    if (-not (Test-Path $full)) {
        Write-Host "    SKIP: $rel (not found)" -ForegroundColor DarkGray
        continue
    }

    $content = Get-Content $full -Raw
    $matches = [regex]::Matches($content, $pattern)

    if ($matches.Count -eq 0) {
        Write-Host "    -    $rel (already clean)" -ForegroundColor DarkGray
        continue
    }

    $newContent = [regex]::Replace($content, $pattern, $replacement)
    $totalReplacements += $matches.Count
    $filesChanged++

    if ($DryRun) {
        Write-Host "    DRY  ${rel}: would replace $($matches.Count) occurrence(s)" -ForegroundColor Yellow
    } else {
        [System.IO.File]::WriteAllText($full, $newContent, [System.Text.UTF8Encoding]::new($false))
        Write-Host "    OK   ${rel}: replaced $($matches.Count) occurrence(s)" -ForegroundColor Green
    }
}

Write-Host ""
if ($DryRun) {
    Write-Host "[!] DRY RUN: $totalReplacements replacements across $filesChanged files (no changes written)" -ForegroundColor Yellow
} else {
    Write-Host "[+] Done. $totalReplacements replacements across $filesChanged files." -ForegroundColor Green
    Write-Host "    Now run: .\deploy.ps1 -Pack" -ForegroundColor Cyan
}
