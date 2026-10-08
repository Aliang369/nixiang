#!/usr/bin/env bash
# 我的修仙世界 4.5 (com.lingsi.xiuxiandanji) TapTap — 外科组装
#
# 官包零改动：classes.dex / classes2.dex / resources.arsc / lib/**.so 字节不变。
# 改动：
#   - 改 AndroidManifest（保留原 application 与 appComponentFactory="android.support.v4.app.CoreComponentFactory"；
#     只 +StarryInitProvider + MenuService + FOREGROUND_SERVICE 两条权限）
#   - 新增 classes3.dex（星光 smali：StarryInitProvider / MenuService / SignSpoof / TapLoginForceWeb / OTA）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#   - 保留官方 META-INF 非签名条目（services/ 下的 ServiceLoader 清单）
# 功能全在 native（WdxsjMod.cpp：百倍伤害 / 无敌 / 无限货币道具），不改任何官方 dex。
#
# 产物：08_output/我的修仙世界/我的修仙世界_4.5_surgical-unsigned.apk（未签名）
#       签名由 04_scripts/我的修仙世界/wdxsj_sign.sh（pipeline sign 阶段）负责
#
# 用法: bash scripts/surgical_wdxsj_4.5.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
GAME="我的修仙世界"
VERSION="4.5"
PKG="com.lingsi.xiuxiandanji"
ORIG_APK="$NIXIANG/02_apks/$GAME/${GAME}_4.5_taptap_official.apk"
ORIG_MD5="e94036940980417bed321949e793cfe2"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${WDXSJ_SO:-$STARRY_SRC/libs_wdxsj/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 wdxsj_build_so.sh）" >&2; exit 1; }
echo "[check] modern so = $SO_PREBUILT"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 $ACTUAL_MD5" >&2; exit 1; }

LOCK_DIR="/tmp/wdxsj_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wdxsj_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease (starry → smali + certs)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → classes3.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] \
  || { echo "ERROR: StarryInitProvider.smali missing（Java 桥没编进 starry app）" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" ]] \
  || { echo "ERROR: SignSpoof.smali missing" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$SMALI_ROOT"
ls -la "$WORK/classes3.dex"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/6] binary AndroidManifest（保留原 application / appComponentFactory）"
if [[ -n "${WDXSJ_BINARY_MANIFEST:-}" && -f "$WDXSJ_BINARY_MANIFEST" ]]; then
  cp "$WDXSJ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $WDXSJ_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s --no-assets + b extract)"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 本游戏 <application> 无 android:name（默认 Application）+ appComponentFactory=CoreComponentFactory：
# 不替换 Application，走 StarryInitProvider（ContentProvider.onCreate 早于 Activity.onCreate）。
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
print("  text Manifest patched (provider 注入, 保留原 Application)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验（provider/service/权限 + application 属性不变）"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  # aapt2 dump xmltree 只吃 zip/apk：把新 manifest 装进最小 zip 再回读
  python3.11 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 StarryInitProvider" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 MenuService" >&2; exit 1; }
  grep -q "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 FOREGROUND_SERVICE_SPECIAL_USE" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  # 新增行数应恰为 19（2 权限 + provider 4 行 + service 5 行 + property 2 行 + 空节点行）；
  # 出现其它差异 = manifest 重建把官包属性改坏，禁止出包
  DIFF_N="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^>' || true)"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 重建丢失了官方节点（$DIFF_D 行）" >&2; exit 1; }
  [[ "$DIFF_N" -ge 17 && "$DIFF_N" -le 22 ]] \
    || { echo "ERROR: manifest 新增节点数异常（$DIFF_N 行，期望 17~22）" >&2; exit 1; }
  echo "  manifest ok (新增 $DIFF_N 行，无删除)"
else
  echo "  WARN: aapt2 不可用，跳过 manifest 回读校验"
fi

echo "[5/6] zip surgical merge → $UNSIGNED"
python3.11 - "$ORIG_APK" "$WORK" "$UNSIGNED" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
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
             "lib/arm64-v8a/libthemis.so",
             "assets/bin/Data/Managed/Metadata/global-metadata.dat"]

zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names, "orig apk missing AndroidManifest.xml"
assert "classes3.dex" not in names, "orig apk already has classes3.dex — 需改用 classes4.dex"
for n in untouched:
    assert n in names, f"orig apk missing {n}"

def is_sig(name: str) -> bool:
    if name in SIG_NAMES:
        return True
    if not name.startswith("META-INF/"):
        return False
    return name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))

zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name):
        continue
    if name in extras:
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

info = zipfile.ZipInfo("classes3.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c3)
n += 1

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    a = hashlib.sha256(zin2.read(name)).hexdigest()
    b = hashlib.sha256(zout2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
for name in ("META-INF/services/kotlinx.coroutines.CoroutineExceptionHandler",
             "META-INF/services/kotlinx.coroutines.internal.MainDispatcherFactory"):
    if name in zin2.namelist():
        assert name in zout2.namelist(), f"lost {name}"
assert "classes3.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
assert not any(n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for n in zout2.namelist()), "signature leftovers"
assert "META-INF/MANIFEST.MF" not in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes3={len(c3)}; untouched {len(untouched)}/{len(untouched)} verified")
zin2.close(); zout2.close()
PY

echo "[6/6] 闭环校验（包内 so == 构建产物；classes3 含 StarryInitProvider）"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
if [[ "$IN_SO_MD5" != "$SRC_SO_MD5" ]]; then
  echo "ERROR: 包内 so ($IN_SO_MD5) != 构建产物 ($SRC_SO_MD5)——旧 so 混包，禁止交付" >&2
  exit 1
fi
echo "[so-verify] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"

if [[ -x "$BAKSMALI_BIN" ]]; then
  rm -rf "$WORK/c3_check"
  unzip -p "$UNSIGNED" classes3.dex > "$WORK/classes3_check.dex"
  "$BAKSMALI_BIN" d -o "$WORK/c3_check" "$WORK/classes3_check.dex" >/dev/null
  for f in com/starry/magic/StarryInitProvider.smali com/starry/magic/menu/MenuService.smali; do
    [[ -f "$WORK/c3_check/$f" ]] || { echo "ERROR: classes3.dex 缺 ${f}（smali 未打进包）" >&2; exit 1; }
  done
  echo "[dex-verify] classes3.dex 含 StarryInitProvider / MenuService ✓"
fi

ls -lh "$UNSIGNED"
echo "DONE(unsigned): $UNSIGNED"
echo "NOTE: 未签名；由 04_scripts/我的修仙世界/wdxsj_sign.sh（pipeline sign 阶段）签名。"
