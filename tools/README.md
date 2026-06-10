# CMZMod tools

## Mod asset pipeline (`pack_mod_assets.ps1`)

Mods can ship textures under `assets/` in their mod folder. `deploy.ps1` calls this script automatically.

### Mod folder layout

```
mods/my-mod/
├── mod.json
├── MyMod.cs
└── assets/
    ├── icons/
    │   └── my-sword.png
    └── textures/
        └── zombie-pink.png
```

Mod PNGs are copied to `Content/ModAssets/` and loaded at runtime via `Texture2D.FromStream`.
xnbcli packing is optional — Xbox 360 needs LZX-compressed XNB, which xnbcli cannot produce on pack.

```csharp
// Item icon — bare filename resolves against the loading mod id
IconTextureName = "my-sword",

// Or full path
IconTextureName = "my-mod/my-sword",

// Enemy reskin (must match vanilla UV layout if reusing zombie model)
TextureAssetName = "my-mod/zombie-pink",

// Runtime load
Texture2D tex = Assets.LoadTexture("my-mod/my-sword");
```

### xnbcli setup (PNG → XNB)

xnbcli does **not** pack PNG files directly. `deploy.ps1` wraps the correct workflow:

1. Copy your PNG into a temp folder
2. Write a Texture2D `.json` descriptor (from `tools/templates/texture2d.json`)
3. Run `xnbcli pack descriptor.json output_dir/` → produces `.xnb`

Install xnbcli:

1. Download [xnbcli-windows-x64.zip](https://github.com/LeonBlade/xnbcli/releases)
2. Extract `xnbcli.exe` to `tools/xnbcli-bin/xnbcli.exe`, **or** set `XNBCLI_PATH`

Without xnbcli, ship a pre-built `.xnb` next to your PNG (same basename).

### Manual pack

```powershell
.\tools\pack_mod_assets.ps1   # loaded by deploy.ps1; use Invoke-ModAssetPipeline directly if needed
```

For ad-hoc xnbcli experiments, use a scratch folder under `tools/asset-work/` (gitignored).

See also `docs/source_modding.md` section 4 for xnbcli background.
