#!/usr/bin/env bash
# 绿梦收官清理（用户 2026-10-28：「绿梦已经完结了，直接清理到 08 这两个安装包」）
# 依据：10_docs/tools/05「收官归档清单」——03/06 大二进制、07、09 可再生；08 只留正式包
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
LOG="10_docs/绿梦/收官清理_20261028.md"
: > /tmp/_lm_del.txt
say() { printf '%s\n' "$*"; }

del() {
  [ -e "$1" ] || return 0
  sz=$(du -sk "$1" 2>/dev/null | cut -f1)
  printf '%s\t%s KB\n' "$1" "$sz" >> /tmp/_lm_del.txt
  rm -rf -- "$1"
}

echo "==== [1] 08_output/绿梦 归位为两个包 ===="
mv -f 08_output/绿梦/_import_20261028/lvmeng_v25.apk 08_output/绿梦/
mv -f 08_output/绿梦/_import_20261028/lvmeng_v32.apk 08_output/绿梦/
rmdir 08_output/绿梦/_import_20261028 2>/dev/null || echo "  !! _import 目录非空：$(ls 08_output/绿梦/_import_20261028)"
ls -l 08_output/绿梦

echo "==== [2] 07_patched/绿梦（中间产物，重跑管线即得）===="
for d in 07_patched/绿梦/resign_test 07_patched/绿梦/tools; do del "$d"; done

echo "==== [3] 09_test/绿梦（过程日志，可再生）===="
for d in 09_test/绿梦/_import_20261028 09_test/绿梦/precheck_20261028_021046; do del "$d"; done
for f in 09_test/绿梦/fg_cage_long_20261027.log 09_test/绿梦/resign_cage_run_2311.txt 09_test/绿梦/resign_test_logcat_20261027.txt 09_test/绿梦/v9_death_trace.txt; do del "$f"; done

echo "==== [4] 06_dumped/绿梦_20261027（热更样本/内存 dump，可再生）===="
del 06_dumped/绿梦_20261027

echo "==== [5] 写清单 ===="
{
  echo "# 绿梦 收官清理（2026-10-28）"
  echo
  echo "> 用户原话：「绿梦已经完结了，直接清理到 08 这两个安装包」。"
  echo "> 依据：\`10_docs/tools/05_目录结构脚本环境与交付清理.md\`「收官归档清单」。"
  echo
  echo "## 08_output/绿梦 最终状态（唯一保留）"
  echo
  echo '| 体积 | 文件 | md5 |'
  echo '|---|---|---|'
  for f in 08_output/绿梦/*.apk; do
    [ -f "$f" ] || continue
    echo "| $(stat -c%s "$f") | \`$(basename "$f")\` | \`$(md5sum "$f" | cut -c1-32)\` |"
  done
  echo
  echo "## 已删（可再生滚动物）"
  echo
  echo '| 路径 | 体积 |'
  echo '|---|---|'
  awk -F'\t' '{printf "| `%s` | %s |\n", $1, $2}' /tmp/_lm_del.txt
  echo
  echo "## 保留"
  echo
  echo "- \`04_scripts/绿梦/\`（源码、探针、\`build_inputs_20261028/\`、\`lvmeng_mod/\`、\`ilpatch/\`）：复现链本体，永留。"
  echo "- \`02_apks/\`：无绿梦底包（历史放在设备/网盘），未涉及。"
  echo "- 迁移来源与体积对照见 \`_import_E_build_清理清单_20261028.md\`。"
} > "$LOG"
echo "清单 -> $LOG ($(wc -l < "$LOG") 行)"
rm -f /tmp/_lm_del.txt

echo "==== 复查 ===="
echo "08_output/绿梦:"; ls -1 08_output/绿梦
echo "07_patched/绿梦:"; ls -1 07_patched/绿梦 2>/dev/null || echo "  (已空/不存在)"
echo "09_test/绿梦:"; ls -1 09_test/绿梦 2>/dev/null || echo "  (已空/不存在)"
echo "06_dumped/绿梦_20261027:"; ls -d 06_dumped/绿梦_20261027 2>/dev/null || echo "  (已删 ✓)"
