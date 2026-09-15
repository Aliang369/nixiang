#!/usr/bin/env bash
# 黑暗领域：从当前 Gradle 源码构建完整 classes4.dex（不手工补旧 dex）。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_build_latest_dex.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
OUT_DEX="$STARRY_SRC/libs_haly/classes4.dex"
SMALI="$NIXIANG/01_tools/smali/smali"
DEXDUMP="$HOME/Library/Android/sdk/build-tools/36.0.0/dexdump"

[[ -x "$SMALI" ]] || { echo "ERROR: smali missing" >&2; exit 1; }
[[ -x "$DEXDUMP" ]] || { echo "ERROR: dexdump missing" >&2; exit 1; }
[[ -f "$STARRY_SRC/app/src/main/java/com/starry/magic/menu/MagicRes.java" ]] || { echo "ERROR: MagicRes.java missing" >&2; exit 1; }

WORK="$(mktemp -d /tmp/haly_latest_dex.XXXXXX)"
trap 'rm -rf "$WORK"; rc=$?; [ $rc -eq 0 ] || echo "ERROR: haly_build_latest_dex.sh rc=$rc" >&2' EXIT

rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"

(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
APP_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$APP_APK" ]] || { echo "ERROR: Gradle app-release.apk missing" >&2; exit 1; }

apktool d -f -o "$WORK/starry_dec" "$APP_APK" >/dev/null
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob

MAGIC_SMALI="$SMALI_ROOT/com/starry/magic/menu/MagicRes.smali"
BRIDGE_SMALI="$SMALI_ROOT/com/starry/magic/bridge/ModBridge.smali"
[[ -f "$MAGIC_SMALI" ]] || { echo "ERROR: current MagicRes.smali missing" >&2; exit 1; }
[[ -f "$BRIDGE_SMALI" ]] || { echo "ERROR: current ModBridge.smali missing" >&2; exit 1; }

python3.11 - "$STARRY_SRC/app/src/main/java/com/starry/magic/menu/MagicRes.java" "$MAGIC_SMALI" "$BRIDGE_SMALI" <<'PY'
from pathlib import Path
import re
import sys
java, smali, bridge = map(Path, sys.argv[1:])
source = java.read_text(encoding='utf-8')
m = re.search(r'FAB_ICON_WEBP_B64\s*=\s*"([A-Za-z0-9+/=]+)"', source)
if not m:
    raise SystemExit('FAB_ICON_WEBP_B64 not found in MagicRes.java')
prefix = m.group(1)[:192]
text = smali.read_text(encoding='utf-8')
if prefix not in text:
    raise SystemExit('current FAB icon base64 fingerprint not found in rebuilt MagicRes.smali')
bridge_text = bridge.read_text(encoding='utf-8')
for name in ('nativeBridgeRegistered', 'nativeGetPinnedCert'):
    if f' {name}(' not in bridge_text:
        raise SystemExit(f'{name} missing in rebuilt ModBridge.smali')
print('current_fab_prefix_ok', len(prefix))
PY

mkdir -p "$(dirname "$OUT_DEX")"
"$SMALI" assemble -o "$OUT_DEX" "$SMALI_ROOT"
"$DEXDUMP" -f "$OUT_DEX" > "$WORK/dexdump.txt" || true
for name in nativeBridgeRegistered nativeGetPinnedCert; do
  grep -Fq "'$name'" "$WORK/dexdump.txt" || { echo "ERROR: rebuilt dex missing $name" >&2; exit 1; }
done

python3.11 - "$STARRY_SRC/app/src/main/java/com/starry/magic/menu/MagicRes.java" "$OUT_DEX" <<'PY'
from pathlib import Path
import re
import sys
java, dex = map(Path, sys.argv[1:])
source = java.read_text(encoding='utf-8')
b64 = re.search(r'FAB_ICON_WEBP_B64\s*=\s*"([A-Za-z0-9+/=]+)"', source).group(1)
probe = b64[:192].encode('ascii')
if probe not in dex.read_bytes():
    raise SystemExit('current FAB icon fingerprint missing in rebuilt dex bytes')
print('rebuild_dex_fab_ok', len(b64))
PY

printf '[latest_dex] output=%s\n[latest_dex] md5=%s\n[latest_dex] sha256=%s\n' \
  "$OUT_DEX" "$(md5 -q "$OUT_DEX")" "$(shasum -a 256 "$OUT_DEX" | awk '{print $1}')"
