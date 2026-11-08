#!/usr/bin/env bash
# 动物大联盟 1.0.25 — 私有「干净安装」wrapper（本轮唯一装机入口）
#
# 授权范围（2026-11-05 用户当轮明确选中【允许干净安装，接受重新下载资源】）：
#   允许在真机 f9a116a2 上：卸载 com.fightingstars.games.animal 官方原版 →
#   删除**仅本包**的外置残留目录（白名单两条）→ 安装本候选 → 启动取证。
#   **不做**存档备份、不暂存/不归位外置资源（用户接受私有数据/登录态/本地存档丢失 + 资源重下）。
#
# 为什么不用共享 install_starry_apk.sh --clean：
#   共享 --clean 默认走「mv 外置目录到 .nxreserve 备胎 → 装后归位 + chown」= **保资源**路径；
#   --allow-resource-loss 只是备胎失败时的放行例外。本轮授权是**从零资源**（不暂存、不归位），
#   套共享默认路径与授权语义相反 ⇒ 本游戏私有出这条 clean-install：
#   已授权卸载 → 仅残留目录清理（显式白名单、无通配符）→ 普通安装 → 装机核对。
#   **不改公共安装器、不改系统签名策略。**
#
# 用法：
#   bash 04_scripts/动物大联盟/clean_install_dwdlm.sh --plan           # 预检 + 打印计划（零写）
#   bash 04_scripts/动物大联盟/clean_install_dwdlm.sh --apply          # 卸载→清残留→安装→核对
#   bash 04_scripts/动物大联盟/clean_install_dwdlm.sh --precheck-only  # 只跑 L2 装机预检（冷启一次）
set -euo pipefail
export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin:${PATH:-}"
if [ -z "${HOME:-}" ]; then HOME="$(cd ~ 2>/dev/null && pwd || true)"; fi
export HOME

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="动物大联盟"
PKG="com.fightingstars.games.animal"
PROFILE="${DWDLM_SO_PROFILE:-env}"
# 单变量对照轮（2026-11-05）：factory=挂 appComponentFactory；provider-only=不挂（-provideronly 候选，
# 独立报告/独立台账，不覆盖 factory 模式证据）。默认 factory ⇒ 旧行为完全不变。
FACTORY_MODE="${DWDLM_FACTORY_MODE:-factory}"
MODE_SUFFIX=""
PROFILE_TAG="$PROFILE"
if [ "$FACTORY_MODE" = "provider-only" ]; then MODE_SUFFIX="_provideronly"; PROFILE_TAG="${PROFILE}-provideronly"; fi
APK="${DWDLM_APK:-$NIXIANG/08_output/$GAME/${GAME}_1.0.25_starry-${PROFILE_TAG}-releaseSigned.apk}"
BASE="$NIXIANG/02_apks/$GAME/${GAME}_1.0.25_official.apk"
SO_LOCAL="$NIXIANG/mod菜单/starry-mod-menu/libs_dwdlm/${PROFILE}/arm64-v8a/libstarry_magic.so"
REMOTE="/data/local/tmp/dwdlm_clean_install.apk"
JOURNAL="$SCRIPT_DIR/clean_install.json"
TXTLOG="$SCRIPT_DIR/clean_install.txt"
STEPS="$(mktemp /tmp/dwdlm_steps.XXXXXX)"
META="$(mktemp /tmp/dwdlm_meta.XXXXXX)"
TMPD="$(mktemp -d /tmp/dwdlm_clean.XXXXXX)"
trap 'rm -rf "$TMPD" "$STEPS" "$META"' EXIT

# ---- 期望值（用户任务书 + 构建/门禁/核验三方记录，运行时逐项交叉核对）----
if [ "$FACTORY_MODE" = "provider-only" ]; then
  # provider-only 对照候选（v2）：与 v1 只差 Manifest + 签名条目，size 相同
  EXPECTED_APK_SHA="9fd9fc0fdc4c4805d2b1e3bf34b1064bb40a84d72a7b54674e0976741647d5ad"
  EXPECTED_APK_SIZE="85402750"
