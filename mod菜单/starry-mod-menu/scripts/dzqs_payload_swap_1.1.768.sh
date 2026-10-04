#!/usr/bin/env bash
# 独奏骑士 1.1.768 — 新菜单载荷置换
#
# 用途：不再从官方底包重打，只以现网过验的星光包为底包，
#       替换新版 classes2.dex（v5 设备指纹菜单）与 libstarry_magic.so。
#
# 默认输入：08_output/独奏骑士/独奏骑士_1.1.768_surgical-releaseSigned.apk
# 默认输出：08_output/独奏骑士/独奏骑士_1.1.768_v5-releaseSigned.apk
#
# 用法：
#   bash scripts/dzqs_payload_swap_1.1.768.sh
#   STARRY_INSTALL=1 bash scripts/dzqs_payload_swap_1.1.768.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
OUT_DIR="$NIXIANG/08_output/独奏骑士"
BASE_APK="${DZQS_BASE_APK:-$OUT_DIR/独奏骑士_1.1.768_surgical-releaseSigned.apk}"
OUT_APK="${DZQS_OUT_APK:-$OUT_DIR/独奏骑士_1.1.768_v5-releaseSigned.apk}"
PKG="com.Shimmer.SoloKnight.cn"
SERIAL="${STARRY_SERIAL:-f9a116a2}"

BASE_MD5="7fceba7fd0baa92ca05ffb08977962e1"
BASE_CLASSES2_MD5="ff191e1f472537f496a3160d6b50d545"
BASE_SO_MD5="e703fdf4d08407e412b104e91449cdf0"

NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
KS="$NIXIANG/01_tools/starry_release.jks"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
PRE_GATE="$NIXIANG/04_scripts/_common/pre_install_gate.sh"
VERIFY="$NIXIANG/04_scripts/_common/verify_installed_apk.sh"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"
    break
  fi
done

[[ -f "$BASE_APK" ]] || { echo "ERROR: 底包不存在: $BASE_APK" >&2; exit 1; }
[[ ! -e "$OUT_APK" ]] || { echo "ERROR: 输出已存在，先移走或指定 DZQS_OUT_APK: $OUT_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: build-tools not found" >&2; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool not found" >&2; exit 1; }

actual_base_md5="$(md5 -q "$BASE_APK")"
[[ "$actual_base_md5" == "$BASE_MD5" ]] \
  || { echo "ERROR: 底包 md5=$actual_base_md5 != $BASE_MD5" >&2; exit 1; }

lock="/tmp/dzqs_payload_swap_1768.lockdir"
mkdir "$lock" 2>/dev/null || { echo "ERROR: lock held: $lock" >&2; exit 1; }
work="$(mktemp -d /tmp/dzqs_payload_swap.XXXXXX)"
remote="/data/local/tmp/dzqs_v5.apk"
cleanup() {
  rm -rf "$work" "$lock"
  if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
    adb -s "$SERIAL" shell "rm -f $remote" >/dev/null 2>&1 || true
  fi
}
trap cleanup EXIT

echo "[1/5] ndk-build (DZQS)"
(cd "$STARRY_SRC/app/src/main/jni" && \
  STARRY_GAME_DZQS=1 "$NDK/ndk-build" \
    NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
new_so="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$new_so" ]] || { echo "ERROR: missing $new_so" >&2; exit 1; }
grep -aq '\[Dzqs\]' "$new_so" || { echo "ERROR: so missing [Dzqs]" >&2; exit 1; }
if grep -aq '\[Dwct\]' "$new_so" || grep -aq '\[Tlgd\]' "$new_so"; then
  echo "ERROR: so contains another game tag" >&2
  exit 1
fi
for dst in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dst"
  cp -f "$new_so" "$dst/libstarry_magic.so"
done
echo "  so md5=$(md5 -q "$new_so")"

echo "[2/5] gradle assembleRelease -> collect com/starry -> classes2.dex"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$work/starry/"
(cd "$work/starry" && ./gradlew assembleRelease --offline --console=plain)
starry_apk="$work/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$starry_apk" ]] || { echo "ERROR: missing $starry_apk" >&2; exit 1; }
apktool d -f -o "$work/starry_dec" "$starry_apk" >/dev/null

smali_root="$work/smali_in"
mkdir -p "$smali_root"
shopt -s nullglob
for d in "$work/starry_dec"/smali "$work/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$smali_root/com/"
done
shopt -u nullglob
[[ -f "$smali_root/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing" >&2; exit 1; }

python3.11 - "$smali_root/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/sgsdk/core/SGGlobalApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/sgsdk/core/SGGlobalApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/sgsdk/core/SGGlobalApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/sgsdk/core/SGGlobalApplication;->onCreate()V",
    ),
]
for a, b in repls:
    t = t.replace(a, b)
