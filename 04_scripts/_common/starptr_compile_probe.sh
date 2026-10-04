#!/usr/bin/env bash
# ================================================================
#  StarPtr 迁移回归探针 —— 每个游戏 Mod.cpp 单翻译单元真编（-c，不链接）
#
#  为什么需要它：Ghidra/IDA 式的「静态看代码」抓不到 GAMES/*.cpp 里手抄的
#  指针区间判据；只有真编一遍才能确认 Includes/StarPtr.h 的 include 与
#  各处 LiveFast/LiveFastCounted/InUserVa 用法在每个游戏宏组合下都能过。
#
#  用法：
#    bash 04_scripts/_common/starptr_compile_probe.sh
#    SRC_FILES="Games/MxytzMod.cpp|-DSTARRY_GAME_MXYZT" bash ..._probe.sh   # 只测指定文件
#
#  机制：jni 树先整份拷到纯 ASCII 暂存目录再编（中文路径会让 Windows clang
#  报 "The system cannot find the file specified."，与 Application.mk 的
#  STARRY_BUILD_EPOCH 是同一类坑）；include 集合抄自 Android_gjc2.mk。
#
#  退出码：0 = 全部 OK；1 = 有 FAIL（日志里有 clang 原文）。
# ================================================================
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
JNI="$ROOT/mod菜单/starry-mod-menu/app/src/main/jni"
LOG="${STARPTR_PROBE_LOG:-$ROOT/09_test/_starptr_compile_log.txt}"
[ -d "$(dirname "$LOG")" ] || LOG="$ROOT/_starptr_compile_log.txt"
: > "$LOG"

[ -d "$JNI" ] || { echo "ERROR: 找不到 jni 树：$JNI" | tee -a "$LOG"; exit 1; }