else
# 2026-11-08：本轮 sigspoof 复刻候选（libsecret 签名路径开关 0x660cd 1→0）取代第五轮同名候选
EXPECTED_APK_SHA="${DWDLM_EXPECT_SHA:-328a826230ebf629108ae5eafc6dbe65c8381f128d45fd4a532a7a0e30e74662}"
EXPECTED_APK_SIZE="${DWDLM_EXPECT_SIZE:-85402750}"
fi
EXPECTED_OFFICIAL_SHA="2c9e85451b41b3bcb38148e488e28c85bcfba005db9c37743c926c0363c4f539"
EXPECTED_SOURCE_FP="${DWDLM_EXPECT_SOURCE_FP:-65fa4e79173e787c7405be6b8d6767e92703a4eecf3af14deaed4e0605f892b0}"
EXPECTED_CERT_SHA="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"
EXPECTED_OFFICIAL_CERT_SHA="de8a75d30a1831f3ddc83f38f285513110fde9ab919fb4e471653f86b5083e0a"
EXPECTED_APK_SO_MD5="${DWDLM_EXPECT_SO_MD5:-3afdae19bb56e5500e58e5aabdc6793b}"

MODE=""
SERIAL=""
while [ $# -gt 0 ]; do
  case "$1" in
    --plan)          MODE="plan"; shift ;;
    --apply)         MODE="apply"; shift ;;
    --precheck-only) MODE="precheck"; shift ;;
    # 2026-11-05 第五轮：同签覆盖安装（不卸载、不清外置目录、不丢资源）
    # 目的：把"本次唯一变量＝加签名替换"落实到设备上——资源/私有数据/外置目录全部保持，
    #       只换 base.apk 的 dex。卸载会让首次冷启变成"从零资源"，混淆 S1 判定。
    --overwrite)     MODE="overwrite"; shift ;;
    --serial)        SERIAL="${2:-}"; shift 2 ;;
    --apk)           APK="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,24p' "$0"; exit 0 ;;
    *) echo "ERROR: 未知参数 $1" >&2; exit 2 ;;
  esac
done
[ -n "$MODE" ] || { echo "ERROR: 必须显式指定 --plan / --apply / --precheck-only" >&2; exit 2; }
[ -n "$SERIAL" ] || SERIAL="$(cat "$SCRIPT_DIR/.device_serial" 2>/dev/null || true)"
[ -n "$SERIAL" ] || { echo "ERROR: 无 serial" >&2; exit 1; }

# L2 预检用独立产物，避免覆盖 install 阶段产物（否则上游阶段产物指纹变化会被判过期）
case "$MODE" in
  precheck) JOURNAL="$SCRIPT_DIR/precheck_l2${MODE_SUFFIX}.json"; TXTLOG="$SCRIPT_DIR/precheck_l2${MODE_SUFFIX}.txt" ;;
  overwrite) JOURNAL="$SCRIPT_DIR/clean_install_overwrite${MODE_SUFFIX}.json"; TXTLOG="$SCRIPT_DIR/clean_install_overwrite${MODE_SUFFIX}.txt" ;;
esac

say()  { printf '%s\n' "$*"; }
note() { printf '%s\n' "$*" >> "$TXTLOG"; }
adbs()  { adb -s "$SERIAL" shell "$@" 2>/dev/null | tr -d '\r' || true; }
adbsu() { adb -s "$SERIAL" shell "su -c '$1'" 2>/dev/null | tr -d '\r'; }
md5f()  { if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi; }
shaf()  { shasum -a 256 "$1" | awk '{print $1}'; }

