#!/usr/bin/env bash
# ================================================================
#  保卫向日葵 (com.bwxrk.yqmy.ty4) 1.46 / vc46 — 星光外科组装
#  （新菜单 provider-only 路线，照 surgical_hzsj_1.1.1.sh）
#
#  底包：用户下载的原版（真机 f9a116a2 只读拉取），无壳（无 libSDKloading/libtoolChecker）
#        Application = com.dodjoy.juhe.sdk.JuHeApplication；Manifest **原本没有**
#        android:appComponentFactory ⇒ 本脚本【新增】该属性（不是替换）。
#  登录：TapSDK（com.taptap.global.*）+ dodjoy juhe SDK（com.juhe.sdk.TapTapLoginActivity）
#
#  手术（其余 15072 条目字节保真）：
#   1) 新增 classes5.dex（星光菜单 Java 桥，bwxrk_build_dex.sh 产；官方包只到 classes4.dex）
#   2) AndroidManifest.xml：
#        + android:appComponentFactory=com.starry.magic.sign.SignSpoofAppComponentFactory
#          （原包无此属性 → 新增；签名回灌必须早于 JuHeApplication/TapSDK 冷启动读签名）
#        + <provider com.starry.magic.StarryInitProvider>（菜单 bootstrap）
#        + <service com.starry.magic.menu.MenuService>（前台悬浮菜单）
#        + SYSTEM_ALERT_WINDOW（已在）/ FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE
#   3) lib/arm64-v8a/libstarry_magic.so 新增（libs_bwxrk 产物）
#   4) lib/arm64-v8a/libc++_shared.so 新增（新 so 的 DT_NEEDED 硬依赖；官方包不带）
#   5) 签名回灌证书 = 官方 CN=QT, O=GD（SHA-256 24086df9…）
#
#  签名：01_tools/starry_release.jks（CN=StarryMagic，alias starry）——禁换（卡密指纹）
#
#  用法:
#    bash scripts/surgical_bwxrk_1.46.sh                    # 出包
#    STARRY_INSTALL=1 SERIAL=f9a116a2 bash ...              # 出包+装机（同签 -r）
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${STARRY_SRC}/../../04_scripts/_common/env.sh"

NIXIANG="${NIXIANG_ROOT}"
GAME="保卫向日葵"
VERSION="${BWXRK_VERSION:-1.46}"
PKG="com.bwxrk.yqmy.ty4"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-f9a116a2}"

# --- 底包：官方原版（真机只读拉取，无壳）------------------------------------
case "${VERSION}" in
  1.46)
    BASE_DEFAULT="${NIXIANG}/09_test/${GAME}/base_official.apk"
    BASE_MD5_EXPECT="${BWXRK_BASE_MD5:-6d88f65d570a173d82c6d2c90b96cff9}"
    DEX_ADD_NAME="${BWXRK_DEX_NAME:-classes5.dex}"    # 官方原包只到 classes4.dex
    LEGACY_MENU_DEX=""                                # 官方包无 com/starry，免清理
    ;;
  *) echo "ERROR: 未知 BWXRK_VERSION=${VERSION}（只支持 1.46）" >&2; exit 1 ;;
esac

BASE_APK="${BWXRK_BASE_APK:-${BASE_DEFAULT}}"
SO_SRC="${STARRY_SRC}/libs_bwxrk/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STARRY_SRC}/libs_bwxrk/arm64-v8a/libc++_shared.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"

# 一次性手术缓存（dex5 去菜单 / 二进制 Manifest）；缓存按"是否做 dex5 清理"分桶
CACHE_DIR="${NIXIANG}/07_patched/${GAME}/surgical"
CACHE_CLS5=""
CACHE_MF="${CACHE_DIR}/AndroidManifest.xml"

DEX6_SRC="${STARRY_SRC}/templates/bwxrk_menu_20261015/${DEX_ADD_NAME}"

KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

