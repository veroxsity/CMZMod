# CMZMod Framework — Modding Guide

This guide covers modding CastleMinerZ using the CMZMod modding framework. Instead of editing the game source directly, you write a mod as a folder of C# files using the ModAPI, drop it into `mods/`, and build. The source tree stays untouched.

For direct source editing (advanced), see `MODDING.md`.

---

## Quick start: your first mod (5 minutes)

Create a folder and two files:

```
mods/
└── cheap-torches/
    ├── mod.json
    └── CheapTorchesMod.cs
```

**`mod.json`**:

```json
{
    "id": "example.cheap-torches",
    "name": "Cheap Torches",
    "author": "Your Name",
    "version": "1.0.0",
    "description": "Makes torches craftable from 1 stick (no coal)",
    "modapi_version": "1"
}
```

**`CheapTorchesMod.cs`**:

```csharp
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;

namespace ExampleCheapTorches
{
    [Mod(Id = "example.cheap-torches", Name = "Cheap Torches", Version = "1.0.0")]
    public static class CheapTorchesMod
    {
        public static void OnLoad()
        {
            // Replace torch recipe: 1 stick → 4 torches (no coal)
            Recipes.Modify(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick);
        }
    }
}
```

Build and deploy:

```powershell
.\deploy.ps1 -Pack
```

FTP `CMZModded.stfs` to `Hdd1:\Content\0000000000000000\584E07D1\`. Launch the game — torches now cost 1 stick.

---

## How it works

The framework introduces three layers:

```
Build pipeline (deploy.ps1 -Pack)
    │
    ├── CastleMinerZ\ModAPI\        ← The API mods use (sealed, shipped with framework)
    ├── mods\your-mod\              ← Your C# files + mod.json
    └── build_temp\                 ← Copy of source + mods, compiled together
                                           │
                                           ▼
                                    CMZModded.stfs
```

- **ModAPI** is a set of static C# classes in `CastleMinerZ\ModAPI\`. Mods call these to change recipes, items, etc.
- **Hooks** are surgical insertions in the engine source that connect ModAPI calls to the game at runtime.
- **The build pipeline** copies the entire game source plus your mod files into `build_temp/`, auto-generates a registry that wires up mod entry points, compiles, and packs the result. The real source tree is never touched.

---

## Mod structure

Every mod is a folder in `mods/`:

```
mods/
└── your-mod-name/
    ├── mod.json         (required)
    ├── YourMod.cs       (required — your entry point)
    └── ...              (optional: more .cs files)
```

### mod.json fields

| Field | Required | Description |
|---|---|---|
| `id` | Yes | Unique namespaced ID. Format: `author.mod-name`. Lowercase letters, numbers, dots, dashes only. |
| `name` | Yes | Human-readable display name |
| `author` | Yes | Your name or handle |
| `version` | Yes | Semver version (e.g. `"1.0.0"`) |
| `description` | No | Short description, shown in load log |
| `modapi_version` | Yes | Must be `"1"` (current version) |

### Entry point rules

Your main class must follow these conventions:

- **Class**: `public static`
- **Attribute**: `[Mod(Id = "your.mod-id")]` — the `Id` must match `mod.json`
- **Method**: `public static void OnLoad()` — called once at game startup
- **Namespace**: anything you want

```csharp
[Mod(Id = "example.my-mod", Name = "My Mod", Version = "1.0.0")]
public static class MyMod
{
    public static void OnLoad()
    {
        // Your code here
    }
}
```

You can have multiple `[Mod]` classes in one mod folder. Each `OnLoad()` runs in an order determined by folder sort.

---

## Multi-file mods

A mod can have any number of `.cs` files in its folder — all get compiled in:

```
mods/fast-zombies/
├── mod.json
├── FastZombiesMod.cs      # entry point with [Mod]
└── ZombieConfig.cs        # helper classes, constants, etc.
```

Files from other mod folders are isolated — they don't see each other's code at compile time (only the ModAPI).

---

## Load order

Folder names determine load order. Prefix with numbers for control:

```
mods/
├── 00-core-fixes\        ← loads first
├── 30-new-weapons\       ← loads second
└── 99-custom-zombies\    ← loads last
```

Later mods can override changes made by earlier mods.

---

## ModAPI reference (Phase 1)

Currently available API classes. More will be added in future framework phases.

### `Recipes` — `using DNA.CastleMinerZ.ModAPI;`

Modify the crafting recipe system.

#### `Recipes.Add(result, resultCount, ingredients...)`

Add a new recipe.

```csharp
// Craft 4 torches from 1 stick + 1 coal
Recipes.Add(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick, InventoryItemIDs.Coal);
```

#### `Recipes.Remove(itemId)`

Remove all recipes that produce the given item.

```csharp
// Disable TNT crafting
Recipes.Remove(InventoryItemIDs.TNT);