# ---- 外置目录白名单（唯一允许删除的两条路径；其余一律拒绝）----
EXT_DATA="/storage/emulated/0/Android/data/$PKG"
EXT_OBB="/storage/emulated/0/Android/obb/$PKG"
check_whitelist() {
  _p="$1"
  case "$_p" in
    "$EXT_DATA"|"$EXT_OBB") ;;
    *) die "拒绝：路径不在白名单内 -> $_p" ;;
  esac
  case "$_p" in
    *'*'*|*'?'*|*'..'*) die "拒绝：路径含通配符或上跳 -> $_p" ;;
  esac
  [ "$(basename "$_p")" = "$PKG" ] || die "拒绝：路径末段不是本包名 -> $_p"
  case "$_p" in
    /storage/emulated/0/Android/data/*|/storage/emulated/0/Android/obb/*) ;;
    *) die "拒绝：不在 Android/data|obb 下 -> $_p" ;;
  esac
  return 0
}

finish() {
  _rc="${1:-0}"
  # 记录修正（2026-11-05 接手轮）：--plan 是零写模式，不得覆盖 clean_install.json/txt 既有证据
  if [ "${MODE:-}" = "plan" ]; then
    say "== 计划预览（零写）：不写 journal、不改证据文件 =="
    exit "$_rc"
  fi
  {
    printf 'game=%s\n' "$GAME"
    printf 'mode=%s\n' "$MODE"
    printf 'serial=%s\n' "$SERIAL"
    printf 'apk=%s\n' "$APK"
    printf 'apk_sha256=%s\n' "${APK_SHA:-}"
    printf 'apk_size=%s\n' "${APK_SIZE:-}"
    printf 'official_apk=%s\n' "$BASE"
    printf 'official_sha256=%s\n' "${OFF_SHA:-}"
    printf 'source_fingerprint=%s\n' "${SRC_FP:-}"
    printf 'cert_sha256=%s\n' "${CERT_SHA:-}"
    printf 'device_model=%s\n' "${DEV_MODEL:-}"
    printf 'device_android=%s\n' "${DEV_REL:-}"
    printf 'device_sdk=%s\n' "${DEV_SDK:-}"
    printf 'installed_before=%s\n' "${INST_BEFORE:-}"
    printf 'installed_before_sha256=%s\n' "${INST_BEFORE_SHA:-}"
    printf 'ext_data_before=%s\n' "${ED_BEFORE:-}"
    printf 'ext_obb_before=%s\n' "${EO_BEFORE:-}"
    printf 'ext_data_after=%s\n' "${ED_AFTER:-}"
    printf 'ext_obb_after=%s\n' "${EO_AFTER:-}"
    printf 'verify_installed=%s\n' "${VERIFY_OK:-not-run}"
    printf 'precheck_rc=%s\n' "${PRECHECK_RC:-not-run}"
    printf 'result=%s\n' "${RESULT:-UNKNOWN}"
  } > "$META"
  python3 - "$STEPS" "$META" "$JOURNAL" <<'PY'
import json, sys, time
steps_f, meta_f, out = sys.argv[1], sys.argv[2], sys.argv[3]
steps = []
for line in open(steps_f, encoding="utf-8"):
    parts = line.rstrip("\n").split("\t")
    if len(parts) == 3:
        steps.append({"step": parts[0], "rc": parts[1], "detail": parts[2]})
meta = {}
for line in open(meta_f, encoding="utf-8"):
    if "=" in line:
        k, v = line.rstrip("\n").split("=", 1)
        meta[k] = v
data = {"game": "动物大联盟", "mode": meta.get("mode"), "serial": meta.get("serial"),
        "authorization": "2026-11-05 用户选中【允许干净安装，接受重新下载资源】",
        "result": meta.get("result"), "steps": steps, "facts": meta,
        "journal_at": time.strftime("%Y-%m-%d %H:%M:%S")}
json.dump(data, open(out, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
print("[journal]", out, "result=" + str(meta.get("result")))
PY
  say "== 结果：${RESULT:-UNKNOWN} =="
  exit "$_rc"
}

die()  { printf 'ERROR: %s\n' "$*" >&2; printf 'ERROR\t%s\t%s\n' "${STEP:-?}" "$*" >> "$STEPS"; RESULT="FAIL"; finish 1; }
step_rc() { printf '%s\t%s\t%s\n' "$1" "$2" "$3" >> "$STEPS"; note "[step] $1 rc=$2 $3"; }

: > "$TXTLOG"
say "== 动物大联盟 clean-install mode=$MODE serial=$SERIAL =="
note "== 动物大联盟 clean-install mode=$MODE serial=$SERIAL =="

# =========== P1 本地预检（候选 / 官方 / 源指纹 / 签名 / 包内 so）===========
STEP="P1_preflight_local"
[ -f "$APK" ]  || die "缺候选包: $APK"
[ -r "$APK" ]  || die "候选包不可读: $APK"
[ -f "$BASE" ] || die "缺官方保全输入: $BASE"
APK_SHA="$(shaf "$APK")"
APK_SIZE="$(stat -f %z "$APK" 2>/dev/null || stat -c %s "$APK")"
OFF_SHA="$(shaf "$BASE")"
say "[P1] 候选: $(basename "$APK") size=$APK_SIZE sha256=$APK_SHA"
say "[P1] 官方: $(basename "$BASE") sha256=$OFF_SHA"
[ "$APK_SHA" = "$EXPECTED_APK_SHA" ] || die "候选 sha256 漂移：$APK_SHA != $EXPECTED_APK_SHA"
[ "$APK_SIZE" = "$EXPECTED_APK_SIZE" ] || die "候选 size 漂移：$APK_SIZE != $EXPECTED_APK_SIZE"
[ "$OFF_SHA" = "$EXPECTED_OFFICIAL_SHA" ] || die "官方保全输入 sha256 漂移：$OFF_SHA"

# 2026-11-05 第五轮：DWDLM_SIGSPOOF=1 时三方记录名为 *_sigspoof.*（同名不同轮会互相覆盖证据）
SIGSPOOF_SUFFIX=""
if [ "${DWDLM_SIGSPOOF:-0}" = "1" ]; then SIGSPOOF_SUFFIX="_sigspoof"; fi
for pair in "assemble_env${MODE_SUFFIX}${SIGSPOOF_SUFFIX}.json:signed_sha256" "artifact_verify_env${MODE_SUFFIX}${SIGSPOOF_SUFFIX}.json:apk_sha256" "gate_env${MODE_SUFFIX}${SIGSPOOF_SUFFIX}.json:apk_sha256"; do
  f="${pair%%:*}"; k="${pair##*:}"
  v="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1])).get(sys.argv[2],""))' "$SCRIPT_DIR/$f" "$k" 2>/dev/null || true)"
  [ "$v" = "$EXPECTED_APK_SHA" ] || die "$f.$k 与候选不一致：${v:-<缺失>}"
done
say "[P1] assemble / artifact_verify / gate 三份记录与候选 sha 一致"

SRC_FP="$(python3 "$SCRIPT_DIR/source_fingerprint_dwdlm.py" --json 2>/dev/null | python3 -c 'import json,sys;print(json.load(sys.stdin)["source_fingerprint"])' 2>/dev/null || true)"
[ "$SRC_FP" = "$EXPECTED_SOURCE_FP" ] || die "源指纹漂移：${SRC_FP:-<读不到>} != $EXPECTED_SOURCE_FP"
SB_FP="$(python3 -c 'import json,sys;print(json.load(open(sys.argv[1])).get("source_fingerprint",""))' "$SCRIPT_DIR/so_build_env.json" 2>/dev/null || true)"
[ "$SB_FP" = "$SRC_FP" ] || die "so_build_env 源指纹与现算不一致：${SB_FP:-<缺失>}"
say "[P1] 源指纹 ${SRC_FP}（现算 = so_build_env）"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[ -n "$BT" ] || die "找不到 apksigner（build-tools），无法验签"
"$BT/apksigner" verify --print-certs "$APK" > "$TMPD/signer.txt" 2>&1 || die "候选 apksigner verify 失败"
CERT_SHA="$(sed -n 's/.*SHA-256 digest: *//p' "$TMPD/signer.txt" | head -1 | tr 'A-F' 'a-f')"
DN="$(sed -n 's/.*certificate DN: *//p' "$TMPD/signer.txt" | head -1)"
[ "$CERT_SHA" = "$EXPECTED_CERT_SHA" ] || die "候选证书 sha256 不符：${CERT_SHA:-<空>}"
case "$DN" in *StarryMagic*) ;; *) die "候选证书 DN 非 StarryMagic：$DN" ;; esac
say "[P1] 候选签名 CN=StarryMagic cert_sha256=${CERT_SHA}（官方 $EXPECTED_OFFICIAL_CERT_SHA ⇒ 异签）"

