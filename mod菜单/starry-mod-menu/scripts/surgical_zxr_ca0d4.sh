#!/usr/bin/env bash
# 镇邪人 1.0.3 — 外科重建（2026-09-23 ca0d4 修复版）
#
# 背景：2026-09-16 星光包的 jsb-engine 补丁把 src bundle 强制指向相对路径
#       "assets/remote/src"，当本地文件版本与服务器版本漂移时，cocos 原生下载器
#       Cocos2dxDownloader$3.run:154 会因 new URI(url).getHost()==null 抛 NPE 崩溃。
#
# 修复：删除相对路径路由，改用引擎自带的「远端 URL → 包内本地文件」优先查找
#       (getPackagePathFromRemoteUrl: /remote/ 之后路径 → assets/<bundle>/...)。
#       把打过战斗补丁的 ca0d4 放到 assets/assets/src/ 即被本地命中，永不进原生下载。
#
# 底包：09_test/镇邪人_crash_20260923/apk/base.apk（09-16 星光包，md5 7073581c…，
#       含 classes8 NativeMenu + libstarry_magic.so + Manifest MenuService，全部保留）
# 用法：bash mod菜单/starry-mod-menu/scripts/surgical_zxr_ca0d4.sh
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${ZXR_BASE_APK:-$NIXIANG/09_test/镇邪人_crash_20260923/apk/base.apk}"
OUT="$NIXIANG/08_output/镇邪人"
KS="$NIXIANG/01_tools/uber-debug.keystore"
KS_ALIAS="androiddebugkey"
KS_PASS="android"
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
OUT_NAME="${ZXR_OUTPUT_NAME:-镇邪人_1.0.3_ca0d4_safe-releaseSigned.apk}"
SIGNED="$OUT/$OUT_NAME"

PATCHED_SRC_JSC="${ZXR_PATCHED_SRC_JSC:-$NIXIANG/07_patched/镇邪人/ca0d4/index.ca0d4.jsc}"
PATCHED_SRC_CONFIG="${ZXR_PATCHED_SRC_CONFIG:-$NIXIANG/07_patched/镇邪人/ca0d4/config.ca0d4.json}"
STOCK_ENGINE="${ZXR_STOCK_ENGINE:-$NIXIANG/07_patched/镇邪人/stock/jsb-engine.js}"
STOCK_MAIN="${ZXR_STOCK_MAIN:-$NIXIANG/07_patched/镇邪人/stock/main.js}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools with zipalign not found"; exit 1; }

for f in "$BASE_APK" "$PATCHED_SRC_JSC" "$PATCHED_SRC_CONFIG" "$STOCK_ENGINE" "$STOCK_MAIN" "$KS"; do
  [[ -f "$f" ]] || { echo "ERROR: missing input $f"; exit 1; }
done

LOCK_DIR="/tmp/zxr_ca0d4.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held"; exit 1; fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zxr_ca0d4.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT"

echo "[1/4] 校验底包身份（应为 09-16 星光包）"
python3.11 - "$BASE_APK" <<'PY'
import sys, zipfile, hashlib
p = sys.argv[1]
h = hashlib.md5(open(p, "rb").read()).hexdigest()
z = zipfile.ZipFile(p)
names = set(z.namelist())
need = ["classes8.dex", "lib/arm64-v8a/libstarry_magic.so", "assets/assets/src/index.d5507.jsc",
        "assets/jsb-adapter/jsb-engine.js", "assets/main.js", "AndroidManifest.xml"]
miss = [n for n in need if n not in names]
if miss:
    raise SystemExit(f"ERROR: base missing {miss}")
c8 = z.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8, "classes8 不是 NativeMenu 星光菜单"
print(f"  base md5={h} entries={len(names)} classes8=NativeMenu ok")
PY

echo "[2/4] surgical merge（注入 ca0d4，恢复原版引擎/main.js，清 b5278 残留与 assets/web）"
python3.11 - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" "$PATCHED_SRC_JSC" "$PATCHED_SRC_CONFIG" "$STOCK_ENGINE" "$STOCK_MAIN" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst, jsc, cfg, engine, main = (Path(a) for a in sys.argv[1:8])

extras = {
    # 引擎自带「远端 URL → 包内本地文件」优先查找：/remote/src/xxx → assets/src/xxx
    # APK 内 cocos 资源根为 assets/，故 zip 路径为 assets/assets/src/xxx
    "assets/assets/src/index.ca0d4.jsc": jsc.read_bytes(),
    "assets/assets/src/config.ca0d4.json": cfg.read_bytes(),
    # 回滚 09-16 的崩溃补丁：相对路径路由 + main.js 守卫全部去掉
    "assets/jsb-adapter/jsb-engine.js": engine.read_bytes(),
    "assets/main.js": main.read_bytes(),
}

DROP_PREFIX = ("assets/web/", "assets/assets/remote/src/", "assets/remote/src/",
               "assets/assets/src/index.b5278.", "assets/src/index.b5278.")
DROP_EXACT = {"assets/web"}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
kept = dropped = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in DROP_EXACT or name.startswith(DROP_PREFIX):
        dropped += 1
        continue
    if name in extras:
        continue
    data = zin.read(name)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    kept += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest(), "resources.arsc drifted"
for dn in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "classes5.dex", "classes6.dex", "classes7.dex", "classes8.dex"):
    assert hashlib.sha256(zin2.read(dn)).hexdigest() == hashlib.sha256(zout2.read(dn)).hexdigest(), f"{dn} drifted"
assert hashlib.sha256(zin2.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest() == hashlib.sha256(zout2.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest(), "so drifted"
assert zout2.read("assets/assets/src/index.ca0d4.jsc") == extras["assets/assets/src/index.ca0d4.jsc"]
assert zout2.read("assets/jsb-adapter/jsb-engine.js") == extras["assets/jsb-adapter/jsb-engine.js"]
assert b'if (bundleName === "src")' not in zout2.read("assets/jsb-adapter/jsb-engine.js"), "引擎仍有相对路径路由补丁"
assert b"starry_zxr_main_guard_v1" not in zout2.read("assets/main.js"), "main.js 仍有守卫"
assert b"remote/src" not in zout2.read("assets/jsb-adapter/jsb-engine.js").replace(b"/remote/", b""), "引擎仍引用 remote/src"
assert not [n for n in zout2.namelist() if n.startswith("assets/web")]
assert not [n for n in zout2.namelist() if "index.b5278" in n]
assert [n for n in zout2.namelist() if n == "assets/assets/src/config.d5507.json"], "本地 d5507 配置丢失"
c8 = zout2.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8 and b"mod-menu-v2.html" not in c8
print(f"  merge ok; kept={kept} dropped={dropped}; 菜单/so/manifest 原样保留；ca0d4 已注入 assets/assets/src/")
zin2.close(); zout2.close()
PY

echo "[3/4] zipalign + sign"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"

echo "[4/4] 后检"
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -E "Signer #1 certificate SHA-256"
python3.11 - "$SIGNED" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
n = z.namelist()
print("  entries=", len(n))
print("  ca0d4 jsc sha256=", hashlib.sha256(z.read("assets/assets/src/index.ca0d4.jsc")).hexdigest())
assert "assets/assets/src/config.ca0d4.json" in n
print("  no assets/web:", not any(x.startswith("assets/web") for x in n))
PY
ls -lh "$SIGNED"
echo "done → $SIGNED"