JAVA17="${NIXIANG_JAVA17}"
JAVA="$JAVA17/bin/java.exe"; [ -f "$JAVA" ] || JAVA="$JAVA17/bin/java"
# smali 工具：win 侧有 01_tools/smali/{baksmali,smali}-fat.jar；mac 侧只有 baksmali-2.5.2.jar
# （fat 版是 win 端现场打的），故按"fat 优先，缺则普通 jar + 依赖 classpath"双轨解析
SMALI_DIR="${NIXIANG}/01_tools/smali"
if [ -f "${SMALI_DIR}/baksmali-fat.jar" ]; then
  BAKSMALI_JAR="${SMALI_DIR}/baksmali-fat.jar"; BAKSMALI_CP=""
else
  BAKSMALI_JAR="${SMALI_DIR}/baksmali-2.5.2.jar"
  BAKSMALI_CP="${SMALI_DIR}/baksmali-2.5.2.jar:${SMALI_DIR}/dexlib2-2.5.2.jar:${SMALI_DIR}/util-2.5.2.jar:${SMALI_DIR}/guava-30.1.1-jre.jar:${SMALI_DIR}/jcommander-1.64.jar:${SMALI_DIR}/antlr-runtime-3.5.2.jar:${SMALI_DIR}/jsr305-3.0.2.jar:${SMALI_DIR}/error_prone_annotations-2.5.1.jar:${SMALI_DIR}/checker-qual-3.8.0.jar:${SMALI_DIR}/failureaccess-1.0.1.jar:${SMALI_DIR}/listenablefuture-9999.0-empty-to-avoid-conflict-with-guava.jar:${SMALI_DIR}/j2objc-annotations-1.3.jar:${SMALI_DIR}/animal-sniffer-annotations-1.20.jar"
fi
if [ -f "${SMALI_DIR}/smali-fat.jar" ]; then
  SMALI_JAR="${SMALI_DIR}/smali-fat.jar"; SMALI_CP=""
else
  SMALI_JAR="${SMALI_DIR}/smali-2.5.2.jar"; SMALI_CP="${BAKSMALI_CP}"
fi
baksmali() { if [ -n "${BAKSMALI_CP}" ]; then "$JAVA" -cp "${BAKSMALI_CP}" org.jf.baksmali.Main "$@"; else "$JAVA" -jar "${BAKSMALI_JAR}" "$@"; fi; }
smali()    { if [ -n "${SMALI_CP}" ];    then "$JAVA" -cp "${SMALI_CP}" org.jf.smali.Main "$@";    else "$JAVA" -jar "${SMALI_JAR}" "$@"; fi; }
# 本仓库 apktool 只有 jar（无 win 的 apktool/apktool 包装脚本）⇒ 两平台都走 java -jar
APKTOOL_JAR="${APKTOOL_JAR:-${NIXIANG}/01_tools/apktool/apktool.jar}"
AAPT2="${AAPT2:-${NIXIANG_BUILD_TOOLS}/aapt2.exe}"
[ -f "$AAPT2" ] || AAPT2="${NIXIANG_BUILD_TOOLS}/aapt2"
PY="${NIXIANG_PYTHON:-python}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-${NIXIANG_BUILD_TOOLS}/zipalign}"
APKSIGNER="${NIXIANG_APKSIGNER:-${NIXIANG_BUILD_TOOLS}/apksigner}"
if [[ -n "${NIXIANG_JAVA17:-}" ]]; then
  export JAVA_HOME="${NIXIANG_JAVA17}"
  export PATH="${NIXIANG_JAVA17}/bin:${PATH}"
fi
apktool() { "$JAVA" -jar "${APKTOOL_JAR}" "$@"; }

die() { echo "ERROR: $*" >&2; exit 1; }
md5f() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }

ASCII_ROOT="${TEMP:-${TMPDIR:-/tmp}}"
if command -v cygpath >/dev/null 2>&1; then
  ASCII_ROOT="$(cygpath -u "${ASCII_ROOT}" 2>/dev/null || printf '%s' "${ASCII_ROOT}")"
