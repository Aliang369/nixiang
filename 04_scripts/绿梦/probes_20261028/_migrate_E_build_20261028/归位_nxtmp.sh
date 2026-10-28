#!/usr/bin/env bash
# E:\nxtmp（6.2M，早期会话的 IL2CPP 速度/移动分析笔记）按「产物不出工作区」归位到 99_archive，附来源说明
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
DST="99_archive/盘外归位_20261028/nxtmp"
mkdir -p "$(dirname "$DST")"

if [ -d /e/nxtmp ]; then
  mv -f /e/nxtmp "$DST"
  echo "已归位: $DST  ($(du -sh "$DST" | cut -f1))"
else
  echo "(!) E:\\nxtmp 不存在"
fi

cat > 99_archive/盘外归位_20261028/README.md <<'EOF'
# 盘外产物归位（2026-10-28）

用户当轮指令：**所有产物必须落在工作区内，不得留在 C/D/E 盘其他地方**；本轮把盘外散件按「能归位就归位、可再生才删」处理。

| 原位置 | 现位置 | 性质 | 说明 |
|---|---|---|---|
| `E:\_build`（6.4 G） | 见 `10_docs/绿梦/_import_E_build_清理清单_20261028.md` | 绿梦(lvmeng) 构建区 | 已按清理规范分流：源码→`04_scripts/绿梦/`、候选包→`08_output/绿梦/_import_20261028/`、.NET SDK→`01_tools/dotnet/`；可再生滚动物 ≈5.6 G 已删 |
| `E:\nxtmp`（6.2 M） | `99_archive/盘外归位_20261028/nxtmp/` | 早期会话的 IL2CPP 速度/移动分析笔记与 python 切片脚本 | **归属游戏未确认**（内容为 `speed/movement/kinematic/loco/setspeed` 类检索笔记与 `find_magic.py`/`slice.py`/`kw_strings.py` 等通用探针），故不并进任何游戏目录，先整体归档；确认后可下沉到 `04_scripts/{游戏}/probes_*/` |
| `E:\_tmp_cage2.py` | `04_scripts/绿梦/probes_20261028/_migrate_E_build_20261028/` | 一次性探针（改 `fg_cage.c` 扫栈定位退出点） | 按规范「一次性探针也保留」 |
| `E:\nixiang_clone.log` | — | clone 日志 | 可再生，已删 |
| `%TEMP%\cstjjs_*.txt`、`nixiang_*.sh/.py`、`tmp*/mod菜单`×40、`*_build/`、`*_redteam/`、`zzz_sym_*`、starry prefs xml | — | 临时脚本/构建残留/取证文本 | 可再生，已删（清单见 `09_test/盘外产物清理_20261028.txt`） |

**未动（非我方产物）**：`E:\27e838e5…`（6.1 G HLS 视频流缓存）、`E:\123pan`/`BaiduNetdiskDownload`/`MCLDownload`/`ldyun`/`leidian`（网盘与模拟器目录）、`C:\Windows` 系系统 dump 文件、`D:\Python313\…\frida`（已装工具）、`D:\Program Files\IDA Pro`。
EOF

echo "README -> 99_archive/盘外归位_20261028/README.md"
echo "== E 盘根复查（我方产物应已归零）=="
ls -1 /e | head -16
