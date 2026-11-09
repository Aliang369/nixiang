#!/usr/bin/env bash
# ================================================================
#  旷野征途 (com.xf.kyzt) 1.8 / vc18 — 星光外科组装
#
#  底包：TapTap 官方 1.8（真机 /data/app 只读拉取，无壳：无 libwbmod/libtoolChecker）
#        sha256 5f2ad3fd…b89fe / md5 aef4b495936ccf5154b03d4104f6a1ec
#  渠道：TapTap（classes.dex 含 com/taptap/sdk/initializer/repository/GatekeeperRepository ×9）
#        ⇒ 按 2026-10-29 双轨制走【改包法】：门闩恒真 + 错误弹窗空实现（不动签名运行时）
#
#  手术（其余条目字节保真）：
#   1) classes.dex 原地重汇编：TapTap 两处 patch（见 [2/7]）
#   2) 新增 classes2.dex（星光菜单 Java 桥，kyzt_build_dex.sh 产；官方只有 classes.dex）
#   3) AndroidManifest.xml：+appComponentFactory=com.starry.magic.sign.StarryAppComponentFactory
#        + StarryInitProvider + MenuService + SYSTEM_ALERT_WINDOW/FOREGROUND_SERVICE(_SPECIAL_USE)
#   4) lib/arm64-v8a/libstarry_magic.so（新增）+ libc++_shared.so（新增，DT_NEEDED）
#
#  用法:
#    bash 04_scripts/旷野征途/surgical_kyzt_1.8.sh
#    STARRY_INSTALL=1 SERIAL=f9a116a2 bash ...   # 出包+装机
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "${HERE}/../.." && pwd)"
# shellcheck source=/dev/null
. "${NIXIANG}/04_scripts/_common/env.sh"
STAR="${NIXIANG}/mod菜单/starry-mod-menu"

GAME="旷野征途"
VERSION="1.8"
PKG="com.xf.kyzt"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-f9a116a2}"

BASE_APK="${KYZT_BASE_APK:-${NIXIANG}/02_apks/旷野征途/旷野征途_1.8_taptap_official.apk}"
BASE_MD5_EXPECT="${KYZT_BASE_MD5:-aef4b495936ccf5154b03d4104f6a1ec}"
DEX_ADD_NAME="classes2.dex"
DEX2_SRC="${STAR}/templates/kyzt_menu_current/${DEX_ADD_NAME}"
SO_SRC="${STAR}/libs_kyzt/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STAR}/libs_kyzt/arm64-v8a/libc++_shared.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"
CACHE_DIR="${NIXIANG}/07_patched/${GAME}/surgical"
CACHE_DEX="${CACHE_DIR}/classes.patched.dex"
CACHE_MF="${CACHE_DIR}/AndroidManifest.xml"
KILLER_SRC="${CACHE_DIR}/killer.dex"   # MT 签名杀手（官方证书回灌，绕 TapTap 服务端验签）

JAVA="${NIXIANG_JAVA17}/bin/java"
PY="${NIXIANG_PYTHON:-python3}"
AAPT2="${NIXIANG_BUILD_TOOLS}/aapt2"
ZIPALIGN="${NIXIANG_ZIPALIGN}"
APKSIGNER="${NIXIANG_APKSIGNER}"
APKTOOL_JAR="${NIXIANG}/01_tools/apktool/apktool.jar"
SMALI_DIR="${NIXIANG}/01_tools/smali"
KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"

die() { echo "ERROR: $*" >&2; exit 1; }
md5f() { "${PY}" -c 'import hashlib,sys;print(hashlib.md5(open(sys.argv[1],"rb").read()).hexdigest())' "$1"; }

mkdir -p "${OUT_DIR}" "${CACHE_DIR}"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/kyzt_surgical.XXXXXX")"
cleanup() { rm -rf "${WORK}"; }
trap cleanup EXIT

echo "[1/7] 输入检查"
[ -f "${BASE_APK}" ] || die "缺底包 ${BASE_APK}（KYZT_BASE_APK=... 覆盖）"
[ -f "${DEX2_SRC}" ] || die "缺 ${DEX2_SRC}（先跑 04_scripts/旷野征途/kyzt_build_dex.sh）"
[ -f "${SO_SRC}" ] || die "缺 ${SO_SRC}（先跑构建）"
[ -f "${LIBCXX_SRC}" ] || die "缺 ${LIBCXX_SRC}"
[ -x "${ZIPALIGN}" ] || die "缺 zipalign"
[ -f "${APKSIGNER}" ] || die "缺 apksigner"
[ -f "${APKTOOL_JAR}" ] || die "缺 apktool.jar"
[ -f "${KS}" ] || die "缺签名库 ${KS}"
[ -f "${KILLER_SRC}" ] || die "缺签名杀手 dex ${KILLER_SRC}"

