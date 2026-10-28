#!/usr/bin/env bash
# 迁移前核对：dotnet 路径引用 / starry 是否与工作区重复 / ilpatch 是否已有对应文件
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8

echo "===== 1) 绿梦脚本/文档里 dotnet 的写法 ====="
grep -rn 'dotnet' 04_scripts/绿梦 10_docs/绿梦 2>/dev/null | grep -v '^Binary' | cut -c1-200 | head -12

echo
echo "===== 2) 工作区是否已有 ilpatch ====="
ls -l 04_scripts/绿梦/ilpatch 2>/dev/null | head -10
echo "--- E 盘 ilpatch ---"
ls -l /e/_build/ilpatch | head -10

echo
echo "===== 3) E:\_build\starry 与工作区 mod菜单 脚本差异（按文件名）====="
ls /e/_build/starry/scripts 2>/dev/null | sort > /tmp/_e_starry.txt
ls mod菜单/starry-mod-menu/scripts 2>/dev/null | sort > /tmp/_ws_starry.txt
echo "E 盘独有："; comm -23 /tmp/_e_starry.txt /tmp/_ws_starry.txt | head -15
echo "工作区独有数：$(comm -13 /tmp/_e_starry.txt /tmp/_ws_starry.txt | wc -l)"
echo "两边同名数：$(comm -12 /tmp/_e_starry.txt /tmp/_ws_starry.txt | wc -l)"
echo "--- E 盘 starry 顶层 ---"
ls /e/_build/starry | head -12
rm -f /tmp/_e_starry.txt /tmp/_ws_starry.txt

echo
echo "===== 4) 绿梦 08_output 现状 ====="
ls -l 08_output/绿梦 2>/dev/null | head -15
echo "===== 5) 绿梦 其它目录 ====="
for d in 02_apks 06_dumped 07_patched 09_test 03_decompiled; do
  echo "-- $d/绿梦: $(ls -d $d/绿梦 2>/dev/null || echo 无)"
done
