#!/usr/bin/env bash
# 勇士之心 1.0.3 TapTap 原版外科组装。
#
# 保留官方 classes.dex/classes2.dex/classes3.dex、resources.arsc、
# lib/arm64-v8a/libcocos2djs.so 与游戏 assets 字节不变。
# 新增星空 classes4.dex + libstarry_magic.so + libc++_shared.so。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
GAME="勇士之心"
VERSION="1.0.3"
PKG="com.fightingstars.games.warrior"
ORIG_APK="${ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_1.0.3_taptap_official.apk}"
ORIG_MD5="${ORIG_MD5:-8d47c857b3c32e37b79d17775f30e2d4}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
NEW_DEX="classes4.dex"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${YSXZ_SO:-$STARRY_SRC/libs_ysxz/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing modern so $SO_PREBUILT" >&2; exit 1; }

# 菜单表断言：1.0.3 用的是 1.0.4 的新序号（1 号位伤害倍率滑块），预置 so 必须是新版。
# 旧菜单 so 一旦被打进 1.0.3，菜单序号会与 JS 补丁错位 ⇒ 无限技能/移速/自动战斗静默失效。
for _tok in "Slider_伤害倍率:1:50:1" "Slider_移动速度:1:10:1" "Toggle_自动战斗"; do
  LC_ALL=C grep -aq -- "$_tok" "$SO_PREBUILT" || {
    echo "ERROR: $SO_PREBUILT 不含新菜单串 $_tok —— 这是旧菜单 so" >&2
    echo "       先跑 bash 04_scripts/勇士之心/ysxz_build_so_103.sh 重建 1.0.3 菜单 so" >&2
    exit 1
  }
done
if LC_ALL=C grep -aq -- "Slider_移动速度:1:5:1" "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 仍是旧菜单（含 Slider_移动速度:1:5:1），拒绝出 1.0.3 包" >&2
  exit 1
fi
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 原包指纹不符" >&2; exit 1; }

LOCK_DIR="/tmp/ysxz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/ysxz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/5] gradle assembleRelease (starry -> dex)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/5] starry com/starry smali -> $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for cls in StarryInitProvider MenuService ModBridge; do
  find "$SMALI_ROOT/com" -type f -name "$cls.smali" -print -quit | grep -q . || {
    echo "ERROR: starry class missing: $cls" >&2; exit 1;
  }
done
# 1.0.4 的过检测类（自定义 appComponentFactory）不搬进 1.0.3：
#   1.0.3 底包 Manifest 用的是 androidx.core.app.CoreComponentFactory，没有任何地方引用它。
find "$SMALI_ROOT" -name 'YsxzAppComponentFactory*.smali' -delete 2>/dev/null || true
find "$SMALI_ROOT" -type d -empty -delete 2>/dev/null || true
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

# 菜单外观断言（外观在 dex，不在 so）：
#  - 必须含星空面板 MagicRes$PanelBgDrawable（2026-09-22 星光紫新外观）
#  - 必须不含 1.0.4 的过检测类 YsxzAppComponentFactory
if ! LC_ALL=C grep -aq -- "MagicRes\$PanelBgDrawable" "$WORK/$NEW_DEX"; then
  echo "ERROR: $NEW_DEX 里没有 MagicRes\$PanelBgDrawable —— 这是美化前的旧菜单外观" >&2
  echo "       检查 $STARRY_SRC/app/src/main/java/com/starry/magic/menu/MagicRes.java 是否为新版" >&2
  exit 1
fi
if LC_ALL=C grep -aq -- "YsxzAppComponentFactory" "$WORK/$NEW_DEX"; then
  echo "ERROR: $NEW_DEX 里混进了 1.0.4 的过检测类 YsxzAppComponentFactory" >&2
  exit 1
fi

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[3/5] AndroidManifest：保留官方 WarriorApplication，仅追加星光组件"
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
  grep -q "org.cocos2dx.javascript.WarriorApplication" "$WORK/tree_new.txt" || { echo "ERROR: official Application lost" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  DIFF_D="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$DIFF_D" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $DIFF_D 行" >&2; exit 1; }
fi

echo "[4/5] zip surgical merge -> $UNSIGNED"
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
untouched = [
    "classes.dex", "classes2.dex", "classes3.dex", "resources.arsc",
    "lib/arm64-v8a/libcocos2djs.so", "lib/arm64-v8a/libBugly_Native.so",
    "lib/arm64-v8a/libsecsdk.so", "assets/main.js",
    "assets/src/cocos2d-jsb.js", "assets/src/settings.js",
    "assets/assets/main/index.js",
]
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

echo "[5/5] 校验产物"
unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
grep -q "^$NEW_DEX$" "$WORK/entries.txt" || { echo "ERROR: $NEW_DEX missing" >&2; exit 1; }
grep -q '^lib/arm64-v8a/libstarry_magic.so$' "$WORK/entries.txt" || { echo "ERROR: starry so missing" >&2; exit 1; }
echo "unsigned=$UNSIGNED"
md5 -q "$UNSIGNED" | sed 's/^/md5=/'
shasum -a 256 "$UNSIGNED" | awk '{print "sha256="$1}'
