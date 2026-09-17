#!/usr/bin/env bash
# 动物大联盟 1.0.23 — spoof-only 中间变体。
# 保留原包主体与 ProxyApplication 初始化，只加 SignSpoof。
# 不包含星光 native、StarryInitProvider、MenuService 或任何 Hook。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
ORIG_APK="${DWDLM_BASE_APK:-$NIXIANG/02_apks/动物大联盟/动物大联盟_1.0.23_official.apk}"
ORIG_MD5="${DWDLM_BASE_MD5:-211cfebb2d273360e22ea3593fdaedd6}"
STARRY_UNSIGNED="${DWDLM_STARRY_UNSIGNED:-$NIXIANG/07_patched/动物大联盟/动物大联盟_1.0.23_starry-unsigned.apk}"
OUT_DIR="$NIXIANG/08_output/动物大联盟/variants"
UNSIGNED_DIR="$NIXIANG/07_patched/动物大联盟/variants"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PKG="com.fightingstars.games.animal"
ADB_BIN="${ADB_BIN:-adb}"
SERIAL="$(tr -d '[:space:]' < "$SCRIPT_DIR/.device_serial")"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || {
  echo "ERROR: base APK fingerprint mismatch" >&2
  exit 1
}
[[ -f "$STARRY_UNSIGNED" ]] || { echo "ERROR: missing starry unsigned $STARRY_UNSIGNED" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali tools missing" >&2; exit 1; }
[[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done
[[ -n "$BT" && -x "$BT/apksigner" ]] || { echo "ERROR: build-tools missing" >&2; exit 1; }

mkdir -p "$OUT_DIR" "$UNSIGNED_DIR"
WORK="$(mktemp -d /tmp/dwdlm_spoof.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

UNSIGNED="$UNSIGNED_DIR/spoof-only-unsigned.apk"
SIGNED="$OUT_DIR/spoof-only-releaseSigned.apk"

echo "[1/6] extract and patch DwdlmApplication smali"
unzip -p "$STARRY_UNSIGNED" classes2.dex > "$WORK/classes2.dex"
mkdir -p "$WORK/smali"
"$BAKSMALI_BIN" d "$WORK/classes2.dex" -o "$WORK/smali" >/dev/null
python3 - "$WORK/smali/com/starry/magic/dwdlm/DwdlmApplication.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding='utf-8')
start = t.find('.method static constructor <clinit>()V')
end = t.find('.end method', start)
if start < 0 or end < 0:
    raise SystemExit('DwdlmApplication <clinit> not found')
noop = '.method static constructor <clinit>()V\n    .registers 0\n\n    return-void\n.end method'
t = t[:start] + noop + t[end + len('.end method'):]
t = t.replace(
    '    invoke-static {}, Lcom/starry/magic/dwdlm/DwdlmApplication;->prepareSecureShell()V\n\n',
    '')
p.write_text(t, encoding='utf-8')
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/smali"

echo "[2/6] patch manifest Application only"
apktool d -f -s -o "$WORK/base_dec" "$ORIG_APK" >/dev/null
python3 - "$WORK/base_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding='utf-8')
old = 'android:name="dev.mars.secure.ProxyApplication"'
new = 'android:name="com.starry.magic.dwdlm.DwdlmApplication"'
if old not in t:
    raise SystemExit('ProxyApplication manifest entry not found')
p.write_text(t.replace(old, new, 1), encoding='utf-8')
PY
rm -rf "$WORK/base_dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/base_dec" -o "$WORK/manifest.apk" >/dev/null
unzip -p "$WORK/manifest.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[3/6] surgical assemble spoof-only apk"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" <<'PY'
import sys, zipfile
from pathlib import Path
orig, work, dst = map(Path, sys.argv[1:4])
dex = (work / 'classes2.dex').read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
replacements = {'classes2.dex': dex, 'AndroidManifest.xml': manifest}
with zipfile.ZipFile(orig, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    for item in zin.infolist():
        name = item.filename
        if name in replacements:
            continue
        if name == 'META-INF/MANIFEST.MF':
            continue
        if name.startswith('META-INF/') and name.endswith(('.SF', '.RSA', '.DSA', '.EC')):
            continue
        data = replacements.get(name)
        if data is None:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == 'resources.arsc' else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
    for name, data in replacements.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
PY

"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify "$SIGNED" >/dev/null

echo "[4/6] verify spoof-only structure"
unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q 'classes2.dex' "$WORK/entries.txt" || { echo "ERROR: classes2.dex missing" >&2; exit 1; }
if grep -q 'lib/arm64-v8a/libstarry_magic.so' "$WORK/entries.txt"; then
  echo "ERROR: spoof-only contains libstarry_magic.so" >&2
  exit 1
fi
if unzip -p "$SIGNED" AndroidManifest.xml | strings | grep -q 'StarryInitProvider'; then
  echo "ERROR: spoof-only manifest still contains StarryInitProvider" >&2
  exit 1
fi
echo "spoof-only ok: no starry so/provider"

echo "[5/6] unsigned=$UNSIGNED"
echo "[6/6] signed=$SIGNED md5=$(md5 -q "$SIGNED")"

if [[ "${DWDLM_SPOOF_INSTALL:-0}" == "1" ]]; then
  if ! "$ADB_BIN" -s "$SERIAL" install -r "$SIGNED"; then
    echo "[spoof-only] install -r failed; uninstall current package and retry"
    "$ADB_BIN" -s "$SERIAL" shell su -c "pm uninstall --user 0 $PKG" >/dev/null
    "$ADB_BIN" -s "$SERIAL" install -r "$SIGNED"
  fi
  if [[ "${DWDLM_SPOOF_CLEAR:-0}" == "1" ]]; then
    "$ADB_BIN" -s "$SERIAL" shell su -c "pm clear $PKG" >/dev/null
  fi
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su
  echo "spoof-only installed"
fi
