#!/usr/bin/env python3
"""
fix_decompile_artifacts.py

The EclipseKatrina CMZ decompilation contains ~200 instances where the IL ->
C# decompiler failed to reconstruct properties, events, and indexers, leaving
direct method-call syntax like `obj.get_X()` instead of property access `obj.X`.

This script rewrites those to valid C#. It's idempotent - running it on
already-fixed code does nothing.

Patterns fixed:
    obj.get_Foo()        ->  obj.Foo
    obj.set_Foo(val)     ->  obj.Foo = val
    obj.add_Evt(h)       ->  obj.Evt += h
    obj.remove_Evt(h)    ->  obj.Evt -= h
    obj.get_Item(i)      ->  obj[i]
    obj.set_Item(i, v)   ->  obj[i] = v

Usage:
    python fix_decompile_artifacts.py "DNA Common" "CastleMinerZ"
"""
import os
import re
import sys


# Match a method call where method name starts with get_/set_/add_/remove_
# followed by an uppercase letter (so we don't catch get_lower_case-style
# legitimate method names).
PROP_RE = re.compile(r'\b(get|set|add|remove)_([A-Z]\w*)\s*\(')


def find_matching_paren(s, start):
    """Return index just past the matching ')' for an open paren at `start`."""
    depth = 1
    i = start + 1
    while i < len(s):
        c = s[i]
        if c == '(':
            depth += 1
        elif c == ')':
            depth -= 1
            if depth == 0:
                return i
        elif c == '"':
            # Skip string literal
            i += 1
            while i < len(s) and s[i] != '"':
                if s[i] == '\\':
                    i += 2
                    continue
                i += 1
        i += 1
    return -1


def split_top_level_commas(args):
    """Split argument list on top-level commas (not inside nested parens)."""
    parts = []
    depth = 0
    last = 0
    for i, c in enumerate(args):
        if c == '(' or c == '<' or c == '[' or c == '{':
            depth += 1
        elif c == ')' or c == '>' or c == ']' or c == '}':
            depth -= 1
        elif c == ',' and depth == 0:
            parts.append(args[last:i].strip())
            last = i + 1
    parts.append(args[last:].strip())
    return parts


def fix_text(text):
    """Apply all fix patterns to source text. Returns (new_text, n_fixes)."""
    out = []
    i = 0
    fixes = 0
    while i < len(text):
        m = PROP_RE.search(text, i)
        if not m:
            out.append(text[i:])
            break
        # Find matching paren
        open_paren = m.end() - 1  # position of '(' captured by regex
        close = find_matching_paren(text, open_paren)
        if close < 0:
            # Malformed, skip
            out.append(text[i:m.end()])
            i = m.end()
            continue

        # Append everything before the match
        out.append(text[i:m.start()])

        # The bit before is what's left of `obj.` - we want to preserve `obj`
        # and replace the trailing `.get_X(...)` with `.X` etc.
        kind = m.group(1)        # get/set/add/remove
        name = m.group(2)        # foo name (capitalized)
        args = text[open_paren + 1:close].strip()

        if kind == 'get':
            if name == 'Item':
                # obj.get_Item(i)  ->  obj[i]
                out.append('[' + args + ']')
            else:
                out.append(name)
        elif kind == 'set':
            if name == 'Item':
                # obj.set_Item(i, v)  ->  obj[i] = v
                parts = split_top_level_commas(args)
                if len(parts) >= 2:
                    idx = ', '.join(parts[:-1])
                    val = parts[-1]
                    out.append('[' + idx + '] = ' + val)
                else:
                    # malformed
                    out.append(m.group(0)[1:] + args + ')')
            else:
                # obj.set_Foo(val)  ->  obj.Foo = val
                out.append(name + ' = ' + args)
        elif kind == 'add':
            # obj.add_Evt(h)  ->  obj.Evt += h
            out.append(name + ' += ' + args)
        elif kind == 'remove':
            # obj.remove_Evt(h)  ->  obj.Evt -= h
            out.append(name + ' -= ' + args)
        fixes += 1
        i = close + 1
    return ''.join(out), fixes


def process_file(path):
    with open(path, 'r', encoding='utf-8', errors='replace') as f:
        original = f.read()
    fixed, n = fix_text(original)
    if n > 0 and fixed != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(fixed)
        return n
    return 0


def main():
    if len(sys.argv) < 2:
        print("Usage: fix_decompile_artifacts.py <root_dir> [<root_dir>...]")
        sys.exit(1)

    grand_total = 0
    files_changed = 0
    for root in sys.argv[1:]:
        for dirpath, _, files in os.walk(root):
            for f in files:
                if not f.endswith('.cs'):
                    continue
                p = os.path.join(dirpath, f)
                n = process_file(p)
                if n > 0:
                    files_changed += 1
                    grand_total += n
                    rel = os.path.relpath(p)
                    print(f"  {rel}: {n} fixes")
    print(f"\nTotal: {grand_total} fixes across {files_changed} files")


if __name__ == '__main__':
    main()
