#!/usr/bin/env bash
# 镇邪人 0d1ae 正式包静态验收：身份、签名、四份钉入战斗 JSC、缓存优先级、
# 菜单 so/dex 与 native 热更兜底（内容指纹版）标签。
#
# 背景：服务器热更 src 从 e4278 漂到 0d1ae，包内只钉了 e4278/ca0d4/d5507，
# 引擎「包内命中优先」落空后走缓存下载未修补的 0d1ae，四项战斗功能静默失效。
# 本轮把 0d1ae 也钉进包内，同时把 native se::evalString 兜底从「文件名白名单」
# 改成「内容指纹」，避免下次再漂。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_0d1ae_star-releaseSigned.apk}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"
PYTHON_BIN="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }

echo "[1/4] 包身份与签名"
"$AAPT2" dump badging "$APK" | grep -F "package: name='com.zxr.dlxmz.zxrwz37zgx10.gg9b' versionCode='11' versionName='1.0.3'"
"$APKSIGNER" verify --print-certs "$APK" | grep -F "Signer #1 certificate SHA-256 digest: 83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

echo "[2/4] 0d1ae/e4278/ca0d4/d5507 四份战斗补丁"
"$PYTHON_BIN" - "$APK" <<'PY'
import hashlib
import sys
import zipfile
import zlib
import xxtea

z = zipfile.ZipFile(sys.argv[1])
key = b"c25d728a-a9d9-4c"
for ver in ("0d1ae", "e4278", "ca0d4", "d5507"):
    entry = f"assets/assets/src/index.{ver}.jsc"
    packed = xxtea.decrypt(z.read(entry), key, padding=False)
    length = int.from_bytes(packed[-4:], "little")
    js = zlib.decompress(packed[:length], 16 + zlib.MAX_WBITS)
    assert b"starry_zxr_safe_v2" in js, entry
    if ver == "d5507":
        # d5507 是更早的一版脚本，全屏秒杀锚点写法不同，只校验另外两处
        hooks = (b"__starryZxrOn(1)", b"__starryZxrOn(2)", b"__starryZxrOn(3)")
    else:
        hooks = (b"__starryZxrOn(1)", b"__starryZxrOn(2)", b"__starryZxrOn(3)", b"__starryZxrOn(4)")
    for tok in hooks:
        assert tok in js, (entry, tok)
    print(f"  {entry}: decrypt ok, marker+hooks, sha256={hashlib.sha256(js).hexdigest()}")
PY

echo "[3/4] 引擎缓存优先级 + 四份 pin 的 config 齐全"
"$PYTHON_BIN" - "$APK" <<'PY'
import sys
import zipfile

z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
eng = z.read("assets/jsb-adapter/jsb-engine.js")
new = b"var packagePath = getPackagePathFromRemoteUrl(url);\n    if (checkExistsInPackage(packagePath)) {\n      inLocal = true;\n      url = packagePath;\n    } else {\n      var cache = cacheManager.getCache(url);"
old = b"var cache = cacheManager.getCache(url);\n    if (cache) {\n      inCache = true;\n      url = cache;\n    } else {\n      var packagePath = getPackagePathFromRemoteUrl(url);"
assert new in eng and old not in eng
for ver in ("0d1ae", "e4278", "ca0d4", "d5507"):
    for tail in (f"config.{ver}.json", f"index.{ver}.jsc"):
        assert f"assets/assets/src/{tail}" in names, tail
print("  package-first cache precedence: ok; pins=0d1ae,e4278,ca0d4,d5507")
PY

echo "[4/4] 菜单 so/dex 与 native 热更兜底形态"
"$PYTHON_BIN" - "$APK" <<'PY'
import hashlib
import sys
import zipfile

z = zipfile.ZipFile(sys.argv[1])
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zxr]" in so and b"zxr\0" in so and b"BEGIN CERTIFICATE" in so
# 内容指纹版兜底：旧的文件名白名单整块已删，新增失败回退分支
assert b"index.b5278" not in so, "so 仍含旧的 bundle 文件名白名单"
assert b"assets/remote/src/index" not in so, "so 仍含旧的 bundle 文件名白名单"
assert b"hotfallback splice eval failed, retry original script" in so, "so 缺失败回退分支"
c8 = z.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8 and b"mod-menu-v2.html" not in c8
print("  classes8 sha256=" + hashlib.sha256(c8).hexdigest())
print("  so size=" + str(len(so)) + " sha256=" + hashlib.sha256(so).hexdigest())
PY

echo "verify=pass"
