#!/usr/bin/env bash
# build.sh - Build stfs_pack from source.
#
# Requires:
#   - g++ with C++20 support
#   - libbotan-2-dev (Botan crypto library)
#   - git
#
# On Ubuntu/Debian:
#   sudo apt install build-essential g++ git libbotan-2-dev
#
# On macOS:
#   brew install gcc botan@2
#
# Output: ./stfs_pack and ./stfs_list executables

set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
VEL="$HERE/Velocity"

# 1. Clone Velocity if missing
if [ ! -d "$VEL" ]; then
    echo "[+] Cloning Velocity..."
    git clone --depth 1 https://github.com/hetelek/Velocity.git "$VEL"
fi

# 2. Apply our patches (idempotent: skip if already applied)
echo "[+] Applying patches..."
cd "$VEL"
# Check via git apply --check whether any patch hunks are still needed
if git apply --check "$HERE/velocity-fixes.patch" 2>/dev/null; then
    git apply "$HERE/velocity-fixes.patch"
    echo "    Applied velocity-fixes.patch"
else
    # Check if reverse-applies (i.e. already applied)
    if git apply --check --reverse "$HERE/velocity-fixes.patch" 2>/dev/null; then
        echo "    Patches already applied, skipping"
    else
        echo "[!] Patch state unclear (partial?). Re-cloning Velocity..."
        cd "$HERE"
        rm -rf "$VEL"
        git clone --depth 1 https://github.com/hetelek/Velocity.git "$VEL"
        cd "$VEL"
        git apply "$HERE/velocity-fixes.patch"
        echo "    Applied velocity-fixes.patch (after re-clone)"
    fi
fi
cd "$HERE"

# 3. Find Botan
BOTAN_INCLUDE="/usr/include/botan-2"
BOTAN_LIB="-lbotan-2"
if [ ! -d "$BOTAN_INCLUDE" ]; then
    # Try Homebrew on macOS
    if [ -d "/opt/homebrew/include/botan-2" ]; then
        BOTAN_INCLUDE="/opt/homebrew/include/botan-2"
        BOTAN_LIB="-L/opt/homebrew/lib -lbotan-2"
    elif [ -d "/usr/local/include/botan-2" ]; then
        BOTAN_INCLUDE="/usr/local/include/botan-2"
        BOTAN_LIB="-L/usr/local/lib -lbotan-2"
    else
        echo "[!] Botan 2 headers not found. Install libbotan-2-dev."
        exit 1
    fi
fi

# 4. Compile
SOURCES=(
    "$VEL/XboxInternals/Stfs/StfsPackage.cpp"
    "$VEL/XboxInternals/Stfs/StfsDefinitions.cpp"
    "$VEL/XboxInternals/Stfs/XContentHeader.cpp"
    "$VEL/XboxInternals/IO/BaseIO.cpp"
    "$VEL/XboxInternals/IO/FileIO.cpp"
    "$VEL/XboxInternals/IO/MemoryIO.cpp"
    "$VEL/XboxInternals/IO/MultiFileIO.cpp"
    "$VEL/XboxInternals/Gpd/XdbfHelpers.cpp"
    "$VEL/XboxInternals/Cryptography/XeCrypt.cpp"
    "$VEL/XboxInternals/AvatarAsset/AssetHelpers.cpp"
)

echo "[+] Compiling stfs_pack..."
g++ -std=c++20 -O2 -DXBOXINTERNALS_STATIC \
    -I"$VEL/XboxInternals" -I"$BOTAN_INCLUDE" \
    "$HERE/stfs_pack.cpp" "${SOURCES[@]}" \
    $BOTAN_LIB \
    -o "$HERE/stfs_pack"

echo "[+] Compiling stfs_list..."
g++ -std=c++20 -O2 -DXBOXINTERNALS_STATIC \
    -I"$VEL/XboxInternals" -I"$BOTAN_INCLUDE" \
    "$HERE/stfs_list.cpp" "${SOURCES[@]}" \
    $BOTAN_LIB \
    -o "$HERE/stfs_list"

echo "[+] Done. Built: ./stfs_pack and ./stfs_list"
"$HERE/stfs_pack" 2>&1 | head -5 || true
