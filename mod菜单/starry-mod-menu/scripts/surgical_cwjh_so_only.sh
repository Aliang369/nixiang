#!/usr/bin/env bash
# 超维进化（cwjh）— so-only 外科换装（2026-09-21 建立）
#
# 背景：02_apks 原始 2.2 base 已被清理，surgical_cwjh_2.2.sh 的全量重建链不可用。
#       设备上已装的星光包即最佳外科底包（classes4.dex / patched classes.dex·3.dex /
#       AndroidManifest / 签名 keystore 全部已就绪），只改 Games/CwjhMod.cpp 时
#       **只换 lib/arm64-v8a/libstarry_magic.so** 即可，classes4.dex 保持不变
#       （本次改动未新增 JNI 方法；若新增 native 桥方法，必须回 2.2 全量链成对出包）。
#
# 前置：Android.mk 必须启用 -DENABLE_COCOS_HOOK -DSTARRY_GAME_CWJH（脚本自检）
# 用法：
#   bash scripts/surgical_cwjh_so_only.sh [base.apk]
#   STARRY_INSTALL=1 bash scripts/surgical_cwjh_so_only.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
OUT="$NIXIANG/08_output/超维进化"
# 底包 = 上一次交付的星光外科包（其中的 so 会被本次新编的替换，classes4.dex 等保持不变）。
# 不传参时自动取 08_output/超维进化 下最新的 *releaseSigned.apk，避免底包路径随版本失效。
if [[ -n "${1:-}" ]]; then
  BASE_APK="$1"
else
  BASE_APK="$(ls -t "$OUT"/*releaseSigned.apk 2>/dev/null | head -1 || true)"
fi
PKG="com.grinning.beta.yszl"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
TAG="$(date +%Y%m%d_%H%M%S)"
SIGNED="$OUT/超维进化_so_only_${TAG}-releaseSigned.apk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]] && { BT="$HOME/Library/Android/sdk/build-tools/$v"; break; }
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools missing"; exit 1; }
[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base apk: $BASE_APK"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: missing $NDK/ndk-build"; exit 1; }

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_CWJH' "$MK" \
  || { echo "ERROR: Android.mk 未启用 STARRY_GAME_CWJH"; exit 1; }

WORK="$(mktemp -d /tmp/cwjh_soonly.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT"

echo "[1/4] ndk-build（独立 obj_cwjh21 / libs_cwjh21，绝对路径）"
(cd "$STARRY_SRC/app/src/main/jni" && "$NDK/ndk-build" \
  NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  APP_ABI=arm64-v8a NDK_OUT="$STARRY_SRC/obj_cwjh21" NDK_LIBS_OUT="$STARRY_SRC/libs_cwjh21" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO="$STARRY_SRC/libs_cwjh21/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: so missing"; exit 1; }
grep -a -q '\[Cwjh\]' "$SO" || { echo "ERROR: so 无 [Cwjh] 标签（可能仍是上一款游戏）"; exit 1; }
LC_ALL=C grep -aq '超级回血' "$SO" || { echo "ERROR: so 缺 CWJH 中文菜单串"; exit 1; }
echo "  so ok: $(md5 -q "$SO") $(stat -f%z "$SO") B"

echo "[2/4] 外科换装（只替换 lib/arm64-v8a/libstarry_magic.so，其余条目字节保留）"
python3 - "$BASE_APK" "$SO" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
base, so, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
newso = so.read_bytes()
target = 'lib/arm64-v8a/libstarry_magic.so'
zin = zipfile.ZipFile(base); zout = zipfile.ZipFile(dst, 'w')
for item in zin.infolist():
    name = item.filename
    if name.startswith('META-INF/'):
        continue
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    if name == target:
        info.compress_type = zipfile.ZIP_STORED   # extractNativeLibs=false：so 必须不压缩
        zout.writestr(info, newso)
    else:
        zout.writestr(info, zin.read(item))
zin.close(); zout.close()
z1, z2 = zipfile.ZipFile(base), zipfile.ZipFile(dst)
assert hashlib.sha256(z2.read(target)).hexdigest() == hashlib.sha256(newso).hexdigest(), 'so 未换入'
for n in ('resources.arsc', 'classes4.dex', 'classes2.dex'):
    assert hashlib.sha256(z1.read(n)).hexdigest() == hashlib.sha256(z2.read(n)).hexdigest(), n
print('  merge ok; so md5', hashlib.md5(newso).hexdigest())
PY

echo "[3/4] zipalign -p 4 + apksigner（keystore 与设备安装包同一把，覆盖安装不丢数据）"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -3

echo "[4/4] 产物：$SIGNED"
if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  adb push "$SIGNED" /data/local/tmp/cwjh_so_only.apk >/dev/null
  adb shell "su -c 'am force-stop $PKG; pm install -r -d --user 0 /data/local/tmp/cwjh_so_only.apk'"
  adb shell "su -c 'rm -f /data/local/tmp/cwjh_so_only.apk'"
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" f9a116a2 "$PKG" "$SIGNED" --su
  echo "log: adb logcat -s Mod_Menu:V StarryMagic:V"
fi
