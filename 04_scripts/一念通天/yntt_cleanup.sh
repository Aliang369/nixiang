#!/usr/bin/env bash
# 一念通天 pipeline 阶段 cleanup —— 收官清理本作可再生中间物（版本无关）。
#
# 只删本作可再生中间物（03_decompiled / 07_patched / 06_dumped 的未跟踪二进制、
# /tmp/yntt_* 工作目录、设备端本次临时 APK）；**不动** 02_apks、08_output、09_test。
# 版本号取自 official_apk.json，锚点摘录路径随版本自动变化 —— 不再有硬编码版本常量。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=/dev/null
source "${SCRIPT_DIR}/yntt_common.sh"

FINAL_APK="${OUT_DIR}/${GAME}_${YNTT_VER}_surgical-releaseSigned.apk"
SUMMARY="${SCRIPT_DIR}/cleanup_summary.json"
SERIAL_FILE="${SCRIPT_DIR}/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
AAPT2="${AAPT2:-${HOME}/Library/Android/sdk/build-tools/36.0.0/aapt2}"

cd "${NIXIANG_ROOT}"
yntt_need_file "${FINAL_APK}"

# 正式包身份校验（替代旧版的硬编码 md5：换版本不再需要改脚本）
BADGING="$("${AAPT2}" dump badging "${FINAL_APK}" 2>/dev/null | sed -n '1p')"
case "${BADGING}" in
  *"package: name='${PKG}'"*"versionName='${YNTT_VER}'"*) ;;
  *) yntt_die "正式包身份不符（期望 ${PKG} ${YNTT_VER}）：${BADGING}" ;;
esac

# 06_dumped 整份 dump 不入库：以 git 已跟踪清单为准 + 本版锚点摘录显式保留
KEEP_LIST="$(mktemp)"
trap 'rm -f "${KEEP_LIST}"' EXIT
git ls-files "06_dumped/${GAME}" > "${KEEP_LIST}"

NEW_ANCHOR="06_dumped/${GAME}/official_${YNTT_VER}/rva_targets.json"
yntt_need_file "${NEW_ANCHOR}"
printf '%s\n' "${NEW_ANCHOR}" >> "${KEEP_LIST}"
git add -- "${NEW_ANCHOR}"

if git diff --cached --name-only -- "06_dumped/${GAME}" | grep -E '\.(apk|ap_|so|o|jar|bin|dat|dex|i64|idb)$' >/dev/null; then
  yntt_die "06_dumped 暂存区出现大二进制，拒绝清理"
fi

BEFORE_KB="$( (du -sk "03_decompiled/${GAME}" "06_dumped/${GAME}" "07_patched/${GAME}" 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
BEFORE_AT="$(date '+%Y-%m-%d %H:%M:%S')"

yntt_log "dry-run：本作可再生中间物"
git clean -xdn -- "03_decompiled/${GAME}" "06_dumped/${GAME}" "07_patched/${GAME}" | sed -n '1,240p'

git clean -xdf -- "03_decompiled/${GAME}"
git clean -xdf -- "07_patched/${GAME}"
git clean -xdf -- "06_dumped/${GAME}"

while IFS= read -r keep; do
  [ -n "${keep}" ] || continue
  yntt_need_file "${keep}"
done < "${KEEP_LIST}"

[ -d "02_apks/${GAME}" ] || yntt_die "02_apks/${GAME} 被误删"
[ -d "08_output/${GAME}" ] || yntt_die "08_output/${GAME} 被误删"
[ -d "09_test/${GAME}" ] || yntt_die "09_test/${GAME} 被误删"

find /tmp -maxdepth 1 -name 'yntt_surgical.*' -print -exec rm -rf {} + 2>/dev/null || true
find /tmp -maxdepth 1 -name 'yntt_sign.*' -print -exec rm -rf {} + 2>/dev/null || true
find /tmp -maxdepth 1 -name 'yntt_gate.*' -print -exec rm -rf {} + 2>/dev/null || true

SERIAL=""
if [ -f "${SERIAL_FILE}" ]; then
  SERIAL="$(tr -d '[:space:]' < "${SERIAL_FILE}")"
fi
if [ -n "${SERIAL}" ] && "${ADB_BIN}" -s "${SERIAL}" get-state >/dev/null 2>&1; then
  yntt_log "设备本次临时文件（只清明确路径）"
  "${ADB_BIN}" -s "${SERIAL}" shell rm -f "/data/local/tmp/yntt_surgical_${YNTT_VER}.apk" || true
  "${ADB_BIN}" -s "${SERIAL}" shell ls -1 /data/local/tmp 2>/dev/null | tr -d '\r' | grep -E '^yntt_' || true
else
  yntt_log "设备不在线或无 serial；跳过设备临时件清理"
fi

AFTER_KB="$( (du -sk "03_decompiled/${GAME}" "06_dumped/${GAME}" "07_patched/${GAME}" 2>/dev/null || true) | awk '{s+=$1} END {print s+0}')"
AFTER_AT="$(date '+%Y-%m-%d %H:%M:%S')"

python3 - "${SUMMARY}" "${GAME}" "${YNTT_VER}" "${BEFORE_KB}" "${AFTER_KB}" "${BEFORE_AT}" "${AFTER_AT}" "${FINAL_APK}" <<'PY'
import json
import sys
from pathlib import Path

out, game, ver, before_kb, after_kb, before_at, after_at, apk = sys.argv[1:]
data = {
    "game": game,
    "version": ver,
    "final_apk": apk,
    "before_kb": int(before_kb),
    "after_kb": int(after_kb),
    "released_kb": max(0, int(before_kb) - int(after_kb)),
    "before_at": before_at,
    "after_at": after_at,
    "kept": [
        "02_apks/" + game + "（用户管理，未动）",
        "08_output/" + game + "（正式包由 prune_output_final.sh 单独裁剪）",
        "06_dumped/" + game + " git-tracked text archives",
        "06_dumped/" + game + "/official_" + ver + "/rva_targets.json 锚点摘录",
        "09_test/" + game + "（本轮证据）",
        "04_scripts/" + game,
        "10_docs/" + game,
        "mod菜单 Yntt sources",
    ],
    "removed_roots": [
        "03_decompiled/" + game,
        "07_patched/" + game,
        "06_dumped/" + game + "/official_" + ver + " ignored binaries (dump.cs/script.json/il2cpp.h/stringliteral.json/DummyDll)",
        "tmp yntt analysis directories",
    ],
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat "${SUMMARY}"
yntt_log "done"
