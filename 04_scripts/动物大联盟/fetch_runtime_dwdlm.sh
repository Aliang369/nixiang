#!/usr/bin/env bash
# 动物大联盟：只读提取真机当前热更后的 files/res 运行态资源。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/runtime_files.json"
PKG="${PKG:-com.fightingstars.games.animal}"
VERSION="${EXPECTED_VERSION:-1.0.23}"
ADB_BIN="${ADB_BIN:-adb}"
OUT_DIR="$NIXIANG_ROOT/06_dumped/动物大联盟/runtime_${VERSION}"
OUT="$OUT_DIR/files_res"
TMP="$OUT.part"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }
[[ ! -e "$OUT" ]] || { echo "错误: 已存在，拒绝覆盖: $OUT" >&2; exit 1; }

REMOTE_DIR="/data/user/0/$PKG/files/res"
REMOTE_TMP="/data/local/tmp/dwdlm_runtime_res.tgz"
REMOTE_CHECK="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'test -d \"$REMOTE_DIR\" && echo yes || echo no'" | tr -d '\r')"
[[ "$REMOTE_CHECK" == "yes" ]] || { echo "错误: 运行态目录不存在: $REMOTE_DIR" >&2; exit 1; }

rm -rf "$TMP"; mkdir -p "$TMP"
cleanup() {
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f $REMOTE_TMP'" >/dev/null 2>&1 || true
  rm -rf "$TMP"
}
trap cleanup EXIT
"$ADB_BIN" -s "$SERIAL" shell "su -c 'tar -C \"$(dirname "$REMOTE_DIR")\" -czf \"$REMOTE_TMP\" \"$(basename "$REMOTE_DIR")\" && chmod 644 \"$REMOTE_TMP\"'"
"$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TMP/runtime_res.tgz" >/dev/null
tar -xzf "$TMP/runtime_res.tgz" -C "$TMP"
rm -f "$TMP/runtime_res.tgz"
mkdir -p "$OUT_DIR"
mv "$TMP/res" "$OUT"
rmdir "$TMP"
trap - EXIT
"$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f $REMOTE_TMP'" >/dev/null 2>&1 || true

python3 - "$OUT" "$META_FILE" "$SERIAL" <<'PY'
import hashlib, json, sys
from pathlib import Path
root = Path(sys.argv[1])
out = Path(sys.argv[2])
serial = sys.argv[3]
records = []
for p in sorted(root.rglob('*')):
    if not p.is_file():
        continue
    data = p.read_bytes()
    records.append({
        'path': str(p.relative_to(root)),
        'size': len(data),
        'sha256': hashlib.sha256(data).hexdigest(),
    })
data = {
    'package': 'com.fightingstars.games.animal',
    'version': '1.0.23',
    'serial': serial,
    'source': '/data/user/0/com.fightingstars.games.animal/files/res',
    'root': str(root),
    'file_count': len(records),
    'total_size': sum(r['size'] for r in records),
    'records': records,
}
out.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(f"file_count={data['file_count']} total_size={data['total_size']}")
for r in records:
    if r['size'] >= 1000:
        print(f"{r['size']}\t{r['sha256']}\t{r['path']}")
PY
printf 'out=%s\nmeta=%s\n' "$OUT" "$META_FILE"
