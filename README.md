# CMZMod

A working modding pipeline for **CastleMiner Z** (XBLIG, version 1.6.3) on RGH/JTAG Xbox 360 consoles.

This repo contains decompiled C# source for CastleMiner Z's game and engine, working `.csproj` files for the XNA 4.0 / Xbox 360 toolchain, scripts that automate the full build-and-package pipeline, and a custom CLI tool for repacking STFS LIVE containers.

After setup, the loop is:

1. Edit C# source
2. Run one PowerShell command
3. FTP the resulting `CMZModded.stfs` to your console
4. Launch and test

---

## Status

- ✅ Pipeline runs end-to-end
- ✅ Custom builds load on real RGH hardware
- ✅ Cosmetic mods, recipe mods, and stat tweaks work
- 🚧 Texture and asset modding documented but not yet polished

If you've got an RGH and want to get involved with active modding work, hop in: **https://discord.gg/by5JD9dcEn**

---

## What's in this repo

```
CMZMod/
├── CastleMinerZ/               Game source (~280 .cs files)
├── DNA Common/                 Engine source (~440 .cs files) + .resx resources
├── CastleMinerZ.sln            VS2010 solution
├── stfs-cli/                   STFS LIVE container repacker (C++/Velocity-based)
├── deploy.ps1                  Build + stage + repack in one command
├── setup_assets.ps1            One-time: extracts assets from your retail STFS
├── patch_mainmenu.ps1          Adds the visible build marker to MainMenu.cs
├── fix_decompile_artifacts.py  Cleans up ~200 decompiler artifacts
├── stfs_extract.py             Standalone STFS unpacker
├── README.md                   This file
└── MODDING.md                  Modding guide (recipes, items, blocks, AI, etc.)
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
| WinSCP | FTP to your RGH |
| MinGW-w64 with g++ supporting C++20 | Required to build the STFS CLI |
| Botan 3.x at `C:\botan\` | Crypto library the STFS CLI needs |

### Things you provide

- **A retail Castle Miner Z STFS file**, extracted from your own legitimately-acquired copy. ~33 MB, no extension, starts with magic bytes `LIVE`.
- **An RGH/JTAG Xbox 360** with FreeStyle Dash or Aurora and FTP enabled.
- **The XNA Framework Redistributable installed on your console** at `Hdd1:\Content\0000000000000000\FFFE07D1\` (every XBLIG title needs this; if your console runs other XBLIG games, you already have it).

### Detailed install notes

**XNA Game Studio 4.0 on Windows 10/11 fails out of the box.** Workaround:

1. Get `XNAGS40_setup.exe` from a Microsoft archive
2. Open it with **7-Zip** (treat the `.exe` as an archive)
3. Inside is `redists.msi` — open *that* with 7-Zip
4. Inside `redists.msi` are the actual installer MSIs, renamed without extensions. Use `tasklist.xml` (also inside) to see proper names
5. Run each MSI individually as admin, **skipping `xnaliveproxy.msi`** (fails harmlessly)

After install, verify:
- `C:\Program Files (x86)\Microsoft XNA\XNA Game Studio\v4.0\References\Xbox360\` exists with the framework DLLs
- `C:\Program Files (x86)\MSBuild\Microsoft\XNA Game Studio\v4.0\` exists with the targets

**MSBuild master targets file may be missing.** If you hit `MSB4057: target "Build" does not exist`, check if `Microsoft.Xna.GameStudio.targets` (singular, the dispatcher) is present at the MSBuild path above. If not, drop a stub there — see the troubleshooting section in [MODDING.md](MODDING.md).

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

# 3. Build the C# source AND repack into an STFS — one command
.\deploy.ps1 -Pack
```

If `.\deploy.ps1 -Pack` succeeds, you'll have `CMZModded.stfs` in the project root. FTP it to:

```
Hdd1:\Content\0000000000000000\584E07D1\
```

Trigger a content rescan in FSD/Aurora, launch the game, and look for the lime-green **CMZMOD DEV BUILD** text on the title screen. That confirms your modded build is loading.

---

## Daily workflow

```powershell
# 1. Edit some C# (e.g. tweak a recipe in CastleMinerZ\DNA.CastleMinerZ.Inventory\Receipe.cs)

# 2. Rebuild + repack
.\deploy.ps1 -Pack

# 3. FTP CMZModded.stfs to the console (overwriting previous)

# 4. Launch and test
```

After the first run, step 2 takes about 5 seconds (only your changed source recompiles).

### Useful flags

- `.\deploy.ps1` — build only, no STFS repack. Good for verifying compile after a syntax-risky edit
- `.\deploy.ps1 -Pack` — full pipeline; the daily-use flag
- `.\deploy.ps1 -Release` — optimised "shipping" build (smaller .exe)
- `.\deploy.ps1 -Clean` — wipe `bin/`/`obj/` first if you hit weird stale-build issues

---

## Modding

See **[MODDING.md](MODDING.md)** for a 600-line guide covering:

- Cosmetic mods (UI text, build marker, version strings)
- Recipe mods (the easiest "real" mod)
- Stat tweaks (weapon damage, enemy health, balance)
- Editing existing textures (`xnbcli` workflow)
- Adding new items (full Diamond Pickaxe walkthrough)
- Adding new blocks
- AI behaviour and spawn tweaks
- New enemies
- Audio replacement (XACT)
- Multiplayer compatibility (the protocol-version trap)
- Debugging mods that don't work

Recommended first mod: change the `BuildTag` constant in `CastleMinerZ\DNA.CastleMinerZ.UI\MainMenu.cs` to your own string. Build, deploy, see your custom text on the title screen. Confirms the loop, takes 30 seconds.

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

Short version: I decompiled the retail 1.6.3 binaries with **ilspycmd** (forcing C# 4 syntax to match what the XNA 4.0 toolchain accepts), wrote working `.csproj` files based on XNA Xbox 360 templates (the SDK-style ones from older decompiles don't build), wrote a script to fix ~200 decompiler artifacts (`get_X()`/`set_X()` patterns), and built a CLI STFS repacker on top of [Velocity's](https://github.com/hetelek/Velocity) `XboxInternals` library — fixing three real upstream bugs along the way. See `stfs-cli/README.md` for the bugs and `MODDING.md` for the gotchas.

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

- **EclipseKatrina's CMZ decompilation** — original 1.4.4 reference: https://github.com/EclipseKatrina/CastleMiner-Z-Decompilation
- **Velocity** — STFS library: https://github.com/hetelek/Velocity
- **ILSpy / ilspycmd** — decompiler used for the 1.6.3 source: https://github.com/icsharpcode/ILSpy
- **Botan** — crypto library: https://botan.randombit.net/
- **DigitalDNA Games** — original CastleMiner Z developers

CastleMiner Z is © DigitalDNA Games. This repository contains decompiled source code for educational and modding purposes only. Game assets are not included; you provide your own legitimately-acquired copy.

---

## License

The original CastleMiner Z C# source is © DigitalDNA Games. The build infrastructure (csprojs, scripts, STFS CLI tool, documentation) added by this project is MIT licensed. See the `stfs-cli/` folder for that subproject's licensing (GPL-3 due to Velocity).

If you're DigitalDNA and you'd prefer this not exist, get in touch via the Discord and we'll talk.
