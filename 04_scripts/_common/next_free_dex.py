#!/usr/bin/env python3
"""Print the smallest unused classesN.dex index for an APK."""
import re
import sys
import zipfile


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: next_free_dex.py <apk>", file=sys.stderr)
        return 2
    apk = sys.argv[1]
    try:
        with zipfile.ZipFile(apk) as z:
            names = set(z.namelist())
    except (OSError, zipfile.BadZipFile) as exc:
        print(f"ERROR: cannot read APK: {exc}", file=sys.stderr)
        return 1

    if "classes.dex" not in names:
        print("ERROR: APK has no classes.dex", file=sys.stderr)
        return 1

    used = set()
    for name in names:
        m = re.fullmatch(r"classes(\d+)\.dex", name)
        if m:
            used.add(int(m.group(1)))

    n = 2
    while n in used:
        n += 1
    print(n)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
