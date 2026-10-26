#!/usr/bin/env bash
# ================================================================
#  绝区零 3.3.0 so-only 重打（mac/Win 跨平台，照 hzsj_so_only.sh）
#
#  用途：真机/模拟器已装**星光包**，只需换 lib/arm64-v8a/libstarry_magic.so
#        （只改 native：R36 只读探针钩子，不动 dex/manifest/资源）→ 同签重签 → install -r。
#
#  底包：优先 $ZZZ_BASE_APK；否则从设备只读拉取**已装** base.apk（同签覆盖安装，不卸载）
#  签名：01_tools/starry_release.jks（CN=StarryMagic）—— 换证书 = 换设备指纹 → 卡密 403，禁换
#
#  用法:
#    bash mod菜单/starry-mod-menu/scripts/zzz_so_only.sh                    # 只出包
#    STARRY_INSTALL=1 SERIAL=f9a116a2 bash mod菜单/starry-mod-menu/scripts/zzz_so_only.sh
#    ZZZ_BASE_APK=/path/to/starry.apk bash ...                             # 指定底包
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${STARRY_SRC}/../../04_scripts/_common/env.sh"

NIXIANG="${NIXIANG_ROOT}"
GAME="绝区零"
VERSION="3.3.0"
PKG="com.miHoYo.Nap"
SERIAL="${SERIAL:-f9a116a2}"

SO_SRC="${ZZZ_SO_SRC:-${STARRY_SRC}/libs_zzz/arm64-v8a/libstarry_magic.so}"
OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-so-only-releaseSigned.apk"

KS="${STARRY_KS:-${NIXIANG}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"

PY="${NIXIANG_PYTHON:-python3}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-${NIXIANG_BUILD_TOOLS}/zipalign}"
APKSIGNER="${NIXIANG_APKSIGNER:-${NIXIANG_BUILD_TOOLS}/apksigner}"
if [[ -n "${NIXIANG_JAVA17:-}" ]]; then
  export JAVA_HOME="${NIXIANG_JAVA17}"
  export PATH="${NIXIANG_JAVA17}/bin:${PATH}"
fi

die() { echo "ERROR: $*" >&2; exit 1; }
md5f() { if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }

# ASCII 暂存（中文路径下 zip/python 也会踩 GBK 乱码）
ASCII_ROOT="${TEMP:-${TMPDIR:-/tmp}}"
if command -v cygpath >/dev/null 2>&1; then
  ASCII_ROOT="$(cygpath -u "${ASCII_ROOT}" 2>/dev/null || printf '%s' "${ASCII_ROOT}")"
fi
BASE_DIR="${ASCII_ROOT}/nx_zzz_so_only"
mkdir -p "${BASE_DIR}" "${OUT_DIR}"
BASE_APK="${ZZZ_BASE_APK:-${BASE_DIR}/base_starry.apk}"

[[ -f "${SO_SRC}" ]] || die "缺 ${SO_SRC}（先跑 04_scripts/绝区零/zzz_build_so.sh）"
[[ -f "${KS}" ]]     || die "缺签名密钥 ${KS}"
[[ -x "${ZIPALIGN}" ]] || die "缺 zipalign: ${ZIPALIGN}"
[[ -x "${APKSIGNER}" || -f "${APKSIGNER}" ]] || die "缺 apksigner: ${APKSIGNER}"

echo "[0/5] 底包准备"
if [[ ! -f "${BASE_APK}" ]]; then
  echo "  底包缺失 → 从设备 ${SERIAL} 只读拉取已装 base.apk"
  remote="$(adb -s "${SERIAL}" shell pm path "${PKG}" 2>/dev/null | tr -d '\r' | sed 's/^package://')"
  [[ -n "${remote}" ]] || die "设备 ${SERIAL} 未装 ${PKG}（可用 ZZZ_BASE_APK=... 指定底包）"
  adb -s "${SERIAL}" pull "${remote}" "${BASE_APK}" >/dev/null || die "adb pull 失败: ${remote}"