unzip -p "$APK" lib/arm64-v8a/libstarry_magic.so > "$TMPD/apk_so.so" 2>/dev/null || die "包内缺 libstarry_magic.so"
APK_SO_MD5="$(md5f "$TMPD/apk_so.so")"
[ "$APK_SO_MD5" = "$EXPECTED_APK_SO_MD5" ] || die "包内 so md5 不符：$APK_SO_MD5"
if [ -f "$SO_LOCAL" ]; then
  [ "$(md5f "$SO_LOCAL")" = "$APK_SO_MD5" ] || die "本地构建 so 与包内 so 不一致（旧 so 混包）"
  say "[P1] 包内 so md5 = 本地构建 so md5 = $APK_SO_MD5"
else
  say "[P1] 本地 so 缺失，仅核包内 so md5 = $APK_SO_MD5"
fi
step_rc "P1_preflight_local" 0 "apk+official+srcfp+cert+so 全一致"

# =========== P2 设备只读预检 ============
STEP="P2_preflight_device"
command -v adb >/dev/null 2>&1 || die "adb 不可用"
ST="$(adb -s "$SERIAL" get-state 2>/dev/null | tr -d '\r' || true)"
[ "$ST" = "device" ] || die "设备 $SERIAL 不在线（get-state=${ST}）"
ROOT_ID="$(adbsu id)"
case "$ROOT_ID" in *uid=0*) ;; *) die "root 不可用（su id=${ROOT_ID}）" ;; esac
DEV_MODEL="$(adbs getprop ro.product.model)"
DEV_REL="$(adbs getprop ro.build.version.release)"
DEV_SDK="$(adbs getprop ro.build.version.sdk)"
say "[P2] 设备: $DEV_MODEL Android $DEV_REL (SDK $DEV_SDK) root=ok serial=$SERIAL"
FREE_KB="$(adbs "df -k /data" | awk 'NR==2{print $4}')"
[ -n "$FREE_KB" ] || die "读不到 /data 剩余空间"
[ "$FREE_KB" -gt 307200 ] || die "/data 剩余不足（${FREE_KB}KB < 300MB）"
say "[P2] /data 剩余 ${FREE_KB}KB"

