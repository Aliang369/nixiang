#!/usr/bin/env bash
# 偶像梦幻祭2 4.5.8861 归档阶段：固化最终产物与文档证据，不执行 git 提交。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="偶像梦幻2"
VERSION="4.5.8861"
FINAL="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
DOC="$NIXIANG/10_docs/偶像梦幻2/星空落地_${VERSION}_20260915.md"
INDEX="$NIXIANG/10_docs/_state/current.md"
STATE="$NIXIANG/10_docs/_state/games/偶像梦幻2.md"
CLEANUP="$NIXIANG/04_scripts/$GAME/cleanup_summary.json"
SUMMARY="$NIXIANG/04_scripts/$GAME/archive_summary.json"
SCOPE="$NIXIANG/04_scripts/$GAME/archive_git_scope.txt"
EXPECTED_MD5="29598948a08e234b83cca24a4a7fbd1c"

[[ -f "$FINAL" ]] || { echo "ERROR: missing final APK" >&2; exit 1; }
[[ "$(md5 -q "$FINAL")" == "$EXPECTED_MD5" ]] || { echo "ERROR: final APK md5 mismatch" >&2; exit 1; }
[[ -f "$DOC" ]] || { echo "ERROR: missing landing document" >&2; exit 1; }
[[ -f "$CLEANUP" ]] || { echo "ERROR: missing cleanup summary" >&2; exit 1; }
[[ -f "$STATE" ]] || { echo "ERROR: missing game state summary" >&2; exit 1; }
grep -q '测试没问题' "$DOC" || { echo "ERROR: user acceptance not recorded in doc" >&2; exit 1; }
grep -q '偶像梦幻祭2.*4.5.8861.*交付完成' "$INDEX" \
  || { echo "ERROR: current index is not updated" >&2; exit 1; }

git -C "$NIXIANG" diff --check
git -C "$NIXIANG" status --short -- \
  04_scripts/偶像梦幻2 \
  10_docs/偶像梦幻2 \
  10_docs/_state/games/偶像梦幻2.md \
  10_docs/_state/current.md \
  mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java \
  mod菜单/starry-mod-menu/app/src/main/jni/Android_es2.mk \
  mod菜单/starry-mod-menu/app/src/main/jni/Games/Es2Mod.cpp \
  mod菜单/starry-mod-menu/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so \
  mod菜单/starry-mod-menu/app/src/main/libs/arm64-v8a/libstarry_magic.so \
  mod菜单/starry-mod-menu/scripts/surgical_es2_4.5.8861.sh > "$SCOPE"

python3 - "$SUMMARY" "$FINAL" "$EXPECTED_MD5" "$DOC" "$INDEX" "$STATE" "$SCOPE" <<'PY'
import json, subprocess, sys
from pathlib import Path
(out, final, md5, doc, index, state, scope) = sys.argv[1:]
data = {
    "game": "偶像梦幻2",
    "version": "4.5.8861",
    "status": "ready_for_git",
    "final_apk": final,
    "final_apk_md5": md5,
    "landing_doc": doc,
    "state_index": index,
    "state_detail": state,
    "git_scope_file": scope,
    "git_head": subprocess.check_output(["git", "-C", "/Users/aliang/Documents/nixiang", "rev-parse", "HEAD"], text=True).strip(),
    "git_branch": subprocess.check_output(["git", "-C", "/Users/aliang/Documents/nixiang", "branch", "--show-current"], text=True).strip(),
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

echo "[archive] final_md5=$EXPECTED_MD5"
echo "[archive] summary=$SUMMARY"
echo "[archive] git_scope=$SCOPE"
