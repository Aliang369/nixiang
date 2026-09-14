#!/usr/bin/env bash
# 追迹迷城：生成并校验二进制 AndroidManifest 补丁缓存。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/official_apk.json"
PATCHER="$SCRIPT_DIR/patch_zjmc_binary_manifest_v2.py"

[[ -f "$META_FILE" ]] || { echo "错误: 缺少 $META_FILE" >&2; exit 1; }
[[ -f "$PATCHER" ]] || { echo "错误: 缺少 $PATCHER" >&2; exit 1; }

while IFS=$'\t' read -r key value; do
  case "$key" in
    apk_path) APK="$value" ;;
    version_name) VERSION="$value" ;;
  esac
done < <(python3 - "$META_FILE" <<'PY'
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
for k in ("apk_path", "version_name"):
    print(f"{k}\t{d[k]}")
PY
)

CACHE_DIR="$NIXIANG_ROOT/07_patched/追迹迷城/surgical_$VERSION"
OUT_XML="$CACHE_DIR/AndroidManifest.xml"
TMP_XML="$(mktemp /tmp/zjmc_manifest.XXXXXX.xml)"
trap 'rm -f "$TMP_XML"' EXIT

mkdir -p "$CACHE_DIR"
unzip -p "$APK" AndroidManifest.xml > "$TMP_XML"
python3 "$PATCHER" "$TMP_XML" "$OUT_XML"

python3 - "$OUT_XML" <<'PY'
import struct
import sys

path = sys.argv[1]
data = open(path, "rb").read()
assert data[:4] == b"\x03\x00\x08\x00", data[:4].hex()

# Minimal AXML validation: expected strings exist and forbidden original slot is gone.
for text in (
    "com.starry.magic.StarryApp",
    "com.starry.magic.menu.MenuService",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
    "foregroundServiceType",
):
    assert text.encode("utf-16-le") in data, text

assert "com.byazt.b.DownloadHandleService".encode("utf-16-le") not in data
assert data.count("android.permission.SYSTEM_ALERT_WINDOW".encode("utf-16-le")) == 1
print("manifest binary validation ok")
PY

cat <<EOF
version=$VERSION
source_apk=$APK
patched_manifest=$OUT_XML
md5=$(md5 -q "$OUT_XML")
size=$(stat -f '%z' "$OUT_XML")
EOF