PMP="$(adbs "pm path $PKG" || true)"
INST_BEFORE="absent"; INST_BEFORE_SHA=""
if [ -n "$PMP" ]; then
  INST_BEFORE="present"
  BASE_INST="$(printf '%s\n' "$PMP" | sed -n 's/^package://p' | grep 'base.apk$' | head -1)"
  say "[P2] 现装 base.apk: $BASE_INST"
  INST_BEFORE_SHA="$(adbsu "sha256sum $BASE_INST" 2>/dev/null | awk '{print $1}' || true)"
  say "[P2] 现装 base.apk sha256=${INST_BEFORE_SHA}（官方期望 ${EXPECTED_OFFICIAL_SHA}）"
  if [ "$MODE" = "apply" ]; then
    [ "$INST_BEFORE_SHA" = "$EXPECTED_OFFICIAL_SHA" ] || die "现装包不是官方原版（sha 不符）=> 停止，不卸载"
    say "[P2] 现装 = 官方原版 ⇒ 符合干净安装前置"
  elif [ "$MODE" = "overwrite" ] || [ "$MODE" = "plan" ]; then
    # 同签覆盖：现装应为本项目的旧候选（同 starry 证书），SHA 与官方/新候选都可能不同 ⇒ 只记录不拦
    say "[P2] overwrite 模式：现装 sha=${INST_BEFORE_SHA}（不卸载；同签覆盖，资源与私有数据保持）"
  else
    [ "$INST_BEFORE_SHA" = "$EXPECTED_APK_SHA" ] || die "precheck 模式下现装包不是本候选（sha=${INST_BEFORE_SHA}）"
    say "[P2] 现装 = 本候选（sha 与本地一致）⇒ 可做 L2 预检"
  fi
  CERTLINE="$(adbs "dumpsys package $PKG" | grep -m1 -i 'signatures' | tr -d '\r' || true)"
  say "[P2] 现装签名行: $CERTLINE"
fi
ED_BEFORE="absent:0"; EO_BEFORE="absent:0"
if [ -n "$(adbsu "ls -d $EXT_DATA 2>/dev/null")" ]; then
  ED_BEFORE="present:$(adbsu "du -sk $EXT_DATA" 2>/dev/null | awk '{print $1}' || true)"
fi
if [ -n "$(adbsu "ls -d $EXT_OBB 2>/dev/null")" ]; then
  EO_BEFORE="present:$(adbsu "du -sk $EXT_OBB" 2>/dev/null | awk '{print $1}' || true)"
fi
say "[P2] 外置目录清理前: data=$ED_BEFORE obb=$EO_BEFORE"
step_rc "P2_preflight_device" 0 "online+root+space+official-installed+ext-before"

