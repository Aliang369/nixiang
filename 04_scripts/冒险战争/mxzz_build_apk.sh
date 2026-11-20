#!/usr/bin/env bash
# 冒险战争 (mxzz) — 外科 zip 组装 + 签名（不动底包除 Manifest/dex/so 之外的任何字节）
#
# 输入（全部本仓产物，无网络、无 apktool 整包重建）：
#   底包   02_apks/冒险战争_1.1.036_7_orig.apk（1,507,414,974 B，sha256 BFA8D753…）
#   清单   07_patched/冒险战争/AndroidManifest.xml（patch_mxzz_binary_manifest.py 产物，缓存复用）
#   dex    07_patched/冒险战争/menu_current/classes6.dex（mxzz_build_dex.sh 产物）
#   so     mod菜单/starry-mod-menu/libs_mxzz/arm64-v8a/libstarry_magic.so（mxzz_build_so.sh 产物）
#
# 产物：08_output/冒险战争/冒险战争_<ver>_b<N>.apk
#
# 纪律：
#   · 不整包 apktool b；只替换/新增 4 类条目（Manifest、classes6.dex、lib/arm64-v8a/libstarry_magic.so、META-INF 清理）
#   · 其余条目逐字节透传，并在组装后**回读断言** classes/2/3/4/5 + resources.arsc 与底包 sha256 相同
#   · 装机不在这里做（异签需卸载；由 Lead 发卡执行）
#
# 用法：bash 04_scripts/冒险战争/mxzz_build_apk.sh
#      MXZZ_BUILD_NO=b2 bash ...       # 覆盖构建编号（默认 b1）
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
# shellcheck source=../_common/env.sh
. "${HERE}/../_common/env.sh"

ROOT="${NIXIANG_ROOT}"
VER="1.1.036"
PKG="com.mxzz.zbml"
BUILD_NO="${MXZZ_BUILD_NO:-b1}"
BASE_APK="${ROOT}/02_apks/冒险战争_1.1.036_7_orig.apk"
BASE_SHA_EXPECT="bfa8d753a7061d196ba81a7ac26019e5bd3b342463f098e7f63657c9a8264ab6"
PDIR="${ROOT}/07_patched/冒险战争"
MF="${PDIR}/AndroidManifest.xml"
DEX="${PDIR}/menu_current/classes6.dex"
ABI="arm64-v8a"
SO="${ROOT}/mod菜单/starry-mod-menu/libs_mxzz/arm64-v8a/libstarry_magic.so"
OUTDIR="${ROOT}/08_output/冒险战争"
OUT="${OUTDIR}/冒险战争_${VER}_${BUILD_NO}.apk"
KS="${ROOT}/01_tools/starry_release.jks"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PY="${NIXIANG_PYTHON:-python}"

die() { echo "ERROR: $*" >&2; exit 1; }
sha256_of() { "${PY}" -c "import hashlib,sys;print(hashlib.sha256(open(sys.argv[1],'rb').read()).hexdigest())" "$1"; }

# ---- libc++_shared.so（b3 起必须随包；b2 漏掉 ⇒ dlopen 失败 ⇒ 菜单「native 未就绪」）----
# 硬约束：必须取**构建本 so 所用的同一套 NDK** 的 prebuilt，ABI/SONAME 才一致。
# 证据：libstarry_magic.so 的 DT_NEEDED 含 libc++_shared.so（llvm-readelf -d）。
NDK="${ANDROID_NDK_HOME:-}"
_REPO_NDK="${ROOT}/01_tools/android-sdk/ndk/android-ndk-r27c"
if [ -d "${_REPO_NDK}" ]; then NDK="${_REPO_NDK}"; fi
[ -n "${NDK}" ] && [ -d "${NDK}" ] || die "找不到 NDK（设 ANDROID_NDK_HOME）"
NDK_BIN="$(ls -d "${NDK}"/toolchains/llvm/prebuilt/*/bin 2>/dev/null | head -1 || true)"
READELF="$(ls "${NDK_BIN}"/llvm-readelf* 2>/dev/null | head -1 || true)"
# 依赖件选择（★ 见实现记录 §十）：优先 verifier 提供的**已交付包同款**参照件
#   09_test/冒险战争/static/ref_libc++_shared_92ee8da6.so（1,292,896 B / 92ee8da6…，5 个已交付包在用）；
#   没有则退回**构建本 so 的同一 NDK**（r27c）的 prebuilt。
#   两者已做符号闭包对照：对 libstarry_magic.so 的 13 个 libc++ 类未定义符号，
#   两份 candidate 的 defined 集都是 2336 个、缺失同为 __cxa_atexit@LIBC / __cxa_finalize@LIBC（libc 提供）⇒ 等价。
REF_LIBCXX="${ROOT}/09_test/冒险战争/static/ref_libc++_shared_92ee8da6.so"
LIBCXX="${MXZZ_LIBCXX:-}"
if [ -z "${LIBCXX}" ] && [ -f "${REF_LIBCXX}" ]; then LIBCXX="${REF_LIBCXX}"; fi
if [ -z "${LIBCXX}" ]; then
  LIBCXX="$(ls "${NDK}"/toolchains/llvm/prebuilt/*/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so 2>/dev/null | head -1 || true)"
