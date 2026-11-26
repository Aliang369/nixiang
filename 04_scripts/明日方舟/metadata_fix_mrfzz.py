#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""明日方舟：官方 global-metadata.dat 弱混淆【静态修复】+ 全表结构自检（替代运行时内存 dump）。

规则来源（2026-11-26/27 实测，见 10_docs/明日方舟/静态分析_2.7.91_20261127.md §2）：
  官方 APK 内 assets/bin/Data/Managed/Metadata/global-metadata.dat 不是强加密，而是弱混淆：
    ① 每 64 KiB 周期内 [4096, 20480) 这 16 KiB 窗口的字节 XOR 0x66；
    ② 文件首块 [0, 4096) 同样参与 XOR（头部 0x110 字节全被异或，不只是 magic）；
    ③ 首 8 字节是固定标记 94437212184b9eec，覆盖掉 sanity + version 两个字段。
  修复 = ①+② 异或还原 + ③ 写回 sanity 0xFAB11BAF、version=24。
  反证（为什么只补 magic 不够）：仅补首 4 字节时 td=0x5c 的 method_count 一致率只有
  237/402=59.0%，解析器直接报「无法判定 Il2CppTypeDefinition 结构尺寸」。
  另一个易错点：①与②若对首块重复施加，[4096,20480) 会被异或两次而还原回混淆态
  ——本脚本把两者做成互斥区间，且用全表自检兜住（这类错误只影响首块，头部计数仍合法）。

用法（Git Bash / Windows 中文路径均可，脚本内全部走 pathlib）：
  python 04_scripts/明日方舟/metadata_fix_mrfzz.py \
      --apk 02_apks/明日方舟/明日方舟_2.7.91_taptap_official_from_ldplayer.apk \
      --out 06_dumped/明日方舟/official/static_meta
  python 04_scripts/明日方舟/metadata_fix_mrfzz.py --in <raw global-metadata.dat> --out <dir>
  python 04_scripts/明日方舟/metadata_fix_mrfzz.py --verify <已修复文件>   # 只校验不写

产物：
  <out>/global-metadata.fixed.dat   修复后的 metadata（只在自检全过时写出）
  <out>/meta_fix.json               输入/输出指纹、改动字节数、结构自检明细
  <out>/meta_fix.failed.json        自检不自洽时才写（不产出坏 dat）

幂等：输入若已是修复态（magic+version 正确且全表自检通过）则不做任何异或，原样输出
      ⇒ 对同一输出再跑一次得到的 sha256 不变，不会被二次异或破坏。

