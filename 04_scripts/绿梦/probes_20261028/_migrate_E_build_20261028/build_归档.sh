#!/usr/bin/env bash
# 阶段4：构建输入归档 + 消化最后残留 + 修正绿梦文档里的 E:\_build 路径
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SRC="07_patched/绿梦/_import_E_build_20261028"
BI="04_scripts/绿梦/build_inputs_20261028"
MAN="10_docs/绿梦/_import_E_build_清理清单_20261028.md"
mkdir -p "$BI"

echo "==== ilpatch.csproj 差异处理（Windows 变体：net8.0 + 本地 Mono.Cecil）===="
if [ -f "$SRC/ilpatch/ilpatch.csproj" ]; then
  cp -f "$SRC/ilpatch/ilpatch.csproj" 04_scripts/绿梦/ilpatch/ilpatch.win.csproj
  echo "  已存为 04_scripts/绿梦/ilpatch/ilpatch.win.csproj"
  rm -rf -- "$SRC/ilpatch"
fi

echo "==== 构建输入归档（assemble.py 直接引用的那一份，规范要求保留）===="
for item in smali_in lib classes4.dex classes4.dex.bak classes4.dex.bak2 classes4.dex.bak3 classes2_patched.dex payload2___data payload2___info jitblk.bin dllman.bin; do
  if [ -e "$SRC/lvmeng/$item" ]; then
    mv -f -- "$SRC/lvmeng/$item" "$BI/"
    echo "  保留 $item"
  fi
done

echo "==== 残留（应已空）===="
find "$SRC" -mindepth 1 2>/dev/null | head -10
if [ -z "$(find "$SRC" -mindepth 1 2>/dev/null | head -1)" ]; then
  rmdir "$SRC/lvmeng" 2>/dev/null
  rmdir "$SRC" 2>/dev/null && echo "  _import_E_build_20261028 已删除 ✓"
else
  echo "  !! 仍有残留，保留待人工看"
fi

echo "==== 补记清单 ===="
{
  echo
  echo "## 补充：构建输入归档（assemble.py 直接引用的那一份，按规范保留）"
  echo
  echo "\`04_scripts/绿梦/build_inputs_20261028/\` 内含："
  echo
  echo "\`smali_in/\`（菜单注入 smali 树）、\`lib/arm64-v8a/{libstarry_magic.so,libc++_shared.so}\`、\`classes4.dex\`（+3 份 .bak）、\`classes2_patched.dex\`、\`payload2___data/__info\`、\`jitblk.bin\`、\`dllman.bin\`。"
  echo "判据：\`assemble.py\` 第 92 行 \`for need in [\"classes4.dex\", \"lib/arm64-v8a/libstarry_magic.so\", \"AndroidManifest.xml\", \"assets/lvmeng/fg_orig.apk\"]\` 与第 12 行 \`classes2_patched.dex\`。"
  echo "体积：$(du -sh "$BI" | cut -f1)"
  echo
  echo "## 补充：Windows 侧 ilpatch 工程变体"
  echo
  echo "E 盘副本的 \`ilpatch.csproj\` 与工作区模板不同（net10.0→net8.0、改本地 \`01_tools/il2cppdumper/Mono.Cecil.dll\` 引用、加 AllowUnsafeBlocks），已存为 \`04_scripts/绿梦/ilpatch/ilpatch.win.csproj\`，其余 4 个 .cs 与工作区逐文件 md5 相同。"
} >> "$MAN"

echo "==== 绿梦文档里的 E:\\_build 口径修正 ===="
python - <<'PY'
import io, os, re
files = [
 "10_docs/_state/games/绿梦.md",
 "10_docs/绿梦/tapsdk签名校验与网页登录_20261028.md",
 "10_docs/绿梦/雷电houdini_FairGuard实测_20261027.md",
]
rep = [
 (r"E:\\_build\\lvmeng\\lvmeng_v24\.apk", "08_output/绿梦/_import_20261028/（v24 已不在磁盘；现存候选 lvmeng_v25/v32）"),
 (r"E:\\_build\\lvmeng\\lvmeng_v2\{5,6,7,9\}\.apk", "08_output/绿梦/_import_20261028/lvmeng_v25.apk、lvmeng_v32.apk（v26/v27/v29 本轮清理时已删，可由 assemble.py 重出）"),
 (r"E:\\_build\\lvmeng\\assemble\.py", "04_scripts/绿梦/probes_20261028/assemble.py"),
 (r"E:\\_build\\lvmeng", "04_scripts/绿梦/build_inputs_20261028"),
 (r"E:\\_build\\starry", "mod菜单/starry-mod-menu"),
 (r"E:\\_build\\ilpatch", "04_scripts/绿梦/ilpatch"),
 (r"_build/ilpatch", "04_scripts/绿梦/ilpatch"),
]
for f in files:
    if not os.path.exists(f):
        print("  缺文件:", f); continue
    s = io.open(f, encoding="utf-8").read()
    o = s
    for a, b in rep:
        s = re.sub(a, b, s)
    if s != o:
        io.open(f, "w", encoding="utf-8").write(s)
        print("  已更新:", f)
    else:
        print("  无改动:", f)
PY

echo "==== 最终体积 ===="
du -sh 04_scripts/绿梦/probes_20261028 04_scripts/绿梦/build_inputs_20261028 08_output/绿梦/_import_20261028 09_test/绿梦/_import_20261028 01_tools/dotnet 2>/dev/null
echo "工作区根是否还有 _build: $(ls -d /e/_build 2>/dev/null || echo 无 ✓)"
