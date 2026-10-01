#!/usr/bin/env bash
# ============================================================
#  霓虹深渊 — 仅换 libstarry_magic.so（modern 注册表构建）
#  底包须已是「星光成品」（含 classes5.dex 菜单 + SignSpoof + 过签 P 补丁）
#
#  2026-10-02 迁移 modern：ndk-build 走 Android_neon.mk
#    （MainModern.cpp + Games/NeonMod.cpp + Games/NeonEntry.cpp + Games/NoAceStubs.cpp）
#    不再要求公共 Android.mk 里的 -DSTARRY_GAME_NEON 处于启用态
#
#  用法:
#    bash scripts/neon_so_only.sh                     # 只出包
#    STARRY_INSTALL=1 bash scripts/neon_so_only.sh    # 出包 + 覆盖安装
#    NEON_BASE_APK=/abs/xxx.apk bash scripts/neon_so_only.sh
#  签名: 01_tools/starry_release.jks（alias starry / starry2026，可用
#        NEON_SIGN_KS / STARRY_KS_ALIAS / STARRY_KS_PASS 覆盖）
# ============================================================
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"        # mod菜单/starry-mod-menu
NX="$(cd "$ROOT/../.." && pwd)"                  # 仓库根
PKG=com.shangyoo.neon
OUT_DIR="$NX/08_output/霓虹深渊"
STARRY_SRC="$ROOT/app"
JNI_DIR="$STARRY_SRC/src/main/jni"

NDK="${ANDROID_NDK_HOME:-${NDK:-$HOME/Library/Android/sdk/ndk/27.0.12077973}}"
[[ -x "$NDK/ndk-build" ]] || {
  NDK="$(ls -d "$HOME/Library/Android/sdk/ndk/"* 2>/dev/null | sort -V | tail -1)"
}
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: 找不到 ndk-build" >&2; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools/apksigner" >&2; exit 1; }

KS="${NEON_SIGN_KS:-$NX/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
[[ -f "$KS" ]] || { echo "ERROR: 缺少签名库 $KS" >&2; exit 1; }

# 底包（已是星光成品）：默认取上次的 surgical 产物
BASE_APK="${NEON_BASE_APK:-$OUT_DIR/霓虹深渊_3.2.860_surgical-releaseSigned.apk}"
if [[ ! -f "$BASE_APK" && -f "$OUT_DIR/霓虹深渊_3.2.860_starry-releaseSigned.apk" ]]; then
  BASE_APK="$OUT_DIR/霓虹深渊_3.2.860_starry-releaseSigned.apk"
fi
[[ -f "$BASE_APK" ]] || {
  echo "ERROR: 缺少星光底包 $BASE_APK" >&2
  echo "  先用 official 底包跑: bash scripts/surgical_neon_3.2.860.sh" >&2
  exit 1
}
# 底包必须已含菜单 dex，否则只换 so 会得到「无菜单」包
if command -v unzip >/dev/null; then
  # 注意: set -o pipefail 下 unzip | grep -q 会因 SIGPIPE 误判，必须先落变量
  DEXLIST="$(unzip -l "$BASE_APK" 2>/dev/null || true)"
  case "$DEXLIST" in
    *classes5.dex*) ;;
    *) echo "ERROR: 底包不含 classes5.dex（不是星光成品包）: $BASE_APK" >&2; exit 1 ;;
  esac
fi
echo "[check] base = $BASE_APK"

# ---- [1/4] ndk-build（modern 注册表）----
echo "[1/4] ndk-build (Android_neon.mk)"
MK="$JNI_DIR/Android_neon.mk"
[[ -f "$MK" ]] || { echo "ERROR: 缺少 $MK" >&2; exit 1; }
grep -q 'Games/NeonMod.cpp' "$MK" || { echo "ERROR: Android_neon.mk 未编译 Games/NeonMod.cpp" >&2; exit 1; }
OBJ_OUT="$STARRY_SRC/src/obj_neon"
LIBS_OUT="$STARRY_SRC/src/libs_neon"
rm -rf "$OBJ_OUT" "$LIBS_OUT"
(cd "$JNI_DIR" && \
  "$NDK/ndk-build" NDK_PROJECT_PATH="$JNI_DIR/../.." APP_BUILD_SCRIPT=./Android_neon.mk \
  NDK_APPLICATION_MK=./Application.mk NDK_OUT="$OBJ_OUT" NDK_LIBS_OUT="$LIBS_OUT" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)" 2>&1 | tail -5)
