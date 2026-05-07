// Verify a packed STFS by listing all files via XboxInternals
#include <iostream>
#include <string>
#include <vector>
#include "Stfs/StfsPackage.h"

void list_recursive(StfsFileListing &fl, const std::string &prefix) {
    for (auto &f : fl.fileEntries) {
        std::cout << "FILE  " << prefix << f.name
                  << " size=" << f.fileSize
                  << " blk=" << f.startingBlockNum << "\n";
    }
    for (auto &d : fl.folderEntries) {
        std::cout << "DIR   " << prefix << d.folder.name << "\n";
        list_recursive(d, prefix + d.folder.name + "/");
    }
}

int main(int argc, char **argv) {
    if (argc < 2) { std::cerr << "Usage: " << argv[0] << " <stfs_file>\n"; return 1; }
    try {
        StfsPackage pkg(argv[1]);
        auto listing = pkg.GetFileListing();
        list_recursive(listing, "");
    } catch (const std::string &e) {
        std::cerr << "Error: " << e;
        return 2;
    }
    return 0;
}
