#!/usr/bin/env bash
# 失落城堡 管线 build_so 阶段后端：私有 Android_slcb.mk 直编（modern 注册表模式）
# 产物 = mod菜单/starry-mod-menu/libs_slcb/arm64-v8a/libstarry_magic.so
# 环境阶段口径：kSlots = 空占位槽 ⇒ 0 hook（只验注入与菜单落地）
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
OUT_LIBS="$STARRY_SRC/libs_slcb"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"

[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$JNI/Android_slcb.mk" ]] || { echo "ERROR: missing $JNI/Android_slcb.mk" >&2; exit 1; }

OBJ="$STARRY_SRC/obj_slcb"
STAMP="$OBJ/.build_stamp"
# ---- obj stamp：mk 内容 / 探针 / trace 开关变化 → 清 obj 强制全量重编 ----
# 背景（2026-09-10 造梦无双教训）：make 只看文件 mtime，不追踪 cflags——
# 探针版构建后去掉开关重编，旧 .o（含探针代码）被原样复用泄漏进"干净版"。
CFG_HASH="$( (cat "$JNI/Android_slcb.mk" "$JNI/Application.mk"; \
  printf 'PROBE=%s\nTRACE=%s\nDIAG=%s\n' "${STARRY_SLCB_PROBE:-}" "${STARRY_TRACE:-}" "${STARRY_DIAG:-}") \
  | shasum -a 256 | awk '{print $1}')"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] 构建配置变化（mk/开关）→ 清 obj_slcb 强制全量重编"
  rm -rf "$OBJ"
fi

# 独立 obj_slcb（并行会话互踩防护）；切游戏无需清共享 app/obj
# NDK_OUT/NDK_LIBS_OUT 用绝对路径：相对路径会被 -C 的 cwd 解析到 app/ 下（2026-09-10 假成功教训）
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_slcb.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: ndk-build rc=$BUILD_RC" >&2; exit 1; }

[[ -f "$SO" ]] || { echo "ERROR: build produced no $SO" >&2; exit 1; }

# ---- 链接活动保险：本次确有编译活动（新 .o）但 so 字节没变 → 假成功嫌疑 ----
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: 本次构建产生了新 .o 但 $SO 未变化——旧 so 顶包嫌疑，禁止通过" >&2
  exit 1
fi
mkdir -p "$OBJ" && echo "$CFG_HASH" > "$STAMP"

# ---- 防呆：so 必须含本游戏标签与菜单串（旧 so 混装假成功教训 2026-09-10）----
# 注意：中文是 UTF-8 多字节，strings 默认 ASCII 会漏报——必须 LC_ALL=C grep -a 按字节搜
check_str() {  # check_str <字节串> <说明>
  local n
  n=$(LC_ALL=C grep -ac -- "$1" "$SO" || true)
  [[ "$n" -ge 1 ]] || { echo "ERROR: $SO 不含 $2（旧 so 混入或源码未编译）" >&2; exit 1; }
}
check_str "Slcb"              "游戏标签 Slcb"
check_str "Toggle_无敌"       "菜单串 Toggle_无敌"
check_str "Toggle_无限技能"   "菜单串 Toggle_无限技能"
check_str "Slider_伤害倍率"   "菜单串 Slider_伤害倍率"
check_str "Toggle_无限金币"   "菜单串 Toggle_无限金币"
check_str "Toggle_无限魂石"   "菜单串 Toggle_无限魂石"
check_str "Toggle_免广告"     "菜单串 Toggle_免广告"
check_str "IsAdReady"         "hook 点 免广告·可点判定"
check_str "feature stage"     "功能阶段构建标记"
check_str "SetIsCanBeAttacked" "hook 点 无敌"
check_str "calPureDamange"     "hook 点 伤害倍率"
check_str "UpdateCoolingTime"  "hook 点 无限技能"
check_str "set_m_money"        "hook 点 无限金币"
check_str "set_m_soul"         "hook 点 无限魂石"
check_str "UseSkill"          "hook 点 无限技能·UseSkill"
check_str "SkillInfo"          "hook 点 无限技能·SkillInfo"
check_str "skl.capture"        "hook 诊断串"
check_str "OnTodayAdFinished"   "hook 点 免广告·完成回调名"

echo "[build_so] done → $SO"
md5 -q "$SO"
