#!/usr/bin/env bash
# 阶段6：核对 payload2 去向、补全清单、跑机器关
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
BI="04_scripts/绿梦/build_inputs_20261028"
PR="04_scripts/绿梦/probes_20261028"
MAN="10_docs/绿梦/_import_E_build_清理清单_20261028.md"

echo "==== payload2 去向 ===="
find . -name 'payload2___data' -o -name 'payload2___info' 2>/dev/null | head -5
echo "（空 = 已在缩容时按「无扩展名=编译产物」规则删除；该文件由 make_payload.sh 从源码重出）"

echo
echo "==== 最终落点内容清点 ===="
echo "-- probes_20261028（源码/脚本/笔记）: $(ls -1 "$PR" | wc -l) 个文件, $(du -sh "$PR" | cut -f1)"
ls -1 "$PR" | head -12
echo "-- build_inputs_20261028: $(du -sh "$BI" | cut -f1)"
ls -1 "$BI"
echo "-- 08_output/绿梦/_import_20261028: $(du -sh 08_output/绿梦/_import_20261028 | cut -f1)"
ls -1 08_output/绿梦/_import_20261028

echo
echo "==== 补全清单尾部 ===="
{
  echo
  echo "## 最终落点与体积（2026-10-28）"
  echo
  echo "| 落点 | 体积 | 内容 |"
  echo "|---|---|---|"
  echo "| \`04_scripts/绿梦/probes_20261028/\` | $(du -sh "$PR" | cut -f1) | FairGuard/cage 探针源码（*.c/*.py/*.sh）、地址与笔记 txt、\`assemble.py\`、\`patch_*.py\`、图标 |"
  echo "| \`04_scripts/绿梦/build_inputs_20261028/\` | $(du -sh "$BI" | cut -f1) | \`assemble.py\` 直接引用的构建输入：\`smali_in/\`、\`lib/arm64-v8a/*.so\`、\`classes4.dex\`(+3 bak)、\`classes2_patched.dex\`、\`jitblk.bin\`、\`dllman.bin\` |"
  echo "| \`04_scripts/绿梦/ilpatch/ilpatch.win.csproj\` | 617 B | Windows 变体工程（net8.0 + 本地 Mono.Cecil） |"
  echo "| \`08_output/绿梦/_import_20261028/\` | $(du -sh 08_output/绿梦/_import_20261028 | cut -f1) | \`lvmeng_v25.apk\`(370,704,022 B, md5 \`fdda9d83f1669fb376a7d2fe2ea4a8d4\`)、\`lvmeng_v32.apk\`(371,105,430 B, md5 \`775f8017babcd182a586d3c8492c27a7\`) |"
  echo "| \`09_test/绿梦/_import_20261028/\` | 3.7 M | \`ft3.log\`/\`ft4.log\` 过程日志 |"
  echo "| \`01_tools/dotnet/\` | 728 M | \`.NET SDK\`（\`sdk-9/\`）与 \`dotnet-install.ps1\`，环境工具 |"
  echo
  echo "**缩容结果**：\`E:\\_build\` 原 6.4 G → 工作区保留 ≈ 0.75 G（dotnet 728 M 属环境工具 + 候选包 708 M），删掉的可再生滚动物 ≈ 5.6 G（33 个 .idsig、7 个反编译/解包目录、11 个候选 APK、整份 mod 工程副本 144 M、ilpatch 构建输出、内存 dump 与设备库）。"
  echo
  echo "**注**：\`payload2___data\`（13 M）在「无扩展名＝编译产物」规则下被删；它由 \`make_payload.sh\` 从源码+官方 dll 重出，属可再生（规范 §200「热更样本/补丁产物可删」）。"
} >> "$MAN"
echo "清单 -> $MAN ($(wc -l < "$MAN") 行)"

echo
echo "==== 机器关 ===="
PYTHONUTF8=1 PYTHONIOENCODING=utf-8 bash 04_scripts/_common/preflight.sh \
  04_scripts/长生天机降世/build_迁移.sh \
  04_scripts/长生天机降世/build_缩容.sh \
  04_scripts/长生天机降世/build_收尾.sh \
  04_scripts/长生天机降世/build_归档.sh \
  04_scripts/长生天机降世/build_阶段5.sh \
  04_scripts/长生天机降世/build_阶段6.sh \
  04_scripts/长生天机降世/build_清单.sh \
  04_scripts/长生天机降世/build_迁移前核对.sh 2>&1 | tail -12
