#!/usr/bin/env bash
# 弹壳战机 1.1.6 (vc27) — 星光外科组装。
#
# 原包游戏本体不变：classes.dex..classes4.dex / resources.arsc /
# assets/** / 原 lib so 逐字节保留。仅：
#   - AndroidManifest.xml：appComponentFactory -> SignSpoofAppComponentFactory，
#     增加 StarryInitProvider、MenuService、前台服务权限
#   - 新增 classes5.dex（星光 Java/smali）
#   - 新增 libstarry_magic.so + libc++_shared.so
#
# 产物：08_output/蛋壳战机/蛋壳战机_1.1.6_starry-unsigned.apk
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="蛋壳战机"
VERSION="1.1.6"
PKG="com.survivor.acecn"
ORIG_APK="${DKZJ_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk}"
ORIG_MD5="${DKZJ_BASE_MD5:-6bd3f3dea5eb38fce6be8d8038d70fb3}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_starry-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-gnu/libc++_shared.so"
[[ -f "$NDK_CXX" ]] || NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${DKZJ_SO:-$STARRY_SRC/libs_dkzj/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing $SO_PREBUILT" >&2; exit 1; }
echo "[check] modern so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
ACTUAL_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] \
  || { echo "ERROR: 原包指纹不符 $ACTUAL_MD5（期望 $ORIG_MD5）" >&2; exit 1; }

LOCK_DIR="/tmp/dkzj_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/dkzj_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease (starry -> smali + certs)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → classes5.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for need in \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/SignSpoof.smali" \
  "com/starry/magic/sign/SignSpoofAppComponentFactory.smali" \
  "com/starry/magic/sign/TapLoginForceWeb.smali"; do
  [[ -f "$SMALI_ROOT/$need" ]] || { echo "ERROR: $need missing" >&2; exit 1; }
done

"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -la "$WORK/classes5.dex"
STAGE_DIR="$NIXIANG/07_patched/$GAME/surgical"
mkdir -p "$STAGE_DIR"
cp -f "$WORK/classes5.dex" "$STAGE_DIR/classes5.dex"
echo "  classes5.dex → $STAGE_DIR/classes5.dex ($(md5 -q "$STAGE_DIR/classes5.dex"))"

# Java/jni 注册面交叉校验
MB_JAVA="$STARRY_SRC/app/src/main/java/com/starry/magic/bridge/ModBridge.java"
[[ -f "$MB_JAVA" ]] || { echo "ERROR: missing $MB_JAVA" >&2; exit 1; }
NATIVE_TOTAL=0
NATIVE_MISS=0
for m in $(LC_ALL=C grep -oE '\bnative[A-Za-z0-9_]+' "$MB_JAVA" | sort -u); do
  NATIVE_TOTAL=$((NATIVE_TOTAL + 1))
  LC_ALL=C grep -aqF "$m" "$WORK/classes5.dex" || {
    echo "ERROR: classes5.dex 缺 native 声明 $m" >&2
    NATIVE_MISS=$((NATIVE_MISS + 1))
  }
done
[[ "$NATIVE_MISS" -eq 0 ]] \
  || { echo "ERROR: classes5.dex 与 ModBridge.java 声明不一致（$NATIVE_MISS/$NATIVE_TOTAL 缺失）" >&2; exit 1; }
echo "  dex↔ModBridge native 声明校验通过（$NATIVE_TOTAL 项）"

echo "[3/6] binary AndroidManifest（保留原 Application + provider/service/factory）"
if [[ -n "${DKZJ_BINARY_MANIFEST:-}" && -f "${DKZJ_BINARY_MANIFEST:-}" ]]; then
  cp "$DKZJ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $DKZJ_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
authority = sys.argv[3]
t = p.read_text(encoding="utf-8")

perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# Original has no custom Application. Keep that attribute absent and inject
# SignSpoof before application creation through AppComponentFactory.
old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.SignSpoofAppComponentFactory"'
if new_factory not in t:
    if old_factory not in t:
        raise SystemExit("expected CoreComponentFactory not found")
    t = t.replace(old_factory, new_factory, 1)

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % authority
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
print("  text Manifest patched (Application preserved)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for need in \
    "com.starry.magic.StarryInitProvider" \
    "com.starry.magic.menu.MenuService" \
    "com.starry.magic.sign.SignSpoofAppComponentFactory" \
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"; do
    grep -q "$need" "$WORK/tree_new.txt" \
      || { echo "ERROR: 新 manifest 缺 $need" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  python3 - "$WORK/o.norm" "$WORK/n.norm" <<'PY'
import sys
from collections import Counter
from pathlib import Path

def canon(path):
    stack, out = [], []
    for ln in Path(path).read_text(encoding='utf-8').splitlines():
        if not ln.strip():
            continue
        ind = len(ln) - len(ln.lstrip(' '))
        core = ln.strip()
        if core.startswith('N:'):
            continue
        if core.startswith('E:'):
            lv = (ind + 2) // 4
            stack = stack[:lv - 1]
            stack.append(core[2:].strip().split(' ')[0])
            out.append('/'.join(stack))
        elif core.startswith('A:') and stack:
            out.append('/'.join(stack[:ind // 4]) + '|' + core)
    return Counter(out)

o, n = canon(sys.argv[1]), canon(sys.argv[2])
dropped = o - n
added = sum((n - o).values())
unexpected = {
    k: v for k, v in dropped.items()
    if 'androidx.core.app.CoreComponentFactory' not in k
}
if unexpected:
    for k, v in list(unexpected.items())[:10]:
        print(f'  LOST x{v}: {k}', file=sys.stderr)
    raise SystemExit(f'manifest 重建丢失官方节点（{sum(unexpected.values())} 项）')
if not (18 <= added <= 22):
    raise SystemExit(f'manifest 新增节点数异常（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 只丢弃旧 factory)')
PY
else
  echo "  WARN: aapt2 不在 $AAPT2，跳过 manifest 回读校验" >&2
fi

echo "[5/6] surgical zip merge（原 dex/arsc/assets/lib 字节不变）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx_path = Path(sys.argv[5])
cxx = cxx_path.read_bytes() if cxx_path.is_file() else b""
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes5 = (work / 'classes5.dex').read_bytes()

replace = {'AndroidManifest.xml': manifest}
extras = {
    'classes5.dex': classes5,
    'lib/arm64-v8a/libstarry_magic.so': so,
}
if cxx:
    extras['lib/arm64-v8a/libc++_shared.so'] = cxx

GUARD = {
    'classes.dex', 'classes2.dex', 'classes3.dex', 'classes4.dex',
    'resources.arsc', 'lib/arm64-v8a/libil2cpp.so',
    'lib/arm64-v8a/libtapsdkcore.so', 'lib/arm64-v8a/libunity.so',
    'lib/arm64-v8a/libmain.so',
    'assets/bin/Data/Managed/Metadata/global-metadata.dat',
}

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name.startswith('META-INF/') and (name == 'META-INF/MANIFEST.MF'
                                             or name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if name in extras:
            continue
        data = replace.get(name)
        if data is None:
            data = zin.read(item)
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

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    changed = 0
    for n in original_names:
        if n.startswith('META-INF/') and (n == 'META-INF/MANIFEST.MF'
                                          or n.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        changed += 1
    for n in GUARD:
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'protected entry changed: {n}')
    if 'classes5.dex' not in out_names:
        raise SystemExit('classes5.dex missing')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'surgical zip verified: {changed} 个原始条目字节不变 + classes5.dex + starry so')
PY

echo "[6/6] surgical done"
echo "[surgical] unsigned = $UNSIGNED"
echo "[surgical] md5 = $(md5 -q "$UNSIGNED")"
ls -lh "$UNSIGNED"
