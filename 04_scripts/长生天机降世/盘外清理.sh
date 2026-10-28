#!/usr/bin/env bash
# 盘外产物清理（%TEMP% 与 E 盘根散件）。规则：逐条打印绝对路径 → 删除 → 复扫。
# 明确不动：E:\_build（绿梦交付候选与 assemble.py，属另一游戏的活路径，待用户裁决迁移）
# 注意：保持 bash 3.2 兼容（mac 侧同仓）——不用 mapfile/readarray。
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
T=/c/Users/Administrator/AppData/Local/Temp
LOG="09_test/盘外产物清理_20261028.txt"
: > "$LOG"
say() { printf '%s\n' "$*" | tee -a "$LOG"; }

say "==== 盘外产物清理 $(date '+%Y-%m-%d %H:%M') ===="

FILES="$T/cstjjs_boot.txt
$T/cstjjs_full.txt
$T/cstjjs_maps.txt
$T/cstjjs_window.txt
$T/starry_magic_prefs.backup.xml
$T/starry_magic_prefs.device.xml
/e/nixiang_clone.log"

DIRS="$T/gdzfdj_build
$T/mxyzt_build
$T/zzz_build
$T/menu_dec_chk
$T/ysxz_redteam
$T/nx_zzz_starry_20261005"

say ""
say "---- 即将删除的文件 ----"
printf '%s\n' "$FILES" | while IFS= read -r f; do
  [ -n "$f" ] && [ -e "$f" ] && say "  F $f"
done
for f in $(ls -1 "$T"/nixiang_*.sh "$T"/nixiang_*.py 2>/dev/null); do say "  F $f"; done

say "---- 即将删除的目录 ----"
printf '%s\n' "$DIRS" | while IFS= read -r d; do
  [ -n "$d" ] && [ -e "$d" ] && say "  D $d"
done
for p in $(ls -1d "$T"/tmp*/mod菜单 2>/dev/null); do say "  D $(dirname "$p")"; done
for d in $(ls -1d "$T"/zzz_sym_* 2>/dev/null); do say "  D $d"; done

say ""
say "---- 总体积 ----"
TOT=0
printf '%s\n' "$FILES" | while IFS= read -r f; do
  [ -n "$f" ] && [ -e "$f" ] && du -sk "$f" 2>/dev/null
done > /tmp/_sz.txt
printf '%s\n' "$DIRS" | while IFS= read -r d; do
  [ -n "$d" ] && [ -e "$d" ] && du -sk "$d" 2>/dev/null
done >> /tmp/_sz.txt
for p in $(ls -1d "$T"/tmp*/mod菜单 2>/dev/null); do du -sk "$(dirname "$p")" 2>/dev/null; done >> /tmp/_sz.txt
for d in $(ls -1d "$T"/zzz_sym_* 2>/dev/null); do du -sk "$d" 2>/dev/null; done >> /tmp/_sz.txt
TOT=$(awk '{s+=$1} END{print s+0}' /tmp/_sz.txt)
rm -f /tmp/_sz.txt
say "  合计约 $((TOT / 1024)) MB"

printf '%s\n' "$FILES" | while IFS= read -r f; do rm -f -- "$f"; done
for f in $(ls -1 "$T"/nixiang_*.sh "$T"/nixiang_*.py 2>/dev/null); do rm -f -- "$f"; done
printf '%s\n' "$DIRS" | while IFS= read -r d; do rm -rf -- "$d"; done
for p in $(ls -1d "$T"/tmp*/mod菜单 2>/dev/null); do rm -rf -- "$(dirname "$p")"; done
for d in $(ls -1d "$T"/zzz_sym_* 2>/dev/null); do rm -rf -- "$d"; done

say ""
say "---- 复查 ----"
CNT=0
printf '%s\n' "$FILES" | while IFS= read -r f; do
  [ -n "$f" ] && [ -e "$f" ] && say "  仍存在: $f"
done
for d in $(printf '%s\n' "$DIRS"); do
  [ -e "$d" ] && say "  仍存在: $d"
done
say "  残留 nixiang_*: $(ls -1 "$T"/nixiang_*.sh 2>/dev/null | wc -l)"
say "  残留 tmp*/mod菜单: $(ls -1d "$T"/tmp*/mod菜单 2>/dev/null | wc -l)"
say "  残留 zzz_sym_*: $(ls -1d "$T"/zzz_sym_* 2>/dev/null | wc -l)"
say ""
say "---- 保留项（人工裁决）----"
say "  D /e/_build                     ~6.4G  绿梦(lvmeng)交付候选+assemble.py，文档仍引用 ⇒ 未删，待裁决"
say "  F Documents/MuMu共享文件夹/Download/.starry_magic    MuMu 运行环境标记"
say "  F Desktop/nixiang 双机体检.lnk                      用户桌面快捷方式"
say "  D AppData/Roaming/leidian14/cache/*.png             雷电自身图标缓存"
