# deploy.ps1 - Build CMZ with optional mod support
#
# Usage:
#   .\deploy.ps1               # Debug build (default, fast path -- no mods required)
#   .\deploy.ps1 -Release      # Release build
#   .\deploy.ps1 -Clean        # Wipe bin/obj before building
#   .\deploy.ps1 -Pack         # Build + stage + repack into STFS LIVE container
#
# When mods/ contains mod folders, the build uses a build_temp pipeline:
#   source + mods -> build_temp -> MSBuild -> stage -> STFS
# The source tree is never modified.

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

$buildTemp  = Join-Path $repoRoot 'build_temp'
$modDir     = Join-Path $repoRoot 'mods'
$modApiVer  = 1

# -- Helpers ------------------------------------------------------------------
function Write-Step($msg) { Write-Host "`n[+] $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "    $msg"   -ForegroundColor Green }
function Write-Bad($msg)  { Write-Host "    $msg"   -ForegroundColor Red   }
function Write-Warn($msg) { Write-Host "    $msg"   -ForegroundColor Yellow }

# -- Sanity checks ------------------------------------------------------------
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

# -- Step 1: Mod discovery ----------------------------------------------------
Write-Step "Discovering mods"

$discoveredMods = @()
if (Test-Path $modDir) {
    foreach ($dir in Get-ChildItem $modDir -Directory) {
        $manifestPath = Join-Path $dir.FullName "mod.json"
        if (-not (Test-Path $manifestPath)) {
            Write-Warn ("  " + $dir.Name + " has no mod.json, skipping")
            continue
        }
        $manifest = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

        foreach ($field in @('id', 'name', 'version', 'modapi_version')) {
            if (-not $manifest.$field) {
                Write-Bad "$($dir.Name)/mod.json missing '$field' field"
                exit 1
            }
        }

        if ($manifest.modapi_version -ne $modApiVer) {
            Write-Bad "$($dir.Name) targets ModAPI v$($manifest.modapi_version), this build is v$modApiVer"
            exit 1
        }

        if ($manifest.id -notmatch '^[a-z0-9._-]+$') {
            Write-Bad "$($dir.Name)/mod.json: id must be lowercase alphanumeric with dots/dashes"
            exit 1
        }

        $discoveredMods += [PSCustomObject]@{
            Manifest   = $manifest
            FolderName = $dir.Name
            SourcePath = $dir.FullName
        }
    }
}

$hasMods = $discoveredMods.Count -gt 0

if ($hasMods) {
    Write-Ok ("Found {0} mod(s):" -f $discoveredMods.Count)
    foreach ($m in $discoveredMods) {
        Write-Ok ("  {0,-30} v{1}  ({2})" -f ($m.Manifest.id), $m.Manifest.version, $m.FolderName)
    }
    $discoveredMods = $discoveredMods | Sort-Object FolderName
} else {
    Write-Ok "No mods found in mods/ -- building vanilla"
}

# -- Step 2: Build_temp setup (modded builds only) ----------------------------
if ($hasMods) {
    Write-Step "Setting up build_temp"

    if (Test-Path $buildTemp) { Remove-Item $buildTemp -Recurse -Force }

    Copy-Item (Join-Path $repoRoot "CastleMinerZ") (Join-Path $buildTemp "CastleMinerZ") -Recurse
    Copy-Item (Join-Path $repoRoot "DNA Common") (Join-Path $buildTemp "DNA Common") -Recurse
    Copy-Item $solution $buildTemp

    foreach ($mod in $discoveredMods) {
        $modTarget = Join-Path $buildTemp "CastleMinerZ\Mods\$($mod.Manifest.id)"
        New-Item -ItemType Directory -Force -Path $modTarget | Out-Null
        Get-ChildItem (Join-Path $mod.SourcePath "*.cs") -File | ForEach-Object {
            Copy-Item $_.FullName $modTarget
        }
        $csCount = (Get-ChildItem $modTarget -Filter "*.cs").Count
        Write-Ok ("  {0,-30} {1} file(s)" -f $mod.Manifest.id, $csCount)
    }

    # -- Step 3: Generate GeneratedModRegistry.cs (overwrite stub in build_temp) --
    Write-Step "Generating ModRegistry"

    $regLines = @()
    $regLines += '// AUTO-GENERATED -- do not edit. Regenerated by deploy.ps1 -Pack.'
    $regLines += "// $($discoveredMods.Count) mod(s) discovered."
    $regLines += ''
    $regLines += 'using System;'
    $regLines += 'using DNA.CastleMinerZ.ModAPI;'
    $regLines += ''
    $regLines += 'namespace DNA.CastleMinerZ.ModAPI.Internal'
    $regLines += '{'
    $regLines += '    public static class GeneratedModRegistry'
    $regLines += '    {'
    $regLines += '        public static void Initialize()'
    $regLines += '        {'
    $regLines += '            ModLog.Info("=== Mod load begin ===");'
    $regLines += '            ModLog.Info("ModAPI version: ' + $modApiVer + '");'
    $regLines += '            ModLog.Info("' + $discoveredMods.Count + ' mod(s) registered");'
    $regLines += '            ModRegistry.LoadedModIds.Clear();'
    $regLines += '            ModRegistry.FailedModIds.Clear();'

    foreach ($mod in $discoveredMods) {
        $modId = $mod.Manifest.id
        $modVer = $mod.Manifest.version
        $modFolder = $mod.FolderName

        $csFiles = Get-ChildItem (Join-Path $mod.SourcePath "*.cs") -File
        $modEntries = @()
        foreach ($csFile in $csFiles) {
            $content = Get-Content $csFile.FullName -Raw -Encoding UTF8
            $nsMatch = [regex]::Match($content, 'namespace\s+([\w.]+)')
            $ns = ""
            if ($nsMatch.Success) { $ns = $nsMatch.Groups[1].Value }

            $classMatches = [regex]::Matches($content, '\[Mod\s*\([^)]*\)\s*\]\s*\r?\n\s*(?:public\s+)?static\s+(?:partial\s+)?class\s+(\w+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)
            foreach ($cm in $classMatches) {
                $className = $cm.Groups[1].Value
                if ($ns -ne "") { $modEntries += "$ns.$className" }
                else            { $modEntries += $className }
            }
        }

        if ($modEntries.Count -eq 0) {
            Write-Warn ("  Could not find [Mod]-attributed class in " + $modFolder)
            $regLines += ''
            $regLines += '            // WARNING: No [Mod] class found in ' + $modFolder
            continue
        }

        foreach ($fullClassName in $modEntries) {
            Write-Ok ("  {0,-30} -> {1}.OnLoad()" -f $modId, $fullClassName)
            $regLines += ''
            $regLines += '            try'
            $regLines += '            {'
            $regLines += '                ModRegistry.CurrentLoadingModId = "' + $modId + '";'
            $regLines += '                ' + $fullClassName + '.OnLoad();'
            $regLines += '                ModLog.Loaded("' + $modId + '", "' + $modVer + '");'
            $regLines += '                ModRegistry.LoadedModIds.Add("' + $modId + '");'
            $regLines += '            }'
            $regLines += '            catch (System.Exception ex)'
            $regLines += '            {'
            $regLines += '                ModLog.Error("Mod ' + $modId + ' failed: " + ex.Message);'
            $regLines += '                ModRegistry.FailedModIds.Add("' + $modId + '");'
            $regLines += '            }'
            $regLines += '            finally'
            $regLines += '            {'
            $regLines += '                ModRegistry.CurrentLoadingModId = null;'
            $regLines += '            }'
        }
    }

    $regLines += ''
    $regLines += '            ModLog.Info("=== Mod load complete ===");'
    $regLines += '        }'
    $regLines += '    }'
    $regLines += '}'

    $regContent = $regLines -join "`r`n"
    $regTarget = Join-Path $buildTemp "CastleMinerZ\ModAPI\Internal\GeneratedModRegistry.cs"
    [System.IO.File]::WriteAllText($regTarget, $regContent)
    Write-Ok ("Wrote GeneratedModRegistry.cs ({0} lines)" -f $regLines.Count)

    # -- Step 4: Validate mod IDs are unique ----------------------------------
    $ids = $discoveredMods | ForEach-Object { $_.Manifest.id }
    $dupes = $ids | Group-Object | Where-Object { $_.Count -gt 1 }
    if ($dupes) {
        Write-Bad ("Duplicate mod IDs found: " + ($dupes.Name -join ', '))
        exit 1
    }

    # -- Step 4b: Validate mod item IDs are unique and well-formed --------------
    Write-Step "Validating mod item IDs"
    $allItemIds = @{}
    $idPattern = '^[a-z0-9_-]+\.[a-z0-9_-]+$'
    foreach ($mod in $discoveredMods) {
        $csFiles = Get-ChildItem (Join-Path $mod.SourcePath "*.cs") -File
        foreach ($csFile in $csFiles) {
            $content = Get-Content $csFile.FullName -Raw -Encoding UTF8
            $matches = [regex]::Matches($content, 'Items\.Register\s*\(\s*"([^"]+)"')
            foreach ($m in $matches) {
                $itemId = $m.Groups[1].Value
                if ($allItemIds.ContainsKey($itemId)) {
                    Write-Bad ("Duplicate mod item ID '" + $itemId + "' registered by '" + $allItemIds[$itemId] + "' and '" + $mod.FolderName + "'")
                    exit 1
                }
                if ($itemId -notmatch $idPattern) {
                    Write-Bad ("Invalid mod item ID '" + $itemId + "' in " + $mod.FolderName + ". IDs must be namespaced (e.g. 'you.my-item') with only lowercase letters, digits, dashes, underscores.")
                    exit 1
                }
                $allItemIds[$itemId] = $mod.FolderName
            }
        }
    }
    if ($allItemIds.Count -gt 0) {
        Write-Ok ("  " + $allItemIds.Count + " mod item ID(s) validated")
    }

    # -- Step 5: Prepare build paths ------------------------------------------
    $buildDnaOutput  = Join-Path $buildTemp "DNA Common\bin\$platform\$config\DNA.Common.dll"
    $buildCmzOutput  = Join-Path $buildTemp "CastleMinerZ\bin\$platform\$config\CastleMinerZ.exe"
    $buildSolution   = Join-Path $buildTemp 'CastleMinerZ.sln'
} else {
    $buildDnaOutput  = $dnaOutput
    $buildCmzOutput  = $cmzOutput
    $buildSolution   = $solution
}

# -- Step 6: MSBuild ----------------------------------------------------------
Write-Step "Building $config|$platform"
$buildStart = Get-Date

$buildOutputPath = Join-Path $repoRoot 'build_output.txt'
& $msbuild $buildSolution /p:Configuration=$config "/p:Platform=$platform" /v:minimal /nologo /m 2>&1 | Tee-Object -FilePath $buildOutputPath

$buildDuration = ((Get-Date) - $buildStart).TotalSeconds
$buildFailed = $LASTEXITCODE -ne 0

if ($hasMods) {
    # Rewrite build_temp paths back to original mods/ paths in output
    $rewrittenOutput = Get-Content $buildOutputPath | ForEach-Object {
        $line = $_
        foreach ($mod in $discoveredMods) {
            $buildPathStr = "CastleMinerZ\Mods\$($mod.Manifest.id)\"
            $srcPathStr   = "mods\$($mod.FolderName)\"
            $line = $line -replace [regex]::Escape($buildPathStr), $srcPathStr
        }
        $line
    }

    if ($buildFailed) {
        Write-Bad "Build FAILED (exit $LASTEXITCODE)"
        Write-Host ""
        Write-Host "  Errors:" -ForegroundColor Red
        $rewrittenOutput | Select-String -Pattern 'error\s+CS\d+' | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
    }
} else {
    if ($buildFailed) {
        Write-Bad "Build FAILED (exit $LASTEXITCODE)"
    }
}

if ($buildFailed) {
    exit $LASTEXITCODE
}

Write-Ok ("build succeeded in {0:N1}s" -f $buildDuration)

# -- Step 7: Verify build artifacts -------------------------------------------
Write-Step "Verifying build artifacts"
if (-not (Test-Path $buildDnaOutput)) { Write-Bad "Missing DNA.Common.dll"; exit 1 }
if (-not (Test-Path $buildCmzOutput)) { Write-Bad "Missing CastleMinerZ.exe"; exit 1 }
Write-Ok ("DNA.Common.dll   $((Get-Item $buildDnaOutput).Length) bytes")
Write-Ok ("CastleMinerZ.exe $((Get-Item $buildCmzOutput).Length) bytes")

if ($hasMods) {
    Write-Step "Copying build artifacts to source tree output paths"
    $dnaOutDir = Split-Path $dnaOutput -Parent
    $cmzOutDir = Split-Path $cmzOutput -Parent
    if (-not (Test-Path $dnaOutDir)) { New-Item -ItemType Directory -Force -Path $dnaOutDir | Out-Null }
    if (-not (Test-Path $cmzOutDir)) { New-Item -ItemType Directory -Force -Path $cmzOutDir | Out-Null }
    Copy-Item $buildDnaOutput $dnaOutput -Force
    Copy-Item $buildCmzOutput $cmzOutput -Force
    Write-Ok "Artifacts copied to source tree output paths"
}

# -- Step 8: Stage and pack ---------------------------------------------------
if (-not (Test-Path $deployDir)) {
    Write-Step "Seeding deploy/ from cmz_extracted/ (first run)"
    Copy-Item -Path $retailDir -Destination $deployDir -Recurse
    Write-Ok ("seeded {0} files" -f (Get-ChildItem $deployDir -Recurse -File).Count)
} else {
    Write-Step "deploy/ already exists, refreshing binaries"
}

Write-Step "Updating binaries in deploy/"
$gameDir = Join-Path $deployDir '584E07D1'
if (-not (Test-Path $gameDir)) {
    Write-Bad "deploy/584E07D1/ folder missing"
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
        --title-id 584E07D1 `
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
    Write-Host "  Hdd1:\Content\0000000000000000\584E07D1\00000002"
} else {
    Write-Host ""
    Write-Host "Tip: pass -Pack to also repack into an STFS LIVE container" -ForegroundColor DarkGray
    Write-Host "     (requires stfs_pack.exe built from the stfs-cli folder)" -ForegroundColor DarkGray
}

# -- Step 9: Cleanup build_temp -----------------------------------------------
if ($hasMods -and (Test-Path $buildTemp)) {
    Write-Step "Cleaning up build_temp"
    Remove-Item $buildTemp -Recurse -Force
    Write-Ok "removed"
}
