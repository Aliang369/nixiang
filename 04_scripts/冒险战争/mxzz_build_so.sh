#!/usr/bin/env bash
# 冒险战争 (mxzz) — so 构建后端
#
# 四件套（照 04_scripts/汉克旅店/build_hkld_so.sh 的既有口径，禁自创）：
#   ① 载荷头（官方 sha 白名单 + 补丁体长度，由 gen_mxzz_payload_header.py 生成）
#   ② objcopy 内嵌补丁 dll 为静态库 jni/libs_mxzz/MxzzPatchedDll.a
#   ③ ndk-build（**绝对路径** NDK_OUT/LIBS_OUT，相对路径会假成功）
#   ④ 产物自检（so 标签 / 菜单串 / 无 emutls / JNI_OnLoad 导出 / 载荷符号不外泄）
#
# 用法:
#   bash 04_scripts/冒险战争/mxzz_build_so.sh
#   MXZZ_ABI="arm64-v8a" bash ...            # 默认只 arm64（底包无 x86_64）
#   MXZZ_SKIP_NDK_BUILD=1 bash ...           # 只重做载荷头 + .a
#
# 前置：07_patched/冒险战争/mygame_mxzz_{dev_1085,base}.dll 已由 ilpatch 产出且 Gate PASS。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "${SCRIPT_DIR}/../.." && pwd)"
STARRY="${NIXIANG}/mod菜单/starry-mod-menu"
# ★ ASCII junction：mod菜单 含中文，ndk-build(Windows) 在非 ASCII 路径下必然
#   "unable to open output file ... No such file or directory"（make 侧路径乱码）。
#   仓库既有 starry_link -> mod菜单/starry-mod-menu（与 04_scripts/无限升级/build_wxsj_vc1637_win.sh 同口径）。
LINK="${NIXIANG}/starry_link"
JNI="${LINK}/app/src/main/jni"
if [ ! -d "${JNI}" ]; then echo "ERROR: 缺 ASCII junction ${LINK}（cmd /c mklink /J ...）"; exit 1; fi
ABI="${MXZZ_ABI:-arm64-v8a}"
LIBS_OUT_DIR="${LINK}/libs_mxzz"
OBJ_DIR="${LINK}/obj_mxzz"

# shellcheck source=/dev/null
source "${NIXIANG}/04_scripts/_common/env.sh"
PY="${NIXIANG_PYTHON:-python}"

# ---- NDK ----
# NDK：优先仓库自带 r27c（与汉克旅店/无限升级同版本），其次 ANDROID_NDK_HOME，最后 SDK 目录
NDK="${ANDROID_NDK_HOME:-}"
_REPO_NDK="${NIXIANG}/01_tools/android-sdk/ndk/android-ndk-r27c"
if [ -d "${_REPO_NDK}" ]; then NDK="${_REPO_NDK}"; fi
if [ -z "${NDK}" ] || [ ! -d "${NDK}" ]; then
  case "$(uname -s 2>/dev/null || echo unknown)" in
    MINGW*|MSYS*|CYGWIN*) _sdk="${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk" ;;
    *)                    _sdk="$HOME/Library/Android/sdk" ;;
  esac
  NDK="$(ls -d "${_sdk}"/ndk/* 2>/dev/null | sort -V | tail -1)"
fi
if [ -z "${NDK}" ] || [ ! -d "${NDK}" ]; then echo "ERROR: 找不到 NDK（设 ANDROID_NDK_HOME）"; exit 1; fi

BIN_DIR="$(ls -d "${NDK}"/toolchains/llvm/prebuilt/*/bin 2>/dev/null | head -1)"
OBJCOPY="$(ls "${BIN_DIR}"/llvm-objcopy* 2>/dev/null | head -1)"
NM="$(ls "${BIN_DIR}"/llvm-nm* 2>/dev/null | head -1)"
AR="$(ls "${BIN_DIR}"/llvm-ar* 2>/dev/null | head -1)"
UNAME_S="$(uname -s 2>/dev/null || echo unknown)"
if [ "${UNAME_S}" = "Darwin" ]; then
  NDKBUILD="${NDK}/ndk-build"
else
  NDKBUILD="${NDK}/ndk-build.cmd"; [ -f "${NDKBUILD}" ] || NDKBUILD="${NDK}/ndk-build"
fi
[ -f "${OBJCOPY}" ] || { echo "ERROR: 缺 llvm-objcopy（${BIN_DIR}）"; exit 1; }
[ -f "${NM}" ] || { echo "ERROR: 缺 llvm-nm"; exit 1; }
[ -f "${AR}" ] || { echo "ERROR: 缺 llvm-ar"; exit 1; }