退出码：0 = 修复成功且全表自洽；2 = 输入/参数错误；3 = 结构自检不自洽。
"""
from __future__ import annotations

import argparse
import datetime as _dt
import hashlib
import json
import struct
import sys
import zipfile
from pathlib import Path

# ---------------------------------------------------------------- 常量

ENTRY_DEFAULT = "assets/bin/Data/Managed/Metadata/global-metadata.dat"
OUT_DEFAULT = "06_dumped/明日方舟/official/static_meta"

MAGIC = b"\xaf\x1b\xb1\xfa"          # little-endian 0xFAB11BAF
MAGIC_U32 = 0xFAB11BAF
VERSION = 24
PERIOD = 0x10000                      # 64 KiB 周期
WIN_LO, WIN_HI = 0x1000, 0x5000       # 周期内被异或的 16 KiB 窗口
XOR_KEY = 0x66

MD_SIZE = 0x20                        # Il2CppMethodDefinition v24
TD_SIZE_EXPECT = 0x5C                 # Il2CppTypeDefinition v24（实测唯一解）
TD_SIZE_GUESSES = (0x5C, 0x58, 0x60)

# 2.7.91 实测基准（只作对照打印，不作硬判——换版本时数值本应变化）
BASELINE = {
    "types": 50548,
    "methods": 307867,
    "td_size": "0x5c",
    "types_with_method_start": 44081,
    "method_count_equal_grouped": 44081,
}

# v24 头部 (名字, offset 字段偏移, count 字段偏移)，count 为 bytes 口径
REGIONS = (
    ("stringLiteral", 0x08, 0x0C),
    ("stringLiteralData", 0x10, 0x14),
    ("string", 0x18, 0x1C),
    ("events", 0x20, 0x24),
    ("properties", 0x28, 0x2C),
    ("methods", 0x30, 0x34),
    ("fields", 0x40, 0x44),
    ("parameters", 0x58, 0x5C),
    ("nestedTypes", 0x80, 0x84),
    ("interfaces", 0x90, 0x94),
    ("typeDefinitions", 0xA0, 0xA4),
)

# 单字节值表：translate 一次完成整段 XOR（比逐字节 Python 循环快一个量级）
_XOR_TABLE = bytes((i ^ XOR_KEY) for i in range(256))


# ---------------------------------------------------------------- 工具

def _u16(b: bytes, off: int) -> int:
    return struct.unpack_from("<H", b, off)[0]


def _i32(b: bytes, off: int) -> int:
    return struct.unpack_from("<i", b, off)[0]


def _u32(b: bytes, off: int) -> int:
    return struct.unpack_from("<I", b, off)[0]


def _sha256(b: bytes) -> str:
    return hashlib.sha256(b).hexdigest()


# ---------------------------------------------------------------- 修复规则

def deobfuscate(raw: bytes) -> bytes:
    """三步修复：①首块 [0,4096) XOR ②每周期 [4096,20480) XOR ③写回 sanity+version。

    ① 与 ② 是**互斥区间**：首块只做一次，不能既算进周期窗口又被整体异或一遍。
    """
    n = len(raw)
    out = bytearray(raw)
    # ① 文件首块 [0, 4096)
    hi0 = min(WIN_LO, n)
    if hi0 > 0:
        out[0:hi0] = raw[0:hi0].translate(_XOR_TABLE)
    # ② 每个 64 KiB 周期内的 [4096, 20480)
    for base in range(0, n, PERIOD):
        lo, hi = base + WIN_LO, min(base + WIN_HI, n)
        if hi > lo:
            out[lo:hi] = raw[lo:hi].translate(_XOR_TABLE)
    # ③ magic + version
    out[0:4] = MAGIC
    out[4:8] = struct.pack("<I", VERSION)
    return bytes(out)


# ---------------------------------------------------------------- 全表自检

def method_histogram(b: bytes, hdr: dict) -> dict[int, int]:
    """按 Il2CppMethodDefinition.declaringType 统计每个类型的方法条数。"""
    mo, mc = hdr["methods"][0], hdr["methods"][1] // MD_SIZE
    grouped: dict[int, int] = {}
    for j in range(mc):
        dt = _i32(b, mo + j * MD_SIZE + 4)
        grouped[dt] = grouped.get(dt, 0) + 1
    return grouped


def td_score(b: bytes, hdr: dict, grouped: dict[int, int], size: int) -> tuple[int, int]:
    """按给定结构尺寸解析全部类型定义，统计 method_count == declaringType 分组数 的条数。"""
    to, tc = hdr["typeDefinitions"][0], hdr["typeDefinitions"][1] // size
    good = 0
    for i in range(tc):
        o = to + i * size
        if o + size > len(b):
            tc = i
            break
        if _u16(b, o + 0x44) == grouped.get(i, 0):
            good += 1
    return good, tc


def self_check(b: bytes) -> dict:
    """结构自检：magic / version / 11 段区间 / 三表规模 / td_size 唯一解 / 分组一致率 / token 高位。"""
    fails: list[str] = []
    n = len(b)
    hdr: dict[str, tuple[int, int]] = {}
    for name, off_at, cnt_at in REGIONS:
        hdr[name] = (_i32(b, off_at), _i32(b, cnt_at))

    magic_u32 = _u32(b, 0)
    if magic_u32 != MAGIC_U32:
        fails.append("magic 异常 0x%08x（应为 0x%08x）" % (magic_u32, MAGIC_U32))
    version = _u32(b, 4)
    if version != VERSION:
        fails.append("version=%d（应为 %d）" % (version, VERSION))

    # 11 段区间：非负、不越界、offset 严格递增
    prev = -1
    for name, _, _ in REGIONS:
        off, cnt = hdr[name]
        if off < 0 or cnt < 0:
            fails.append("%s: offset/count 为负 (%d/%d)" % (name, off, cnt))
        elif off + cnt > n:
            fails.append("%s: 区间越界 (%d+%d>%d)" % (name, off, cnt, n))
        if off < prev:
            fails.append("%s: offset 未递增 (%d<%d)" % (name, off, prev))
        prev = off
    if hdr["methods"][1] % MD_SIZE:
        fails.append("methodsCount=%d 不是 %d 的整数倍" % (hdr["methods"][1], MD_SIZE))
    if hdr["typeDefinitions"][1] % TD_SIZE_EXPECT:
        fails.append("typeDefinitionsCount=%d 不是 %#x 的整数倍"
                     % (hdr["typeDefinitions"][1], TD_SIZE_EXPECT))

    grouped = method_histogram(b, hdr)
    methods_n = hdr["methods"][1] // MD_SIZE
    types_n = hdr["typeDefinitions"][1] // TD_SIZE_EXPECT
    to = hdr["typeDefinitions"][0]

    # td_size 唯一解：三种候选尺寸全表打分
    td_cand = {}
    for size in TD_SIZE_GUESSES:
        good, checked = td_score(b, hdr, grouped, size)
        td_cand["0x%02x" % size] = {"good": good, "checked": checked,
                                    "frac": round(good / checked, 6) if checked else 0.0}
    best = max(td_cand.items(), key=lambda kv: kv[1]["frac"])
    td_size = int(best[0], 16)
    if td_size != TD_SIZE_EXPECT or best[1]["frac"] < 1.0:
        fails.append("td_size 判定失败：最佳 %s 一致率 %d/%d=%.2f%%（应 0x%02x 且 100%%）"
                     % (best[0], best[1]["good"], best[1]["checked"],
                        best[1]["frac"] * 100, TD_SIZE_EXPECT))

    # method_count vs declaringType 分组（全表）
    good = sum(1 for i in range(types_n)
               if _u16(b, to + i * TD_SIZE_EXPECT + 0x44) == grouped.get(i, 0))
    frac = good / types_n if types_n else 0.0
    if frac < 1.0:
        fails.append("method_count 与 declaringType 分组不一致：%d/%d = %.2f%%"
                     % (good, types_n, frac * 100))

    # 与解析器同口径的两个计数（锚点 JSON 里记作 types_with_method_start / method_count_equal_grouped）
    direct = crossok = 0
    for i in range(types_n):
        o = to + i * TD_SIZE_EXPECT
        ms, mc = _i32(b, o + 0x24), _u16(b, o + 0x44)
        if 0 <= ms and ms + mc <= methods_n:
            direct += 1
            if grouped.get(i, 0) == mc:
                crossok += 1

    # token 高位一致性（类型 0x02 / 方法 0x06）—— 一错必崩的强约束
    type_tok_bad = sum(1 for i in range(types_n)
                       if (_u32(b, to + i * TD_SIZE_EXPECT + 0x58) >> 24) != 0x02)
    mo = hdr["methods"][0]
    method_tok_bad = sum(1 for j in range(methods_n)
                         if (_u32(b, mo + j * MD_SIZE + 0x14) >> 24) != 0x06)
    if type_tok_bad:
        fails.append("类型 token 高位非 0x02：%d/%d" % (type_tok_bad, types_n))
    if method_tok_bad:
        fails.append("方法 token 高位非 0x06：%d/%d" % (method_tok_bad, methods_n))

    argc_bad = sum(1 for j in range(methods_n) if _u16(b, mo + j * MD_SIZE + 0x1E) > 16)

    return {
        "ok": not fails,
        "failures": fails,
        "magic": "0x%08x" % magic_u32,
        "version": version,
        "size": n,
        "regions": {k: {"offset": v[0], "count": v[1]} for k, v in hdr.items()},
        "types": types_n,
        "methods": methods_n,
        "td_size": "0x%02x" % td_size,
        "td_candidates": td_cand,
        "method_count_vs_grouped": {"good": good, "checked": types_n, "frac": round(frac, 6)},
        "types_with_method_start": direct,
        "method_count_equal_grouped": crossok,
        "type_token_bad": type_tok_bad,
        "method_token_bad": method_tok_bad,
        "argc_gt16": argc_bad,
        "baseline_2_7_91": BASELINE,
    }


def print_check(chk: dict) -> None:
    h = chk["regions"]
    print("[check] ===== 结构自检 =====")
    print("[check] magic                 = %s  (expect 0x%08x)" % (chk["magic"], MAGIC_U32))
    print("[check] version               = %d  (expect %d)" % (chk["version"], VERSION))
    print("[check] size                  = %d B" % chk["size"])
    for k in ("string", "methods", "parameters", "nestedTypes", "typeDefinitions"):
        print("[check] %-20s = %d / %d" % (k, h[k]["offset"], h[k]["count"]))
    print("[check] types                 = %d   (baseline %d)"
          % (chk["types"], BASELINE["types"]))
    print("[check] methods               = %d (baseline %d)"
          % (chk["methods"], BASELINE["methods"]))
    print("[check] td_size               = %s  (expect 0x%02x)"
          % (chk["td_size"], TD_SIZE_EXPECT))
    for k, v in chk["td_candidates"].items():
        flag = "<== 采用" if k == chk["td_size"] else ""
        print("[check]   td=%s: method_count 一致 %d/%d = %.2f%% %s"
              % (k, v["good"], v["checked"], v["frac"] * 100, flag))
    g = chk["method_count_vs_grouped"]
    print("[check] method_count vs declaringType 分组（全表） = %d/%d = %.2f%%"
          % (g["good"], g["checked"], g["frac"] * 100))
    print("[check] types_with_method_start       = %d (baseline %d)"
          % (chk["types_with_method_start"], BASELINE["types_with_method_start"]))
    print("[check] method_count_equal_grouped    = %d (baseline %d)"
          % (chk["method_count_equal_grouped"], BASELINE["method_count_equal_grouped"]))
    print("[check] 类型 token 高位非 0x02        = %d/%d"
          % (chk["type_token_bad"], chk["types"]))
    print("[check] 方法 token 高位非 0x06        = %d/%d"
          % (chk["method_token_bad"], chk["methods"]))
    print("[check] argc>16 方法                  = %d/%d" % (chk["argc_gt16"], chk["methods"]))


# ---------------------------------------------------------------- 主流程

def resolve_apk_from_json(meta_json: Path) -> tuple[Path, str]:
    """从 official_apk.json 取底包路径；path 失效时按 version_name 回退 02_apks/<游戏>/。

    与 dump_official_runtime_mrfzz.sh 的 resolve_apk 同口径：另一台机器写下的绝对路径
    在本机必然不存在，靠版本号在 02_apks 里找同名包。
    """
    meta = json.loads(meta_json.read_text(encoding="utf-8"))
    raw = meta.get("apk_path") or ""
    cand = Path(raw) if raw else Path("")
    if raw and cand.is_file():
        return cand, ""
    ver = meta.get("version_name") or ""
    root = meta_json.resolve().parents[2]
    game_dir = root / "02_apks" / meta.get("game", "") if meta.get("game") else root / "02_apks" / "明日方舟"
    hits = sorted(game_dir.glob("*%s*.apk" % ver)) if ver else []
    if hits:
        return hits[0], "fallback by version_name=%s" % ver
    return cand, "未命中回退"


def read_apk_entry(apk: Path, entry: str) -> tuple[bytes, dict]:
    with zipfile.ZipFile(apk) as z:
        try:
            info = z.getinfo(entry)
        except KeyError:
            raise SystemExit("错误: APK 内无条目 %s" % entry)
        data = z.read(entry)
    return data, {
        "kind": "apk",
        "apk": str(apk),
        "apk_size": apk.stat().st_size,
        "entry": entry,
        "entry_size": info.file_size,
        "entry_crc32": "%08x" % info.CRC,
    }


def main() -> int:
    ap = argparse.ArgumentParser(
        description="明日方舟官方 global-metadata.dat 弱混淆静态修复 + 全表结构自检")
    src = ap.add_mutually_exclusive_group()
    src.add_argument("--apk", help="底包 APK（直接读包内 global-metadata.dat，不落中间文件）")
    src.add_argument("--apk-from", dest="apk_from",
                     help="从 official_apk.json 取 apk_path（找不到时按 version_name 回退 02_apks/<游戏>/）")
    src.add_argument("--in", dest="inp", help="已解出的 raw global-metadata.dat")
    src.add_argument("--verify", help="只校验一份已修复的 metadata（不写任何文件）")
    ap.add_argument("--out", default=OUT_DEFAULT, help="输出目录（默认 %s）" % OUT_DEFAULT)
    ap.add_argument("--entry", default=ENTRY_DEFAULT,
                    help="APK 内条目路径（默认 %s）" % ENTRY_DEFAULT)
    ap.add_argument("--force", action="store_true", help="输出已存在时覆盖")
    args = ap.parse_args()

    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")

    # ---- 只校验模式
    if args.verify:
        blob = Path(args.verify).read_bytes()
        chk = self_check(blob)
        print_check(chk)
        if not chk["ok"]:
            print()
            for f in chk["failures"]:
                print("[check] ✗ %s" % f)
            return 3
        print()
        print("[verify] OK %s" % args.verify)
        return 0

    if not args.apk and not args.apk_from and not args.inp:
        ap.error("需要 --apk / --apk-from / --in / --verify 之一")

    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)
    dst = out_dir / "global-metadata.fixed.dat"
    if dst.exists() and not args.force:
        ap.error("输出已存在：%s（加 --force 覆盖）" % dst)

    # ---- 读输入
    if args.apk_from:
        apk, hint = resolve_apk_from_json(Path(args.apk_from))
        print("[in ] apk_from=%s -> %s%s" % (args.apk_from, apk, (" (" + hint + ")") if hint else ""))
        if not apk.is_file():
            print("错误: APK 不存在: %s（official_apk.json 与 02_apks 回退均未命中）" % apk,
                  file=sys.stderr)
            return 2
        raw, src_info = read_apk_entry(apk, args.entry)
    elif args.apk:
        apk = Path(args.apk)
        if not apk.is_file():
            print("错误: APK 不存在: %s" % apk, file=sys.stderr)
            return 2
        raw, src_info = read_apk_entry(apk, args.entry)
    else:
        raw_path = Path(args.inp)
        if not raw_path.is_file():
            print("错误: 输入文件不存在: %s" % raw_path, file=sys.stderr)
            return 2
        raw = raw_path.read_bytes()
        src_info = {"kind": "file", "path": str(raw_path)}

    raw_sha, raw_size = _sha256(raw), len(raw)
    print("[in ] source=%s" % (src_info.get("apk") or src_info.get("path")))
    if src_info["kind"] == "apk":
        print("[in ] entry=%s size=%d crc32=%s"
              % (src_info["entry"], src_info["entry_size"], src_info["entry_crc32"]))
    print("[in ] raw_size=%d raw_sha256=%s" % (raw_size, raw_sha))
    print("[in ] raw_head16=%s" % raw[:16].hex())

    # ---- 幂等：magic+version 已是修复态且全表自洽 ⇒ 原样输出（不二次异或）
    magic_ok = raw[0:4] == MAGIC and _u32(raw, 4) == VERSION
    report = {
        "schema": "nixiang.mrfzz.metadata_fix.v1",
        "game": "明日方舟",
        "generated_at": _dt.datetime.now().isoformat(timespec="seconds"),
        "input": dict(src_info, raw_size=raw_size, raw_sha256=raw_sha),
        "rule": {
            "xor_key": "0x%02x" % XOR_KEY,
            "period": PERIOD,
            "window_per_period": [WIN_LO, WIN_HI],
            "first_block": [0, WIN_LO],
            "magic_written": MAGIC.hex(),
            "version_written": VERSION,
        },
        "output": {"path": str(dst)},
    }

    if magic_ok:
        chk = self_check(raw)
        if chk["ok"]:
            fixed, state = raw, "already_fixed"
            print("[fix] 输入已是修复态（magic/version 正确且全表自洽）⇒ 跳过 XOR，原样输出（幂等）")
        else:
            fixed, state = None, "half_fixed"
            print("[fix] ❌ 首 8 字节是 magic+version24，但全表自检不自洽")
            print("       ⇒ 这是「只补 magic / 首块被异或两次」的中间产物，正文仍是混淆态；")
            print("         正确输入是原始 disk 文件（未修复态，首 8 字节 94437212184b9eec）。")
            report.update({"state": state, "self_check": chk, "ok": False})
            bad = out_dir / "meta_fix.failed.json"
            bad.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n",
                           encoding="utf-8")
            for f in chk["failures"]:
                print("[check] ✗ %s" % f)
            print("[check] 诊断已写: %s" % bad)
            return 3
    else:
        state = "deobfuscated"
        fixed = deobfuscate(raw)
        print("[fix] 规则: 首块 [0,%d) XOR 0x%02x + 每 %d 周期内 [%d,%d) XOR 0x%02x；"
              "首 8 字节 = %s + version %d"
              % (WIN_LO, XOR_KEY, PERIOD, WIN_LO, WIN_HI, XOR_KEY, MAGIC.hex(), VERSION))
        chk = self_check(fixed)

    changed = sum(1 for a, b in zip(raw, fixed) if a != b)
    fixed_sha = _sha256(fixed)
    print("[fix] bytes_changed=%d/%d fixed_sha256=%s" % (changed, raw_size, fixed_sha))
    print()
    print_check(chk)

    report.update({
        "state": state,
        "bytes_changed": changed,
        "output": dict(report["output"], size=len(fixed), sha256=fixed_sha),
        "self_check": chk,
        "ok": chk["ok"],
    })

    if not chk["ok"]:
        bad = out_dir / "meta_fix.failed.json"
        bad.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print()
        print("[check] ❌ 结构自检不自洽 —— 不产出 metadata 文件：")
        for f in chk["failures"]:
            print("         - %s" % f)
        print("[check] 诊断已写: %s" % bad)
        return 3

    dst.write_bytes(fixed)
    rep = out_dir / "meta_fix.json"
    rep.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print()
    print("[check] ✅ 结构自检全部自洽（state=%s）" % state)
    print("[out ] %s  (%d B, sha256=%s)" % (dst, len(fixed), fixed_sha))
    print("[out ] %s" % rep)
    return 0


if __name__ == "__main__":
    sys.exit(main())
