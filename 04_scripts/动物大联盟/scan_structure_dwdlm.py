#!/usr/bin/env python3
"""动物大联盟 官方原包结构扫描（只读）。

产出：04_scripts/动物大联盟/official_structure.json / .txt
覆盖：DEX 清单与占用（首个空闲 dex）、TapSDK Gatekeeper 落点、
      native so 清单、APKSecure 壳资源（加密 payload / 私有 so 区）、assets 关键运行时文件。
不修改 APK、不解包落盘，不做玩法 hook / RVA / metadata 分析。
"""
import hashlib
import io
import json
import re
import sys
import zipfile
from collections import Counter
from pathlib import Path

HERE = Path(__file__).resolve().parent
META = HERE / "official_apk.json"
OUT_JSON = HERE / "official_structure.json"
OUT_TXT = HERE / "official_structure.txt"

DEX_RE = re.compile(r"^classes(\d*)\.dex$")

# dex string_data_item 是连续 MUTF-8/ASCII，ASCII 子串可直接在字节流里检索
PROBES = {
    "tapsdk": [b"com/taptap/sdk/", b"TapLoginHelper", b"TapTapSdk"],
    "gatekeeper": [b"GatekeeperRepository", b"isGatekeeperLoadedSuccessfully"],
    "apksecure": [b"dev/mars/secure/", b"APKSecure", b"apksecurefile"],
    "starry": [b"com/starry/magic/"],
    "cocos_js": [b"cocos2d-jsb", b"jsb.reflection"],
}

SHELL_LIBS = {
    "libsecret.so": "APKSecure 状态机（1.0.23 实测 G2 关键，双映像归一化）",
    "libEncryptorP.so": "加密/资源保护",
    "libBugly.so": "腾讯 Bugly",
    "libnesec.so": "网易易盾",
    "libjiagu.so": "360 加固",
    "libDexHelper.so": "梆梆加固",
    "libtapsdkcore.so": "TapSDK native 核心",
    "libzeus_direct_dex.so": "直接加载 dex 的引导库（壳侧）",
    "libterrain.so": "壳/插件引导候选",
    "libluster.so": "壳/反调试候选",
    "libsgcore.so": "SS 分包/加载候选",
}

NAME_RE = re.compile(r':name\(0x[0-9a-f]+\)="([^"]*)"')


def dex_header_info(raw):
    if len(raw) < 0x70 or raw[:4] != b"dex\n":
        return None
    try:
        version = raw[4:7].decode("ascii")
    except UnicodeDecodeError:
        version = None
    return {
        "dex_version": version,
        "string_ids": int.from_bytes(raw[0x38:0x3C], "little"),
        "method_ids": int.from_bytes(raw[0x58:0x5C], "little"),
        "class_defs": int.from_bytes(raw[0x60:0x64], "little"),
    }


def probe(blob):
    hits = []
    for tag, needles in PROBES.items():
        if any(n in blob for n in needles):
            hits.append(tag)
    return hits


