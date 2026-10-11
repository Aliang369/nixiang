#!/usr/bin/env bash
# 一念通天 747 (com.qinglv.Immortal) — 外科组装。
#
# 原包零改动：classes.dex / classes2.dex / resources.arsc / lib/**.so / metadata 字节不变。
# 改动：
#   - AndroidManifest：保留原 GameApplication，仅新增 StarryInitProvider / MenuService 与前台服务权限
#   - 新增 classes3.dex（星光 smali）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#   - 保留官方 META-INF 非签名条目，移除旧签名条目
#
# 产物：08_output/一念通天/一念通天_747_surgical-unsigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
GAME="一念通天"
VERSION="747"
PKG="com.qinglv.Immortal"
ORIG_APK="$NIXIANG/02_apks/$GAME/${GAME}_747_official_from_device.apk"
ORIG_MD5="1d035885b2aea678c6c9acafb39241d8"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
NEW_DEX="classes3.dex"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${YNTT_SO:-$STARRY_SRC/libs_yntt/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing modern so $SO_PREBUILT" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 原包指纹不符" >&2; exit 1; }

LOCK_DIR="/tmp/yntt_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/yntt_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] gradle assembleRelease (starry → smali)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
YNTT_SMALI="$STARRY_SRC/yntt_smali"
[[ -d "$YNTT_SMALI" ]] || { echo "ERROR: missing $YNTT_SMALI" >&2; exit 1; }
rsync -a "$YNTT_SMALI/" "$SMALI_ROOT/"
[[ -f "$SMALI_ROOT/com/starry/magic/sign/YnttLoginRequest.smali" ]] || { echo "ERROR: YnttLoginRequest missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/sign/YnttLoginRequest\$1.smali" ]] || { echo "ERROR: YnttLoginRequest CREATOR missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] || { echo "ERROR: StarryInitProvider missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] || { echo "ERROR: MenuService missing" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] AndroidManifest（保留官方 GameApplication）"
apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
if "FOREGROUND_SERVICE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg
    t = t.replace("</application>", prov + "\n    </application>", 1)
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  manifest patched: provider + service + foreground permissions")
PY
apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null || {
    echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1;
  }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" || { echo "ERROR: FGS permission missing" >&2; exit 1; }
  grep -q "com.qk.game.GameApplication" "$WORK/tree_new.txt" || { echo "ERROR: official GameApplication lost" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $DIFF_D 行" >&2; exit 1; }
fi

echo "[4/6] zip surgical merge → $UNSIGNED"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
src, work, dst, new_dex = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3]), sys.argv[4]
mf = (work / "AndroidManifest.xml").read_bytes()
cnew = (work / new_dex).read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
SIG_NAMES = {"META-INF/MANIFEST.MF"}
untouched = ["classes.dex", "classes2.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libtapsdkcore.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"]
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names
for n in untouched:
    assert n in names, f"orig apk missing {n}"
def is_sig(name):
    if name in SIG_NAMES: return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))
kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else (zipfile.ZIP_DEFLATED if name == "AndroidManifest.xml" else item.compress_type)
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
info = zipfile.ZipInfo(new_dex); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
zout.writestr(info, cnew); n += 1
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name); info.compress_type = zipfile.ZIP_DEFLATED; info.create_system = 3
    zout.writestr(info, data); n += 1
zin.close(); zout.close()
zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zout2.namelist())
for name in kept_meta: assert name in out_names, f"lost {name}"
assert new_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert not any(x.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print(f"  merged ok; entries≈{n}; {new_dex}={len(cnew)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

echo "[5/6] 闭环校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] || { echo "ERROR: so md5 mismatch" >&2; exit 1; }
IN_SO_TAG="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | LC_ALL=C grep -ac 'yntt' || true)"
[[ "$IN_SO_TAG" -ge 1 ]] || { echo "ERROR: so tag yntt missing" >&2; exit 1; }

echo "[6/6] 完成（未签名）"
ls -lh "$UNSIGNED"
echo "DONE(unsigned): $UNSIGNED"
echo "NOTE: 由 04_scripts/一念通天/yntt_sign_747.sh 签名。"
