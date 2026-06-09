# stfs_pack — Xbox 360 STFS LIVE container CLI repacker

A small command-line tool to build LIVE-format STFS (Xbox 360 .stfs) packages
from a folder. Built on Velocity's `XboxInternals` library, with several bug
fixes applied for cross-platform builds and large file tables.

Designed for **RGH/JTAG** consoles. Output packages have no valid signature
(retail consoles will reject them, dev-mode consoles ignore signatures).

## What's in this folder

```
build.sh                Linux/macOS build script
build_windows.bat       Windows build script (MinGW + Botan 3)
stfs_pack.cpp           CLI repacker source
stfs_list.cpp           CLI listing tool source
velocity-fixes.patch    Bug fixes that get applied to Velocity's code
README.md               This file
```

## Quick start (Linux / macOS)

```bash
# Install build dependencies
sudo apt install build-essential g++ git libbotan-2-dev   # Ubuntu/Debian
# or:
brew install gcc botan@2                                  # macOS

# Build
cd stfs-cli/
./build.sh

# Use
./stfs_pack ./deploy ./CMZModded.stfs --title-id 584E07D1 --wrap-folder 584E07D1
```

## Quick start (Windows)

1. Install MinGW-w64 with g++ supporting C++20 (e.g. via MSYS2 or Qt's MinGW).
2. Install Botan 3.x to `C:\botan\` (see Velocity's `COMPILING.md`).
3. Add MinGW's `bin/` directory to PATH.
4. Run `build_windows.bat` from this folder.

```cmd
build_windows.bat
stfs_pack.exe deploy CMZModded.stfs --title-id 584E07D1 --wrap-folder 584E07D1
```

## Usage

```
stfs_pack <input_dir> <output_file> [options]

Options:
  --title-id HEX         Title ID (default: 584E07D1 — CMZ).
                         CMZ retail uses 584E07D1 in metadata, even though the
                         on-console folder name is 584E07D1.
  --content-type N       Content type (default: 2 = MarketPlaceContent).
  --display-name STR     Dashboard tile text. Make this distinctive so you can
                         tell modded builds apart from retail.
  --title-name STR       Category name (default: "Indie Games").
  --no-wrap              Don't wrap files in <title-id>/ folder.
  --wrap-folder NAME     Override wrapper folder name (default: title-id hex).

Example for CMZ Modded:
  stfs_pack ./extracted ./CMZModded.stfs \
      --title-id 584E07D1 \
      --wrap-folder 584E07D1 \
      --display-name "CastleMiner Z (Modded)"
```

## Input directory layout

Two equally valid options:

**Option A — package-rooted (recommended, matches retail XBLIG layout exactly):**
```
deploy/                          <- pass this as <input_dir>
├── DashboardIcon.png            <- root files (dashboard assets)
├── GameInfo.bin
├── GameInfo.xml
├── TitleImage.png
└── 584E07D1/                    <- title-ID-named wrapper folder
    ├── CastleMinerZ.exe
    ├── DNA.Common.dll
    ├── Content/
    └── de/, es/, fr/, it/, ja/  <- localizations
```
With this layout, run with `--no-wrap` (don't add another wrapper).

**Option B — game-only (auto-wraps):**
```
deploy/                          <- pass this as <input_dir>
├── CastleMinerZ.exe
├── DNA.Common.dll
├── Content/
└── de/, es/, fr/, it/, ja/
```
With this layout, run *without* `--no-wrap`. The tool wraps it automatically
(under the name from `--wrap-folder`, defaulting to title-id hex).

## stfs_list — quick listing tool

`stfs_list <stfs_file>` prints the package's full file tree to stdout. Useful
for verifying that your output looks like retail.

## Bugs fixed in Velocity

The patch in `velocity-fixes.patch` fixes three real issues:

1. **`winnames.h`: `DWORD` was `unsigned long` (8 bytes on Linux x86-64).**
   This caused `bad_alloc` on read because the upper 4 bytes of `DWORD`s read
   as 32-bit values were uninitialized. Fixed by using `uint32_t`.

2. **`StfsPackage::WriteFileListing`: off-by-one in folder block boundary.**
   The check `(i + 1) % 0x40 == 0` triggered one iteration too early, leaving
   slot 63 of each block unused. Combined with bug #3, this silently dropped
   any folder past the first 63. Fixed by changing to `i % 0x40 == 0`.

3. **`StfsPackage::FindDirectoryListing`: shadowed-variable inner loop.**
   The recursive lookup iterated over wrong-tree siblings instead of recursing
   into the matched folder. With deep trees containing duplicate-named
   subfolders (e.g. multiple `White_0.xnb` in different `*.fbm` directories),
   this returned spurious matches. Fixed by recursing directly into the
   matched folder.

These fixes are needed for any non-trivial package on Linux/macOS, and bugs
#2 and #3 affect Windows too.

## Compatibility notes

- **Tested**: Linux x86-64 (Ubuntu 24.04, g++ 13.3, Botan 2.19)
- **Windows**: requires MinGW + Botan 3 (per Velocity's existing build setup)
- **macOS**: should work via Homebrew Botan

The Windows build path uses Botan 3 (matching Velocity's current setup); the
Linux build path uses Botan 2 (Ubuntu's apt default). Either Botan version is
fine — only `HashFunction::create_or_throw("SHA-1")` is used.

## License

Velocity is GPL-3.0. This tool extends Velocity, so it inherits GPL-3.0.
