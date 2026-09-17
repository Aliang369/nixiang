#!/usr/bin/env bash
# 动物大联盟 1.0.23 - shell startup isolation probe.
# Builds the original transport path with only SignSpoof + APKSecure shell
# startup hooks enabled. Network/session/header/signature probe hooks and the
# assets/main.js bootstrap patch are excluded at compile/build time.
set -euo pipefail

MODE="${1:-build}"
case "$MODE" in
  build|install) ;;
  *) echo "ERROR: mode must be build or install" >&2; exit 1 ;;
esac

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
ORIG_APK="${DWDLM_BASE_APK:-$NIXIANG/02_apks/动物大联盟/动物大联盟_1.0.23_official.apk}"
ORIG_MD5="${DWDLM_BASE_MD5:-211cfebb2d273360e22ea3593fdaedd6}"
STARRY_UNSIGNED="${DWDLM_STARRY_UNSIGNED:-$NIXIANG/07_patched/动物大联盟/动物大联盟_1.0.23_starry-unsigned.apk}"
OUT_DIR="$NIXIANG/08_output/动物大联盟/variants"
UNSIGNED_DIR="$NIXIANG/07_patched/动物大联盟/variants"
VARIANT="${DWDLM_SHELL_VARIANT:-shell}"
case "$VARIANT" in
  shell|state|data) ;;
  *) echo "ERROR: DWDLM_SHELL_VARIANT must be shell or state" >&2; exit 1 ;;
esac
UNSIGNED="$UNSIGNED_DIR/${VARIANT}-only-unsigned.apk"
SIGNED="$OUT_DIR/${VARIANT}-only-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PKG="com.fightingstars.games.animal"
ADB_BIN="${ADB_BIN:-adb}"
SERIAL_FILE="$SCRIPT_DIR/.device_serial"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
JNI="$STARRY_SRC/app/src/main/jni"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
OBJ="$STARRY_SRC/obj_dwdlm_${VARIANT}"
OUT_LIBS="$STARRY_SRC/libs_dwdlm_${VARIANT}"
SO="$OUT_LIBS/arm64-v8a/libstarry_magic.so"
PATCH_JS="$SCRIPT_DIR/patch_dwdlm.js"

