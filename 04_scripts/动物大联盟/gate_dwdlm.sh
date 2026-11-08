#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 装机前产物门禁包装（pipeline gate_pre_install 阶段专用）
#
# 为什么要包装：pre_install_gate.sh 的查2（JNI 配套）要 --dex 指向**包内注入 dex 的实体文件**，
# 查9/查12 要 --apk/--base-apk，查10 要 --menu-dex/--menu-dex-name。少传任何一个都会变成 SKIP，
# 而 SKIP 不能当证据用。本脚本把参数一次配齐，只解一个临时 dex，跑完即清（不写仓库、不写产品）。
#
# 查10 说明：本作 dex 由 surgical 脚本在本次构建内现编（gradle→smali + javac+d8 工厂），不是模板产物、
# 不带 starry-menu 戳 ⇒ 传 REQUIRE_STAMP=0 走门禁自带的"无戳但现编"模式（输出 WARN，不是 SKIP）。
# 替代新鲜度证据 = assemble_<profile>.json 里记录的构建输入 sha（factory_injected_from / base_sha256）。
# 查4 说明：2026-11-05 第四轮起补 --menu-json 指向 10_docs/动物大联盟/dwdlm_menu_template.json
# （只含用户批准的六项，feat_num＝kDwdlm_Features 真实下标）。此前该参数未传 ⇒ 查4 从未执行。
# 注意：env profile 的 DwdlmMod.cpp 里没有 getVal/getRet 调用（六项取值在 JS 补丁 starry_dwdlm_v1
# 内完成，且 ENV_ONLY 编译期剔除）⇒ 查4 读到空集，PASS 属"空集通过"，不能当六项控件类型已核对。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="动物大联盟"
PKG="com.fightingstars.games.animal"
PROFILE="${DWDLM_SO_PROFILE:-env}"
# 单变量对照轮（2026-11-05）：factory=挂 appComponentFactory；provider-only=不挂（私有候选名 -provideronly）
FACTORY_MODE="${DWDLM_FACTORY_MODE:-factory}"
MODE_SUFFIX=""
TAG="${GAME}_1.0.25_starry-${PROFILE}"
if [[ "$FACTORY_MODE" == "provider-only" ]]; then MODE_SUFFIX="_provideronly"; TAG="${TAG}-provideronly"; fi
# 2026-11-05 第五轮：DWDLM_SIGSPOOF=1 ⇒ 成品名/门禁报告名带 -sigspoof / _sigspoof
SIGSPOOF_SUFFIX=""
if [[ "${DWDLM_SIGSPOOF:-0}" == "1" ]]; then SIGSPOOF_SUFFIX="_sigspoof"; TAG="${TAG}-sigspoof"; fi
APK="${DWDLM_APK:-$NIXIANG/08_output/$GAME/${TAG}-releaseSigned.apk}"
BASE="$NIXIANG/02_apks/$GAME/${GAME}_1.0.25_official.apk"
JNI="$NIXIANG/mod菜单/starry-mod-menu/app/src/main/jni"

say() { printf '%s
' "$*"; }
die() { printf 'ERROR: %s
' "$*" >&2; exit 1; }

[[ -f "$APK" ]]  || die "缺成品包: ${APK}"
[[ -f "$BASE" ]] || die "缺底包: ${BASE}"
[[ -f "$NIXIANG/04_scripts/_common/pre_install_gate.sh" ]] || die "缺 pre_install_gate.sh"

TMPD="$(mktemp -d "${TMPDIR:-/tmp}/dwdlm_gate.XXXXXX")"
cleanup() { rm -rf "$TMPD"; }
trap cleanup EXIT
unzip -p "$APK" classes2.dex > "$TMPD/menu-classes2.dex" 2>/dev/null || true
[[ -s "$TMPD/menu-classes2.dex" ]] || die "成品包没有 classes2.dex（注入 dex 丢了）"

say "[gate] apk      : $APK"
say "[gate] mode     : ${FACTORY_MODE}（provider-only ⇒ Manifest 不挂 appComponentFactory，属有意对照；dex 仍含工厂类，needle 照旧）"
say "[gate] base-apk : $BASE"
say "[gate] dex      : 包内 classes2.dex（临时解出 $(stat -f "%z" "$TMPD/menu-classes2.dex") B，跑完即删）"
say "[gate] cpp      : Games/DwdlmMod.cpp + Games/DwdlmSecShell.cpp"
say "[gate] menu-dex : classes2.dex（REQUIRE_STAMP=0：本 dex 本次现编，无模板戳；见文件头说明）"
say "[gate] menu-json: 10_docs/$GAME/dwdlm_menu_template.json（查4；六项，feat_num 1..6）"
say ""
GATE_LOG="$SCRIPT_DIR/gate_${PROFILE}${MODE_SUFFIX}${SIGSPOOF_SUFFIX}.log"
GATE_JSON="$SCRIPT_DIR/gate_${PROFILE}${MODE_SUFFIX}${SIGSPOOF_SUFFIX}.json"
set +e
REQUIRE_STAMP=0 bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
  --apk "$APK" \
  --base-apk "$BASE" \
  --cpp "$JNI/Games/DwdlmMod.cpp" \
  --cpp "$JNI/Games/DwdlmSecShell.cpp" \
  --dex "$TMPD/menu-classes2.dex" \
  --menu-json "$NIXIANG/10_docs/$GAME/dwdlm_menu_template.json" \
  --menu-dex "$APK" \
  --menu-dex-name classes2.dex \
  --needle "com/starry/magic/dwdlm/DwdlmAppComponentFactory" \
  --needle "nativeEnvStart" \
  --game "$GAME" 2>&1 | tee "$GATE_LOG"
GATE_RC=${PIPESTATUS[0]}
set -e
P_PASS="$(LC_ALL=C grep -c "^✓ PASS" "$GATE_LOG" || true)"
P_FAIL="$(LC_ALL=C grep -c "^✗ FAIL" "$GATE_LOG" || true)"
P_SKIP="$(LC_ALL=C grep -c "^- SKIP" "$GATE_LOG" || true)"
say ""
gate_json() {
  local verdict="$1"
  local apk_sha
  apk_sha="$(shasum -a 256 "$APK" | cut -d " " -f 1)"
  cat > "$GATE_JSON" <<JSON
{
  "game": "$GAME",
  "profile": "$PROFILE",
  "factory_mode": "$FACTORY_MODE",
  "apk": "$APK",
  "apk_sha256": "$apk_sha",
  "gate_rc": ${GATE_RC},
  "pass": ${P_PASS:-0},
  "fail": ${P_FAIL:-0},
  "skip": ${P_SKIP:-0},
  "verdict": "$verdict",
  "gate_log": "$GATE_LOG",
  "skip_note": "SKIP 不是证据：查10 走 REQUIRE_STAMP=0（本次现编 dex、无模板戳）；查9 对 Cocos 包无 il2cpp/metadata，按门禁自身判据 SKIP",
  "at": "$(date "+%Y-%m-%d %H:%M:%S")"
}
JSON
}

if [[ $GATE_RC -ne 0 ]]; then
  gate_json FAIL
  die "pre_install_gate FAIL（rc=${GATE_RC}）=> 禁止装机；逐项原因见 $GATE_LOG"
fi
gate_json PASS
say ""
say "[gate] pre_install_gate 全查通过（PASS=${P_PASS:-0} FAIL=0 SKIP=${P_SKIP:-0}；SKIP 项原因见 ${GATE_LOG}）"
say "[gate] 结果文件: $GATE_JSON"
