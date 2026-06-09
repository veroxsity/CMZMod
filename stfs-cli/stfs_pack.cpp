// stfs_pack - CLI repacker built on Velocity's XboxInternals
//
// Builds a LIVE-format STFS package from a folder, matching retail XBLIG layout.
// Output runs on RGH/JTAG consoles (signature is not present, so retail
// consoles will reject it).
//
// Layout produced (matches retail XBLIG exactly when --wrap is on, default):
//   Root of STFS:
//     <title-id-hex>/         e.g. 584E07D1/, holds the actual game
//       CastleMinerZ.exe
//       DNA.Common.dll
//       Content/...
//       de/, es/, fr/, it/, ja/    (localizations)

#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <filesystem>
#include <algorithm>
#include <cstdint>
#include <cstdio>

#include "Stfs/StfsPackage.h"
#include "Stfs/StfsConstants.h"
#include "Stfs/XContentHeader.h"

namespace fs = std::filesystem;

struct Entry {
    bool is_folder;
    std::string path_in_package;
    fs::path abs_path;
    int depth;
};

static std::string to_pkg_path(const std::vector<std::string> &parts) {
    std::string out;
    for (size_t i = 0; i < parts.size(); ++i) {
        if (i) out += '\\';
        out += parts[i];
    }
    return out;
}

static void collect(const fs::path &root,
                    std::vector<std::string> &cur_parts,
                    std::vector<Entry> &out)
{
    std::vector<fs::directory_entry> items;
    for (auto &de : fs::directory_iterator(root)) items.push_back(de);
    std::sort(items.begin(), items.end(), [](const auto &a, const auto &b) {
        return a.path().filename().string() < b.path().filename().string();
    });

    for (auto &de : items) {
        std::string name = de.path().filename().string();
        cur_parts.push_back(name);
        if (de.is_directory()) {
            out.push_back({true, to_pkg_path(cur_parts), {}, (int)cur_parts.size()});
            collect(de.path(), cur_parts, out);
        } else if (de.is_regular_file()) {
            out.push_back({false, to_pkg_path(cur_parts), de.path(), (int)cur_parts.size()});
        }
        cur_parts.pop_back();
    }
}

static std::string title_id_hex(uint32_t id) {
    char buf[16];
    std::snprintf(buf, sizeof(buf), "%08X", id);
    return std::string(buf);
}

static void usage(const char *p) {
    std::cerr <<
        "Usage: " << p << " <input_dir> <output_file> [options]\n"
        "\n"
        "Options:\n"
        "  --title-id HEX           Title ID (default: 584E07D1 - CMZ)\n"
        "  --content-type N         Content type (default: 2 = MarketPlaceContent)\n"
        "  --display-name STR       Dashboard tile text\n"
        "  --title-name STR         Category name (default: \"Indie Games\")\n"
        "  --no-wrap                Don't wrap files in <title-id>/ folder\n"
        "  --wrap-folder NAME       Override wrapper folder name (default: title-id hex)\n"
        "                           (default: wrap, matches retail XBLIG layout)\n";
}

