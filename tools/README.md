# CMZMod tools

## Mod asset pipeline (`pack_mod_assets.ps1`)

Mods ship textures as **PNG** files under `assets/`. `deploy.ps1` copies them into `Content/ModAssets/` and the game loads them at runtime via `Texture2D.FromStream`.

No XNA Content Pipeline or xnbcli required for mod assets.

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

### Why not XNB?

Vanilla CMZ textures are LZX-compressed XNB files. Tools like xnbcli produce uncompressed XNB that the Xbox 360 runtime rejects. PNG sidesteps that entirely.

To edit **vanilla** `.xnb` assets (block atlas, existing item icons), use xnbcli via the workflow in `docs/source_modding.md` — that's separate from the mod asset pipeline.

For ad-hoc experiments, use a scratch folder under `tools/asset-work/` (gitignored).