def main():
    if not META.is_file():
        sys.exit("错误: 缺少 %s（先跑 pull_official）" % META)
    meta = json.loads(META.read_text(encoding="utf-8"))
    apk = Path(meta["apk_path"])
    if not apk.is_file():
        sys.exit("错误: APK 不存在: %s" % apk)

    dex_list = []
    dex_occupancy = {}
    probes_hit = {}
    native = []
    shell_markers = {}
    shell_payload = []
    packed_native = []
    key_assets = []
    assets = {"count": 0, "jsc": 0, "js": 0}
    top_dirs = Counter()
    entries_total = 0
    total_uncompressed = 0

    with zipfile.ZipFile(apk) as zf:
        for info in zf.infolist():
            if info.is_dir():
                continue
            entries_total += 1
            total_uncompressed += info.file_size
            name = info.filename
            parts = name.split("/")
            if len(parts) >= 2:
                top_dirs[parts[0]] += 1

            m = DEX_RE.match(name)
            if m:
                raw = zf.read(name)
                rec = {
                    "name": name,
                    "size": info.file_size,
                    "sha256": hashlib.sha256(raw).hexdigest(),
                    "class_defs": None,
                    "dex_version": None,
                }
                hdr = dex_header_info(raw)
                if hdr:
                    rec.update(hdr)
                rec["probe_hits"] = probe(raw)
                dex_list.append(rec)
                idx = 1 if name == "classes.dex" else int(m.group(1))
                dex_occupancy[idx] = name
                for tag in rec["probe_hits"]:
                    probes_hit.setdefault(tag, []).append(name)
                continue

            if name.startswith("lib/") and name.endswith(".so"):
                base = parts[-1]
                native.append({
                    "path": name,
                    "abi": parts[1] if len(parts) > 2 else None,
                    "size": info.file_size,
                    "sha256": hashlib.sha256(zf.read(name)).hexdigest(),
                })
                if base in SHELL_LIBS:
                    shell_markers[base] = SHELL_LIBS[base]
                continue

            if name.startswith("assets/apksecuresolib/") and name.endswith(".so"):
                raw = zf.read(name)
                packed_native.append({
                    "path": name,
                    "abi": parts[2] if len(parts) > 3 else None,
                    "size": info.file_size,
                    "sha256": hashlib.sha256(raw).hexdigest(),
                    "magic": raw[:4].hex(),
                    "is_elf": raw[:4] == b"\x7fELF",
                })
                continue

            if name.startswith("assets/apksecure") or re.match(r"^assets/\d{6,}$", name):
                raw = zf.read(name) if info.file_size <= 16 * 1048576 else b""
                nested = None
                if raw[:2] == b"PK":
                    try:
                        nz = zipfile.ZipFile(io.BytesIO(raw))
                        sub = [n for n in nz.namelist() if not n.endswith("/")]
                        tops = Counter(n.split("/")[0] for n in sub)
                        nested = {
                            "entries": len(sub),
                            "top_dirs": dict(tops.most_common(5)),
                            "has_dex": any(n.endswith(".dex") for n in sub),
                            "has_so": any(n.endswith(".so") for n in sub),
                        }
                    except Exception:
                        nested = None
                is_zip = raw[:2] == b"PK"
                is_dex = raw[:4] == b"dex\n"
                is_elf = raw[:4] == b"\x7fELF"
                shell_payload.append({
                    "path": name,
                    "size": info.file_size,
                    "sha256": hashlib.sha256(zf.read(name)).hexdigest(),
                    "magic": raw[:8].hex() if raw else None,
                    "has_dex_magic": is_dex,
                    "is_zip": is_zip,
                    "is_elf": is_elf,
                    "encrypted_payload": bool(raw) and not (is_zip or is_dex or is_elf),
                    "nested": nested,
                    "probe_hits": probe(raw) if raw else [],
                })
                continue

            if name in ("assets/main.js", "assets/project.json", "assets/app_info.json",
                        "assets/whiteList.json", "assets/supplierconfig.json") or \
               name.startswith("assets/jsb-adapter/") or re.match(r"^assets/src/", name):
                key_assets.append({"path": name, "size": info.file_size})
                continue

            if name.startswith("assets/"):
                assets["count"] += 1
                if name.endswith(".jsc"):
                    assets["jsc"] += 1
                elif name.endswith(".js"):
                    assets["js"] += 1

    def first_free():
        idx = 1
        while idx in dex_occupancy:
            idx += 1
        return "classes.dex" if idx == 1 else "classes%d.dex" % idx

    dex_list.sort(key=lambda r: (0 if r["name"] == "classes.dex" else 1, r["name"]))
    native.sort(key=lambda r: r["path"])
    shell_payload.sort(key=lambda r: r["path"])

    gate_dex = sorted({d for tag in ("gatekeeper", "tapsdk") for d in probes_hit.get(tag, [])})
    all_probes_flat = sorted(probes_hit)

    data = {
        "apk": str(apk),
        "version_name": meta.get("version_name"),
        "version_code": meta.get("version_code"),
        "sha256": meta.get("sha256"),
        "size": meta.get("size"),
        "entries_total": entries_total,
        "total_uncompressed": total_uncompressed,
        "top_dirs": dict(top_dirs.most_common(15)),
        "dex": dex_list,
        "dex_count": len(dex_list),
        "dex_occupancy": {str(k): v for k, v in sorted(dex_occupancy.items())},
        "first_free_dex": first_free(),
        "static_dex_probe_hits": all_probes_flat,
        "tap_gatekeeper": {
            "present_in_static_dex": bool(probes_hit.get("gatekeeper")),
            "dex": probes_hit.get("gatekeeper", []),
            "tapsdk_dex": probes_hit.get("tapsdk", []),
            "note": "静态 dex 未命中不等于运行时不存在：壳会把真实 dex 解密到内存（见 shell_payload / 1.0.23 内存 dump 先例）",
        },
        "native": native,
        "abis": sorted({r["abi"] for r in native if r["abi"]}),
        "shell_markers": shell_markers,
        "shell_payload": shell_payload,
        "packed_native": packed_native,
        "key_assets": key_assets,
        "assets": assets,
    }
    OUT_JSON.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    lines = [
        "apk=%s" % apk,
        "version=%s/%s" % (data["version_name"], data["version_code"]),
        "sha256=%s" % data["sha256"],
        "size=%s entries=%d uncompressed=%d" % (data["size"], entries_total, total_uncompressed),
        "abis=%s native_libs=%d" % (",".join(data["abis"]), len(native)),
        "dex_count=%d first_free_dex=%s" % (data["dex_count"], data["first_free_dex"]),
        "static_dex_probes=%s" % (",".join(all_probes_flat) or "<none>"),
        "tap_gatekeeper_in_static_dex=%d dex=%s" % (
            1 if data["tap_gatekeeper"]["present_in_static_dex"] else 0,
            ",".join(data["tap_gatekeeper"]["dex"]) or "<none>"),
        "shell_markers=%s" % (",".join(sorted(shell_markers)) or "<none>"),
        "assets_count=%d jsc=%d js=%d" % (assets["count"], assets["jsc"], assets["js"]),
    ]
    for r in dex_list:
        lines.append("dex %s size=%d class_defs=%s ver=%s probes=%s" % (
            r["name"], r["size"], r["class_defs"], r["dex_version"],
            ",".join(r["probe_hits"]) or "-"))
    for r in shell_payload:
        lines.append("shell_payload %s size=%d magic=%s dex=%d zip=%d elf=%d encrypted=%d nested=%s probes=%s" % (
            r["path"], r["size"], r["magic"], 1 if r["has_dex_magic"] else 0,
            1 if r.get("is_zip") else 0, 1 if r.get("is_elf") else 0,
            1 if r.get("encrypted_payload") else 0,
            (str(r["nested"]) if r.get("nested") else "-"),
            ",".join(r["probe_hits"]) or "-"))
    for r in packed_native:
        lines.append("packed_so %s size=%d elf=%d" % (r["path"], r["size"], 1 if r["is_elf"] else 0))
    for r in key_assets:
        lines.append("asset %s size=%d" % (r["path"], r["size"]))
    for r in native:
        lines.append("so %s size=%d sha256=%s" % (r["path"], r["size"], r["sha256"][:16]))
    OUT_TXT.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print("\n".join(lines[:16]))


if __name__ == "__main__":
    main()
