#!/usr/bin/env python3.11
"""Merge starry magic_* res into an apktool game tree and remap com.starry.magic.R$*.

inject scripts copy smali/so but historically omitted res/. Starry R$drawable uses
standalone IDs (often 0x7f02xxxx = animator in large games) → ResourceNotFound / SIGBUS.

Usage:
  python3.11 scripts/_merge_starry_res.py <PATCH_DIR> <STARRY_DEC_DIR>
"""
from __future__ import annotations

import re
import shutil
import sys
from pathlib import Path

PUBLIC_RE = re.compile(
    r'<public\s+type="([^"]+)"\s+name="([^"]+)"\s+id="(0x[0-9a-fA-F]+)"\s*/>'
)
FIELD_RE = re.compile(
    r"^\.field public static (\w+):I = (0x[0-9a-fA-F]+)\s*$", re.M
)


def find_starry_smali(patch: Path) -> Path:
    for p in patch.glob("smali*/com/starry/magic"):
        if (p / "R$drawable.smali").is_file():
            return p
    raise SystemExit(f"ERROR: com/starry/magic R$ missing under {patch}")


def load_public(pub: Path) -> tuple[str, dict[str, dict[str, int]], dict[str, int]]:
    text = pub.read_text(encoding="utf-8")
    by_type: dict[str, dict[str, int]] = {}
    max_id: dict[str, int] = {}
    for m in PUBLIC_RE.finditer(text):
        t, name, hid = m.group(1), m.group(2), int(m.group(3), 16)
        by_type.setdefault(t, {})[name] = hid
        max_id[t] = max(max_id.get(t, 0), hid)
    return text, by_type, max_id


def next_id(max_id: dict[str, int], by_type: dict[str, dict[str, int]], rtype: str) -> int:
    if rtype in max_id:
        return max_id[rtype] + 1
    # infer type package from any sibling in public (0x7fTTNNNN)
    for t, ids in by_type.items():
        if ids:
            sample = next(iter(ids.values()))
            type_byte = (sample >> 16) & 0xFF
            # pick unused type slot — prefer known Android defaults if empty
            break
    else:
        sample = 0x7F000000
    # If type never seen, invent after highest type byte used
    used = {((v >> 16) & 0xFF) for d in by_type.values() for v in d.values()}
    type_byte = max(used) + 1 if used else 0x01
    # Prefer conventional slots when free
    prefer = {"color": 0x05, "drawable": 0x07, "string": 0x0E, "layout": 0x0B}
    if rtype in prefer and prefer[rtype] not in used:
        type_byte = prefer[rtype]
    return 0x7F000000 | (type_byte << 16)


def copy_magic_drawables(starry_res: Path, patch_res: Path) -> list[str]:
    names: list[str] = []
    for src_dir in sorted(starry_res.glob("drawable*")):
        if not src_dir.is_dir():
            continue
        dst_dir = patch_res / src_dir.name
        dst_dir.mkdir(parents=True, exist_ok=True)
        for f in src_dir.iterdir():
            if not f.is_file():
                continue
            if not (f.name.startswith("magic_") or f.stem.startswith("magic_")):
                continue
            shutil.copy2(f, dst_dir / f.name)
            # resource name = file stem (ignore density folder)
            stem = f.stem
            if stem not in names:
                names.append(stem)
    return names


def merge_magic_colors(starry_res: Path, patch_res: Path) -> list[str]:
    src = starry_res / "values" / "colors.xml"
    if not src.is_file():
        return []
    text = src.read_text(encoding="utf-8")
    entries = re.findall(
        r'(<color\s+name="(magic_[^"]+)"[^>]*>.*?</color>)', text, re.S
    )
    if not entries:
        return []
    out = patch_res / "values" / "starry_magic_colors.xml"
    out.parent.mkdir(parents=True, exist_ok=True)
    body = "\n".join(e[0] for e in entries)
    out.write_text(
        '<?xml version="1.0" encoding="utf-8"?>\n'
        f"<resources>\n{body}\n</resources>\n",
        encoding="utf-8",
    )
    return [e[1] for e in entries]


