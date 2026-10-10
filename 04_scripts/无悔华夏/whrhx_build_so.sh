#!/usr/bin/env bash
# 无悔华夏 3.8.280 九游 - 私有构建（Android_whrhx.mk）
#
# 用法：
#   bash 04_scripts/无悔华夏/whrhx_build_so.sh            # release（出货用）
#   bash 04_scripts/无悔华夏/whrhx_build_so.sh --debug    # debug（带 DWARF，供 so_symbolize.sh 用）
#
# 产物：
#   release → mod菜单/starry-mod-menu/libs_whrhx/arm64-v8a/libstarry_magic.so
#   debug   → mod菜单/starry-mod-menu/obj_whrhx_dbg/local/arm64-v8a/libstarry_magic.so
#
# P1-④ 构建自证：末尾必打一行 `BUILD_OK md5=… size=… stamp=… mode=…`，
#   并把该行（含 buildid / 路径 / git commit）追加进 04_scripts/无悔华夏/so_build_map.txt，
#   由 04_scripts/_common/so_symbolize.sh 反查崩溃帧用哪份 so。
#   ★ 调用方禁止 `bash … | tail`：管道会把退出码掩成 0（本轮真踩过：ndk-build rc=2 却报成功）。
# P1-⑤ 门禁：构建前跑 lint_mod_thread.sh --strict（线程/maps/热路径日志三类规则），
#   有违规直接失败、不产出 so。临时跳过用 WHRHX_SKIP_LINT=1（仅限 lint 脚本未就位时）。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
LINT="$NIXIANG/04_scripts/_common/lint_mod_thread.sh"
BUILD_MAP="$NIXIANG/04_scripts/无悔华夏/so_build_map.txt"

MODE=release
case "${1:-}" in
  --debug) MODE=debug ;;
  --release|"") MODE=release ;;
  -h|--help) sed -n '2,22p' "$0"; exit 0 ;;
  *) echo "ERROR: unknown arg $1 (use --debug / --release)" >&2; exit 2 ;;
esac

if [[ "$MODE" == debug ]]; then
  OUT_LIBS="$STARRY_SRC/obj_whrhx_dbg/local"
  OBJ="$STARRY_SRC/obj_whrhx_dbg"
else
  OUT_LIBS="$STARRY_SRC/libs_whrhx"
  OBJ="$STARRY_SRC/obj_whrhx"
fi
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
STAMP="$OBJ/.build_stamp"
LLVM_BIN="$(dirname "$(ls -1 "$NDK"/toolchains/llvm/prebuilt/darwin-*/bin/llvm-readelf 2>/dev/null | head -1 || true)")"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_whrhx.mk" ]] || { echo "ERROR: missing Android_whrhx.mk" >&2; exit 1; }

# ---- P1-⑤ 源码门禁（lint）----
# ★ 作用域＝**本 so 真正编译的 TU**（见 Android_whrhx.mk 的 LOCAL_SRC_FILES）：Main.cpp +
#   Games/WhrhxMod.cpp。若按默认扫整个 Games/，其它游戏（nhsy2Mod.cpp / 我本布衣世家录Mod.cpp …）
#   的历史违规会挡住本游戏的构建 —— 那是别的会话的活，不该成为此处的硬失败。
LINT_TARGETS=("$JNI/Main.cpp" "$JNI/Games/WhrhxMod.cpp")
if [[ -f "$LINT" ]]; then
  echo "[lint] $LINT --strict ${LINT_TARGETS[*]#$NIXIANG/}"
  bash "$LINT" --strict "${LINT_TARGETS[@]}" || { echo "ERROR: lint gate failed - 修完再构建（临时跳过 WHRHX_SKIP_LINT=1）" >&2; exit 1; }
elif [[ "${WHRHX_SKIP_LINT:-0}" == "1" ]]; then
  echo "[lint] WARN: $LINT 不存在且 WHRHX_SKIP_LINT=1 ⇒ 本轮未做门禁" >&2
else
  echo "ERROR: lint script missing: ${LINT}（P1-⑤ 硬门禁；临时跳过用 WHRHX_SKIP_LINT=1）" >&2
  exit 1
fi

