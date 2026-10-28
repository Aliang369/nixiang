#!/usr/bin/env bash
# 阶段1：E:\_build 整体迁入工作区（同盘 mv，瞬间完成），不做任何删除
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
DST="07_patched/绿梦/_import_E_build_20261028"
mkdir -p "$DST"

if [ -d /e/_build ]; then
  for item in /e/_build/*; do
    [ -e "$item" ] || continue
    mv -- "$item" "$DST/" || echo "!! 移动失败: $item"
  done
  rmdir /e/_build 2>/dev/null || echo "!! /e/_build 非空，未删除"
else
  echo "(!) /e/_build 不存在（可能已迁移）"
fi

echo "== 迁移后 E 盘根 =="
ls -d /e/_build 2>/dev/null || echo "  E:\\_build 已不存在 ✓"
echo "== 目标体积 =="
du -sh "$DST"
echo "== 目标内容 =="
du -sh "$DST"/* | sort -h
