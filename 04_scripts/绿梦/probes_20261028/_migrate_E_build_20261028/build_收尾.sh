#!/usr/bin/env bash
# 阶段3：收尾——核对 ilpatch 源码与工作区一致后删副本、删 dexdec、清单落 10_docs、算保留包 hash
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SRC="07_patched/绿梦/_import_E_build_20261028"
MAN="10_docs/绿梦/_import_E_build_清理清单_20261028.md"

echo "==== ilpatch 源码一致性核对（E 副本 vs 工作区）===="
SAME=1
for f in Dumper.cs Patcher.cs Program.cs TolerantRefs.cs ilpatch.csproj; do
  a=$(md5sum "$SRC/ilpatch/$f" 2>/dev/null | cut -d' ' -f1)
  b=$(md5sum "04_scripts/绿梦/ilpatch/$f" 2>/dev/null | cut -d' ' -f1)
  if [ "$a" = "$b" ] && [ -n "$a" ]; then
    echo "  同  $f"
  else
    echo "  异/缺  $f  ($a vs $b)"
    SAME=0
  fi
done

if [ "$SAME" = "1" ]; then
  sz=$(du -sk "$SRC/ilpatch" | cut -f1)
  printf 'ilpatch/（与工作区 04_scripts/绿梦/ilpatch 逐文件 md5 相同，冗余副本）\t%s KB\n' "$sz" >> /tmp/_del_manifest.txt
  rm -rf -- "$SRC/ilpatch"
  echo "  => 已删冗余 ilpatch 副本"
else
  echo "  !! 存在差异，保留 $SRC/ilpatch 不动，需人工合并"
fi

echo "==== 删 dexdec（smali 反编译输出，可 baksmali 再生）===="
if [ -d "$SRC/lvmeng/dexdec" ]; then
  sz=$(du -sk "$SRC/lvmeng/dexdec" | cut -f1)
  printf 'lvmeng/dexdec/（smali 反编译输出，可再生）\t%s KB\n' "$sz" >> /tmp/_del_manifest.txt
  rm -rf -- "$SRC/lvmeng/dexdec"
fi

echo "==== 剩余 ===="
find "$SRC" 2>/dev/null | head -10
echo "体积: $(du -sh "$SRC" 2>/dev/null | cut -f1)"

echo "==== 保留 APK 身份 ===="
for f in 08_output/绿梦/_import_20261028/*.apk; do
  [ -f "$f" ] || continue
  printf '%s  %s  md5=%s\n' "$(stat -c%s "$f")" "$(basename "$f")" "$(md5sum "$f" | cut -c1-32)"
done | tee /tmp/_apk_id.txt

echo "==== 写清单 ===="
{
  echo "# 绿梦：E:\\_build 迁入与清理清单（2026-10-28）"
  echo
  echo "> 缘起：用户 2026-10-28 指令「E:\\_build（6.4 G，绿梦）是迁进工作区，然后清理，到工作区后，按照清理规范在进行清理缩减」。"
  echo "> 规范依据：\`10_docs/tools/05_目录结构脚本环境与交付清理.md\`「清理规则」「收官归档清单」（可再生滚动物删、源码/文档永留、08_output 裁剪需具体口令）。"
  echo
  echo "## 落点（迁入后）"
  echo
  echo "| 原位置 | 现位置 | 内容 |"
  echo "|---|---|---|"
  echo "| \`E:\\_build\\lvmeng\\*.c/*.py/*.sh/*.txt\`、\`assemble.py\`、\`classes2_patched.dex\`、\`payload2___*\`、\`jitblk.bin\`、\`dllman.bin\`、图标 | \`04_scripts/绿梦/probes_20261028/\` | FairGuard cage 探针源码与构建输入（自写工具，按规范保留） |"
  echo "| \`E:\\_build\\lvmeng_mod\\StarryLvmeng.csproj\` 等 | \`04_scripts/绿梦/lvmeng_mod/\` | C# 注入工具工程 |"
  echo "| \`E:\\_build\\lvmeng\\lvmeng_v25.apk\`、\`lvmeng_v32.apk\` | \`08_output/绿梦/_import_20261028/\` | 现存候选包（v24 已不在磁盘，文档口径过期） |"
  echo "| \`E:\\_build\\lvmeng\\ft3.log\`、\`ft4.log\` | \`09_test/绿梦/_import_20261028/\` | 过程日志（不入库） |"
  echo "| \`E:\\_build\\dotnet\\\`（727 M，.NET SDK）、\`dotnet-install.ps1\` | \`01_tools/dotnet/sdk-9/\`、\`01_tools/dotnet/\` | 环境工具，非游戏产物；不属 01_tools 清理白名单，保留 |"
  echo
  echo "## 已删（可再生滚动物，逐条列出）"
  echo
  echo "| 相对路径 | 体积 |"
  echo "|---|---|"
  awk -F'\t' '{printf "| `%s` | %s |\n", $1, $2}' /tmp/_del_manifest.txt
  echo
  echo "## 保留 APK 身份"
  echo
  echo '| 体积 | 文件 | md5 |'
  echo '|---|---|---|'
  awk '{printf "| %s | `%s` | `%s` |\n", $1, $2, $3}' /tmp/_apk_id.txt
  echo
  echo "## 未做/待裁决"
  echo
  echo "- \`08_output/绿梦/_import_20261028/\` 仍有两个候选包：按规范「08_output 裁剪为唯一正式 APK」需用户「收尾+正式包+清理」口令，本轮未裁。"
  echo "- 绿梦文档里「当前交付候选 = \`E:\\_build\\lvmeng\\lvmeng_v24.apk\`」的口径已过期（v24 不在磁盘），本轮只把路径改成新位置，不替绿梦判正式包。"
} > "$MAN"
echo "清单 -> $MAN  ($(wc -l < "$MAN") 行)"
rm -f /tmp/_del_manifest.txt /tmp/_apk_id.txt
