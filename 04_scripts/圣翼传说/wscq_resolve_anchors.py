#!/usr/bin/env python3
"""圣翼传说 7.6.06: resolve the 10 native anchors from dump methods.txt."""
from __future__ import annotations

import argparse
import hashlib
import json
import struct
from pathlib import Path

TARGETS = [
    {
        "key": "EnemyGetHurt",
        "class": "public class Enemy : MonoBehaviour",
        "sig": "public void GetHurt(long bloodNum",
        "old_rva": 0x132BC80,
        "old_insn": 0xFC170FEC,
    },
    {
        "key": "PlayerGetHurt",
        "class": "public class PlayerCtr : MonoBehaviour",
        "sig": "public void GetHurt(float HurtNum",
        "old_rva": 0x13C3318,
        "old_insn": 0x6DB92BEB,
    },
    {
        "key": "SetTimeScale",
        "class": "public class Time",
        "sig": "public static void set_timeScale(float value)",
        "old_rva": 0x2680AA8,
        "old_insn": 0xFC1E0FE8,
    },
    {
        "key": "PlayerFixed",
        "class": "public class PlayerCtr : MonoBehaviour",
        "sig": "private void FixedUpdate()",
        "old_rva": 0x13C51A0,
        "old_insn": 0x6DBB23E9,
    },
    {
        "key": "EnemiesFixed",
        "class": "public class EnemiesCtr : MonoBehaviour",
        "sig": "private void FixedUpdate()",
        "old_rva": 0x13283D8,
        "old_insn": 0xFC1E0FE8,
    },
    {
        "key": "ReviveBoss",
        "class": "public class EnemiesCtr : MonoBehaviour",
        "sig": "public void 复活BOSS()",
        "old_rva": 0x1326C34,
        "old_insn": 0xD101C3FF,
    },
    {
        "key": "EnemyCtr2Fixed",
        "class": "public class EnemyCtr2 : Enemy",
        "sig": "private void FixedUpdate()",
        "old_rva": 0x13852D8,
        "old_insn": 0x6DBA33ED,
    },
    {
        "key": "AntiSpeed",
        "class": "EnemiesCtr.<检测是否使用变速器>",
        "sig": "private bool MoveNext()",
        "old_rva": 0x132A634,
        "old_insn": 0xF81D0FFE,
    },
    {
        "key": "Ban1",
        "class": "public class CameraCtr : MonoBehaviour",
        "sig": "public void 公用封杀并退出游戏()",
        "old_rva": 0x11CE26C,
        "old_insn": 0xD10143FF,
    },
    {
        "key": "Ban2",
        "class": "public class CameraCtr : MonoBehaviour",
        "sig": "public void 封杀并退出游戏()",
        "old_rva": 0x11CE61C,
        "old_insn": 0xD10183FF,
    },
]


def md5(path: Path) -> str:
    h = hashlib.md5()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--methods", required=True)
    ap.add_argument("--so", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--version", required=True)
    ap.add_argument("--base-apk", required=True)
    args = ap.parse_args()

    methods = Path(args.methods)
    so_path = Path(args.so)
    so = so_path.read_bytes()
    rows = methods.read_text(encoding="utf-8").splitlines()[1:]
    parsed = []
    for line in rows:
        parts = line.split("|", 4)
        if len(parts) != 5:
            continue
        parsed.append(parts)

    anchors: dict[str, dict] = {}
    for target in TARGETS:
        hits = [
            p for p in parsed
            if target["class"] in p[3] and p[4].startswith(target["sig"])
        ]
        if len(hits) != 1:
            raise SystemExit(
                f'{target["key"]}: expected exactly 1 hit, got {len(hits)}'
            )
        rva_s, off_s, cid, class_decl, sig = hits[0]
        rva = int(rva_s, 16)
        file_off = int(off_s, 16)
        if rva - file_off != 0x4000:
            raise SystemExit(
                f'{target["key"]}: unexpected RVA-file_off=0x{rva-file_off:X}'
            )
        if file_off < 0 or file_off + 4 > len(so):
            raise SystemExit(f'{target["key"]}: file offset out of range')
        insn = struct.unpack_from("<I", so, file_off)[0]
        anchors[target["key"]] = {
            "class": class_decl,
            "method": sig,
            "cid": int(cid),
            "rva": rva,
            "file_off": file_off,
            "insn": insn,
            "old_rva": target["old_rva"],
            "rva_delta_from_7_5_10": rva - target["old_rva"],
            "old_insn": target["old_insn"],
            "insn_same_as_7_5_10": insn == target["old_insn"],
        }

    out = {
        "version": args.version,
        "base_apk": str(Path(args.base_apk).resolve()),
        "libil2cpp_sha256": hashlib.sha256(so).hexdigest(),
        "libil2cpp_md5": md5(so_path),
        "methods_txt": str(methods.resolve()),
        "rva_file_off_delta": "0x4000",
        "anchors": anchors,
    }
    out_path = Path(args.out)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(
        json.dumps(out, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps({
        k: {
            "rva": f'0x{v["rva"]:X}',
            "file_off": f'0x{v["file_off"]:X}',
            "insn": f'0x{v["insn"]:08X}',
            "rva_delta": v["rva_delta_from_7_5_10"],
            "insn_same": v["insn_same_as_7_5_10"],
        }
        for k, v in anchors.items()
    }, ensure_ascii=False, indent=2))
    print(f"[anchors] wrote {out_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
