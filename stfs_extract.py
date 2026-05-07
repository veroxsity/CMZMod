#!/usr/bin/env python3
"""Minimal STFS (CON/LIVE/PIRS) extractor."""

import os
import sys
import struct


def block_to_offset(block, header_type):
    BASE = 0xC000
    BLOCK = 0x1000
    block_adjust = 0
    if block >= 0xAA:
        block_adjust += ((block // 0xAA) + 1) * (2 if header_type else 1)
    if block >= 0x70E4:
        block_adjust += ((block // 0x70E4) + 1) * (2 if header_type else 1)
    return BASE + (block_adjust + block) * BLOCK


def extract_stfs(stfs_path, out_dir):
    with open(stfs_path, 'rb') as f:
        data = f.read()

    magic = data[:4]
    if magic not in (b'CON ', b'LIVE', b'PIRS'):
        raise ValueError(f"Not an STFS file (magic={magic!r})")
    header_type = 1 if magic == b'CON ' else 0
    print(f"  Magic: {magic.decode()}")

    vd = 0x379
    assert data[vd] == 0x24

    ft_block_count = struct.unpack('<H', data[vd + 3:vd + 5])[0]
    ft_block_number = data[vd + 5] | (data[vd + 6] << 8) | (data[vd + 7] << 16)
    total_blocks = struct.unpack('>I', data[vd + 0x1C:vd + 0x20])[0]
    print(f"  Total blocks: {total_blocks} ({total_blocks * 0x1000 / (1024*1024):.1f} MB)")
    print(f"  File table: {ft_block_count} block(s) starting at block {ft_block_number}")

    listings = []
    for i in range(ft_block_count):
        off = block_to_offset(ft_block_number + i, header_type)
        block = data[off:off + 0x1000]
        for j in range(0, 0x1000, 0x40):
            entry = block[j:j + 0x40]
            if len(entry) < 0x40 or entry[0] == 0:
                continue
            listings.append(entry)

    parsed = []
    for entry in listings:
        flags = entry[0x28]
        name_len = flags & 0x3F
        if name_len == 0:
            continue
        is_dir = (flags & 0x80) != 0
        name_bytes = entry[:name_len]
        # Skip entries with embedded nulls or path separators in names (corrupt)
        if b'\x00' in name_bytes or b'/' in name_bytes:
            continue
        name = name_bytes.decode('latin-1', errors='replace')
        start_block = entry[0x2F] | (entry[0x30] << 8) | (entry[0x31] << 16)
        path_indicator = struct.unpack('>h', entry[0x32:0x34])[0]
        file_size = struct.unpack('>I', entry[0x34:0x38])[0]
        parsed.append({
            'name': name, 'is_dir': is_dir, 'start_block': start_block,
            'path_indicator': path_indicator, 'size': file_size,
        })

    print(f"  Parsed {len(parsed)} entries")

    def full_path(idx):
        parts, cur, seen = [], idx, set()
        while cur != -1 and cur < len(parsed) and cur not in seen:
            seen.add(cur)
            parts.append(parsed[cur]['name'])
            cur = parsed[cur]['path_indicator']
        return '/'.join(reversed(parts))

    os.makedirs(out_dir, exist_ok=True)
    files_extracted = 0
    bytes_extracted = 0
    for i, e in enumerate(parsed):
        path = full_path(i)
        if not path:
            continue
        full_out = os.path.join(out_dir, path)
        if e['is_dir']:
            os.makedirs(full_out, exist_ok=True)
            continue
        os.makedirs(os.path.dirname(full_out) or '.', exist_ok=True)
        remaining = e['size']
        block = e['start_block']
        with open(full_out, 'wb') as out_f:
            while remaining > 0:
                off = block_to_offset(block, header_type)
                chunk = min(0x1000, remaining)
                out_f.write(data[off:off + chunk])
                remaining -= chunk
                block += 1
        files_extracted += 1
        bytes_extracted += e['size']

    print(f"  Extracted {files_extracted} files ({bytes_extracted / (1024*1024):.1f} MB) to {out_dir}/")


if __name__ == '__main__':
    if len(sys.argv) != 3:
        print(f"Usage: {sys.argv[0]} <stfs_file> <output_dir>")
        sys.exit(1)
    extract_stfs(sys.argv[1], sys.argv[2])
