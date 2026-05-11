---
hide:
  - navigation
  - toc
---

# CMZMod

A working modding pipeline for **CastleMiner Z** (XBLIG 1.6.3) on RGH/JTAG Xbox 360 consoles.

Edit C# source or drop a mod folder into `mods/`, run one PowerShell command, FTP the resulting `CMZModded.stfs` to your console, launch and test.

[Get started on GitHub :fontawesome-brands-github:](https://github.com/veroxsity/CMZMod){ .md-button .md-button--primary }
[Join the Discord :fontawesome-brands-discord:](https://discord.gg/by5JD9dcEn){ .md-button }

---

## Two ways to mod

<div class="grid cards" markdown>

-   :material-puzzle-outline: **Framework modding**

    ---

    Write a self-contained mod as a folder of C# files. Drop it into `mods/`, build, deploy. The game source is never touched. Covers recipes, custom items, item stat tweaks, and a fixed set of behaviors (sword, pickaxe, spade, axe, block, consumable).

    **Recommended for new mods.**

    [Open the guide →](framework_modding.md)

-   :material-code-braces: **Direct source editing**

    ---

    Edit the decompiled C# directly. Full control over anything in the codebase: textures, audio, new blocks, custom enemies, multiplayer protocol, save format. Higher friction, but unlimited in scope.

    **For things the framework doesn't cover.**

    [Open the guide →](source_modding.md)

</div>

---

## At a glance

| | |
|---|---|
| **Game** | CastleMiner Z (XBLIG), version 1.6.3 |
| **Console requirement** | RGH or JTAG Xbox 360 with FreeStyle Dash or Aurora |
| **Toolchain** | Visual Studio 2010, XNA Game Studio 4.0, Xbox 360 SDK |
| **Title ID** | `584E07D1` (folder), `584E07D2` (metadata) |
| **Install path** | `Hdd1:\Content\0000000000000000\584E07D1\` |
| **Status** | Pipeline runs end-to-end on real RGH hardware |

---

## Getting started

The full setup walkthrough lives in the project [README on GitHub](https://github.com/veroxsity/CMZMod#setup): prerequisites, asset extraction, building the STFS CLI, and the first build/deploy cycle.

Once your build is loading on the console (look for the green **CMZMOD DEV BUILD** marker on the title screen), come back here and pick a modding path above.

---

## Community

- **Discord:** [discord.gg/by5JD9dcEn](https://discord.gg/by5JD9dcEn) — the CastleMiner Z modding community server. Active dev work, mod releases, help with RGH setup.
- **GitHub Issues:** for bugs in the pipeline, ModAPI, or STFS CLI.
- **Pull requests welcome:** especially for ModAPI extensions, deploy script improvements, and documentation fixes.

---

## Credits

Built on top of:

- [EclipseKatrina's CastleMiner Z decompilation](https://github.com/EclipseKatrina/CastleMiner-Z-Decompilation) (original 1.4.4 reference)
- [Velocity](https://github.com/hetelek/Velocity) (STFS library)
- [ILSpy](https://github.com/icsharpcode/ILSpy) (decompiler used for 1.6.3 source)
- [Botan](https://botan.randombit.net/) (crypto library)

CastleMiner Z is © DigitalDNA Games. This project contains decompiled source for educational and modding purposes only. No game assets are distributed.
