#!/usr/bin/env python3.11
"""勇士之心：核对 TapTap 原版身份、签名与核心输入，并与斌哥包对照。"""
from __future__ import annotations

import hashlib
import json
import subprocess
import sys
import zipfile
from pathlib import Path

ROOT = Path("/Users/aliang/Documents/nixiang")
SCRIPT_DIR = ROOT / "04_scripts/勇士之心"
MOD_META = SCRIPT_DIR / "binge_apk.json"
OFF_META = SCRIPT_DIR / "official_apk.json"
OUT_JSON = SCRIPT_DIR / "official_check.json"
MANIFEST_TXT = SCRIPT_DIR / "official_manifest.txt"
SIGNATURE_TXT = SCRIPT_DIR / "official_signature.txt"
COMPARE_TXT = SCRIPT_DIR / "official_compare.txt"
AAPT2 = Path("/Users/aliang/Library/Android/sdk/build-tools/36.0.0/aapt2")
APKSIGNER = Path("/Users/aliang/Library/Android/sdk/build-tools/36.0.0/apksigner")

ENTRIES = [
    "lib/arm64-v8a/libcocos2djs.so",
    "lib/arm64-v8a/libSDKloading.so",
    "assets/main.js",
    "assets/src/cocos2d-jsb.js",
    "assets/src/settings.js",
    "assets/assets/main/index.js",
]


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def hash_entry(apk: Path, entry: str) -> str:
    with zipfile.ZipFile(apk) as z:
        if entry not in z.namelist():
            return "MISSING"
        return hashlib.sha256(z.read(entry)).hexdigest()


def main() -> int:
    if not MOD_META.is_file() or not OFF_META.is_file():
        print("missing meta json", file=sys.stderr)
        return 2

    mod_meta = json.loads(MOD_META.read_text(encoding="utf-8"))
    off_meta = json.loads(OFF_META.read_text(encoding="utf-8"))
    mod_apk = Path(mod_meta["apk_path"])
    off_apk = Path(off_meta["apk_path"])
    for path in (mod_apk, off_apk):
        if not path.is_file():
            print(f"missing apk: {path}", file=sys.stderr)
            return 2
    for tool in (AAPT2, APKSIGNER):
        if not tool.is_file():
            print(f"missing tool: {tool}", file=sys.stderr)
            return 2

    manifest = subprocess.run(
        [str(AAPT2), "dump", "badging", str(off_apk)],
        check=False,
        text=True,
        capture_output=True,
    )
    if manifest.returncode != 0:
        print(manifest.stderr, file=sys.stderr)
        return manifest.returncode
    MANIFEST_TXT.write_text(manifest.stdout, encoding="utf-8")

    signature = subprocess.run(
        [str(APKSIGNER), "verify", "--print-certs", str(off_apk)],
        check=False,
        text=True,
        capture_output=True,
    )
    if signature.returncode != 0:
        print(signature.stderr, file=sys.stderr)
        return signature.returncode
    SIGNATURE_TXT.write_text(signature.stdout, encoding="utf-8")

    lines = manifest.stdout.splitlines()
    package = ""
    version_name = ""
    version_code = ""
    activity = ""
    for line in lines:
        if line.startswith("package:"):
            for token in line.split():
                if token.startswith("name="):
                    package = token.split("'", 2)[1]
                elif token.startswith("versionName="):
                    version_name = token.split("'", 2)[1]
                elif token.startswith("versionCode="):
                    version_code = token.split("'", 2)[1]
        elif line.startswith("launchable-activity:"):
            activity = line.split("name='", 1)[1].split("'", 1)[0]

    sig_lines = signature.stdout.splitlines()
    signer_dn = ""
    signer_sha256 = ""
    for line in sig_lines:
        if line.startswith("Signer #1 certificate DN:"):
            signer_dn = line.split(":", 1)[1].strip()
        elif line.startswith("Signer #1 certificate SHA-256 digest:"):
            signer_sha256 = line.split(":", 1)[1].strip()

    compare = {
        entry: {
            "official_sha256": hash_entry(off_apk, entry),
            "mod_sha256": hash_entry(mod_apk, entry),
        }
        for entry in ENTRIES
    }
    for item in compare.values():
        item["match"] = item["official_sha256"] == item["mod_sha256"]

    compare_lines = [
        "# 勇士之心 TapTap 原版 / 斌哥包核心输入对照",
        "",
        f"- mod: {mod_apk.name} md5={mod_meta.get('md5')}",
        f"- official: {off_apk.name} md5={hashlib.md5(off_apk.read_bytes()).hexdigest()}",
        "",
        f"{'entry':52} {'official_sha256':64} match_mod",
    ]
    for entry, item in compare.items():
        compare_lines.append(
            f"{entry:52} {item['official_sha256']:64} "
            f"{'yes' if item['match'] else 'NO mod=' + item['mod_sha256']}"
        )
    COMPARE_TXT.write_text("\n".join(compare_lines) + "\n", encoding="utf-8")

    data = {
        "game": "勇士之心",
        "mod_apk": str(mod_apk),
        "mod_md5": mod_meta.get("md5"),
        "official_apk": str(off_apk),
        "official_md5": hashlib.md5(off_apk.read_bytes()).hexdigest(),
        "official_sha256": sha256_file(off_apk),
        "official_size": off_apk.stat().st_size,
        "package": package,
        "version_name": version_name,
        "version_code": version_code,
        "launchable_activity": activity,
        "signer_dn": signer_dn,
        "signer_sha256": signer_sha256,
        "channel": off_meta.get("channel"),
        "compare": compare,
    }
    OUT_JSON.write_text(
        json.dumps(data, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    print(COMPARE_TXT.read_text(encoding="utf-8"))
    print(OUT_JSON.read_text(encoding="utf-8"))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
