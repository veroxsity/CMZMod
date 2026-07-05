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
    ├── assets/          (optional: PNG textures — see Assets)
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

| API | Purpose |
|---|---|
| `Recipes`, `Items`, `Blocks` | Crafting, items, terrain blocks |
| `Events`, `Data` | Gameplay hooks and per-world persistence |
| `Assets` | Mod PNG textures (icons, enemy reskins, block texture packs) |
| `Audio` | Vanilla sound cue override / trigger |
| `UI` | HUD overlays, main menu, toasts |
| `Entities` | Enemy stats, spawns, custom types |
| `Worldgen` | Ore frequency, custom ores, post-gen structures |

Working examples for every row above live in `mods-examples/`.

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

`IconTextureName` controls the inventory/hotbar icon. Three forms work:

- **Mod PNG** — bare filename if the PNG lives in your mod's `assets/icons/` folder (e.g. `"my-sword"`). Resolved automatically to `your-mod-id/my-sword` at build time.
- **Full mod path** — `"author.mod/my-sword"` when referencing another mod's asset.
- **Vanilla `.xnb`** — name of an existing texture in the game's `Content/` folder, without extension (e.g. `"DiamondLaserSword"`).

See the **`Assets`** section for the folder layout and deploy behavior.

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
| `IconTextureName` | `string` | Hotbar/inventory icon: mod PNG filename, `mod-id/name`, or vanilla `.xnb` name |
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

### `Blocks`

Register new block types in the world. Mod blocks take up slots 200 to 255 in the block type table (vanilla uses 0 to 45, the rest is reserved). You can register up to 56 mod blocks.

#### Register a new block

```csharp
Blocks.Register("you.marble", new BlockDef {
    DisplayName       = "Marble",
    Hardness          = 4,                              // 1 fast, 5 unbreakable
    LightTransmission = 0.2f,                           // 0 opaque, 1 fully transparent
    SelfIllumination  = 0.1f,                           // 0 dark, 1 fully lit
    TileIndices       = new int[6] { 0, 0, 0, 0, 0, 0 },
    BlockPlayer       = true,
    CanBeDug          = true,
});
```

Once registered, the block type exists in the world but nothing places it yet. To make it placeable, register an inventory item with the Block behavior using the same ID:

```csharp
Items.Register("you.marble", new ItemDef {
    DisplayName   = "Marble Block",
    Description1  = "A polished block of marble",
    BehaviorClass = ItemBehaviors.Block,
});
```

The item resolves the block slot by matching the ID string. As long as the block ID and the item ID are the same, you don't need to wire them up explicitly.

Add a recipe:

```csharp
Recipes.Add("you.marble", 4,
    InventoryItemIDs.RockBlock, InventoryItemIDs.RockBlock,
    InventoryItemIDs.RockBlock, InventoryItemIDs.RockBlock);
```

Craft, place, mine, get marble back. That's the full setup.

#### Modify a vanilla block

```csharp
Blocks.Modify(BlockTypeEnum.Rock, def => {
    def.Hardness = 2;   // easier to mine
});
```

Or modify another mod's block (e.g. a balance patch):

```csharp
Blocks.Modify("someone.marble", def => {
    def.Hardness = 3;
});
```

### `BlockDef` reference

All fields are public properties on a data class:

| Field | Type | Notes |
|---|---|---|
| `DisplayName` | `string` | Name shown in tooltips and the mining UI |
| `Hardness` | `int` | 1 to 5. Controls dig time. 5 is unbreakable. |
| `LightTransmission` | `float` | 0 opaque, 1 fully transparent (like glass) |
| `SelfIllumination` | `float` | 0 dark, 1 emits full light (like a lantern) |
| `TileIndices` | `int[6]` | Texture indices for each face. See note below. |
| `BlockPlayer` | `bool` | True if the player collides with it |
| `CanBeDug` | `bool` | False for decorative blocks you can't mine |
| `CanBeTouched` | `bool` | Whether the cursor can target it |
| `CanBuildOn` | `bool` | Whether other blocks can be placed on top |
| `HasAlpha` | `bool` | True for blocks with transparent pixels in their texture |
| `DrawFullBright` | `bool` | Ignore lighting, always render at full brightness |
| `BouncesLasers` | `bool` | Laser sword projectiles reflect off this |
| `BounceRestitution` | `float` | How much energy is preserved on bounce |
| `SpawnEntity` | `bool` | Spawns as a falling entity rather than a static block |
| `DamageTransmission` | `float` | 0 absorbs all explosive damage, 1 lets it all through |
| `IsItemEntity` | `bool` | Used for pickups (dropped items, not placed blocks) |
| `LightAsTranslucent` | `bool` | Light passes through but the block is still visually opaque |
| `NeedsFancyLighting` | `bool` | Use the higher quality lighting path |
| `InteriorFaces` | `bool` | Render faces inside the block (used for water, lava) |
| `AllowSlopes` | `bool` | Whether ramps and slopes can connect to this block |
| `Facing` | `BlockFace` | Default rotation when placed. `BlockFace.NUM_FACES` for none. |
| `ParentBlockType` | `BlockTypeEnum` | Controls what drops when mined. Defaults to itself. |

**On `TileIndices`:** these reference the existing vanilla texture atlas. Index 0 is dirt, 2 is the grass top, 5 is rock, and so on. Custom block face art requires extending the atlas via direct source editing (`source_modding.md`). Item hotbar icons can use mod PNGs via **`Assets`**.

**On `ParentBlockType`:** vanilla CMZ stores "what does this block drop when mined" via the ParentBlockType field. Rock points to Rock, Dirt to Dirt, etc. For mod blocks the framework defaults this to the block's own slot, so mining marble gives you a marble item. If you want a fake stone block that drops Rock when mined, set `ParentBlockType = BlockTypeEnum.Rock` explicitly.

**On `Hardness`:** dig time for mod blocks is Hardness seconds, regardless of which pickaxe you're using. So 1 is fast, 4 is slow, 5 is effectively unbreakable. Tier aware dig times for mod blocks aren't wired up yet. If you want a wood pick to be slower than a diamond pick on your block specifically, you'd need to extend the pickaxe switch tables directly in source.

---

### `Events`

Subscribe to gameplay events to react when stuff happens in the world. Subscribe inside `OnLoad()`:

```csharp
public static void OnLoad()
{
    Events.PlayerTakeDamage += OnDamage;
    Events.BlockDestroyed   += OnBlockDestroyed;
}
```

Each event hands you an args object with everything relevant. If a handler throws, the framework catches it and logs the error. Other handlers still run.

#### `Events.PlayerTakeDamage`

Fires every time the local player takes damage from anything (zombies, falls, lava, explosions).

```csharp
Events.PlayerTakeDamage += args => {
    if (args.DamageAmount > 50f) args.DamageAmount = 50f;  // cap incoming damage
    // or args.Cancel = true to negate it entirely
};
```

| Field | Type | Notes |
|---|---|---|
| `DamageAmount` | `float` | Damage about to be applied. Modify to scale. |
| `DamageSource` | `Vector3` | World position the damage came from |
| `Player` | `Player` | The player taking damage |
| `Cancel` | `bool` | Set true to skip the damage entirely |

Good for god mode, damage absorption armor, difficulty scalers.

#### `Events.BlockDestroyed`

Fires every time a block transitions to Empty. Catches player digging, explosions, zombie digging, and remote AlterBlockMessage syncs. It's the low level "block went away" event.

```csharp
Events.BlockDestroyed += args => {
    ModLog.Info("Block " + args.BlockType + " destroyed at " + args.BlockPosition);
};
```

