#!/usr/bin/env bash
# 镇邪人 1.0.3 — 外科升级原生菜单（禁止对游戏包 apktool b）
#
# 底包：真机已注入的旧 WebView 星光包（含百度 Stub / JMYBT 补丁 / TapSign / 免广告）
# 改动：
#   - 替换 classes8.dex（com/starry → NativeMenu，去掉 WebView 菜单）
#   - 替换 libstarry_magic.so / libc++_shared.so / assets/certs
#   - 删除 assets/web/**
#   - 保留 classes7 玩法补丁、Manifest StubApplication+MenuService、sq_plugin
#
# 用法:
#   bash scripts/surgical_zxr_1.0.3.sh
#   STARRY_INSTALL=1 bash scripts/surgical_zxr_1.0.3.sh
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
BASE_APK="${ZXR_BASE_APK:-$NIXIANG/02_apks/镇邪人/镇邪人_1.0.3_device_starry.apk}"
OUT="$NIXIANG/08_output/镇邪人"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/uber-debug.keystore"
# 底包是旧 inject（uber debug）；覆盖装必须同证。正式 starry_release 另见 ZXR_USE_RELEASE_KS=1
if [[ "${ZXR_USE_RELEASE_KS:-0}" == "1" ]]; then
  KS="$NIXIANG/01_tools/starry_release.jks"
fi
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
# 正式包名：release 构建 + 签名。默认 uber-debug 证（与真机旧包同证）；ZXR_USE_RELEASE_KS=1 才用 starry_release.jks
OUT_NAME="${ZXR_OUTPUT_NAME:-镇邪人_1.0.3_starry-releaseSigned.apk}"
SIGNED="$OUT/$OUT_NAME"
PATCHED_JSC="${ZXR_PATCHED_JSC:-}"
ALIAS_APK="$OUT/镇邪人_1.0.3_starry.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base $BASE_APK (adb pull device starry apk first)"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_ZXR' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ZXR"; exit 1; }
echo "[check] Android.mk target=zxr"

# 底包必须已是旧星光（含 MenuService / classes8 / libstarry）
python3.11 - "$BASE_APK" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
need = ("lib/arm64-v8a/libstarry_magic.so", "classes8.dex")
miss = [n for n in need if n not in names]
if miss:
    raise SystemExit(f"ERROR: base 不是完整星光包，缺 {miss}")
print("[check] base has libstarry + classes8")
PY

LOCK_DIR="/tmp/zxr_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zxr_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] ndk-build → jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
# zxr 仅 3 字符，默认 strings 最小长度 4 会漏检 → 用 NUL 定界 / -n 3
python3.11 - "$SO_BUILT" <<'PY'
from pathlib import Path
import sys
b = Path(sys.argv[1]).read_bytes()
if b"[Zxr]" not in b or b"zxr\0" not in b:
    raise SystemExit("ERROR: so 不是 ZXR（缺 [Zxr] 或 game_id zxr）")
bad = [t for t in (b"[Tlgd]", b"[Sc33]", b"[Dwct]", b"[Gyws]", b"[Hzsj]", b"[Pcr]") if t in b]
if bad:
    raise SystemExit(f"ERROR: so 仍含其它游戏标签: {bad}")
print("  so tags ok ([Zxr] + zxr\\0)")
PY
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so → jniLibs ok ([Zxr]/zxr verified)"

echo "[1b/6] gradle ${GRADLE_TASK} (starry template only)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "${GRADLE_TASK}" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/${BUILD_TYPE}/${APK_NAME}"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q '\[Zxr\]' "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 ZXR"; exit 1; }

echo "[2/6] apktool d starry template -> com/starry smali"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] assemble classes8.dex = com/starry (NativeMenu)"
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/menu/NativeMenuController.smali" ]] \
  || { echo "ERROR: NativeMenuController.smali missing — template not native menu"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/cocos/ZxrHooks.smali" ]] \
  || { echo "ERROR: ZxrHooks.smali missing"; exit 1; }
# Baidu stub: keep Application as Stub; StarryApp stays default Application super
"$SMALI_BIN" assemble -o "$WORK/classes8.dex" "$SMALI_ROOT"
ls -la "$WORK/classes8.dex"

mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] zip surgical merge（替换菜单 dex/so/certs，删 web）"
python3.11 - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import hashlib, sys, zipfile
import os
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
c8 = (work / "classes8.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
patched_jsc = os.environ.get("ZXR_PATCHED_JSC", "").strip()
if patched_jsc:
    extras["assets/assets/src/index.d5507.jsc"] = Path(patched_jsc).read_bytes()
patched_b5278 = os.environ.get("ZXR_PATCHED_JSC_B5278", "").strip()
if patched_b5278:
    data_b5278 = Path(patched_b5278).read_bytes()
    extras["assets/assets/src/index.b5278.jsc"] = data_b5278
    extras["assets/src/index.b5278.jsc"] = data_b5278
    extras["assets/assets/remote/src/index.b5278.jsc"] = data_b5278
    extras["assets/remote/src/index.b5278.jsc"] = data_b5278
    extras["assets/assets/remote/src/index.b5278.js"] = Path(os.environ.get("ZXR_PATCHED_JSC_B5278_JS", "")).read_bytes()
    extras["assets/remote/src/index.b5278.js"] = Path(os.environ.get("ZXR_PATCHED_JSC_B5278_JS", "")).read_bytes()
patched_b5278_config = os.environ.get("ZXR_PATCHED_B5278_CONFIG", "").strip()
if patched_b5278_config:
    data_config = Path(patched_b5278_config).read_bytes()
    extras["assets/assets/remote/src/config.b5278.json"] = data_config
    extras["assets/remote/src/config.b5278.json"] = data_config
patched_jsb_engine = os.environ.get("ZXR_PATCHED_JSB_ENGINE", "").strip()
if patched_jsb_engine:
    extras["assets/jsb-adapter/jsb-engine.js"] = Path(patched_jsb_engine).read_bytes()
patched_main = os.environ.get("ZXR_PATCHED_MAIN", "").strip()
if patched_main:
    extras["assets/main.js"] = Path(patched_main).read_bytes()

replace = {"classes8.dex": c8}
# 底包里的 so/certs 也走 extras 覆盖
skip_from_base = set(extras) | set(replace) | {"assets/web"}

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
kept = 0
dropped_web = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name.startswith("assets/web/") or name == "assets/web":
        dropped_web += 1
        continue
    if name in skip_from_base:
        continue
    if name in replace:
        data = replace[name]
    else:
        data = zin.read(name)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    kept += 1

info = zipfile.ZipInfo("classes8.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c8)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

# 完整性：resources.arsc / 非替换 dex 字节不变
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
arsc_o = hashlib.sha256(zin2.read("resources.arsc")).hexdigest()
arsc_n = hashlib.sha256(zout2.read("resources.arsc")).hexdigest()
assert arsc_o == arsc_n, "resources.arsc drifted"
for dn in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
           "classes5.dex", "classes6.dex", "classes7.dex"):
    assert hashlib.sha256(zin2.read(dn)).hexdigest() == hashlib.sha256(zout2.read(dn)).hexdigest(), f"{dn} drifted"
assert hashlib.sha256(zout2.read("classes8.dex")).hexdigest() != hashlib.sha256(zin2.read("classes8.dex")).hexdigest()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
web_left = [n for n in zout2.namelist() if n.startswith("assets/web")]
assert not web_left, f"web residual: {web_left[:5]}"
# 新菜单标志
c8s = zout2.read("classes8.dex")
assert b"NativeMenuController" in c8s, "classes8 missing NativeMenu"
assert b"ZxrHooks" in c8s, "classes8 missing ZxrHooks"
assert b"mod-menu-v2.html" not in c8s
print(f"  merge ok; kept={kept} dropped_web={dropped_web}; arsc+c1-7 unchanged; classes8+so+certs replaced")
zin2.close(); zout2.close()
PY

echo "[5/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
if [[ "${ZXR_USE_RELEASE_KS:-0}" == "1" ]]; then
  KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
  KS_PASS="${STARRY_KS_PASS:-starry2026}"
else
  # uber-apk-signer embedded debug.keystore (matches 2026-08-09 inject / device)
  [[ -f "$KS" ]] || { jar xf "$NIXIANG/01_tools/uber-apk-signer.jar" debug.keystore && mv debug.keystore "$KS"; }
  KS_ALIAS="androiddebugkey"
  KS_PASS="android"
fi
mkdir -p "$OUT"
if [[ -n "$BT" && -f "$KS" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
else
  java -jar "$NIXIANG/01_tools/uber-apk-signer.jar" -a "$ALIGNED" --out "$OUT" --allowResign --overwrite
  CAND=$(ls -t "$OUT"/*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
cp -f "$SIGNED" "$ALIAS_APK"
rm -f "$OUT/"*.idsig \
  "$OUT"/镇邪人_1.0.3_native-menu-*.apk 2>/dev/null || true
ls -lh "$SIGNED" "$ALIAS_APK"

echo "[6/6] post-check strings"
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
python3.11 - "$WORK/final.so" <<'PY'
from pathlib import Path
import sys
b = Path(sys.argv[1]).read_bytes()
assert b"[Zxr]" in b and b"zxr\0" in b, "final so missing ZXR tags"
print("  final so tags ok")
PY
python3.11 - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
web = [n for n in z.namelist() if n.startswith("assets/web")]
assert not web, f"assets/web still present: {web[:5]}"
c8 = z.read("classes8.dex")
assert b"NativeMenuController" in c8 and b"ZxrHooks" in c8
assert b"mod-menu-v2.html" not in c8
print("  final apk: no web, NativeMenu+ZxrHooks in classes8")
PY
echo "  final ok"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (set STARRY_INSTALL=1 to cover-install)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_zxr_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
adb shell am force-stop "$PKG" 2>/dev/null || true
adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'"
adb shell rm -f "$REMOTE_APK" 2>/dev/null || true
# shellcheck source=/dev/null
source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG" || true
echo "installed → $SIGNED"
echo "log: adb logcat -s StarryApp:V StarryMagic:V NativeMenu:V Zxr:V | grep -E 'auth game_id|native menu|Zxr'"
PATCHED_JSC_B5278="${ZXR_PATCHED_JSC_B5278:-}"
PATCHED_MAIN="${ZXR_PATCHED_MAIN:-}"
PATCHED_B5278_CONFIG="${ZXR_PATCHED_B5278_CONFIG:-}"
PATCHED_JSB_ENGINE="${ZXR_PATCHED_JSB_ENGINE:-}"
