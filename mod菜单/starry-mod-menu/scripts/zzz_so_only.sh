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
# 2026-10-28：SignSpoof 下线后本包必须带「改包法」dex 补丁（TapTap 验签 + 网页登录）
#   classes4.dex = 游戏侧 dex（TapSDK 门闩/登录 handler），由 zzz_tapsdk_dexpatch_3.3.0.py 产出
#   classes6.dex = 星光菜单 dex（由 zzz_build_dex.sh 产出，已不含 SignSpoof）
DEX4_SRC="${ZZZ_CLASSES4_DEX:-${NIXIANG}/07_patched/${GAME}/tapsdk_patch/classes4.dex}"
DEX6_SRC="${ZZZ_CLASSES6_DEX:-${STARRY_SRC}/templates/zzz_menu_current/classes6.dex}"
OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"

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
[[ -f "${DEX4_SRC}" ]] || die "缺 ${DEX4_SRC}（先跑 python3 04_scripts/绝区零/zzz_tapsdk_dexpatch_3.3.0.py）"
[[ -f "${DEX6_SRC}" ]] || die "缺 ${DEX6_SRC}（先跑 04_scripts/绝区零/zzz_build_dex.sh）"
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
echo "  dex4 = ${DEX4_SRC} ($(wc -c <"${DEX4_SRC}" | tr -d ' ') B, md5 $(md5f "${DEX4_SRC}"))"
echo "  dex6 = ${DEX6_SRC} ($(wc -c <"${DEX6_SRC}" | tr -d ' ') B, md5 $(md5f "${DEX6_SRC}"))"

WORK="$(mktemp -d "${ASCII_ROOT}/zzz_so_only.XXXXXX")"
trap 'rm -rf "${WORK}"' EXIT
MERGED="${WORK}/merged.apk"

echo "[1/5] 外科 zip-merge：替换 so + classes4.dex（TapSDK 改包法）+ classes6.dex（菜单），其余条目字节保真"
"${PY}" - "${BASE_APK}" "${MERGED}" \
  "lib/arm64-v8a/libstarry_magic.so=${SO_SRC}" \
  "classes4.dex=${DEX4_SRC}" \
  "classes6.dex=${DEX6_SRC}" <<'PY'
import sys, zipfile

orig, out = sys.argv[1], sys.argv[2]
repl = {}
for spec in sys.argv[3:]:
    k, _, v = spec.partition('=')
    repl[k] = v

def is_sig(n):
    u = n.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return n.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

zin = zipfile.ZipFile(orig, 'r')
names = set(zin.namelist())
missing = [k for k in repl if k not in names]
if missing:
    sys.exit('base apk 缺条目 %s（底包不对？）' % ', '.join(missing))

n_sig = copied = 0
replaced = []
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
        if n in repl:
            zout.writestr(zi, open(repl[n], 'rb').read())
            replaced.append(n)
            continue
        zout.writestr(zi, zin.read(n))
        copied += 1

z2 = zipfile.ZipFile(out, 'r')
bad = [n for n in names if n not in repl and not is_sig(n)
       and (zin.getinfo(n).CRC != z2.getinfo(n).CRC or zin.getinfo(n).file_size != z2.getinfo(n).file_size)]
if bad:
    sys.exit('passthrough mismatch (%d), first=%s' % (len(bad), bad[0]))
if sorted(replaced) != sorted(repl):
    sys.exit('替换条目不全：%s' % ', '.join(replaced))
for k in replaced:
    print('  replaced %-38s %d B' % (k, len(repl[k]) and __import__('os').path.getsize(repl[k])))
print('  entries copied=%d replaced=%d dropped_sig=%d' % (copied, len(replaced), n_sig))
print('  out size = %d B' % __import__('os').path.getsize(out))
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

echo "[4/5] 校验：包内 so/dex md5 == 构建产物；签发证书 == StarryMagic"
"${PY}" - "${FINAL}" "${WORK}" <<'PY'
import sys, zipfile, hashlib
apk, work = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
for key, name in (('lib/arm64-v8a/libstarry_magic.so', 'in_so.so'),
                  ('classes4.dex', 'in_classes4.dex'),
                  ('classes6.dex', 'in_classes6.dex')):
    open(work + '/' + name, 'wb').write(z.read(key))
PY
for pair in "lib/arm64-v8a/libstarry_magic.so:${WORK}/in_so.so:${SO_SRC}" \
            "classes4.dex:${WORK}/in_classes4.dex:${DEX4_SRC}" \
            "classes6.dex:${WORK}/in_classes6.dex:${DEX6_SRC}"; do
  key="${pair%%:*}"; rest="${pair#*:}"; got="${rest%%:*}"; want="${rest##*:}"
  a="$(md5f "${want}")"; b="$(md5f "${got}")"
  [[ "${a}" = "${b}" ]] || die "包内 ${key} md5(${b}) != 构建产物(${a})"
  echo "  ${key} md5 ok = ${a}"
done
"${APKSIGNER}" verify --print-certs "${FINAL}" > "${WORK}/certs.txt" 2>&1 || true
LC_ALL=C grep -qi "${CERT_SHA256}" "${WORK}/certs.txt" || die "签发证书 SHA-256 不符"
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
