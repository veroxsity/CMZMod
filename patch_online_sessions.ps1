#!/usr/bin/env pwsh
# patch_online_sessions.ps1 - Make CMZ work on RGH consoles bypassing LIVE
#
# Why: CMZ has three LIVE-dependent gates that fail on RGH+XBGuard:
#
#   1. Guide.IsTrialMode returns true (license blob mismatch).
#      Fix: replace with literal `false`. Bypasses 300m cap, fixed seed,
#      no guns/survival/creative.
#
#   2. Privileges.AllowOnlineSessions returns false (profile not Gold-flagged).
#      Fix: replace with literal `true`. Bypasses "XBox Live Gold Account
#      Required" dialog when picking Host Online or Join Online.
#
#   3. NetworkSessionType.PlayerMatch fails to create session ("Hosting
#      Error" dialog). PlayerMatch requires real LIVE matchmaking which
#      XBGuard cannot fake.
#      Fix: replace with NetworkSessionType.SystemLink. Uses XNA's LAN
#      discovery instead. No LIVE service required. For internet play,
#      players run XLink Kai or XBSlink to bridge sessions over the net.
#
# All three patches are independent - any subset can be applied. Default
# is all three. This is non-destructive: you can re-run the script and
# it will skip already-patched files.

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$DryRun,
    [switch]$SkipTrial,
    [switch]$SkipPrivileges,
    [switch]$SkipSessionType
)

$ErrorActionPreference = 'Stop'
$repoRoot = $PSScriptRoot

$replacements = @()

if (-not $SkipTrial) {
    $replacements += @{
        Name        = 'Guide.IsTrialMode -> false'
        Pattern     = 'Guide\.IsTrialMode'
        Replacement = 'false /* Guide.IsTrialMode patched out for RGH */'
        Files = @(
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
    }
}

if (-not $SkipPrivileges) {
    $replacements += @{
        Name        = 'Privileges.AllowOnlineSessions -> true'
        Pattern     = 'Screen\.CurrentGamer\.Privileges\.AllowOnlineSessions'
        Replacement = 'true /* AllowOnlineSessions patched out for RGH */'
        Files = @(
            'CastleMinerZ\DNA.CastleMinerZ.UI\MainMenu.cs',
            'CastleMinerZ\DNA.CastleMinerZ\FrontEndScreen.cs'
        )
    }
}

if (-not $SkipSessionType) {
    $replacements += @{
        Name        = 'NetworkSessionType.PlayerMatch -> SystemLink'
        Pattern     = 'NetworkSessionType\.PlayerMatch'
        Replacement = 'NetworkSessionType.SystemLink /* PlayerMatch->SystemLink for RGH (use XLink Kai for internet play) */'
        Files = @(
            'CastleMinerZ\DNA.CastleMinerZ\CastleMinerZGame.cs',
            'CastleMinerZ\DNA.CastleMinerZ.Terrain\ChunkCache.cs'
        )
    }
}

Write-Host "`n[+] RGH unlock patch script" -ForegroundColor Cyan
Write-Host "    Patches to apply:"
foreach ($r in $replacements) {
    Write-Host "      - $($r.Name)"
}
Write-Host ""

if (-not $Force -and -not $DryRun) {
    $confirm = Read-Host "Continue? (y/N)"
    if ($confirm -ne 'y' -and $confirm -ne 'Y') {
        Write-Host "[!] Aborted" -ForegroundColor Yellow
        exit 0
    }
}

$grandTotal = 0

foreach ($r in $replacements) {
    Write-Host "[+] $($r.Name)" -ForegroundColor Cyan
    $totalReplacements = 0

    foreach ($rel in $r.Files) {
        $full = Join-Path $repoRoot $rel
        if (-not (Test-Path $full)) {
            Write-Host "    SKIP: $rel (not found)" -ForegroundColor DarkGray
            continue
        }

        $content = Get-Content $full -Raw

        # Skip if this specific replacement is already present (idempotent)
        if ($content -match [regex]::Escape($r.Replacement)) {
            Write-Host "    -    ${rel} (already patched)" -ForegroundColor DarkGray
            continue
        }

        $matches = [regex]::Matches($content, $r.Pattern)

        if ($matches.Count -eq 0) {
            Write-Host "    -    ${rel} (already clean)" -ForegroundColor DarkGray
            continue
        }

        $newContent = [regex]::Replace($content, $r.Pattern, $r.Replacement)
        $totalReplacements += $matches.Count

        if ($DryRun) {
            Write-Host "    DRY  ${rel} ($($matches.Count) occurrence(s))" -ForegroundColor Yellow
        } else {
            [System.IO.File]::WriteAllText($full, $newContent, [System.Text.UTF8Encoding]::new($false))
            Write-Host "    OK   ${rel} ($($matches.Count) occurrence(s))" -ForegroundColor Green
        }
    }

    Write-Host "    Total: $totalReplacements replacements`n"
    $grandTotal += $totalReplacements
}

if ($DryRun) {
    Write-Host "[!] DRY RUN: $grandTotal total replacements (no changes written)" -ForegroundColor Yellow
} else {
    Write-Host "[+] Done. $grandTotal total replacements." -ForegroundColor Green
    Write-Host "    Now run: .\deploy.ps1 -Pack"
    Write-Host ""
    Write-Host "  Note: Online play now uses XNA SystemLink. For LAN play, just play."
    Write-Host "  For internet play, host and clients run XLink Kai or XBSlink to"
    Write-Host "  bridge SystemLink traffic across the net."
}