bmd5="$(md5f "${BASE_APK}")"
[ "${bmd5}" = "${BASE_MD5_EXPECT}" ] || die "底包指纹不符 ${bmd5}（期望 ${BASE_MD5_EXPECT}）"
for s in 'com/starry/magic/StarryInitProvider' 'com/starry/magic/menu/MenuService'; do
  c="$(LC_ALL=C grep -ac "$s" "${DEX2_SRC}" || true)"
  [ "${c}" -ge 1 ] || die "${DEX_ADD_NAME} 缺 $s"
done
for s in 'KYZT' '游戏加速'; do
  LC_ALL=C grep -aq "$s" "${SO_SRC}" || die "so 缺 $s"
done
echo "      底包 $(wc -c <"${BASE_APK}" | tr -d ' ') B md5 ${bmd5} / dex2 $(wc -c <"${DEX2_SRC}" | tr -d ' ') B / so $(wc -c <"${SO_SRC}" | tr -d ' ') B"

if [ ! -f "${CACHE_DEX}" ]; then
  echo "[2/7] 主 dex TapTap 改包（门闩恒真 + 弹窗空实现）"
  "${PY}" - "${BASE_APK}" "${WORK}/classes.dex" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
if 'classes.dex' not in names:
    sys.exit('底包无 classes.dex')
for k in ('classes2.dex', 'classes3.dex'):
    if k in names:
        sys.exit('底包已含 %s（dex 序号冲突）' % k)
open(sys.argv[2], 'wb').write(z.read('classes.dex'))
print('      classes.dex extracted')
PY
  "${JAVA}" -cp "${SMALI_DIR}/baksmali-2.5.2.jar:${SMALI_DIR}/dexlib2-2.5.2.jar:${SMALI_DIR}/util-2.5.2.jar:${SMALI_DIR}/guava-30.1.1-jre.jar:${SMALI_DIR}/jcommander-1.64.jar:${SMALI_DIR}/antlr-runtime-3.5.2.jar:${SMALI_DIR}/jsr305-3.0.2.jar:${SMALI_DIR}/error_prone_annotations-2.5.1.jar:${SMALI_DIR}/checker-qual-3.8.0.jar:${SMALI_DIR}/failureaccess-1.0.1.jar:${SMALI_DIR}/listenablefuture-9999.0-empty-to-avoid-conflict-with-guava.jar:${SMALI_DIR}/j2objc-annotations-1.3.jar:${SMALI_DIR}/animal-sniffer-annotations-1.20.jar" \
    org.jf.baksmali.Main d -o "${WORK}/dis" "${WORK}/classes.dex" >"${WORK}/baksmali.log" 2>&1 \
    || { tail -20 "${WORK}/baksmali.log" >&2; die "baksmali 失败"; }

  "${PY}" - "${WORK}/dis" <<'PY'
import sys, re, pathlib
root = pathlib.Path(sys.argv[1])
ini = root / 'com/taptap/sdk/initializer/TapTapSdkInitializer.smali'
gk  = root / 'com/taptap/sdk/initializer/repository/GatekeeperRepository.smali'
if not ini.exists() or not gk.exists():
    sys.exit('锚点类缺失（SDK 版本变了？）')

t = ini.read_text(encoding='utf-8')
pat = re.compile(r'(\.method public final isGatekeeperLoadedSuccessfully\(\)Z\n)(.*?)(\.end method)', re.S)
m = pat.search(t)
if not m:
    sys.exit('TapTapSdkInitializer.isGatekeeperLoadedSuccessfully 未找到')
body = '    .registers 2\n\n    const/4 v0, 0x1\n\n    return v0\n\n'
t = t[:m.start(2)] + body + t[m.end(2):]
ini.write_text(t, encoding='utf-8')

