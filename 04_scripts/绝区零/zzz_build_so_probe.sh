#!/usr/bin/env bash
# ================================================================
#  绝区零 3.3.0 — 骨架探针 so 构建（只读探针，不装 hook）
#
#  产物：mod菜单/starry-mod-menu/libs_zzz_probe/arm64-v8a/libstarry_magic.so
#  与正式构建共用 zzz_build_so.sh，只换 makefile / 输出目录 / obj 目录（互不污染）：
#    源文件三件套 = MainModern.cpp + Games/ZzzProbeMod.cpp + Games/ZzzEntry.cpp
#
#  用法：bash 04_scripts/绝区零/zzz_build_so_probe.sh
# ================================================================
set -euo pipefail
SELF_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="${NIXIANG:-$(cd "$SELF_DIR/../.." && pwd)}"
JNI="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni"

[ -f "$JNI/Android_zzz_probe.mk" ] || { echo "ERROR: 缺 $JNI/Android_zzz_probe.mk" >&2; exit 1; }
[ -f "$JNI/Games/ZzzProbeMod.cpp" ] || { echo "ERROR: 缺 $JNI/Games/ZzzProbeMod.cpp" >&2; exit 1; }

export ZZZ_MK="$JNI/Android_zzz_probe.mk"
export ZZZ_OUT_LIBS="$NIXIANG/mod菜单/starry-mod-menu/libs_zzz_probe"
export ZZZ_OBJ="$NIXIANG/mod菜单/starry-mod-menu/obj_zzz_probe"
export ZZZ_TAG="zzzprobe"

exec bash "$SELF_DIR/zzz_build_so.sh"
