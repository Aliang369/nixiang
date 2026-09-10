#!/usr/bin/env bash
# ============================================================
# new_game_mod.sh — 新游戏 Mod 生成器 v2（注册表模式，四件套产出）
#
# 用法:
#   bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh <Game> [日志标签]
# 例:
#   bash new_game_mod.sh MyGame
#   bash new_game_mod.sh MyGame MyTag
#
# 行为:
#   1. 复制 Games/_TemplateMod.h/.cpp   → Games/{Game}Mod.h/.cpp（游戏逻辑骨架）
#   2. 复制 Games/_TemplateEntry.cpp   → Games/{Game}Entry.cpp（注册表：菜单/参数/回调）
#   3. 复制 Android_modern_template.mk → Android_<tag小写>.mk（私有构建）
#   4. 自动替换: Template_ → {Game}_ / TEMPLATE_ → {GAME}_ /
#      TemplateFeat → {Game}Feat / [Tpl] → [{Tag}] / {GAME} 占位
#
# 接线（v2 起零接触共享文件）:
#   ✗ 不碰 Main.cpp / MainModern.cpp / GameConfig.h / 公共 Android.mk
#   ✓ 只填 {Game}Entry.cpp 的 TODO（game_id / waitLib / 菜单）
#   ✓ 只改 Android_<tag>.mk 差异区（源文件组合）
#   ✓ 构建传独立 obj（绝对路径）：NDK_OUT=<starry根>/obj_<tag>（并行会话互不踩；
#     相对路径会被 ndk-build -C 解析到 app/ 下 → 旧 so 顶包假成功，2026-09-10 事故）
#
# 铁律提醒: 生成后先读 skill/re-implement/hook-rules.md（§16 懒装 / §17 缺失降级），
# 只填 Mod.cpp 的 TODO 区；禁止从旧游戏 Mod 复制代码。
# ============================================================
set -euo pipefail

GAME="${1:?用法: new_game_mod.sh <Game> [日志标签]}"
TAG="${2:-${GAME}}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
JNI_DIR="${SCRIPT_DIR}/../app/src/main/jni"
GAMES_DIR="${JNI_DIR}/Games"
UPPER="$(echo "${GAME}" | tr '[:lower:]-' '[:upper:]_')"
LOWERTAG="$(echo "${TAG}" | tr '[:upper:]-' '[:lower:]_')"

MK_OUT="${JNI_DIR}/Android_${LOWERTAG}.mk"

sed_tpl() {  # sed_tpl <输入模板> <输出> [额外规则...]
  local in="$1"; local out="$2"; shift 2
  sed -e "s/Template_/${GAME}_/g" \
      -e "s/TEMPLATE_/${UPPER}_/g" \
      -e "s/TemplateFeat/${GAME}Feat/g" \
      -e "s/STARRY_TEMPLATE_MOD_H/STARRY_${UPPER}_MOD_H/g" \
      -e "s/\[Tpl\]/[${TAG}]/g" \
      -e "s/\"_TemplateMod\.h\"/\"${GAME}Mod.h\"/g" \
      "$@" "${in}" > "${out}"
}

for f in _TemplateMod.h _TemplateMod.cpp; do
  ext="${f##*.}"
  out="${GAMES_DIR}/${GAME}Mod.${ext}"
  if [ -e "${out}" ]; then
    echo "❌ 已存在: ${out}（换名或先删除）"
    exit 1
  fi
  sed_tpl "${GAMES_DIR}/${f}" "${out}"
  echo "✅ 生成 ${out}"
done

ENTRY_OUT="${GAMES_DIR}/${GAME}Entry.cpp"
if [ -e "${ENTRY_OUT}" ]; then
  echo "❌ 已存在: ${ENTRY_OUT}（换名或先删除）"
  exit 1
fi
sed_tpl "${GAMES_DIR}/_TemplateEntry.cpp" "${ENTRY_OUT}" \
  -e "s/\"Tpl\"/\"${TAG}\"/"
echo "✅ 生成 ${ENTRY_OUT}"

if [ -e "${MK_OUT}" ]; then
  echo "❌ 已存在: ${MK_OUT}（换名或先删除）"
  exit 1
fi
sed -e "s/{GAME}/${GAME}/g" \
    "${JNI_DIR}/Android_modern_template.mk" > "${MK_OUT}"
echo "✅ 生成 ${MK_OUT}"

STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
JNI_DIR_ABS="$(cd "${JNI_DIR}" && pwd)"

cat <<EOF

下一步接线（只碰新生成的三处，共享文件零接触）:
  1. ${GAME}Entry.cpp : TODO 三项
       authGameId（后台 games.game_id，拼音首字母）/ waitLib（目标库）/ featureList（菜单）
  2. ${GAME}Mod.cpp   : TODO 区（①开关/orig ②偏移与 RVA ⑤挂点表 ⑥功能组）
  3. Android_${LOWERTAG}.mk : 差异区（Cocos 系去掉 Unity/Il2CppApi.cpp；Trace 需要时加宏）

机械操作走管线（代码填完后注册）:
  注册 04_scripts/${GAME}/pipeline.json（按 _common/stages.md 阶段表），其中：
    - build_so 后端照范本 04_scripts/魂坠深境/hzsj_build_so.sh 写 ${LOWERTAG}_build_so.sh
      （绝对路径 / obj stamp / 链接活动保险 / 菜单串字节校验四件套，不要自己发明）
    - install 阶段末尾必须接 _common/verify_installed_apk.sh（设备包 md5 == 本地 APK）
    - surgical 脚本末尾加包内 so md5 == 构建产物校验

手工验证构建（仅调试用，正式出包走管线）:
  cd ${JNI_DIR_ABS} && ndk-build NDK_PROJECT_PATH=../.. \\
    APP_BUILD_SCRIPT=./Android_${LOWERTAG}.mk NDK_APPLICATION_MK=./Application.mk \\
    NDK_OUT=${STARRY_SRC}/obj_${LOWERTAG} NDK_LIBS_OUT=${STARRY_SRC}/libs_${LOWERTAG}
  （NDK_OUT/LIBS_OUT 必须绝对路径：相对路径会被 -C 解析到 app/ 下 → 旧 so 顶包假成功）

然后填 ${GAME}Mod.cpp 的 TODO 区:
  ① 开关/orig  ② 偏移与 RVA  ⑤ hook 函数与挂点表  ⑥ 功能组
构建前自检（按字节搜，strings 不认 UTF-8 中文菜单串）:
  LC_ALL=C grep -ac '${TAG}' ${STARRY_SRC}/libs_${LOWERTAG}/arm64-v8a/libstarry_magic.so
EOF
