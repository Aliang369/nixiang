#!/usr/bin/env bash
# ================================================================
#  audit_with_waivers.sh — 框架入口审计「已知豁免」包装器（2026-10-15 山海幻境 4.3 复盘 P0-4）
#
#  为什么有它：framework_entry_audit.sh 的检查是**通用**的，但有几项对某些游戏是
#  「已知无害偏离」（隔离构建导致 mk 分支不符 / Application 入口形态 / TapLoginForceWeb
#  白名单不在本路径）。没有豁免机制时，每轮换版本都要人工重判一遍并重写说明（本轮实测
#  白耗 3 次调用 + 一段推理），且容易把「已知」和「新问题」混在一起。
#
#  做法：**不改通用审计本体**（多游戏共用，改动面太大），而是在其输出上做后处理：
#    跑审计 → 逐条 ✗ 行按 10_docs/{游戏}/审计豁免.json 的 match 判定
#    → 命中且未过期：改写为 ✓豁免[<id>] 并打印理由/证据/复核期限
#    → 命中但已过期：保留 ✗ 并打印「豁免过期，需重判」
#    → 汇总行重算；全部 ✗ 都被豁免时 rc=0，否则沿用审计原 rc。
#
#  用法:
#    bash 04_scripts/_common/audit_with_waivers.sh --game 山海幻境 --pkg com.chx.shhj [--apk <apk>]
#    [--waivers <豁免.json>] [--audit-only]     # --audit-only 只跑原审计（对照用）
#  退出码: 0=无未豁免 ✗；1=存在未豁免/已过期的 ✗；2=用法错
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GAME="" PKG="" APK="" WAIVERS="" AUDIT_ONLY=0

usage() { sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//' >&2; exit 2; }
while [[ $# -gt 0 ]]; do
  case "$1" in
    --game)   GAME="${2:-}"; shift 2 ;;
    --pkg)    PKG="${2:-}"; shift 2 ;;
    --apk)    APK="${2:-}"; shift 2 ;;
    --waivers) WAIVERS="${2:-}"; shift 2 ;;
    --audit-only) AUDIT_ONLY=1; shift ;;
    -h|--help) usage ;;
    *) echo "错误: 未知参数 $1" >&2; usage ;;
  esac
done
[[ -n "$GAME" && -n "$PKG" ]] || usage
[[ -n "$WAIVERS" ]] || WAIVERS="$ROOT/10_docs/$GAME/审计豁免.json"

AUDIT="$SCRIPT_DIR/framework_entry_audit.sh"
[[ -f "$AUDIT" ]] || { echo "错误: 缺少 $AUDIT" >&2; exit 2; }

TMP="$(mktemp)"; trap 'rm -f "$TMP"' EXIT
set +e
bash "$AUDIT" --game "$GAME" --pkg "$PKG" ${APK:+--apk "$APK"} >"$TMP" 2>&1
AUDIT_RC=$?
set -e

if [[ "$AUDIT_ONLY" == "1" || ! -f "$WAIVERS" ]]; then
  cat "$TMP"
  [[ -f "$WAIVERS" ]] || echo "[waivers] 无豁免表（${WAIVERS}）⇒ 原样返回审计结论"
  exit "$AUDIT_RC"
fi

python3 - "$TMP" "$WAIVERS" "$GAME" <<'PY'
import json, re, sys
from datetime import date
from pathlib import Path

out_path, wf, game = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3]
spec = json.loads(wf.read_text(encoding="utf-8"))
today = date.today().isoformat()
lines = out_path.read_text(encoding="utf-8", errors="replace").splitlines()
used: dict[str, int] = {}
kept_fail = 0
expired: list[str] = []

for ln in lines:
    # 审计自带的结论行不含检查项语义，一律丢弃，最后按豁免结果重算（否则它自己的 ✗ 会被当成一项未豁免）
    if "结论：" in ln or ln.startswith("════"):
        continue
    m = re.search(r"✗(.*)$", ln)
    if not m:
        print(ln)
        continue
    hit = None
    for w in spec.get("waivers", []):
        if any(k in ln for k in w.get("match", [])):
            hit = w
            break
    if hit is None:
        kept_fail += 1
        print(ln)
        continue
    if today > str(hit.get("review_by", "9999-12-31")):
        expired.append(hit["id"])
        kept_fail += 1
        print(ln)
        print(f"      ⚠ 豁免『{hit['id']}』已于 {hit['review_by']} 过期 ⇒ 需重新判定（不改写为豁免）")
        continue
    used[hit["id"]] = used.get(hit["id"], 0) + 1
    print(f"  ✓豁免[{hit['id']}] {m.group(1).strip()}")
    print(f"      理由: {hit['conclusion']} —— {hit['reason']}")
    print(f"      证据: {hit.get('evidence','（缺）')}")
    print(f"      登记 {hit.get('date','?')} ｜ 复核期限 {hit.get('review_by','?')}")

print("")
print("════════ 豁免汇总 ════════")
if used:
    for k, v in used.items():
        print(f"  ✓豁免 {k}（命中 {v} 行）")
else:
    print("  （本次无命中豁免）")
print(f"  未豁免 ✗ 行数 = {kept_fail}" + (f"；豁免过期 {len(expired)} 项: {', '.join(expired)}" if expired else ""))
print(f"  豁免表: {wf}")
if kept_fail == 0:
    print(f"结论：✅ 全部 ✗ 已豁免（{len(used)} 项，附理由与证据）⇒ 可出包；豁免到期后自动恢复 ✗")
else:
    print(f"结论：❌ 仍有 {kept_fail} 项未豁免 ✗ ⇒ 禁止出包")
raise SystemExit(0 if kept_fail == 0 else 1)
PY
RC=$?
exit "$RC"
