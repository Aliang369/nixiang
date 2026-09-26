#!/usr/bin/env bash
# 正中靶心2 官方 1.9：确认星光 JS 补丁依赖的类/方法名仍存在。
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
VERSION_NAME="$(python3 - "$SCRIPT_DIR/official_apk.json" <<'PY'
import json,sys
print(json.load(open(sys.argv[1],encoding='utf-8'))['version_name'])
PY
)"
JS="$NIXIANG_ROOT/06_dumped/正中靶心2/official_${VERSION_NAME}/decrypted_js/src/assets/Script/src/GameLogic.js"
OUT="$SCRIPT_DIR/official_anchors.json"
[[ -f "$JS" ]] || { echo "错误: 缺少 $JS" >&2; exit 1; }
python3 - "$JS" "$OUT" <<'PY'
import json,sys
from pathlib import Path
js=Path(sys.argv[1]).read_text(encoding='utf-8')
needles=['BaseObjectComponent','getBuffCanBeAttack','changeHp','DamageType','EntitySide','GameWorld','gameSpeed',
         'AdvertMgr','adsPlay','startAdverView','finishAdverViewed','C2S_WATCH_ADVERT',
         # 1.9 功能3 改版依赖：游戏内战斗加速的月卡门
         'ShopVipMgr','outStatusAdvip','setOutStatusVip','_outStatusAdvip','VipType',
         'SpeedUpBtn','_speedTyep','setSpeed','getSpeed','SPEED_BTN','BattleDataKey','BattleGameType','AtOnce']
res={n:(n in js) for n in needles}
missing=[n for n,v in res.items() if not v]
Path(sys.argv[2]).write_text(json.dumps({'source':sys.argv[1],'needles':res,'missing':missing,'ready':not missing},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'missing':missing,'ready':not missing},ensure_ascii=False))
if missing: raise SystemExit(1)
PY
