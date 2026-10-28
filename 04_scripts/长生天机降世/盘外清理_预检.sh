#!/usr/bin/env bash
# 盘外产物清理：先打印解析后的绝对路径与体积，确认无仓内引用后删除，再复扫
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
T=/c/Users/Administrator/AppData/Local/Temp

echo "==== [A] 待删清单（绝对路径 + 体积）===="
TARGETS=(
  "$T/cstjjs_boot.txt" "$T/cstjjs_full.txt" "$T/cstjjs_maps.txt" "$T/cstjjs_window.txt"
  "$T/gdzfdj_build" "$T/mxyzt_build" "$T/zzz_build" "$T/menu_dec_chk"
  "$T/ysxz_redteam" "$T/nx_zzz_starry_20261005"
  "$T/starry_magic_prefs.backup.xml" "$T/starry_magic_prefs.device.xml"
  "/e/_build" "/e/nixiang_clone.log"
)
for t in "${TARGETS[@]}"; do
  if [ -e "$t" ]; then
    printf '  %-70s %s\n' "$t" "$(du -sh "$t" 2>/dev/null | cut -f1)"
  fi
done
echo "  -- nixiang_*.* 临时脚本与 tmp*/mod菜单 残留 --"
ls -d "$T"/nixiang_*.sh "$T"/nixiang_*.py 2>/dev/null | wc -l | xargs echo "  nixiang_* 文件数:"
ls -d "$T"/tmp*/mod菜单 2>/dev/null | wc -l | xargs echo "  tmp*/mod菜单 目录数:"
ls -d "$T"/zzz_sym_* 2>/dev/null | wc -l | xargs echo "  zzz_sym_* 目录数:"

echo
echo "==== [B] 仓内引用检查（防止删掉仍被脚本引用的路径）===="
for kw in 'E:/_build' 'E:\\_build' '/e/_build' 'nixiang_clone.log' 'AppData/Local/Temp/nixiang_' 'AppData/Local/Temp/cstjjs' 'AppData/Local/Temp/zzz_build' 'AppData/Local/Temp/mxyzt_build' 'AppData/Local/Temp/gdzfdj_build'; do
  n=$(grep -rIl -- "$kw" 04_scripts skill 10_docs mod菜单 AGENTS.md 2>/dev/null | grep -v '09_test/' | wc -l)
  echo "  $kw -> 引用文件数 $n"
done
