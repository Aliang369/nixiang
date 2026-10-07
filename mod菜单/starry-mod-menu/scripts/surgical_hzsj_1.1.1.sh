#!/usr/bin/env bash
# ================================================================
#  魂坠深境 1.1.1 (com.chillyroom.containerofsoul.yw) — 星光外科组装
#  （新菜单 provider-only 路线，照 surgical_zzz_3.2.0.sh）
#
#  背景：2026-10-07 用户要求「dex 菜单换新源码」。旧包（f191e461，2026-09-10
#  gradle+smali 注入路线）把菜单 smali 混进了 classes5.dex；新路线改为
#  「新菜单 classes6.dex + Manifest 注册 StarryInitProvider」——菜单/授权/签名
#  回灌/TapTap 强制网页登录都由 Java 侧 Provider 内化（StarryInitProvider.java），
#  不再需要逐游戏 smali 补丁；此后换菜单只需换 classes6.dex。
#
#  底包：02_apks/魂坠深境/魂坠深境_1.1.1_device.apk（实测 md5=f191e461… 星光正式包）
#  手术（四件，其余条目字节保真）：
#   1) classes5.dex  重建为「去 com/starry」（baksmali→删→smali；原 dex 仅 1.48MB，
#      内容 = kotlin/okhttp3/okio/org 789 文件 + 旧菜单 127 文件）→ 缓存复用
#   2) classes6.dex  新增（templates/hzsj_menu_20261007，hzsj_build_dex.sh 产）
#   3) AndroidManifest.xml  加 <provider com.starry.magic.StarryInitProvider>
#      （MenuService/权限旧包已有；application/ComponentFactory 零改动）→ 缓存复用
#   4) lib/arm64-v8a/libstarry_magic.so  替换（libs_hzsj 产物，含设备无关指针判据）
#
#  ★旧包 Activity 插桩（onCreate 调 MenuService.start / SignSpoof.install /
#    StarryNativeLoad.ensureLoaded）**保留不动**：classes5 的旧 com/starry 删除后，
#    这些引用自动解析到 classes6 的新类；MenuService.start 幂等（instance!=null 即 skip），
#    与 Provider 双启动无副作用（已核 11-07）。
#  ★不注入 libc++_shared.so（官包自带）。
#
#  签名：01_tools/starry_release.jks（CN=StarryMagic，alias starry）——禁换（卡密指纹）
#
#  用法:
#    bash scripts/surgical_hzsj_1.1.1.sh                    # 出包
#    STARRY_INSTALL=1 SERIAL=127.0.0.1:16416 bash ...       # 出包+装机（同签 -r）
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${STARRY_SRC}/../../04_scripts/_common/env.sh"

NIXIANG="${NIXIANG_ROOT}"
GAME="魂坠深境"
VERSION="1.1.1"
PKG="com.chillyroom.containerofsoul.yw"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-127.0.0.1:16416}"

BASE_APK="${HZSJ_BASE_APK:-${NIXIANG}/02_apks/${GAME}/${GAME}_1.1.1_device.apk}"
BASE_MD5_EXPECT="f191e461b4d83ef9d079d693e94cca35"
DEX6_NAME="classes6.dex"
DEX6_SRC="${STARRY_SRC}/templates/hzsj_menu_20261007/${DEX6_NAME}"
SO_SRC="${STARRY_SRC}/libs_hzsj/arm64-v8a/libstarry_magic.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-newmenu-releaseSigned.apk"

# 一次性手术缓存（dex5 去菜单 / 二进制 Manifest）
CACHE_DIR="${NIXIANG}/07_patched/${GAME}/surgical"
CACHE_CLS5="${CACHE_DIR}/classes5_nostarry.dex"
CACHE_MF="${CACHE_DIR}/AndroidManifest.xml"

KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

JAVA17="${NIXIANG_JAVA17}"
JAVA="$JAVA17/bin/java.exe"; [ -f "$JAVA" ] || JAVA="$JAVA17/bin/java"
BAKSMALI="${NIXIANG}/01_tools/smali/baksmali-fat.jar"
SMALI="${NIXIANG}/01_tools/smali/smali-fat.jar"
APKTOOL="${APKTOOL:-${NIXIANG}/01_tools/apktool/apktool}"
AAPT2="${AAPT2:-${NIXIANG_BUILD_TOOLS}/aapt2.exe}"
[ -f "$AAPT2" ] || AAPT2="${NIXIANG_BUILD_TOOLS}/aapt2"
PY="${NIXIANG_PYTHON:-python}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-${NIXIANG_BUILD_TOOLS}/zipalign}"
APKSIGNER="${NIXIANG_APKSIGNER:-${NIXIANG_BUILD_TOOLS}/apksigner}"
if [[ -n "${NIXIANG_JAVA17:-}" ]]; then
  export JAVA_HOME="${NIXIANG_JAVA17}"
  export PATH="${NIXIANG_JAVA17}/bin:${PATH}"