# 构建戳（查8）：Application.mk 从 git 取不到就退回 1790184000，而兜底值恒小于真戳 ⇒
# 覆盖安装不清 starry_ota 旧 so（玩家跑旧 so）。这里显式从 git 取提交时间后传下去。
if [ -z "${STARRY_BUILD_EPOCH:-}" ]; then
  STARRY_BUILD_EPOCH="$(git -C "${NIXIANG}" log -1 --format=%ct 2>/dev/null || true)"
  case "${STARRY_BUILD_EPOCH}" in
    ''|*[!0-9]*) echo "WARN: 取不到 git 时间，查8 会失败（或传 STARRY_BUILD_EPOCH=<秒>）"; STARRY_BUILD_EPOCH=1790184000 ;;
  esac
fi
export STARRY_BUILD_EPOCH

echo "[0/5] 功能下标门禁（featNum 与 MXZZ_FEAT_* 对齐）"
"${PY}" "${SCRIPT_DIR}/check_mxzz_feat_index.py" || die "功能下标门禁未过（开错功能的典型根因）"

echo "[1/5] 载荷头（官方 sha 白名单 + 补丁体长度/sha）"
"${PY}" "${SCRIPT_DIR}/gen_mxzz_payload_header.py" || { echo "ERROR: 载荷头生成失败"; exit 1; }
HDR="${JNI}/Games/MxzzPayload.h"
[ -f "${HDR}" ] || { echo "ERROR: 缺 ${HDR}"; exit 1; }
grep -q 'MXZZ_PAYLOAD_GENERATED 1' "${HDR}" || { echo "ERROR: 载荷头不是生成物"; exit 1; }
if grep -q 'define MXZZ_PAYLOAD_COUNT 0' "${HDR}"; then echo "ERROR: 载荷表为空"; exit 1; fi

echo "[2/5] objcopy 内嵌 .a（dev + base 两份补丁体）"
mkdir -p "${JNI}/libs_mxzz" "${LIBS_OUT_DIR}" "${OBJ_DIR}"
WORK="$(mktemp -d)"
trap 'rm -rf "${WORK}"' EXIT
DEV_DLL="${NIXIANG}/07_patched/冒险战争/mygame_mxzz_dev_1085.dll"
BASE_DLL="${NIXIANG}/07_patched/冒险战争/mygame_mxzz_base.dll"
[ -f "${DEV_DLL}" ] || { echo "ERROR: 缺 ${DEV_DLL}（先跑 ilpatch）"; exit 1; }
[ -f "${BASE_DLL}" ] || { echo "ERROR: 缺 ${BASE_DLL}（先跑 ilpatch）"; exit 1; }
cp "${DEV_DLL}" "${WORK}/mxzz_dev.bin"
cp "${BASE_DLL}" "${WORK}/mxzz_base.bin"
( cd "${WORK}" && \
  "${OBJCOPY}" -I binary -O elf64-littleaarch64 -B aarch64 mxzz_dev.bin  mxzz_dev.o  && \
  "${OBJCOPY}" -I binary -O elf64-littleaarch64 -B aarch64 mxzz_base.bin mxzz_base.o && \
  rm -f MxzzPatchedDll.a && \
  "${AR}" rcs MxzzPatchedDll.a mxzz_dev.o mxzz_base.o )
cp -f "${WORK}/MxzzPatchedDll.a" "${JNI}/libs_mxzz/MxzzPatchedDll.a"
"${NM}" "${JNI}/libs_mxzz/MxzzPatchedDll.a" | grep -q '_binary_mxzz_dev_bin_start' \
  || { echo "ERROR: .a 缺 _binary_mxzz_dev_bin_start"; exit 1; }
"${NM}" "${JNI}/libs_mxzz/MxzzPatchedDll.a" | grep -q '_binary_mxzz_base_bin_start' \
  || { echo "ERROR: .a 缺 _binary_mxzz_base_bin_start"; exit 1; }
echo "  libs_mxzz/MxzzPatchedDll.a <- dev $(wc -c < "${DEV_DLL}" | tr -d ' ') B + base $(wc -c < "${BASE_DLL}" | tr -d ' ') B"

if [ "${MXZZ_SKIP_NDK_BUILD:-0}" = "1" ]; then
  echo "[3/5] 跳过 ndk-build（MXZZ_SKIP_NDK_BUILD=1）"; exit 0
fi