# ---- NDK：环境变量 → 本机 Android SDK → macOS 默认位置 ----
pick_ndk() {
  local base
  for base in \
    "${ANDROID_NDK_HOME:-}" \
    "$(cygpath -u "${LOCALAPPDATA:-}" 2>/dev/null)/Android/Sdk/ndk" \
    "${ANDROID_SDK_ROOT:-}/ndk" "${ANDROID_HOME:-}/ndk" \
    "$HOME/Android/Sdk/ndk" "$HOME/Library/Android/sdk/ndk"; do
    [ -n "$base" ] || continue
    [ -d "$base" ] || continue
    local d
    for d in $(ls -d "$base"/*/ 2>/dev/null | sort -V -r); do
      if [ -x "$d/toolchains/llvm/prebuilt/windows-x86_64/bin/clang++.exe" ]; then echo "$d"; return 0; fi
      if [ -x "$d/toolchains/llvm/prebuilt/darwin-x86_64/bin/clang++" ]; then echo "$d"; return 0; fi
    done
  done
  return 1
}
NDK="$(pick_ndk)" || { echo "ERROR: 找不到 NDK（设 ANDROID_NDK_HOME）" | tee -a "$LOG"; exit 1; }
for TRIPLE in windows-x86_64 darwin-x86_64 linux-x86_64; do
  CLANG="$NDK/toolchains/llvm/prebuilt/$TRIPLE/bin/clang++"
  [ -x "$CLANG" ] || CLANG="$CLANG.exe"
  [ -x "$CLANG" ] && break
done
[ -x "$CLANG" ] || { echo "ERROR: 找不到 clang++（NDK=$NDK）" | tee -a "$LOG"; exit 1; }
echo "NDK  : $NDK" | tee -a "$LOG"
echo "clang: $CLANG" | tee -a "$LOG"

# ---- 纯 ASCII 暂存 ----
STAGE="$(cygpath -u "${TEMP:-${TMPDIR:-/tmp}}")/starptr_check"
rm -rf "$STAGE"; mkdir -p "$STAGE"
cp -a "$JNI" "$STAGE/jni" || { echo "ERROR: 拷贝 jni 失败" | tee -a "$LOG"; exit 1; }
SJ="$STAGE/jni"
echo "stage: $STAGE" | tee -a "$LOG"

SRC="$(cygpath -m "$SJ")"
INC=(-I"$SRC" -I"$SRC/Includes" -I"$SRC/Unity" -I"$SRC/Games"
     -I"$SRC/Dobby" -I"$SRC/KittyMemory"
     -I"$SRC/KittyMemory/Deps/Keystone/includes" -I"$SRC/xDL")
FLAGS=(--target=aarch64-linux-android21 -std=c++17 -fexceptions -fpermissive -w -c -O0)

# ---- 待测清单："源文件|该游戏在 Android*.mk 里启用的宏" ----
# 宏取自各游戏的 mk（例：Android_gjc2.mk 的 -DSTARRY_GAME_GJC2 -DENABLE_UNITY_HOOK）。
# 只启用一个 STARRY_GAME_* 是 GameConfig.h 的硬要求（见 SWITCH_GAME.md）。
DEFAULT_FILES=(
  "Games/MxytzMod.cpp|-DSTARRY_GAME_MXYZT"
  "Games/Archero2Mod.cpp|-DSTARRY_GAME_GJC2 -DENABLE_UNITY_HOOK"
  "Games/SlcbMod.cpp|"
  "Games/WxsjMod.cpp|-DSTARRY_GAME_WXSJ -DENABLE_UNITY_HOOK"
  "Games/WxsjVc1637Mod.cpp|-DSTARRY_GAME_WXSJ -DENABLE_UNITY_HOOK"
  "Games/DzqsMod.cpp|-DSTARRY_GAME_DZQS -DENABLE_UNITY_HOOK"
  "Games/HzsjMod.cpp|-DSTARRY_GAME_HZSJ -DENABLE_UNITY_HOOK"
  "Games/PhigrosMod.cpp|-DSTARRY_GAME_PHIGROS -DENABLE_UNITY_HOOK"
  "Games/TlgdMod.cpp|-DSTARRY_GAME_TLGD -DENABLE_UNITY_HOOK"
  "Games/TlgdTapTapMod.cpp|"
  "Games/YwgnMod.cpp|-DSTARRY_GAME_YWGN -DENABLE_UNITY_HOOK"
)
if [ -n "${SRC_FILES:-}" ]; then
  IFS=$'\n' read -r -d '' -a FILES <<< "$SRC_FILES" || true
else
  FILES=("${DEFAULT_FILES[@]}")
fi

OKC=0; BADC=0; BADLIST=""
for ent in "${FILES[@]}"; do
  f="${ent%%|*}"; defs="${ent#*|}"
  if [ ! -f "$SJ/$f" ]; then
    echo "MISS $f（源文件不存在）" | tee -a "$LOG"; BADC=$((BADC+1)); BADLIST="$BADLIST $f"; continue
  fi
  out="$STAGE/$(basename "$f").o"
  # shellcheck disable=SC2086
  if "$CLANG" "${FLAGS[@]}" $defs "${INC[@]}" "$(cygpath -m "$SJ/$f")" -o "$(cygpath -m "$out")" >>"$LOG" 2>&1; then
    echo "OK   $f" | tee -a "$LOG"; OKC=$((OKC+1))
  else
    echo "FAIL $f  <-- 见日志尾部 clang 原文" | tee -a "$LOG"; BADC=$((BADC+1)); BADLIST="$BADLIST $f"
  fi
done

# ---- 铁律扫描：除 Includes/StarPtr.h 外，任何地方都不许手抄指针区间判据 ----
# 判据：出现 0x800000000000（旧的上界）或 0x00FFFFFFFFFFFFFF（掩码）即视为手抄。
RAW=0
while IFS= read -r hit; do
  [ -n "$hit" ] || continue
  echo "FAIL 手抄指针区间判据（应改用 Includes/StarPtr.h）：$hit" | tee -a "$LOG"
  RAW=$((RAW+1))
done < <(grep -rn -E '0x800000000000|0x00FFFFFFFFFFFFFF' "$SJ" \
           --include='*.cpp' --include='*.h' --include='*.hpp' 2>/dev/null \
         | grep -v '/Includes/StarPtr\.h:' \
         | grep -vE ':[0-9]+:[[:space:]]*(//|\*|/\*)' || true)
[ "$RAW" -eq 0 ] && echo "OK   指针区间判据唯一入口 = Includes/StarPtr.h" | tee -a "$LOG"

# ---- 覆盖度提示（不失败）：Games 下未纳入探针的文件数 ----
TODO_CNT=0
for g in "$SJ"/Games/*.cpp; do
  b="Games/$(basename "$g")"
  case " ${FILES[*]//|/ } " in *" ${b%%|*} "*) ;; *) TODO_CNT=$((TODO_CNT+1)) ;; esac
done
echo "INFO 探针覆盖 $((${#FILES[@]} - BADC)) 个 TU；Games 下另有 $TODO_CNT 个 .cpp 未登记（其指针判据由上面的铁律扫描兜底）" | tee -a "$LOG"

echo "----" | tee -a "$LOG"
echo "OK=$OKC FAIL=$((BADC+RAW))${BADLIST:+  ($BADLIST)}" | tee -a "$LOG"
[ "$((BADC+RAW))" -eq 0 ] || exit 1