fi

die() { echo "ERROR: $*" >&2; exit 1; }
md5f() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }

ASCII_ROOT="${TEMP:-${TMPDIR:-/tmp}}"
if command -v cygpath >/dev/null 2>&1; then
  ASCII_ROOT="$(cygpath -u "${ASCII_ROOT}" 2>/dev/null || printf '%s' "${ASCII_ROOT}")"
fi
mkdir -p "${ASCII_ROOT}" "${OUT_DIR}" "${CACHE_DIR}"

[[ -f "${BASE_APK}" ]] || die "缺底包 ${BASE_APK}（HZSJ_BASE_APK=... 覆盖）"
[[ -f "${DEX6_SRC}" ]] || die "缺 ${DEX6_SRC}（先跑 04_scripts/魂坠深境/hzsj_build_dex.sh）"
[[ -f "${SO_SRC}" ]]   || die "缺 ${SO_SRC}（先跑 04_scripts/魂坠深境/hzsj_build_so.sh）"
[[ -f "${JAVA}" ]]     || die "缺 java: ${JAVA}"
[[ -f "${BAKSMALI}" && -f "${SMALI}" ]] || die "缺 smali 工具（01_tools/smali）"
[[ -x "${ZIPALIGN}" ]] || die "缺 zipalign: ${ZIPALIGN}"
[[ -x "${APKSIGNER}" || -f "${APKSIGNER}" ]] || die "缺 apksigner: ${APKSIGNER}"

LOCK_DIR="/tmp/hzsj_surgical.lockdir"
if ! mkdir "${LOCK_DIR}" 2>/dev/null; then
  die "lock ${LOCK_DIR} held（另一个外科进程在跑）"
fi
echo $$ >"${LOCK_DIR}/pid"
WORK=""
cleanup() { [[ -n "${WORK}" && -d "${WORK}" ]] && rm -rf "${WORK}"; rm -rf "${LOCK_DIR}"; }
trap cleanup EXIT
WORK="$(mktemp -d "${ASCII_ROOT}/hzsj_surgical.XXXXXX")"
WORKW="$(cygpath -m "${WORK}" 2>/dev/null || printf '%s' "${WORK}")"
MERGED="${WORK}/merged.apk"

echo "[1/6] 输入检查"
bsize="$(wc -c <"${BASE_APK}" | tr -d ' ')"
bmd5="$(md5f "${BASE_APK}")"
[[ "${bmd5}" == "${BASE_MD5_EXPECT}" ]] || die "底包指纹不符 ${bmd5}（期望 ${BASE_MD5_EXPECT}）"
for s in 'com/starry/magic/StarryInitProvider' 'com/starry/magic/menu/MenuService'; do
  c="$(LC_ALL=C grep -ac "$s" "${DEX6_SRC}" || true)"
  [[ "${c}" -ge 1 ]] || die "classes6.dex 缺 ${s}"
done
for s in 'Toggle_秒杀' 'Toggle_自动胜利' '[Hzsj]'; do
  LC_ALL=C grep -aq "$s" "${SO_SRC}" || die "so 缺 ${s}"
done
echo "[check] 底包 ${bsize} B (md5 ${bmd5}) / dex6 $(wc -c <"${DEX6_SRC}" | tr -d ' ') B / so $(wc -c <"${SO_SRC}" | tr -d ' ') B ok"

echo "[2/6] classes5.dex → 去 com/starry 重建"
if [[ ! -f "${CACHE_CLS5}" ]]; then
  "${PY}" - "${BASE_APK}" "${WORKW}/classes5.dex" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
