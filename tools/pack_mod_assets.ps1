# pack_mod_assets.ps1 — helpers for deploy.ps1 mod asset pipeline.
# Converts mod PNGs to XNB via xnbcli when available; always accepts pre-built .xnb.

function Get-XnbCliTool {
    param([string]$RepoRoot)

    $candidates = @(
        $env:XNBCLI_PATH,
        (Join-Path $RepoRoot 'tools\xnbcli-bin\xnbcli.exe'),
        (Join-Path $RepoRoot 'tools\xnbcli\xnbcli.exe'),
        (Join-Path $RepoRoot '..\xnbcli\xnbcli.exe'),
        "C:\Users\$env:USERNAME\Documents\Programming\xnbcli\xnbcli.exe"
    )

    foreach ($path in $candidates) {
        if ([string]::IsNullOrWhiteSpace($path)) { continue }
        if (Test-Path $path) {
            return @{ Mode = 'exe'; Path = (Resolve-Path $path).Path }
        }
    }

    return $null
}

function Convert-PngToXnb {
    param(
        [string]$RepoRoot,
        [string]$PngPath,
        [string]$XnbPath,
        [hashtable]$Tool
    )

    if (-not $Tool) { return $false }

    $templateJson = Join-Path $RepoRoot 'tools\templates\texture2d.json'
    if (-not (Test-Path $templateJson)) {
        return $false
    }

    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($PngPath)
    $pngFileName = "$baseName.png"
    $workDir = Join-Path ([System.IO.Path]::GetTempPath()) ("cmzmod-asset-" + [Guid]::NewGuid().ToString('N'))
    $packOutDir = Join-Path $workDir 'packed'

    try {
        New-Item -ItemType Directory -Force -Path $workDir, $packOutDir | Out-Null

        Copy-Item $PngPath (Join-Path $workDir $pngFileName) -Force

        $jsonText = [System.IO.File]::ReadAllText($templateJson)
        $jsonText = $jsonText -replace 'PLACEHOLDER\.png', $pngFileName
        $jsonPath = Join-Path $workDir "$baseName.json"
        [System.IO.File]::WriteAllText($jsonPath, $jsonText)

        $destDir = Split-Path $XnbPath -Parent
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        }

        & $Tool.Path pack $jsonPath $packOutDir 2>$null | Out-Null

        $builtXnb = Join-Path $packOutDir "$baseName.xnb"
        if (-not (Test-Path $builtXnb)) {
            return $false
        }

        Copy-Item $builtXnb $XnbPath -Force
        return $true
    }
    catch {
        return $false
    }
    finally {
        if (Test-Path $workDir) {
            Remove-Item $workDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

function Get-ModAssetSources {
    param(
        [string]$ModSourcePath,
        [object]$Manifest
    )

    $files = @{}
    $assetsRoot = Join-Path $ModSourcePath 'assets'
    if (Test-Path $assetsRoot) {
        Get-ChildItem $assetsRoot -Recurse -File | Where-Object {
            $_.Extension -in '.png', '.xnb'
        } | ForEach-Object {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
            if ($files.ContainsKey($baseName) -and $files[$baseName].Extension -eq '.xnb') {
                return
            }
            $files[$baseName] = $_
        }
    }

    if ($Manifest.assets) {
        foreach ($field in @('icons', 'textures')) {
            $list = $Manifest.assets.$field
            if (-not $list) { continue }
            foreach ($relPath in $list) {
                $fullPath = Join-Path $ModSourcePath ($relPath -replace '/', '\')
                if (-not (Test-Path $fullPath)) { continue }
                $baseName = [System.IO.Path]::GetFileNameWithoutExtension($fullPath)
                $files[$baseName] = Get-Item $fullPath
            }
        }
    }

    return $files.Values
}

function Invoke-ModAssetPipeline {
    param(
        [string]$RepoRoot,
        [array]$DiscoveredMods,
        [string]$StagingRoot
    )

    if (Test-Path $StagingRoot) {
        Remove-Item $StagingRoot -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $StagingRoot | Out-Null

    $tool = Get-XnbCliTool -RepoRoot $RepoRoot
    $manifestLines = @()
    $packedCount = 0
    $copiedCount = 0
    $pngCount = 0
    $skippedPng = @()

    foreach ($mod in $DiscoveredMods) {
        $modId = $mod.Manifest.id
        $sources = Get-ModAssetSources -ModSourcePath $mod.SourcePath -Manifest $mod.Manifest

        foreach ($file in $sources) {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
            $logicalName = "$modId/$baseName"
            $contentPath = "ModAssets\\$modId\\$baseName"
            $pngRelativePath = "ModAssets\\$modId\\$baseName.png"
            $destDir = Join-Path $StagingRoot "ModAssets\$modId"
            $destXnb = Join-Path $destDir "$baseName.xnb"
            $destPng = Join-Path $destDir "$baseName.png"
            $registered = $false

            if ($file.Extension -eq '.xnb') {
                New-Item -ItemType Directory -Force -Path $destDir | Out-Null
                Copy-Item $file.FullName $destXnb -Force
                $copiedCount++
                $sidecarPng = [System.IO.Path]::ChangeExtension($file.FullName, '.png')
                if (Test-Path $sidecarPng) {
                    Copy-Item $sidecarPng $destPng -Force
                    $pngCount++
                } else {
                    $pngRelativePath = $null
                }
                $registered = $true
            }
            elseif ($file.Extension -eq '.png') {
                New-Item -ItemType Directory -Force -Path $destDir | Out-Null
                Copy-Item $file.FullName $destPng -Force
                $pngCount++

                $sidecarXnb = [System.IO.Path]::ChangeExtension($file.FullName, '.xnb')
                if (Test-Path $sidecarXnb) {
                    Copy-Item $sidecarXnb $destXnb -Force
                    $copiedCount++
                }
                elseif ($tool -and (Convert-PngToXnb -RepoRoot $RepoRoot -PngPath $file.FullName -XnbPath $destXnb -Tool $tool)) {
                    $packedCount++
                }

                $registered = $true
            }

            if (-not $registered) {
                continue
            }

            if ($pngRelativePath) {
                $manifestLines += "            AssetRegistry.Register(`"$logicalName`", `"$contentPath`", `"$pngRelativePath`");"
            } else {
                $manifestLines += "            AssetRegistry.Register(`"$logicalName`", `"$contentPath`", null);"
            }
        }
    }

    return [PSCustomObject]@{
        ManifestLines = $manifestLines
        PackedCount   = $packedCount
        CopiedCount   = $copiedCount
        PngCount      = $pngCount
        SkippedPng    = $skippedPng
        ToolFound     = ($null -ne $tool)
    }
}

function Write-GeneratedAssetManifest {
    param(
        [string]$TargetPath,
        [string[]]$RegisterLines
    )

    $lines = @()
    $lines += '// AUTO-GENERATED -- do not edit. Regenerated by deploy.ps1.'
    $lines += 'namespace DNA.CastleMinerZ.ModAPI.Internal'
    $lines += '{'
    $lines += '    public static class GeneratedAssetManifest'
    $lines += '    {'
    $lines += '        public static void Initialize()'
    $lines += '        {'

    if ($RegisterLines -and $RegisterLines.Count -gt 0) {
        $lines += $RegisterLines
    }

    $lines += '        }'
    $lines += '    }'
    $lines += '}'

    [System.IO.File]::WriteAllText($TargetPath, ($lines -join "`r`n"))
}