RESULT="PRECHECK_ONLY"
if [ "$MODE" = "plan" ]; then
  say ""
  say "== 计划（--plan 零写，不执行下列写入动作）=="
  say "  1) adb -s $SERIAL shell pm uninstall $PKG                     # 系统卸载（连带清本包私有数据）"
  say "  2) su -c 'rm -rf $EXT_DATA'   （仅当仍存在的残留；白名单 1/2）"
  say "  3) su -c 'rm -rf $EXT_OBB'    （仅当仍存在的残留；白名单 2/2）"
  say "  4) adb push $(basename "$APK") $REMOTE && su -c 'pm install --user 0 $REMOTE'"
  say "  5) _common/verify_installed_apk.sh $SERIAL $PKG <apk> --su"
  say "  6) 明确不做：存档备份 / 资源暂存归位(.nxreserve) / 其他包 / 通配符"
  say "  注：以上 1)2)3) 是 --apply（干净安装）路线。本轮的 --overwrite 路线**跳过 1)2)3)**，"
  say "      直接 4) 用 'pm install -r -d --user 0' 同签覆盖（不丢资源/私有数据），其余相同。"
  say "== 预检全过：可 apply（本轮回执 = --overwrite）=="
  finish 0
fi

if [ "$MODE" = "precheck" ]; then
  STEP="P7_precheck_l2"
  set +e
  bash "$NIXIANG/04_scripts/_common/post_install_precheck.sh" --game "$GAME" --serial "$SERIAL" --pkg "$PKG"
  PRECHECK_RC=$?
  set -e
  say "[P7] post_install_precheck rc=$PRECHECK_RC"
  step_rc "P7_precheck_l2" "$PRECHECK_RC" "post_install_precheck --game $GAME"
  RESULT="PRECHECK_RC=$PRECHECK_RC"
  # 记录错误修复（2026-11-05 接手轮）：L2 门禁 rc≠0 必须让管线看到失败，不再 finish 0 记成 done
  finish "$PRECHECK_RC"
fi

# =========== P3/P4 在 overwrite 模式下整体跳过 ===========
if [ "$MODE" = "overwrite" ]; then
  say ""
  say "== P3/P4 跳过（overwrite：不卸载、不清外置目录；同签覆盖安装）=="
  say "[P3] 保留现装包与它的私有数据（下一个动作直接覆盖安装）"
  say "[P4] 保留 Android/data 与 Android/obb（资源不重下载，保证本次单变量）"
  step_rc "P3_uninstall" 0 "skipped(overwrite)"
  step_rc "P4_residual_clean" 0 "skipped(overwrite)"
  ED_AFTER="$ED_BEFORE"; EO_AFTER="$EO_BEFORE"
else
# =========== P3 卸载（已授权）===========
STEP="P3_uninstall"
say ""
say "== P3 卸载（用户已授权：接受私有数据/登录态/本地存档丢失）=="
if [ -z "$PMP" ]; then
  say "[P3] 预检时包已不在（本轮前序尝试已卸载）⇒ 跳过卸载，直接进入残留清理"
  UN_OUT="already-absent"
  UN_RC=0
else
UN_OUT="$(adbs "pm uninstall $PKG" || true)"
UN_RC=0
case "$UN_OUT" in *Success*) UN_RC=0 ;; *) UN_RC=1 ;; esac
if [ "$UN_RC" -ne 0 ]; then
  say "[P3] shell pm uninstall 未成功（${UN_OUT}），改用 su"
  UN_OUT="$(adbsu "pm uninstall $PKG" || true)"
  case "$UN_OUT" in *Success*) UN_RC=0 ;; *) UN_RC=1 ;; esac
fi
say "[P3] uninstall 输出: $UN_OUT"
step_rc "P3_uninstall" "$UN_RC" "$UN_OUT"
[ "$UN_RC" -eq 0 ] || die "卸载失败，停止后续破坏性步骤"
PMP2="$(adbs "pm path $PKG" || true)"
[ -z "$PMP2" ] || die "卸载后 pm path 仍有返回（${PMP2}）"
say "[P3] pm path 已空 ⇒ 卸载确认"
fi