open(sys.argv[2], 'wb').write(z.read('classes5.dex'))
print('  classes5.dex extracted')
PY
  "$JAVA" -jar "${BAKSMALI}" d -o "${WORKW}/dis5" "${WORKW}/classes5.dex" >"${WORK}/baksmali.log" 2>&1 \
    || { tail -20 "${WORK}/baksmali.log" >&2; die "baksmali 失败"; }
  [[ -d "${WORK}/dis5/com/starry" ]] || die "classes5 反编后未找到 com/starry（底包结构变化？）"
  n_starry="$(find "${WORK}/dis5/com/starry" -name '*.smali' | wc -l | tr -d ' ')"
  n_total="$(find "${WORK}/dis5" -name '*.smali' | wc -l | tr -d ' ')"
  rm -rf "${WORK}/dis5/com/starry"
  "$JAVA" -jar "${SMALI}" a -o "${WORKW}/classes5_nostarry.dex" "${WORKW}/dis5" >"${WORK}/smali.log" 2>&1 \
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

echo "[3/6] 二进制 Manifest（+StarryInitProvider；application/Factory 零改动）"
if [[ ! -f "${CACHE_MF}" ]]; then
  "${APKTOOL}" d -f -s --no-assets -o "${WORK}/mf_dec" "${BASE_APK}" >"${WORK}/apktool_d.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_d.log" >&2; die "apktool d 失败"; }
  "${PY}" - "${WORK}/mf_dec/AndroidManifest.xml" "${AUTHORITY}" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
t = p.read_text(encoding='utf-8')
if 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"' not in t:
    raise SystemExit('manifest assertion failed: missing appComponentFactory')
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
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
print('  manifest patched (provider+service+perms)，application 零改动')
PY
  rm -rf "${WORK}/mf_dec"/smali* 2>/dev/null || true
  "${APKTOOL}" b -f "${WORK}/mf_dec" -o "${WORK}/mf_only.apk" >"${WORK}/apktool_b.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_b.log" >&2; die "apktool b 失败"; }
  unzip -p "${WORK}/mf_only.apk" AndroidManifest.xml > "${CACHE_MF}" || die "提取二进制 manifest 失败"
  echo "  缓存 ${CACHE_MF} ($(wc -c <"${CACHE_MF}" | tr -d ' ') B)"
else
  echo "  缓存命中 ${CACHE_MF}"
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
  "androidx.core.app.CoreComponentFactory" \
  "android.permission.SYSTEM_ALERT_WINDOW" \
  "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"; do
  grep -q "$need" "${WORK}/tree_new.txt" || die "新 manifest 缺 $need"
done
echo "  manifest ok（provider/service/Factory/权限均在）"

echo "[5/6] surgical zip merge（dex5 重建 + dex6 新增 + manifest + so）"
"${PY}" - "${BASE_APK}" "${CACHE_CLS5}" "${CACHE_MF}" "${DEX6_SRC}" "${SO_SRC}" "${MERGED}" <<'PY'
import os, sys, zipfile

orig, cls5, mf, dex6, so, out = sys.argv[1:7]
REPL = {
    'AndroidManifest.xml': mf,
    'classes5.dex': cls5,
    'lib/arm64-v8a/libstarry_magic.so': so,
}
ADD = {'classes6.dex': dex6}

def is_sig(name):
    u = name.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return name.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

zin = zipfile.ZipFile(orig, 'r')
names = set(zin.namelist())
if 'classes6.dex' in names:
    sys.exit('底包已含 classes6.dex（重复注入嫌疑）—— 请核对底包或改名')
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

"${PY}" - "${FINAL}" "${SO_SRC}" "${CACHE_CLS5}" <<'PY'
import sys, zipfile, hashlib
apk, so, cls5 = sys.argv[1:4]
z = zipfile.ZipFile(apk)
def md5b(b): return hashlib.md5(b).hexdigest()
in_so = md5b(z.read('lib/arm64-v8a/libstarry_magic.so'))
exp_so = md5b(open(so, 'rb').read())
assert in_so == exp_so, 'so md5 mismatch %s != %s' % (in_so, exp_so)
d6 = z.read('classes6.dex')
for s in (b'StarryInitProvider', b'com/starry/magic/menu/MenuService'):
    assert s in d6, 'classes6.dex missing %s' % s.decode()
d5 = z.read('classes5.dex')
assert b'com/starry/magic' not in d5, 'classes5.dex still has com/starry'
assert b'kotlin' in d5, 'classes5.dex lost kotlin'
print('  包内校验 ok: so md5=%s / dex6 含新菜单 / dex5 无旧菜单且保留 kotlin' % in_so)
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