def rewrite_r_smali(path: Path, mapping: dict[str, int]) -> int:
    if not path.is_file():
        return 0
    text = path.read_text(encoding="utf-8")

    def repl(m: re.Match[str]) -> str:
        name, _old = m.group(1), m.group(2)
        if name not in mapping:
            return m.group(0)
        return f".field public static {name}:I = 0x{mapping[name]:08x}"

    new, n = FIELD_RE.subn(repl, text)
    if n:
        path.write_text(new, encoding="utf-8")
    return n


def merge(patch: Path, starry_dec: Path) -> None:
    starry_res = starry_dec / "res"
    if not starry_res.is_dir():
        # fallback: source tree
        alt = Path(__file__).resolve().parents[1] / "app/src/main/res"
        if alt.is_dir():
            starry_res = alt
            print(f"merge: using source res {starry_res}")
        else:
            raise SystemExit(f"ERROR: no res in {starry_dec}")

    patch_res = patch / "res"
    pub = patch_res / "values" / "public.xml"
    if not pub.is_file():
        raise SystemExit(f"ERROR: missing {pub}")

    drawables = copy_magic_drawables(starry_res, patch_res)
    colors = merge_magic_colors(starry_res, patch_res)
    print(f"merge: drawables={len(drawables)} colors={len(colors)}")

    text, by_type, max_id = load_public(pub)
    assign: dict[str, dict[str, int]] = {"drawable": {}, "color": {}}

    for name in drawables:
        if name in by_type.get("drawable", {}):
            assign["drawable"][name] = by_type["drawable"][name]
            continue
        nid = next_id(max_id, by_type, "drawable")
        by_type.setdefault("drawable", {})[name] = nid
        max_id["drawable"] = nid
        assign["drawable"][name] = nid

    for name in colors:
        if name in by_type.get("color", {}):
            assign["color"][name] = by_type["color"][name]
            continue
        nid = next_id(max_id, by_type, "color")
        by_type.setdefault("color", {})[name] = nid
        max_id["color"] = nid
        assign["color"][name] = nid

    # append missing public entries before </resources>
    lines = []
    for rtype, names in assign.items():
        existing = by_type.get(rtype, {})
        for name, hid in names.items():
            # always ensure public line exists (rewrite file section)
            pass
    # rebuild public: keep original, append new names not in original text
    additions = []
    for rtype, names in assign.items():
        for name, hid in sorted(names.items(), key=lambda x: x[1]):
            needle = f'type="{rtype}" name="{name}"'
            if needle in text:
                # update id if present with wrong value
                text2, n = re.subn(
                    rf'(<public\s+type="{rtype}"\s+name="{name}"\s+id=")0x[0-9a-fA-F]+("\s*/>)',
                    rf"\g<1>0x{hid:08x}\2",
                    text,
                    count=1,
                )
                if n:
                    text = text2
                continue
            additions.append(
                f'    <public type="{rtype}" name="{name}" id="0x{hid:08x}" />'
            )
    if additions:
        if "</resources>" not in text:
            raise SystemExit("ERROR: public.xml missing </resources>")
        text = text.replace(
            "</resources>", "\n".join(additions) + "\n</resources>", 1
        )
        pub.write_text(text, encoding="utf-8")
        print(f"merge: public.xml +{len(additions)} entries")
    else:
        pub.write_text(text, encoding="utf-8")
        print("merge: public.xml ids refreshed")

    magic = find_starry_smali(patch)
    n_d = rewrite_r_smali(magic / "R$drawable.smali", assign["drawable"])
    n_c = rewrite_r_smali(magic / "R$color.smali", assign["color"])
    print(f"merge: R$drawable fields={n_d} R$color fields={n_c}")
    print(
        "merge: sample drawable",
        {k: f"0x{v:08x}" for k, v in list(assign["drawable"].items())[:3]},
    )


def main() -> None:
    if len(sys.argv) != 3:
        print(__doc__.strip())
        raise SystemExit(2)
    merge(Path(sys.argv[1]), Path(sys.argv[2]))


if __name__ == "__main__":
    main()
