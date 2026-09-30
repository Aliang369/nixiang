#!/usr/bin/env bash
# 将 08_output/<游戏> 裁剪为唯一正式 APK。
# 默认 dry-run；传 --apply 才删除其它 APK / .apk.part / .idsig 文件。
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
GAME_DIR="${1:-}"
KEEP_INPUT="${2:-}"
MODE="${3:-dry-run}"
MANIFEST=""
if [[ "${4:-}" == "--manifest" ]]; then
  MANIFEST="${5:-}"
fi

usage() {
  echo "用法: bash 04_scripts/_common/prune_output_final.sh <游戏输出目录> <保留APK> [--apply] [--manifest 文件]" >&2
  exit 2
}

[[ -n "$GAME_DIR" && -n "$KEEP_INPUT" ]] || usage
[[ "$MODE" == "dry-run" || "$MODE" == "--apply" ]] || usage

if [[ "$GAME_DIR" != /* ]]; then
  GAME_DIR="$ROOT/$GAME_DIR"
fi
[[ -d "$GAME_DIR" ]] || { echo "ERROR: 输出目录不存在: $GAME_DIR" >&2; exit 1; }
GAME_DIR="$(cd "$GAME_DIR" && pwd -P)"
case "$GAME_DIR" in
  "$ROOT"/08_output/*) ;;
  *) echo "ERROR: 只允许处理 08_output 下的游戏目录: $GAME_DIR" >&2; exit 1 ;;
esac

if [[ "$KEEP_INPUT" == /* ]]; then
  KEEP="$KEEP_INPUT"
else
  if [[ -f "$KEEP_INPUT" ]]; then
    KEEP="$(cd "$(dirname "$KEEP_INPUT")" && pwd -P)/$(basename "$KEEP_INPUT")"
  else
    KEEP="$GAME_DIR/$KEEP_INPUT"
  fi
fi
[[ -f "$KEEP" ]] || { echo "ERROR: 保留 APK 不存在: $KEEP" >&2; exit 1; }
KEEP="$(cd "$(dirname "$KEEP")" && pwd -P)/$(basename "$KEEP")"
case "$KEEP" in
  "$GAME_DIR"/*) ;;
  *) echo "ERROR: 保留 APK 必须位于目标目录: $KEEP" >&2; exit 1 ;;
esac
[[ "$KEEP" == *.apk ]] || { echo "ERROR: 保留文件必须是 .apk: $KEEP" >&2; exit 1; }

APKSIGNER="${APKSIGNER:-}"
if [[ -z "${APKSIGNER:-}" ]]; then
  for v in 36.0.0 35.0.0 34.0.0; do
    if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]]; then
      APKSIGNER="$HOME/Library/Android/sdk/build-tools/$v/apksigner"
      break
    fi
  done
fi
[[ -x "$APKSIGNER" ]] || { echo "ERROR: 未找到 apksigner，拒绝裁剪: ${APKSIGNER:-未检测到}" >&2; exit 1; }
"$APKSIGNER" verify "$KEEP" >/dev/null || { echo "ERROR: 保留 APK 签名校验失败: $KEEP" >&2; exit 1; }

TMP="$(mktemp /tmp/prune_output.XXXXXX)"
trap 'rm -f "$TMP"' EXIT
while IFS= read -r -d '' f; do
  [[ "$f" == "$KEEP" ]] && continue
  printf '%s\t%s\t%s\n' "$(basename "$f")" "$(stat -f '%z' "$f")" "$(md5 -q "$f")" >> "$TMP"
done < <(find "$GAME_DIR" -maxdepth 1 -type f \( -name '*.apk' -o -name '*.apk.part' -o -name '*.idsig' -o -name '*.idsig.part' \) -print0)

if [[ -n "$MANIFEST" ]]; then
  [[ "$MANIFEST" == /* ]] || MANIFEST="$ROOT/$MANIFEST"
  mkdir -p "$(dirname "$MANIFEST")"
  python3.11 - "$MANIFEST" "$GAME_DIR" "$KEEP" "$MODE" "$TMP" <<'PY'
import hashlib
import json
import sys
from pathlib import Path
manifest, game_dir, keep, mode, listing = sys.argv[1:]
removed = []
for line in Path(listing).read_text(encoding='utf-8').splitlines():
    if not line.strip():
        continue
    name, size, md5 = line.split('\t')
    removed.append({'name': name, 'size': int(size), 'md5': md5})
keep_bytes = Path(keep).read_bytes()
data = {
    'game_dir': game_dir,
    'keep': Path(keep).name,
    'keep_size': len(keep_bytes),
    'keep_md5': hashlib.md5(keep_bytes).hexdigest(),
    'mode': mode,
    'removed': removed,
    'removed_count': len(removed),
}
Path(manifest).write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
PY
fi

if [[ "$MODE" == "--apply" ]]; then
  while IFS= read -r -d '' f; do
    [[ "$f" == "$KEEP" ]] && continue
    rm -f -- "$f"
  done < <(find "$GAME_DIR" -maxdepth 1 -type f \( -name '*.apk' -o -name '*.apk.part' -o -name '*.idsig' -o -name '*.idsig.part' \) -print0)
else
  echo "[dry-run] 将删除以下包文件，保留 $(basename "$KEEP"):"
  cut -f1 "$TMP"
fi

LEFT="$(find "$GAME_DIR" -maxdepth 1 -type f \( -name '*.apk' -o -name '*.apk.part' -o -name '*.idsig' -o -name '*.idsig.part' \) | wc -l | tr -d ' ')"
if [[ "$MODE" == "--apply" ]]; then
  [[ "$LEFT" == "1" ]] || { echo "ERROR: 裁剪后包文件数量=${LEFT}，期望=1" >&2; exit 1; }
fi
echo "[prune_output] keep=$(basename "$KEEP") removed=$(wc -l < "$TMP" | tr -d ' ') mode=$MODE"
if [[ -n "$MANIFEST" ]]; then echo "[prune_output] manifest=$MANIFEST"; fi
