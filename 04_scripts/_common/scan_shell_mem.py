#!/usr/bin/env python3.11
"""Scan binge shell/il2cpp memory dumps for Dobby / il2cpp / menu strings."""
from __future__ import annotations

import argparse
import collections
import re
import sys
from pathlib import Path

NEEDLES_UTF8 = [
    "秒杀", "无敌", "全屏", "全屏秒杀", "菜单", "伤害", "血量", "斌哥",
    "开启", "关闭", "功能", "霓虹",
]
ASCII_KEYS = re.compile(
    rb"(?i)(il2cpp_|DobbyHook|Dobby|feature|toggle|menu|damage|health|god|kill|"
    rb"wudi|invinc|hook|bytehook|statusc|reduce|ondamage|imgui|an_[123]|binge|"
    rb"verify|boolean|cheat|hack|oneshot|onehit|immortal|godmode|sharedprefs|"
    rb"putboolean|BW_MEMORY)"
)
SYM_LIST = [
    b"DobbyHook", b"Dobby", b"ByteHook", b"shadowhook",
    b"il2cpp_method_get_name", b"il2cpp_runtime_invoke",
    b"il2cpp_resolve_icall", b"il2cpp_class_from_name",
    b"il2cpp_domain_get", b"il2cpp_assembly_get_image",
    b"wudi", b"Toggle", b"AN_1", b"AN_2", b"AN_3",
    b"BW_MEMORYTOOL", b"StatusC", b"OnDamage",
]


def scan_file(f: Path) -> None:
    data = f.read_bytes()
    print(f"\n==== {f.name} size={len(data)} ====")
    for s in NEEDLES_UTF8:
        c8 = data.count(s.encode())
        c16 = data.count(s.encode("utf-16le"))
        if c8 or c16:
            print(
                f"  CN {s}: utf8={c8} u16={c16} "
                f"@8={data.find(s.encode())} @16={data.find(s.encode('utf-16le'))}"
            )

    strs = re.findall(rb"[\x20-\x7e]{4,160}", data)
    hits = [s.decode("latin1") for s in strs if ASCII_KEYS.search(s)]
    print(f"  keyword ascii={len(hits)}")
    for h in hits[:300]:
        print("   ", h)

    for s in SYM_LIST:
        c = data.count(s)
        if c:
            print(f"  SYM {s.decode('latin1')}: {c} @ {data.find(s)}")

    for pat in [
        rb"il2cpp_[a-z0-9_]+",
        rb"Dobby[A-Za-z0-9_]*",
        rb"AN_[0-9]+",
        rb"[A-Za-z][A-Za-z0-9_]+\$\$[A-Za-z0-9_]+",
    ]:
        ms = re.findall(pat, data)
        if ms:
            print(f"  PAT {pat.decode()}: {collections.Counter(ms).most_common(40)}")

    uniq = []
    for m in re.finditer(rb"(?:[\xe4-\xe9][\x80-\xbf]{2}){2,16}", data):
        try:
            t = m.group().decode("utf-8")
        except Exception:
            continue
        if t not in uniq and any("\u4e00" <= x <= "\u9fff" for x in t):
            uniq.append(t)
    if uniq:
        print(f"  chinese utf8 runs={len(uniq)}")
        for t in uniq[:120]:
            print("   ", t)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("outdir", help="memdiff OUTDIR containing .bin dumps")
    args = ap.parse_args()
    root = Path(args.outdir)
    if not root.is_dir():
        print(f"not a dir: {root}", file=sys.stderr)
        return 1

    bins = sorted(root.glob("*.bin"))
    if not bins:
        bins = sorted((root / "raw").glob("*.bin")) if (root / "raw").is_dir() else []
    if not bins:
        print("no .bin dumps found", file=sys.stderr)
        return 1

    report_lines = []
    for f in bins:
        # capture print
        import io
        buf = io.StringIO()
        old = sys.stdout
        sys.stdout = buf
        try:
            scan_file(f)
        finally:
            sys.stdout = old
        text = buf.getvalue()
        print(text, end="")
        report_lines.append(text)

        if f.name in ("shell_rx_bss.bin", "shell_rw_bss.bin", "sdk_rx_1.bin"):
            data = f.read_bytes()
            strs = sorted(set(re.findall(rb"[\x20-\x7e]{5,160}", data)))
            (root / f"{f.stem}_strings.txt").write_bytes(b"\n".join(strs))
            filt = [s for s in strs if ASCII_KEYS.search(s)]
            (root / f"{f.stem}_interesting.txt").write_bytes(b"\n".join(filt))
            print(f"Wrote {len(strs)} strings, {len(filt)} interesting for {f.name}")

    (root / "scan_report.txt").write_text("".join(report_lines), encoding="utf-8")
    print(f"\nWrote {root / 'scan_report.txt'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