fi
mkdir -p "${ASCII_ROOT}" "${OUT_DIR}" "${CACHE_DIR}"

[[ -f "${BASE_APK}" ]] || die "缺底包 ${BASE_APK}（BWXRK_BASE_APK=... 覆盖）"
[[ -f "${DEX6_SRC}" ]] || die "缺 ${DEX6_SRC}（先跑 04_scripts/保卫向日葵/bwxrk_build_dex.sh）"
[[ -f "${SO_SRC}" ]]   || die "缺 ${SO_SRC}（先跑 04_scripts/保卫向日葵/bwxrk_build_so.sh）"
[[ -f "${LIBCXX_SRC}" ]] || die "缺 ${LIBCXX_SRC}（NDK 构建顺产出，被删就跑 bwxrk_build_so.sh）"
[[ -f "${JAVA}" ]]     || die "缺 java: ${JAVA}"
[[ -f "${APKTOOL_JAR}" ]] || die "缺 apktool.jar: ${APKTOOL_JAR}"
[[ -f "${BAKSMALI_JAR}" && -f "${SMALI_JAR}" ]] || die "缺 smali 工具（01_tools/smali：baksmali/smali jar）"
[[ -x "${ZIPALIGN}" ]] || die "缺 zipalign: ${ZIPALIGN}"
[[ -x "${APKSIGNER}" || -f "${APKSIGNER}" ]] || die "缺 apksigner: ${APKSIGNER}"
[[ -f "${AAPT2}" ]] || die "缺 aapt2: ${AAPT2}（build-tools）"

LOCK_DIR="/tmp/bwxrk_surgical.lockdir"
if ! mkdir "${LOCK_DIR}" 2>/dev/null; then
  die "lock ${LOCK_DIR} held（另一个外科进程在跑）"
fi
echo $$ >"${LOCK_DIR}/pid"
WORK=""
cleanup() { [[ -n "${WORK}" && -d "${WORK}" ]] && rm -rf "${WORK}"; rm -rf "${LOCK_DIR}"; }
trap cleanup EXIT
WORK="$(mktemp -d "${ASCII_ROOT}/bwxrk_surgical.XXXXXX")"
WORKW="$(cygpath -m "${WORK}" 2>/dev/null || printf '%s' "${WORK}")"
MERGED="${WORK}/merged.apk"

echo "[1/6] 输入检查"
bsize="$(wc -c <"${BASE_APK}" | tr -d ' ')"
bmd5="$(md5f "${BASE_APK}")"
[[ "${bmd5}" == "${BASE_MD5_EXPECT}" ]] || die "底包指纹不符 ${bmd5}（期望 ${BASE_MD5_EXPECT}）"
for s in 'com/starry/magic/StarryInitProvider' 'com/starry/magic/menu/MenuService'; do
  c="$(LC_ALL=C grep -ac "$s" "${DEX6_SRC}" || true)"
  [[ "${c}" -ge 1 ]] || die "${DEX_ADD_NAME} 缺 ${s}"
done
for s in 'Toggle_拿全部技能' '[Bwxrk]'; do
  LC_ALL=C grep -aq "$s" "${SO_SRC}" || die "so 缺 ${s}"
done

# 共享表登记断言（复盘 P1-1，2026-10-15）：本包必须已在 SignSpoof / TapLoginForceWeb 两张表里。
# 漏登记的症状是「装机成功但 TapTap 登录失败（CONFIG_ERROR / SDK not initialized）」，
# 纯静态可查却会白耗一轮装机+用户实测 ⇒ 前置成硬门禁。
bash "${STARRY_SRC}/scripts/_lib/check_shared_tables.sh" --pkg "${PKG}" --channel taptap --quiet \
  || die "共享表未登记（见上方补法）—— 首装 TapTap 登录必失败"
