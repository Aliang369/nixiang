#!/usr/bin/env bash
# 我本布衣：世家录 1.1.46 TapTap 原版 → 星空魔法外科组装（Unity IL2CPP）
#
# 产出：08_output/我本布衣世家录/我本布衣世家录_1.1.46_surgical-unsigned.apk
#
# 注入内容
#   classes5.dex                       星空菜单（com.starry.magic.**，含 SignSpoof/TapLoginForceWeb）
#   lib/arm64-v8a/libstarry_magic.so   native：6 功能 hook
#   lib/arm64-v8a/libc++_shared.so     星空 so 依赖
#   assets/certs/starlight.crt         星空证书
#   AndroidManifest.xml                分支 B（StarryInitProvider + MenuService + FGS 权限）
#                                      ★不替换官方 Application
#
# ★本作官方 dex 有 4 个（classes-4），不 patch 任何官方 dex：
#   全部功能在 native 层 hook，官方 classes*.dex 必须逐字节不变。
#   新增 dex 落位 classes5.dex（最小空闲下标；本包已有 4 dex）。
#
# 字节不变（脚本内 sha256 断言）：classes*.dex / resources.arsc / lib/arm64-v8a/libil2cpp.so
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"

GAME="我本布衣世家录"
VERSION="1.1.46"
PKG="com.JAGames.TalesoftheClan"
NEW_DEX="classes5.dex"             # 官方已有 classes.dex/2/3/4（最小空闲下标）

ORIG_APK="${ORIG_APK:-$NIXIANG/02_apks/$GAME/${GAME}_1.1.46_vc55_base.apk}"
ORIG_MD5="${ORIG_MD5:-ad87df648e5c57b3bfb9a9c9e3889ae0}"
ORIG_CLASSES_MD5="${ORIG_CLASSES_MD5:-9ebaf83c3f29a5ef034f27e8fc85f923}"
ORIG_ARSC_MD5="${ORIG_ARSC_MD5:-83f670f03721779a8ab1de5aefc2dd89}"
ORIG_IL2CPP_SO_SHA256="${ORIG_IL2CPP_SO_SHA256:-1379f595b90c6ae820d51a88e644bad51ea7cc0a2852c4937eefc82b7eaa7adb}"

OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
SO_PREBUILT="${WBJY_SO:-$STARRY_SRC/libs_wbjy/arm64-v8a/libstarry_magic.so}"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]]     || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]]    || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]]  || { echo "ERROR: missing $SO_PREBUILT (先跑 build_so)" >&2; exit 1; }

echo "[check] 底包 = $ORIG_APK"
GOT_MD5="$(md5 -q "$ORIG_APK")"
[[ "$GOT_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 原包 md5=$GOT_MD5 != $ORIG_MD5" >&2; exit 1; }
echo "[check] 原包 md5 ok"

# so 归属校验（防串包：上一款游戏的 so 顶包）。★用 -F，方括号是 grep 正则元字符
if ! LC_ALL=C grep -aqF -- '[wbjy]' "$SO_PREBUILT" \
   || ! LC_ALL=C grep -aq -- 'Toggle_免广告' "$SO_PREBUILT"; then
  echo "ERROR: $SO_PREBUILT 不是 Wbjy 版（缺 [wbjy] / Toggle_免广告）" >&2
  strings -a "$SO_PREBUILT" | grep -aoE '^\[[A-Za-z]+\]' | sort -u | head -20 || true
  exit 1
fi
for s in '[Gyzw]' '[Ysxz]' '[Dwct]' '[Tlgd]' '[Blfx]'; do
  if LC_ALL=C grep -aqF -- "$s" "$SO_PREBUILT"; then
    echo "ERROR: $SO_PREBUILT 含别家标签 ${s}，libs_wbjy 被覆盖" >&2; exit 1
  fi
done
SO_MD5="$(md5 -q "$SO_PREBUILT")"
echo "[check] so = $SO_PREBUILT md5=$SO_MD5"

LOCK_DIR="/tmp/wbjy_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wbjy_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -f "$LOCK_DIR/pid"; rmdir "$LOCK_DIR" 2>/dev/null || true' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

# ---------------------------------------------------------------- 1/5
echo "[1/5] gradle assembleRelease（星空 APK → smali）"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }
# 星空 APK 内 so 必须是 Wbjy 版（gradle 打进 jniLibs 的那份）
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/starry_inner.so"
if ! LC_ALL=C grep -aqF -- '[wbjy]' "$WORK/starry_inner.so"; then
  echo "ERROR: 星空 APK 内 so 不是 Wbjy 版（jniLibs 未刷新）" >&2
  strings -a "$WORK/starry_inner.so" | grep -aoE '^\[[A-Za-z]+\]' | sort -u | head || true
  exit 1