// Disable all crafting involving logs
Recipes.Remove(InventoryItemIDs.LogBlock);
```

#### `Recipes.Modify(itemId, newResultCount, ingredients...)`

Replace an existing recipe's output count and ingredients. Same as `Remove` + `Add`.

```csharp
// Diamond pickaxe: 3 diamonds + 2 sticks (was 2 diamonds + 3 gold)
Recipes.Modify(InventoryItemIDs.DiamondPickAxe, 1, InventoryItemIDs.Diamond, 3, InventoryItemIDs.Stick, 2);
```

#### `Recipes.Clear()`

Remove all recipes. Use with care — typically before re-adding a custom set.

```csharp
Recipes.Clear();
// Now add your custom recipes
Recipes.Add(...);
```

### Available item IDs

Items are referenced by `InventoryItemIDs` enum values — the same identifiers used in the game's own recipe definitions. Common ones:

| ID | Item |
|---|---|
| `InventoryItemIDs.Torch` | Torch |
| `InventoryItemIDs.Stick` | Stick |
| `InventoryItemIDs.Coal` | Coal |
| `InventoryItemIDs.Iron` | Iron bar |
| `InventoryItemIDs.Gold` | Gold bar |
| `InventoryItemIDs.Diamond` | Diamond |
| `InventoryItemIDs.WoodBlock` | Wood planks |
| `InventoryItemIDs.LogBlock` | Log |
| `InventoryItemIDs.RockBlock` | Cobblestone |
| `InventoryItemIDs.DirtBlock` | Dirt |
| `InventoryItemIDs.StonePickAxe` | Stone pickaxe |
| `InventoryItemIDs.IronPickAxe` | Iron pickaxe |
| `InventoryItemIDs.DiamondPickAxe` | Diamond pickaxe |
| `InventoryItemIDs.Pistol` | Pistol |
| `InventoryItemIDs.AssultRifle` | Assault Rifle |
| `InventoryItemIDs.Grenade` | Grenade |

See `CastleMinerZ\DNA.CastleMinerZ.Inventory\InventoryItemIDs.cs` for the full list.

---

## Combining mods

Multiple mods work together automatically. Each mod's `OnLoad()` runs in folder sort order.

If two mods modify the same recipe, the later one wins (last-loaded-wins):

```csharp
// 00-core-fixes\CoreMod.cs — loads first, sets torch to 1 stick
Recipes.Modify(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick);

// 99-custom-zombies\ZombieMod.cs — loads later, overrides torch back to normal
Recipes.Modify(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick, InventoryItemIDs.Coal);
```

---

## Sharing a mod

Zip the mod folder. Recipient unzips into their `mods/` directory, runs `.\deploy.ps1 -Pack`, FTPs. No registry, no package manager.

---

## Removing a mod

Delete the folder. Run `.\deploy.ps1 -Pack`. The mod is gone from the next build.

---

## Building without mods

Running `.\deploy.ps1` with nothing in `mods/` builds the unmodified game — zero behavior changes, same as before the framework existed.

---

## Troubleshooting

### Build errors show wrong paths

The pipeline rewrites paths so errors reference your `mods/` folder:

```
mods/cheap-torches/CheapTorchesMod.cs(15,32): error CS0117:
'Recipes' does not contain a definition for 'AddCheap'
```

### Mod loads but doesn't do anything

- Check console output for mod load logs (`[CMZMod] Loaded ...`)
- If a mod throws in `OnLoad()`, it's caught and logged — the game doesn't crash, but the mod is disabled
- Verify your `[Mod]` attribute `Id` matches the `id` in `mod.json`

### "ModAPI version mismatch"

Your `mod.json` has a different `modapi_version` than the framework. Update it to match what the framework expects (currently `"1"`).

### Vanilla build still works

Running without `mods/` = unchanged game. This is by design.

---

## What's next

This is Phase 1 of the framework — recipe manipulation only. Future phases will add APIs for:

- **Phase 2**: Custom items (new weapons, tools, consumables)
- **Phase 3**: Custom blocks, gameplay events, data persistence
- **Phase 4**: Custom enemies, world generation, UI, audio
- **Phase 5**: Achievements, cheats, game modes, network messages

Watch the repo and Discord for updates.
