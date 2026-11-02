#!/usr/bin/env bash
# ================================================================
#  爆裂防线 1.02.01（com.muyegame.blfx）— 星光外科组装（Windows/MSYS 后端）
#
#  与 mac 侧 surgical_blfx_1.02.01.sh **改动面完全一致**：
#    · AndroidManifest：appComponentFactory → StarryAppComponentFactory
#      + StarryInitProvider + MenuService + 三条权限（其余官方节点字节不变）
#    · 新增 classes3.dex（底包双 dex ⇒ 最小空闲下标）
#    · 新增 lib/arm64-v8a/{libstarry_magic.so, libc++_shared.so}
#    · classes.dex / classes2.dex / resources.arsc / assets/** / 官方 so 字节不变
#
#  平台差异（只用本机现成工具）：
#    A. 菜单 dex 不再走 gradle assembleRelease + apktool d starry
#       → 用 blfx_build_dex_win.sh 的 javac+d8 产物（同一份 starry Java 源）
#    B. apktool 用 java -jar 01_tools/apktool/apktool.jar
#    C. md5 -q / stat -f / darwin-x86_64 → md5f / filesize / windows-x86_64
#
#  产物：08_output/爆裂防线/爆裂防线_1.02.01_surgical-unsigned.apk
#       07_patched/爆裂防线/surgical/{AndroidManifest.xml,classes3.dex}
#  用法：bash 04_scripts/爆裂防线/blfx_surgical_win.sh
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=blfx_win_common.sh
. "$HERE/blfx_win_common.sh"

ORIG_APK="${BLFX_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_original_from_device.apk}"
ORIG_MD5="${BLFX_BASE_MD5:-c4f293d9009e90aa7098e29acea2d053}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$SURGICAL_DIR/AndroidManifest.xml"
NEW_DEX="$SURGICAL_DIR/classes3.dex"
# TapTap 改包法产物（blfx_patch_tapsdk_dex.py 输出）；存在则替换官方 classes.dex
PATCHED_CLASSES="${BLFX_PATCHED_CLASSES:-$SURGICAL_DIR/classes.dex}"
# 签名杀手 dex（blfx_build_killer_dex.py 输出）；存在则作为 classes4.dex 注入
KILLER_DEX="${BLFX_KILLER_DEX:-$SURGICAL_DIR/classes4.dex}"
LOGIN_CARRIER="$STARRY_SRC/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java"
AUTHORITY="$PKG.starryinit"

JAVA_BIN="$JAVA_HOME_J17/bin/java.exe"
[ -f "$JAVA_BIN" ] || JAVA_BIN="$JAVA_HOME_J17/bin/java"
PY="${NIXIANG_PYTHON:-python}"

[ -f "$ORIG_APK" ] || { echo "ERROR: missing ${ORIG_APK}（BLFX_BASE_APK=... 可覆盖）" >&2; exit 1; }
[ -f "$APKTOOL_JAR" ] || { echo "ERROR: missing ${APKTOOL_JAR}" >&2; exit 1; }
[ -f "$JAVA_BIN" ] || { echo "ERROR: java 不存在（\$NIXIANG_JAVA17=${JAVA_HOME_J17}）" >&2; exit 1; }

SO_PREBUILT="${BLFX_SO:-$OUT_LIBS/arm64-v8a/libstarry_magic.so}"
[ -f "$SO_PREBUILT" ] || { echo "ERROR: missing so ${SO_PREBUILT}（先跑 blfx_build_so_win.sh）" >&2; exit 1; }
[ -f "$NEW_DEX" ] || { echo "ERROR: missing ${NEW_DEX}（先跑 blfx_build_dex_win.sh）" >&2; exit 1; }

if [ "$HOST" = "win" ]; then
  NDK_CXX="$NDK_DIR/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
else
  NDK_CXX="$NDK_DIR/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
fi
[ -f "$NDK_CXX" ] || { echo "ERROR: missing libc++_shared.so at $NDK_CXX" >&2; exit 1; }