# =========== P4 仅残留目录清理（显式白名单，无通配符）===========
STEP="P4_residual_clean"
say ""
say "== P4 残留目录清理（白名单逐条，删前打印解析路径）=="
clean_one() {
  _dir="$1"; _tag="$2"
  check_whitelist "$_dir"
  _resolved="$(adbsu "readlink -f $_dir 2>/dev/null" || true)"
  say "[P4] $_tag 白名单路径: $_dir"
  say "[P4] $_tag 解析后绝对路径: ${_resolved:-<不存在>}"
  if [ -z "$_resolved" ]; then
    say "[P4] $_tag 不存在 ⇒ 无需删除"
    step_rc "P4_$_tag" 0 "absent"
    return 0
  fi
  if [ "$_resolved" != "$_dir" ]; then
    die "$_tag 解析路径与白名单不一致（$_resolved != ${_dir}）=> 拒绝删除"
  fi
  _kb="$(adbsu "du -sk $_dir 2>/dev/null" 2>/dev/null | awk '{print $1}' || true)"
  say "[P4] $_tag 删除前: present ${_kb}KB"
  adbsu "rm -rf $_dir" || true
  if [ -n "$(adbsu "ls -d $_dir 2>/dev/null")" ]; then
    step_rc "P4_$_tag" 1 "still exists after rm"
    die "$_tag 删除后仍存在 => 停止（不安装到不确定状态）"
  fi
  step_rc "P4_$_tag" 0 "removed ${_kb}KB"
  say "[P4] $_tag 已删除（${_kb}KB）"
  return 0
}
clean_one "$EXT_DATA" "ext_data"
clean_one "$EXT_OBB" "ext_obb"
if [ -n "$(adbsu "ls -d $EXT_DATA 2>/dev/null")" ]; then ED_AFTER="present"; else ED_AFTER="absent:0"; fi
if [ -n "$(adbsu "ls -d $EXT_OBB 2>/dev/null")" ]; then EO_AFTER="present"; else EO_AFTER="absent:0"; fi
say "[P4] 清理后: data=$ED_AFTER obb=$EO_AFTER"
OTHER="$(adbsu "ls /storage/emulated/0/Android/data 2>/dev/null" 2>/dev/null | grep -v -x "$PKG" | wc -l | tr -d ' ' || true)"
say "[P4] 反扫：Android/data 下其他包目录 $OTHER 个（未触碰；本包 nxreserve 备份未创建）"

fi
# =========== P5 安装 ===========
STEP="P5_install"
say ""
say "== P5 安装唯一候选 =="
adb -s "$SERIAL" push "$APK" "$REMOTE" > "$TMPD/push.txt" 2>&1 || die "push 失败（见 $TMPD/push.txt）"
PUSH_SHA="$(adbsu "sha256sum $REMOTE" 2>/dev/null | awk '{print $1}' || true)"
say "[P5] 设备侧临时包 sha256=$PUSH_SHA"
if [ "$PUSH_SHA" != "$APK_SHA" ]; then
  adbsu "rm -f $REMOTE" || true
  step_rc "P5_install" 1 "push sha mismatch"
  die "推送包 sha 与本地不一致，已删临时包，停止"
fi
set +e
if [ "$MODE" = "overwrite" ]; then
  # -r = 覆盖已装同签包（不卸载）；-d 允许同 versionCode 覆盖重装
  INS_OUT="$(adbsu "pm install -r -d --user 0 $REMOTE" || true)"
  if [ -z "$INS_OUT" ] || [ "${INS_OUT#*Success}" = "$INS_OUT" ]; then
    say "[P5] su 安装未成功（${INS_OUT}），改用 shell pm install -r"
    INS_OUT="$(adbs "pm install -r -d $REMOTE" || true)"
  fi
else
  INS_OUT="$(adbsu "pm install --user 0 $REMOTE" || true)"
fi
set -e
say "[P5] pm install 输出: $INS_OUT"
adbsu "rm -f $REMOTE" || true
case "$INS_OUT" in
  *Success*) step_rc "P5_install" 0 "Success" ;;
  *) step_rc "P5_install" 1 "$INS_OUT"; die "pm install 失败（未重试；先检视已完成效果）" ;;
esac

# =========== P6 装机核对（包 + so 与本地 hash 一致）===========
STEP="P6_verify_installed"
say ""
say "== P6 装机核对 =="
set +e
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
VRC=$?
set -e
if [ "$VRC" -ne 0 ]; then
  VERIFY_OK="FAIL(rc=$VRC)"
  step_rc "P6_verify_installed" "$VRC" "verify_installed_apk FAIL"
  RESULT="INSTALL_VERIFY_FAIL"
  die "装机核对失败（设备包/so 与本地不一致）"
fi
VERIFY_OK="PASS"
step_rc "P6_verify_installed" 0 "base.apk+so md5 == 本地"
say "[P6] 装机核对 PASS（设备 base.apk 与包内 so 均与本地一致）"
RESULT="INSTALLED_VERIFIED_NOT_LAUNCHED"
finish 0