#!/usr/bin/env bash
# 潮汐守望者（com.gf.cxswz）管线 build_so 阶段后端：私有 Android_cxswz.mk 直编
# 产物 = mod菜单/starry-mod-menu/libs_cxswz/arm64-v8a/libstarry_magic.so
#
# 环境阶段（2026-09-30）：kSlots 空表 = 0 hook，仅壳 + 菜单 + 去签名 + 强制网页登录。
# 探针阶段（2026-09-30）：只挂 HpChangeEntitySystem.ExcuteOnce 只读探针（不改内存）。
# 功能阶段（2026-09-30）：同一挂点改 hData.fValue —— 无敌（我方承伤清零）
#   / 伤害倍率（对敌伤害乘倍率，1 = 关闭）；开关默认全关，关时行为等同只读探针。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_cxswz"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_cxswz.mk" ]] || { echo "ERROR: missing $JNI/Android_cxswz.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_cxswz"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / trace 开关变化 → 清 obj 强制全量重编 ----
CFG_HASH="$( (cat "$JNI/Android_cxswz.mk" "$JNI/Application.mk"; \
  printf 'TRACE=%s\n' "${STARRY_TRACE:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_cxswz 强制全量重编"
  rm -rf "$OBJ"
fi

# NDK_OUT/NDK_LIBS_OUT 必须绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（假成功教训）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_cxswz.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：有新 .o 但 so 字节没变 → 旧 so 顶包嫌疑 ----
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 归属防呆：本作串必须在、别家标签必须不在（跨会话共享目录）----
# 中文 UTF-8 多字节，strings 默认 ASCII 会漏报 ⇒ LC_ALL=C grep -a 按字节搜
for s in "Category_战斗" "Toggle_无敌" "Slider_伤害倍率" "[Cxswz]" "liblogic.so"; do
  n="$(LC_ALL=C grep -acF -- "$s" "$SO" || true)"
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含本作串 ${s}（旧 so 混入或源码未编译）" >&2; exit 1; }
done
for s in "[Jjdm]" "[Gyzw]" "[Hxwj]" "[Blfx]" "[Sjzz]" "[WbjY]"; do
  # -F：标签里的 [] 是字面字符，否则会被 grep 当字符类（曾假报含别家标签）
  if LC_ALL=C grep -aqF -- "$s" "$SO"; then
    echo "ERROR: $SO 含别家标签 ${s}（libs_cxswz 被并行会话覆盖）" >&2
    exit 1
  fi
done

# ---- 阶段标记自检：功能阶段必须含功能标记、不得残留环境阶段标记 ----
LC_ALL=C grep -aqF -- "feature stage" "$SO" \
  || { echo "ERROR: $SO 未见功能阶段标记（源码未切到功能阶段）" >&2; exit 1; }
LC_ALL=C grep -aqF -- "DMG x%d" "$SO" \
  || { echo "ERROR: $SO 未见伤害倍率改写日志（功能未编译进去）" >&2; exit 1; }
LC_ALL=C grep -aqF -- "INV e=%p" "$SO" \
  || { echo "ERROR: $SO 未见无敌改写日志（功能未编译进去）" >&2; exit 1; }
LC_ALL=C grep -aqF -- "cxswz-e10-" "$SO" \
  || { echo "ERROR: $SO 未见构建身份标记（CXSWZ_BUILD_TAG 未编入，需 cxswz-e10-）" >&2; exit 1; }
# ---- 运行时布局解析（1.2.65.385.1 起取代静态 RVA；缺任一 = 源码未切到 e5 路线）----
for s in "cxswz_tables.txt" "cxswz_cfg.txt" "layout: ok=%d how=%s" "S3 reconcile" \
         "SetTrustLib" "hotpath: layout not ok" "waiting il2cpp runtime" "runtime not up" \
         "timing layout:" "timing tick:" "timing hooks:" "diag asm(v5):" "shortscan img#" \
         "il2cpp_init hooked" "il2cpp_init 已返回" "sweep HIT" "首次看到程序集" \
         "ResolveExport %s -> %p" "等待初始化" "short index: images=" \
         "S5 class layout" "hpd inline fix" "D#%d hd=" "D#%d list="; do
  LC_ALL=C grep -aqF -- "$s" "$SO" \
    || { echo "ERROR: $SO 未见运行时布局解析标记 ${s}" >&2; exit 1; }
done
# 静态 RVA 路径必须已短路（rva=0）；旧序言常量只许作为 *DOC_ONLY* 件出现在符号表外
if LC_ALL=C grep -aqF -- "rva drift got=" "$SO"; then
  : # 字符串仍在（HookAtRva 保留），但 rva 传 0 ⇒ 不会走到；无需报错
fi
# 滑条范围必须与需求一致（1..10）；旧 1..100 不得残留
LC_ALL=C grep -aqF -- "Slider_伤害倍率:1:10:1" "$SO" \
  || { echo "ERROR: $SO 伤害倍率滑条不是 1:10（菜单串与需求不一致）" >&2; exit 1; }
if LC_ALL=C grep -aqF -- "Slider_伤害倍率:1:100:1" "$SO"; then
  echo "ERROR: $SO 仍含旧 1:100 滑条" >&2; exit 1
fi
if LC_ALL=C grep -aqF -- "env stage kSlots=0" "$SO"; then
  echo "ERROR: $SO 仍含环境阶段标记（源码与 so 不一致）" >&2; exit 1
fi

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
