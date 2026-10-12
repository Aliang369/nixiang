#!/usr/bin/env bash
# ============================================================================
#  疯狂水世界：取回「已解密游戏脚本」（阶段② 锚点来源）
#
#  原理：装机版 §⑨ 的催抓线程经 CocosJsBridge.evalString 注入 dump JS，
#        游戏把已解密脚本/模块源码写到自己的外部私有目录
#          /sdcard/Android/data/com.gx.sw.qa.fkssj004.esj/files/
#        该目录对 adb shell 可读（本机无 root 也通，实测），直接 pull。
#
#  用法：bash 04_scripts/疯狂水世界/dump_pull.sh [--wait 秒]
#  产物：09_test/疯狂水世界/jsdump/（已被 .gitignore 覆盖，不入库）
# ============================================================================
set -uo pipefail

SERIAL="${SERIAL:-96f0d883}"
PKG="com.gx.sw.qa.fkssj004.esj"
REMOTE="/sdcard/Android/data/${PKG}/files"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "${SCRIPT_DIR}/../.." && pwd)"
OUT="${NIXIANG}/09_test/疯狂水世界/jsdump"

WAIT_SEC=0
if [[ "${1:-}" == "--wait" ]]; then WAIT_SEC="${2:-30}"; fi

echo "[dump] serial=${SERIAL} pkg=${PKG}"
adb -s "${SERIAL}" get-state >/dev/null 2>&1 || { echo "[dump] 设备未连接"; exit 1; }

if [[ "${WAIT_SEC}" -gt 0 ]]; then
    echo "[dump] 等待 ${WAIT_SEC}s 让游戏跑起来…"
    sleep "${WAIT_SEC}"
fi

echo "[dump] 远端目录："
adb -s "${SERIAL}" shell "ls -l ${REMOTE} 2>/dev/null" | sed 's/^/  /' || true

mkdir -p "${OUT}"

# 逐个 pull（不存在的静默跳过，bash 3.2 兼容）
PULLED=0
for f in fkssj_status.txt fkssj_modlist.txt fkssj_dump.js; do
    if adb -s "${SERIAL}" shell "[ -f ${REMOTE}/${f} ]" >/dev/null 2>&1; then
        adb -s "${SERIAL}" pull "${REMOTE}/${f}" "${OUT}/${f}" >/dev/null 2>&1 && {
            printf '[dump] ✓ %-22s %s B\n' "${f}" "$(wc -c < "${OUT}/${f}" | tr -d ' ')"
            PULLED=$((PULLED + 1))
        }
    else
        echo "[dump] ✗ ${f} 不存在"
    fi
done

# fkssj_raw_* 可能多个
RAWS="$(adb -s "${SERIAL}" shell "ls ${REMOTE}/fkssj_raw_* 2>/dev/null" | tr -d '\r')"
if [[ -n "${RAWS}" ]]; then
    for r in ${RAWS}; do
        b="$(basename "${r}")"
        adb -s "${SERIAL}" pull "${r}" "${OUT}/${b}" >/dev/null 2>&1 && {
            printf '[dump] ✓ %-22s %s B\n' "${b}" "$(wc -c < "${OUT}/${b}" | tr -d ' ')"
            PULLED=$((PULLED + 1))
        }
    done
fi

echo "[dump] 共取回 ${PULLED} 个文件 → ${OUT}"

if [[ -f "${OUT}/fkssj_status.txt" ]]; then
    echo "[dump] ---- fkssj_status.txt（末尾 20 行）----"
    tail -20 "${OUT}/fkssj_status.txt" | sed 's/^/  /'
fi
if [[ -f "${OUT}/fkssj_dump.js" ]]; then
    echo "[dump] ---- dump 概况 ----"
    grep -c '^///// MOD ' "${OUT}/fkssj_dump.js" | sed 's/^/  模块段落数: /'
    grep -o '^///// MODULE NAMES ([0-9]*' "${OUT}/fkssj_dump.js" | head -1 | sed 's/^/  /'
fi
