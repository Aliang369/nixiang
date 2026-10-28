#!/usr/bin/env bash
# 阶段2：按清理规范缩减（可再生滚动物删、源码与文档留、正式候选入 08_output）
# 规范依据：10_docs/tools/05_目录结构脚本环境与交付清理.md「清理规则」「收官归档清单」
set -uo pipefail
cd /e/nixiang || exit 1
export LC_ALL=C.UTF-8
SRC="07_patched/绿梦/_import_E_build_20261028"
MAN="10_docs/绿梦/_import_E_build_清理清单_20261028.md"
PROBE="04_scripts/绿梦/probes_20261028"
OUTAPK="08_output/绿梦/_import_20261028"
LOGD="09_test/绿梦/_import_20261028"
mkdir -p "$PROBE" "$OUTAPK" "$LOGD"

: > /tmp/_del_manifest.txt
del() { # $1=path
  if [ -e "$1" ]; then
    sz=$(du -sk "$1" 2>/dev/null | cut -f1)
    printf '%s\t%s KB\n' "${1#07_patched/绿梦/_import_E_build_20261028/}" "$sz" >> /tmp/_del_manifest.txt
    rm -rf -- "$1"
  fi
}

echo "==== [1] 删 .idsig（重新签名即得，33 个）===="
for f in "$SRC"/lvmeng/*.idsig; do [ -e "$f" ] && del "$f"; done

echo "==== [2] 删反编译/解包中间物（03_decompiled 类滚动物）===="
for d in orig_dec osmali1 osmali2 osmali3 official_dex starry_dec assets; do
  del "$SRC/lvmeng/$d"
done

echo "==== [3] 删已编译探针二进制（.c 源码保留）===="
for f in "$SRC"/lvmeng/*; do
  [ -f "$f" ] || continue
  bn=$(basename "$f")
  case "$bn" in
    *.*) continue ;;            # 有扩展名的一律先跳过
  esac
  del "$f"
done

echo "==== [4] 删 E 盘整份 mod 工程副本（与工作区 mod菜单 同名 200 文件、E 盘无独有）===="
del "$SRC/starry"

echo "==== [5] 删 ilpatch 构建输出与补丁 dll（源码已在 04_scripts/绿梦/ilpatch）===="
for f in bin obj Assembly-ACT.p2.dll rt.dll; do del "$SRC/ilpatch/$f"; done

echo "==== [6] 删内存 dump / 设备侧拉取库（可重取）===="
for f in fg_dump.bin fg_after.bin fg_live.bin host_libc.so host_houdini.so libtapsdkcore.so; do
  del "$SRC/lvmeng/$f"
done

echo "==== [7] 保留正式候选 v25 / v32，其余候选 APK 删（assemble.py + 源码可重出）===="
for f in "$SRC"/lvmeng/*.apk; do
  [ -e "$f" ] || continue
  bn=$(basename "$f")
  case "$bn" in
    lvmeng_v25.apk|lvmeng_v32.apk) mv -f -- "$f" "$OUTAPK/" ;;
    *) del "$f" ;;
  esac
done

echo "==== [8] 过程日志移到 09_test ===="
for f in "$SRC"/lvmeng/ft3.log "$SRC"/lvmeng/ft4.log; do
  [ -e "$f" ] && mv -f -- "$f" "$LOGD/"
done

echo "==== [9] 探针源码/笔记/构建输入移到 04_scripts/绿梦/probes_20261028 ===="
for f in "$SRC"/lvmeng/*; do
  [ -f "$f" ] || continue
  bn=$(basename "$f")
  case "$bn" in
    *.apk|*.idsig) continue ;;
  esac
  mv -f -- "$f" "$PROBE/"
done
# 空目录清理
rmdir "$SRC/lvmeng" 2>/dev/null || echo "  (lvmeng 仍有残留，稍后复查)"

echo "==== [10] lvmeng_mod（C# 注入工具工程）移入 04_scripts/绿梦/lvmeng_mod ===="
mkdir -p 04_scripts/绿梦/lvmeng_mod
for f in "$SRC"/lvmeng_mod/*; do
  [ -e "$f" ] || continue
  mv -f -- "$f" 04_scripts/绿梦/lvmeng_mod/
done
rmdir "$SRC/lvmeng_mod" 2>/dev/null || true

echo "==== [11] dotnet SDK（环境工具，非游戏产物）移入 01_tools/dotnet ===="
mkdir -p 01_tools/dotnet
if [ -d "$SRC/dotnet" ]; then mv -f -- "$SRC/dotnet" 01_tools/dotnet/sdk-9 ; fi
[ -f "$SRC/dotnet-install.ps1" ] && mv -f -- "$SRC/dotnet-install.ps1" 01_tools/dotnet/

echo "==== [12] 复查剩余 ===="
if [ -d "$SRC" ]; then
  find "$SRC" | head -20
  du -sh "$SRC"
fi
echo "== 各落点体积 =="
du -sh "$PROBE" "$OUTAPK" "$LOGD" 01_tools/dotnet 2>/dev/null
