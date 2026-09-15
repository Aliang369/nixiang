#!/usr/bin/env bash
# 勇士之心：只读导出斌哥壳运行配置，以及运行时热更 JS/顶层配置。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="${PKG:-com.fightingstars.games.warrior}"
ADB_BIN="${ADB_BIN:-adb}"
OUT_DIR="$NIXIANG_ROOT/09_test/勇士之心/runtime"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

REMOTE_ROOT="/data/data/$PKG/files"
REMOTE_SW="$REMOTE_ROOT/mod_sw.txt"

rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR/res"

"$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -laR \"$REMOTE_ROOT\"'" | tr -d '\r' > "$OUT_DIR/files_listing.txt"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'find \"$REMOTE_ROOT\" -maxdepth 8 -type f'" | tr -d '\r' | sort > "$OUT_DIR/files_paths.txt"

"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat \"$REMOTE_SW\"'" > "$OUT_DIR/mod_sw.txt"
[[ -s "$OUT_DIR/mod_sw.txt" ]] || { echo "错误: 未读取到 mod_sw.txt" >&2; exit 1; }

BRIDGE_COUNT=0
JS_COUNT=0
CONFIG_COUNT=0
while IFS= read -r remote; do
  remote="$(printf '%s' "$remote" | tr -d '\r')"
  [[ -n "$remote" ]] || continue
  case "$remote" in
    "$REMOTE_ROOT"/*) ;;
    *) continue ;;
  esac
  case "$remote" in
    *.js) ;;
    */res/config.json) ;;
    */res/project.json) ;;
    */res/assets/*/config.json) ;;
    *) continue ;;
  esac

  rel="${remote#"$REMOTE_ROOT"/}"
  dest="$OUT_DIR/res/$rel"
  mkdir -p "$(dirname "$dest")"
  "$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat \"$remote\"'" > "$dest"
  case "$remote" in
    *.js) JS_COUNT=$((JS_COUNT + 1)) ;;
    *.json) CONFIG_COUNT=$((CONFIG_COUNT + 1)) ;;
  esac
  case "$remote" in
    *mod_bridge.js) BRIDGE_COUNT=$((BRIDGE_COUNT + 1)) ;;
  esac
done < "$OUT_DIR/files_paths.txt"

python3 - "$OUT_DIR/runtime_pull.json" "$PKG" "$SERIAL" "$REMOTE_SW" "$BRIDGE_COUNT" "$JS_COUNT" "$CONFIG_COUNT" <<'PY'
import json
import sys
from pathlib import Path

out, package, serial, remote_sw, bridge_count, js_count, config_count = sys.argv[1:]
data = {
    "package": package,
    "serial": serial,
    "remote_sw": remote_sw,
    "bridge_count": int(bridge_count),
    "js_count": int(js_count),
    "config_count": int(config_count),
    "mod_sw_path": "09_test/勇士之心/runtime/mod_sw.txt",
    "listing_path": "09_test/勇士之心/runtime/files_listing.txt",
}
Path(out).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

printf 'remote_root=%s\nremote_sw=%s\nbridge_count=%d\njs_count=%d\nconfig_count=%d\nout=%s\n' \
  "$REMOTE_ROOT" "$REMOTE_SW" "$BRIDGE_COUNT" "$JS_COUNT" "$CONFIG_COUNT" "$OUT_DIR"
cat "$OUT_DIR/mod_sw.txt"
