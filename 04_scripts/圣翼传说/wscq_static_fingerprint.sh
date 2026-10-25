#!/usr/bin/env bash
# 圣翼传说新版本静态三查：dex 布局、Manifest 关键特征、libil2cpp/metadata 指纹。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
BASE="${WSCQ_BASE_APK:-$NIXIANG/09_test/圣翼传说/base_20261026/base.apk}"
OUT_DIR="${WSCQ_STATIC_OUT_DIR:-$NIXIANG/09_test/圣翼传说/base_20261026/static}"
REPORT="$OUT_DIR/fingerprint.txt"
BT_DIR="${ANDROID_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"
AAPT2="$BT_DIR/aapt2"
OLD_SO_MD5="563b40a81370833858ef2d4452f8dc3f"
OLD_META_MD5="ef84cf230a6f4be6858f13cc47e2ab5b"

die() { echo "ERROR: $*" >&2; exit 1; }
[[ -f "$BASE" ]] || die "缺少底包: $BASE"
[[ -x "$AAPT2" ]] || die "缺少 aapt2: $AAPT2"
mkdir -p "$OUT_DIR"

"$AAPT2" dump badging "$BASE" > "$OUT_DIR/badging.txt"
"$AAPT2" dump xmltree "$BASE" --file AndroidManifest.xml > "$OUT_DIR/AndroidManifest.xmltree.txt"
unzip -Z1 "$BASE" > "$OUT_DIR/entries.txt"

VERSION_NAME="$(sed -n "s/^package: name='[^']*' versionCode='[^']*' versionName='\([^']*\)'.*/\1/p" "$OUT_DIR/badging.txt" | head -1)"
VERSION_CODE="$(sed -n "s/^package: name='[^']*' versionCode='\([^']*\)'.*/\1/p" "$OUT_DIR/badging.txt" | head -1)"
LAUNCH_ACTIVITY="$(sed -n "s/^launchable-activity: name='\([^']*\)'.*/\1/p" "$OUT_DIR/badging.txt" | head -1)"
[[ -n "$VERSION_NAME" && -n "$VERSION_CODE" ]] || die "badging 读不到版本"

SO_MD5="$(unzip -p "$BASE" lib/arm64-v8a/libil2cpp.so | md5 -q)"
META_MD5="$(unzip -p "$BASE" assets/bin/Data/Managed/Metadata/global-metadata.dat | md5 -q)"

python3 - "$BASE" "$OUT_DIR/dex_header.txt" <<'PY'
import struct, sys, zipfile
apk, out = sys.argv[1], sys.argv[2]
with zipfile.ZipFile(apk) as z, open(out, 'w', encoding='utf-8') as f:
    names = sorted(n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex'))
    for name in names:
        b = z.read(name)
        if len(b) < 0x70 or b[:4] != b'dex\n':
            raise SystemExit(f'{name}: not a dex')
        method_ids = struct.unpack_from('<I', b, 0x58)[0]
        class_defs = struct.unpack_from('<I', b, 0x60)[0]
        type_ids = struct.unpack_from('<I', b, 0x40)[0]
        string_ids = struct.unpack_from('<I', b, 0x38)[0]
        line = (f'{name} size={len(b)} method_ids={method_ids} class_defs={class_defs} '
                f'type_ids={type_ids} string_ids={string_ids} method_remaining={65536-method_ids}')
        print(line)
        f.write(line + '\n')
    meta = z.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
    sanity, version = struct.unpack_from('<Ii', meta, 0)
    if sanity != 0xFAB11BAF:
        raise SystemExit(f'bad metadata sanity: 0x{sanity:08x}')
    line = f'metadata sanity=0x{sanity:08x} version={version} size={len(meta)}'
    print(line)
    f.write(line + '\n')
PY

APP_COMPONENT="$(sed -n 's/.*android:appComponentFactory(0x[0-9a-f]*)=\"\([^\"]*\)\".*/\1/p' "$OUT_DIR/AndroidManifest.xmltree.txt" | head -1)"
APP_NAME="$(awk '/E: application /{in_app=1; next} in_app && /^          E: /{exit} in_app' "$OUT_DIR/AndroidManifest.xmltree.txt" \
  | sed -n 's/.*android:name(0x[0-9a-f]*)=\"\([^\"]*\)\".*/\1/p' | head -1)"
EXTRACT_NATIVE="$(sed -n '/E: application /,/E: .*activity /s/.*android:extractNativeLibs(0x[0-9a-f]*)=\([^ ]*\).*/\1/p' "$OUT_DIR/AndroidManifest.xmltree.txt" | head -1)"
TAPTAP_ACTIVITY="no"
grep -q 'com.taptap.sdk.ui.TapTapActivity' "$OUT_DIR/AndroidManifest.xmltree.txt" && TAPTAP_ACTIVITY="yes"
PROVIDER_COUNT="$(grep -c 'E: provider ' "$OUT_DIR/AndroidManifest.xmltree.txt" || true)"
SERVICE_COUNT="$(grep -c 'E: service ' "$OUT_DIR/AndroidManifest.xmltree.txt" || true)"
PERMISSION_COUNT="$(grep -c 'E: uses-permission ' "$OUT_DIR/AndroidManifest.xmltree.txt" || true)"

if [[ "$SO_MD5" == "$OLD_SO_MD5" ]]; then SO_CHANGED="no"; else SO_CHANGED="yes"; fi
if [[ "$META_MD5" == "$OLD_META_MD5" ]]; then META_CHANGED="no"; else META_CHANGED="yes"; fi

{
  echo "versionName=$VERSION_NAME"
  echo "versionCode=$VERSION_CODE"
  echo "launchable_activity=$LAUNCH_ACTIVITY"
  echo "libil2cpp_md5=$SO_MD5"
  echo "libil2cpp_changed_from_7.5.10=$SO_CHANGED"
  echo "global_metadata_md5=$META_MD5"
  echo "global_metadata_changed_from_7.5.10=$META_CHANGED"
  echo "application_name=${APP_NAME:-<absent>}"
  echo "appComponentFactory=$APP_COMPONENT"
  echo "extractNativeLibs=$EXTRACT_NATIVE"
  echo "taptap_activity=$TAPTAP_ACTIVITY"
  echo "provider_count=$PROVIDER_COUNT"
  echo "service_count=$SERVICE_COUNT"
  echo "uses_permission_count=$PERMISSION_COUNT"
  cat "$OUT_DIR/dex_header.txt"
} | tee "$REPORT"

echo "[static] done → $REPORT"