| Field | Type | Notes |
|---|---|---|
| `BlockType` | `BlockTypeEnum` | The block that was destroyed |
| `BlockPosition` | `IntVector3` | World position |
| `DestroyedBy` | `Player` | Currently always null. Reserved. |

For "the player just mined a block" specifically, use `PlayerMinedBlock` below. That gives you the actual item the player got, which is more useful for drop tables.

#### `Events.PlayerMinedBlock`

Fires when the local player successfully mines a block, after vanilla mining has produced its drop. The args include the actual item the player got, the tool they used, and the block.

```csharp
Events.PlayerMinedBlock += args => {
    if (args.Drop == null) return;  // wrong tier pickaxe gave nothing
    // duplicate the drop
    InventoryItem extra = args.Drop.ItemClass.CreateItem(args.Drop.StackCount);
    Vector3 pos = IntVector3.ToVector3(args.BlockPosition) + new Vector3(0.5f, 0.5f, 0.5f);
    PickupManager.Instance.CreatePickup(extra, pos, false);
};
```

| Field | Type | Notes |
|---|---|---|
| `BlockType` | `BlockTypeEnum` | What was mined |
| `BlockPosition` | `IntVector3` | Where |
| `Drop` | `InventoryItem` | What vanilla just dropped. May be null. |
| `Tool` | `InventoryItem` | The tool the player was holding |
| `Player` | `Player` | The player |

This is the right event for double drops, fortune style enchants, custom drop tables, anything reacting to "the player got an item from mining". The vanilla drop already exists when the event fires.

#### `Events.GameTick`

Fires every frame.

```csharp
Events.GameTick += args => {
    // runs at ~30 Hz on Xbox 360
};
```

| Field | Type | Notes |
|---|---|---|
| `GameTime` | `GameTime` | XNA game time (elapsed since last frame, total elapsed) |

Be careful what you do here, it fires a lot. If you need to run something every few seconds, track time yourself and only act when enough has passed.

#### `Events.PlayerRespawn`

Fires when the local player respawns after dying.

```csharp
Events.PlayerRespawn += args => {
    args.Player.WorldPosition = mySpawnPoint;  // teleport to custom spawn
};
```

| Field | Type | Notes |
|---|---|---|
| `Player` | `Player` | The respawning player |

#### `Events.ItemCrafted`

Fires when the player crafts an item via the crafting UI. Covers both creative (infinite resources) and survival modes.

```csharp
Events.ItemCrafted += args => {
    ModLog.Info("Crafted " + args.Result.StackCount + "x " + args.Result.ItemClass.Name);
};
```

| Field | Type | Notes |
|---|---|---|
| `Recipe` | `Receipe` | The recipe definition that was used |
| `Result` | `InventoryItem` | The item that was added to inventory |

#### `Events.EnemyKilled`

Fires when an enemy is killed by the local player. Covers zombies, aliens, and dragons.

```csharp
Events.EnemyKilled += args => {
    if (args.EnemyTypeName == "Dragon") ModLog.Info("Dragon down!");
};
```

| Field | Type | Notes |
|---|---|---|
| `Enemy` | `BaseZombie` | The enemy entity. Null for dragons. |
| `KillingItemID` | `InventoryItemIDs` | The weapon used |
| `ShooterID` | `byte` | Network ID of the player who got the kill |
| `DeathPosition` | `Vector3` | World position |
| `EnemyTypeName` | `string` | "Zombie", "Alien", "Dragon", etc. |

Good for kill counters, achievement systems, custom XP, mob loot tables.

---

### `Data`

Persist data with the world save. Mods get a key-value store keyed automatically per mod, saved alongside the world and reloaded when the world loads.

#### Per world data

Saved with the world, lost if the world is deleted.

```csharp
Data.SetWorld("kills", "42");
string kills = Data.GetWorld("kills", "0");

Data.SetWorldInt("kills", 42);
int n = Data.GetWorldInt("kills", 0);
```

#### Global data

Saved globally, survives across worlds.