fi
echo "  星空 APK ok（内嵌 so = Wbjy 版）"

# ---------------------------------------------------------------- 2/5
echo "[2/5] com/starry smali → $NEW_DEX"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for cls in StarryInitProvider MenuService ModBridge sign/SignSpoof sign/TapLoginForceWeb; do
  find "$SMALI_ROOT/com" -type f -name "$(basename "$cls").smali" | grep -q . || {
    echo "ERROR: starry smali 缺 $cls" >&2; exit 1; }
done
# 两张表必须同步（SignSpoof 证书表 + TapLoginForceWeb 白名单）
grep -q 'com[./]JAGames[./]TalesoftheClan' "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" \
  || { echo "ERROR: SignSpoof.smali 未含 com.JAGames.TalesoftheClan" >&2; exit 1; }
grep -q 'com[./]JAGames[./]TalesoftheClan' "$SMALI_ROOT/com/starry/magic/sign/TapLoginForceWeb.smali" \
  || { echo "ERROR: TapLoginForceWeb.smali 白名单未含 com.JAGames.TalesoftheClan" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT"
ls -la "$WORK/$NEW_DEX"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi
[[ -f "$WORK/assets/certs/starlight.crt" ]] || { echo "ERROR: starlight.crt missing" >&2; exit 1; }

# ---------------------------------------------------------------- 3/5
echo "[3/5] AndroidManifest：分支 B（StarryInitProvider）+ MenuService + FGS 权限"
if [[ -n "${WBJY_BINARY_MANIFEST:-}" && -f "${WBJY_BINARY_MANIFEST:-}" ]]; then
  cp "$WBJY_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  from WBJY_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  # 168MB 原包直接 apktool 太慢：用只含 Manifest+arsc+classes.dex 的迷你 zip 做一次
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src); zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name); data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
for perm in ("FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    full = "android.permission." + perm
    if ('android:name="%s"' % full) not in t:
        perms.append('    <uses-permission android:name="%s"/>' % full)
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if "com.starry.magic.StarryInitProvider" not in t:
    prov = ('''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg)
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
print("  text Manifest patched (provider + service + perms)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree "$ORIG_APK" --file AndroidManifest.xml > "$WORK/tree_orig.txt"
  python3.11 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml"); i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read()); z.close()
PY
  "$AAPT2" dump xmltree "$WORK/mf_check.apk" --file AndroidManifest.xml > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService"     "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
  grep -q "FOREGROUND_SERVICE_SPECIAL_USE"        "$WORK/tree_new.txt" || { echo "ERROR: FGS perm missing" >&2; exit 1; }
  grep -q "com.SecShell.SecShell.AW"              "$WORK/tree_new.txt" || true  # 本作无 SecShell
  grep -q "$PKG" "$WORK/tree_new.txt" || { echo "ERROR: 包名丢失" >&2; exit 1; }
  # platformBuildVersionName/Code 是 aapt2 编译期工具链元数据（非应用节点），
  # apktool 回编时必然重算/省略 ⇒ 比对前从两侧剔除，否则误报「丢失官方节点」。
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" | grep -vE 'platformBuildVersion(Name|Code)=' > "$WORK/o.norm" || true
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt"  | grep -vE 'platformBuildVersion(Name|Code)=' > "$WORK/n.norm" || true
  LOST="$(diff "$WORK/o.norm" "$WORK/n.norm" | grep -c '^<' || true)"
  [[ "$LOST" == "0" ]] || { echo "ERROR: manifest 丢失官方节点 $LOST 行" >&2; diff "$WORK/o.norm" "$WORK/n.norm" | grep '^<' | head -10; exit 1; }
  echo "  aapt2 校验通过（官方节点零丢失）"
fi

# ---------------------------------------------------------------- 4/5
echo "[4/5] zip surgical merge → $UNSIGNED"
python3.11 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
new_dex = sys.argv[4]

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

# 官方条目必须逐字节不变（本作无任何官方 dex 需要 patch）
UNTOUCHED = ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
             "resources.arsc", "lib/arm64-v8a/libil2cpp.so"]
SIG_EXT = (".SF", ".RSA", ".DSA", ".EC")
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
assert "AndroidManifest.xml" in names
assert new_dex not in names, f"{new_dex} already in orig"
for n in UNTOUCHED:
    assert n in names, f"orig apk missing {n}"

def is_sig(name):
    if name == "META-INF/MANIFEST.MF": return True
    if not name.startswith("META-INF/"): return False
    return name.upper().endswith(SIG_EXT)

kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
n_written = 0
for item in zin.infolist():
    name = item.filename
    if is_sig(name) or name in extras:
        continue
    if name == "AndroidManifest.xml":
        data = mf
        ct = zipfile.ZIP_DEFLATED
    else:
        data = zin.read(item)
        ct = zipfile.ZIP_STORED if name == "resources.arsc" else item.compress_type
    info = zipfile.ZipInfo(name, item.date_time)
    info.compress_type = ct
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
for name, data in [(new_dex, cnew)] + sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n_written += 1
zin.close(); zout.close()

zi2, zo2 = zipfile.ZipFile(src, "r"), zipfile.ZipFile(dst, "r")
for name in UNTOUCHED:
    a = hashlib.sha256(zi2.read(name)).hexdigest()
    b = hashlib.sha256(zo2.read(name)).hexdigest()
    assert a == b, f"changed: {name}"
out_names = set(zo2.namelist())
for name in kept_meta:
    assert name in out_names, f"lost {name}"
assert new_dex in out_names
assert "lib/arm64-v8a/libstarry_magic.so" in out_names
assert "lib/arm64-v8a/libc++_shared.so" in out_names
assert "assets/certs/starlight.crt" in out_names
assert not any(x.upper().endswith(SIG_EXT) for x in out_names)
assert "META-INF/MANIFEST.MF" not in out_names
print(f"  merged ok; entries={n_written}; {new_dex}={len(cnew)}; untouched {len(UNTOUCHED)} verified")
zi2.close(); zo2.close()
PY

PKG_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$SO_MD5" ]] || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != $SO_MD5" >&2; exit 1; }
unzip -Z1 "$UNSIGNED" > "$WORK/entries.txt"
for e in "$NEW_DEX" "classes.dex" "lib/arm64-v8a/libstarry_magic.so" \
         "lib/arm64-v8a/libc++_shared.so" "assets/certs/starlight.crt"; do
  grep -q "^$e\$" "$WORK/entries.txt" || { echo "ERROR: $e missing" >&2; exit 1; }
done
echo "  包内 so md5 ok；$NEW_DEX 到位"

# ---------------------------------------------------------------- 5/5
echo "[5/5] 完成（未签名）→ $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_sha256=$(shasum -a 256 "$UNSIGNED" | awk '{print $1}')"
ls -lh "$UNSIGNED"
echo "DONE: ${UNSIGNED}（下一步：sign）"
