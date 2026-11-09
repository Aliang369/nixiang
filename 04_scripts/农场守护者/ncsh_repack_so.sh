#!/usr/bin/env bash
# ================================================================
#  ncsh_repack_so.sh — 农场守护者「只换 so」重打（底包缺失时的唯一路径）
#
#  背景（2026-11-11）：02_apks/农场守护者/ 被清空（官方底包与斌哥包同时消失），
#  surgical_ncsh_2.1.21.sh 无法再跑。而本轮只改 native（菜单项由 featureList 决定），
#  classes2.dex / AndroidManifest 完全不用动 ⇒ 直接在成品包上换 so + 重签即可。
#
#  用法:
#    bash 04_scripts/农场守护者/ncsh_repack_so.sh [--apk <成品APK>] [--so <新so>] [--dry-run]
#
#  校验口径：条目齐全（classes.dex/classes2.dex/Manifest/so/libc++）、包内 so md5==新 so、
#          菜单串只在 so 里出现应有的两项、apksigner verify 通过。
# ================================================================
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
source "${ROOT}/04_scripts/_common/env.sh" 2>/dev/null || true

GAME="农场守护者"
VERSION="2.1.21"
PKG="com.qld.ncsh.xiyou"

OUT_DIR="${ROOT}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"
KEEP_DIR="${ROOT}/07_patched/${GAME}/surgical"

APK="${FINAL}"
SO="${ROOT}/mod菜单/starry-mod-menu/libs_ncsh/arm64-v8a/libstarry_magic.so"
DRY=0

while [ $# -gt 0 ]; do
  case "$1" in
    --apk) APK="$2"; shift 2 ;;
    --so)  SO="$2"; shift 2 ;;
    --dry-run) DRY=1; shift ;;
    -h|--help) sed -n '2,20p' "$0" | sed 's/^# \?//'; exit 0 ;;
    *) echo "unknown: $1" >&2; exit 2 ;;
  esac
done

PY="${NIXIANG_PYTHON:-python3}"
ZIPALIGN="${NIXIANG_ZIPALIGN}"
APKSIGNER="${NIXIANG_APKSIGNER}"
KS="${STARRY_KS:-${ROOT}/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SO_IN_APK="lib/arm64-v8a/libstarry_magic.so"
md5f() { "${PY}" -c 'import hashlib,sys;print(hashlib.md5(open(sys.argv[1],"rb").read()).hexdigest())' "$1"; }
die() { echo "ERROR: $*" >&2; exit 1; }

[ -f "${APK}" ] || die "缺成品包 ${APK}"
[ -f "${SO}" ]  || die "缺新 so ${SO}"
[ -x "${ZIPALIGN}" ] || die "缺 zipalign"
[ -f "${APKSIGNER}" ] || die "缺 apksigner"

SO_MD5="$(md5f "${SO}")"
echo "[1/5] 输入"
echo "      apk=${APK} ($(wc -c <"${APK}" | tr -d ' ') B, $(md5f "${APK}"))"
echo "      so =${SO} ($(wc -c <"${SO}" | tr -d ' ') B, ${SO_MD5})"

mkdir -p "${KEEP_DIR}"
WORK="$(mktemp -d "${TMPDIR:-/tmp}/ncsh_repack.XXXXXX")"
cleanup() { rm -rf "${WORK}"; }
trap cleanup EXIT

if [ "${DRY}" = "1" ]; then
  echo "[dry-run] 将执行：换 so -> zipalign -p 4 -> apksigner -> 覆盖 ${FINAL}"
  exit 0
fi

# 旧成品包是底包丢失后唯一可回退副本 ⇒ 先保全（只保留最近一份）
if [ -f "${FINAL}" ]; then
  KB="${KEEP_DIR}/ncsh_prev_$(md5f "${FINAL}" | cut -c1-8).apk"
  if [ ! -f "${KB}" ]; then
    echo "[2/5] 保全旧成品包 -> ${KB}"
    cp -f "${FINAL}" "${KB}"
  else
    echo "[2/5] 旧成品包已有保全副本，跳过"
  fi