```csharp
Data.SetGlobal("first-launch", "2025-01-15");
string firstLaunch = Data.GetGlobal("first-launch", null);
```

#### Notes

- The store is keyed per mod automatically. Calls from inside your `OnLoad()` and event handlers know which mod they belong to, you don't have to namespace your keys.
- Data is saved when the world saves, which happens periodically and on quit. Don't expect a SetWorld call to be flushed instantly.
- The store is string to string. For int helpers there are SetWorldInt and GetWorldInt. For anything more complex (lists, structs), serialize to string yourself (JSON, comma separated, whatever fits).
- If your mod is uninstalled, its saved data stays in the world file but becomes inert. Reinstalling the mod will pick it back up.

#### Example: a kill counter

```csharp
[Mod(Id = "you.kill-counter", Name = "Kill Counter", Version = "1.0.0")]
public static class KillCounterMod
{
    public static void OnLoad()
    {
        int kills = Data.GetWorldInt("total-kills", 0);
        ModLog.Info("Kill count loaded: " + kills);

        Events.EnemyKilled += args => {
            int current = Data.GetWorldInt("total-kills", 0);
            Data.SetWorldInt("total-kills", current + 1);
        };
    }
}
```

---

### `Assets`

Ship PNG textures with your mod. `deploy.ps1` discovers files under `assets/` and copies them into `Content/ModAssets/` in the built STFS. The game loads PNGs at runtime — no XNA Content Pipeline required on your PC.

#### Mod folder layout

```
mods/my-mod/
├── mod.json
├── MyMod.cs
    └── assets/
        ├── icons/
        │   └── my-sword.png       → hotbar/inventory icons
        ├── textures/
        │   └── zombie-pink.png    → enemy reskins, runtime LoadTexture
        └── blocks/
            ├── dirt.png           → terrain texture pack (atlas patch)
            ├── grass_top.png
            └── cobblestone.png
```

PNG files in `assets/icons/` and `assets/textures/` are registered for runtime loading. Files in `assets/blocks/` patch the terrain diffuse atlas at startup (Minecraft-style texture packs).

#### Using icons on items

```csharp
Items.Register("you.my-sword", new ItemDef {
    IconTextureName = "my-sword",   // resolves to you.my-sword/my-sword
    // ...
});
```

Or reference explicitly: `IconTextureName = "you.my-sword/my-sword"`.

#### Runtime loading

```csharp
if (Assets.Exists("you.my-sword/my-sword"))
{
    Texture2D tex = Assets.LoadTexture("you.my-sword/my-sword");
}
```

`Assets.ResolveIcon(modId, fileName)` returns the full logical name when a PNG exists, otherwise `null`.

#### Block texture packs (terrain)

Drop PNGs in `assets/blocks/`. The filename (without extension) is an **alias** that maps to a slot in CMZ's 8×8 terrain atlas. At startup the game patches the diffuse and mip-diffuse atlases in place; fancy-lit tiles (ores, walls) also get a flat rock normal/spec template so custom art isn't lit by stale normal data.

| PNG filename | Replaces |
|---|---|
| `dirt.png` | Dirt |
| `grass_top.png` + `grass_side.png` | Grass block faces |
| `cobblestone.png` | Rock / stone |
| `sand.png`, `coal_ore.png`, `iron_ore.png`, … | Matching vanilla blocks |
| `log_top.png` + `log_side.png` | Log ends vs bark |
| `tnt_top.png` + `tnt_side.png` | TNT |

Any square PNG works (16×16 Faithful / Minecraft textures scale fine). No C# required — an empty `[Mod]` class is enough.

Texture packs are **self-contained mods** — the PNGs ship inside the mod folder and `deploy.ps1` discovers them automatically. No import step, no extra tooling: drop the folder into `mods/`, pack, play.