echo "[check] 共享表登记 ok（SignSpoof + TapLoginForceWeb 均含 ${PKG}）"
echo "[check] VERSION=${VERSION} 底包 ${bsize} B (md5 ${bmd5}) / ${DEX_ADD_NAME} $(wc -c <"${DEX6_SRC}" | tr -d ' ') B / so $(wc -c <"${SO_SRC}" | tr -d ' ') B ok"

# 底包结构闸门：既不能已含注入目标（重复注入），也不能缺替换目标
"${PY}" - "${BASE_APK}" "${DEX_ADD_NAME}" <<'PY' || die "底包结构闸门不过"
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1]); names = set(z.namelist()); add = sys.argv[2]
if add in names:
    sys.exit('底包已含 %s（重复注入嫌疑：底包选错或 dex 序号冲突）' % add)
for k in ('AndroidManifest.xml',):
    if k not in names:
        sys.exit('底包缺条目 %s' % k)
so = 'lib/arm64-v8a/libstarry_magic.so'
print('  底包结构 ok（无 %s；%s %s）' % (add, so, '可替换' if so in names else '待新增'))
PY

echo "[2/6] 旧菜单清理（本作官方原包无 com/starry，本步只做 dex 序号撞名检查）"
if [[ -z "${LEGACY_MENU_DEX}" ]]; then
  # 官方原包：4 个 dex 内 com/starry 命中 0（已核，见 10_docs/保卫向日葵/斌哥包抓点_20261015.md §2）
  # ⇒ 无旧菜单可清；同时确认底包确实没有 classes5.dex（否则 DEX_ADD_NAME 会撞名）
  "${PY}" - "${BASE_APK}" <<'PY' || die "官方底包含 classes5.dex（dex 序号冲突，改 BWXRK_DEX_NAME）"
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
if 'classes5.dex' in z.namelist():
    sys.exit('底包已含 classes5.dex')
print('  跳过：官方底包无旧菜单（com/starry 命中 0）且无 classes5.dex')
PY
elif [[ ! -f "${CACHE_CLS5}" ]]; then
  "${PY}" - "${BASE_APK}" "${WORKW}/classes5.dex" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
open(sys.argv[2], 'wb').write(z.read('classes5.dex'))
print('  classes5.dex extracted')
PY
  baksmali d -o "${WORKW}/dis5" "${WORKW}/classes5.dex" >"${WORK}/baksmali.log" 2>&1 \
    || { tail -20 "${WORK}/baksmali.log" >&2; die "baksmali 失败"; }
  [[ -d "${WORK}/dis5/com/starry" ]] || die "classes5 反编后未找到 com/starry（底包结构变化？）"
  n_starry="$(find "${WORK}/dis5/com/starry" -name '*.smali' | wc -l | tr -d ' ')"
  n_total="$(find "${WORK}/dis5" -name '*.smali' | wc -l | tr -d ' ')"
  rm -rf "${WORK}/dis5/com/starry"
  smali a -o "${WORKW}/classes5_nostarry.dex" "${WORKW}/dis5" >"${WORK}/smali.log" 2>&1 \
    || { tail -20 "${WORK}/smali.log" >&2; die "smali 重汇编失败"; }
  [[ -f "${WORK}/classes5_nostarry.dex" ]] || die "smali 未产出 dex"
  if LC_ALL=C grep -aq 'com/starry/magic' "${WORK}/classes5_nostarry.dex"; then
    die "重建的 classes5.dex 仍含 com/starry（清理不净）"
  fi
  LC_ALL=C grep -aq 'kotlin' "${WORK}/classes5_nostarry.dex" || die "重建的 classes5.dex 丢了 kotlin（误删）"
  cp -f "${WORK}/classes5_nostarry.dex" "${CACHE_CLS5}"
  echo "  starry=$n_starry / total=$n_total → 保留 $((n_total - n_starry)) 个类，缓存 ${CACHE_CLS5}"
else
  echo "  缓存命中 ${CACHE_CLS5}"
  if LC_ALL=C grep -aq 'com/starry/magic' "${CACHE_CLS5}"; then
    die "缓存 classes5.dex 含 com/starry（缓存污染，删除后重跑）"
  fi
