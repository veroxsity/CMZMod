# CastleMiner Z Modding Guide

This is the practical follow-up to `README.md`. The README gets you a working build pipeline; this document covers what to actually edit to make mods.

The guide is organized by mod type, in roughly increasing order of difficulty:

1. [Cosmetic mods (UI text, build marker, version strings)](#1-cosmetic-mods)
2. [Recipe mods (add or change craftables)](#2-recipe-mods)
3. [Stat tweaks (weapon damage, enemy health, balance changes)](#3-stat-tweaks)
4. [Editing existing textures](#4-editing-existing-textures)
5. [Adding new items](#5-adding-new-items)
6. [Adding new blocks](#6-adding-new-blocks)
7. [AI tweaks (enemy behavior, spawn rates)](#7-ai-and-spawn-tweaks)
8. [New enemies (cloning and modifying existing AI types)](#8-new-enemies)
9. [Audio replacement](#9-audio-replacement)
10. [Multiplayer compatibility (the protocol-version trap)](#10-multiplayer-compatibility)
11. [Debugging mods that don't work](#11-debugging)

Throughout this guide, file paths are relative to `C:\Users\Dan\Documents\Programming\CMZMod\` unless otherwise stated.

---

## How the source is organized

A reference map for finding things:

| What you want to change | Where to look |
|---|---|
| Title screen, menus, HUD | `CastleMinerZ\DNA.CastleMinerZ.UI\` |
| Recipes (crafting) | `CastleMinerZ\DNA.CastleMinerZ.Inventory\Receipe.cs` (note the typo: "Receipe") |
| Item definitions | `CastleMinerZ\DNA.CastleMinerZ.Inventory\InventoryItemIDs.cs` and item class files |
| Block types | `CastleMinerZ\DNA.CastleMinerZ.Terrain\BlockTypeEnum.cs` and `BlockType.cs` |
| World generation | `CastleMinerZ\DNA.CastleMinerZ.Terrain.WorldBuilders\CastleMinerZBuilder.cs` |
| Enemy AI | `CastleMinerZ\DNA.CastleMinerZ.AI\` |
| Player class | `CastleMinerZ\DNA.CastleMinerZ\LocalPlayer.cs` |
| Multiplayer | `CastleMinerZ\DNA.CastleMinerZ.Net\` |
| Achievements | `CastleMinerZ\DNA.CastleMinerZ.Achievements\` |
| Engine (rarely modified) | `DNA Common\` |

After every edit: `.\deploy.ps1 -Pack`, FTP, test on console.

---

## 1. Cosmetic mods

The easiest place to start. These prove your pipeline works without risking gameplay-breaking bugs.

### Build marker (already in place)

`CastleMinerZ\DNA.CastleMinerZ.UI\MainMenu.cs`:

```csharp
private const string BuildTag = "CMZMOD DEV BUILD";
```

Change to whatever you want. Useful pattern: tag each test build (`"BUILD 0.3 - recipe test"`) so you know on the title screen which version of your code is running on the console.

### Game version string

The version constant lives near `CastleMinerZGame.cs`. Find it with:

```powershell
cd C:\Users\Dan\Documents\Programming\CMZMod
Get-ChildItem -Recurse -Filter *.cs | Select-String "CastleMiner Z v" | Select Path, LineNumber, Line
```

### Menu items

`CastleMinerZ\DNA.CastleMinerZ.UI\MainMenu.cs` constructor calls `AddMenuItem(...)` for each menu entry. You can rename, reorder, or hide entries. Caveat: each menu item maps to a `MainMenuItems` enum value; if you add a new item without handling its action, clicking it does nothing.

---

## 2. Recipe mods

The fastest way to make a visible gameplay change and confirm the modding pipeline works in-game.

### Where recipes live

`CastleMinerZ\DNA.CastleMinerZ.Inventory\Receipe.cs` (typo'd in the original source, so always reference it as "Receipe" not "Recipe"). Inside this file is a class called `CookBook` which holds a static list of recipes registered with `.Add(...)` calls. Each `.Add` typically takes:

* The output item ID (what you craft)
* The output count
* A list of ingredients (item ID + count)

### Example: free torches

```csharp
// Before:
CookBook.Add(InventoryItemIDs.Torch, 1,
    new RecipeIngredient(InventoryItemIDs.Stick, 1),
    new RecipeIngredient(InventoryItemIDs.Coal, 1));

// After (1 stick, no coal):
CookBook.Add(InventoryItemIDs.Torch, 1,
    new RecipeIngredient(InventoryItemIDs.Stick, 1));
```

### Recommended first mod

Make torches craftable from 1 stick (no coal). Build, deploy, test. If you see the change in-game (open inventory, check torch recipe), the pipeline is proven end-to-end and you can mod with confidence.

### Recipe ideas

* Lower craft costs across the board for sandbox/creative play
* Add recipes for items that aren't craftable in vanilla (some weapons, blocks)
* Make exotic resources craftable from common ones

### Tooling note

The recipe list is long and easy to break with a syntax error. After editing, run `.\deploy.ps1` (no `-Pack`) first. This builds without packing, so if you broke something MSBuild tells you immediately and you save 10 seconds vs the full `-Pack` cycle.

---

## 3. Stat tweaks

Stat constants live alongside their classes. Look for hardcoded numbers in:

| Stat | File |
|---|---|
| Player max health | `LocalPlayer.cs` (search for `MaxHealth` or `100f`) |
| Player movement speed | `LocalPlayer.cs` (search for `WalkSpeed`, `RunSpeed`) |
| Weapon damage | `CastleMinerZ\DNA.CastleMinerZ.Inventory\` weapon class files |
| Enemy max health | `CastleMinerZ\DNA.CastleMinerZ.AI\` enemy class files |
| Mining speed | `CastleMinerZ\DNA.CastleMinerZ.Inventory\` pickaxe/tool files |
| Day/night cycle length | `CastleMinerZGame.cs` or related world clock files |

### Pattern: find with grep

```powershell
cd C:\Users\Dan\Documents\Programming\CMZMod\CastleMinerZ
Get-ChildItem -Recurse -Filter *.cs | Select-String "MaxHealth\s*=" | Select Path, LineNumber, Line
```

This pattern works for any constant: `Damage`, `Speed`, `SpawnRate`, etc.

### Watch out for

* **Magic numbers without names**: e.g. `health -= 10f;` in damage calculation. Hard to find by name; you have to read the surrounding code.
* **Tuning by playing**: a 2x damage buff sounds great until everything dies in one hit. Test in-game before declaring a value "right."
* **Multiplayer desync**: see section 10. If players have different stat values, the game state desyncs, which manifests as weird teleports/rubberbanding.

---

## 4. Editing existing textures

Texture editing is genuinely doable but it has a specific workflow you have to set up once. Once it works, you're set.

### Understanding the .xnb format

CMZ ships every texture as a `.xnb` file in the `Content\` folder. The `.xnb` format is XNA's compiled-content format. It contains the raw pixel data plus metadata (mipmaps, color format, dimensions) optimized for the runtime to load directly into GPU memory. **You cannot open a .xnb in Photoshop**. Editing requires a three-step process:

1. **Decompile** the `.xnb` to extract the underlying PNG
2. **Edit** the PNG in any image editor
3. **Recompile** the PNG back to a `.xnb`

### Tools you need

The community standard is **xnbcli** (https://github.com/LeonBlade/xnbcli). It's a Node.js command-line tool that handles decompile/recompile in both directions for textures, and it works without the full XNA Content Pipeline. Install Node.js if you don't have it, then:

```cmd
cd C:\Users\Dan\Documents\Programming
git clone https://github.com/LeonBlade/xnbcli.git
cd xnbcli
npm install
```

Now `xnbcli` is ready to use from `C:\Users\Dan\Documents\Programming\xnbcli\`.

Alternative tools exist (XNB Extract, XNB Reverter) but xnbcli is the most actively maintained and has the simplest install.

### Workflow: replace a texture

Using the torch icon as the example because it's small and easy to spot in-game:

```powershell
# 1. Find the source .xnb
$source = "C:\Users\Dan\Documents\Programming\CMZMod\cmz_extracted\584E07D1\Content\Torch.xnb"

# 2. Back it up first. Always.
Copy-Item $source "$source.backup"

# 3. Decompile to PNG
cd C:\Users\Dan\Documents\Programming\xnbcli
.\xnbcli.exe unpack $source .\workspace
# Produces .\workspace\Torch.png and .\workspace\Torch.json

# 4. Edit .\workspace\Torch.png in any image editor

# 5. Recompile back to .xnb
.\xnbcli.exe pack .\workspace .\workspace_packed
# Produces .\workspace_packed\Torch.xnb

# 6. Replace the original
Copy-Item .\workspace_packed\Torch.xnb $source -Force

# 7. Build and deploy
cd C:\Users\Dan\Documents\Programming\CMZMod
.\deploy.ps1 -Pack
```

If your modified torch shows up correctly in-game, texture modding works. From there you can replace anything.

### Texture difficulty by category

| Texture type | Difficulty | Notes |
|---|---|---|
| Item/weapon icons | Easy | Single small textures, xnbcli round-trips cleanly |
| UI elements (menus, HUD) | Easy | Same as above |
| Skybox textures | Easy | Standard 2D textures |
| Avatar/character body textures | Medium | Need to know which file maps to which character |
| Block texture atlas | Hard | Single large file, all blocks share it, see below |
| Font sprite sheets | Hard | Sprite sheet plus kerning data, regenerating is involved |

### The block texture atlas (hard)

`Content\Terrain\Textures.xnb` is the master block atlas. It is a single ~2.7 MB file containing every block's texture as tiles in a grid. Each block face has UV coordinates pointing to its tile inside the atlas.

**You can modify existing tiles** (edit pixels in-place, leave UVs alone). That works straightforwardly with xnbcli. Decompile the atlas to PNG, paint over a tile, recompile.

**You cannot easily add new tiles**. Adding a new block with a new texture means extending the atlas and registering new UV coordinates in the C# code. The atlas dimensions are constrained to power-of-two sizes (the GPU expects this), so you can't just append rows.

**Strong recommendation when you start:** modify existing block textures rather than adding new ones. You can completely retexture the entire game without ever extending the atlas.

### Things that will go wrong

* **Texture appears corrupted in-game**: usually means the format got mangled during repack. Some textures use DXT5 compression, some use plain RGBA. xnbcli normally handles this but you can force a specific format if needed; check its `--format` flag.
* **Texture appears at wrong size**: you saved the PNG at different dimensions than the original. Keep dimensions identical to the source unless you know the engine handles dynamic sizes for that asset (most don't).
* **Game crashes loading the level**: malformed `.xnb` header. Diff your output against the backup with a hex tool and look for differences in the first 16 bytes.
* **Forgot to back up the original**: re-extract from your retail STFS via `setup_assets.ps1` and you have a fresh copy.

### Practical first texture mod

Pink torches:

1. Run the workflow above on `Torch.xnb`
2. Open the PNG in MS Paint, pick a pink, fill the flame
3. Repack, deploy, test
4. If torches glow pink in-game, you have a working texture pipeline

After that, scale up to anything you want.

---

## 5. Adding new items

This is genuinely a multi-step process and the order matters. Done right, your first item takes a couple of hours. Done wrong, it takes a couple of days. Read the whole section before starting.

### The conceptual model

Every item in CMZ has all of these:

* **An ID** (an integer in the `InventoryItemIDs` enum)
* **A class** that defines its behavior (extends `InventoryItem` or one of its derivatives like `WeaponInventoryItem`, `BlockInventoryItem`, `Pickaxe`, etc.)
* **A registration** somewhere that maps the ID to a class instance
* **A texture** (icon shown in inventory and held in hand)
* **Optionally: a recipe** (how to craft it, see section 2)

Each of these lives in a different place in the codebase. Adding an item means touching all of them. Skip one and the item silently breaks.

### Step-by-step walkthrough: adding a "Diamond Pickaxe"

This example assumes you want a pickaxe that mines twice as fast as iron with three times the durability. We'll build it up incrementally so each step is testable.

#### Step 1: Add the enum value

`CastleMinerZ\DNA.CastleMinerZ.Inventory\InventoryItemIDs.cs`:

```csharp
public enum InventoryItemIDs : int
{
    // ... a long list of existing IDs ...
    IronPickaxe = 17,
    SteelPickaxe = 18,
    // ... more existing items ...

    // ADD AT THE END to avoid breaking save compatibility:
    DiamondPickaxe = 999,
}
```

**Critical: add at the end, not in the middle**. Reordering existing IDs corrupts every existing save file because the integer values get serialized into save data. Always append.

#### Step 2: Find the parent class

Look at how `IronPickaxe` is defined. Search:

```powershell
cd C:\Users\Dan\Documents\Programming\CMZMod\CastleMinerZ
Get-ChildItem -Recurse -Filter *.cs | Select-String "class IronPickaxe" | Select Path, LineNumber
```

You'll find a class file (probably `Pickaxe.cs` or `IronPickaxe.cs`) showing the inheritance pattern. It will look something like:

```csharp
public class IronPickaxe : Pickaxe
{
    public IronPickaxe() : base(InventoryItemIDs.IronPickaxe)
    {
        MiningSpeed = 3f;
        MaxDurability = 250;
        IconTexture = "IronPickaxe";  // points at Content\IronPickaxe.xnb
    }
}
```

The exact field names will differ in real CMZ; this is illustrative. Read the actual file to learn what properties you can set.

#### Step 3: Create your class

Same folder as `IronPickaxe.cs`, new file `DiamondPickaxe.cs`:

```csharp
namespace DNA.CastleMinerZ.Inventory
{
    public class DiamondPickaxe : Pickaxe
    {
        public DiamondPickaxe() : base(InventoryItemIDs.DiamondPickaxe)
        {
            MiningSpeed = 6f;          // 2x iron's speed
            MaxDurability = 800;       // ~3x iron's durability
            IconTexture = "IronPickaxe";  // reuse iron's icon for now
        }
    }
}
```

Our csproj uses `<Compile Include="**\*.cs" />` so it auto-picks up the new file. No need to manually add anything.

**Note about `IconTexture = "IronPickaxe"`**: we're deliberately reusing the existing texture as a first step. The goal here is to test the *logic* of adding an item without also debugging texture loading. Once the diamond pickaxe works in-game (looks like an iron pickaxe but mines faster), we'll replace the texture.

#### Step 4: Register the item with the factory

There is almost certainly an item factory or registry somewhere that maps `InventoryItemIDs` enum values to class instances. The game uses this to construct items from saved data, network messages, drops, etc.

Find it by searching for where `IronPickaxe()` is constructed in code that's not the class itself:

```powershell
cd C:\Users\Dan\Documents\Programming\CMZMod\CastleMinerZ
Get-ChildItem -Recurse -Filter *.cs | Select-String "new IronPickaxe\(\)" | Select Path, LineNumber, Line
```

This will show every `new IronPickaxe()` call. One of them is the registry: a switch statement, a dictionary, or a list of all items. Add yours alongside:

```csharp
case InventoryItemIDs.DiamondPickaxe:
    return new DiamondPickaxe();
```

This is the step most likely to be missed and silently break your item. Without it, the recipe might exist but trying to craft it produces nothing because the factory doesn't know how to build the class from the ID.

#### Step 5: Add a recipe

`Receipe.cs`:

```csharp
CookBook.Add(InventoryItemIDs.DiamondPickaxe, 1,
    new RecipeIngredient(InventoryItemIDs.Diamond, 3),
    new RecipeIngredient(InventoryItemIDs.Stick, 2));
```

Diamond and Stick already exist as item IDs in vanilla CMZ.

#### Step 6: Build, deploy, test

```powershell
.\deploy.ps1 -Pack
```

FTP, launch, open inventory crafting, look for the diamond pickaxe recipe. Craft it. Use it on stone. If mining is faster, your item works.

#### Step 7: Custom texture (optional, after the logic is proven)

Once the item works with a borrowed texture, give it its own:

1. Copy `cmz_extracted\584E07D1\Content\IronPickaxe.xnb` to `cmz_extracted\584E07D1\Content\DiamondPickaxe.xnb`
2. Use the workflow from section 4 to retexture it (decompile, edit PNG, recompile)
3. Update `IconTexture = "DiamondPickaxe";` in `DiamondPickaxe.cs`
4. Rebuild and test

### Things that will go wrong

| Symptom | Likely cause | Fix |
|---|---|---|
| Item doesn't appear in recipe list | Recipe wasn't added or has a typo | Re-check `Receipe.cs`, look for compile errors |
| Recipe shows but crafting produces nothing | Factory registration (step 4) is missing | Find the registry, add the case |
| Item appears but crashes when used | Missing required overrides on parent class | Check what abstract methods `Pickaxe` requires |
| Item shows default/missing texture | `IconTexture` value doesn't match a `.xnb` filename | Verify file exists at `Content\<IconTexture>.xnb` |
| Items vanish from saved worlds when reloading | Reordered existing enum values | Don't reorder; only append. Restore enum, start a new world |
| Multiplayer kicks you out | Custom items fail vanilla protocol check | Bump protocol version intentionally (section 10) |

### A more conservative first item

Don't start with a custom pickaxe. **Start by cloning an existing item with no behavior changes**. Add `MyTestItem` that's exactly an `IronPickaxe` clone with a different ID and the same stats. Get that to appear in the recipe book and work in-game. Once that works, you've proven the pipeline. *Then* iterate to add real changes (different stats, different texture, different name).

The mistake most people make is trying to add a complex new item (new model, new stats, new texture, new sound) on day one. Each of those is a separate failure mode. **One axis of change per attempt.**

---

## 6. Adding new blocks

Conceptually similar to adding items, but harder because of the texture atlas constraint.

### Block registry

`CastleMinerZ\DNA.CastleMinerZ.Terrain\BlockTypeEnum.cs` defines all block IDs. `BlockType.cs` registers their properties: hardness, drop, what tool mines them, render type (cube, plant, fluid, etc.).

Adding a block follows the same pattern as adding an item:

1. Append a new value to `BlockTypeEnum`
2. Register it in `BlockType.cs` with its properties
3. (Hard) Add a texture to the atlas, OR reuse an existing UV slot

### The texture atlas problem

`Content\Terrain\Textures.xnb` is the master block atlas. Adding a new block with a new texture is the hardest single task in CMZ modding.

**Easier approach: reuse existing UV coordinates.** Make your new block use the dirt texture, the stone texture, etc. The `BlockType` definition tells each block which atlas tiles to use for its faces. You can register a new block that points at existing tiles and skip the atlas modification entirely.

**Hard approach: extend the atlas.** Decompile `Textures.xnb` to PNG, add new tile rows, recompile, then update C# code to know about the new UV coordinates. Doable but expect a weekend of trial and error.

### Easier intermediate step

Tweak existing blocks instead of adding new ones. Make stone harder to mine, change which tool drops what, change drop tables. All editable in `BlockType.cs` registration without touching the atlas.

---

## 7. AI and spawn tweaks

### Enemy spawn rate

In the world builder or a spawn manager class. Search:

```powershell
cd C:\Users\Dan\Documents\Programming\CMZMod\CastleMinerZ
Get-ChildItem -Recurse -Filter *.cs | Select-String "SpawnRate|spawnTime|MaxEnemies"
```

### Enemy behavior

Each enemy has a class in `DNA.CastleMinerZ.AI\`. Look at:

* `Zombie.cs`: basic melee chaser, simplest AI to learn from
* `Skeleton.cs` / `SkeletonArcher.cs`: ranged AI
* `Dragon.cs`: flying / complex AI
* `Demon.cs`, `Alien.cs`: more complex behaviors

Modifying `Update()` methods changes behavior. Common tweaks: aggression range, attack damage, attack speed, projectile speed.

### Pattern: make zombies less aggressive

Find the aggro range in `Zombie.cs` (often a constant like `_aggroDistance = 30f`). Lower it, rebuild, test in-game.

---

## 8. New enemies

Same pattern as new items: clone an existing class, give it a new ID, register it. The trickier parts:

* **Spawn registration**: a spawn manager picks which enemy to spawn based on biome/depth/conditions. You need to add your new enemy to that selection logic, otherwise it never appears in the world.
* **Models and textures**: reuse existing ones initially. Custom 3D models require XNA Content Pipeline knowledge (out of scope for this guide).
* **Animations**: look at `Content\Enemies\<EnemyType>\` for animation `.xnb` files to know what animation states are expected to exist.

---

## 9. Audio replacement

Audio is harder than textures and worth its own honest section.

### How CMZ audio works

`Content\Sounds.xgs`, `Sounds.xsb`, `Sounds.xwb`, and `SoundsStreaming.xwb` are the four files that make up the XACT audio bundle. XACT is XNA's audio system; the four files together hold all the sound effects, music cues, and audio metadata.

* `.xgs`: the global settings (master volumes, categories)
* `.xsb`: the sound bank (cue definitions, what plays when triggered)
* `.xwb`: the wave bank (actual audio data, often compressed)
* `SoundsStreaming.xwb`: streaming audio (longer tracks loaded on demand)

### What works

To **modify a single sound effect**, the workflow is:

1. Extract the `.xwb` wave bank to individual `.wav` files using a tool like **XACTPlay** or **xwbtool**
2. Replace one of the WAVs with your custom audio (matching sample rate and format)
3. Recompile the wave bank using the **XACT3 Audio Authoring Tool** from the Xbox 360 SDK
4. Place the new `.xwb` in `Content\` and rebuild

This is the official supported workflow but it requires the XACT3 GUI tool, which is part of the Xbox 360 SDK install.

### What's hard

* **Adding new sounds** (not just replacing): you need to define new cues in the sound bank, which means rebuilding the entire `.xsb` from a XACT project file. Without a project file from DigitalDNA Games (which we don't have), this means reverse-engineering the cue layout.
* **Streaming music**: replacing tracks in `SoundsStreaming.xwb` requires the same XACT tool.

### Honest take

Audio modding is **considerably harder than texture or code modding**. If you're early in your modding journey, **skip audio entirely** and focus on code-only mods (recipes, stats, behaviors) and texture replacement. Even ambitious mods can go very far without ever touching audio. Coming back to audio after you've shipped a few code mods is fine; trying to learn XACT alongside everything else is a recipe for burnout.

---

## 10. Multiplayer compatibility

CMZ has multiplayer over Xbox Live. **Your modded build is compatible with vanilla retail clients only by accident.** Once you change game logic, you can desync or be kicked.

### What stays compatible

Cosmetic-only mods (build marker, menu text). The protocol doesn't transmit visible UI state.

### What desyncs

Anything that affects game state: recipes, stats, items, blocks. If your modded client thinks an enemy has 200 HP and the host thinks it has 100, the game gets confused.

### What gets you kicked

There's typically a protocol version constant somewhere in `DNA.CastleMinerZ.Net\`. If you change it (intentionally or not), retail clients refuse to connect.

### Recommendations for modded multiplayer

* **Bump the protocol version intentionally** to declare your mod incompatible with vanilla. This prevents accidental "join with random retail player who then crashes" scenarios.
* **Distribute your mod to friends so they all run the same version.** Multiplayer between modded clients works fine *if they're running identical code*.
* **Test single-player first.** Always.

---

## 11. Debugging

### "It crashes on my console"

The dashboard shows a generic error. To get more detail:

* **Check FreeStyle Dash trace logs**: FSD logs unhandled exceptions to a file on the HDD. Path varies but typically `Hdd1:\Aurora\Data\` or `Hdd1:\Plugins\`. Pull the latest log via FTP and read it.
* **Check System Update logs**: sometimes `Hdd1:\Cache\Microsoft\xbox360.lpf` records launch failures with diagnostic codes.

### "It builds but doesn't show my change in-game"

99% of the time this is a stale build problem:

1. Did `.\deploy.ps1 -Pack` actually run successfully? (Look for `[+] Done` at the end.)
2. Did the FTP upload actually replace the file? In WinSCP, check the file timestamp on the console matches what you just uploaded.
3. Is the dashboard showing your build marker? If not, your STFS isn't loading and the dashboard is running an older copy. Delete leftover loose files in `Hdd1:\Content\0000000000000000\584E07D1\` and re-upload only your `CMZModded.stfs`.
4. Did you rebuild after editing? `.\deploy.ps1 -Pack` does a fresh build automatically, but worth confirming MSBuild output shows your file got recompiled.

### "It used to work, now it doesn't"

Common causes, in order of frequency:

1. You renamed a variable that's referenced elsewhere. Check the build output for compile errors you might have missed.
2. You modified a serialized class (one with `[Serializable]` attribute or one used in save files). Save files from before the change won't load. Start a new world for testing.
3. You broke the wildcard csproj by adding a `.cs.bak` or `.cs.old` file. Our csproj globs `**\*.cs` so any file with `.cs` extension gets compiled. Remove the backup or rename it to a different extension.

### "deploy.ps1 -Pack succeeds but the STFS won't load on console"

Run `stfs-cli\stfs_list.exe CMZModded.stfs > listing.txt` and compare against retail. The retail STFS should have 437 entries. If your modded one has fewer, files are missing from `cmz_extracted\` and you need to re-run `setup_assets.ps1`.

### Iterative loop tips

* Keep the title screen build marker version-stamped (`"BUILD 0.7"` etc.) so you can confirm at a glance which version is running.
* Take a screenshot or photo of the screen each time something works. When a later build breaks it, you can compare.
* Commit working states to git before risky changes. `git checkout` is faster than re-deriving lost code.

---

## Appendix: useful PowerShell snippets

### Find all hardcoded constants of a given name

```powershell
Get-ChildItem -Recurse -Path C:\Users\Dan\Documents\Programming\CMZMod\CastleMinerZ -Filter *.cs |
    Select-String "MaxHealth|Damage|SpawnRate" |
    Select Path, LineNumber, Line | Format-List
```

### Quick grep across the whole codebase

```powershell
Get-ChildItem -Recurse -Path C:\Users\Dan\Documents\Programming\CMZMod -Filter *.cs |
    Select-String "your search term"
```

### Compare your build's binaries against vanilla

```powershell
$vanilla = "C:\Users\Dan\Documents\Programming\CMZMod\cmz_extracted\584E07D1"
$modded  = "C:\Users\Dan\Documents\Programming\CMZMod\deploy\584E07D1"
Compare-Object `
    (Get-FileHash $vanilla\CastleMinerZ.exe).Hash `
    (Get-FileHash $modded\CastleMinerZ.exe).Hash
```

If hashes match, you're somehow still shipping vanilla; investigate.

### Find which file contains an item ID

```powershell
Get-ChildItem -Recurse -Path C:\Users\Dan\Documents\Programming\CMZMod\CastleMinerZ -Filter *.cs |
    Select-String "InventoryItemIDs\.IronPickaxe" |
    Select Path, LineNumber
```

Useful when learning the codebase: shows every place a specific item is referenced (factory, recipes, drops, etc.).

---

## Where to go next

* **Start with section 1 or 2.** Those are guaranteed wins and give you the dopamine hit of seeing your change in-game.
* **Build up to section 4 (textures) and section 5 (new items)** once you're comfortable with the build/test loop.
* **Treat sections 6, 8, and 9 as long-term goals.** They require deeper toolchain knowledge and have more failure modes.

The most common mistake when starting CMZ modding is trying to do too much at once. **One change per build cycle.** Test, confirm, commit, repeat. When you have a half-dozen working small mods, combining them into a unified release is straightforward.

Good luck.
