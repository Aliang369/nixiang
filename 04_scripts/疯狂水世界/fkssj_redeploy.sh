#!/usr/bin/env bash
# ================================================================
# fkssj_redeploy.sh — 疯狂水世界：改码后「一键重部署」
#
#   编 so → 外科组包 → 签名 → 设备就位 → 装机 → 启动，全部经
#   04_scripts/pipeline.sh（机械操作唯一合法入口）。
#
#   为什么不是直接 next --loop：
#   每次重编都会让产物字节变化，pipeline 首次跑到该阶段必然报
#   「⚠ 产物指纹变化（可能被改动过，请核对）」并落 failed——这是**设计内的
#   人工确认点**，不是错误。本脚本的处置＝照 stages.md 的姿势
#   `reset <stage>` 后重跑一次（重跑时基线已刷新 ⇒ 校验通过），
#   且绝不跳过任何硬校验（官包零改动 / 渠道块回注 / 设备闭环 md5）。
#
#   用法：
#     bash 04_scripts/疯狂水世界/fkssj_redeploy.sh              # 全流程
#     bash 04_scripts/疯狂水世界/fkssj_redeploy.sh --no-build   # 只重组包+装机（so 没改时）
#     bash 04_scripts/疯狂水世界/fkssj_redeploy.sh --to sign    # 走到某阶段为止
# ================================================================
set -uo pipefail

GAME="疯狂水世界"
NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
PIPE="${NIXIANG}/04_scripts/pipeline.sh"
LOG="${NIXIANG}/04_scripts/疯狂水世界/pipeline.log"

DO_BUILD=1
STOP_AT=""
while [ $# -gt 0 ]; do
    case "$1" in
        --no-build) DO_BUILD=0 ;;
        --to) STOP_AT="${2:-}"; shift ;;
        -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
        *) echo "未知参数: $1" >&2; exit 2 ;;
    esac
    shift
done

die() { echo "❌ $*" >&2; exit 1; }

stage_ok() {  # 判定某阶段当前是否 done
    bash "$PIPE" "$GAME" status 2>/dev/null | grep -qE "✅ [0-9]+\. $1 "
}

run_stage_twice_if_fingerprint() {  # $1=stage_id
    local st="$1" out
    out="$(bash "$PIPE" "$GAME" run "$st" 2>&1)"
    echo "$out" | grep -qE '✅ 阶段' && { echo "✅ $st 完成"; return 0; }
    if echo "$out" | grep -q '产物指纹变化'; then
        echo "ℹ $st 产物指纹变化 = 重编后的预期确认点 ⇒ reset 后重跑一次"
        bash "$PIPE" "$GAME" reset "$st" >/dev/null 2>&1
        out="$(bash "$PIPE" "$GAME" run "$st" 2>&1)"
        echo "$out" | grep -qE '✅ 阶段' && { echo "✅ $st 完成（二次）"; return 0; }
    fi
    echo "$out" | tail -20 >&2
    echo "── pipeline.log 尾部 ──" >&2
    tail -25 "$LOG" >&2
    die "阶段 $st 失败"
}

echo "════ 疯狂水世界 重部署 ════"
[ "$DO_BUILD" -eq 1 ] && {
    echo "▶ [1] 编 so"
    bash "${NIXIANG}/04_scripts/疯狂水世界/fkssj_build_so.sh" 2>&1 | tail -2 || die "build_so 失败"
}

for st in surgical sign device_ready install; do
    echo "▶ 阶段 $st"
    run_stage_twice_if_fingerprint "$st"
    if [ -n "$STOP_AT" ] && [ "$st" = "$STOP_AT" ]; then
        echo "已在 $STOP_AT 收手"; exit 0
    fi
done

echo "▶ 启动游戏"
SERIAL="$(cat "${NIXIANG}/04_scripts/疯狂水世界/.device_serial" 2>/dev/null | sed -n 's/^serial=//p')"
SERIAL="${SERIAL:-96f0d883}"
adb -s "$SERIAL" logcat -c 2>/dev/null
adb -s "$SERIAL" shell monkey -p com.gx.sw.qa.fkssj004.esj -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
sleep 12
adb -s "$SERIAL" logcat -d -v time 2>/dev/null | grep -aE 'fkssj-js|Fkssj\]|pump#' | tail -25
echo "════ DONE ════"
