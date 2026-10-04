#!/usr/bin/env bash
# 鬼谷八荒 1.1.329 (com.guigugame.guigubahuang)：管线 build_so 后端 —— 私有 Android_ggbh.mk 直编（modern 注册表，不切共享 mk）
# 引擎：Unity IL2CPP（waitLib=libil2cpp.so）⇒ 源文件含 Unity/Il2CppApi.cpp
# 产物 = mod菜单/starry-mod-menu/libs_ggbh/arm64-v8a/libstarry_magic.so
#
# 底包（本脚本不读底包，仅登记期望值）：官方 TapTap 原版 1.1.329
#   1,678,372,111 B / md5 f6cc1a4eb063bc0505cb93b68020d714
#   默认路径 $NIXIANG/02_apks/鬼谷八荒/鬼谷八荒_1.1.329_taptap_official_from_device.apk
#   （该默认路径当前不存在；本机实测底包在 /tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk，
#    首次跑 surgical 时用 GGBH_BASE_APK=/tmp/ggbh_stock/ggbh_1.1.329_official_from_device.apk 指过去，
#    脚本本身不写死 /tmp。见 mod菜单/starry-mod-menu/scripts/surgical_ggbh_1.1.329.sh）
# 产物路径：unsigned 08_output/鬼谷八荒/鬼谷八荒_1.1.329_surgical-unsigned.apk
#          signed   08_output/鬼谷八荒/鬼谷八荒_1.1.329_surgical-releaseSigned.apk
#          留证     07_patched/鬼谷八荒/surgical/{AndroidManifest.xml,classes2.dex}
#
# 注意：jniLibs/arm64-v8a/libstarry_magic.so 是 gradle 打包时实际取用的那份，
# 且被所有并行游戏会话共享 —— surgical 脚本会在 gradle 之后按字节校验归属，
# 此处只负责产出 libs_ggbh 并同步到 jniLibs。
#
#  普通构建： bash 04_scripts/鬼谷八荒/ggbh_build_so.sh
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_ggbh"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_ggbh.mk" ]] || { echo "ERROR: missing $JNI/Android_ggbh.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_ggbh"
STAMP="$OBJ/.build_stamp"
CFG_HASH="$( (cat "$JNI/Android_ggbh.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_ggbh 强制全量重编"
  rm -rf "$OBJ"
fi

SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_ggbh.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# 链接活动保险：有新 .o 但 so 字节未变 → 旧 so 顶包嫌疑
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 归属防呆：本作串必须在、别家串必须不在（跨会话共享目录，禁止凭 mtime 判断）----
# 中文 UTF-8 多字节，strings 默认 ASCII 会漏报 ⇒ LC_ALL=C grep -a 按字节搜
# 命中计数实测（2026-10-04，md5 b333dd6b8f834b1366dc1122ce3da8a3 / 2,190,416 B）：
#   Category_鬼谷功能=1  Toggle_GM菜单=1  SeekBar_加速倍率_1_20_1=1  ggbh=1
#   gmCheat=2  deltaTime=5  setTimeScale=3  SELFCHECK=1  HITS=1  [Ggbh]=10
# ①② 本作唯一串（换包必失败）；③ 功能挂点锚点（防「源码没编进 so」）；
# ④ gmCheat/deltaTime/setTimeScale/HITS/SELFCHECK 是挂点标识串，本作独占来源，作存在性断言。
for s in "Category_鬼谷功能" "Toggle_GM菜单" "SeekBar_加速倍率_1_20_1" "ggbh"; do
  n="$(LC_ALL=C grep -ac -- "$s" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含本作串 ${s}（旧 so 混入或源码未编译）" >&2; exit 1; }
done
for s in "gmCheat" "deltaTime" "setTimeScale" "SELFCHECK" "HITS"; do
  n="$(LC_ALL=C grep -ac -- "$s" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含功能挂点锚点 ${s}（GgbhMod.cpp 未编进 so）" >&2; exit 1; }
done
n="$(LC_ALL=C grep -ac -- "\[Ggbh\]" "$SO" || true)"
[[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含 [Ggbh] 日志标签" >&2; exit 1; }
# 注意：必须转义方括号，否则 "[Xnbz]" 被 grep 当正则字符类，匹配任意 X/n/b/z 字节 → 恒真误报
for s in "\[Xnbz\]" "\[Gyzw\]" "\[Jfqs\]" "\[Ysxz\]" "\[Dwct\]" "\[Tlgd\]"; do
  if LC_ALL=C grep -aq -- "$s" "$SO"; then
    echo "ERROR: $SO 含别家标签 ${s}（libs_ggbh 被并行会话覆盖）" >&2
    exit 1
  fi
done

# ---- 同步到 gradle 实际取用的 jniLibs（共享路径，必须字节一致）----
SO_MD5="$(md5 -q "$SO")"
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -d "$d" ]] || continue
  cp -f "$SO" "$d/libstarry_magic.so"
done
for d in "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
         "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
         "$STARRY_SRC/app/libs/arm64-v8a"; do
  [[ -f "$d/libstarry_magic.so" ]] || continue
  m="$(md5 -q "$d/libstarry_magic.so")"
  [[ "$m" == "$SO_MD5" ]] || { echo "ERROR: $d 同步失败（$m != ${SO_MD5}）" >&2; exit 1; }
done

echo "[build_so] done → $SO ($SO_MD5, $(stat -f%z "$SO") B)；jniLibs 已同步"
