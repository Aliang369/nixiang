#!/usr/bin/env bash
# 黑暗领域 fix2：给旧 classes4.dex 的 ModBridge 补两项 native 声明，匹配 v2 so。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_patch_dex_fix2.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="黑暗领域"
BASE_APK="$NIXIANG/02_apks/$GAME/${GAME}_1.0_starry_from_device.apk"
SO="$NIXIANG/mod菜单/starry-mod-menu/libs_haly/arm64-v8a/libstarry_magic.so"
OUT_DIR="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT_DIR/${GAME}_1.0_combo_fix2-unsigned.apk"
BAKSMALI="$NIXIANG/01_tools/smali/baksmali"
SMALI="$NIXIANG/01_tools/smali/smali"
DEXDUMP="$HOME/Library/Android/sdk/build-tools/36.0.0/dexdump"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing v2 so" >&2; exit 1; }
[[ -x "$BAKSMALI" && -x "$SMALI" ]] || { echo "ERROR: smali tools missing" >&2; exit 1; }
[[ -x "$DEXDUMP" ]] || { echo "ERROR: dexdump missing" >&2; exit 1; }
LC_ALL=C grep -aq 'build=starry_haly_v2' "$SO" || { echo "ERROR: so is not v2" >&2; exit 1; }

mkdir -p "$OUT_DIR"
WORK="$(mktemp -d /tmp/haly_dex_fix2.XXXXXX)"
trap 'rm -rf "$WORK"; rc=$?; [ $rc -eq 0 ] || echo "ERROR: haly_patch_dex_fix2.sh rc=$rc" >&2' EXIT

unzip -p "$BASE_APK" classes4.dex > "$WORK/classes4_old.dex"
"$BAKSMALI" d -o "$WORK/smali" "$WORK/classes4_old.dex"
BRIDGE="$WORK/smali/com/starry/magic/bridge/ModBridge.smali"
[[ -f "$BRIDGE" ]] || { echo "ERROR: ModBridge.smali not found" >&2; exit 1; }

python3.11 - "$BRIDGE" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
s = p.read_text(encoding='utf-8')
methods = [
    ('.method public static native nativeBridgeRegistered()Z\n.end method', 'nativeBridgeRegistered'),
    ('.method public static native nativeGetPinnedCert()[B\n.end method', 'nativeGetPinnedCert'),
]
for block, name in methods:
    if f' {name}(' in s:
        print(f'{name}: already present')
        continue
    s = s.rstrip() + '\n\n# starry_haly_v2_native_bridge\n' + block + '\n'
    print(f'{name}: added')
p.write_text(s, encoding='utf-8')
PY

"$SMALI" assemble -o "$WORK/classes4_fix2.dex" "$WORK/smali"
"$DEXDUMP" -f "$WORK/classes4_fix2.dex" > "$WORK/dexdump.txt"
grep -q "'nativeGetPinnedCert'" "$WORK/dexdump.txt" || { echo "ERROR: nativeGetPinnedCert missing after reassemble" >&2; exit 1; }
grep -q "'nativeBridgeRegistered'" "$WORK/dexdump.txt" || { echo "ERROR: nativeBridgeRegistered missing after reassemble" >&2; exit 1; }
REASSEMBLED_MD5="$(md5 -q "$WORK/classes4_fix2.dex")"

cp -f "$BASE_APK" "$WORK/base.apk"
python3.11 - "$BASE_APK" "$WORK/base.apk" "$WORK/classes4_fix2.dex" "$SO" <<'PY'
from pathlib import Path
from zipfile import ZipFile
import sys
base, out, dex, so = sys.argv[1:]
replacements = {
    'classes4.dex': Path(dex).read_bytes(),
    'lib/arm64-v8a/libstarry_magic.so': Path(so).read_bytes(),
}
with ZipFile(base, 'r') as zin, ZipFile(out, 'w') as zout:
    seen = set()
    for info in zin.infolist():
        data = replacements.get(info.filename, zin.read(info.filename))
        if info.filename in replacements:
            seen.add(info.filename)
        zout.writestr(info, data)
    for name, data in replacements.items():
        if name not in seen:
            zout.writestr(name, data)
PY

SO_MD5="$(md5 -q "$SO")"
IN_SO_MD5="$(unzip -p "$WORK/base.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$SO_MD5" == "$IN_SO_MD5" ]] || { echo "ERROR: patched so mismatch" >&2; exit 1; }
unzip -p "$WORK/base.apk" classes4.dex > "$WORK/classes4_check.dex"
CHECK_MD5="$(md5 -q "$WORK/classes4_check.dex")"
[[ "$CHECK_MD5" == "$REASSEMBLED_MD5" ]] || { echo "ERROR: final dex md5 mismatch rebuilt=$REASSEMBLED_MD5 packaged=$CHECK_MD5" >&2; exit 1; }
FINAL_DUMP="$("$DEXDUMP" -f "$WORK/classes4_check.dex" 2>&1 || true)"
grep -Fq "'nativeGetPinnedCert'" <<< "$FINAL_DUMP" || { echo "ERROR: final dex missing nativeGetPinnedCert" >&2; exit 1; }
grep -Fq "'nativeBridgeRegistered'" <<< "$FINAL_DUMP" || { echo "ERROR: final dex missing nativeBridgeRegistered" >&2; exit 1; }
unzip -tq "$WORK/base.apk" >/dev/null
cp -f "$WORK/base.apk" "$UNSIGNED"
printf '[dex_fix2] output=%s\n[dex_fix2] classes4_md5=%s\n[dex_fix2] so_md5=%s\n' "$UNSIGNED" "$(md5 -q "$WORK/classes4_fix2.dex")" "$IN_SO_MD5"
