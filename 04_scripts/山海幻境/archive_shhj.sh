#!/usr/bin/env bash
# 山海幻境 4.0 归档准备：核对文档、正式包与清理台账，生成 Git 提交范围。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="山海幻境"
FINAL="$NIXIANG/08_output/$GAME/${GAME}_4_loginfix-releaseSigned.apk"
DOC="$NIXIANG/10_docs/$GAME/星光落地_4_20260916.md"
FORMAL="$NIXIANG/10_docs/$GAME/收尾与最小复现清单_4_20260916.md"
CLEANUP="$NIXIANG/04_scripts/$GAME/cleanup_summary.json"
SUMMARY="$NIXIANG/04_scripts/$GAME/archive_summary.json"
SCOPE="$NIXIANG/04_scripts/$GAME/archive_git_scope.txt"
EXPECTED_MD5="6bd976050b7571d71a9cd3597513427d"

[[ -f "$FINAL" ]] || { echo "错误: 正式包不存在" >&2; exit 1; }
[[ "$(md5 -q "$FINAL")" == "$EXPECTED_MD5" ]] || { echo "错误: 正式包 md5 不符" >&2; exit 1; }
[[ -f "$DOC" ]] || { echo "错误: 落点文档不存在" >&2; exit 1; }
[[ -f "$FORMAL" ]] || { echo "错误: 收尾与最小复现清单不存在" >&2; exit 1; }
[[ -f "$CLEANUP" ]] || { echo "错误: 清理台账不存在" >&2; exit 1; }
grep -q '登录 ret=5 非法客户端修复' "$DOC" || { echo "错误: 登录修复记录缺失" >&2; exit 1; }
grep -q '山海幻境.*4 / vc54' "$NIXIANG/10_docs/_state/current.md" || { echo "错误: current.md 索引未更新" >&2; exit 1; }

git -C "$NIXIANG" diff --check -- \
  04_scripts/$GAME \
  10_docs/$GAME \
  10_docs/_state/current.md \
  mod菜单/starry-mod-menu/scripts/surgical_shhj_3.8.sh

git -C "$NIXIANG" status --short -- \
  04_scripts/$GAME \
  10_docs/$GAME \
  mod菜单/starry-mod-menu/scripts/surgical_shhj_3.8.sh > "$SCOPE"

python3 - "$SUMMARY" "$FINAL" "$EXPECTED_MD5" "$DOC" "$FORMAL" "$SCOPE" <<'PY'
import json
import subprocess
import sys
from pathlib import Path

(out, final, md5, doc, formal, scope) = sys.argv[1:]
root = "/Users/aliang/Documents/nixiang"
data = {
    "game": "山海幻境",
    "version": "4 / vc54",
    "status": "ready_for_git",
    "final_apk": final,
    "final_apk_md5": md5,
    "landing_doc": doc,
    "formal_closure_doc": formal,
    "cleanup_summary": f"{root}/04_scripts/山海幻境/cleanup_summary.json",
    "git_scope_file": scope,
    "git_head": subprocess.check_output(["git", "-C", root, "rev-parse", "HEAD"], text=True).strip(),
    "git_branch": subprocess.check_output(["git", "-C", root, "branch", "--show-current"], text=True).strip(),
    "notes": "current.md 仅暂存本作对应 hunk；无关会话改动不得纳入提交。",
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "[archive] final_md5=$EXPECTED_MD5"
echo "[archive] summary=$SUMMARY"
echo "[archive] git_scope=$SCOPE"