fi


[ -f "${BASE_APK}" ] || die "缺底包 ${BASE_APK}"
[ -f "${DEX}" ] || die "缺 dex ${DEX}（先跑 mxzz_build_dex.sh）"
[ -f "${SO}" ] || die "缺 so ${SO}（先跑 mxzz_build_so.sh）"
[ -f "${KS}" ] || die "缺签名库 ${KS}"
[ -n "${NIXIANG_ZIPALIGN:-}" ] && [ -f "${NIXIANG_ZIPALIGN}" ] || die "缺 zipalign（env.sh）"
[ -n "${NIXIANG_APKSIGNER:-}" ] && [ -f "${NIXIANG_APKSIGNER}" ] || die "缺 apksigner（env.sh）"

echo "[0/6] 底包身份核对"
BSHA="$(sha256_of "${BASE_APK}")"
if [ "${BSHA}" != "${BASE_SHA_EXPECT}" ]; then
  die "底包 sha256 不符：${BSHA}（期望 ${BASE_SHA_EXPECT}）"
fi
echo "  底包 ok sha256=${BSHA} size=$(wc -c < "${BASE_APK}" | tr -d ' ')"

echo "[1/6] Manifest 补丁（缓存复用；底包换了必须删缓存）"
mkdir -p "${PDIR}" "${OUTDIR}"
if [ ! -f "${MF}" ]; then
  TMPMF="${PDIR}/_scratch/AndroidManifest.xml"
  mkdir -p "${PDIR}/_scratch"
  "${PY}" - "${BASE_APK}" "${TMPMF}" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
open(sys.argv[2], 'wb').write(z.read('AndroidManifest.xml'))
print('  extracted AndroidManifest.xml %d B' % len(open(sys.argv[2],'rb').read()))
PY
  "${PY}" "${HERE}/patch_mxzz_binary_manifest.py" "${TMPMF}" "${MF}" || die "Manifest 补丁失败"
else
  echo "  复用 ${MF}（$(wc -c < "${MF}" | tr -d ' ') B）"
fi

echo "[1b/6] so 依赖闭包（DT_NEEDED）"
# llvm-readelf.exe 打不开含中文的路径（mod菜单/…）⇒ 走仓库既有 ASCII junction（starry_link）
SO_ASCII="${ROOT}/starry_link/libs_mxzz/${ABI}/libstarry_magic.so"
[ -f "${SO_ASCII}" ] || SO_ASCII="${SO}"
"${READELF}" -d "${SO_ASCII}" 2>/dev/null | grep NEEDED | sed -E 's/.*\[(.*)\]/  NEEDED \1/' || die "readelf 读不出 DT_NEEDED"
SYSTEM_LIBS="liblog.so libandroid.so libc.so libm.so libdl.so ld-android.so libstdc++.so"
NEED_BUNDLED=""
for dep in $("${READELF}" -d "${SO_ASCII}" 2>/dev/null | grep NEEDED | sed -E 's/.*\[(.*)\]/\1/'); do
  case " $SYSTEM_LIBS " in
    *" $dep "*) echo "  [系统提供] $dep" ;;
    *) echo "  [必须随包] $dep"; NEED_BUNDLED="$NEED_BUNDLED $dep" ;;
  esac