t = gk.read_text(encoding='utf-8')
pat = re.compile(r'(\.method private final showErrorToast\(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;\)V\n)(.*?)(\.end method)', re.S)
m = pat.search(t)
if not m:
    sys.exit('GatekeeperRepository.showErrorToast 未找到')
body = '    .registers 5\n\n    return-void\n\n'
t = t[:m.start(2)] + body + t[m.end(2):]
gk.write_text(t, encoding='utf-8')

# 3) 登录入口恒 Web：DefaultTapLoginFragmentProvider.provide 分支表强制 v2=3（WebLoginFragment）
prov = root / 'com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali'
if not prov.exists():
    sys.exit('DefaultTapLoginFragmentProvider 缺失（登录路由变了？）')
t = prov.read_text(encoding='utf-8')
if 'const/4 v2, 0x3' in t:
    print('      login route already forced to Web')
else:
    needle = 'aget v2, v3, v2'
    if t.count(needle) != 1:
        sys.exit('aget 锚点不唯一（%d 处）' % t.count(needle))
    t = t.replace(needle, needle + '\n\n    const/4 v2, 0x3', 1)
    prov.write_text(t, encoding='utf-8')
    print('      patched: login route forced -> WebLoginFragment')
print('      patched: isGatekeeperLoadedSuccessfully=1 / showErrorToast=void')
PY

  "${JAVA}" -cp "${SMALI_DIR}/smali-2.5.2.jar:${SMALI_DIR}/dexlib2-2.5.2.jar:${SMALI_DIR}/util-2.5.2.jar:${SMALI_DIR}/guava-30.1.1-jre.jar:${SMALI_DIR}/jcommander-1.64.jar:${SMALI_DIR}/antlr-runtime-3.5.2.jar:${SMALI_DIR}/jsr305-3.0.2.jar:${SMALI_DIR}/error_prone_annotations-2.5.1.jar:${SMALI_DIR}/checker-qual-3.8.0.jar:${SMALI_DIR}/failureaccess-1.0.1.jar:${SMALI_DIR}/listenablefuture-9999.0-empty-to-avoid-conflict-with-guava.jar:${SMALI_DIR}/j2objc-annotations-1.3.jar:${SMALI_DIR}/animal-sniffer-annotations-1.20.jar" \
    org.jf.smali.Main a -o "${WORK}/classes.patched.dex" "${WORK}/dis" >"${WORK}/smali.log" 2>&1 \
    || { tail -20 "${WORK}/smali.log" >&2; die "smali 重汇编失败"; }
  [ -f "${WORK}/classes.patched.dex" ] || die "smali 未产出 dex"
  cp -f "${WORK}/classes.patched.dex" "${CACHE_DEX}"
else
  echo "[2/7] 主 dex patch 缓存命中 ${CACHE_DEX}"
fi

if [ ! -f "${CACHE_MF}" ]; then
  echo "[3/7] 二进制 Manifest 注入"
  "${JAVA}" -jar "${APKTOOL_JAR}" d -f -s --no-assets -o "${WORK}/mf_dec" "${BASE_APK}" >"${WORK}/apktool_d.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_d.log" >&2; die "apktool d 失败"; }
  "${PY}" - "${WORK}/mf_dec/AndroidManifest.xml" "${AUTHORITY}" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
t = p.read_text(encoding='utf-8')
FACTORY = 'com.starry.magic.sign.StarryAppComponentFactory'
if 'android:appComponentFactory=' in t:
    raise SystemExit('manifest 已有 appComponentFactory，需人工核对')
i = t.find('<application'); j = t.find('>', i)
if i < 0 or j < 0:
    raise SystemExit('no <application>')
t = t[:j] + '\n        android:appComponentFactory="%s"' % FACTORY + t[j:]
perms = []
if 'SYSTEM_ALERT_WINDOW' not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if 'FOREGROUND_SERVICE_SPECIAL_USE' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find('<uses-permission')
    if i < 0:
        raise SystemExit('no <uses-permission>')
    t = t[:i] + '\n'.join(perms) + '\n' + t[i:]
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = ('\n        <provider\n            android:name="com.starry.magic.StarryInitProvider"\n'
            '            android:authorities="%s"\n            android:enabled="true"\n'
            '            android:exported="false"\n            android:initOrder="100" />\n') % authority
    t = t.replace('</application>', prov + '\n    </application>', 1)
