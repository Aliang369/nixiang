#!/usr/bin/env bash
# ================================================================
#  农场守护者 (com.qld.ncsh.xiyou) 2.1.21 / vc21 — 星光外科组装
#
#  底包：TapTap 官方 2.1.21（干净：1 个 dex / 6 个 so，无 libSDKloading 壳）
#  手术（其余条目字节保真）：
#   1) 新增 classes2.dex（星光菜单 Java 桥，ncsh_build_dex.sh 产；官方只有 classes.dex）
#   2) AndroidManifest.xml：+appComponentFactory + StarryInitProvider + MenuService + 三权限
#   3) lib/arm64-v8a/libstarry_magic.so（新增）+ libc++_shared.so（新增，DT_NEEDED）
#  注意：官方 classes.dex 原样保留（本作无 TapTap 门闩补丁需求，先按原样试）
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "${HERE}/../.." && pwd)"
# shellcheck source=/dev/null
. "${NIXIANG}/04_scripts/_common/env.sh"
STAR="${NIXIANG}/mod菜单/starry-mod-menu"

GAME="农场守护者"
VERSION="2.1.21"
PKG="com.qld.ncsh.xiyou"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-f9a116a2}"

BASE_APK="${NCSH_BASE_APK:-${NIXIANG}/02_apks/农场守护者/农场守护者_2.1.21_taptap_official.apk}"
DEX_ADD_NAME="classes2.dex"
DEX2_SRC="${STAR}/templates/ncsh_menu_current/${DEX_ADD_NAME}"
SO_SRC="${STAR}/libs_ncsh/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STAR}/libs_ncsh/arm64-v8a/libc++_shared.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"
CACHE_DIR="${NIXIANG}/07_patched/${GAME}/surgical"
CACHE_MF="${CACHE_DIR}/AndroidManifest.xml"

JAVA="${NIXIANG_JAVA17}/bin/java"
PY="${NIXIANG_PYTHON:-python3}"
AAPT2="${NIXIANG_BUILD_TOOLS}/aapt2"
ZIPALIGN="${NIXIANG_ZIPALIGN}"
APKSIGNER="${NIXIANG_APKSIGNER}"
APKTOOL_JAR="${NIXIANG}/01_tools/apktool/apktool.jar"
KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

die() { echo "ERROR: $*" >&2; exit 1; }
md5f() { "${PY}" -c 'import hashlib,sys;print(hashlib.md5(open(sys.argv[1],"rb").read()).hexdigest())' "$1"; }

mkdir -p "${OUT_DIR}" "${CACHE_DIR}"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/ncsh_surgical.XXXXXX")"
cleanup() { rm -rf "${WORK}"; }
trap cleanup EXIT

echo "[1/6] 输入检查"
[ -f "${BASE_APK}" ] || die "缺底包 ${BASE_APK}（NCSH_BASE_APK=... 覆盖）"
[ -f "${DEX2_SRC}" ] || die "缺 ${DEX2_SRC}（先跑 04_scripts/农场守护者/ncsh_build_dex.sh）"
[ -f "${SO_SRC}" ] || die "缺 ${SO_SRC}（先跑 ndk-build -f Android_ncsh.mk）"
[ -f "${LIBCXX_SRC}" ] || die "缺 ${LIBCXX_SRC}"
[ -x "${ZIPALIGN}" ] || die "缺 zipalign"
[ -f "${APKSIGNER}" ] || die "缺 apksigner"
[ -f "${APKTOOL_JAR}" ] || die "缺 apktool.jar"
[ -f "${KS}" ] || die "缺签名库 ${KS}"

for s in 'com/starry/magic/StarryInitProvider' 'com/starry/magic/menu/MenuService'; do
  c="$(LC_ALL=C grep -ac "$s" "${DEX2_SRC}" || true)"
  [ "${c}" -ge 1 ] || die "${DEX_ADD_NAME} 缺 $s"
done
for s in 'NCSH' '百倍伤害'; do
  LC_ALL=C grep -aq "$s" "${SO_SRC}" || die "so 缺 $s"
done
echo "      底包 $(wc -c <"${BASE_APK}" | tr -d ' ') B / dex2 $(wc -c <"${DEX2_SRC}" | tr -d ' ') B / so $(wc -c <"${SO_SRC}" | tr -d ' ') B"

