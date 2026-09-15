#!/usr/bin/env bash
# 黑暗领域：把当前完整 classes4.dex + v2 so 外科替换进星光底包。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_pack_latest_dex.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="黑暗领域"
BASE_APK="$NIXIANG/02_apks/$GAME/${GAME}_1.0_starry_from_device.apk"
DEX="$NIXIANG/mod菜单/starry-mod-menu/libs_haly/classes4.dex"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_haly/arm64-v8a/libstarry_magic.so"
OUT_DIR="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT_DIR/${GAME}_1.0_combo_fix3-unsigned.apk"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base APK" >&2; exit 1; }
[[ -f "$DEX" ]] || { echo "ERROR: missing latest classes4.dex" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing v2 so" >&2; exit 1; }
LC_ALL=C grep -aq 'build=starry_haly_v2' "$SO" || { echo "ERROR: so is not v2" >&2; exit 1; }

mkdir -p "$OUT_DIR"
WORK="$(mktemp -d /tmp/haly_pack_latest.XXXXXX)"
trap 'rm -rf "$WORK"; rc=$?; [ $rc -eq 0 ] || echo "ERROR: haly_pack_latest_dex.sh rc=$rc" >&2' EXIT

python3.11 - "$BASE_APK" "$WORK/base.apk" "$DEX" "$SO" <<'PY'
from pathlib import Path
from zipfile import ZipFile
import sys
base, out, dex, so = sys.argv[1:]
replacements = {
    'classes4.dex': Path(dex).read_bytes(),
    'lib/arm64-v8a/libstarry_magic.so': Path(so).read_bytes(),
}
seen = set()
with ZipFile(base, 'r') as zin, ZipFile(out, 'w') as zout:
    for info in zin.infolist():
        data = replacements.get(info.filename, zin.read(info.filename))
        if info.filename in replacements:
            seen.add(info.filename)
        zout.writestr(info, data)
    for name, data in replacements.items():
        if name not in seen:
            zout.writestr(name, data)
print('replaced', sorted(seen))
PY

unzip -p "$WORK/base.apk" classes4.dex > "$WORK/classes4.dex"
[[ "$(md5 -q "$WORK/classes4.dex")" == "$(md5 -q "$DEX")" ]] || { echo "ERROR: packaged classes4.dex mismatch" >&2; exit 1; }
IN_SO_MD5="$(unzip -p "$WORK/base.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$IN_SO_MD5" == "$(md5 -q "$SO")" ]] || { echo "ERROR: packaged so mismatch" >&2; exit 1; }
unzip -tq "$WORK/base.apk" >/dev/null
cp -f "$WORK/base.apk" "$UNSIGNED"
printf '[pack_latest] output=%s\n[pack_latest] dex_md5=%s\n[pack_latest] so_md5=%s\n' "$UNSIGNED" "$(md5 -q "$DEX")" "$IN_SO_MD5"
