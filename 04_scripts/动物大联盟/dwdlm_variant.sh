#!/usr/bin/env bash
# 动物大联盟 1.0.23 — 修改差异隔离变体构建/签名/可选安装。
# 只用于定位“哪一处星光改动让原版下载路径失效”，不是正式出包脚本。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
ORIG_APK="${DWDLM_BASE_APK:-$NIXIANG/02_apks/动物大联盟/动物大联盟_1.0.23_official.apk}"
ORIG_MD5="${DWDLM_BASE_MD5:-211cfebb2d273360e22ea3593fdaedd6}"
STARRY_UNSIGNED="${DWDLM_STARRY_UNSIGNED:-$NIXIANG/07_patched/动物大联盟/动物大联盟_1.0.23_starry-unsigned.apk}"
VARIANT="${DWDLM_VARIANT:-resign}"
OUT_DIR="$NIXIANG/08_output/动物大联盟/variants"
UNSIGNED_DIR="$NIXIANG/07_patched/动物大联盟/variants"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PKG="com.fightingstars.games.animal"
ADB_BIN="${ADB_BIN:-adb}"
SERIAL="$(tr -d '[:space:]' < "$SCRIPT_DIR/.device_serial")"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || {
  echo "ERROR: base APK fingerprint mismatch" >&2
  exit 1
}
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$STARRY_UNSIGNED" ]] || { echo "ERROR: missing starry unsigned $STARRY_UNSIGNED" >&2; exit 1; }
[[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }

case "$VARIANT" in
  resign|dex|so|dextso|app) ;;
  *)
    echo "ERROR: DWDLM_VARIANT must be resign|dex|so|dextso|app" >&2
    exit 1
    ;;
esac

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
WORK="$(mktemp -d /tmp/dwdlm_variant.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

UNSIGNED="$UNSIGNED_DIR/${VARIANT}-unsigned.apk"
SIGNED="$OUT_DIR/${VARIANT}-releaseSigned.apk"

python3 - "$ORIG_APK" "$STARRY_UNSIGNED" "$UNSIGNED" "$VARIANT" <<'PY'
import sys
import zipfile
from pathlib import Path

orig, starry, dst, variant = map(Path, sys.argv[1:5])
variant = variant.decode() if isinstance(variant, bytes) else str(variant)

overlay_names = set()
if variant in {"dex", "dextso", "app"}:
    overlay_names.add("classes2.dex")
if variant in {"so", "dextso", "app"}:
    overlay_names.update({
        "lib/arm64-v8a/libstarry_magic.so",
        "lib/arm64-v8a/libc++_shared.so",
    })
if variant == "app":
    overlay_names.update({"AndroidManifest.xml", "assets/main.js"})

replacements = {}
if overlay_names:
    with zipfile.ZipFile(starry, "r") as zs:
        for name in overlay_names:
            try:
                replacements[name] = zs.read(name)
            except KeyError:
                raise SystemExit(f"missing overlay entry in starry unsigned: {name}")

with zipfile.ZipFile(orig, "r") as zin, zipfile.ZipFile(dst, "w") as zout:
    for item in zin.infolist():
        name = item.filename
        if name == "META-INF/MANIFEST.MF":
            continue
        if name.startswith("META-INF/") and name.endswith((".SF", ".RSA", ".DSA", ".EC")):
            continue
        data = replacements.get(name)
        if data is None:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
    for name, data in replacements.items():
        info = zipfile.ZipInfo(name)
        if name.startswith("lib/") and name.endswith(".so"):
            info.compress_type = zipfile.ZIP_STORED
        else:
            info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)

print(f"variant={variant} unsigned={dst}")
PY

"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify "$SIGNED" >/dev/null

echo "[variant] built $SIGNED"
echo "[variant] md5=$(md5 -q "$SIGNED")"

if [[ "${DWDLM_VARIANT_INSTALL:-0}" == "1" ]]; then
  "$ADB_BIN" -s "$SERIAL" install -r "$SIGNED"
  if [[ "${DWDLM_VARIANT_CLEAR:-0}" == "1" ]]; then
    "$ADB_BIN" -s "$SERIAL" shell su -c "pm clear $PKG" >/dev/null
  fi
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su
  echo "[variant] installed $VARIANT"
fi
