# CMZMod Framework: Modding Guide

This guide covers modding CastleMiner Z using the CMZMod modding framework. Instead of editing the game source directly, you write a mod as a folder of C# files using the `ModAPI`, drop it into `mods/`, and build. The source tree stays untouched.

For direct source editing (advanced, more dangerous, but unlimited in scope), see `source_modding.md`.

---

## Quick start: your first mod (5 minutes)

The fastest way in is to copy one of the example mods:

```powershell
# From the repo root
Copy-Item -Recurse mods-examples\cheap-torches mods\cheap-torches
.\deploy.ps1 -Pack
```

That's it. The pipeline picks up the mod automatically. FTP `CMZModded.stfs` to `Hdd1:\Content\0000000000000000\584E07D1\`, launch the game, open inventory, and torches now cost 1 stick (no coal).

If you'd rather write it yourself, here's what `cheap-torches` looks like:

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
            // Replace torch recipe: 1 stick produces 4 torches (no coal)
            Recipes.Modify(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick);
        }
    }
}
```

Build and deploy:

```powershell
.\deploy.ps1 -Pack
```

---

## How it works

The framework introduces three layers:

```
deploy.ps1 -Pack
    │
    ├── CastleMinerZ\ModAPI\        The API mods use (sealed, shipped with framework)
    ├── mods\your-mod\              Your C# files + mod.json
    └── build_temp\                 Copy of source + mods, compiled together
                                           │
                                           ▼
                                    CMZModded.stfs
