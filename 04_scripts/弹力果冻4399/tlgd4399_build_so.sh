#!/usr/bin/env bash
# 弹力果冻 **4399 渠道**（com.ginicat.sweet.m4399，vc4412）管线 build_so 阶段后端
#   2026-10-07 稳定性整改：把 TapTap 2.4.0 已实测稳定的架构移植到 4399 包
# 产物 = mod菜单/starry-mod-menu/libs_tlgdm4399/arm64-v8a/libstarry_magic.so
#
# 与 TapTap 版（04_scripts/弹力果冻/tlgd_build_so.sh）的差异：
#   - APP_BUILD_SCRIPT=Android_tlgdm4399.mk（入口 Games/TlgdM4399Entry.cpp，tag=Tlgd）
#   - 独立 OUT_LIBS=libs_tlgdm4399 / OBJ=obj_tlgdm4399（绝不与 TapTap 构建互踩）
#   - 字节校验换成「必须含 4399 新架构证据 + 必须不含 tlgdtt」
#   - mk 内生 -DSTARRY_TLGD_M4399 → 编译期即确立 kFileTrampoline=0xBAAB0C
#
# 范本 = 04_scripts/魂坠深境/hzsj_build_so.sh（四件套：绝对路径 / obj stamp /
#        链接活动保险 / 菜单串按字节校验），不自行发明。
set -euo pipefail

# 2026-10-26 双机：NIXIANG 可由环境覆盖（mac 默认路径保留）；Windows 侧先 source tlgd4399_env_win.sh
NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_tlgdm4399"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
OBJ="$STARRY_SRC/obj_tlgdm4399"
MK="Android_tlgdm4399.mk"

[[ -x "$NDK/ndk-build" || -f "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/$MK" ]] || { echo "ERROR: missing $JNI/$MK" >&2; exit 1; }
[[ -f "$JNI/Games/TlgdM4399Entry.cpp" ]] || { echo "ERROR: missing TlgdM4399Entry.cpp" >&2; exit 1; }
[[ -f "$JNI/Games/TlgdTapTapMod.cpp" ]] || { echo "ERROR: missing TlgdTapTapMod.cpp" >&2; exit 1; }

# ---- 构建前护栏：mk 必须定义 4399 标记（否则 TlgdTapTapMod.cpp 会退回 TapTap 偏移）----
LC_ALL=C grep -q -- '-DSTARRY_TLGD_M4399' "$JNI/$MK" \
  || { echo "ERROR: $MK 缺 -DSTARRY_TLGD_M4399 —— 会挂错地址（TapTap 0xBABB0C）" >&2; exit 1; }

STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk / 开关 / **全部参与编译的本游戏源码**变化 → 清 obj 强制全量重编 ----
# 范本只哈希 mk + Application.mk；实测这不够：MainModern.cpp / Unity/* / Includes/*
# 改动不会触发重编 → 可能拿到旧 so。故本脚本把 jni 根、Games/、Unity/、Includes/
# 下的 .cpp/.c/.h 全量纳入哈希（第三方目录 Dobby/KittyMemory/xDL 除外，它们随 NDK 固定）。
SRC_FILES="$(find "$JNI" -maxdepth 1 -type f \( -name '*.cpp' -o -name '*.c' -o -name '*.h' \) ; \
            find "$JNI/Games" "$JNI/Unity" "$JNI/Includes" -type f \( -name '*.cpp' -o -name '*.c' -o -name '*.h' \) 2>/dev/null \
            | LC_ALL=C sort)"
CFG_HASH="$( (cat "$JNI/$MK" "$JNI/Application.mk"; \
  # shellcheck disable=SC2086
  cat $SRC_FILES; \
  printf 'PROBE=%s\nTRACE=%s\nEPOCH=%s\n' "${STARRY_TLGD_PROBE:-1}" "${STARRY_TRACE:-}" "${STARRY_EPOCH:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置/源码变化（mk / Application.mk / jni 源码）→ 清 obj_tlgdm4399 强制全量重编"
  rm -rf "$OBJ"
fi

