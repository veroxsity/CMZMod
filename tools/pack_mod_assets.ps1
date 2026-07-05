# pack_mod_assets.ps1 — helpers for deploy.ps1 mod asset pipeline.
# Mod textures ship as PNG; the game loads them at runtime via Texture2D.FromStream.
function Get-ModIconTextureSources {
    param(
        [string]$ModSourcePath,
        [object]$Manifest
    )
    $files = @{}
    $iconsRoot = Join-Path $ModSourcePath 'assets\icons'
    if (Test-Path $iconsRoot) {
        Get-ChildItem $iconsRoot -Recurse -File -Filter '*.png' | ForEach-Object {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
            $files[$baseName] = $_
        }
    }
    $texturesRoot = Join-Path $ModSourcePath 'assets\textures'
    if (Test-Path $texturesRoot) {
        Get-ChildItem $texturesRoot -Recurse -File -Filter '*.png' | ForEach-Object {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
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
                if ([System.IO.Path]::GetExtension($fullPath) -ne '.png') { continue }
                $baseName = [System.IO.Path]::GetFileNameWithoutExtension($fullPath)
                $files[$baseName] = Get-Item $fullPath
            }
        }
    }
    return $files.Values
}
function Get-ModBlockTextureSources {
    param([string]$ModSourcePath)
    $files = @{}
    $blocksRoot = Join-Path $ModSourcePath 'assets\blocks'
    if (-not (Test-Path $blocksRoot)) {
        return @()
    }
    Get-ChildItem $blocksRoot -File -Filter '*.png' | ForEach-Object {
        $alias = [System.IO.Path]::GetFileNameWithoutExtension($_.Name).ToLowerInvariant()
        $files[$alias] = $_
    }
    return $files.Values
}
function Get-ModItemTextureSources {
    param([string]$ModSourcePath)
    $files = @{}
    $itemsRoot = Join-Path $ModSourcePath 'assets\items'
    if (-not (Test-Path $itemsRoot)) {
        return @()
    }
    Get-ChildItem $itemsRoot -File -Filter '*.png' | ForEach-Object {
        $alias = [System.IO.Path]::GetFileNameWithoutExtension($_.Name).ToLowerInvariant()
        $files[$alias] = $_
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
    $manifestLines = @()
    $blockManifestLines = @()
    $pngCount = 0
    $blockCount = 0
    foreach ($mod in $DiscoveredMods) {
        $modId = $mod.Manifest.id
        $iconSources = Get-ModIconTextureSources -ModSourcePath $mod.SourcePath -Manifest $mod.Manifest
        $blockSources = Get-ModBlockTextureSources -ModSourcePath $mod.SourcePath
        $itemSources = Get-ModItemTextureSources -ModSourcePath $mod.SourcePath
        foreach ($file in $iconSources) {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
            $logicalName = "$modId/$baseName"
            $contentPath = "ModAssets\\$modId\\$baseName"
            $pngRelativePath = "ModAssets\\$modId\\$baseName.png"
            $destDir = Join-Path $StagingRoot "ModAssets\$modId"
            $destPng = Join-Path $destDir "$baseName.png"
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Copy-Item $file.FullName $destPng -Force
            $pngCount++
            $manifestLines += "            AssetRegistry.Register(`"$logicalName`", `"$contentPath`", `"$pngRelativePath`");"
        }
        foreach ($file in $blockSources) {
            $alias = [System.IO.Path]::GetFileNameWithoutExtension($file.Name).ToLowerInvariant()
            $pngRelativePath = "ModAssets\\$modId\\blocks\\$alias.png"
            $destDir = Join-Path $StagingRoot "ModAssets\$modId\blocks"
            $destPng = Join-Path $destDir "$alias.png"
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Copy-Item $file.FullName $destPng -Force
            $blockCount++
            $blockManifestLines += "            BlockTextureRegistry.Register(`"$alias`", `"$pngRelativePath`");"
        }
        foreach ($file in $itemSources) {
            $alias = [System.IO.Path]::GetFileNameWithoutExtension($file.Name).ToLowerInvariant()
            $pngRelativePath = "ModAssets\\$modId\\items\\$alias.png"
            $destDir = Join-Path $StagingRoot "ModAssets\$modId\items"
            $destPng = Join-Path $destDir "$alias.png"
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
            Copy-Item $file.FullName $destPng -Force
            $blockCount++
            $blockManifestLines += "            VanillaItemIconRegistry.Register(`"$alias`", `"$pngRelativePath`");"
        }
    }
    return [PSCustomObject]@{
        ManifestLines      = $manifestLines
        BlockManifestLines = $blockManifestLines
        PngCount           = $pngCount
        BlockCount         = $blockCount
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
function Write-GeneratedBlockTextureManifest {
    param(
        [string]$TargetPath,
        [string[]]$RegisterLines
    )
    $lines = @()
    $lines += '// AUTO-GENERATED -- do not edit. Regenerated by deploy.ps1.'
    $lines += 'namespace DNA.CastleMinerZ.ModAPI.Internal'
    $lines += '{'
    $lines += '    public static class GeneratedBlockTextureManifest'
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