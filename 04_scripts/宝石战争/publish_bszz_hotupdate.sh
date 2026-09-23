#!/usr/bin/env bash
# 宝石战争：备份并发布当前 HotUpdate 1.0.0.01 的补丁 DLL/manifest/receipt。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.acing.gow.adv"
ADB_BIN="${ADB_BIN:-adb}"
PATCH_ROOT="$NIXIANG_ROOT/07_patched/宝石战争/${BSZZ_VERSION:-6.0.013}/assets/Android"
LOCAL_BACKUP="$NIXIANG_ROOT/09_test/宝石战争/hotupdate_backup_20260920"
REMOTE_HOT="/storage/emulated/0/Android/data/$PKG/files/HotUpdate/Android"
REMOTE_BACKUP="/data/local/tmp/bszz_hotupdate_backup_20260920"
REMOTE_STAGE="/data/local/tmp/bszz_hotupdate_patched_20260920"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"

for rel in Dlls/System.Network.dll.bytes manifest.json RuntimeAssembliesReceipt.json; do
  [[ -f "$PATCH_ROOT/$rel" ]] || { echo "ERROR: 缺少补丁产物: $PATCH_ROOT/$rel" >&2; exit 1; }
done

mkdir -p "$LOCAL_BACKUP/Dlls"
for rel in Dlls/System.Network.dll.bytes manifest.json RuntimeAssembliesReceipt.json; do
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_HOT/$rel" "$LOCAL_BACKUP/$rel.part" >/dev/null
  mv -f "$LOCAL_BACKUP/$rel.part" "$LOCAL_BACKUP/$rel"
done

"$ADB_BIN" -s "$SERIAL" shell "su -c 'mkdir -p \"$REMOTE_BACKUP/Dlls\" \"$REMOTE_STAGE/Dlls\" && chmod 777 \"$REMOTE_STAGE\" \"$REMOTE_STAGE/Dlls\" && cp -f \"$REMOTE_HOT/Dlls/System.Network.dll.bytes\" \"$REMOTE_BACKUP/Dlls/System.Network.dll.bytes\" && cp -f \"$REMOTE_HOT/manifest.json\" \"$REMOTE_BACKUP/manifest.json\" && cp -f \"$REMOTE_HOT/RuntimeAssembliesReceipt.json\" \"$REMOTE_BACKUP/RuntimeAssembliesReceipt.json\"'"

for rel in Dlls/System.Network.dll.bytes manifest.json RuntimeAssembliesReceipt.json; do
  "$ADB_BIN" -s "$SERIAL" push "$PATCH_ROOT/$rel" "$REMOTE_STAGE/$rel" >/dev/null
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp -f \"$REMOTE_STAGE/$rel\" \"$REMOTE_HOT/$rel\"'"
done

python3 - "$LOCAL_BACKUP/deploy_meta.json" "$PATCH_ROOT" "$SERIAL" "$REMOTE_HOT" "$REMOTE_BACKUP" <<'PY'
from pathlib import Path
import hashlib
import json
import sys

meta, patch_root, serial, remote_hot, remote_backup = sys.argv[1:]
patch_root = Path(patch_root)

def fingerprint(path):
    data = path.read_bytes()
    return {
        "size": len(data),
        "md5": hashlib.md5(data).hexdigest(),
        "sha256": hashlib.sha256(data).hexdigest(),
    }

data = {
    "game": "宝石战争",
    "package": "com.acing.gow.adv",
    "version": "6.0.012",
    "hotupdate_version": "1.0.0.01",
    "serial": serial,
    "local_backup": str(Path(meta).parent),
    "remote_hotupdate_dir": remote_hot,
    "remote_backup_dir": remote_backup,
    "published": {
        "system_network_bytes": fingerprint(patch_root / "Dlls/System.Network.dll.bytes"),
        "manifest": fingerprint(patch_root / "manifest.json"),
        "receipt": fingerprint(patch_root / "RuntimeAssembliesReceipt.json"),
    },
}
Path(meta).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY

echo "hotupdate_backup=$LOCAL_BACKUP"
echo "hotupdate_published=$REMOTE_HOT"