if 'com.starry.magic.menu.MenuService' not in t:
    svc = ('\n        <service\n            android:name="com.starry.magic.menu.MenuService"\n'
           '            android:enabled="true"\n            android:exported="false"\n'
           '            android:foregroundServiceType="specialUse"\n            android:stopWithTask="false">\n'
           '            <property\n                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"\n'
           '                android:value="MOD menu overlay" />\n        </service>\n')
    t = t.replace('</application>', svc + '\n    </application>', 1)
p.write_text(t, encoding='utf-8')
print('      manifest patched')
PY
  "${JAVA}" -jar "${APKTOOL_JAR}" b -f "${WORK}/mf_dec" -o "${WORK}/mf_only.apk" >"${WORK}/apktool_b.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_b.log" >&2; die "apktool b 失败"; }
  "${PY}" - "${WORK}/mf_only.apk" "${CACHE_MF}" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
open(sys.argv[2], 'wb').write(z.read('AndroidManifest.xml'))
print('      manifest %d B' % len(open(sys.argv[2],'rb').read()))
PY
else
  echo "[3/7] Manifest 缓存命中 ${CACHE_MF}"
fi

echo "[4/7] 组装 APK（其余条目字节保真）"
"${PY}" - "${BASE_APK}" "${WORK}/merged.apk" "${CACHE_DEX}" "${CACHE_MF}" "${DEX2_SRC}" "${SO_SRC}" "${LIBCXX_SRC}" "${KILLER_SRC}" <<'PY'
import sys, zipfile, shutil
base, out, dex1, mf, dex2, so, cxx, killer = sys.argv[1:9]
repl = {
    'classes.dex': dex1,
    'AndroidManifest.xml': mf,
    'classes2.dex': dex2,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
    'classes3.dex': killer,
}
zin = zipfile.ZipFile(base)
with zipfile.ZipFile(out, 'w') as zo:
    for it in zin.infolist():
        data = open(repl[it.filename], 'rb').read() if it.filename in repl else zin.read(it.filename)
        zi = zipfile.ZipInfo(it.filename, date_time=it.date_time)
        zi.compress_type = it.compress_type
        zi.external_attr = it.external_attr
        zo.writestr(zi, data)
    have = set(zin.namelist())
    for name, path in repl.items():
        if name in have:
            continue
        zi = zipfile.ZipInfo(name, date_time=(1981,1,1,1,1,0))
        zi.compress_type = zipfile.ZIP_STORED if name.endswith('.so') else zipfile.ZIP_DEFLATED
        zi.external_attr = 0o644 << 16
        zo.writestr(zi, open(path, 'rb').read())
print('      merged ->', out)
PY

echo "[5/7] zipalign"
"${ZIPALIGN}" -f -p 4 "${WORK}/merged.apk" "${WORK}/aligned.apk" || die "zipalign 失败"

echo "[6/7] 签名（${KS_ALIAS} / CN=StarryMagic）"
"${APKSIGNER}" sign --ks "${KS}" --ks-key-alias "${KS_ALIAS}" --ks-pass "pass:${KS_PASS}" \
  --key-pass "pass:${KS_PASS}" --v4-signing-enabled false \
  --out "${WORK}/signed.apk" "${WORK}/aligned.apk" || die "apksigner 失败"
cp -f "${WORK}/signed.apk" "${FINAL}"

echo "[7/7] 产物校验"
"${AAPT2}" dump badging "${FINAL}" 2>/dev/null | grep -E "^package|application-label:" | head -3
"${PY}" - "${FINAL}" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1]); n = set(z.namelist())
need = ['classes.dex','classes2.dex','classes3.dex','AndroidManifest.xml',
        'lib/arm64-v8a/libstarry_magic.so','lib/arm64-v8a/libc++_shared.so']
miss = [k for k in need if k not in n]
print('      entries=%d  missing=%s' % (len(n), miss or 'none'))
d = z.read('classes.dex')
print('      GatekeeperRepository 仍在:', b'GatekeeperRepository' in d)
print('      starry 菜单类:', b'com/starry/magic/menu/MenuService' in z.read('classes2.dex'))
sys.exit(1 if miss else 0)
PY
echo "OK -> ${FINAL}"
echo "     md5 $(md5f "${FINAL}") / $(wc -c <"${FINAL}" | tr -d ' ') B"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  echo "[install] adb -s ${SERIAL} install -r"
  adb -s "${SERIAL}" install -r "${FINAL}"
fi
