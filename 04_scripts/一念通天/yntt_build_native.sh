#!/usr/bin/env bash
# 一念通天 pipeline 阶段 build_so —— 用 Android_yntt.mk 直编星光 so（modern 注册表）。
#
# 防呆四件套（沿用 GAME_MOD_GUIDE 定稿）：
#   ① 绝对路径 NDK_OUT / NDK_LIBS_OUT
#   ② obj stamp：mk / 探针开关变化即清 obj 全量重编（防新旧 .o 混链）
#   ③ 链接活动保险：本次产出新 .o 而 so 字节未变 = 旧 so 顶包嫌疑，直接 FAIL
#   ④ 菜单串按字节校验：6 个点名单必须能在 so 里找到
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

STARRY_SRC="${NIXIANG_ROOT}/mod菜单/starry-mod-menu"
JNI="${STARRY_SRC}/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-${HOME}/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="${STARRY_SRC}/libs_yntt"
SO="${OUT_LIBS}/arm64-v8a/libstarry_magic.so"
OBJ="${STARRY_SRC}/obj_yntt"
STAMP="${OBJ}/.build_stamp"

yntt_need_exec "${NDK}/ndk-build"
yntt_need_file "${JNI}/Android_yntt.mk"
yntt_need_file "${JNI}/Application.mk"
yntt_need_file "${JNI}/Games/YnttMod.cpp"

CFG_HASH="$( { cat "${JNI}/Android_yntt.mk" "${JNI}/Application.mk"; printf 'TRACE=%s\n' "${STARRY_TRACE:-}"; } | shasum -a 256 | awk '{print $1}')"
if [ -f "${STAMP}" ] && [ "$(cat "${STAMP}" 2>/dev/null)" != "${CFG_HASH}" ]; then
  yntt_log "构建配置变化 → 清 obj_yntt 强制全量重编"
  rm -rf "${OBJ}"
fi

SO_MD5_BEFORE="$(md5 -q "${SO}" 2>/dev/null || echo none)"
T0="$(mktemp)"
touch "${T0}"
BUILD_RC=0
"${NDK}/ndk-build" -C "${JNI}" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_yntt.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="${OBJ}" \
  NDK_LIBS_OUT="${OUT_LIBS}" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "${OBJ}" -name '*.o' -newer "${T0}" 2>/dev/null | head -1)"
rm -f "${T0}"
[ "${BUILD_RC}" -eq 0 ] || yntt_die "ndk-build 失败 rc=${BUILD_RC}"
yntt_need_file "${SO}"

if [ -n "${NEW_O}" ] && [ "$(md5 -q "${SO}")" = "${SO_MD5_BEFORE}" ]; then
  yntt_die "本次构建产生了新 .o 但 ${SO} 未变化——旧 so 顶包嫌疑，禁止通过"
fi
mkdir -p "${OBJ}"
printf '%s' "${CFG_HASH}" > "${STAMP}"

TAG_HITS="$(LC_ALL=C grep -ac 'yntt' "${SO}" || true)"
[ "${TAG_HITS}" -ge 1 ] || yntt_die "${SO} 不含 yntt 标签"
for menu in "Toggle_秒杀" "Toggle_无敌" "Slider_战斗加速" "Toggle_无限体力" "Toggle_无限货币" "Toggle_免广告"; do
  HITS="$(LC_ALL=C grep -ac "${menu}" "${SO}" || true)"
  [ "${HITS}" -ge 1 ] || yntt_die "${SO} 不含菜单串 ${menu}"
done

yntt_log "so=${SO}"
yntt_log "md5=$(md5 -q "${SO}")"
yntt_log "sha256=$(yntt_sha256 "${SO}")"
yntt_log "size=$(yntt_size "${SO}")"
yntt_log "done"
