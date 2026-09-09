#!/usr/bin/env bash
# 物质与魔法 6.03 — 新原生菜单外科组装（底包=已注入星光包，勿 apktool b 游戏包）
#
# 底包默认：02_apks/物质与魔法/物质与魔法_6.03_starry_from_device.apk
# （无官方原包时以此为准：已含 MenuService 入口 / qkcheck RET / loadLibrary）
#
# 本脚本：
#   1) 切 WZYM 编新 libstarry_magic.so + 原生菜单 smali
#   2) 替换 classes3 内 com/starry（保留 okhttp3/okio）
#   3) classes8 ActivityHandler → 强制网页登录
#   4) 换 so / libc++ / certs，删除旧 assets/web
#
# 用法:
#   bash scripts/surgical_wzym_6.03.sh
#   STARRY_INSTALL=1 bash scripts/surgical_wzym_6.03.sh
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
NIXIANG="/Users/aliang/Documents/nixiang"
BASE_APK="${WZYM_BASE_APK:-$NIXIANG/02_apks/物质与魔法/物质与魔法_6.03_starry_from_device.apk}"
OUT="$NIXIANG/08_output/物质与魔法"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.huanshits.materialMagic"
SIGNED="$OUT/物质与魔法_6.03_surgical-nativeMenu-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/物质与魔法_6.03_surgical-nativeMenu-debugSigned.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing $BASE_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_WZYM' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_WZYM"; exit 1; }
grep -E 'Games/WzymMod\.cpp' "$MK" \
  || { echo "ERROR: Android.mk must compile WzymMod.cpp"; exit 1; }
echo "[check] Android.mk target=wzym"

LOCK_DIR="/tmp/wzym_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wzym_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] rsync + ndk-build + gradle ($GRADLE_TASK) in ASCII work"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
APP="$WORK/starry/app"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$APP/build/intermediates/ndk/libs/arm64-v8a/"*.so "$APP/src/main/jniLibs/arm64-v8a/"
python3.11 - <<PY
from pathlib import Path
import re
p = Path("$APP/build.gradle")
t = p.read_text()
t2 = re.sub(
    r"\n    externalNativeBuild \{[\s\S]*?\n    \}\n",
    "\n    // externalNativeBuild disabled (ASCII CI): so from jniLibs\n",
    t,
    count=1,
)
p.write_text(t2)
PY
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry template → smali/so/certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || SO_SRC="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO_SRC" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi
# 自检 so 含 Wzym 符号/字符串
strings -a "$WORK/lib/arm64-v8a/libstarry_magic.so" | grep -q 'Wzym\|物质与魔法\|wzym' \
  && echo "  so: wzym strings OK" || echo "  WARN: wzym strings not found in so (may be stripped)"

echo "[3/6] refresh classes3 com/starry (keep okhttp3/okio)"
unzip -p "$BASE_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
rm -rf "$WORK/c3smali/com/starry"
mkdir -p "$WORK/c3smali/com"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$WORK/c3smali/com/"
done
shopt -u nullglob
[[ -f "$WORK/c3smali/com/starry/magic/menu/NativeMenuController.smali" ]] \
  || { echo "ERROR: NativeMenuController.smali missing — 不是新原生菜单"; exit 1; }
[[ -f "$WORK/c3smali/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

echo "[4/6] patch classes8 ActivityHandler → force WebLogin"
unzip -p "$BASE_APK" classes8.dex > "$WORK/classes8_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c8smali" "$WORK/classes8_orig.dex"
python3.11 - "$WORK/c8smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
ah = root / "com/taptap/sdk/ui/ActivityHandler.smali"
t = ah.read_text(encoding="utf-8")
if "starry_force_web_login" in t:
    print("  already patched")
else:
    FORCE = """.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .registers 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string p1, "starry_force_web_login"

    invoke-direct {v0, p1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method"""
    t2, n = re.subn(
        r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V[\s\S]*?\.end method",
        FORCE, t, count=1)
    if n != 1:
        raise SystemExit("ActivityHandler.authorize patch failed")
    ah.write_text(t2, encoding="utf-8")
    print("  ActivityHandler.authorize → force WebLogin")
PY
"$SMALI_BIN" assemble -o "$WORK/classes8.dex" "$WORK/c8smali"
ls -la "$WORK/classes8.dex"

echo "[5/6] zip surgical merge (drop assets/web)"
python3.11 - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
replace = {
    "classes3.dex": (work / "classes3.dex").read_bytes(),
    "classes8.dex": (work / "classes8.dex").read_bytes(),
    "lib/arm64-v8a/libstarry_magic.so": (work / "lib/arm64-v8a/libstarry_magic.so").read_bytes(),
}
cxx = work / "lib/arm64-v8a/libc++_shared.so"
if cxx.is_file():
    replace["lib/arm64-v8a/libc++_shared.so"] = cxx.read_bytes()
extras = {}
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
dropped = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name.startswith("assets/web/") or name == "assets/web":
        dropped += 1
        continue
    if name in extras:
        continue
    data = replace[name] if name in replace else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = item.create_system
    zout.writestr(info, data)
    n += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("resources.arsc", "classes.dex", "classes2.dex", "lib/arm64-v8a/libqkcheck.so"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert b"starry_force_web_login" in zout2.read("classes8.dex")
assert b"NativeMenuController" in zout2.read("classes3.dex")
assert not any(x.startswith("assets/web/") for x in zout2.namelist())
print(f"  merged ok; entries={n}; dropped_web={dropped}; so={len(replace['lib/arm64-v8a/libstarry_magic.so'])}")
zin2.close(); zout2.close()
PY

echo "[6/6] zipalign + uber sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
mkdir -p "$WORK/signed"
UBER_ARGS=(-a "$ALIGNED" --allowResign --out "$WORK/signed")
[[ -n "$BT" ]] && UBER_ARGS+=(--zipAlignPath "$BT/zipalign")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
SIGNED_SRC="$(ls -t "$WORK/signed"/*Signed*.apk 2>/dev/null | head -1 || true)"
[[ -n "${SIGNED_SRC:-}" ]] || SIGNED_SRC="$(ls -t "$WORK/signed"/*.apk 2>/dev/null | head -1)"
[[ -f "${SIGNED_SRC:-}" ]] || { echo "ERROR: signed apk missing"; ls -la "$WORK/signed"; exit 1; }
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"
if [[ -n "$BT" ]]; then
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
fi

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/wzym_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover-install failed — 不自动卸载"
    exit 1
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  adb shell appops set "$PKG" SYSTEM_ALERT_WINDOW allow 2>/dev/null || true
  echo "installed: $PKG"
  echo "log: adb logcat -s StarryMagic:V Mod_Menu:V NativeMenu:V Wzym:V MenuService:V"
fi

echo "DONE: $SIGNED"