# NDK_OUT/NDK_LIBS_OUT 必须绝对路径（相对路径会被 ndk-build -C 解析到 app/ 下 → 假成功）
# 构建戳：Application.mk 用 `git log -1 --format=%ct` 取提交时间，但 Windows 分支把 jni stage 到
# /c/tlgd_build_stage（不在 git 仓库内）⇒ 取不到并退回兜底 1790184000，gate check8 直接 FAIL。
# 因此在这里从仓库取一次，显式传给 ndk-build（并在 CFG_HASH 里带上，戳变了强制重编）。
STARRY_EPOCH="${STARRY_BUILD_EPOCH:-$( (cd "$NIXIANG" && git log -1 --format=%ct) 2>/dev/null | tr -cd '0-9')}"
[[ -n "$STARRY_EPOCH" ]] || { echo "ERROR: 取不到 git 提交时间，请显式传 STARRY_BUILD_EPOCH=<秒>" >&2; exit 1; }
echo "[build_so 4399] STARRY_BUILD_EPOCH=$STARRY_EPOCH"

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
# ---- Windows(Git Bash) 分支（2026-10-26 雷电轮）------------------------------------
# NDK 在 Windows 上给 make 发的是 cmd 批处理命令（rem/del/copy/md/move），MSYS 的 /usr/bin/sh
# 不认 ⇒ 必须走 ndk-build.cmd（cmd.exe 驱动）；另本仓 jni 路径含中文（mod菜单/）⇒ 先 stage 到
# ASCII 目录再编。范本 = 04_scripts/勇士之心/ysxz_build_so.sh（2026-10-25 雷电实测打通）。
_TLGD_IS_WIN=0
case "$(uname -s 2>/dev/null || echo unknown)" in
  MINGW*|MSYS*|CYGWIN*) _TLGD_IS_WIN=1 ;;
esac
if [[ "$_TLGD_IS_WIN" == "1" ]]; then
  NDKC="$(cygpath -u "$NDK")/build/ndk-build.cmd"
  [[ -f "$NDKC" ]] || { echo "ERROR: 缺 Windows ndk-build.cmd: $NDKC" >&2; exit 1; }
  STAGE="${TLGD_WIN_STAGE:-/c/tlgd_build_stage}"
  rm -rf "$STAGE/jni" "$STAGE/obj" "$STAGE/libs"
  mkdir -p "$STAGE/jni"
  cp -R "$JNI/." "$STAGE/jni/"
  W_JNI="$(cygpath -w "$STAGE/jni")"
  W_PROJ="$(cygpath -w "$STAGE")"
  W_OBJ="$(cygpath -w "$STAGE/obj")"
  W_LIBS="$(cygpath -w "$STAGE/libs")"
  echo "[build_so 4399] win 分支: stage=$STAGE"
  ( cd "$STAGE/jni" && cmd //c "$(cygpath -w "$NDKC")" -C "$W_JNI" \
      NDK_PROJECT_PATH="$W_PROJ" \
      APP_BUILD_SCRIPT="./$MK" \
      NDK_APPLICATION_MK="./Application.mk" \
      NDK_OUT="$W_OBJ" \
      NDK_LIBS_OUT="$W_LIBS" \
      STARRY_BUILD_EPOCH="$STARRY_EPOCH" \
      -j"$(nproc 2>/dev/null || echo 8)" ) || BUILD_RC=$?
  if [[ $BUILD_RC -eq 0 && -f "$STAGE/libs/arm64-v8a/libstarry_magic.so" ]]; then
    mkdir -p "$OUT_LIBS/arm64-v8a"
    cp -f "$STAGE/libs/arm64-v8a/libstarry_magic.so" "$SO"
    # APP_STL=c++_shared ⇒ ndk-build 的 Install 步骤会把 libc++_shared.so 一并放进 libs/，
    # 必须同步回仓内 libs 目录，否则 surgical 阶段找不到（2026-10-26 Windows 分支实测）。
    for _so in "$STAGE/libs/arm64-v8a/"*.so; do
      [[ "$(basename "$_so")" == "libstarry_magic.so" ]] && continue
      cp -f "$_so" "$OUT_LIBS/arm64-v8a/"
    done
    unset _so
    NEW_O="$(find "$STAGE/obj" -name '*.o' 2>/dev/null | head -1 || true)"
  fi
  rm -f "$T0"
