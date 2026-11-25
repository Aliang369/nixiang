#!/usr/bin/env bash
# 黑暗领域 1.0 复刻：Windows（Git Bash）+ 中文路径可跑的完整出包链
# 依据：04_scripts/_common/stages.md「Windows 硬规则」；中文路径经验的源头是
#      04_scripts/阿比斯少年冒险团/surgical_abssnmxt_2.36.sh:27-31。
# 用法：bash 04_scripts/黑暗领域/haly_pack_win.sh
set -euo pipefail
trap 'rc=$?; echo "ERROR haly_pack_win.sh line $LINENO rc=$rc" >&2' ERR

HERE="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$HERE/../.." && pwd)"
GAME="黑暗领域"
SRC="$NIXIANG/mod菜单/starry-mod-menu"
BASE="$NIXIANG/02_apks/$GAME/${GAME}_1.0_from_device.apk"
OUTDIR="$NIXIANG/08_output/$GAME"
OUT="$OUTDIR/${GAME}_1.0_combo_v3-releaseSigned.apk"
STAGE="$NIXIANG/13_scratch/haly_stage"
KS="$NIXIANG/01_tools/starry_release.jks"
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"
BAKSMALI_JAR="$NIXIANG/01_tools/smali/baksmali-fat.jar"

hash256() { if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | cut -d" " -f1; else shasum -a 256 "$1" | cut -d" " -f1; fi; }
md5f()    { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | cut -d" " -f1; else md5 -q "$1"; fi; }

JH="$(printenv JAVA_HOME || true)"
JAVA_BIN="java"
[ -n "$JH" ] && JAVA_BIN="$JH/bin/java"
PY="$(command -v python3.11 || command -v python3 || command -v python || true)"

[ -f "$BASE" ] || { echo "ERROR missing base apk: $BASE" >&2; exit 1; }
[ -f "$APKTOOL_JAR" ] || { echo "ERROR missing apktool.jar" >&2; exit 1; }
[ -n "$PY" ] || { echo "ERROR python not found" >&2; exit 1; }

BT=""
for c in "$NIXIANG/01_tools/android-sdk/build-tools/android-15" "/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/36.1.0" "/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/35.0.0"; do
  if [ -f "$c/lib/apksigner.jar" ]; then BT="$c"; break; fi
