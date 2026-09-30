#!/usr/bin/env bash
# 收官交付物打包：唯一正式 APK、icon 512/144、菜单模板副本、可选裁剪清单。
# 默认 dry-run；--apply 才写入 delivery 目录和裁剪 08_output。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
GAME=""
APK=""
ICON_SOURCE=""
MENU_TEMPLATE=""
DATE="$(date +%Y%m%d)"
MODE="dry-run"
PRUNE=0

usage() {
  cat >&2 <<'EOF'
用法:
  bash 04_scripts/_common/finalize_game_delivery.sh \
    --game 潮汐守望者 \
    [--apk 08_output/潮汐守望者/xxx.apk] \
    [--icon-source /path/app_icon.png] \
    [--menu-template 10_docs/潮汐守望者/cxswz_menu_template.json] \
    [--date YYYYMMDD] \
    [--prune] \
    [--apply]

说明:
  - 默认 dry-run，只检查并打印将生成的交付路径。
  - --apply 才创建 08_output/{游戏}/delivery_{日期}/。
  - --prune 才调用 prune_output_final.sh 裁剪 08_output/{游戏} 为唯一 APK。
  - icon 源图存在时生成 512/144 PNG；菜单模板会做字段严格自检并复制一份。
EOF
  exit 2
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --game) GAME="${2:-}"; shift 2 ;;
    --apk) APK="${2:-}"; shift 2 ;;
    --icon-source) ICON_SOURCE="${2:-}"; shift 2 ;;
    --menu-template) MENU_TEMPLATE="${2:-}"; shift 2 ;;
    --date) DATE="${2:-}"; shift 2 ;;
    --prune) PRUNE=1; shift ;;
    --apply) MODE="apply"; shift ;;
    -h|--help) usage ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage ;;
  esac
done

[[ -n "$GAME" ]] || usage
[[ "$DATE" =~ ^[0-9]{8}$ ]] || { echo "ERROR: --date 需为 YYYYMMDD" >&2; exit 2; }
GAME_DIR="$ROOT/08_output/$GAME"
[[ -d "$GAME_DIR" ]] || { echo "ERROR: 输出目录不存在: $GAME_DIR" >&2; exit 1; }

abs_path() {
  local p="$1"
  if [[ "$p" == /* ]]; then printf '%s\n' "$p"; else printf '%s\n' "$ROOT/$p"; fi
}

md5_of() {
  if command -v md5 >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | awk '{print $1}'; fi
}
sha256_of() {
  if command -v shasum >/dev/null 2>&1; then shasum -a 256 "$1" | awk '{print $1}'; else sha256sum "$1" | awk '{print $1}'; fi
}

if [[ -z "$APK" ]]; then
  apks=()
  while IFS= read -r -d '' f; do apks+=("$f"); done < <(find "$GAME_DIR" -maxdepth 1 -type f -name '*.apk' -print0)
  [[ "${#apks[@]}" -eq 1 ]] || {
    echo "ERROR: $GAME_DIR 顶层 APK 数=${#apks[@]}，请显式传 --apk" >&2
    exit 1
  }
  APK="${apks[0]}"
fi
APK="$(abs_path "$APK")"
[[ -f "$APK" ]] || { echo "ERROR: APK 不存在: $APK" >&2; exit 1; }
case "$APK" in
  "$GAME_DIR"/*) ;;
  *) echo "ERROR: 正式包必须在 $GAME_DIR 下: $APK" >&2; exit 1 ;;
esac
[[ "$APK" == *.apk ]] || { echo "ERROR: 正式包必须是 .apk: $APK" >&2; exit 1; }

APKSIGNER="${APKSIGNER:-}"
if [[ -z "$APKSIGNER" ]]; then
  for v in 36.0.0 35.0.0 34.0.0; do
    if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
      APKSIGNER="$HOME/Library/Android/sdk/build-tools/$v/apksigner"
      break
    fi
  done
fi
[[ -x "$APKSIGNER" ]] || { echo "ERROR: 未找到 apksigner，拒绝交付: ${APKSIGNER:-未检测到}" >&2; exit 1; }
"$APKSIGNER" verify "$APK" >/dev/null || { echo "ERROR: 正式包签名校验失败: $APK" >&2; exit 1; }

if [[ -z "$MENU_TEMPLATE" ]]; then
  menus=()
  while IFS= read -r -d '' f; do menus+=("$f"); done < <(find "$ROOT/10_docs/$GAME" -maxdepth 1 -type f -name '*_menu_template.json' -print0 2>/dev/null)
  [[ "${#menus[@]}" -eq 1 ]] || {
    echo "ERROR: 未找到唯一菜单模板，请显式传 --menu-template（当前命中 ${#menus[@]} 个）" >&2
    exit 1
  }
  MENU_TEMPLATE="${menus[0]}"
fi
MENU_TEMPLATE="$(abs_path "$MENU_TEMPLATE")"
[[ -f "$MENU_TEMPLATE" ]] || { echo "ERROR: 菜单模板不存在: $MENU_TEMPLATE" >&2; exit 1; }

if [[ -n "$ICON_SOURCE" ]]; then
  ICON_SOURCE="$(abs_path "$ICON_SOURCE")"
  [[ -f "$ICON_SOURCE" ]] || { echo "ERROR: icon 源图不存在: $ICON_SOURCE" >&2; exit 1; }
  command -v sips >/dev/null 2>&1 || { echo "ERROR: 需要 sips 生成 icon" >&2; exit 1; }
fi

python3 - "$MENU_TEMPLATE" <<'PY'
import json, sys
p = sys.argv[1]
d = json.load(open(p, encoding='utf-8'))
assert set(d) == {'template_id', 'name', 'package', 'source', 'features'}, sorted(d)
assert all(set(x) == {'feat_num', 'name'} for x in d['features'])
print('menu_template OK', d['template_id'], [x['name'] for x in d['features']])
PY

DELIVERY="$GAME_DIR/delivery_$DATE"
echo "[finalize] game=$GAME"
echo "[finalize] apk=$APK"
echo "[finalize] apk_md5=$(md5_of "$APK")"
echo "[finalize] apk_sha256=$(sha256_of "$APK")"
echo "[finalize] menu_template=$MENU_TEMPLATE"
echo "[finalize] delivery=$DELIVERY"
if [[ -n "$ICON_SOURCE" ]]; then
  echo "[finalize] icon_source=$ICON_SOURCE"
  echo "[finalize] icon_out=icon_${GAME}_512.png, icon_${GAME}_144.png"
else
  echo "[finalize] icon_source=(未提供，跳过 icon 生成)"
fi

if [[ "$MODE" != "apply" ]]; then
  echo "[finalize] dry-run：未写文件。确认后加 --apply。"
  exit 0
fi

mkdir -p "$DELIVERY"
if [[ -n "$ICON_SOURCE" ]]; then
  sips -z 512 512 "$ICON_SOURCE" --out "$DELIVERY/icon_${GAME}_512.png" >/dev/null
  sips -z 144 144 "$ICON_SOURCE" --out "$DELIVERY/icon_${GAME}_144.png" >/dev/null
fi
cp -f "$MENU_TEMPLATE" "$DELIVERY/menu_template.json"

if [[ "$PRUNE" == "1" ]]; then
  MANIFEST="$ROOT/10_docs/$GAME/正式包裁剪清单_${DATE}.json"
  bash "$ROOT/04_scripts/_common/prune_output_final.sh" "$GAME_DIR" "$(basename "$APK")" --apply --manifest "$MANIFEST"
fi

echo "[finalize] done"
find "$DELIVERY" -maxdepth 1 -type f -print | sort