echo "[check] so        = $SO_PREBUILT ($(md5f "$SO_PREBUILT"))"
echo "[check] dex       = $NEW_DEX ($(md5f "$NEW_DEX"))"
ACTUAL_MD5="$(md5f "$ORIG_APK")"
[ "$ACTUAL_MD5" = "$ORIG_MD5" ] \
  || { echo "ERROR: 底包指纹不符 ${ACTUAL_MD5}（期望 ${ORIG_MD5}）" >&2; exit 1; }
echo "[check] 底包      = $ORIG_APK ($ACTUAL_MD5)"

# ---- TapSDK 路径探测（本作实测新版；旧版才需要 yntt_smali）------------------
TAP_PATH="$("$PY" - "$(winpath "$ORIG_APK")" <<'PY'
import sys, zipfile
OLD = b'com/taptap/sdk/ui/TapTapActivity'
NEW = b'com/taptap/sdk/login/impl/ui/activity/TapTapLoginActivity'
blob = b''
with zipfile.ZipFile(sys.argv[1]) as z:
    for n in z.namelist():
        if n.startswith('classes') and n.endswith('.dex'):
            blob += z.read(n)
if OLD in blob:
    print('old')
elif NEW in blob:
    print('new')
else:
    print('none')
PY
)"
echo "[check] TapSDK 路径 = $TAP_PATH"
case "$TAP_PATH" in
  old)
    echo "ERROR: 底包走旧版 TapTap 路径，需要 yntt_smali；本 Windows 链未接入该分支（mac 侧脚本可用）" >&2
    exit 1 ;;
  new)
    if [ "${BLFX_SKIP_LOGIN_WHITELIST_GATE:-0}" != "1" ]; then
      [ -f "$LOGIN_CARRIER" ] || { echo "ERROR: missing ${LOGIN_CARRIER}" >&2; exit 1; }
      grep -qF "\"$PKG\"" "$LOGIN_CARRIER" \
        || { echo "ERROR: TapTap 新路径登录白名单缺 ${PKG}（改签后 SSO 会 signature not match）" >&2; exit 1; }
      echo "[check] TapTap 登录白名单含 $PKG ✓"
    else
      echo "[check] WARN: 已跳过 TapTap 登录白名单门禁（登录可能失败，须在装机报告写明）"
    fi ;;
  none)
    echo "WARN: 底包未检出 TapTap 登录入口" >&2 ;;
esac

# ---- dex 内容校验（防旧 dex / 空 dex 混入）----------------------------------
for s in "com/starry/magic/StarryInitProvider" "com/starry/magic/menu/MenuService" \
         "com/starry/magic/sign/StarryAppComponentFactory" "com/starry/magic/sign/TapLoginForceWeb"; do
  LC_ALL=C grep -aqF "$s" "$NEW_DEX" || { echo "ERROR: $NEW_DEX 缺 $s" >&2; exit 1; }
done
echo "[check] dex 关键类齐备"

WORK="$(mktemp -d /tmp/blfx_surgical_win.XXXXXX)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$SURGICAL_DIR"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
cp "$NDK_CXX" "$WORK/lib/arm64-v8a/"
cp -f "$NEW_DEX" "$WORK/classes3.dex"

echo "[1/4] binary AndroidManifest（默认 Application；换 appComponentFactory + provider/service/权限）"
if [ -n "${BLFX_BINARY_MANIFEST:-}" ] && [ -f "${BLFX_BINARY_MANIFEST:-}" ]; then
  cp "$BLFX_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $BLFX_BINARY_MANIFEST"
elif [ -f "$MF_CACHE" ]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once（apktool d -s --no-assets → 文本改 → b）"
  APKTOOL=( "$JAVA_BIN" -jar "$(winpath "$APKTOOL_JAR")" )
  "${APKTOOL[@]}" d -f -s --no-assets -o "$(winpath "$WORK/orig_dec")" "$(winpath "$ORIG_APK")" >/dev/null
  "$PY" - "$(winpath "$WORK/orig_dec/AndroidManifest.xml")" "$PKG" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); pkg = sys.argv[2]; authority = sys.argv[3]
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