SO="$LIBS_OUT/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: 缺 $SO" >&2; exit 1; }

# ---- [2/4] so 自检（strings 落文件后再 grep，避免 pipefail + SIGPIPE 误判）----
STRFILE="$(mktemp /tmp/neon_so_strings.XXXXXX)"
strings -a "$SO" > "$STRFILE" || true
grep -q '\[Neon\]' "$STRFILE" || { rm -f "$STRFILE"; echo "ERROR: so 自检失败，未见 [Neon]" >&2; exit 1; }
if grep -qE '\[Wggls\]|\[Shhj\]|\[Gyws\]' "$STRFILE"; then
  rm -f "$STRFILE"; echo "ERROR: so 仍含其它游戏标签" >&2; exit 1
fi
rm -f "$STRFILE"
echo "[2/4] so ok: $(md5 -q "$SO") $(basename "$SO") $(stat -f%z "$SO") B"

# ---- [3/4] 外科换 so + 签名 ----
WORKDIR="$(mktemp -d /tmp/neon_so_only.XXXXXX)"
trap 'rm -rf "$WORKDIR" "$OBJ_OUT" "$LIBS_OUT"' EXIT
cp -f "$BASE_APK" "$WORKDIR/base.apk"
mkdir -p "$WORKDIR/lib/arm64-v8a"
cp -f "$SO" "$WORKDIR/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORKDIR" && zip -q -u base.apk lib/arm64-v8a/libstarry_magic.so)
# 旧签名文件必须清掉，否则 apksigner 可能带着陈旧的 META-INF 摘要
(cd "$WORKDIR" && zip -q -d base.apk 'META-INF/*.SF' 'META-INF/*.RSA' 'META-INF/*.DSA' 'META-INF/*.EC' >/dev/null 2>&1 || true)

SIGNED="$OUT_DIR/霓虹深渊_3.2.860_surgical-releaseSigned.apk"
mkdir -p "$OUT_DIR"
"$BT/zipalign" -f -p 4 "$WORKDIR/base.apk" "$WORKDIR/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORKDIR/aligned.apk"
echo "[sign] ks=$(basename "$KS") alias=$KS_ALIAS"
"$BT/apksigner" verify --print-certs "$SIGNED" | grep -E "certificate (DN|MD5|SHA-1)" || true

cp -f "$SIGNED" "$OUT_DIR/霓虹深渊_3.2.860_starry-releaseSigned.apk"
echo "[3/4] 出包 → $SIGNED"
ls -lh "$SIGNED" | awk '{print "      "$5, $9}'
md5 -q "$SIGNED" | sed 's/^/      md5 /'

# ---- [4/4] 可选装机 ----
if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  ADB=(adb)
  [[ -n "${ANDROID_SERIAL:-}" ]] && ADB+=( -s "$ANDROID_SERIAL" )
  "${ADB[@]}" push "$SIGNED" /data/local/tmp/neon_so.apk
  "${ADB[@]}" shell am force-stop "$PKG" 2>/dev/null || true
  "${ADB[@]}" shell su -c "pm install -r -d --user 0 /data/local/tmp/neon_so.apk" \
    || "${ADB[@]}" install -r --no-incremental "$SIGNED"
  "${ADB[@]}" shell rm -f /data/local/tmp/neon_so.apk 2>/dev/null || true
  echo "[4/4] installed; 日志: adb logcat -s StarryMagic:V Mod_Menu:I MenuService:V Neon:V"
else
  echo "[4/4] 未装机（加 STARRY_INSTALL=1）"
fi