fi

echo "[3/5] 重组 zip（替换 ${SO_IN_APK}）"
"${PY}" - "${APK}" "${SO}" "${WORK}/merged.apk" "${SO_IN_APK}" <<'PY'
import sys, zipfile
apk, so, out, target = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
zin = zipfile.ZipFile(apk)
have = set(zin.namelist())
if target not in have:
    sys.exit('APK 内没有 %s（不是本游戏的星光包？）' % target)
newdata = open(so, 'rb').read()
with zipfile.ZipFile(out, 'w') as zo:
    for it in zin.infolist():
        if it.filename == target:
            data = newdata
        else:
            data = zin.read(it.filename)
        zi = zipfile.ZipInfo(it.filename, date_time=it.date_time)
        zi.compress_type = it.compress_type if it.filename != target else zipfile.ZIP_STORED
        zi.external_attr = it.external_attr
        zo.writestr(zi, data)
print('      entries=%d so=%d B' % (len(have), len(newdata)))
PY

echo "[4/5] zipalign + 签名"
"${ZIPALIGN}" -f -p 4 "${WORK}/merged.apk" "${WORK}/aligned.apk" || die "zipalign 失败"
"${APKSIGNER}" sign --ks "${KS}" --ks-key-alias "${KS_ALIAS}" --ks-pass "pass:${KS_PASS}" \
  --key-pass "pass:${KS_PASS}" --v4-signing-enabled false \
  --out "${WORK}/signed.apk" "${WORK}/aligned.apk" || die "apksigner 失败"

echo "[5/5] 产物校验 + 落盘"
"${PY}" - "${WORK}/signed.apk" "${SO_MD5}" <<'PY'
import sys, zipfile, hashlib
f, want = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(f); n = set(z.namelist())
need = ['classes.dex','classes2.dex','AndroidManifest.xml',
        'lib/arm64-v8a/libstarry_magic.so','lib/arm64-v8a/libc++_shared.so']
miss = [k for k in need if k not in n]
got = hashlib.md5(z.read('lib/arm64-v8a/libstarry_magic.so')).hexdigest()
print('      entries=%d missing=%s' % (len(n), miss or 'none'))
print('      so md5 in apk=%s (want %s) %s' % (got, want, 'OK' if got == want else 'MISMATCH'))
mf = z.read('AndroidManifest.xml')
so = z.read('lib/arm64-v8a/libstarry_magic.so')
Z = lambda x: x.encode('utf-8')
Z16 = lambda x: x.encode('utf-16-le')   # AXML 字符串池是 UTF-16，必须按 UTF-16 匹配
checks = [
    (Z16('StarryInitProvider') in mf or b'StarryInitProvider' in mf, 'provider 注入在'),
    (b'com/starry/magic/menu/MenuService' in z.read('classes2.dex'), '菜单 dex 在'),
    (Z('Toggle_百倍伤害') in so, '菜单串 百倍伤害'),
    (Z('Toggle_无敌') in so, '菜单串 无敌'),
    (Z('Toggle_无限技能') in so, '菜单串 无限技能'),
    (Z('Toggle_解锁加速') in so, '菜单串 解锁加速'),
]
bad = [nm for ok, nm in checks if not ok]
for ok, nm in checks: print('      %s %s' % ('OK  ' if ok else 'FAIL', nm))
sys.exit(1 if (miss or got != want or bad) else 0)
PY

cp -f "${WORK}/signed.apk" "${FINAL}"
"${APKSIGNER}" verify "${FINAL}" || die "apksigner verify 失败"
echo "OK -> ${FINAL}"
echo "     $(wc -c <"${FINAL}" | tr -d ' ') B / md5 $(md5f "${FINAL}")"
echo "     old kept: ${KB:-(none)}"