int main(int argc, char **argv) {
    if (argc < 3) { usage(argv[0]); return 1; }

    std::string in_dir   = argv[1];
    std::string out_file = argv[2];

    uint32_t title_id     = 0x584E07D1;
    uint32_t content_type = 2;
    std::wstring display_name = L"CastleMiner Z (Modded)";
    std::wstring title_name   = L"Indie Games";
    bool wrap_in_title_folder = true;
    std::string wrap_override;

    for (int i = 3; i < argc; ++i) {
        std::string flag = argv[i];
        auto next = [&]() -> std::string {
            if (i + 1 >= argc) { std::cerr << "missing arg for " << flag << "\n"; std::exit(1); }
            return argv[++i];
        };
        if      (flag == "--title-id")     title_id = std::stoul(next(), nullptr, 16);
        else if (flag == "--content-type") content_type = std::stoul(next(), nullptr, 0);
        else if (flag == "--display-name") { auto s = next(); display_name.assign(s.begin(), s.end()); }
        else if (flag == "--title-name")   { auto s = next(); title_name.assign(s.begin(), s.end()); }
        else if (flag == "--no-wrap")      wrap_in_title_folder = false;
        else if (flag == "--wrap-folder")  { wrap_override = next(); }
        else { std::cerr << "Unknown flag: " << flag << "\n"; usage(argv[0]); return 1; }
    }

    if (!fs::is_directory(in_dir)) {
        std::cerr << "[!] not a directory: " << in_dir << "\n"; return 1;
    }
    if (fs::exists(out_file)) fs::remove(out_file);

    std::cout << "[+] Walking " << in_dir << std::endl;
    std::vector<Entry> entries;
    std::vector<std::string> stack;

    std::string wrap_name = wrap_override.empty() ? title_id_hex(title_id) : wrap_override;
    if (wrap_in_title_folder) {
        stack.push_back(wrap_name);
        entries.push_back({true, wrap_name, {}, 1});
    }
    collect(in_dir, stack, entries);

    std::vector<Entry> folders, files;
    for (auto &e : entries) (e.is_folder ? folders : files).push_back(e);
    std::stable_sort(folders.begin(), folders.end(),
        [](const Entry &a, const Entry &b) { return a.depth < b.depth; });

    std::cout << "    " << folders.size() << " folders, " << files.size() << " files\n";

    std::cout << "[+] Creating empty package: " << out_file << std::endl;
    StfsPackage *pkg = nullptr;
    try {
        pkg = new StfsPackage(out_file, StfsPackageCreate | StfsPackageFemale);
    } catch (const std::string &e) {
        std::cerr << "[!] StfsPackage create failed: " << e;
        return 2;
    }

    std::cout << "[+] Setting metadata\n";
    pkg->metaData->magic       = LIVE;
    pkg->metaData->contentType = static_cast<ContentType>(content_type);
    pkg->metaData->titleID     = title_id;
    pkg->metaData->displayName = display_name;
    pkg->metaData->titleName   = title_name;

    // Set unrestricted license so the dashboard treats this as a full
    // game, not a trial. Without this, XBLA Indie titles hit the
    // platform-level 8-minute trial timer regardless of in-game patches.
    pkg->metaData->licenseData[0].type = Unrestricted;
    pkg->metaData->licenseData[0].data = 0xFFFFFFFFFFFFFFFFULL;
    pkg->metaData->licenseData[0].bits = 0;
    pkg->metaData->licenseData[0].flags = 0;

    std::cout << "    title-id     = 0x" << title_id_hex(title_id) << "\n";
    std::cout << "    content-type = " << content_type << "\n";
    std::cout << "    wrap-folder  = " << (wrap_in_title_folder ? wrap_name : std::string("(none)")) << "\n";

    std::cout << "[+] Creating " << folders.size() << " folders\n";
    for (size_t i = 0; i < folders.size(); ++i) {
        try {
            pkg->CreateFolder(folders[i].path_in_package);
        } catch (const std::string &err) {
            std::cerr << "[!] folder #" << (i + 1) << " '" << folders[i].path_in_package
                      << "': " << err;
            delete pkg;
            return 3;
        }
    }

    std::cout << "[+] Injecting " << files.size() << " files\n";
    size_t injected = 0;
    for (size_t i = 0; i < files.size(); ++i) {
        try {
            pkg->InjectFile(files[i].abs_path.string(), files[i].path_in_package);
            ++injected;
            if (injected % 50 == 0)
                std::cout << "    " << injected << "/" << files.size() << "\r" << std::flush;
        } catch (const std::string &err) {
            std::cerr << "\n[!] file #" << (i + 1) << " '" << files[i].path_in_package
                      << "': " << err;
            delete pkg;
            return 4;
        }
    }
    std::cout << "    " << injected << "/" << files.size() << " injected\n";

    std::cout << "[+] Writing metadata, recomputing hashes\n";
    pkg->metaData->WriteMetaData();
    pkg->Rehash();

    pkg->Close();
    delete pkg;

    auto sz = fs::file_size(out_file);
    std::cout << "[+] Done: " << (sz / (1024 * 1024)) << " MB written to " << out_file << "\n";
    return 0;
}
