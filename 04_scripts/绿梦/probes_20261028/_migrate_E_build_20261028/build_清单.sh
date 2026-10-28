#!/usr/bin/env bash
# E:\_build 详细清单（供迁移与清理决策）：只读
set -uo pipefail
export LC_ALL=C.UTF-8
cd /e/_build || exit 1

echo "===== 顶层 ====="
du -sh * 2>/dev/null | sort -h

echo
echo "===== lvmeng 顶层文件（非目录）====="
find lvmeng -maxdepth 1 -type f -printf '%10s  %f\n' 2>/dev/null | sort -n

echo
echo "===== lvmeng 一级目录 ====="
du -sh lvmeng/*/ 2>/dev/null | sort -h

echo
echo "===== starry 结构（前两层）====="
find starry -maxdepth 2 | head -30

echo
echo "===== ilpatch 文件 ====="
find ilpatch -maxdepth 1 -type f -printf '%10s  %f\n' 2>/dev/null | sort -n

echo
echo "===== lvmeng_mod ====="
find lvmeng_mod -maxdepth 2 -type f -printf '%10s  %p\n' 2>/dev/null | sort -n

echo
echo "===== dotnet 是否被引用 ====="
cd /e/nixiang || exit 1
grep -rIl '_build/dotnet\|_build\\\\dotnet' 04_scripts skill 10_docs mod菜单 2>/dev/null | head -5
