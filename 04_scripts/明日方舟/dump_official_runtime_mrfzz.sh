#!/usr/bin/env bash
# 明日方舟：只读定位并导出原版进程运行时解密后的 IL2CPP metadata。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
# shellcheck source=../_common/env.sh
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/official_apk.json"
HELPER="$SCRIPT_DIR/search_metadata_mem_device.sh"
PKG="com.hypergryph.arknights"
OUT="$NIXIANG_ROOT/06_dumped/明日方舟/official/runtime_current"
DEV_DIR="/data/local/tmp/mrfzz_official_metadata"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE，请先跑 fetch_official" >&2; exit 1; }
[[ -f "$HELPER" ]] || { echo "错误: 缺少设备端 helper: $HELPER" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
PID="$("$ADB_BIN" -s "$SERIAL" shell pidof "$PKG" | tr -d '\r' | awk '{print $1}')"
[[ -n "$PID" ]] || { echo "错误: 原版游戏进程未运行，请先把 TapTap 原版启动并保持进程存活" >&2; exit 1; }

rm -rf "$OUT"
mkdir -p "$OUT/raw"

"$ADB_BIN" -s "$SERIAL" push "$HELPER" /data/local/tmp/search_metadata_mem_device.sh >/dev/null
"$ADB_BIN" -s "$SERIAL" shell "su -c 'chmod 755 /data/local/tmp/search_metadata_mem_device.sh'"
"$ADB_BIN" -s "$SERIAL" shell "su -c '/data/local/tmp/search_metadata_mem_device.sh $PID $DEV_DIR'" | tee "$OUT/dump_log.txt"

"$ADB_BIN" -s "$SERIAL" pull "$DEV_DIR/maps.txt" "$OUT/maps.txt" >/dev/null
"$ADB_BIN" -s "$SERIAL" pull "$DEV_DIR/magic_hits.txt" "$OUT/magic_hits.txt" >/dev/null
"$ADB_BIN" -s "$SERIAL" shell "su -c 'ls -1 $DEV_DIR/metadata_runtime_*.bin'" 2>/dev/null | tr -d '\r' > "$OUT/metadata_files.txt"
[[ -s "$OUT/metadata_files.txt" ]] || { echo "错误: 未找到运行时 metadata magic" >&2; exit 1; }

first_remote="$(sed -n '1p' "$OUT/metadata_files.txt")"
first_base="$(basename "$first_remote")"
while IFS= read -r remote; do
  [[ -n "$remote" ]] || continue
  base="$(basename "$remote")"
  "$ADB_BIN" -s "$SERIAL" pull "$remote" "$OUT/raw/$base" >/dev/null
done < "$OUT/metadata_files.txt"
cp -f "$OUT/raw/$first_base" "$OUT/global-metadata.runtime.dat"

SIZE="$(stat -f '%z' "$OUT/global-metadata.runtime.dat")"
MAGIC="$(xxd -p -l 4 "$OUT/global-metadata.runtime.dat")"
[[ "$SIZE" -eq 43646828 ]] || { echo "错误: runtime metadata 大小异常: $SIZE" >&2; exit 1; }
if [[ "$MAGIC" != "af1bb1fa" ]]; then
  echo "警告: runtime metadata 首 4 字节非标准 magic，已保留原样供离线分析: $MAGIC" >&2
fi

python3 - "$META_FILE" "$OUT" "$SERIAL" "$PID" "$SIZE" "$MAGIC" <<'PY'
import json
import sys
from pathlib import Path

meta_path, out, serial, pid, size, magic = sys.argv[1:]
official = json.loads(Path(meta_path).read_text(encoding="utf-8"))
out = Path(out)
data = {
    "kind": "official_runtime_metadata",
    "serial": serial,
    "pid": int(pid) if pid.isdigit() else pid,
    "package": official["package"],
    "version_name": official["version_name"],
    "version_code": official["version_code"],
    "maps": str(out / "maps.txt"),
    "magic_hits": str(out / "magic_hits.txt"),
    "metadata": str(out / "global-metadata.runtime.dat"),
    "metadata_size": int(size),
    "metadata_magic": magic,
    "official_apk": official["apk_path"],
}
(out / "runtime_metadata.json").write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY

echo "OK pid=$PID metadata=$OUT/global-metadata.runtime.dat"
