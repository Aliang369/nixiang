#!/usr/bin/env bash
# 镇邪人 1.0.3(vc11) 星空包静态验收：身份/签名、原版底包外科注入（Manifest 菜单服务、
# classes7 入口 smali、sq_plugin 签名固定）、引擎包内命中优先、钉入战斗补丁、菜单 dex/so。
#
# 用法：bash 04_scripts/镇邪人/verify_apk_cb9dc_zxr.sh <apk> [官方原版底包]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_cb9dc_star-releaseSigned.apk}"
BASE="${2:-}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
APKSIGNER="${APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"
PY="${NIXIANG_PYTHON:-/opt/homebrew/bin/python3.11}"
[[ -x "$PY" ]] || PY="$(command -v python3)"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"
ORIG_CERT_MD5="8fa00ae1397609e5e09ff34cc43030e3"

echo "[1/6] 包身份与签名"
"$AAPT2" dump badging "$APK" | grep -F "package: name='${PKG}' versionCode='11' versionName='1.0.3'"
"$APKSIGNER" verify --print-certs "$APK" | grep -F "Signer #1 certificate SHA-256 digest: ${CERT_SHA256}"

echo "[2/6] 二进制 Manifest：菜单服务 + 官方 Application 保留 + 悬浮窗权限"
"$PY" - "$APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
mf = z.read("AndroidManifest.xml")
def has(tok: str) -> bool:
    b = tok.encode()
    return b in mf or b.decode().encode("utf-16-le") in mf
for tok, label in (
    ("com.starry.magic.menu.MenuService", "MenuService"),
    ("com.sanqi.jmybt.game37.JMYBTApplication", "官方 JMYBTApplication"),
    ("android.permission.SYSTEM_ALERT_WINDOW", "SYSTEM_ALERT_WINDOW"),
    ("android.permission.FOREGROUND_SERVICE", "FOREGROUND_SERVICE"),
):
    assert has(tok), f"manifest 缺 {label}"
assert not has("com.starry.magic.StarryApp"), "manifest 不应把 Application 换成 StarryApp"
print("  manifest entries ok")
PY

echo "[3/6] classes7 入口 smali / classes8 菜单 dex / so 标签"
"$PY" - "$APK" <<'PY'
import hashlib, sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
c7 = z.read("classes7.dex")
for probe in (b"com/starry/magic/menu/MenuService", b"com/starry/magic/cocos/ZxrHooks",
              b"starry_magic", b"get_ret", b"set_ret"):
    assert probe in c7, f"classes7 缺 {probe!r}"
c8 = z.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8 and b"mod-menu-v2.html" not in c8
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zxr]" in so and b"zxr\0" in so and b"BEGIN CERTIFICATE" in so
assert b"assets/remote/src/index" not in so and b"index.b5278" not in so
assert b"hotfallback splice eval failed, retry original script" in so
print("  classes7 sha256=" + hashlib.sha256(c7).hexdigest())
print("  classes8 sha256=" + hashlib.sha256(c8).hexdigest())
print("  so size=" + str(len(so)) + " sha256=" + hashlib.sha256(so).hexdigest())
PY

echo "[4/6] sq_plugin 内 TapSignKit 签名固定（原版证书 MD5）"
"$PY" - "$APK" "$ORIG_CERT_MD5" <<'PY'
import io, sys, zipfile
z = zipfile.ZipFile(sys.argv[1]); md5 = sys.argv[2].encode()
plugins = [n for n in z.namelist() if n.startswith("assets/sq_plugin_") and n.endswith(".apk")]
assert len(plugins) == 1, plugins
pz = zipfile.ZipFile(io.BytesIO(z.read(plugins[0])))
assert "classes4.dex" in pz.namelist()
dex = pz.read("classes4.dex")
assert md5 in dex, "plugin TapSignKit 未固定原版 MD5"
assert not any(n.startswith("META-INF/") for n in pz.namelist()), "plugin 应已丢弃旧 META-INF"
print(f"  {plugins[0]} classes4 ok")
PY

echo "[5/6] 引擎包内命中优先 + 钉入战斗补丁"
"$PY" - "$APK" <<'PY'
import hashlib, sys, zipfile, zlib
import xxtea

z = zipfile.ZipFile(sys.argv[1])
eng = z.read("assets/jsb-adapter/jsb-engine.js")
assert hashlib.sha256(eng).hexdigest() == "22a6732420b27457c585e144c548d19ad5d2ff84a8df1d14c1320e9d329f9319", "引擎不是包内命中优先版"
assert b"var packagePath = getPackagePathFromRemoteUrl(url);" in eng
key = b"c25d728a-a9d9-4c"
names = set(z.namelist())
for ver in ("d5507", "cb9dc"):
    entry = f"assets/assets/src/index.{ver}.jsc"
    assert entry in names, entry
    assert f"assets/assets/src/config.{ver}.json" in names
    packed = xxtea.decrypt(z.read(entry), key, padding=False)
    length = int.from_bytes(packed[-4:], "little")
    js = zlib.decompress(packed[:length], 16 + zlib.MAX_WBITS)
    assert b"starry_zxr_safe_v2" in js, entry
    for i in (1, 2, 3, 4):
        assert f"__starryZxrOn({i})".encode() in js, (entry, i)
    print(f"  {entry}: decrypt ok marker+4 hooks sha256={hashlib.sha256(js).hexdigest()[:16]}…")
PY

echo "[6/6] 未动条目（main.js / libcocos2djs.so）"
if [[ -n "$BASE" ]]; then
  [[ -f "$BASE" ]] || { echo "ERROR: base missing: $BASE" >&2; exit 1; }
  "$PY" - "$APK" "$BASE" <<'PY'
import hashlib, sys, zipfile
new, base = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
for n in ("assets/main.js", "lib/arm64-v8a/libcocos2djs.so", "resources.arsc"):
    if n not in base.namelist():
        continue
    a, b = new.read(n), base.read(n)
    assert a == b, f"{n} 与底包不一致"
    print(f"  {n} identical sha256={hashlib.sha256(a).hexdigest()[:16]}…")
PY
else
  echo "  SKIP（未给底包）"
fi

echo "verify=pass"
