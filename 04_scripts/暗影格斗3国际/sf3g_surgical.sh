#!/usr/bin/env bash
# 暗影格斗3 国际服 1.46.1 — 星光注入阶段（在 merged 单包上外科注入，不重建资源）
#   输入：08_output/暗影格斗3国际/暗影格斗3国际-1.46.1-merged-unsigned.apk
#   注入：AndroidManifest 组件/权限 + classes10.dex（菜单）+ lib/arm64-v8a/libstarry_magic.so
#   输出：...-starry-releaseSigned.apk（CN=StarryMagic）
set -euo pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"
# shellcheck source=../_common/env.sh
. "$NIXIANG/04_scripts/_common/env.sh" >/dev/null 2>&1 || true

GAME="暗影格斗3国际"
VERSION="1.46.1"
PKG="com.nekki.shadowfight3"
AUTHORITY="$PKG.starryinit"
OUT="$NIXIANG/08_output/$GAME"
BASE="${SF3G_BASE_UNSIGNED:-$OUT/$GAME-$VERSION-merged-unsigned.apk}"
MENU_DEX="${SF3G_MENU_DEX:-$NIXIANG/mod菜单/starry-mod-menu/templates/sf3g_menu_current/classes10.dex}"
SO="${SF3G_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_sf3g/arm64-v8a/libstarry_magic.so}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
CXX="${SF3G_CXX_SO:-$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so}"
UNSIGNED="$OUT/$GAME-$VERSION-starry-unsigned.apk"
SIGNED="$OUT/$GAME-$VERSION-starry-releaseSigned.apk"
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

PY="$(command -v python3)"
ZIPALIGN="${NIXIANG_ZIPALIGN:-}"
APKSIGNER="${NIXIANG_APKSIGNER:-}"
AAPT2="${NIXIANG_AAPT2:-}"
BT="${NIXIANG_BUILD_TOOLS:-}"
if [ -z "$AAPT2" ] && [ -n "$BT" ] && [ -f "$BT/aapt2" ]; then AAPT2="$BT/aapt2"; fi
if command -v /usr/libexec/java_home >/dev/null 2>&1; then
  export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
fi
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

for f in "$BASE" "$MENU_DEX" "$SO" "$CXX" "$KS"; do
  [ -f "$f" ] || { echo "ERROR: missing $f" >&2; exit 1; }
done
[ -n "$ZIPALIGN" ] && [ -n "$APKSIGNER" ] && [ -n "$AAPT2" ] || { echo "ERROR: build-tools 不全" >&2; exit 1; }

WORK="$(mktemp -d /tmp/sf3g_surg.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT

echo "[1/5] apktool d（只取 Manifest）"
apktool d -f -s --no-assets -o "$WORK/dec" "$BASE" >/dev/null