fi

echo "[3/6] 二进制 Manifest（+StarryInitProvider；appComponentFactory → SignSpoofAppComponentFactory）"
if [[ ! -f "${CACHE_MF}" ]]; then
  apktool d -f -s --no-assets -o "${WORK}/mf_dec" "${BASE_APK}" >"${WORK}/apktool_d.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_d.log" >&2; die "apktool d 失败"; }
  "${PY}" - "${WORK}/mf_dec/AndroidManifest.xml" "${AUTHORITY}" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
t = p.read_text(encoding='utf-8')
NEW_FACTORY = 'com.starry.magic.sign.SignSpoofAppComponentFactory'
if NEW_FACTORY in t:
    print('  manifest 已是星光 Factory，跳过')
elif 'android:appComponentFactory=' in t:
    raise SystemExit('manifest 已有其它 appComponentFactory，需人工核对（本作原包应为「无」）')
else:
    # ★本作原包【没有】appComponentFactory 属性 ⇒ 新增（不是替换）。hzsj 路线是替换
    #   androidx.core.app.CoreComponentFactory；SignSpoofAppComponentFactory 自身在该类
    #   存在时委托给它、不存在时回退框架默认，故「新增」安全。
    i = t.find('<application')
    if i < 0:
        raise SystemExit('no <application> in manifest')
    j = t.find('>', i)
    if j < 0:
        raise SystemExit('malformed <application> tag')
    t = t[:j] + '\n        android:appComponentFactory="%s"' % NEW_FACTORY + t[j:]
    print('  manifest 新增 appComponentFactory=%s（原包无该属性）' % NEW_FACTORY)
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
p.write_text(t, encoding='utf-8')
print('  manifest patched (provider+service+perms+SignSpoof Factory)，application@name 零改动')
PY
  rm -rf "${WORK}/mf_dec"/smali* 2>/dev/null || true
  apktool b -f "${WORK}/mf_dec" -o "${WORK}/mf_only.apk" >"${WORK}/apktool_b.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_b.log" >&2; die "apktool b 失败"; }
  unzip -p "${WORK}/mf_only.apk" AndroidManifest.xml > "${CACHE_MF}" || die "提取二进制 manifest 失败"
  echo "  缓存 ${CACHE_MF} ($(wc -c <"${CACHE_MF}" | tr -d ' ') B)"
else
  echo "  缓存命中 ${CACHE_MF}（若为 2026-10-13 前的旧缓存，[4/6] 的 Factory 断言会失败；删缓存重跑即可）"
fi

echo "[4/6] 二进制 Manifest 回读校验（aapt2）"
"${PY}" - "${CACHE_MF}" "${WORKW}/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
"$AAPT2" dump xmltree --file AndroidManifest.xml "${WORK}/mf_check.apk" > "${WORK}/tree_new.txt" 2>/dev/null \
  || die "aapt2 无法解析新 manifest"
for need in \
  "com.starry.magic.StarryInitProvider" \
  "com.starry.magic.menu.MenuService" \
  "com.starry.magic.sign.SignSpoofAppComponentFactory" \
  "android.permission.SYSTEM_ALERT_WINDOW" \
  "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"; do
  grep -q "$need" "${WORK}/tree_new.txt" || die "新 manifest 缺 $need"
done
echo "  manifest ok（provider/service/Factory/权限均在）"

echo "[5/6] surgical zip merge（manifest + ${DEX_ADD_NAME} 新增 + so${LEGACY_MENU_DEX:+ + dex5 重建}）"
"${PY}" - "${BASE_APK}" "${CACHE_CLS5}" "${CACHE_MF}" "${DEX6_SRC}" "${DEX_ADD_NAME}" "${SO_SRC}" "${LIBCXX_SRC}" "${MERGED}" <<'PY'
import os, sys, zipfile

