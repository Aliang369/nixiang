#!/usr/bin/env bash
# 宝石战争：只读提取当前安装 base APK 与 HotUpdate 1.0.0.01 输入。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="com.acing.gow.adv"
ADB_BIN="${ADB_BIN:-adb}"
OUT_DIR="$NIXIANG_ROOT/06_dumped/宝石战争/${BSZZ_VERSION:-6.0.013}/current"
BASE_OUT="$OUT_DIR/base.apk"
HOT_OUT="$OUT_DIR/hotupdate/Android"
META_OUT="$OUT_DIR/current_inputs.json"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"
EXPECT_VERSION_NAME="${BSZZ_EXPECT_VERSION_NAME:-6.0.013}"
EXPECT_VERSION_CODE="${BSZZ_EXPECT_VERSION_CODE:-6000013}"
[[ "$VERSION_NAME" == "$EXPECT_VERSION_NAME" && "$VERSION_CODE" == "$EXPECT_VERSION_CODE" ]] || {
  echo "ERROR: 版本不符，期望 ${EXPECT_VERSION_NAME}/vc${EXPECT_VERSION_CODE}，实际 ${VERSION_NAME}/${VERSION_CODE}" >&2
  exit 1
}
[[ "$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')" -eq 1 ]] || {
  echo "ERROR: 非单 base APK，拒绝" >&2
  exit 1
}
REMOTE_BASE="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
REMOTE_HOT="/storage/emulated/0/Android/data/$PKG/files/HotUpdate/Android"

pull_file() {
  local remote="$1"
  local dest="$2"
  local name
  name="$(basename "$remote")"
  mkdir -p "$(dirname "$dest")"
  if "$ADB_BIN" -s "$SERIAL" pull "$remote" "$dest.part" >/dev/null 2>&1; then
    mv -f "$dest.part" "$dest"
    return 0
  fi
  local remote_tmp="/data/local/tmp/bszz_current_${name}"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp \"$remote\" \"$remote_tmp\" && chmod 644 \"$remote_tmp\"'" >/dev/null
  "$ADB_BIN" -s "$SERIAL" pull "$remote_tmp" "$dest.part" >/dev/null
  mv -f "$dest.part" "$dest"
}

mkdir -p "$OUT_DIR" "$HOT_OUT/Dlls"
pull_file "$REMOTE_BASE" "$BASE_OUT"
pull_file "$REMOTE_HOT/Dlls/System.Network.dll.bytes" "$HOT_OUT/Dlls/System.Network.dll.bytes"
pull_file "$REMOTE_HOT/Dlls/System.Platform.dll.bytes" "$HOT_OUT/Dlls/System.Platform.dll.bytes"
pull_file "$REMOTE_HOT/manifest.json" "$HOT_OUT/manifest.json"
pull_file "$REMOTE_HOT/RuntimeAssembliesReceipt.json" "$HOT_OUT/RuntimeAssembliesReceipt.json"

python3 - "$META_OUT" "$BASE_OUT" "$HOT_OUT" "$SERIAL" "$REMOTE_BASE" "$REMOTE_HOT" "$VERSION_NAME" "$VERSION_CODE" <<'PY'
from pathlib import Path
import hashlib
import json
import sys

meta, base, hot, serial, remote_base, remote_hot, VERSION_NAME, VERSION_CODE = sys.argv[1:]
base = Path(base)
hot = Path(hot)

def fingerprint(path):
    data = path.read_bytes()
    return {
        "size": len(data),
        "md5": hashlib.md5(data).hexdigest(),
        "sha256": hashlib.sha256(data).hexdigest(),
    }

manifest = json.loads((hot / "manifest.json").read_text(encoding="utf-8"))
data = {
    "game": "宝石战争",
    "package": "com.acing.gow.adv",
    "version": VERSION_NAME,
    "version_code": int(VERSION_CODE),
    "serial": serial,
    "remote_base_apk": remote_base,
    "remote_hotupdate_dir": remote_hot,
    "hotupdate_manifest_version": manifest.get("version"),
    "base_apk": str(base),
    "base_apk_fingerprint": fingerprint(base),
    "hotupdate": {
        "system_network_bytes": fingerprint(hot / "Dlls/System.Network.dll.bytes"),
        "system_platform_bytes": fingerprint(hot / "Dlls/System.Platform.dll.bytes"),
        "manifest": fingerprint(hot / "manifest.json"),
        "receipt": fingerprint(hot / "RuntimeAssembliesReceipt.json"),
    },
}
Path(meta).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(data, ensure_ascii=False, indent=2))
PY

echo "current_input=$META_OUT"
