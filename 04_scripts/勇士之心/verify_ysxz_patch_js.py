#!/usr/bin/env python3.11
"""Static regression checks for generated YsxzPatchJs.h."""
from __future__ import annotations

import re
import subprocess
from pathlib import Path

ROOT = Path("/Users/aliang/Documents/nixiang")
HEADER = ROOT / "mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzPatchJs.h"
START = 'R"YSXZJS('
END = ')YSXZJS";'


def main() -> int:
    text = HEADER.read_text(encoding="utf-8")
    start = text.find(START)
    end = text.rfind(END)
    if start < 0 or end < 0 or end <= start:
        raise SystemExit("raw JS payload not found")
    js = text[start + len(START):end]
    if js.count("MOD.feature4") != 1:
        raise SystemExit("feature4 occurrences changed")
    if "if (MOD.speed > 1) {" not in js:
        raise SystemExit("movement speed does not use slider value")
    if "if (MOD.feature5)" not in js or "AUTO_BATTLE" not in js:
        raise SystemExit("auto-battle privilege hook missing")
    if "restoreCosts" not in js:
        raise SystemExit("skill-cost exception fallback missing")
    if "try { touched[j][0].hTimeScale" not in js:
        raise SystemExit("movement restore exception guard missing")
    proc = subprocess.run(
        ["node", "--check", "/dev/stdin"],
        input=js,
        text=True,
        capture_output=True,
    )
    if proc.returncode != 0:
        raise SystemExit("node --check failed:\n" + proc.stderr)
    if not re.search(r"function tryAuto\(Cls\)", js):
        raise SystemExit("tryAuto implementation missing")
    print("verify_ysxz_patch_js: OK")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