if "Lcom/sgsdk/core/SGGlobalApplication;" not in t:
    raise SystemExit("ERROR: StarryApp missing SGGlobalApplication")
bad = [x for x in (
    ".super Landroid/app/Application;",
    "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
    "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
    "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
) if x in t]
if bad:
    raise SystemExit("ERROR: Application super dispatch remains: " + "; ".join(bad))
p.write_text(t, encoding="utf-8")
PY
new_dex="$work/classes2.dex"
"$SMALI_BIN" assemble -o "$new_dex" "$smali_root"
grep -aq 'ver=v5' "$new_dex" || { echo "ERROR: classes2 missing v5 marker" >&2; exit 1; }
grep -aq 'auth_legacy_device_ids' "$new_dex" || { echo "ERROR: classes2 missing legacy device key" >&2; exit 1; }
echo "  classes2.dex md5=$(md5 -q "$new_dex")"

echo "[3/5] replace only classes2.dex + libstarry_magic.so"
python3.11 - "$BASE_APK" "$work/unsigned.apk" "$new_dex" "$new_so" \
  "$BASE_CLASSES2_MD5" "$BASE_SO_MD5" <<'PY'
import hashlib
import sys
import zipfile
from pathlib import Path

base, dst, new_dex, new_so = map(Path, sys.argv[1:5])
want_c2, want_so = sys.argv[5], sys.argv[6]
targets = {"classes2.dex", "lib/arm64-v8a/libstarry_magic.so"}

with zipfile.ZipFile(base, "r") as z:
    got_c2 = hashlib.md5(z.read("classes2.dex")).hexdigest()
    got_so = hashlib.md5(z.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest()
if got_c2 != want_c2:
    raise SystemExit(f"base classes2 md5 {got_c2} != {want_c2}")
if got_so != want_so:
    raise SystemExit(f"base so md5 {got_so} != {want_so}")

new_data = {
    "classes2.dex": new_dex.read_bytes(),
    "lib/arm64-v8a/libstarry_magic.so": new_so.read_bytes(),
}
with zipfile.ZipFile(base, "r") as zin, zipfile.ZipFile(dst, "w", allowZip64=True) as zout:
    for item in zin.infolist():
        if item.filename.startswith("META-INF/"):
            continue
        data = new_data.get(item.filename)
        if data is None:
            data = zin.read(item)
        info = zipfile.ZipInfo(item.filename, item.date_time)
        info.compress_type = item.compress_type
        info.comment = item.comment
        info.extra = item.extra
        info.internal_attr = item.internal_attr
        info.external_attr = item.external_attr
        info.create_system = item.create_system
        zout.writestr(info, data)

def manifest(z):
    out = []
    for i in z.infolist():
        if i.filename.startswith("META-INF/"):
            continue
        out.append((i.filename, hashlib.sha256(z.read(i)).hexdigest()))
    return out

with zipfile.ZipFile(base, "r") as a, zipfile.ZipFile(dst, "r") as b:
    ma = dict(manifest(a))
    mb = dict(manifest(b))
    if set(ma) != set(mb):
        raise SystemExit("entry set changed unexpectedly")
    for name in ma:
        if name in targets:
            continue
        if ma[name] != mb[name]:
            raise SystemExit(f"non-target entry changed: {name}")
    if hashlib.md5(b.read("classes2.dex")).hexdigest() != hashlib.md5(new_dex.read_bytes()).hexdigest():
        raise SystemExit("classes2.dex copy mismatch")
    if hashlib.md5(b.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest() != hashlib.md5(new_so.read_bytes()).hexdigest():
        raise SystemExit("starry so copy mismatch")
print("  zip audit ok: only classes2.dex + libstarry_magic.so changed")
PY

echo "[4/5] zipalign + sign"
aligned="$work/aligned.apk"
"$BT/zipalign" -f -p 4 "$work/unsigned.apk" "$aligned"
mkdir -p "$OUT_DIR"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "${STARRY_KS_ALIAS:-starry}" \
  --ks-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --key-pass "pass:${STARRY_KS_PASS:-starry2026}" \
  --out "$OUT_APK" "$aligned"
"$BT/apksigner" verify --print-certs "$OUT_APK" >/dev/null
echo "  output=$OUT_APK"
echo "  md5=$(md5 -q "$OUT_APK")"

echo "[5/5] pre-install gate"
bash "$PRE_GATE" \
  --cpp "$STARRY_SRC/app/src/main/jni/Games/DzqsMod.cpp" \
  --dex "$new_dex" \
  --so "$new_so"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install, keep data"
  adb -s "$SERIAL" push "$OUT_APK" "$remote"
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$remote\"'"
  bash "$VERIFY" "$SERIAL" "$PKG" "$OUT_APK" --su
fi

echo "DONE: $OUT_APK"
