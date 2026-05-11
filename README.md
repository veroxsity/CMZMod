# CMZMod

A working modding pipeline for **CastleMiner Z** (XBLIG, version 1.6.3) on RGH/JTAG Xbox 360 consoles.

This repo contains decompiled C# source for CastleMiner Z's game and engine, working `.csproj` files for the XNA 4.0 / Xbox 360 toolchain, a build pipeline that handles both source-level and folder-based mods, and a custom CLI tool for repacking STFS LIVE containers.

After setup, the daily loop is:

1. Edit C# source, or drop a mod folder into `mods/`
2. Run one PowerShell command
3. FTP the resulting `CMZModded.stfs` to your console
4. Launch and test

---

## Status

The pipeline runs end-to-end on real RGH hardware. Two ways to mod are supported:

- **Framework modding (recommended).** Write a self-contained mod as a folder in `mods/`, using the ModAPI. The game source is never touched. Currently supports recipe changes, custom items, item stat tweaks, and a fixed set of item behaviors (sword, pickaxe, spade, axe, block, consumable). See [docs/framework_modding.md](docs/framework_modding.md).
- **Direct source editing (advanced).** Edit the decompiled C# in `CastleMinerZ\` directly. Full control over anything in the codebase. Texture and audio asset modding is documented but considerably harder than code-only mods. See [docs/source_modding.md](docs/source_modding.md).

If you have an RGH and want to get involved: **https://discord.gg/by5JD9dcEn**

---

## What's in this repo

```
CMZMod/
├── CastleMinerZ/                Game source (~280 .cs files)
│   └── ModAPI/                  The framework API mods compile against
├── DNA Common/                  Engine source (~440 .cs files) + .resx resources
├── CastleMinerZ.sln             VS2010 solution
├── stfs-cli/                    STFS LIVE container repacker (C++/Velocity-based)
├── mods-examples/               Ready-to-build example mods (copy into mods/ to try)
├── docs/
│   ├── framework_modding.md     Framework modding guide (start here)
│   └── source_modding.md        Direct source editing guide (advanced)
├── deploy.ps1                   Build + stage + repack in one command
├── setup_assets.ps1             One-time: extracts assets from your retail STFS
├── patch_mainmenu.ps1           Adds the visible build marker to MainMenu.cs
├── patch_online_sessions.ps1    Adjusts online session compatibility
├── fix_decompile_artifacts.py   Cleans up ~200 decompiler artifacts
├── stfs_extract.py              Standalone STFS unpacker
└── README.md                    This file
```

**What's NOT in this repo** (and never will be, for copyright reasons):

- `Content/` folder (game textures, models, sounds)
- Retail `CastleMinerZ.exe` / `DNA.Common.dll` binaries
- Any `.stfs` package containing assets
- Localization DLLs

