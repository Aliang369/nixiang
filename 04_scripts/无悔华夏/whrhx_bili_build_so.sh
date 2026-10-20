#!/usr/bin/env bash
# 无悔华夏 B 站渠道（com.yhhy.whrhx.bilibili）- 私有 so 构建（Android_whrhxbili.mk）
#
# 产物：mod菜单/starry-mod-menu/libs_whrhxbili/arm64-v8a/libstarry_magic.so
# 功能链：复用 WhrhxMod 的 WHRHX 分支（九游 11 项）；WHRHXBILI 宏只覆盖 game_id / 包名 / 标签。
# 门禁：构建前 lint（线程/maps/热路径日志），构建后字符串断言 + 构建戳 + BUILD_OK 自证。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
LINT="$NIXIANG/04_scripts/_common/lint_mod_thread.sh"
OUT_LIBS="$STARRY_SRC/libs_whrhxbili"
OBJ="$STARRY_SRC/obj_whrhxbili"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
BUILD_MAP="$NIXIANG/04_scripts/无悔华夏/so_build_map.txt"
LLVM_BIN="$(dirname "$(ls -1 "$NDK"/toolchains/llvm/prebuilt/darwin-*/bin/llvm-readelf 2>/dev/null | head -1 || true)")"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_whrhxbili.mk" ]] || { echo "ERROR: missing Android_whrhxbili.mk" >&2; exit 1; }

LINT_TARGETS=("$JNI/Main.cpp" "$JNI/Games/WhrhxMod.cpp")
if [[ -f "$LINT" ]]; then
  echo "[lint] $LINT --strict ${LINT_TARGETS[*]#$NIXIANG/}"
  bash "$LINT" --strict "${LINT_TARGETS[@]}" || { echo "ERROR: lint gate failed - 修完再构建（临时跳过 WHRHX_SKIP_LINT=1）" >&2; exit 1; }
elif [[ "${WHRHX_SKIP_LINT:-0}" == "1" ]]; then
  echo "[lint] WARN: $LINT 不存在且 WHRHX_SKIP_LINT=1 ⇒ 本轮未做门禁" >&2
else
  echo "ERROR: lint script missing: ${LINT}（临时跳过用 WHRHX_SKIP_LINT=1）" >&2
  exit 1
fi

CFG_HASH="$( (cat "$JNI/Android_whrhxbili.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") | shasum -a 256 | awk '{print $1}')"
STAMP="$OBJ/.build_stamp"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] build config changed -> clean $OBJ"
  rm -rf "$OBJ"
fi

mkdir -p "$OBJ" "$OUT_LIBS"
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_whrhxbili.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  APP_OPTIM=release \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: new objects but so unchanged - stale output suspected" >&2
  exit 1
fi
echo "$CFG_HASH" > "$STAMP"

chk() {
  local pattern="$1"
  LC_ALL=C grep -a -q "$pattern" "$SO" || {
    echo "ERROR: so missing [$pattern]" >&2
    exit 1
  }
}
chk "[Whrhx]"
chk "whhxbili"
chk "target=bilibili"
chk "GetCountryDataById"
chk "GetCityData"
chk "identity api ready"
chk "research hooks ready"
chk "policy panel gate ready"
chk "GetPolicyItemShowState"
chk "科研立即完成"
chk "selftest ok"
chk "ImageOfRaw"
# 反向断言：不得把上一渠道的 id/标签编进来
if LC_ALL=C grep -a -q "whhx4399" "$SO"; then
  echo "ERROR: so still carries whhx4399 strings - wrong channel build" >&2
  exit 1
fi

# 反例门禁：so 不得引用 emutls（游戏侧 libc++_shared 是旧版，没有 __emutls_get_address）
if LC_ALL=C grep -a -q "__emutls_get_address" "$SO"; then
  echo "ERROR: so 仍引用 __emutls_get_address ⇒ 装机 dlopen 必失败（需 -fno-emulated-tls）" >&2
  exit 1
fi

MD5="$(md5 -q "$SO")"
SIZE="$(stat -f %z "$SO")"
EPOCH="$(LC_ALL=C grep -a -o 'STARRYSTAMP1:EPOCH=[0-9]*;' "$SO" | head -1 || true)"
[[ -n "$EPOCH" ]] || { echo "ERROR: so missing STARRYSTAMP1 build stamp" >&2; exit 1; }
BUILDID="unknown"
if [[ -n "$LLVM_BIN" && -x "$LLVM_BIN/llvm-readelf" ]]; then
  BUILDID="$("$LLVM_BIN/llvm-readelf" -n "$SO" 2>/dev/null | awk '/Build ID:/{print $3; exit}')"
  [[ -n "$BUILDID" ]] || BUILDID="none"
fi
COMMIT="$(git -C "$NIXIANG" rev-parse --short HEAD 2>/dev/null || echo unknown)"
echo "BUILD_OK md5=$MD5 size=$SIZE stamp=$EPOCH buildid=$BUILDID commit=$COMMIT target=bilibili"
printf '%s\t%s\t%s\t%s\t%s\n' "$(date '+%Y-%m-%d %H:%M:%S')" "whrhxbili" "$MD5" "$SIZE" "$BUILDID" >> "$BUILD_MAP"
echo "[build_so] done -> $SO"