orig, cls5, mf, dex_src, dex_name, so, libcxx, out = sys.argv[1:9]
LEGACY = bool(cls5) and os.path.isfile(cls5)

REPL = {
    'AndroidManifest.xml': mf,
}
ADD = {dex_name: dex_src}
if LEGACY:
    REPL['classes5.dex'] = cls5

def is_sig(name):
    u = name.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return name.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

zin = zipfile.ZipFile(orig, 'r')
names = set(zin.namelist())
if dex_name in names:
    sys.exit('底包已含 %s（重复注入嫌疑）—— 请核对底包或改名' % dex_name)

# so（注入目标）：底包已有 = 替换（1.1.1 星光底包）；没有 = 新增（官方原包）
SO_ENTRY = 'lib/arm64-v8a/libstarry_magic.so'
if SO_ENTRY in names:
    REPL[SO_ENTRY] = so
else:
    ADD[SO_ENTRY] = so

# libc++_shared.so：新 so 的 DT_NEEDED（硬依赖）。官方 1.1.2 原包不带 ⇒ 必须注入，
# 否则 dlopen 失败：cannot locate symbol / library "libc++_shared.so" not found。
CXX_ENTRY = 'lib/arm64-v8a/libc++_shared.so'
if CXX_ENTRY in names:
    print('  底包自带 %s（%d B）→ 不覆盖' % (CXX_ENTRY, zin.getinfo(CXX_ENTRY).file_size))
else:
    ADD[CXX_ENTRY] = libcxx
    print('  底包缺 %s → 注入 NDK 同源副本 %d B' % (CXX_ENTRY, os.path.getsize(libcxx)))

for k in REPL:
    if k not in names:
        sys.exit('底包缺条目 %s' % k)

n_sig = copied = replaced = 0
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
    for item in zin.infolist():
        n = item.filename
        if is_sig(n):
            n_sig += 1
            continue
        zi = zipfile.ZipInfo(n, date_time=item.date_time)
        zi.external_attr = item.external_attr
        zi.internal_attr = item.internal_attr
        zi.create_system = item.create_system
        zi.comment = item.comment
        zi.compress_type = item.compress_type
        if n in REPL:
            zout.writestr(zi, open(REPL[n], 'rb').read())
            replaced += 1
            continue
        zout.writestr(zi, zin.read(n))
        copied += 1
    for n, p in sorted(ADD.items()):
        zi = zipfile.ZipInfo(n, date_time=(2026, 10, 7, 0, 0, 0))
        zi.external_attr = 0o100644 << 16
        zi.create_system = 3
        zi.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(zi, open(p, 'rb').read())

z2 = zipfile.ZipFile(out, 'r')
bad = []
for n in names:
    if n in REPL or is_sig(n):
        continue
    a, b = zin.getinfo(n), z2.getinfo(n)
    if a.CRC != b.CRC or a.file_size != b.file_size:
        bad.append(n)
if bad:
    sys.exit('passthrough mismatch (%d entries), first=%s' % (len(bad), bad[0]))
print('  entries: orig=%d copied=%d replaced=%d added=%d dropped_sig=%d'
      % (len(names), copied, replaced, len(ADD), n_sig))
print('  replaced: %s' % ','.join(sorted(REPL)))
print('  added   : %s' % ','.join(sorted(ADD)))
print('  passthrough CRC verified: %d entries' % copied)
print('  out size = %d B' % os.path.getsize(out))
PY

echo "[6/6] zipalign + apksigner + 校验"
"${ZIPALIGN}" -f -p 4 "${MERGED}" "${WORK}/aligned.apk" || die "zipalign failed"
rm -f "${FINAL}"
"${APKSIGNER}" sign \
  --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "${WORK}/final.apk" "${WORK}/aligned.apk" || die "apksigner failed"
cp -f "${WORK}/final.apk" "${FINAL}"

"${APKSIGNER}" verify --print-certs "${FINAL}" > "${WORK}/certs.txt" 2>&1 || true
LC_ALL=C grep -qi "${CERT_SHA256}" "${WORK}/certs.txt" || die "签发证书 SHA-256 不符"