if [ ! -f "${CACHE_MF}" ]; then
  echo "[2/6] 生成二进制 Manifest（apktool 仅处理清单，不整包重建）"
  "${JAVA}" -jar "${APKTOOL_JAR}" d -f -s --no-assets -o "${WORK}/mf_dec" "${BASE_APK}" >"${WORK}/apktool_d.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_d.log" >&2; die "apktool d 失败"; }
  "${PY}" - "${WORK}/mf_dec/AndroidManifest.xml" "${AUTHORITY}" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
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
    if i < 0: sys.exit("no <uses-permission>")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:enabled="true"
            android:exported="false"
            android:initOrder="2000" />
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

factory = "com.starry.magic.sign.StarryAppComponentFactory"
if "appComponentFactory" not in t:
    i = t.find("<application")
    if i < 0: sys.exit("no <application>")
    j = t.find(">", i)
    t = t[:j] + ' android:appComponentFactory="%s"' % factory + t[j:]

p.write_text(t, encoding="utf-8")
print("      manifest patched")
PY
  "${JAVA}" -jar "${APKTOOL_JAR}" b -f "${WORK}/mf_dec" -o "${WORK}/mf_only.apk" >"${WORK}/apktool_b.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_b.log" >&2; die "apktool b 失败"; }
  "${PY}" - "${WORK}/mf_only.apk" "${CACHE_MF}" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
data = z.read('AndroidManifest.xml')
open(sys.argv[2], 'wb').write(data)
print('      manifest %d B' % len(data))
PY
else
  echo "[2/6] Manifest 缓存命中"
fi

echo "[3/6] 组装 APK（其余条目字节保真）"
"${PY}" - "${BASE_APK}" "${WORK}/merged.apk" "${CACHE_MF}" "${DEX2_SRC}" "${SO_SRC}" "${LIBCXX_SRC}" <<'PY'
import sys, zipfile
base, out, mf, dex2, so, cxx = sys.argv[1:7]
repl = {
    'AndroidManifest.xml': mf,
    'classes2.dex': dex2,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
zin = zipfile.ZipFile(base)
have = set(zin.namelist())
if 'classes2.dex' in have:
    sys.exit('底包已含 classes2.dex（dex 序号冲突）')
with zipfile.ZipFile(out, 'w') as zo:
    for it in zin.infolist():
        data = open(repl[it.filename], 'rb').read() if it.filename in repl else zin.read(it.filename)
        zi = zipfile.ZipInfo(it.filename, date_time=it.date_time)
        zi.compress_type = it.compress_type
        zi.external_attr = it.external_attr
        zo.writestr(zi, data)
    for name, path in repl.items():
        if name in have: continue
        zi = zipfile.ZipInfo(name, date_time=(1981,1,1,1,1,0))
        zi.compress_type = zipfile.ZIP_STORED if name.endswith('.so') else zipfile.ZIP_DEFLATED
        zi.external_attr = 0o644 << 16
        zo.writestr(zi, open(path, 'rb').read())
print('      merged ->', out)
PY

echo "[4/6] zipalign"
"${ZIPALIGN}" -f -p 4 "${WORK}/merged.apk" "${WORK}/aligned.apk" || die "zipalign 失败"

echo "[5/6] 签名"
"${APKSIGNER}" sign --ks "${KS}" --ks-key-alias "${KS_ALIAS}" --ks-pass "pass:${KS_PASS}" \
  --key-pass "pass:${KS_PASS}" --v4-signing-enabled false \
  --out "${WORK}/signed.apk" "${WORK}/aligned.apk" || die "apksigner 失败"
cp -f "${WORK}/signed.apk" "${FINAL}"

echo "[6/6] 产物校验"
"${AAPT2}" dump badging "${FINAL}" 2>/dev/null | grep -E "^package" | head -1
"${PY}" - "${FINAL}" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1]); n = set(z.namelist())
need = ['classes.dex','classes2.dex','AndroidManifest.xml',
        'lib/arm64-v8a/libstarry_magic.so','lib/arm64-v8a/libc++_shared.so']
miss = [k for k in need if k not in n]
print('      entries=%d missing=%s' % (len(n), miss or 'none'))
print('      manifest has provider:', b'StarryInitProvider' in z.read('AndroidManifest.xml'))
print('      menu class:', b'com/starry/magic/menu/MenuService' in z.read('classes2.dex'))
sys.exit(1 if miss else 0)
PY
echo "OK -> ${FINAL}"
echo "     md5 $(md5f "${FINAL}") / $(wc -c <"${FINAL}" | tr -d ' ') B"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  echo "[install] adb -s ${SERIAL} install -r"
  adb -s "${SERIAL}" install -r "${FINAL}"
fi
