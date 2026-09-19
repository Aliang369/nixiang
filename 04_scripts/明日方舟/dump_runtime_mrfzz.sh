#!/usr/bin/env bash
# 明日方舟：只读导出斌哥壳、IL2CPP RX 段和近邻匿名段。
# Usage: dump_runtime_mrfzz.sh [label]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PACKAGE_FILE="$SCRIPT_DIR/.device_package"
HELPER="$NIXIANG_ROOT/04_scripts/_common/dump_binge_shell_mem_device.sh"
LABEL="${1:-current}"
case "$LABEL" in
  *[!A-Za-z0-9._-]*|'') echo "错误: label 只允许 A-Za-z0-9._-: $LABEL" >&2; exit 2 ;;
esac
OUT="$NIXIANG_ROOT/06_dumped/明日方舟/runtime_$LABEL"
DEV_DIR="/data/local/tmp/mrfzz_memdump_$LABEL"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" && -f "$PACKAGE_FILE" ]] || { echo "错误: 缺少设备状态，请先跑 device_ready" >&2; exit 1; }
[[ -f "$HELPER" ]] || { echo "错误: 缺少设备端 dump 脚本: $HELPER" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
PKG="$(tr -d '[:space:]' < "$PACKAGE_FILE")"

PID="$("$ADB_BIN" -s "$SERIAL" shell pidof "$PKG" | tr -d '\r' | awk '{print $1}')"
[[ -n "$PID" ]] || { echo "错误: 游戏进程未运行: $PKG" >&2; exit 1; }

rm -rf "$OUT"
mkdir -p "$OUT/raw"

"$ADB_BIN" -s "$SERIAL" push "$HELPER" /data/local/tmp/dump_binge_shell_mem_device.sh >/dev/null
"$ADB_BIN" -s "$SERIAL" shell "su -c 'chmod 755 /data/local/tmp/dump_binge_shell_mem_device.sh'"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'cat /proc/$PID/maps'" > "$OUT/maps.txt"
"$ADB_BIN" -s "$SERIAL" shell "su -c '/data/local/tmp/dump_binge_shell_mem_device.sh $PID $DEV_DIR'" | tee "$OUT/dump_log.txt"

while IFS= read -r f; do
  f="$(printf '%s' "$f" | tr -d '\r')"
  [[ -n "$f" ]] || continue
  "$ADB_BIN" -s "$SERIAL" pull "$DEV_DIR/$f" "$OUT/raw/$f" 2>/dev/null || \
    "$ADB_BIN" -s "$SERIAL" exec-out su -c "cat '$DEV_DIR/$f'" > "$OUT/raw/$f"
done < <("$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -1 $DEV_DIR'" | tr -d '\r')

cp -f "$OUT/raw/"* "$OUT/" 2>/dev/null || true

# Shared prefs are useful evidence for the menu feature bits; read-only copy.
if "$ADB_BIN" -s "$SERIAL" exec-out su -c "tar -C /data/data/$PKG -cf - shared_prefs" > "$OUT/shared_prefs.tar" 2>/dev/null; then
  mkdir -p "$OUT/shared_prefs"
  tar -xf "$OUT/shared_prefs.tar" -C "$OUT" 2>/dev/null || true
fi

python3 - "$SCRIPT_DIR/device_info.json" "$SCRIPT_DIR/binge_apk.json" "$OUT" "$SERIAL" "$PKG" "$PID" "$LABEL" <<'PY'
import json
import sys
from pathlib import Path

device_path, apk_path, out, serial, package, pid, label = sys.argv[1:]
out = Path(out)
device = json.loads(Path(device_path).read_text(encoding="utf-8")) if Path(device_path).is_file() else {}
apk = json.loads(Path(apk_path).read_text(encoding="utf-8")) if Path(apk_path).is_file() else {}

def count(root, pattern):
    return len(list(root.glob(pattern)))

data = {
    "label": label,
    "serial": serial,
    "package": package,
    "pid": int(pid) if pid.isdigit() else pid,
    "device": device,
    "binge_apk": apk,
    "maps": str(out / "maps.txt"),
    "sdk_rx_count": count(out, "sdk_rx_*.bin"),
    "il2cpp_rx_count": count(out, "il2cpp_rx_*.bin"),
    "anon_rwx_count": count(out, "anon_rwx_*.bin"),
    "shell_rx_bss_count": count(out, "shell_rx_bss_*.bin"),
    "shell_ro_bss_count": count(out, "shell_ro_bss_*.bin"),
    "shell_rw_bss_count": count(out, "shell_rw_bss_*.bin"),
    "shell_rwx_bss_count": count(out, "shell_rwx_bss_*.bin"),
    "near_exec_count": count(out, "near_exec_*.bin"),
}
(out / "runtime_dump.json").write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY

ls -lah "$OUT" | tee "$OUT/host_listing.txt"
echo "OK pid=$PID -> $OUT"