fi
[[ -f "${BASE_APK}" ]] || die "无可用底包 ${BASE_APK}"
echo "  base = ${BASE_APK} ($(wc -c <"${BASE_APK}" | tr -d ' ') B)"
echo "  so   = ${SO_SRC} ($(wc -c <"${SO_SRC}" | tr -d ' ') B, md5 $(md5f "${SO_SRC}"))"

WORK="$(mktemp -d "${ASCII_ROOT}/zzz_so_only.XXXXXX")"
trap 'rm -rf "${WORK}"' EXIT
MERGED="${WORK}/merged.apk"

echo "[1/5] 外科 zip-merge：仅替换 lib/arm64-v8a/libstarry_magic.so，其余条目字节保真"
"${PY}" - "${BASE_APK}" "${WORK}" "${MERGED}" "${SO_SRC}" <<'PY'
import os, sys, zipfile

orig, work, out, so = sys.argv[1:5]
KEY = 'lib/arm64-v8a/libstarry_magic.so'

def is_sig(n):
    u = n.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return n.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

zin = zipfile.ZipFile(orig, 'r')
names = set(zin.namelist())
if KEY not in names:
    sys.exit('base apk 缺 %s（底包不是星光包？）' % KEY)

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
        if n == KEY:
            zout.writestr(zi, open(so, 'rb').read())
            replaced += 1
            continue
        zout.writestr(zi, zin.read(n))
        copied += 1

z2 = zipfile.ZipFile(out, 'r')
bad = [n for n in names if n != KEY and not is_sig(n)
       and (zin.getinfo(n).CRC != z2.getinfo(n).CRC or zin.getinfo(n).file_size != z2.getinfo(n).file_size)]
if bad:
    sys.exit('passthrough mismatch (%d), first=%s' % (len(bad), bad[0]))
print('  entries copied=%d replaced=%d dropped_sig=%d' % (copied, replaced, n_sig))
print('  out size = %d B' % os.path.getsize(out))
PY

echo "[2/5] zipalign -f -p 4"
"${ZIPALIGN}" -f -p 4 "${MERGED}" "${WORK}/aligned.apk" || die "zipalign failed"

echo "[3/5] apksigner（CN=StarryMagic，v1+v2+v3）"
rm -f "${FINAL}"
"${APKSIGNER}" sign \
  --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "${WORK}/final.apk" "${WORK}/aligned.apk" || die "apksigner failed"
cp -f "${WORK}/final.apk" "${FINAL}"
[[ -f "${FINAL}" ]] || die "copy to ${FINAL} failed"

echo "[4/5] 校验：包内 so md5 == 构建产物；签发证书 == StarryMagic"
"${PY}" -c "import zipfile,sys;open(sys.argv[2],'wb').write(zipfile.ZipFile(sys.argv[1]).read('lib/arm64-v8a/libstarry_magic.so'))" "${FINAL}" "${WORK}/in_so.so"
a="$(md5f "${SO_SRC}")"; b="$(md5f "${WORK}/in_so.so")"
[[ "${a}" = "${b}" ]] || die "包内 so md5(${b}) != 构建产物(${a})"
"${APKSIGNER}" verify --print-certs "${FINAL}" > "${WORK}/certs.txt" 2>&1 || true
LC_ALL=C grep -qi "${CERT_SHA256}" "${WORK}/certs.txt" || die "签发证书 SHA-256 不符"
echo "  so md5 ok = ${a}"
echo "  apk md5   = $(md5f "${FINAL}")"
echo "[done] ${FINAL} ($(wc -c <"${FINAL}" | tr -d ' ') B)"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install（STARRY_INSTALL=1 SERIAL=... 装机）"
  exit 0
fi

echo "[5/5] 装机 → ${SERIAL}（同签覆盖 -r，无需卸载/无 su）"
adb -s "${SERIAL}" shell am force-stop "${PKG}" 2>/dev/null || true
adb -s "${SERIAL}" install -r --no-incremental "${FINAL}" || die "adb install 失败"
echo "done → ${FINAL}"
echo "取证: adb -s ${SERIAL} logcat -s StarryMagic"