done
case "$NEED_BUNDLED" in
  *libc++_shared.so*) echo "  → 随包 libc++_shared.so：${LIBCXX}（$(wc -c < "${LIBCXX}" | tr -d ' ') B, sha256 $(sha256_of "${LIBCXX}")）" ;;
  *) echo "  → 无第三方依赖需要随包" ;;
esac

echo "[2/6] 外科 zip 组装"
WORK="$(mktemp -d)"
trap 'rm -rf "${WORK}"' EXIT
UNSIGNED="${WORK}/unsigned.apk"
MXZZ_NEED_BUNDLED="${NEED_BUNDLED}" "${PY}" - "${BASE_APK}" "${UNSIGNED}" "${MF}" "${DEX}" "${SO}" "${LIBCXX}" <<'PY' || die "zip 组装失败"
import hashlib, os, shutil, sys, zipfile
base, out, mf, dex, so, libcxx = sys.argv[1:7]
LIB_CXX_NAME = 'lib/arm64-v8a/libc++_shared.so'
libcxx_b = open(libcxx, 'rb').read()
DEX_NAME = 'classes6.dex'
SO_NAME = 'lib/arm64-v8a/libstarry_magic.so'
mf_b = open(mf, 'rb').read()
dex_b = open(dex, 'rb').read()
so_b = open(so, 'rb').read()

zin = zipfile.ZipFile(base, 'r')
zout = zipfile.ZipFile(out, 'w', allowZip64=True)
n_pass = n_repl = 0
so_done = False
for item in zin.infolist():
    name = item.filename
    if name.startswith('META-INF/'):
        continue
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    if name == 'AndroidManifest.xml':
        info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, mf_b); n_repl += 1; continue
    if name == SO_NAME:      # 底包若已带同名 so 则替换（本包实际没有，走下面的新增分支）
        info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, so_b); n_repl += 1; so_done = True; continue
    if name == DEX_NAME:
        raise SystemExit('底包已存在 %s，构建脚本需换 dex 名' % DEX_NAME)
    info.compress_type = item.compress_type
    with zin.open(item) as src, zout.open(info, 'w') as dst:
        shutil.copyfileobj(src, dst, 1 << 20)
    n_pass += 1
# libc++_shared.so：底包没有就新增（本包确实没有），有则替换成本 NDK 版本
cxx_done = False
if not cxx_done:
    info = zipfile.ZipInfo(LIB_CXX_NAME)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, libcxx_b)
    n_repl += 1

if not so_done:
    # 底包没有 lib/arm64-v8a/libstarry_magic.so ⇒ 新增（mod so 只对 arm64 出，底包 primaryCpuAbi=arm64-v8a）
    info = zipfile.ZipInfo(SO_NAME)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, so_b)
    n_repl += 1

# 新 dex（multidex 追加，不动既有 classes..classes5）
info = zipfile.ZipInfo(DEX_NAME)
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, dex_b)
n_repl += 1
zin.close(); zout.close()
print('  透传 %d 条，替换/新增 %d 条（Manifest + %s + so + libc++_shared.so）' % (n_pass, n_repl, DEX_NAME))
PY

echo "[3/6] 回读断言（未动条目必须逐字节一致）"
MXZZ_NEED_BUNDLED="${NEED_BUNDLED}" "${PY}" - "${BASE_APK}" "${UNSIGNED}" "${MF}" "${DEX}" "${SO}" "${LIBCXX}" <<'PY' || die "回读断言失败"
import hashlib, os, sys, zipfile
base, new, mf, dex, so, libcxx = sys.argv[1:7]
LIB_CXX_NAME = 'lib/arm64-v8a/libc++_shared.so'

def h(z, n):
    return hashlib.sha256(z.read(n)).hexdigest()

zo, zn = zipfile.ZipFile(base, 'r'), zipfile.ZipFile(new, 'r')
same = ['classes.dex', 'classes2.dex', 'classes3.dex', 'classes4.dex', 'classes5.dex', 'resources.arsc']
for n in same:
    a, b = h(zo, n), h(zn, n)
    assert a == b, '被改动: %s' % n
    print('  %-16s 逐字节一致' % n)
