#!/usr/bin/env bash
# 镇邪人候选包静态验收：签名、包结构、SO/JSC 源产物一致性。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_safe-releaseSigned.apk}"
AAPT2="${AAPT2:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-/Users/aliang/Library/Android/sdk/build-tools/36.0.0/apksigner}"
SO="$ROOT/mod菜单/starry-mod-menu/app/src/libs/arm64-v8a/libstarry_magic.so"
JSC_D5507="$ROOT/07_patched/镇邪人/assets/src/index.d5507.jsc"
JSC_B5278="$ROOT/07_patched/镇邪人/b5278/index.b5278.jsc"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: SO missing: $SO" >&2; exit 1; }
[[ -f "$JSC_D5507" ]] || { echo "ERROR: patched d5507 JSC missing: $JSC_D5507" >&2; exit 1; }
[[ -f "$JSC_B5278" ]] || { echo "ERROR: patched b5278 JSC missing: $JSC_B5278" >&2; exit 1; }

BADGING="$("$AAPT2" dump badging "$APK")"
printf '%s\n' "$BADGING" | grep -Fq "package: name='com.zxr.dlxmz.zxrwz37zgx10.gg9b' versionCode='11' versionName='1.0.3'"
"$APKSIGNER" verify --verbose "$APK" >/dev/null
[[ "$(unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so | shasum -a 256 | awk '{print $1}')" == "$(shasum -a 256 "$SO" | awk '{print $1}')" ]]
[[ "$(unzip -p "$APK" assets/assets/src/index.d5507.jsc | shasum -a 256 | awk '{print $1}')" == "$(shasum -a 256 "$JSC_D5507" | awk '{print $1}')" ]]
[[ "$(unzip -p "$APK" assets/assets/src/index.b5278.jsc | shasum -a 256 | awk '{print $1}')" == "$(shasum -a 256 "$JSC_B5278" | awk '{print $1}')" ]]

"$PYTHON_BIN" - "$APK" <<'PY'
import hashlib
import sys
import zipfile
import zlib
from pathlib import Path

import xxtea

apk = Path(sys.argv[1])
key = b"c25d728a-a9d9-4c"
with zipfile.ZipFile(apk) as z:
    for entry in ("assets/assets/src/index.d5507.jsc", "assets/assets/src/index.b5278.jsc"):
        packed = xxtea.decrypt(z.read(entry), key, padding=False)
        dec = zlib.decompressobj(16 + zlib.MAX_WBITS)
        js = dec.decompress(packed) + dec.flush()
        if not dec.eof:
            raise SystemExit(f"ERROR: {entry} gzip stream incomplete")
        if b"starry_zxr_safe_v2" not in js:
            raise SystemExit(f"ERROR: {entry} safe marker missing")
        print(f"jsc_marker={entry}:starry_zxr_safe_v2")
    classes8 = z.read("classes8.dex")
    if b"MenuSafeHandler" not in classes8 or b"ZxrHooks" not in classes8:
        raise SystemExit("ERROR: classes8 missing safe menu/ZxrHooks")
    so = z.read("lib/arm64-v8a/libstarry_magic.so")
    if b"[Zxr]" not in so or b"starry_zxr_safe_v2" not in so:
        raise SystemExit("ERROR: final so missing ZXR safe marker")
    print("classes8=MenuSafeHandler+ZxrHooks")
    print("apk_sha256=" + hashlib.sha256(apk.read_bytes()).hexdigest())
PY

echo "verify=pass"