echo "[2/5] Manifest 注入星光组件与权限"
"$PY" - "$WORK/dec/AndroidManifest.xml" "$PKG" <<'PYEOF'
import re, sys
from pathlib import Path
p = Path(sys.argv[1]); pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android:name="android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    assert i >= 0
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# Application 替换（与成功成品同形态：StarryApp 承载 attachBaseContext 里加载 so）
# 不替换 Application（保留官方 MultiDexApplication）：改用完整 Provider 加载 so
if "com.starry.magic.StarryInitProvider" not in t:
    t = t.replace("</application>", '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % pkg + "\n    </application>", 1)
if "com.starry.magic.menu.MenuService" not in t:
    t = t.replace("</application>", '''
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
''' + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  perms added:", len(perms))
assert "StarryInitProvider" in t and "MenuService" in t
PYEOF

echo "[3/5] apktool b → binary Manifest"
rm -rf "$WORK/dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/dec" -o "$WORK/mf.apk" >/dev/null
"$PY" -c 'import sys,zipfile;open(sys.argv[2],"wb").write(zipfile.ZipFile(sys.argv[1]).read("AndroidManifest.xml"))' \
  "$WORK/mf.apk" "$WORK/AndroidManifest.xml"

echo "[4/6] d8 合并菜单 dex → classes9.dex（Android 只加载连续 classesN，classes10 实测不加载）"
unzip -p "$BASE" classes9.dex > "$WORK/classes9.dex"
mkdir -p "$WORK/d8"
"$BT/d8" --min-api 26 --no-desugaring --output "$WORK/d8" "$WORK/classes9.dex" "$MENU_DEX" >/dev/null 2>&1   || { echo "ERROR: d8 合并失败" >&2; exit 1; }
[ -f "$WORK/d8/classes.dex" ] || { echo "ERROR: d8 无输出" >&2; exit 1; }
echo "  merged classes9.dex: $(stat -f%z "$WORK/d8/classes.dex") bytes"

echo "[5/6] zip 注入 dex + so + manifest"
"$PY" - "$BASE" "$WORK/AndroidManifest.xml" "$WORK/d8/classes.dex" "$SO" "$UNSIGNED" "$CXX" <<'PYEOF'
import sys, zipfile
from pathlib import Path
base, mf, dex, so, dst, cxx = (Path(x) for x in sys.argv[1:7])  # dex = 已合并菜单的 classes9
new_mf = mf.read_bytes()
if dst.exists(): dst.unlink()
n = 0
with zipfile.ZipFile(dst, "w", zipfile.ZIP_DEFLATED) as out:
    with zipfile.ZipFile(base) as zb:
        for it in zb.infolist():
            name = it.filename
            if name == "AndroidManifest.xml":
                out.writestr(name, new_mf); continue
            if name.startswith("META-INF/") and name.split("/")[-1].split(".")[-1].upper() in ("RSA","SF","MF","DSA","EC"):
                continue
            if name in ("lib/arm64-v8a/libstarry_magic.so", "lib/arm64-v8a/libc++_shared.so"):
                continue
            if name == "classes9.dex":
                out.writestr(name, dex.read_bytes()); n += 1; continue
            zi = zipfile.ZipInfo(name, date_time=it.date_time)
            zi.compress_type = it.compress_type
            zi.external_attr = it.external_attr
            out.writestr(zi, zb.read(name)); n += 1
    for arc, src in (("lib/arm64-v8a/libstarry_magic.so", so),
                     ("lib/arm64-v8a/libc++_shared.so", cxx)):
        zi = zipfile.ZipInfo(arc, date_time=(1981,1,1,1,1,0))
        zi.compress_type = zipfile.ZIP_DEFLATED
        out.writestr(zi, src.read_bytes())
print("  injected: base entries=%d (classes9 含菜单) + libstarry_magic.so + libc++_shared.so" % n)
PYEOF

echo "[6/6] 校验 + 对齐 + 签名"
"$PY" - "$UNSIGNED" "$MENU_DEX" "$SO" <<'PYEOF'
import sys, zipfile, hashlib
apk, dex, so = sys.argv[1], sys.argv[2], sys.argv[3]
z = zipfile.ZipFile(apk)
names = z.namelist()
assert "classes9.dex" in names
assert "lib/arm64-v8a/libstarry_magic.so" in names, "starry so missing"
print("  dex=%d  so_md5=%s" % (sum(1 for n in names if n.startswith("classes") and n.endswith(".dex")),
                                hashlib.md5(z.read("lib/arm64-v8a/libstarry_magic.so")).hexdigest()))
PYEOF
"$ZIPALIGN" -f -p 4 "$UNSIGNED" "$WORK/aligned.apk"
rm -f "$SIGNED"
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$SIGNED" "$WORK/aligned.apk"
"$APKSIGNER" verify --print-certs "$SIGNED" 2>&1 | sed -n '1,3p'

BADGING="$("$AAPT2" dump badging "$SIGNED" 2>/dev/null | head -1 || true)"
case "$BADGING" in
  *"package: name='$PKG'"*) echo "  badging ok" ;;
  *) echo "ERROR: badging 异常: $BADGING" >&2; exit 1 ;;
esac
for s in "Category_功能" "Slider_伤害倍率:1:100:1" "Toggle_上帝模式"; do
  LC_ALL=C grep -aq "$s" "$SO" || { echo "ERROR: so 缺菜单串 $s" >&2; exit 1; }
done
echo "  => $SIGNED"
ls -l "$SIGNED"