```

- **ModAPI** is a set of static C# classes in `CastleMinerZ\ModAPI\`. Mods call these to change recipes, register items, tweak stats, and so on.
- **Hooks** are surgical insertions in the engine source that connect ModAPI calls to the game at runtime.
- **The build pipeline** copies the entire game source plus your mod files into `build_temp/`, auto-generates a registry that wires up mod entry points, compiles, and packs the result. The real source tree is never touched.

Running `.\deploy.ps1 -Pack` with an empty (or missing) `mods/` directory produces a vanilla build, byte-for-byte identical to building without the framework. The framework adds zero overhead when no mods are loaded.

---

## Mod structure

Every mod is a folder in `mods/`:

```
mods/
└── your-mod-name/
    ├── mod.json         (required)
    ├── YourMod.cs       (required: your entry point)
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
- **Attribute**: `[Mod(Id = "your.mod-id")]`. The `Id` must match `mod.json`
- **Method**: `public static void OnLoad()`. Called once at game startup
- **Namespace**: anything you want (pick something distinctive so it doesn't collide with another mod)

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

You can have multiple `[Mod]` classes in one mod folder if you want to split work logically. Each `OnLoad()` runs in folder-sort order.

---

## Multi-file mods

A mod can have any number of `.cs` files in its folder. All of them get compiled together:

```
mods/fast-zombies/
├── mod.json
├── FastZombiesMod.cs      # entry point with [Mod]
└── ZombieConfig.cs        # helper classes, constants, etc.
```

Files from other mod folders are isolated. They don't see each other's code at compile time. The only common surface area is the ModAPI itself, plus the public game types it exposes.

---

## Load order

Folder names determine load order. Prefix with numbers for control:

```
mods/
├── 00-core-fixes\        loads first
├── 30-new-weapons\       loads second
└── 99-custom-zombies\    loads last
```

Later mods can override changes made by earlier ones (e.g. last `Recipes.Modify` for the same item wins).

---

## API reference

All ModAPI types live in the `DNA.CastleMinerZ.ModAPI` namespace.

### The `Mod` attribute

Marks a class as a mod entry point.

```csharp
[Mod(Id = "you.my-mod", Name = "My Mod", Version = "1.0.0")]
public static class MyMod { ... }
```

- `Id`: must match the `id` in `mod.json`
- `Name`: display name (currently used for logging)
- `Version`: free-form version string (semver recommended)

### `ModLog`

Write diagnostic output. Visible on the Xbox 360 dev console output and in FreeStyle/Aurora trace logs:

```csharp
ModLog.Info("Loaded 12 custom recipes");
ModLog.Warn("Could not find vanilla item to replace");
ModLog.Error("Skipping malformed config");
```

### `Recipes`

Modify the crafting recipe system.

#### Add a new recipe

```csharp
// Craft 4 torches from 1 stick + 1 coal
Recipes.Add(InventoryItemIDs.Torch, 4,
    InventoryItemIDs.Stick,
    InventoryItemIDs.Coal);
```

Overloads exist for adding recipes that produce *custom* items (registered via `Items.Register`), referenced by their string ID:

```csharp
// Custom item as output, vanilla ingredients
Recipes.Add("you.diamond-sword", 1,
    InventoryItemIDs.Diamond,
    InventoryItemIDs.Stick);

// Custom item as output and as ingredient
Recipes.Add("you.upgraded-sword", 1,
    "you.diamond-sword",
    "you.magic-dust");
```

#### Remove all recipes for an item

```csharp
Recipes.Remove(InventoryItemIDs.TNT);   // vanilla item
Recipes.Remove("you.diamond-sword");    // custom item
```

#### Replace an existing recipe

```csharp
// Diamond pickaxe: 3 diamonds + 2 sticks
Recipes.Modify(InventoryItemIDs.DiamondPickAxe, 1,
    InventoryItemIDs.Diamond,
    InventoryItemIDs.Stick);
```

Equivalent to `Remove` followed by `Add`, but expresses intent more clearly.

#### Wipe everything

```csharp
Recipes.Clear();
// Re-add only what you want
Recipes.Add(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick);
```

Use sparingly. Wiping all vanilla recipes will surprise players who expect the normal crafting tree.

### `Items`

Register new items, or modify existing ones.

#### Register a custom item

```csharp
Items.Register("you.diamond-sword", new ItemDef {
    DisplayName    = "Diamond Sword",
    Description1   = "A blade of pure diamond",
    Description2   = "Devastates undead in melee combat",
    IconTextureName = "DiamondLaserSword",
    MaxStackSize   = 1,
    BehaviorClass  = ItemBehaviors.Sword,
    EnemyDamage    = 25f,
    EnemyDamageType = DamageType.BLADE,
    CoolDownTime   = TimeSpan.FromMilliseconds(400),
    IsMeleeWeapon  = true,
});
```

The ID string (`"you.diamond-sword"` here) is how the item is referenced from recipes and from other mods. Use the `author.name` convention to avoid collisions.

`IconTextureName` is the name of an existing `.xnb` in the game's `Content/` folder, without the extension. The example above reuses the vanilla diamond laser sword icon. Adding wholly new textures requires direct asset modding (see `source_modding.md`).

`BehaviorClass` selects how the item acts in-game. See `ItemBehaviors` below for the available choices.

See the **`ItemDef` reference** further down for every field you can set.

#### Tweak vanilla items in place

```csharp
// Buff the damage on the assault rifle
Items.SetEnemyDamage(InventoryItemIDs.AssultRifle, 15f);

// Change how often a tool can be swung
Items.SetCooldown(InventoryItemIDs.IronPickAxe, TimeSpan.FromMilliseconds(250));

// Cap stack size lower
Items.SetMaxStack(InventoryItemIDs.Grenade, 4);

// Change durability cost per use
Items.SetSelfDamagePerUse(InventoryItemIDs.IronPickAxe, 0.5f);

// Rename in the UI
Items.SetDisplayName(InventoryItemIDs.Torch, "Magic Torch");

// Change the description (two lines)
Items.SetDescription(InventoryItemIDs.Torch,
    "Burns forever", "Drops from skeletons");
```

#### Mutate a custom item after registration

```csharp
Items.Modify("you.diamond-sword", def => {
    def.EnemyDamage = 30f;
    def.CoolDownTime = TimeSpan.FromMilliseconds(300);
});
```

Useful for a "balance patch" mod loaded after the mod that originally registers the item.

#### Look one up

```csharp
ItemDef def = Items.Get("you.diamond-sword");
if (def != null) {
    ModLog.Info("Found " + def.DisplayName);
}
```

### `ItemBehaviors`

The set of behavior classes you can assign to a custom item via `ItemDef.BehaviorClass`:

| Property | What it gives you |
|---|---|
| `ItemBehaviors.Sword` | Melee weapon. Swings, hits enemies in front. |
| `ItemBehaviors.PickAxe` | Mines blocks. Speed governed by item stats. |
| `ItemBehaviors.Spade` | Digs softer terrain. |
| `ItemBehaviors.Axe` | Chops trees/wood blocks. |
| `ItemBehaviors.Block` | Placeable block (uses item's icon as placement preview). |
| `ItemBehaviors.Consumable` | Single-use item (cooldown then consumed). |

```csharp
BehaviorClass = ItemBehaviors.Sword
```

Each behavior wires up the correct in-game systems (animation, damage application, mining speed, etc.) without you needing to write any of that code yourself.

### `ItemDef` reference

The data class that describes a custom item. All fields are public properties you can set in the object initializer.

| Field | Type | Notes |
|---|---|---|
| `DisplayName` | `string` | Shown in inventory and hotbar |
| `Description1` | `string` | First line of in-inventory description |
| `Description2` | `string` | Second line (optional, can be empty) |
| `IconTextureName` | `string` | `.xnb` name (without extension) from `Content/` |
| `MaxStackSize` | `int` | Default 100. Use 1 for weapons/tools. |
| `BehaviorClass` | `Type` | One of `ItemBehaviors.*`. Required. |
| `EnemyDamage` | `float` | Damage per hit (weapons/consumables) |
| `EnemyDamageType` | `DamageType` | `BLUNT`, `BLADE`, `BULLET`, etc. |
| `CoolDownTime` | `TimeSpan` | Minimum time between uses |
| `ItemSelfDamagePerUse` | `float` | Durability decrement per use (tools) |
| `UseSoundCue` | `string` | XACT sound cue triggered on use (optional) |
| `PlayerMode` | `PlayerMode` | Player animation mode while equipped |
| `IsMeleeWeapon` | `bool` | True for swords / axes used as weapons |
| `PickupTimeoutLength` | `float` | Seconds before a dropped item despawns |
| `LaserColor` | `Color` | Sword trail / laser color (defaults to blue) |
| `SerializeCustomData` | `Func<...>` | Advanced: per-item save data writer |
| `DeserializeCustomData` | `Func<...>` | Advanced: per-item save data reader |

### `IItemId`, `VanillaItemId`, `ModItemId`

The framework uses these wrappers internally so vanilla items (referenced by `InventoryItemIDs` enum) and mod items (referenced by string) can be passed to the same APIs. You normally don't construct these directly. `Recipes.Add` and friends have overloads that take either form. If you ever need to handle both in your own helper code:

```csharp
IItemId id = new ModItemId("you.diamond-sword");
if (id.IsVanilla) {
    InventoryItemIDs v = id.VanillaId;
} else {
    string m = id.ModId;
}
```

---

## Walkthrough: a custom item (diamond sword)

A full working version of this is in `mods-examples/diamond-sword/`. Copy it to `mods/diamond-sword/` and build if you just want to see it run.

To build it from scratch, create the folder structure first:

```
mods/
└── diamond-sword/
    ├── mod.json
    └── DiamondSwordMod.cs
```

**`mod.json`**:

```json
{
    "id": "you.diamond-sword",
    "name": "Diamond Sword",
    "author": "Your Name",
    "version": "1.0.0",
    "description": "A craftable diamond sword that hits harder than vanilla weapons",
    "modapi_version": "1"
}
```

**`DiamondSwordMod.cs`**:

```csharp
using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;

namespace YouDiamondSword
{
    [Mod(Id = "you.diamond-sword", Name = "Diamond Sword", Version = "1.0.0")]
    public static class DiamondSwordMod
    {
        public static void OnLoad()
        {
            Items.Register("you.diamond-sword", new ItemDef {
                DisplayName     = "Diamond Sword",
                Description1    = "A blade of pure diamond",
                Description2    = "Devastates undead in melee combat",
                IconTextureName = "DiamondLaserSword",
                MaxStackSize    = 1,
                BehaviorClass   = ItemBehaviors.Sword,
                EnemyDamage     = 25f,
                EnemyDamageType = DamageType.BLADE,
                CoolDownTime    = TimeSpan.FromMilliseconds(400),
                IsMeleeWeapon   = true,
            });

            Recipes.Add("you.diamond-sword", 1,
                InventoryItemIDs.Diamond,
                InventoryItemIDs.Stick);

            ModLog.Info("Diamond Sword registered");
        }
    }
}
```

Build:

```powershell
.\deploy.ps1 -Pack
```

FTP, launch, open the crafting menu. The diamond sword should appear in the recipe list. Craft it (you'll need a diamond and a stick), equip it, swing it at a zombie. Hit damage should match what you set.

### Iterating on the item

Common follow-ups:

- **Tune damage / cooldown.** Rebuild after every change. Test against the same enemy each time for consistent comparison.
- **Swap the icon.** Replace `IconTextureName` with another `.xnb` name from `Content/`. Discover available names by listing the folder:
  ```powershell
  Get-ChildItem cmz_extracted\584E07D1\Content -Filter *.xnb | Select-Object Name
  ```
- **Use a different behavior.** Change `BehaviorClass = ItemBehaviors.PickAxe` to make the same definition mine blocks instead. Adjust other stats accordingly (`ItemSelfDamagePerUse`, etc.).
- **Make it cheaper / more expensive.** Change the `Recipes.Add(...)` ingredients.

For a fully custom texture (your own art rather than reusing a vanilla icon), see the texture section in `source_modding.md`. That part still requires direct asset modding.

---

## Common item IDs

Items are referenced by `InventoryItemIDs` enum values: the same identifiers used in the game's own recipe definitions. The most useful ones:

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
| `InventoryItemIDs.AssultRifle` | Assault Rifle (yes, typo'd in source) |
| `InventoryItemIDs.Grenade` | Grenade |
| `InventoryItemIDs.TNT` | TNT |

For the full list, open `CastleMinerZ\DNA.CastleMinerZ.Inventory\InventoryItemIDs.cs`. There are around 90 entries.

---

## Combining mods

Multiple mods work together automatically. Each mod's `OnLoad()` runs in folder-sort order.

If two mods modify the same recipe, the later one wins:

```csharp
// 00-core-fixes\CoreMod.cs (loads first): cheap torches
Recipes.Modify(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick);

// 99-rebalance\RebalanceMod.cs (loads later): torches back to vanilla
Recipes.Modify(InventoryItemIDs.Torch, 4,
    InventoryItemIDs.Stick, InventoryItemIDs.Coal);
```

Same idea for items: a `Items.Modify(...)` call in a later-loading mod overrides an earlier `Items.Register(...)` or `Items.Modify(...)`.

If two mods register a custom item with the same ID, the later registration wins and a warning is logged. Use distinct `author.name` IDs to avoid this.

---

## Sharing a mod

Zip the mod folder. Recipient unzips into their `mods/` directory, runs `.\deploy.ps1 -Pack`, and FTPs. No registry, no package manager, no metadata server.

Mods are pure-source-form: the recipient compiles them as part of their own build. This makes mods portable across CMZMod versions as long as the ModAPI surface stays compatible (`modapi_version` matches).

---

## Removing a mod

Delete the folder. Run `.\deploy.ps1 -Pack`. The mod is gone from the next build.

---

## Building without mods

Running `.\deploy.ps1` with an empty (or absent) `mods/` directory builds the unmodified game. Zero behavior changes, same output as before the framework existed.

---

## Troubleshooting

### Build errors show the wrong paths

The pipeline rewrites paths so errors reference your `mods/` folder rather than `build_temp/`:

```
mods/cheap-torches/CheapTorchesMod.cs(15,32): error CS0117:
'Recipes' does not contain a definition for 'AddCheap'
```

Open the file at that line, fix, rebuild.

### Mod loads but doesn't do anything

- Check the load log. You should see `[CMZMod] Loaded <mod-id> v<version>` for every successfully loaded mod, written to standard output on the console.
- If a mod throws inside `OnLoad()`, the framework catches it, logs `[CMZMod ERROR] ...`, and disables the mod. The game keeps running.
- Verify the `[Mod]` attribute `Id` matches the `id` in `mod.json` exactly.

### "ModAPI version mismatch"

Your `mod.json` has a different `modapi_version` than the framework supports. Update it to `"1"`.

### Vanilla build still works

Running with an empty `mods/` produces an unchanged game. That's by design, not a bug. If you expected a mod to load, check that:

- The folder is directly under `mods/`, not nested deeper
- It contains a valid `mod.json`
- The `.cs` files compile cleanly (look at the MSBuild output)

### Items.Register succeeds but the item doesn't appear in crafting

You registered the item but didn't add a recipe for it. Add a `Recipes.Add(...)` call in the same `OnLoad`.

### Item appears in inventory with no icon

`IconTextureName` doesn't match an `.xnb` in `Content/`. List the folder to find valid names:

```powershell
Get-ChildItem cmz_extracted\584E07D1\Content -Filter *.xnb | Select-Object Name
```

Match the name exactly (case-sensitive on console, even though Windows is case-insensitive).

### Items.SetEnemyDamage has no effect

Check the item actually exists in vanilla. Some items in `InventoryItemIDs` are placeholders. Look up the item in `InventoryItem.GetClass(item)` to confirm it has a real class before setting properties on it.

### My console crashes on launch after adding a mod

- Run `.\deploy.ps1` without `-Pack` to verify the source compiles.
- Check the load log on the console (FreeStyle/Aurora trace logs). If the crash happens before `[CMZMod] Loaded ...` appears, it's a compile-time issue. If after, it's a runtime issue in your `OnLoad`.
- Try moving your mod folder out of `mods/` and rebuilding. If the vanilla build works, the issue is definitely in the mod.

---

## What the framework doesn't (yet) cover

Things you currently can't do via the framework, and have to drop into direct source editing (`source_modding.md`) for:

- **New textures.** You can name an existing `.xnb`, but adding new ones requires the asset pipeline.
- **New blocks** with new atlas tiles.
- **New enemies** or AI behavior changes.
- **Audio replacement** (XACT bundles).
- **Multiplayer protocol changes.**
- **World generation rules.**
- **Achievements, game modes, network message types.**

These may show up in the framework over time. For now, mod those via direct source editing.

---

Good luck. Start with `mods-examples/cheap-torches/`, get something working in-game, then scale up. The build/test loop is fast enough that iteration is cheap; lean into it.