old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"'
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
print("  text Manifest patched（原 <application> 无 android:name，保持默认 Application）")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  "${APKTOOL[@]}" b -f "$(winpath "$WORK/orig_dec")" -o "$(winpath "$WORK/mf_only.apk")" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
echo "  manifest size = $(filesize "$WORK/AndroidManifest.xml") B"

echo "[2/4] binary Manifest 回读校验（provider/service/权限/factory + 官方节点不变）"
if [ -n "$AAPT2_BIN" ]; then
  "$AAPT2_BIN" dump xmltree --file AndroidManifest.xml "$(winpath "$ORIG_APK")" > "$WORK/tree_orig.txt"
  "$PY" - "$(winpath "$WORK/AndroidManifest.xml")" "$(winpath "$WORK/mf_check.apk")" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2_BIN" dump xmltree --file AndroidManifest.xml "$(winpath "$WORK/mf_check.apk")" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for t in "com.starry.magic.StarryInitProvider" "com.starry.magic.menu.MenuService" \
           "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" \
           "com.starry.magic.sign.StarryAppComponentFactory" \
           "com.unity3d.player.MyMainActivity"; do
    grep -q "$t" "$WORK/tree_new.txt" || { echo "ERROR: 新 manifest 缺 $t" >&2; exit 1; }
  done
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  "$PY" - "$(winpath "$WORK/o.norm")" "$(winpath "$WORK/n.norm")" <<'PY'
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
unexpected = {k: v for k, v in dropped.items()
              if not ('appComponentFactory' in k
                      and 'androidx.core.app.CoreComponentFactory' in k)}
if unexpected:
    for k, v in list(unexpected.items())[:10]:
        print(f'  LOST x{v}: {k}', file=sys.stderr)
    raise SystemExit(f'manifest 重建丢失官方节点（{sum(unexpected.values())} 项）')
if not (15 <= added <= 40):
    raise SystemExit(f'manifest 新增节点数异常（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 丢弃 {sum(dropped.values())} 项=旧 factory)')
PY
else
  echo "  WARN: aapt2 不在 ${AAPT2_BIN}，跳过 manifest 回读校验" >&2
fi

echo "[3/4] surgical zip merge（原 dex/arsc/assets/lib 字节不变）"
"$PY" - "$(winpath "$ORIG_APK")" "$(winpath "$WORK")" "$(winpath "$UNSIGNED")" \
  "$(winpath "$SO_PREBUILT")" "$(winpath "$NDK_CXX")" "classes3.dex" \
  "$(if [ -f "${PATCHED_CLASSES:-}" ]; then winpath "$PATCHED_CLASSES"; fi)" \
  "$(if [ -f "${KILLER_DEX:-}" ]; then winpath "$KILLER_DEX"; fi)" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1]); work = Path(sys.argv[2]); dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx = Path(sys.argv[5]).read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
dex_name = sys.argv[6]
dex_new = (work / dex_name).read_bytes()
patched = sys.argv[7] if len(sys.argv) > 7 else ''
killer = sys.argv[8] if len(sys.argv) > 8 else ''

replace = {'AndroidManifest.xml': manifest}
if patched:
    replace['classes.dex'] = Path(patched).read_bytes()
    print('  classes.dex 使用 TapTap 改包法补丁产物（不再与底包逐字节相同）')
extras = {
    dex_name: dex_new,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
if killer:
    extras['classes4.dex'] = Path(killer).read_bytes()
    print('  classes4.dex = 签名杀手（KillerApplication193，包名/官方证书已改写）')

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name == 'META-INF/MANIFEST.MF' or (name.startswith('META-INF/') and name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
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
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        changed += 1
    for n in ('classes.dex', 'classes2.dex', 'resources.arsc',
              'lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libunity.so',
              'lib/arm64-v8a/libtuanjie.so',
              'assets/bin/Data/Managed/Metadata/global-metadata.dat'):
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
    if dex_name not in out_names:
        raise SystemExit(f'{dex_name} missing')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'surgical zip verified: {changed} 个原始条目字节不变 + {dex_name} + starry so')
PY

echo "[4/4] unsigned output → $UNSIGNED"
echo "unsigned_md5=$(md5f "$UNSIGNED")"
echo "unsigned_size=$(filesize "$UNSIGNED")"