done
[ -n "$BT" ] || { echo "ERROR build-tools (apksigner.jar) not found" >&2; exit 1; }
NDK=""
for d in "$NIXIANG"/01_tools/android-sdk/ndk/*; do
  if [ -d "$d/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android" ] || [ -d "$d/prebuilt" ]; then NDK="$d"; fi
done
[ -n "$NDK" ] || { echo "ERROR NDK not found" >&2; exit 1; }

rm -rf "$STAGE"; mkdir -p "$STAGE" "$OUTDIR"
echo "[1/8] gradle assembleRelease（原路径，不改工程布局）"
LP="$SRC/local.properties"
LP_BAK="$STAGE/local.properties.bak"
[ -f "$LP" ] && cp "$LP" "$LP_BAK"
restore_lp() { if [ -f "$LP_BAK" ]; then cp "$LP_BAK" "$LP"; fi; }
trap 'restore_lp; rc=$?; echo "ERROR haly_pack_win.sh line $LINENO rc=$rc" >&2' ERR

SDKROOT=""
for c in "$NIXIANG/01_tools/android-sdk" "/c/Users/Administrator/AppData/Local/Android/Sdk"; do
  if [ -d "$c/platforms" ]; then SDKROOT="$c"; break; fi
done
[ -n "$SDKROOT" ] || { echo "ERROR android sdk (platforms) not found" >&2; exit 1; }
# 不写 local.properties（仓库里那份是 mac 路径且被 git 跟踪）：移走它，改用 ANDROID_HOME，
# 跑完再移回。避免路径转义与「sdk.dir 空值」两类坑。
SDK_WIN="$(cygpath -w "$SDKROOT" 2>/dev/null || echo "$SDKROOT")"
export ANDROID_HOME="$SDK_WIN"
export ANDROID_SDK_ROOT="$SDK_WIN"
rm -f "$LP"

( cd "$SRC" && ./gradlew.bat assembleRelease --console=plain --offline )
APP_APK="$SRC/app/build/outputs/apk/release/app-release.apk"
[ -f "$APP_APK" ] || { echo "ERROR gradle apk missing" >&2; exit 1; }
echo "  app-release.apk = $(md5f "$APP_APK")"

echo "[2/8] apktool d gradle apk"
"$JAVA_BIN" -jar "$APKTOOL_JAR" d -f -o "$STAGE/dec" "$APP_APK" >/dev/null

echo "[3/8] classes4.dex = com/starry smali"
mkdir -p "$STAGE/smali_starry/com"
for d in "$STAGE/dec"/smali "$STAGE/dec"/smali_classes*; do
  if [ -d "$d/com/starry" ]; then cp -R "$d/com/starry" "$STAGE/smali_starry/com/"; fi
done
[ -f "$STAGE/smali_starry/com/starry/magic/cocos/CocosJsBridge.smali" ] || { echo "ERROR CocosJsBridge 被 R8 改名/删除（proguard keep 缺失）" >&2; exit 1; }
"$JAVA_BIN" -jar "$SMALI_JAR" assemble -o "$STAGE/classes4.dex" "$STAGE/smali_starry"
[ -f "$STAGE/classes4.dex" ] || { echo "ERROR classes4 assemble failed" >&2; exit 1; }
"$PY" "$HERE/haly_win_check_dex.py" "$STAGE/classes4.dex" "$SRC/app/src/main/java/com/starry/magic/menu/MagicRes.java"

echo "[4/8] classes2.dex 注入（TapSign 回灌 / 强制 WebLogin / MenuService）"
unzip -p "$BASE" classes2.dex > "$STAGE/classes2_orig.dex"
"$JAVA_BIN" -jar "$BAKSMALI_JAR" d -o "$STAGE/c2smali" "$STAGE/classes2_orig.dex"
ORIG_MD5="d639fb03a75167fbe77bc75e2b4a4304"
export ORIG_MD5
"$PY" "$HERE/haly_win_patch_c2.py" "$STAGE/c2smali"
"$JAVA_BIN" -jar "$SMALI_JAR" assemble -o "$STAGE/classes2.dex" "$STAGE/c2smali"
[ -f "$STAGE/classes2.dex" ] || { echo "ERROR classes2 assemble failed" >&2; exit 1; }

echo "[5/8] AndroidManifest（StarryApp + MenuService + 权限）"
"$JAVA_BIN" -jar "$APKTOOL_JAR" d -f -s -o "$STAGE/orig_dec" "$BASE" >/dev/null
"$PY" "$HERE/haly_win_patch_mf.py" "$STAGE/orig_dec/AndroidManifest.xml"
"$JAVA_BIN" -jar "$APKTOOL_JAR" b -f "$STAGE/orig_dec" -o "$STAGE/mf_only.apk" >/dev/null
unzip -p "$STAGE/mf_only.apk" AndroidManifest.xml > "$STAGE/AndroidManifest.xml"
[ -s "$STAGE/AndroidManifest.xml" ] || { echo "ERROR manifest extract failed" >&2; exit 1; }

echo "[6/8] libc++_shared.so from NDK"
CXX="$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
[ -f "$CXX" ] || { echo "ERROR libc++_shared.so not found in NDK" >&2; exit 1; }
cp "$CXX" "$STAGE/libc++_shared.so"

echo "[7/8] 外科合并"
"$PY" "$HERE/haly_win_merge.py" "$BASE" "$STAGE/unsigned.apk" "$STAGE/AndroidManifest.xml" "$STAGE/classes2.dex" "$STAGE/classes4.dex" "$SRC/libs_haly/arm64-v8a/libstarry_magic.so" "$STAGE/libc++_shared.so"

echo "[8/8] zipalign + apksigner"
"$BT/zipalign.exe" -f -p 4 "$STAGE/unsigned.apk" "$STAGE/aligned.apk" 2>/dev/null || "$BT/zipalign" -f -p 4 "$STAGE/unsigned.apk" "$STAGE/aligned.apk"
"$JAVA_BIN" -jar "$BT/lib/apksigner.jar" sign --ks "$KS" --ks-key-alias starry --ks-pass pass:starry2026 --key-pass pass:starry2026 --out "$OUT" "$STAGE/aligned.apk"
restore_lp
echo "DONE $OUT"
echo "  size   = $(wc -c < "$OUT")"
echo "  md5    = $(md5f "$OUT")"
echo "  sha256 = $(hash256 "$OUT")"