echo "[3/5] ndk-build（ABI=${ABI}，绝对路径 obj/libs）"
# 构建戳变了必须**重编**：ndk-build 只看源文件 mtime，env 变化不会触发 relink（实测：
# 只改 STARRY_BUILD_EPOCH 时 so md5 完全不变 ⇒ 查8 仍看兜底值）。用 epoch 戳文件强制失效。
EPOCH_STAMP="${OBJ_DIR}/.starry_epoch"
RECORDED="$(cat "${EPOCH_STAMP}" 2>/dev/null || echo none)"
if [ "${RECORDED}" != "${STARRY_BUILD_EPOCH}" ]; then
  echo "  epoch ${RECORDED} -> ${STARRY_BUILD_EPOCH}：清 obj 强制重编"
  rm -rf "${OBJ_DIR}/local"
  printf '%s' "${STARRY_BUILD_EPOCH}" > "${EPOCH_STAMP}"
fi
OBJ_OUT="$("${PY}" -c "import os,sys;print(os.path.abspath(sys.argv[1]))" "${OBJ_DIR}")"
LIBS_OUT="$("${PY}" -c "import os,sys;print(os.path.abspath(sys.argv[1]))" "${LIBS_OUT_DIR}")"
if command -v cygpath >/dev/null 2>&1; then
  OBJ_OUT="$(cygpath -m "${OBJ_OUT}")"
  LIBS_OUT="$(cygpath -m "${LIBS_OUT}")"
fi
( cd "${JNI}" && "${NDKBUILD}" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_mxzz.mk \
    NDK_APPLICATION_MK=./Application.mk APP_ABI="${ABI}" \
    NDK_OUT="${OBJ_OUT}" NDK_LIBS_OUT="${LIBS_OUT}" -j 4 )
SO="${LIBS_OUT_DIR}/${ABI}/libstarry_magic.so"
[ -f "${SO}" ] || { echo "ERROR: 缺 ${SO}"; exit 1; }

# [4/5] jniLibs 同步：**默认关**。
#   本作出包走外科 zip（so 从 libs_mxzz 直接取），不需要 Gradle 打包；
#   把 so 拷进 app/src/main/jniLibs/ 反而会污染**其它游戏**的菜单 APK 构建
#   （Gradle 会把该目录的 so 一并打进去 ⇒ 别的游戏菜单里混进 mxzz 的 so）。
#   确实要用 Gradle 出菜单 APK 时：MXZZ_SYNC_JNILIBS=1 bash ...
if [ "${MXZZ_SYNC_JNILIBS:-0}" = "1" ]; then
  echo "[4/5] 同步 jniLibs（MXZZ_SYNC_JNILIBS=1）"
  for dest in "${STARRY}/app/src/main/jniLibs/${ABI}" "${STARRY}/app/src/main/libs/${ABI}" "${STARRY}/app/libs/${ABI}"; do
    mkdir -p "${dest}"; cp -f "${SO}" "${dest}/libstarry_magic.so"
  done
else
  echo "[4/5] 跳过 jniLibs 同步（默认；外科出包不依赖 Gradle）"
fi

echo "[5/5] 产物自检"
LC_ALL=C grep -a -q '\[Mxzz\]' "${SO}" || { echo "ERROR: so 无 [Mxzz] 标签"; exit 1; }
for s in 秒杀 无敌; do
  LC_ALL=C grep -a -q "${s}" "${SO}" || { echo "ERROR: so 缺菜单串 ${s}"; exit 1; }
done
LC_ALL=C grep -a -q 'Toggle_秒杀' "${SO}" || { echo "ERROR: so 缺 Toggle_秒杀"; exit 1; }
if "${NM}" -D --undefined-only "${SO}" 2>/dev/null | grep -q emutls; then
  echo "ERROR: so 含 __emutls_* 未定义符号"; exit 1
fi
"${NM}" -D --defined-only "${SO}" 2>/dev/null | grep -q ' T JNI_OnLoad' || { echo "ERROR: so 未导出 JNI_OnLoad"; exit 1; }
if "${NM}" -D --defined-only "${SO}" 2>/dev/null | grep -q ' T _binary_mxzz'; then
  echo "ERROR: 载荷符号被导出（应 --exclude-libs,ALL 隐藏）"; exit 1
fi
SHA_PY='import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],"rb").read()).hexdigest())'
echo "  OK  ${SO}"
echo "      size=$(wc -c < "${SO}" | tr -d ' ')  sha256=$("${PY}" -c "${SHA_PY}" "${SO}")"
echo "      dev 补丁体 sha256=$("${PY}" -c "${SHA_PY}" "${DEV_DLL}")"
echo "      base 补丁体 sha256=$("${PY}" -c "${SHA_PY}" "${BASE_DLL}")"