```
mods/my-texture-pack/
├── mod.json                   { "id": "you.my-texture-pack", ... }
├── MyTexturePack.cs           an empty [Mod] class is enough
└── assets/
    └── blocks/
        ├── dirt.png
        ├── grass_top.png
        └── cobblestone.png
```

Look for `BlockTextureRegistry: applied N tile(s) in place` in the load log.

**Example:** `mods-examples/faithful-textures/` ships ready to use (block art from the [Faithful](https://faithfulpack.net/) pack):

```powershell
Copy-Item -Recurse mods-examples\faithful-textures mods\00-faithful-textures
.\deploy.ps1 -Pack
```

#### Item icon packs (vanilla items)

Drop PNGs in `assets/items/` to replace the hotbar/inventory icons of **vanilla items** (which are normally live-rendered 3D snapshots). The filename is an alias onto the vanilla item id: `iron_pickaxe.png`, `diamond_spade.png` (or `_shovel`), `knife.png` / `gold_knife.png` / `diamond_knife.png` / `bloodstone_knife.png` (sword art works well here), `compass.png`, `clock.png`, `torch.png`, `stick.png`, `door.png`, `iron_ore.png`, and the other pickaxe/spade tiers. Items without an override keep their vanilla 3D-rendered icon; unknown aliases log a warning. Guns and laser tools have no aliases yet — they stay 3D.

#### Custom art for your own blocks

A PNG whose filename matches a **registered mod block** — the full id, or its last dot-segment (`marble-block.png` → `example.marble-block`) — is automatically assigned a free atlas tile (vanilla uses 31 of the 64 slots; 31–63 are free) and the block's `TileIndices` are rewired to it at load. Optional per-face art: `<name>_top.png`, `<name>_side.png`, `<name>_bottom.png`. The `TileIndices` you set in the `BlockDef` then act only as a fallback for when the PNG is missing. See `mods-examples/marble-block/` for a complete example.

#### Notes

- Mod assets are PNG only. No compile step — `deploy.ps1` copies files and registers them automatically.
- Vanilla-alias PNGs replace existing atlas tiles; PNGs named after registered mod blocks claim free tiles (31–63) automatically.
- Enemy reskins use `EnemyDef.TextureAssetName` (see **`Entities`**). The texture must match the vanilla model's UV layout if you're reusing a zombie/alien mesh.
- To edit existing **vanilla** `.xnb` files (not mod assets), see `docs/source_modding.md` — that workflow uses xnbcli separately.

---

### `Audio`

Override, suppress, or trigger **existing vanilla XACT cues**. Custom sound banks are not supported yet — you can only remap or replay cues already in the game.

#### Override a cue globally

```csharp
Audio.Override("Hit", "pickupitem");   // damage sounds like a pickup
```

#### Suppress or reset

```csharp
Audio.Suppress("FootStep");
Audio.Reset("FootStep");               // back to vanilla
```

#### Play a cue on demand

```csharp
Audio.Play("pickupitem");                          // 2D
Audio.Play("pickupitem", args.DeathPosition);      // 3D at world position
```

#### React to any sound about to play

```csharp
Audio.SoundPlay += args => {
    if (args.CueName == "GunShot3") args.Suppressed = true;
};
```

`Audio.GetAllCueNames()` returns a starter list of known cue names (`Hit`, `Dig`, `ZombieGrowl`, `pickupitem`, etc.). Not exhaustive — discover more by searching the source or experimenting.

**Example mods:** `mods-examples/silly-sounds/`, `mods-examples/kill-cheer/`

---

### `UI`

Add main-menu entries and draw HUD overlays during gameplay.

#### HUD overlay

```csharp
UI.RegisterHUDOverlay("my-overlay", args => {
    var font = UI.GetSmallFont();
    if (font == null) return;

    args.SpriteBatch.Begin();
    args.SpriteBatch.DrawString(font, "Hello", new Vector2(10, 10), Color.White);
    args.SpriteBatch.End();
});
```

`HUDDrawArgs` provides:

| Field | Notes |
|---|---|
| `SpriteBatch`, `Device`, `GameTime` | Standard XNA draw context |
| `Bounds` | Title-safe screen rectangle |
| `Player` | Local player (may be null on menus) |
| `BelowDistanceReadoutY` | Y coordinate aligned with vanilla distance HUD |

Use `UI.GetSmallFont()` or `UI.GetMedFont()` — don't reach into private game fields.

#### Main menu item

```csharp
UI.AddMainMenuItem("My Mod Info", () => {
    UI.ShowMessage("Hello from my mod!", TimeSpan.FromSeconds(3));
});
```

Set `ShowOnTitle = true` in `MainMenuItemOptions` to pin an item on the root title menu instead of the auto-generated **Mods** submenu.

#### Toast messages

```csharp
UI.ShowMessage("Structure placed!", TimeSpan.FromSeconds(2), Color.Yellow);
```

#### Custom screens

```csharp
UI.PushScreen(myCustomScreen);   // Screen subclass from DNA.Drawing.UI
UI.PopScreen();
```

**Example mods:** `mods-examples/coords-hud/`, `mods-examples/kill-counter/`, `mods-examples/mod-info/`

---

### `Entities`

Tweak vanilla enemy stats, control spawn rates, or register custom enemy types.

#### Stat overrides (vanilla enemies)

```csharp
Entities.SetMaxHealth(EnemyTypeEnum.ZOMBIE_0_0, 50f);
Entities.SetMoveSpeed(EnemyTypeEnum.ZOMBIE_0_0, 3f);       // slow / fast pair auto-scaled
Entities.SetEmergeSpeed(EnemyTypeEnum.ZOMBIE_0_0, 2f);
Entities.SetDamage(EnemyTypeEnum.ZOMBIE_0_0, 15f);
Entities.SetSpawnWeight(EnemyTypeEnum.ZOMBIE_0_0, 2f);     // more common in spawn pool
Entities.SetSpawnEnabled(EnemyTypeEnum.ALIEN_0_0, false);    // disable entirely
```

#### Custom enemy type

Subclass an existing enemy type (usually `ZombieEnemyType`), then register:

```csharp
public class FastZombieType : ZombieEnemyType
{
    public FastZombieType(EnemyTypeEnum slot)
        : base(slot, ModelNameEnum.ZOMBIE, TextureNameEnum.ZOMBIE_2,
               FoundInEnum.ABOVEGROUND, 2, 0.1f)
    {
        StartingHealth = 12f;
        BaseSlowSpeed = 5f;
        BaseFastSpeed = 12f;
    }
}

Entities.RegisterCustom<FastZombieType>("example.fast-zombie", new EnemyDef {
    FoundIn = EnemyType.FoundInEnum.ABOVEGROUND,
    SpawnWeight = 1f,
    TextureAssetName = "example.fast-zombies/zombie-pink",  // optional mod PNG reskin
});
```

Custom enemies take enum slots 200–255 (same pattern as mod blocks). All players in multiplayer need the same mod set so enum slots match.

#### Per-spawn hook

```csharp
Entities.EnemySpawning += args => {
    if (args.Type == EnemyTypeEnum.ALIEN_0_0) args.Cancel = true;
    // args.Type is mutable — substitute another type
    // args.Overrides can patch stats for this spawn only
};
```

**Example mods:** `mods-examples/tougher-zombies/`, `mods-examples/no-aliens/`, `mods-examples/fast-zombies/`

---

### `Worldgen`

Adjust ore generation and place structures after chunks generate. **Worldgen changes only affect newly created worlds** — existing saves keep their already-generated terrain.

#### Ore frequency multiplier

```csharp
Worldgen.SetOreFrequency(BlockTypeEnum.DiamondOre, 10f);   // 10× diamonds
```

#### Custom mod ore

Requires a registered mod block (see **`Blocks`**):

```csharp
Blocks.Register("example.mythril-ore", new BlockDef { /* ... */ });
Items.Register("example.mythril-ore", new ItemDef { BehaviorClass = ItemBehaviors.Block, /* ... */ });

Worldgen.RegisterOre("example.mythril-ore", new OreSpawnDef {
    MinY = 5,
    MaxY = 45,
    Frequency = 1.5f,
    NoiseOffset = 1337,
});
```

| `OreSpawnDef` field | Notes |
|---|---|
| `MinY` / `MaxY` | World Y range (inclusive min, exclusive max) |
| `Frequency` | Relative density vs vanilla ores |
| `NoiseOffset` | Separates vein patterns between different ores |

#### Post-generation structures

```csharp
Worldgen.ChunkGenerated += args => {
    // args.Terrain, args.ChunkMin, args.ChunkSize (16×256×16)
    Worldgen.PlaceStructure(args.Terrain, origin, myStructure);
};
```

Implement `IStructure` to place blocks relative to an origin:

```csharp
public class MyStructure : IStructure
{
    public void Place(BlockTerrain terrain, IntVector3 origin)
    {
        Worldgen.TrySetBlock(terrain, origin, BlockTypeEnum.Rock);
    }
}
```

`Worldgen.TrySetBlock` handles entity blocks (torches, doors) correctly — use it instead of writing `terrain._blocks` directly.

`Worldgen.RegisterBiome` is reserved but not implemented. Use `ChunkGenerated` for custom terrain overlays for now.

**Example mods:** `mods-examples/diamond-rush/`, `mods-examples/mythril-ore/`, `mods-examples/ruins/`

---

## Walkthrough: a custom item (diamond sword)

A full working version of this is in `mods-examples/diamond-sword/`. Copy it to `mods/diamond-sword/` and build if you just want to see it run.

To build it from scratch, create the folder structure first:

```
mods/
└── diamond-sword/
    ├── mod.json
    ├── DiamondSwordMod.cs
    └── assets/
        └── icons/
            └── diamond-sword.png
```

Place any PNG you want for the hotbar icon in `assets/icons/`. Square works best; 64×64 or similar is fine.

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
                IconTextureName = "diamond-sword",
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
- **Swap the icon.** Replace the PNG in `assets/icons/`, or set `IconTextureName` to a vanilla `.xnb` name from `Content/`.
- **Use a different behavior.** Change `BehaviorClass = ItemBehaviors.PickAxe` to make the same definition mine blocks instead. Adjust other stats accordingly (`ItemSelfDamagePerUse`, etc.).
- **Make it cheaper / more expensive.** Change the `Recipes.Add(...)` ingredients.

The 3D view model in first person is still whatever the sword behavior uses from vanilla assets. Custom icons only affect the hotbar/inventory sprite unless you also mod source assets.

---

## Walkthrough: a custom block (marble)

A full version of this is in `mods-examples/marble-block/`. Copy it to `mods/marble-block/` and build to see it run.

```
mods/
└── marble-block/
    ├── mod.json
    └── MarbleBlockMod.cs
```

**`mod.json`**:

```json
{
    "id": "example.marble-block",
    "name": "Marble Block",
    "author": "Your Name",
    "version": "1.0.0",
    "description": "A new placeable block crafted from stone",
    "modapi_version": "1"
}
```

**`MarbleBlockMod.cs`**:

```csharp
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.Terrain;

namespace ExampleMarbleBlock
{
    [Mod(Id = "example.marble-block", Name = "Marble Block", Version = "1.0.0")]
    public static class MarbleBlockMod
    {
        public static void OnLoad()
        {
            // Register a new block type (allocates a slot 200-255)
            Blocks.Register("example.marble-block", new BlockDef {
                DisplayName       = "Marble",
                Hardness          = 4,
                LightTransmission = 0.2f,
                SelfIllumination  = 0.1f,
                TileIndices       = new int[6] { 0, 0, 0, 0, 0, 0 },
                BlockPlayer       = true,
                CanBeDug          = true,
                CanBeTouched      = true,
                CanBuildOn        = true,
            });

            // Register the inventory item that places it
            Items.Register("example.marble-block", new ItemDef {
                DisplayName   = "Marble Block",
                Description1  = "A polished block of marble",
                Description2  = "Slightly luminous. Crafted from stone.",
                MaxStackSize  = 100,
                BehaviorClass = ItemBehaviors.Block,
            });

            // Recipe: 4 rocks -> 4 marble blocks
            Recipes.Add("example.marble-block", 4,
                InventoryItemIDs.RockBlock, InventoryItemIDs.RockBlock,
                InventoryItemIDs.RockBlock, InventoryItemIDs.RockBlock);
        }
    }
}
```

Build and deploy:

```powershell
.\deploy.ps1 -Pack
```

Open the crafting menu in-game. The Marble Block recipe should appear. Craft it, place it, mine it, get it back. Hardness 4 means about 4 seconds to dig with any pickaxe.

### Iterating on the block

Common follow-ups:

- **Change the look.** Swap `TileIndices` values to other tiles from the vanilla atlas (0=dirt, 2=grass top, 5=rock, etc). Each of the 6 array entries is a different face. Pass the same value six times for a uniform block.
- **Make it glow.** Bump `SelfIllumination` toward 1.0 to emit light without needing a lantern next to it.
- **Make it harder or softer.** Hardness 1 mines instantly, Hardness 4 takes a few seconds, Hardness 5 is unbreakable. Pick the feel you want.
- **Drop something other than itself.** Set `ParentBlockType = BlockTypeEnum.Rock` to make mining marble give you a Rock item. Useful for fake-look blocks like a hidden stone variant.
- **React when it's mined.** Subscribe to `Events.PlayerMinedBlock`, check `args.BlockType` against your slot, and run custom logic (spawn particles, give a bonus item, trigger a quest, etc).

For wholly new block face textures (not reusing atlas tiles), see the texture section in `source_modding.md`. The framework registers block slots but does not extend the terrain atlas yet.

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

### Item appears in inventory with no icon / wrong icon

For mod PNG icons:

- Confirm the PNG is at `assets/icons/your-name.png` in the mod folder
- `IconTextureName` should be the bare filename (`"your-name"`) or full path (`"mod-id/your-name"`)
- Rebuild with `.\deploy.ps1 -Pack` so assets get staged into `Content/ModAssets/`
- Check the load log for `Failed to load mod asset` errors

For vanilla `.xnb` icons, `IconTextureName` must match a texture in `Content/`:

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

Things you still need direct source editing (`source_modding.md`) for:

- **Custom normal/spec maps for texture packs.** Block packs patch diffuse and give fancy-lit tiles a flat rock normal/spec template; authoring your own normal maps still needs engine work.
- **Block atlas expansion beyond 64 tiles.** Vanilla uses 31 tiles and custom block art claims the 33 free slots automatically; going past 64 unique tiles needs engine work.
- **Per pickaxe tier dig times for mod blocks.** Mod blocks dig in `Hardness` seconds regardless of which pickaxe is used.
- **Novel enemy AI.** You can subclass `EnemyType` and register custom types, but complex new state machines require editing the AI source.
- **Custom XACT sound banks.** Audio API only remaps or replays existing vanilla cues.
- **Full biome registration.** Use `Worldgen.ChunkGenerated` for overlays; `RegisterBiome` is not wired up yet.
- **Multiplayer protocol changes.** Mod blocks sync via existing `AlterBlockMessage`; new message types aren't supported.
- **Achievements, game modes, custom network messages.**

These may arrive in future ModAPI versions. For now, mod those via direct source editing.

---

Good luck. Start with `mods-examples/cheap-torches/`, get something working in-game, then scale up. The build/test loop is fast enough that iteration is cheap; lean into it.
