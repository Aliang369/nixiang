#!/usr/bin/env bash
# ================================================================
#  so_hotswap_mrfzz.sh — 明日方舟：so 热替换入口（只改 so 的轮次，秒级迭代）
#
#  用法:
#    bash 04_scripts/明日方舟/so_hotswap_mrfzz.sh [--restart] [--dry-run] [--no-judge]
#    bash 04_scripts/明日方舟/so_hotswap_mrfzz.sh --restore      # 从唯一正式包还原 so
#
#  默认取:
#    serial/pkg  ← 04_scripts/明日方舟/official_apk.json（package / serial 或 device_id.json）
#    so          ← mod菜单/starry-mod-menu/libs_mrfzz/arm64-v8a/libstarry_magic.so（build_so 产物）
#    judge       ← 04_scripts/明日方舟/实测判读_mrfzz.sh
#
#  硬边界见 04_scripts/_common/so_hotswap.sh 头注释（不改 APK 本体；交付轮必须回完整链路）。
#  bash 3.2 兼容
# ================================================================
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
HOTSWAP="${ROOT}/04_scripts/_common/so_hotswap.sh"
JUDGE="${SCRIPT_DIR}/实测判读_mrfzz.sh"
SO="${ROOT}/mod菜单/starry-mod-menu/libs_mrfzz/arm64-v8a/libstarry_magic.so"
APK_JSON="${SCRIPT_DIR}/official_apk.json"
DEV_JSON="${SCRIPT_DIR}/device_info.json"

RESTART=0; DRY=0; NOJUDGE=0; RESTORE=0
while [ $# -gt 0 ]; do
  case "$1" in
    --restart)  RESTART=1; shift ;;
    --dry-run)  DRY=1; shift ;;
    --no-judge) NOJUDGE=1; shift ;;
    --restore)  RESTORE=1; shift ;;
    -h|--help)  sed -n '2,20p' "$0"; exit 0 ;;
    *) echo "✗ 未知参数: $1" >&2; exit 2 ;;
  esac
done

[ -f "$HOTSWAP" ] || { echo "✗ 缺少 $HOTSWAP" >&2; exit 1; }

SERIAL=""; PKG=""
if [ -f "$APK_JSON" ]; then
  SERIAL="$(python3 -c "import json,sys;d=json.load(open('$APK_JSON'));print(d.get('serial',''))" 2>/dev/null || true)"
  PKG="$(python3 -c "import json,sys;d=json.load(open('$APK_JSON'));print(d.get('package',''))" 2>/dev/null || true)"
fi
if [ -z "$SERIAL" ] && [ -f "$DEV_JSON" ]; then
  SERIAL="$(python3 -c "import json,sys;d=json.load(open('$DEV_JSON'));print(d.get('serial',''))" 2>/dev/null || true)"
fi
[ -n "$SERIAL" ] || SERIAL="f9a116a2"
[ -n "$PKG" ] || PKG="com.hypergryph.arknights"

ARGS="--serial $SERIAL --pkg $PKG --lib-name libstarry_magic.so"
if [ "$RESTART" = "1" ]; then ARGS="$ARGS --restart"; fi
if [ "$DRY" = "1" ]; then ARGS="$ARGS --dry-run"; fi

if [ "$RESTORE" = "1" ]; then
  APK="$(ls -t "${ROOT}"/08_output/明日方舟/*releaseSigned.apk 2>/dev/null | head -1)"
  [ -n "$APK" ] || { echo "✗ 找不到 08_output/明日方舟/*releaseSigned.apk（正式包）" >&2; exit 1; }
  echo "[还原] 从正式包抽 so: $APK"
  # shellcheck disable=SC2086
  exec bash "$HOTSWAP" $ARGS --restore-from "$APK"
fi

if [ ! -f "$SO" ]; then
  # build_so 产物已被收尾清理时：--dry-run 允许用正式包内的 so 做适用性核验（只读，不落地）
  if [ "$DRY" = "1" ]; then
    APK_FB="$(ls -t "${ROOT}"/08_output/明日方舟/*releaseSigned.apk 2>/dev/null | head -1)"
    if [ -n "$APK_FB" ]; then
      SO_FB="/tmp/_hs_from_apk_$$.so"
      unzip -p "$APK_FB" 'lib/arm64-v8a/libstarry_magic.so' > "$SO_FB" 2>/dev/null || true
      if [ -s "$SO_FB" ]; then
        echo "[提示] libs_mrfzz 不存在（已收尾清理）⇒ 本次用正式包内 so 做适用性核验"
        SO="$SO_FB"
      fi
    fi
  fi
fi
[ -f "$SO" ] || {
  echo "✗ 找不到 so: $SO" >&2
  echo "  先跑：bash 04_scripts/pipeline.sh 明日方舟 run build_so" >&2
  exit 1
}
if [ "$NOJUDGE" != "1" ]; then ARGS="$ARGS --judge $JUDGE"; fi
# shellcheck disable=SC2086
exec bash "$HOTSWAP" $ARGS --so "$SO"