assert h(zn, 'AndroidManifest.xml') == hashlib.sha256(open(mf, 'rb').read()).hexdigest(), 'Manifest 未按预期替换'
assert h(zn, 'lib/arm64-v8a/libstarry_magic.so') == hashlib.sha256(open(so, 'rb').read()).hexdigest(), 'so 未按预期替换'
assert h(zn, 'classes6.dex') == hashlib.sha256(open(dex, 'rb').read()).hexdigest(), 'classes6.dex 不符'
assert h(zn, LIB_CXX_NAME) == hashlib.sha256(open(libcxx, 'rb').read()).hexdigest(), 'libc++_shared.so 不符'
print('  Manifest / classes6.dex / so 均按产物写入')
names = set(zn.namelist())
assert not any(n.startswith('META-INF/') for n in names), 'META-INF 未清干净'
assert 'classes7.dex' not in names, '意外新增 classes7.dex'
s = zn.read('lib/arm64-v8a/libstarry_magic.so')
assert b'[Mxzz]' in s and b'Toggle_\xe7\xa7\x92\xe6\x9d\x80' in s, 'so 不是 mxzz 产物'
d = zn.read('classes6.dex')
for need in (b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService'):
    assert need in d, need
m = zn.read('AndroidManifest.xml')
for need in ('com.starry.magic.StarryInitProvider', 'com.mxzz.zbml.starryinit',
             'com.starry.magic.menu.MenuService', 'com.unity3d.main.GameApplication'):
    assert need.encode('utf-16-le') in m, need
print('  入口/包名/服务/官方 Application 齐备；META-INF 已清；无多余 dex')

# ── 依赖闭包闭环：so 的每个非系统 DT_NEEDED 都必须在 lib/<abi>/ 里真实存在 ──
SYSTEM_LIBS = {'liblog.so','libandroid.so','libc.so','libm.so','libdl.so','ld-android.so','libstdc++.so'}
need = os.environ.get('MXZZ_NEED_BUNDLED','').split()
for dep in need:
    p = 'lib/arm64-v8a/' + dep
    assert p in names, '依赖缺失: %s（DT_NEEDED 要求随包）' % dep
    print('  依赖闭包 %-22s OK' % dep)
if 'libc++_shared.so' in need:
    got = hashlib.sha256(zn.read('lib/arm64-v8a/libc++_shared.so')).hexdigest()
    want = hashlib.sha256(open(libcxx,'rb').read()).hexdigest()
    assert got == want, 'libc++_shared.so 与 NDK 源不一致'
    print('  libc++_shared.so sha256=%s…（与 NDK 源逐字节一致）' % got[:16])
PY

echo "[4/6] zipalign -p 4"
ALIGNED="${WORK}/aligned.apk"
"${NIXIANG_ZIPALIGN}" -f -p 4 "${UNSIGNED}" "${ALIGNED}" || die "zipalign 失败"
rm -f "${UNSIGNED}"

echo "[5/6] apksigner 签名（星光自签 $(basename "${KS}")）"
rm -f "${OUT}" "${OUT}.idsig"
"${NIXIANG_APKSIGNER}" sign --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
  --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
  --min-sdk-version 23 \
  --out "${OUT}" "${ALIGNED}" || die "apksigner 失败"
rm -f "${ALIGNED}" "${OUTDIR}"/*.idsig 2>/dev/null || true

echo "[6/6] 签名验证 + 指纹"
"${NIXIANG_APKSIGNER}" verify --print-certs "${OUT}" 2>&1 | head -8
APK_SHA="$(sha256_of "${OUT}")"
APK_MD5="$("${PY}" -c "import hashlib,sys;print(hashlib.md5(open(sys.argv[1],'rb').read()).hexdigest())" "${OUT}")"
SO_SHA="$(sha256_of "${SO}")"
DEX_SHA="$(sha256_of "${DEX}")"
MF_SHA="$(sha256_of "${MF}")"
echo "------------------------------------------------------------"
echo "APK    : ${OUT}"
echo "size   : $(wc -c < "${OUT}" | tr -d ' ') B"
echo "sha256 : ${APK_SHA}"
echo "md5    : ${APK_MD5}"
echo "so     : ${SO_SHA}"
echo "dex    : ${DEX_SHA}"
echo "manif. : ${MF_SHA}"
echo "------------------------------------------------------------"
