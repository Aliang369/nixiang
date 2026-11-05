#!/usr/bin/env bash
# 无悔华夏 4399：收官清理（305 线）——只删本项目可再生大件，不碰 02_apks / 08_output。
#
# 保留：06_dumped/无悔华夏4399/runtime_<tag>/realdex/{biz_dex.json + 6 份业务 DEX + scan.log}
# 删除：同目录 pkg/base.odex.apk（1.6G，可从设备 lebian 重拉）、非业务 boot dex（039）、
#       03_decompiled/无悔华夏4399、obj_whrhx4399 构建中间对象。
# 删除一律先打印绝对路径再逐个 unlink（不用通配符），删后复扫一次。
set -euo pipefail

NIXIANG="${NIXIANG:-/Users/aliang/Documents/nixiang}"
TAG="${RUN_TAG:-20261105}"
GAME_DIR="$NIXIANG/06_dumped/无悔华夏4399"
RUN_DIR="$GAME_DIR/runtime_$TAG"
REALDEX="$RUN_DIR/realdex"

echo "== 保留清单（业务 DEX，来自 biz_dex.json）=="
[ -f "$REALDEX/biz_dex.json" ] || { echo "ERROR: 缺 biz_dex.json: $REALDEX" >&2; exit 1; }

echo "== 删除可再生的拆包/底包/中间对象（先打印再删）=="
python3.11 - "$REALDEX" "$NIXIANG" <<'PY'
import json, os, sys
from pathlib import Path
realdex = Path(sys.argv[1]); root = Path(sys.argv[2])
keep = {r['name'] for r in json.loads((realdex / 'biz_dex.json').read_text(encoding='utf-8'))}
targets = []
for p in sorted(realdex.glob('*.dex')):
    if p.name not in keep:
        targets.append(p)
for t in targets:
    print('  DEL', t.resolve())
    t.unlink()
print('  boot/非业务 dex 删除 %d 个，保留 %d 个业务 dex' % (len(targets), len(keep)))
PY

for p in "$RUN_DIR/pkg/base.odex.apk" "$RUN_DIR/pkg/base_odex.tar" \
         "$NIXIANG/03_decompiled/无悔华夏4399" \
         "$NIXIANG/mod菜单/starry-mod-menu/obj_whrhx4399"; do
  if [ -e "$p" ]; then
    echo "  DEL $p"
    find "$p" -depth -delete
  fi
done
# pkg 目录删空后去掉
[ -d "$RUN_DIR/pkg" ] && rmdir "$RUN_DIR/pkg" 2>/dev/null || true

echo "== 复扫：确认被删项已不存在 =="
for p in "$RUN_DIR/pkg/base.odex.apk" "$NIXIANG/03_decompiled/无悔华夏4399" "$NIXIANG/mod菜单/starry-mod-menu/obj_whrhx4399"; do
  [ -e "$p" ] && { echo "  !! 仍存在: $p" >&2; exit 1; } || echo "  ok gone: $p"
done
"$NIXIANG/04_scripts/_common/trash_paths.sh" --help >/dev/null 2>&1 || true

echo "== 收尾体量 =="
du -sh "$RUN_DIR" "$REALDEX" 2>/dev/null || true
ls -la "$REALDEX" | head -12
echo "CLEANUP_DONE"
