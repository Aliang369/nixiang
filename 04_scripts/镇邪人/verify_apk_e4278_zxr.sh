#!/usr/bin/env bash
# 镇邪人 e4278 正式包静态验收：身份、签名、三份战斗 JSC、缓存优先级与 so 标签。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_e4278_star-releaseSigned.apk}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }

echo "[1/4] 包身份与签名"
"$AAPT2" dump badging "$APK" | grep -F "package: name='com.zxr.dlxmz.zxrwz37zgx10.gg9b' versionCode='11' versionName='1.0.3'"
"$APKSIGNER" verify --print-certs "$APK" | grep -F "Signer #1 certificate SHA-256 digest: 83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

echo "[2/4] e4278/ca0d4/d5507 战斗补丁"
"$PYTHON_BIN" - "$APK" <<'PY'
import hashlib
import sys
import zipfile
import zlib
import xxtea

z = zipfile.ZipFile(sys.argv[1])
key = b"c25d728a-a9d9-4c"
for entry in (
    "assets/assets/src/index.e4278.jsc",
    "assets/assets/src/index.ca0d4.jsc",
    "assets/assets/src/index.d5507.jsc",
):
    packed = xxtea.decrypt(z.read(entry), key, padding=False)
    length = int.from_bytes(packed[-4:], "little")
    js = zlib.decompress(packed[:length], 16 + zlib.MAX_WBITS)
    assert b"starry_zxr_safe_v2" in js, entry
    for tok in (b"__starryZxrOn(1)", b"__starryZxrOn(2)", b"__starryZxrOn(3)", b"__starryZxrOn(4)"):
        assert tok in js, (entry, tok)
    print(f"  {entry}: decrypt ok, marker+4 hooks, sha256={hashlib.sha256(js).hexdigest()}")
PY

echo "[3/4] 引擎缓存优先级"
"$PYTHON_BIN" - "$APK" <<'PY'
import sys
import zipfile

z = zipfile.ZipFile(sys.argv[1])
eng = z.read("assets/jsb-adapter/jsb-engine.js")
new = b"var packagePath = getPackagePathFromRemoteUrl(url);\n    if (checkExistsInPackage(packagePath)) {\n      inLocal = true;\n      url = packagePath;\n    } else {\n      var cache = cacheManager.getCache(url);"
old = b"var cache = cacheManager.getCache(url);\n    if (cache) {\n      inCache = true;\n      url = cache;\n    } else {\n      var packagePath = getPackagePathFromRemoteUrl(url);"
assert new in eng and old not in eng
for tail in ("config.e4278.json", "index.e4278.jsc", "config.ca0d4.json", "index.ca0d4.jsc", "config.d5507.json", "index.d5507.jsc"):
    assert "assets/assets/src/" + tail in z.namelist(), tail
print("  package-first cache precedence: ok")
PY

echo "[4/4] 菜单 so 与 OTA 载荷"
"$PYTHON_BIN" - "$APK" <<'PY'
import hashlib
import sys
import zipfile

z = zipfile.ZipFile(sys.argv[1])
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zxr]" in so and b"zxr\0" in so and b"BEGIN CERTIFICATE" in so
c8 = z.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8
print("  classes8 sha256=" + hashlib.sha256(c8).hexdigest())
print("  so size=" + str(len(so)) + " sha256=" + hashlib.sha256(so).hexdigest())
PY

echo "verify=pass"