else
  "$NDK/ndk-build" -C "$JNI" \
    NDK_PROJECT_PATH="../.." \
    APP_BUILD_SCRIPT="./$MK" \
    NDK_APPLICATION_MK="./Application.mk" \
    NDK_OUT="$OBJ" \
    NDK_LIBS_OUT="$OUT_LIBS" \
    STARRY_BUILD_EPOCH="$STARRY_EPOCH" \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || nproc 2>/dev/null || echo 8)" || BUILD_RC=$?
  # `|| true`：set -o pipefail 下 find|head 的 SIGPIPE 会让赋值返回 141 而中止脚本（同 install.sh 踩坑）
  NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1 || true)"
  rm -f "$T0"
fi
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险（本脚本与 tlgd_build_so.sh 范本的两处**有意**差异，2026-10-07）----
# 范本判据 = 「有新 .o 但 so md5 未变 → 报旧 so 顶包」。两个问题实测：
#   ① 假阳性：只改 mk 注释这类惰性变更时，stamp 清 obj → 全量重编 → 产出**字节相同**的 so，
#      被误判成顶包，拦住正确构建（本轮移除 wxsj_dll 时正好复现）。
#   ② 想改成「obj 产物 vs libs 产物 md5 相等」也不行：ndk-build 的 Install 步骤会再跑一次
#      strip（实测 obj=166,576 B / libs=166,568 B，有 651 字节差异），两者 md5 天然不同。
# 因此本脚本改为：把「旧 so 顶包」的防线前移到 **obj stamp 覆盖全部本游戏源码**（见上），
# 构建后只做「有新 .o 且 so 字节未变」的提示（惰性改动属正常），再叠加下面的证据串字节校验
# —— 证据串能直接分辨「这是不是本次该出的那个 so」，比 md5 时间戳判据可靠。
if [[ -n "${NEW_O:-}" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "[note] 本次重编了 .o 但 so 字节未变（$(md5 -q "$SO")）——mk/源码改动是惰性的，属正常"
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 防呆 A：按字节校验菜单串/标签（strings 不认 UTF-8 中文，必须 LC_ALL=C）----
NEW_MENU=$(LC_ALL=C grep -ac "Category_战斗" "$SO" || true)
[[ "$NEW_MENU" -ge 1 ]] || { echo "ERROR: $SO 不含 Category_战斗（旧 so 混入或源码未编译）" >&2; exit 1; }
NEW_FEAT=$(LC_ALL=C grep -ac "Toggle_全屏秒杀" "$SO" || true)
[[ "$NEW_FEAT" -ge 1 ]] || { echo "ERROR: $SO 不含 Toggle_全屏秒杀（新功能位未编入）" >&2; exit 1; }
NEW_FTAG=$(LC_ALL=C grep -ac "\[Tlgd\]" "$SO" || true)
[[ "$NEW_FTAG" -ge 1 ]] || { echo "ERROR: $SO 不含 [Tlgd] 框架标签字面量" >&2; exit 1; }

# ---- 防呆 B：必须含「TapTap 稳定架构」证据串，证明不是旧 4399 legacy mod ----
# TlgdTapTapMod.cpp:617 LOGI(TLGD_TAG " 解释器入口已挂：file=0x%llx va=%p orig=%p ...")
NEW_ARCH=$(LC_ALL=C grep -ac "解释器入口已挂" "$SO" || true)
[[ "$NEW_ARCH" -ge 1 ]] \
  || { echo "ERROR: $SO 不含「解释器入口已挂」——不是 TlgdTapTapMod 架构（旧 legacy so 混入？）" >&2; exit 1; }
NEW_ARCH2=$(LC_ALL=C grep -ac "原生桥补发" "$SO" || true)
[[ "$NEW_ARCH2" -ge 1 ]] \
  || { echo "ERROR: $SO 不含「HpHandle 原生桥补发」证据串" >&2; exit 1; }

# ---- 防呆 C：绝不允许 TapTap 入口串混入（authGameId 串字面量 "tlgdtt"）----
if LC_ALL=C grep -aq "tlgdtt" "$SO"; then
  echo "ERROR: $SO 含 tlgdtt —— 编进了 TapTap Entry，authGameId 会错成 tlgdtt" >&2; exit 1
fi
if LC_ALL=C grep -aq "\[Dwct\]" "$SO"; then
  echo "ERROR: $SO 含 [Dwct]（上一款游戏残留，jniLibs/mk 接错）" >&2; exit 1
fi

echo "[build_so 4399] done → $SO"
ls -l "$SO"
md5 -q "$SO"