You provide your own retail copy of the game (see [Setup](#setup) below).

---

## What you need

### Software (Windows PC)

| Tool | Purpose |
|---|---|
| Visual Studio 2010 | Required for XNA Game Studio integration |
| Xbox 360 SDK (XDK) | Supporting tools |
| XNA Game Studio 4.0 | The compiler/targets for Xbox 360 |
| XNA Framework Redistributable 4.0 Refresh | Runtime |
| Python 3 | Used by helper scripts |
| Git for Windows | Cloning + the STFS CLI's build script |
| WinSCP (or any FTP client) | FTP to your RGH |
| MinGW-w64 with g++ supporting C++20 | Required to build the STFS CLI |
| Botan 3.x at `C:\botan\` | Crypto library the STFS CLI needs |

### Things you provide

- **A retail Castle Miner Z STFS file**, extracted from your own legitimately-acquired copy. ~33 MB, no extension, starts with magic bytes `LIVE`.
- **An RGH/JTAG Xbox 360** with FreeStyle Dash or Aurora and FTP enabled.
- **The XNA Framework Redistributable installed on your console** at `Hdd1:\Content\0000000000000000\FFFE07D1\`. Every XBLIG title needs this; if your console runs other XBLIG games, you already have it.

### Detailed install notes

**XNA Game Studio 4.0 on Windows 10/11 fails out of the box.** Workaround:

1. Get `XNAGS40_setup.exe` from a Microsoft archive
2. Open it with **7-Zip** (treat the `.exe` as an archive)
3. Inside is `redists.msi`, open *that* with 7-Zip too
4. Inside `redists.msi` are the actual installer MSIs, renamed without extensions. Use `tasklist.xml` (also inside) to see proper names
5. Run each MSI individually as admin, **skipping `xnaliveproxy.msi`** (fails harmlessly)

After install, verify:
- `C:\Program Files (x86)\Microsoft XNA\XNA Game Studio\v4.0\References\Xbox360\` exists with the framework DLLs
- `C:\Program Files (x86)\MSBuild\Microsoft\XNA Game Studio\v4.0\` exists with the targets

**MSBuild master targets file may be missing.** If you hit `MSB4057: target "Build" does not exist`, check if `Microsoft.Xna.GameStudio.targets` (singular, the dispatcher) is present at the MSBuild path above. If not, drop a stub there. See the troubleshooting section in [docs/source_modding.md](docs/source_modding.md).

**Botan 3 is finicky.** The MSYS2 prebuilt Botan is the fastest path:

```bash
pacman -S mingw-w64-ucrt-x86_64-botan
```

Then point the build at `C:\msys64\ucrt64\` instead of `C:\botan\` (edit `stfs-cli\build_windows.bat`). Or use the prebuilt download linked in Velocity's `COMPILING.md`.

---

## Setup

After all the prerequisites are installed and you have a retail CMZ STFS file at hand:

```powershell
git clone https://github.com/veroxsity/CMZMod.git
cd CMZMod

# 1. Extract the assets from your retail STFS (one-time)
.\setup_assets.ps1 -StfsPath "C:\path\to\Castle Miner Z"

# 2. Build the STFS CLI tool (one-time)
cd stfs-cli
.\build_windows.bat
cd ..

# 3. Build the C# source AND repack into an STFS (one command)
.\deploy.ps1 -Pack
```

If `.\deploy.ps1 -Pack` succeeds you'll have `CMZModded.stfs` in the project root. FTP it to:

```
Hdd1:\Content\0000000000000000\584E07D1\
```

Trigger a content rescan in FSD/Aurora, launch the game, and look for the lime-green **CMZMOD DEV BUILD** text on the title screen. That confirms your modded build is loading.

---

## Daily workflow

### Framework modding (recommended for new mods)

Drop mod folders into `mods/`. Each folder is a self-contained mod with a `mod.json` and one or more `.cs` files. No source tree edits needed.

```powershell
# 1. Create or copy a mod into mods/your-mod/
#    (try one from mods-examples/ first)

# 2. Rebuild + repack (discovers mods automatically)
.\deploy.ps1 -Pack

# 3. FTP CMZModded.stfs to the console (overwriting previous)

# 4. Launch and test
```

Two working example mods ship in `mods-examples/`:

- `cheap-torches/` modifies the torch recipe to 1 stick (no coal)
- `diamond-sword/` registers a new craftable diamond sword item

Copy either into `mods/`, build, and you have a working mod in minutes. Full walkthrough in **[docs/framework_modding.md](docs/framework_modding.md)**.

### Direct source editing (full control)

```powershell
# 1. Edit game source directly (e.g. tweak a recipe in Receipe.cs)

# 2. Rebuild + repack
.\deploy.ps1 -Pack

# 3. FTP, launch, test
```

The pipeline auto-detects when `mods/` is empty and builds straight from source.

### Useful flags

- `.\deploy.ps1` builds only, no STFS repack. Good for verifying compile after a syntax-risky edit.
- `.\deploy.ps1 -Pack` is the daily-use flag (full pipeline).
- `.\deploy.ps1 -Release` produces an optimised "shipping" build (smaller `.exe`).
- `.\deploy.ps1 -Clean` wipes `bin/`/`obj/` first if you hit weird stale-build issues.

---

## Modding

Pick the approach that fits what you want to do.

### Framework modding (no source edits)

Use the **ModAPI**: write C# files in a `mods/` folder, drop them in, build. The game source is never touched.

Start here: **[docs/framework_modding.md](docs/framework_modding.md)**.

Currently covers: recipes (add, remove, modify, clear), custom items (with sword / pickaxe / spade / axe / block / consumable behaviors), and item stat tweaks (damage, max stack, cooldown, display name, description).

### Direct source editing (full control)

Edit the game source directly for maximum flexibility. Covers everything the framework doesn't yet support: textures, audio, new blocks, custom enemies, multiplayer protocol changes, and so on.

See: **[docs/source_modding.md](docs/source_modding.md)**.

---

## Game metadata

| | |
|---|---|
| Title ID (folder name) | `584E07D1` |
| Title ID (metadata) | `584E07D2` |
| Console install path | `Hdd1:\Content\0000000000000000\584E07D1\` |
| XNA platform | Xbox 360, XNA 4.0, HiDef profile |
| Source version | 1.6.3 (matches retail) |

---

## How this got built

Short version: the retail 1.6.3 binaries were decompiled with **ilspycmd** (forcing C# 4 syntax to match what the XNA 4.0 toolchain accepts), working `.csproj` files were rebuilt from XNA Xbox 360 templates (the SDK-style ones from older decompiles don't build), a script was written to fix ~200 decompiler artifacts (`get_X()`/`set_X()` patterns), and a CLI STFS repacker was built on top of [Velocity's](https://github.com/hetelek/Velocity) `XboxInternals` library, fixing three real upstream bugs along the way. See `stfs-cli/README.md` for the bugs and [docs/source_modding.md](docs/source_modding.md) for the gotchas.

This isn't pretty, but it works. Pull requests welcome.

---

## Distributing modded builds to non-developer users

When releasing a mod for other RGH owners, package your `CMZModded.stfs` with the **XNA Framework Redistributable** so they don't need to install it separately:

```
CMZ-MyMod-v1.0/
├── 584E07D1/                <- your mod (drop into Hdd1:\Content\0000000000000000\)
│   ├── CastleMinerZ.exe
│   ├── DNA.Common.dll
│   ├── Content/
│   └── localization folders
├── FFFE07D1/                <- XNA Framework Redist (Microsoft, free to redistribute)
│   └── (XNA STFS package)
└── README.txt               <- "Drop both folders into Hdd1:\Content\0000000000000000\, rescan in FSD"
```

The XNA Framework Redist was published by Microsoft for free distribution as a runtime dependency.

---

## Credits & references

- **EclipseKatrina's CMZ decompilation**, original 1.4.4 reference: https://github.com/EclipseKatrina/CastleMiner-Z-Decompilation
- **Velocity**, STFS library: https://github.com/hetelek/Velocity
- **ILSpy / ilspycmd**, decompiler used for the 1.6.3 source: https://github.com/icsharpcode/ILSpy
- **Botan**, crypto library: https://botan.randombit.net/
- **DigitalDNA Games**, original CastleMiner Z developers

CastleMiner Z is © DigitalDNA Games. This repository contains decompiled source code for educational and modding purposes only. Game assets are not included; you provide your own legitimately-acquired copy.

---

## License

The original CastleMiner Z C# source is © DigitalDNA Games. The build infrastructure (csprojs, scripts, STFS CLI tool, documentation) added by this project is MIT licensed. See the `stfs-cli/` folder for that subproject's licensing (GPL-3 due to Velocity).

If you're DigitalDNA and you'd prefer this not exist, get in touch via the Discord and we'll talk.
