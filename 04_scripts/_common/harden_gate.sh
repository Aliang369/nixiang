#!/usr/bin/env bash
# ⛔ 停用件（2026-10-27 用户裁决）：加固门禁不再被任何规则/装机门禁要求（AGENTS.md §6）。
#    文件保留仅因三个已接入游戏的出包脚本仍在调用；各游戏去加固时摘掉调用后即可删除。
# ================================================================
#  harden_gate.sh — 出包加固门禁的**共享入口**（所有游戏默认强制）
#
#  为什么要有这层壳（2026-10-27，`10_docs/防御加固_通用路线图.md` §1 耦合点 2.4）：
#    加固门禁原先只有勇士之心的 assemble 脚本里手写了调用，别的游戏全靠"记得跑"。
#    这里把它收成一个动词，任何游戏出包脚本末尾加**一行**即接入：
#        bash 04_scripts/_common/harden_gate.sh <成品APK> [--game <游戏名|tag>]
#    游戏归属默认按 APK 所在目录名推断（08_output/<游戏>/…），也可显式 --game。
#
#  行为：跑 harden_apk.py check（C1 公钥 / C2 清单 / C3 去路标 / C4 产物同一 /
#        C6–C9 载荷链路，native 型游戏 C6–C9 自动 N/A）；PASS 自动写装机凭证
#        09_test/<游戏>/harden/<apk_md5>.stamp（pre_install_gate.sh 查11 认这个），
#        FAIL 返回非 0 ⇒ 调用方必须拒绝交付该包。
#
#  用法：
#    bash 04_scripts/_common/harden_gate.sh 08_output/冒险与征途/xxx.apk
#    bash 04_scripts/_common/harden_gate.sh 08_output/冒险与征途/xxx.apk --game mxyzt
# ================================================================
set -u

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=env.sh
. "$HERE/env.sh"

APK=""
GAME=""
while [ "$#" -gt 0 ]; do
  case "$1" in
    --game) GAME="${2:-}"; shift 2 ;;
    -h|--help)
      echo "用法: bash 04_scripts/_common/harden_gate.sh <成品APK> [--game <游戏名|tag>]"
      exit 0 ;;
    *) [ -z "$APK" ] && APK="$1"; shift ;;
  esac
done

[ -n "$APK" ] || { echo "ERROR: 缺成品 APK 参数（用法见文件头）" >&2; exit 2; }
[ -f "$APK" ] || { echo "ERROR: APK 不存在：$APK" >&2; exit 2; }

# ⛔ 2026-10-27 用户裁决：加固门禁已下线 ⇒ 默认不判定、不拦交付。
#    保留本脚本只为兼容三个已接入游戏的出包脚本（它们仍在调用）；需要真跑时显式 HARDEN_GATE_ENABLED=1。
if [ "${HARDEN_GATE_ENABLED:-}" != "1" ]; then
  echo "⛔ 加固门禁已下线（2026-10-27 用户裁决）⇒ 不判定、不拦交付：$APK"
  echo "   需要真跑：HARDEN_GATE_ENABLED=1 bash 04_scripts/_common/harden_gate.sh <apk> [--game <名|tag>]"
  exit 0
fi

PY="${NIXIANG_PYTHON:-python}"
[ -n "$PY" ] || { echo "ERROR: python 未解析（env.sh NIXIANG_PYTHON 为空）" >&2; exit 2; }

if [ -n "$GAME" ]; then
  "$PY" "$HERE/harden_apk.py" check --apk "$APK" --game "$GAME"
else
  "$PY" "$HERE/harden_apk.py" check --apk "$APK"
fi
rc=$?

if [ "$rc" -ne 0 ]; then
  echo "" >&2
  echo "✗ 加固门禁未通过 ⇒ 拒绝交付该包（修复上方 FAIL 项后重跑本脚本）" >&2
  echo "  逃生门（仅应急，用了必须在汇报里写明理由）：harden_apk.py 直接跑或 STAGE_NEXT_FORCE=1" >&2
  exit "$rc"
fi
echo "✓ 加固门禁 PASS（凭证已写入 09_test/<游戏>/harden/）"
exit 0