CFG_HASH="$( (cat "$JNI/Android_whrhx.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\nMODE=%s\n' "${STARRY_TRACE:-}" "$MODE") | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] build config changed -> clean $OBJ"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
if [[ "$MODE" == debug ]]; then
  "$NDK/ndk-build" -C "$JNI" \
    NDK_PROJECT_PATH="../.." \
    APP_BUILD_SCRIPT="./Android_whrhx.mk" \
    NDK_APPLICATION_MK="./Application.mk" \
    NDK_OUT="$OBJ" \
    NDK_LIBS_OUT="$OUT_LIBS" \
    APP_OPTIM=debug NDK_DEBUG=1 \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
else
  "$NDK/ndk-build" -C "$JNI" \
    NDK_PROJECT_PATH="../.." \
    APP_BUILD_SCRIPT="./Android_whrhx.mk" \
    NDK_APPLICATION_MK="./Application.mk" \
    NDK_OUT="$OBJ" \
    NDK_LIBS_OUT="$OUT_LIBS" \
    APP_OPTIM=release \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
fi
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }
if [[ "$MODE" == release && -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: new objects but so unchanged - stale output suspected" >&2
  exit 1
fi

mkdir -p "$OBJ"
echo "$CFG_HASH" > "$STAMP"

chk() {
  local pattern="$1"
  LC_ALL=C grep -a -q "$pattern" "$SO" || {
    echo "ERROR: so missing [$pattern]" >&2
    exit 1
  }
}
chk "[Whrhx]"
chk "whhx"
chk "GetCountryDataById"
chk "GetCityData"
chk "identity api ready"
chk "research hooks ready"
chk "policy panel gate ready"
# 注意：这里断言的是**代码里真的会出现的字面量**。删掉手写 kExpect[] 后，
# "PolicyDataMgr.GetPolicyItemShowState" 这个拼接串只存在于被删的 tag 里，
# so 中仅剩按名查类/查方法用的两个独立字面量 ⇒ 断言必须跟着改，
# 否则门禁会去要求一个从未由代码发出的字符串（本次踩坑）。
chk "GetPolicyItemShowState"
chk "PolicyDataMgr"
chk "科研立即完成"
chk "selftest ok"          # P2-⑨ 免登录自检
chk "ImageOfRaw"           # ANR 根因修复路径（按名兜底不依赖 Api::ImageOf）

# ---- P1-④ 构建自证 ----
MD5="$(md5 -q "$SO")"
SIZE="$(stat -f %z "$SO")"
STAMP_STR="missing"
EPOCH="$(LC_ALL=C grep -a -o 'STARRYSTAMP1:EPOCH=[0-9]*;' "$SO" | head -1 || true)"
[[ -n "$EPOCH" ]] && STAMP_STR="$EPOCH"
BUILDID="unknown"
if [[ -n "$LLVM_BIN" && -x "$LLVM_BIN/llvm-readelf" ]]; then
  BUILDID="$("$LLVM_BIN/llvm-readelf" -n "$SO" 2>/dev/null | awk '/Build ID:/{print $3; exit}')"
  [[ -n "$BUILDID" ]] || BUILDID="none"
fi
COMMIT="$(git -C "$NIXIANG" rev-parse --short HEAD 2>/dev/null || echo unknown)"
UTC="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

if [[ "$MODE" == debug ]]; then
  DBG="$SO"; REL="$STARRY_SRC/libs_whrhx/arm64-v8a/libstarry_magic.so"
else
  REL="$SO"; DBG="$STARRY_SRC/obj_whrhx_dbg/local/arm64-v8a/libstarry_magic.so"
fi
[[ -f "$REL" ]] || REL="-"
[[ -f "$DBG" ]] || DBG="-"
printf 'md5=%s  size=%s  buildid=%s  debug=%s  release=%s  commit=%s  utc=%s  note=%s\n' \
  "$MD5" "$SIZE" "$BUILDID" "$DBG" "$REL" "$COMMIT" "$UTC" "${2:-auto}" >> "$BUILD_MAP"

echo "[build_so] so = $SO"
echo "BUILD_OK md5=$MD5 size=$SIZE stamp=$STAMP_STR mode=$MODE buildid=$BUILDID"
