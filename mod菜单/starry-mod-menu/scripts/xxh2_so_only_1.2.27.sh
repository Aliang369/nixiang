#!/usr/bin/env bash
# 消消英雄2 1.2.27 —— 只换 libstarry_magic.so（外科 zip -u，不重出 dex / Manifest）
#
# 用途：日常改 native 代码（含 Lua 诊断包 MATCH2_LUA_DUMP_TRACE=1）时的快出口。
# 底包默认＝上一个正式星光包；换完 so 后用 release 证书重签（同名同证可覆盖安装，不清档）。
#
# 用法:
#   STARRY_INSTALL=1 bash scripts/xxh2_so_only_1.2.27.sh
#   STARRY_LUA_DUMP_TRACE=1 STARRY_INSTALL=1 bash scripts/xxh2_so_only_1.2.27.sh   # 诊断：dump 全部 Lua
#   STARRY_BASE_APK=<其它星光包> STARRY_INSTALL=1 bash scripts/xxh2_so_only_1.2.27.sh
#
# 注意（bash 3.2 教训）：$VAR 后面紧跟中文字符会被并入变量名 → 一律写 ${VAR}。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
VERSION="1.2.27"
PKG="com.blina.match2.tt"
OUT="${NIXIANG}/08_output/消消英雄2"
BASE="${STARRY_BASE_APK:-${OUT}/消消英雄2_${VERSION}_starry-releaseSigned.apk}"
SIGNED="${OUT}/消消英雄2_${VERSION}_starry-soOnly-releaseSigned.apk"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: 找不到 build-tools/zipalign" >&2; exit 1; }

MK="${STARRY_SRC}/app/src/main/jni/Android.mk"
grep -qE '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_MATCH2' "$MK" \
  || { echo "ERROR: Android.mk 未启用 STARRY_GAME_MATCH2 分支" >&2; exit 1; }
[[ -f "$BASE" ]] || { echo "ERROR: 缺底包 ${BASE}（可 STARRY_BASE_APK= 指定）" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: 缺 keystore ${KS}" >&2; exit 1; }

LOCK_DIR="/tmp/xxh2_so_only.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock ${LOCK_DIR} held（另一次 so_only 在跑）" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/xxh2_so_only.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT

# 中文路径会让 gradle/ndk-build 子进程解析失败 → 在 /tmp ASCII 目录里构建
echo "[1/4] ndk-build libstarry_magic.so"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "${STARRY_SRC}/" "$WORK/starry/"
APP="$WORK/starry/app"
EXTRA_CFLAGS=""
if [[ "${STARRY_LUA_DUMP_TRACE:-0}" == "1" ]]; then
  EXTRA_CFLAGS="-DMATCH2_LUA_DUMP_TRACE=1"
  echo "  (MATCH2_LUA_DUMP_TRACE=1 → 全部 Lua 模块落盘 starry_lua_dump/)"
fi
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  APP_CFLAGS="$EXTRA_CFLAGS" \
  -j8 >/dev/null
SO="$APP/build/intermediates/ndk/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO" ]] || { echo "ERROR: ndk-build 未产出 so" >&2; exit 1; }
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$SO" "$APP/src/main/jniLibs/arm64-v8a/libstarry_magic.so"

SO_MD5="$(md5 -q "$SO")"
echo "  so=$(stat -f%z "$SO") B md5=${SO_MD5}"
grep -aq '\[Match2\]' "$SO" || { echo "ERROR: so 缺 [Match2] 标签" >&2; exit 1; }
grep -aq 'xxh2' "$SO" || { echo "ERROR: so 缺 game_id xxh2" >&2; exit 1; }
if grep -aq '__starry_ac_ok' "$SO"; then echo "ERROR: so 仍含过检测标记 __starry_ac_ok" >&2; exit 1; fi
for t in Neon Dzqs Cstjjs Gjc2 Whrhx Sc33 Wggls Pcr Hzsj Zxr Cwjh Shhj; do
  if grep -aq "\[$t\]" "$SO" 2>/dev/null; then echo "ERROR: so 含其它游戏标签 [$t]" >&2; exit 1; fi
done

echo "[2/4] zip -u 换 so → ${SIGNED}"
cp -f "$BASE" "$WORK/in.apk"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO" "$WORK/lib/arm64-v8a/libstarry_magic.so"
(cd "$WORK" && zip -q -u in.apk lib/arm64-v8a/libstarry_magic.so)
IN_SO_MD5="$(unzip -p "$WORK/in.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$IN_SO_MD5" == "$SO_MD5" ]] || { echo "ERROR: 包内 so 与源 so 不一致" >&2; exit 1; }

echo "[3/4] zipalign + apksigner（release 证书）"
"$BT/zipalign" -f -p 4 "$WORK/in.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,6p' || true
CERT_MD5="$("$BT/apksigner" verify --print-certs "$SIGNED" 2>/dev/null | grep -i 'certificate MD5' | head -1 | awk '{print $NF}' | tr -d ':' | tr 'A-Z' 'a-z')"
echo "  signer cert MD5: ${CERT_MD5}（应为 161fca32a36d3df274458682a44cb774）"

echo "[4/4] 产物"
ls -lh "$SIGNED"
md5 -q "$SIGNED"
shasum -a 256 "$SIGNED" | awk '{print $1}'

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "--- 覆盖安装（同签名，不清档）---"
  REMOTE="/data/local/tmp/xxh2_so_only.apk"
  adb push "$SIGNED" "$REMOTE" >/dev/null
  adb shell su -c "pm install -r -d --user 0 '${REMOTE}'"
  adb shell rm -f "$REMOTE" 2>/dev/null || true
  bash "${NIXIANG}/04_scripts/_common/verify_installed_apk.sh" "$(adb get-serialno)" "$PKG" "$SIGNED" --su || true
  # shellcheck source=/dev/null
  source "${NIXIANG}/04_scripts/_common/starry_post_install.sh"
  starry_post_install "$PKG" || true
fi
echo "DONE: ${SIGNED}"
