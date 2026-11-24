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
  echo "  清单契约: --manifest 写的是 prune 清单（键 removed:[{name,size,md5}]），供" >&2
  echo "            pipeline_record_cleanup.py / check_state_hash_sync.py 消费；与" >&2
  echo "            trash_paths.sh 的清理清单（--path 列表）互不通用。" >&2
  echo "  默认 dry-run 只打印候选；--apply 才删除（删前校验签名与 hash）。" >&2
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
# Win 上 apksigner 只有 apksigner.bat（`[ -x ]` 对 .bat 恒假），SDK 默认根在 %LOCALAPPDATA%。
# 旧写法只探 mac 路径 + `[ -x ]` ⇒ Windows 上收尾裁剪恒报「未找到 apksigner，拒绝裁剪」。
# 统一复用 _common/env.sh 的探测（NIXIANG_APKSIGNER 已处理 .bat 与 win 根），再做本地兜底。
if [[ -z "$APKSIGNER" && -f "$ROOT/04_scripts/_common/env.sh" ]]; then
  # shellcheck disable=SC1091
  . "$ROOT/04_scripts/_common/env.sh" >/dev/null 2>&1 || true
  APKSIGNER="${NIXIANG_APKSIGNER:-}"
fi
if [[ -z "$APKSIGNER" ]]; then
  for v in 36.1.0 36.0.0 35.0.0 34.0.0; do
    for c in \
      "$HOME/Library/Android/sdk/build-tools/$v/apksigner" \
      "${ANDROID_HOME:-${ANDROID_SDK_ROOT:-${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk}}/build-tools/$v/apksigner" \
      "${ANDROID_HOME:-${ANDROID_SDK_ROOT:-${LOCALAPPDATA:-$HOME/AppData/Local}/Android/Sdk}}/build-tools/$v/apksigner.bat"; do
      if [[ -f "$c" ]]; then APKSIGNER="$c"; break 2; fi
    done
  done
fi
[[ -f "$APKSIGNER" ]] || { echo "ERROR: 未找到 apksigner，拒绝裁剪: ${APKSIGNER:-未检测到}" >&2; exit 1; }
# apksigner(.bat) 是 java 程序，argv 里的路径必须 Windows 形态（同 finalize_game_delivery.sh:111）
if command -v cygpath >/dev/null 2>&1; then
  APKSIGNER="$(cygpath -m "$APKSIGNER" 2>/dev/null || printf '%s' "$APKSIGNER")"
  KEEP_APK="$(cygpath -m "$KEEP" 2>/dev/null || printf '%s' "$KEEP")"
else
  KEEP_APK="$KEEP"
fi
"$APKSIGNER" verify "$KEEP_APK" >/dev/null || { echo "ERROR: 保留 APK 签名校验失败: $KEEP" >&2; exit 1; }

# 跨平台文件大小 / md5：GNU（Linux / Git Bash）是 stat -c '%s' / md5sum；macOS BSD 是 stat -f '%z' / md5 -q。
# 顺序必须 GNU 优先（2026-11-24 修）：Git Bash 的 GNU stat 对 `-f '%z'` 先吐 5 行文件系统信息且 rc=1，
#   多行已经进了 stdout，`||` 救不回来 ⇒ 清单行被撑成多行 ⇒ 下游 python 报
#   ValueError: not enough values to unpack（2026-11-24 旷野征途正式包裁剪实测，靠临时 stat 垫片才跑通）。
#   实测（本机 Git Bash 5.2 + GNU coreutils 8.32）：
#     stat -f '%z' preflight.sh → rc=1，stdout 5 行（File:/ID:/Block size:/Blocks:/Inodes:）
#     stat -c '%s' preflight.sh → rc=0，单行 27615
#   macOS 上 stat -c 立即失败且不写 stdout（用法错到 stderr），回退 BSD 分支安全。
_file_size() { stat -c '%s' "$1" 2>/dev/null || stat -f '%z' "$1"; }
# md5 顺序保持不动（2026-11-24 实测）：本机无 md5 可执行文件，`md5 -q` 由 .nx_shims.sh 的 shell 函数代理为
#   `md5sum | awk '{print $1}'`，输出干净 32 位 hex（910791d8ebb0b0ccfe695b8992064923，与 `md5sum | awk` 逐字一致）；
#   裸环境没有该函数时直接失败并回退 md5sum，同样是 32 位 hex，两种环境都不需要调顺序。
_file_md5()  { md5 -q "$1" 2>/dev/null || md5sum "$1" | awk '{print $1}'; }

TMP="$(mktemp /tmp/prune_output.XXXXXX)"
trap 'rm -f "$TMP"' EXIT
while IFS= read -r -d '' f; do
  [[ "$f" == "$KEEP" ]] && continue
  printf '%s\t%s\t%s\n' "$(basename "$f")" "$(_file_size "$f")" "$(_file_md5 "$f")" >> "$TMP"
done < <(find "$GAME_DIR" -maxdepth 1 -type f \( -name '*.apk' -o -name '*.apk.part' -o -name '*.idsig' -o -name '*.idsig.part' \) -print0)

if [[ -n "$MANIFEST" ]]; then
  [[ "$MANIFEST" == /* ]] || MANIFEST="$ROOT/$MANIFEST"
  mkdir -p "$(dirname "$MANIFEST")"
PY="${PYTHON:-}"
if [[ -z "$PY" ]]; then
  for c in python3.11 python3 python py; do
    command -v "$c" >/dev/null 2>&1 && { PY="$c"; break; }
  done
fi
[[ -n "$PY" ]] || { echo "ERROR: 找不到 python，无法写裁剪清单（可设 PYTHON=/path/to/python）" >&2; exit 1; }
"$PY" - "$MANIFEST" "$GAME_DIR" "$KEEP" "$MODE" "$TMP" <<'PYEOF'
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
PYEOF
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
