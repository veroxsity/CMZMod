@echo off
REM build_windows.bat - Build stfs_pack on Windows with MinGW + Botan
REM
REM Requires:
REM   - MinGW-w64 with g++ supporting C++20 (e.g. via MSYS2 or Qt's MinGW)
REM   - Botan 3.x installed at C:\botan\ (per Velocity's COMPILING.md)
REM   - git
REM
REM If you have Qt installed with MinGW, the Qt MinGW binaries are typically at:
REM   C:\Qt\Tools\mingw1310_64\bin\
REM Add that to your PATH before running this script.

setlocal

set HERE=%~dp0
set VEL=%HERE%Velocity
set BOTAN_INCLUDE=C:\botan\include\botan-3
set BOTAN_LIB=C:\botan\lib\libbotan-3.a

REM 1. Clone Velocity if missing
if not exist "%VEL%" (
    echo [+] Cloning Velocity...
    git clone --depth 1 https://github.com/hetelek/Velocity.git "%VEL%"
    if errorlevel 1 exit /b 1
)

REM 2. Apply patches (idempotent)
echo [+] Applying patches...
findstr /C:"i %% 0x40 == 0" "%VEL%\XboxInternals\Stfs\StfsPackage.cpp" >nul
if errorlevel 1 (
    pushd "%VEL%"
    git apply "%HERE%velocity-fixes.patch"
    if errorlevel 1 (
        echo [!] Failed to apply patch
        popd
        exit /b 1
    )
    popd
    echo     Applied velocity-fixes.patch
) else (
    echo     Patches already applied, skipping
)

REM 3. Verify Botan exists
if not exist "%BOTAN_INCLUDE%" (
    echo [!] Botan 3 headers not found at %BOTAN_INCLUDE%
    echo     See Velocity's COMPILING.md or download from
    echo     https://www.mediafire.com/file/i77h4kgrsj6vo9l/botan-3.5.0_win64.7z
    exit /b 1
)
if not exist "%BOTAN_LIB%" (
    echo [!] Botan 3 library not found at %BOTAN_LIB%
    exit /b 1
)

REM 4. Compile
echo [+] Compiling stfs_pack.exe...
g++ -std=c++20 -O2 -DXBOXINTERNALS_STATIC ^
    -I"%VEL%\XboxInternals" -I"%BOTAN_INCLUDE%" ^
    "%HERE%stfs_pack.cpp" ^
    "%VEL%\XboxInternals\Stfs\StfsPackage.cpp" ^
    "%VEL%\XboxInternals\Stfs\StfsDefinitions.cpp" ^
    "%VEL%\XboxInternals\Stfs\XContentHeader.cpp" ^
    "%VEL%\XboxInternals\IO\BaseIO.cpp" ^
    "%VEL%\XboxInternals\IO\FileIO.cpp" ^
    "%VEL%\XboxInternals\IO\MemoryIO.cpp" ^
    "%VEL%\XboxInternals\IO\MultiFileIO.cpp" ^
    "%VEL%\XboxInternals\Gpd\XdbfHelpers.cpp" ^
    "%VEL%\XboxInternals\Cryptography\XeCrypt.cpp" ^
    "%VEL%\XboxInternals\AvatarAsset\AssetHelpers.cpp" ^
    "%BOTAN_LIB%" -ladvapi32 -luser32 ^
    -o "%HERE%stfs_pack.exe"
if errorlevel 1 exit /b 1

echo [+] Compiling stfs_list.exe...
g++ -std=c++20 -O2 -DXBOXINTERNALS_STATIC ^
    -I"%VEL%\XboxInternals" -I"%BOTAN_INCLUDE%" ^
    "%HERE%stfs_list.cpp" ^
    "%VEL%\XboxInternals\Stfs\StfsPackage.cpp" ^
    "%VEL%\XboxInternals\Stfs\StfsDefinitions.cpp" ^
    "%VEL%\XboxInternals\Stfs\XContentHeader.cpp" ^
    "%VEL%\XboxInternals\IO\BaseIO.cpp" ^
    "%VEL%\XboxInternals\IO\FileIO.cpp" ^
    "%VEL%\XboxInternals\IO\MemoryIO.cpp" ^
    "%VEL%\XboxInternals\IO\MultiFileIO.cpp" ^
    "%VEL%\XboxInternals\Gpd\XdbfHelpers.cpp" ^
    "%VEL%\XboxInternals\Cryptography\XeCrypt.cpp" ^
    "%VEL%\XboxInternals\AvatarAsset\AssetHelpers.cpp" ^
    "%BOTAN_LIB%" -ladvapi32 -luser32 ^
    -o "%HERE%stfs_list.exe"
if errorlevel 1 exit /b 1

echo [+] Done. Built: stfs_pack.exe and stfs_list.exe
endlocal
