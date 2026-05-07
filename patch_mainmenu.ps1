# patch_mainmenu.ps1
# Adds the BuildTag mod marker to MainMenu.cs in the 1.6.3 source.
# Uses regex matching so it tolerates whitespace differences (tabs vs spaces).

$ErrorActionPreference = 'Stop'

$file = "CastleMinerZ\DNA.CastleMinerZ.UI\MainMenu.cs"

if (-not (Test-Path $file)) {
    Write-Host "[!] $file not found. Run from project root." -ForegroundColor Red
    exit 1
}

$content = Get-Content $file -Raw

if ($content -match 'BuildTag') {
    Write-Host "[!] BuildTag already present - nothing to do." -ForegroundColor Yellow
    exit 0
}

# --- Patch 1: insert BuildTag constant after class declaration ---
$pattern1 = '(public\s+class\s+MainMenu\s*:\s*MenuScreen\s*\r?\n\s*\{\s*\r?\n)(\s*)(private\s+CastleMinerZGame\s+_game\s*;)'

if ($content -notmatch $pattern1) {
    Write-Host "[!] Could not find class declaration with regex." -ForegroundColor Red
    Write-Host "    First 30 lines of file:" -ForegroundColor Red
    Get-Content $file | Select-Object -First 30 | ForEach-Object { Write-Host "    $_" }
    exit 1
}

$replacement1 = '$1$2// === MOD BUILD MARKER ===' + "`r`n" + `
                '$2// Renders on the title screen so we can confirm at a glance that this' + "`r`n" + `
                '$2// is our custom build, not vanilla retail. Change for each test build.' + "`r`n" + `
                '$2private const string BuildTag = "CMZMOD DEV BUILD";' + "`r`n" + `
                "`r`n" + `
                '$2$3'

$content = [regex]::Replace($content, $pattern1, $replacement1)

# --- Patch 2: insert marker drawing inside OnDraw, after spriteBatch.Begin() ---
$pattern2 = '(spriteBatch\.Begin\(\)\s*;\s*\r?\n)(\s*)(int\s+num\s*=\s*512\s*;)'

if ($content -notmatch $pattern2) {
    Write-Host "[!] Could not find OnDraw block with regex." -ForegroundColor Red
    Write-Host "    Searching for spriteBatch.Begin() in file ..." -ForegroundColor Red
    Select-String -Path $file -Pattern 'spriteBatch\.Begin' | ForEach-Object {
        Write-Host "    Line $($_.LineNumber): $($_.Line.Trim())"
    }
    exit 1
}

$replacement2 = '$1' + "`r`n" + `
                '$2// Draw the mod build marker top-left of the title-safe area.' + "`r`n" + `
                '$2SpriteFont markerFont = _game._largeFont;' + "`r`n" + `
                '$2Vector2 markerPos = new Vector2(titleSafeArea.Left + 10f, titleSafeArea.Top + 10f);' + "`r`n" + `
                '$2spriteBatch.DrawString(markerFont, BuildTag, markerPos + new Vector2(2f, 2f), Color.Black);' + "`r`n" + `
                '$2spriteBatch.DrawString(markerFont, BuildTag, markerPos, Color.Lime);' + "`r`n" + `
                "`r`n" + `
                '$2$3'

$content = [regex]::Replace($content, $pattern2, $replacement2)

# Save
Set-Content -Path $file -Value $content -NoNewline

Write-Host "[+] Patched MainMenu.cs" -ForegroundColor Green
Write-Host ""
Write-Host "BuildTag references in file:" -ForegroundColor Cyan
Select-String -Path $file -Pattern 'BuildTag' | ForEach-Object {
    Write-Host "    Line $($_.LineNumber): $($_.Line.Trim())"
}
Write-Host ""
Write-Host "Next: run  .\deploy.ps1 -Pack" -ForegroundColor Cyan