"${PY}" - "${FINAL}" "${SO_SRC}" "${LIBCXX_SRC}" "${CACHE_CLS5}" "${DEX_ADD_NAME}" "${AAPT2}" <<'PY'
import sys, zipfile, hashlib, os, subprocess
apk, so, libcxx, cls5, dex_name, aapt2 = sys.argv[1:7]
legacy = bool(cls5) and os.path.isfile(cls5)
z = zipfile.ZipFile(apk)
md5b = lambda b: hashlib.md5(b).hexdigest()
in_so = md5b(z.read('lib/arm64-v8a/libstarry_magic.so'))
assert in_so == md5b(open(so, 'rb').read()), 'so md5 mismatch %s' % in_so
# libc++_shared.so：底包自带或注入，都必须存在且优先核对为注入源（新 so 硬依赖）
assert 'lib/arm64-v8a/libc++_shared.so' in z.namelist(), 'libc++_shared.so 缺失（dlopen 必失败）'
d = z.read(dex_name)
for s in (b'StarryInitProvider', b'com/starry/magic/menu/MenuService',
          b'com/starry/magic/bridge/ModBridge', b'com/starry/magic/ota/StarryNativeLoad'):
    assert s in d, '%s missing %s' % (dex_name, s.decode())
msg = 'so md5=%s / %s 含新菜单' % (in_so, dex_name)
# appComponentFactory 改写是 2026-10-13「TapTap 签名校验时机」修复的核心；最终包内回读确认。
# 注：二进制 AXML 的字符串池是 UTF-16，不能按 bytes 搜 ASCII —— 用 aapt2 dump 成文本再断言。
os.makedirs('/tmp/bwxrk_mfchk', exist_ok=True)
mfx = '/tmp/bwxrk_mfchk/mf.zip'
with zipfile.ZipFile(mfx, 'w', zipfile.ZIP_DEFLATED) as wz:
    wi = zipfile.ZipInfo('AndroidManifest.xml')
    wi.compress_type = zipfile.ZIP_DEFLATED
    wz.writestr(wi, z.read('AndroidManifest.xml'))
# aapt2 dump xmltree 只吃 zip/apk（不给裸 axml，会 rc=1）
r = subprocess.run([aapt2, 'dump', 'xmltree', '--file', 'AndroidManifest.xml', mfx],
                   capture_output=True, text=True)
assert r.returncode == 0, 'aapt2 dump 失败: %s' % r.stderr[-300:]
tree = r.stdout
assert 'com.starry.magic.sign.SignSpoofAppComponentFactory' in tree, \
    'manifest 未换 SignSpoofAppComponentFactory（签名伪装会晚于 TapSDK 冷启动）'
assert 'com.starry.magic.StarryInitProvider' in tree, 'manifest 缺 StarryInitProvider'
msg += ' / manifest Factory=SignSpoof'
if legacy:
    d5 = z.read('classes5.dex')
    assert b'com/starry/magic' not in d5, 'classes5.dex still has com/starry'
    assert b'kotlin' in d5, 'classes5.dex lost kotlin'
    msg += ' / dex5 无旧菜单且保留 kotlin'
print('  包内校验 ok: %s' % msg)
PY
fsize="$(wc -c <"${FINAL}" | tr -d ' ')"
echo "[done] ${FINAL} (${fsize} B)"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install（STARRY_INSTALL=1 SERIAL=... 装机）"
  exit 0
fi

echo "[install] 同签覆盖 → ${SERIAL}"
adb -s "${SERIAL}" shell am force-stop "${PKG}" 2>/dev/null || true
adb -s "${SERIAL}" install -r --no-incremental "${FINAL}" || die "adb install 失败"
echo "done → ${FINAL}"
echo "取证: adb -s ${SERIAL} logcat -s StarryMagic Hzsj Mod_Menu StarryInitProvider MenuService"