if [[ "$MODE" == "install" ]]; then
  [[ -f "$SIGNED" ]] || { echo "ERROR: missing shell-only APK $SIGNED" >&2; exit 1; }
  [[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
  SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
  [[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }
  if ! "$ADB_BIN" -s "$SERIAL" install -r "$SIGNED"; then
    echo "[shell-only] install -r failed; uninstall current package and retry"
    "$ADB_BIN" -s "$SERIAL" shell su -c "pm uninstall --user 0 $PKG" >/dev/null
    "$ADB_BIN" -s "$SERIAL" install -r "$SIGNED"
  fi
  "$ADB_BIN" -s "$SERIAL" shell su -c "pm clear $PKG" >/dev/null
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su
  echo "[shell-only] installed and cleared $PKG"
  exit 0
fi

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || {
  echo "ERROR: base APK fingerprint mismatch" >&2
  exit 1
}
[[ -f "$STARRY_UNSIGNED" ]] || { echo "ERROR: missing starry unsigned $STARRY_UNSIGNED" >&2; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS" >&2; exit 1; }
[[ -f "$PATCH_JS" ]] || { echo "ERROR: missing $PATCH_JS" >&2; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK" >&2; exit 1; }
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared at $NDK_CXX" >&2; exit 1; }

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

mkdir -p "$OUT_DIR" "$UNSIGNED_DIR" "$OBJ/generated"
WORK="$(mktemp -d /tmp/dwdlm_shell_probe.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/6] build shell-only so"
BUILD_VARS=(STARRY_SHELL_ONLY=1)
if [[ "$VARIANT" == "state" ]]; then
  BUILD_VARS+=(STARRY_SHELL_STATE_ONLY=1)
fi
if [[ "$VARIANT" == "data" ]]; then
  BUILD_VARS=(STARRY_DATA_GUARD_ONLY=1)
fi
CFG_HASH="$( (cat "$JNI/Android_dwdlm.mk" "$JNI/Application.mk" \
  "$JNI/Games/DwdlmMod.cpp" "$JNI/Games/DwdlmEntry.cpp" "$PATCH_JS"; \
  printf 'SHELL_ONLY=1\nVARIANT=%s\n' "$VARIANT") | shasum -a 256 | awk '{print $1}')"
STAMP="$OBJ/.build_stamp"
if [[ -f "$STAMP" && "$(cat "$STAMP" 2>/dev/null)" != "$CFG_HASH" ]]; then
  echo "[stamp] $VARIANT build config changed -> clean $(basename "$OBJ")"
  rm -rf "$OBJ"
  mkdir -p "$OBJ/generated"
fi
python3 - "$PATCH_JS" "$OBJ/generated/DwdlmPatchJs.h" <<'PY'
import sys
from pathlib import Path
text = Path(sys.argv[1]).read_text(encoding='utf-8')
if ')DWDLMJS"' in text:
    raise SystemExit('patch JS contains raw-string terminator')
new = 'static const char kPatchJs[] = R"DWDLMJS(' + text + ')DWDLMJS";\n'
dst = Path(sys.argv[2])
if not dst.exists() or dst.read_text(encoding='utf-8') != new:
    dst.write_text(new, encoding='utf-8')
PY
export STARRY_DWDLM_GEN="$OBJ/generated"
SO_MD5_BEFORE="$(md5 -q "$SO" 2>/dev/null || echo none)"
T0="$(mktemp)"; touch "$T0"
BUILD_RC=0
"$NDK/ndk-build" -C "$JNI" \
  NDK_PROJECT_PATH="../.." \
  APP_BUILD_SCRIPT="./Android_dwdlm.mk" \
  NDK_APPLICATION_MK="./Application.mk" \
  NDK_OUT="$OBJ" \
  NDK_LIBS_OUT="$OUT_LIBS" \
  "${BUILD_VARS[@]}" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 8)" || BUILD_RC=$?
NEW_O="$(find "$OBJ" -name '*.o' -newer "$T0" 2>/dev/null | head -1)"
rm -f "$T0"
[[ $BUILD_RC -eq 0 ]] || { echo "ERROR: $VARIANT ndk-build rc=$BUILD_RC" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: $VARIANT build produced no $SO" >&2; exit 1; }
if [[ -n "$NEW_O" && "$(md5 -q "$SO")" == "$SO_MD5_BEFORE" ]]; then
  echo "ERROR: new .o produced but $VARIANT so did not change" >&2
  exit 1
fi
echo "$CFG_HASH" > "$STAMP"

LC_ALL=C grep -aq "dwdlm" "$SO" || { echo "ERROR: so missing dwdlm tag" >&2; exit 1; }
if [[ "$VARIANT" == "shell" ]]; then
  LC_ALL=C grep -aq "boot shellGateReady -> 1" "$SO" || { echo "ERROR: so missing shellGateReady hook" >&2; exit 1; }
elif [[ "$VARIANT" == "state" ]]; then
  LC_ALL=C grep -aq "state-only: skip cocos boot hooks" "$SO" || { echo "ERROR: so missing state-only marker" >&2; exit 1; }
else
  LC_ALL=C grep -aq "data-guard only: start shell state polling" "$SO" || { echo "ERROR: so missing data-guard marker" >&2; exit 1; }
fi
if [[ "$VARIANT" != "data" ]]; then
  LC_ALL=C grep -aq "onShellCreate enter api=" "$SO" || { echo "ERROR: so missing onShellCreate hook" >&2; exit 1; }
fi
for marker in \
  "local session bypass applied" \
  "config baseUrl before=" \
  "http header[" \
  "sign key=" \
  "boot updateTaskEnqueue enter" \
  "starry_dwdlm_v1"; do
  if LC_ALL=C grep -aqF "$marker" "$SO"; then
    echo "ERROR: $VARIANT so still contains disabled marker: $marker" >&2
    exit 1
  fi
done
if [[ "$VARIANT" == "state" ]] && LC_ALL=C grep -aqF "hooked export Require" "$SO"; then
  echo "ERROR: state-only so still installs secret export hooks" >&2
  exit 1
fi
if [[ "$VARIANT" == "data" ]]; then
  for marker in "hooked check-report" "hooked onShellCreate" "hooked export" "patched cocos shell"; do
    if LC_ALL=C grep -aqF "$marker" "$SO"; then
      echo "ERROR: data-guard so still contains code-hook marker: $marker" >&2
      exit 1
    fi
  done
fi
echo "[$VARIANT-only] so md5=$(md5 -q "$SO")"

echo "[2/6] extract reusable Java payload from full starry unsigned"
unzip -p "$STARRY_UNSIGNED" classes2.dex > "$WORK/classes2.dex"
unzip -p "$STARRY_UNSIGNED" lib/arm64-v8a/libc++_shared.so > "$WORK/libc++_shared.so"
[[ -s "$WORK/classes2.dex" && -s "$WORK/libc++_shared.so" ]] || {
  echo "ERROR: failed to extract Java payload" >&2
  exit 1
}

echo "[3/6] patch manifest: DwdlmApplication only, no provider/service"
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
t = t.replace(old, new, 1)
if 'StarryInitProvider' in t or 'MenuService' in t:
    raise SystemExit('unexpected starry provider/service in base manifest')
p.write_text(t, encoding='utf-8')
PY
rm -rf "$WORK/base_dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/base_dec" -o "$WORK/manifest.apk" >/dev/null
unzip -p "$WORK/manifest.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[4/6] surgical assemble shell-only apk"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO" "$WORK/libc++_shared.so" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
orig, work, dst, so_path, cxx_path = map(Path, sys.argv[1:6])
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes2 = (work / 'classes2.dex').read_bytes()
cxx = cxx_path.read_bytes()
so = so_path.read_bytes()
extras = {
    'classes2.dex': classes2,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
with zipfile.ZipFile(orig, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name == 'META-INF/MANIFEST.MF':
            continue
        if name.startswith('META-INF/') and name.endswith(('.SF', '.RSA', '.DSA', '.EC')):
            continue
        if name in extras:
            continue
        data = manifest if name == 'AndroidManifest.xml' else zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == 'resources.arsc' else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
    for name, data in extras.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name.startswith('lib/') and name.endswith('.so') else zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)
with zipfile.ZipFile(orig, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    for n in original_names:
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in extras or n == 'AndroidManifest.xml':
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
    for n in extras:
        if n not in out_names:
            raise SystemExit(f'missing extra: {n}')
    if hashlib.sha256(zout.read('assets/main.js')).digest() != hashlib.sha256(zin.read('assets/main.js')).digest():
        raise SystemExit('assets/main.js was modified')
    if hashlib.sha256(zout.read('lib/arm64-v8a/libstarry_magic.so')).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged shell-only so != build output')
print('shell-only surgical zip verified: transport path + pristine main.js')
PY

echo "[5/6] sign and verify"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$WORK/aligned.apk"
"$BT/apksigner" verify "$SIGNED" >/dev/null

unzip -l "$SIGNED" > "$WORK/entries.txt"
grep -q 'classes2.dex' "$WORK/entries.txt" || { echo "ERROR: classes2.dex missing" >&2; exit 1; }
grep -q 'lib/arm64-v8a/libstarry_magic.so' "$WORK/entries.txt" || { echo "ERROR: shell-only so missing" >&2; exit 1; }
if unzip -p "$SIGNED" AndroidManifest.xml | strings | grep -Eq 'StarryInitProvider|MenuService'; then
  echo "ERROR: shell-only manifest contains menu/provider" >&2
  exit 1
fi

echo "[6/6] shell-only build complete"
echo "unsigned=$UNSIGNED"
echo "signed=$SIGNED"
echo "md5=$(md5 -q "$SIGNED")"
