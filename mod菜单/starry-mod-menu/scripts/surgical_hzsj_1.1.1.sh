#!/usr/bin/env bash
# ================================================================
#  魂坠深境 (com.chillyroom.containerofsoul.yw) — 星光外科组装
#  （新菜单 provider-only 路线，照 surgical_zzz_3.2.0.sh）；
#  **同一脚本管三代底包：VERSION=1.1.1（旧星光底包）/ 1.1.2 / 1.1.3（官方 TapTap 原包）**
#
#  背景：旧包（f191e461，2026-09-10 gradle+smali 注入路线）把菜单 smali 混进了
#  classes5.dex，靠 Activity onCreate 插桩拉起。新路线改为「新菜单 dex + Manifest
#  注册 StarryInitProvider」——菜单/授权/签名回灌/TapTap 强制网页登录都由 Java 侧
#  Provider 内化（StarryInitProvider.java），不再需要逐游戏 smali 补丁。
#
#  底包：02_apks/魂坠深境/魂坠深境_${VERSION}_device{,_official}.apk
#        （1.1.1 = 旧星光底包 md5 f191e461…；1.1.2 = 用户真机官方 TapTap 原包）
#  手术（其余 2460+ 条目字节保真）：
#   0) 【仅 VERSION=1.1.1】classes5.dex 重建为「去 com/starry」（baksmali→删→smali；
#      旧星光底包把 127 个旧菜单类混在此 dex）→ 缓存复用
#      【VERSION=1.1.2 跳过】：官方包全 4 dex 内 `com/starry` 命中 0，无需清理
#   1) 新菜单 dex 新增（templates/hzsj_menu_current，hzsj_build_dex.sh 产）；
#      名字 = 包内最大 classesN.dex 的下一位（1.1.1 ⇒ classes6.dex，1.1.2 ⇒ classes5.dex）
#   2) AndroidManifest.xml 加 <provider com.starry.magic.StarryInitProvider>
#      + <service MenuService>(foregroundServiceType=specialUse) + 两条权限
#      **+ appComponentFactory: androidx.core.app.CoreComponentFactory
#        → com.starry.magic.sign.StarryAppComponentFactory**（application@name 零改动）
#      【2026-10-29「双轨制」】Factory 名从已删除的 SignSpoofAppComponentFactory 换成本类；
#      本类不再装签名伪装，只在 instantiateApplication 里 Class.forName 触发随包注入的
#      killer dex（B 轨运行时签名替换），登录侧由 classes3.dex 的 A 轨静态改包负责。
#      历史（2026-10-13）：签名伪装原本只在 Provider.onCreate 装，晚于 Application 实例化
#      与 TapSDK 冷启动读签名（TapSignKit.getSign → pm.getPackageInfo(pkg,0x40)），
#      用户实测报「TapTap 签名校验没处理」⇒ 改由 AppComponentFactory.instantiateApplication
#      在 Application 实例化前装（本仓库通用做法）。
#      ★服务在前台跑需要 SYSTEM_ALERT_WINDOW（悬浮窗）+ FOREGROUND_SERVICE_SPECIAL_USE，
#        旧星光底包已有，官方 1.1.2+ 原包**没有**，由本步补
#   3) lib/arm64-v8a/libstarry_magic.so 替换（libs_hzsj 产物，含设备无关指针判据）
#   4) libc++_shared.so：新 so 的 DT_NEEDED 含 libc++_shared.so；旧星光底包自带，
#      官方 1.1.2 原包**不带** ⇒ 底包缺则注入 NDK 构建产物同源副本（缺它 = 装载失败）
#
#  ★旧包 Activity 插桩（onCreate 调 MenuService.start 等）在 1.1.1 底包中保留不动：
#    classes5 的旧 com/starry 删除后引用自动解析到新菜单 dex；MenuService.start
#    幂等（instance!=null 即 skip），与 Provider 双启动无副作用（已核 11-07）。
#
#  签名：01_tools/starry_release.jks（CN=StarryMagic，alias starry）——禁换（卡密指纹）
#
#  用法:
#    bash scripts/surgical_hzsj_1.1.1.sh                    # 出包（默认 1.1.2 官方底包）
#    HZSJ_VERSION=1.1.1 bash scripts/surgical_hzsj_1.1.1.sh  # 出旧代包
#    STARRY_INSTALL=1 SERIAL=f9a116a2 bash ...              # 出包+装机（同签 -r）
# ================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "${SCRIPT_DIR}/.." && pwd)"
# shellcheck source=/dev/null
source "${STARRY_SRC}/../../04_scripts/_common/env.sh"

NIXIANG="${NIXIANG_ROOT}"
GAME="魂坠深境"
VERSION="${HZSJ_VERSION:-1.1.3}"
PKG="com.chillyroom.containerofsoul.yw"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-127.0.0.1:16416}"

# --- 代数相关（1.1.1 = 旧星光底包 / 1.1.2 = 官方 TapTap 原包）-----------------
case "${VERSION}" in
  1.1.1)
    BASE_DEFAULT="${NIXIANG}/02_apks/${GAME}/${GAME}_1.1.1_device.apk"
    BASE_MD5_EXPECT="${HZSJ_BASE_MD5:-f191e461b4d83ef9d079d693e94cca35}"
    DEX_ADD_NAME="${HZSJ_DEX_NAME:-classes6.dex}"     # 星光底包已是 dex…dex5
    LEGACY_MENU_DEX="1"                               # 1 = 需重建 classes5 去 com/starry
    ;;
  1.1.2)
    BASE_DEFAULT="${NIXIANG}/02_apks/${GAME}/${GAME}_1.1.2_device_official.apk"
    BASE_MD5_EXPECT="${HZSJ_BASE_MD5:-4890e8053f0df45c8c321b2cdc04e65e}"
    DEX_ADD_NAME="${HZSJ_DEX_NAME:-classes5.dex}"     # 官方原包只到 dex4
    LEGACY_MENU_DEX=""                                # 官方包无 com/starry，免清理
    ;;
  1.1.3)
    # 2026-10-28 官方 TapTap 1.1.3 / vc44（真机 f9a116a2 只读拉取）。
    # 结构与 1.1.2 同型：4 个 dex（无 classes5）、com/starry 命中 0、无 libc++_shared.so、
    # metadata v31（名字串加密，只能运行时按名解析）、manifest appComponentFactory 仍为
    # androidx.core.app.CoreComponentFactory、extractNativeLibs=true ⇒ 组装路径零变化。
    BASE_DEFAULT="${NIXIANG}/02_apks/${GAME}/${GAME}_1.1.3_device_official.apk"
    BASE_MD5_EXPECT="${HZSJ_BASE_MD5:-0e4a80ea14e1c85fb3b09c03a421c46e}"
    DEX_ADD_NAME="${HZSJ_DEX_NAME:-classes5.dex}"     # 官方原包只到 dex4
    LEGACY_MENU_DEX=""                                # 官方包无 com/starry，免清理
    ;;
  *) echo "ERROR: 未知 HZSJ_VERSION=${VERSION}（只支持 1.1.1 / 1.1.2 / 1.1.3）" >&2; exit 1 ;;
esac

BASE_APK="${HZSJ_BASE_APK:-${BASE_DEFAULT}}"

# ============================================================================
#  2026-10-29「双轨制」新增件（用户口令「用新方法改」，口径见
#  10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md 的 2026-10-29 增补节）
#   B 轨（签名/完整性校验）= 运行时签名替换：注入 killer dex（MT ApkSignatureKillerEx 形态，
#      <clinit> 里换 PackageInfo.CREATOR + 清 PackageManager.sPackageInfoCache /
#      Parcel.mCreators / Parcel.sPairedCreators），由 StarryAppComponentFactory
#      .instantiateApplication 在 Application 实例化前 Class.forName 触发 ⇒ 官方 dex 零改动。
#   A 轨（登录方式/渠道分支）= 静态改包：TapSDK 客户端 SSO 的 action 常量改成不匹配值
#      ⇒ 唤起 TapTap 客户端的 intent 恒 resolve 不到，登录自动落内嵌网页(H5)。
#  两条轨**同一链条禁止双修**：不得再引入 SignSpoof / TapLoginForceWeb 白名单
#  （后者已把本包移出 PKG_WHITELIST）。
# ============================================================================
KILLER_SMALI_DIR="${NIXIANG}/07_patched/${GAME}/surgical/killer_smali"
DEX_KILLER_NAME="${HZSJ_KILLER_DEX_NAME:-classes6.dex}"
KILLER_CLASS="bin.mt.signature.KillerApplication193"
KILLER_CERT_MD5="a3e3c07b81aa322215f6315d7e60b151"   # 官方证书 DER（CN=O=Chillyroom）
LOGIN_DEX_ENTRY="classes3.dex"                        # TapSDK 登录实现在 classes3.dex
LOGIN_STR_OLD="com.taptap.sdk.action"                 # 21 字符：等长就地替换
LOGIN_STR_NEW="com.taptap.sdk.act1on"                 # 21 字符：客户端 SSO 恒不匹配

SO_SRC="${STARRY_SRC}/libs_hzsj/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STARRY_SRC}/libs_hzsj/arm64-v8a/libc++_shared.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-newmenu-releaseSigned.apk"

# 一次性手术缓存（dex5 去菜单 / 二进制 Manifest）；缓存按"是否做 dex5 清理"分桶
CACHE_DIR="${NIXIANG}/07_patched/${GAME}/surgical${LEGACY_MENU_DEX:+_1.1.1}"
CACHE_CLS5="${CACHE_DIR}/classes5_nostarry.dex"
# 二进制 Manifest 缓存**按底包版本分桶**（2026-10-28 修）：原为共用一个 AndroidManifest.xml，
# 1.1.3 复用 1.1.2 的缓存会静默丢掉新版新增组件（实测 1.1.3 新增
# com.chillyroom.sdk.wrapper.wxapi.WXEntryActivity，缓存里没有 ⇒ 覆盖安装即缺该组件）。
CACHE_MF="${CACHE_DIR}/AndroidManifest_${VERSION}.xml"


# 菜单模板新鲜度门禁（2026-10-26 起）：模板必须与当前 Java 源码 HEAD 同步，
# 否则复制的就是旧菜单 dex（无限升级 vc1637 / 镇邪人 7f94233e 同一根因）。
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "${STARRY_SRC}/templates/hzsj_menu_current" || {
  echo "ERROR: 菜单模板过期 ⇒ 先重跑: bash 04_scripts/魂坠深境/hzsj_build_dex.sh" >&2
  exit 1
}

DEX6_SRC="${STARRY_SRC}/templates/hzsj_menu_current/${DEX_ADD_NAME}"

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

[[ -f "${BASE_APK}" ]] || die "缺底包 ${BASE_APK}（HZSJ_BASE_APK=... 覆盖）"
[[ -f "${DEX6_SRC}" ]] || die "缺 ${DEX6_SRC}（先跑 04_scripts/魂坠深境/hzsj_build_dex.sh）"
[[ -f "${SO_SRC}" ]]   || die "缺 ${SO_SRC}（先跑 04_scripts/魂坠深境/hzsj_build_so.sh）"
[[ -f "${LIBCXX_SRC}" ]] || die "缺 ${LIBCXX_SRC}（NDK 构建顺产出，被删就跑 hzsj_build_so.sh）"
[[ -f "${JAVA}" ]]     || die "缺 java: ${JAVA}"
[[ -f "${APKTOOL_JAR}" ]] || die "缺 apktool.jar: ${APKTOOL_JAR}"
[[ -f "${BAKSMALI_JAR}" && -f "${SMALI_JAR}" ]] || die "缺 smali 工具（01_tools/smali：baksmali/smali jar）"
[[ -x "${ZIPALIGN}" ]] || die "缺 zipalign: ${ZIPALIGN}"
[[ -x "${APKSIGNER}" || -f "${APKSIGNER}" ]] || die "缺 apksigner: ${APKSIGNER}"
[[ -f "${AAPT2}" ]] || die "缺 aapt2: ${AAPT2}（build-tools）"

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

echo "[1/8] 输入检查"
bsize="$(wc -c <"${BASE_APK}" | tr -d ' ')"
bmd5="$(md5f "${BASE_APK}")"
[[ "${bmd5}" == "${BASE_MD5_EXPECT}" ]] || die "底包指纹不符 ${bmd5}（期望 ${BASE_MD5_EXPECT}）"
for s in 'com/starry/magic/StarryInitProvider' 'com/starry/magic/menu/MenuService' \
         'com/starry/magic/sign/StarryAppComponentFactory' 'bin.mt.signature.KillerApplication193'; do
  c="$(LC_ALL=C grep -ac "$s" "${DEX6_SRC}" || true)"
  [[ "${c}" -ge 1 ]] || die "${DEX_ADD_NAME} 缺 ${s}"
done
[[ -d "${KILLER_SMALI_DIR}/bin/mt/signature" ]] \
  || die "缺 killer smali 源 ${KILLER_SMALI_DIR}（07_patched/魂坠深境/surgical/killer_smali）"
for s in 'Toggle_秒杀' 'Toggle_自动胜利' '[Hzsj]'; do
  LC_ALL=C grep -aq "$s" "${SO_SRC}" || die "so 缺 ${s}"
done
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

echo "[2/8] 旧菜单清理（仅 VERSION=1.1.1 底包需要：classes5.dex 去 com/starry）"
if [[ -z "${LEGACY_MENU_DEX}" ]]; then
  # 官方 1.1.2 原包：4 个 dex 内 com/starry 命中 0（已核，见 10_docs/魂坠深境/重构_1.1.2_*.md）
  # ⇒ 无旧菜单可清；同时确认底包确实没有 classes5.dex（否则 DEX_ADD_NAME 会撞名）
  "${PY}" - "${BASE_APK}" <<'PY' || die "官方底包含 classes5.dex（dex 序号冲突，改 HZSJ_DEX_NAME）"
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

echo "[3/8] B 轨：killer dex（${DEX_KILLER_NAME}）—运行时签名替换"
cp -R "${KILLER_SMALI_DIR}" "${WORK}/killer_smali" || die "复制 killer smali 失败"
smali a -o "${WORKW}/${DEX_KILLER_NAME}" "${WORKW}/killer_smali" >"${WORK}/smali_killer.log" 2>&1 \
  || { tail -20 "${WORK}/smali_killer.log" >&2; die "killer smali 汇编失败"; }
[[ -f "${WORK}/${DEX_KILLER_NAME}" ]] || die "killer smali 未产出 dex"
"${PY}" - "${WORK}/${DEX_KILLER_NAME}" "${KILLER_CLASS//\//.}" "${PKG}" "${KILLER_CERT_MD5}" <<'PY' \
  || die "killer dex 回读断言不过"
import base64, hashlib, re, sys
p, cls, pkg, cert_md5 = sys.argv[1:5]
d = open(p, 'rb').read()
cls_desc = ('L%s;' % cls.replace('.', '/')).encode()
assert cls_desc in d, 'killer dex 缺 %s' % cls_desc
assert pkg.encode() in d, 'killer dex 缺包名 %s' % pkg
assert b'com/wanda' not in d, 'killer dex 仍引用 sina/壳类（父类未改干净）'
assert b'Landroid/app/Application;' in d, 'killer 父类不是 android.app.Application'
# 证书：从 dex 里取出折行 base64（含 \n）解码回 DER 并核 md5
i = d.find(b'MIIC')
assert i > 0, 'killer dex 里找不到证书 base64'
j = i
while j < len(d) and (chr(d[j]).isalnum() or d[j] in b'+/=\n'):
    j += 1
b64 = re.sub(rb'[^A-Za-z0-9+/=]', b'', d[i:j])
der = base64.b64decode(b64)
got = hashlib.md5(der).hexdigest()
assert got == cert_md5, 'killer 证书 DER md5=%s（期望 %s）' % (got, cert_md5)
print('  killer dex ok: %s / pkg=%s / 官方证书 DER %d B md5=%s'
      % (cls, pkg, len(der), got))
PY

echo "[4/8] A 轨：${LOGIN_DEX_ENTRY} 登录改包（TapSDK client-SSO action 等长就地替换）"
"${PY}" - "${BASE_APK}" "${LOGIN_DEX_ENTRY}" "${WORKW}/login_orig.dex" "${WORKW}/login_patched.dex" \
  "${LOGIN_STR_OLD}" "${LOGIN_STR_NEW}" <<'PY' || die "登录改包失败"
import hashlib, sys, zipfile, zlib
apk, entry, orig_out, patched_out, old, new = sys.argv[1:7]
old_b, new_b = old.encode(), new.encode()
assert len(old_b) == len(new_b), '替换必须等长（%d vs %d）' % (len(old_b), len(new_b))
d = bytearray(zipfile.ZipFile(apk).read(entry))
open(orig_out, 'wb').write(d)
assert d[:8] == b'dex\n035\x00' or d[:4] == b'dex\n', '不是 dex 文件'
n_old = d.count(old_b)
if n_old != 1:
    sys.exit('%s 里 "%s" 命中 %d 次（期望 1，锚点漂移）' % (entry, old, n_old))
if d.count(new_b):
    sys.exit('%s 里已存在 "%s"（重复打补丁？）' % (entry, new))
d = d.replace(old_b, new_b, 1)
# dex 头两个摘要字段按规范重算：signature = SHA-1(bytes[32:])，checksum = adler32(bytes[12:])
d[12:32] = hashlib.sha1(bytes(d[32:])).digest()
d[8:12] = zlib.adler32(bytes(d[12:])).to_bytes(4, 'little')
open(patched_out, 'wb').write(d)
# 回读断言
v = open(patched_out, 'rb').read()
assert len(v) == len(zipfile.ZipFile(apk).read(entry)), 'dex 尺寸变化（等长替换失败）'
assert v.count(new_b) == 1 and v.count(old_b) == 0, '回读断言失败'
assert v[12:32] == hashlib.sha1(v[32:]).digest(), 'dex signature 未更新'
assert int.from_bytes(v[8:12], 'little') == zlib.adler32(v[12:]), 'dex checksum 未更新'
print('  %s: "%s" → "%s"（等长 %d B 就地替换，dex %d B，头部摘要已重算）'
      % (entry, old, new, len(old_b), len(v)))
PY


echo "[5/8] 二进制 Manifest（+StarryInitProvider；appComponentFactory → StarryAppComponentFactory）"
if [[ ! -f "${CACHE_MF}" ]]; then
  apktool d -f -s --no-assets -o "${WORK}/mf_dec" "${BASE_APK}" >"${WORK}/apktool_d.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_d.log" >&2; die "apktool d 失败"; }
  "${PY}" - "${WORK}/mf_dec/AndroidManifest.xml" "${AUTHORITY}" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
t = p.read_text(encoding='utf-8')
OLD_FACTORY = 'androidx.core.app.CoreComponentFactory'
NEW_FACTORY = 'com.starry.magic.sign.StarryAppComponentFactory'
if NEW_FACTORY in t:
    print('  manifest 已是星光 Factory，跳过替换')
elif 'android:appComponentFactory="%s"' % OLD_FACTORY in t:
    t = t.replace('android:appComponentFactory="%s"' % OLD_FACTORY,
                  'android:appComponentFactory="%s"' % NEW_FACTORY, 1)
else:
    raise SystemExit('manifest assertion failed: 既无 %s 也无 %s' % (OLD_FACTORY, NEW_FACTORY))
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
print('  manifest patched (provider+service+perms+Starry Factory)，application@name 零改动')
PY
  rm -rf "${WORK}/mf_dec"/smali* 2>/dev/null || true
  apktool b -f "${WORK}/mf_dec" -o "${WORK}/mf_only.apk" >"${WORK}/apktool_b.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_b.log" >&2; die "apktool b 失败"; }
  unzip -p "${WORK}/mf_only.apk" AndroidManifest.xml > "${CACHE_MF}" || die "提取二进制 manifest 失败"
  echo "  缓存 ${CACHE_MF} ($(wc -c <"${CACHE_MF}" | tr -d ' ') B)"
else
  echo "  缓存命中 ${CACHE_MF}（若为 2026-10-29 前的旧缓存，[6/8] 的 Factory 断言会失败；删缓存重跑即可）"
fi

echo "[6/8] 二进制 Manifest 回读校验（aapt2）"
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
  "com.starry.magic.sign.StarryAppComponentFactory" \
  "android.permission.SYSTEM_ALERT_WINDOW" \
  "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"; do
  grep -q "$need" "${WORK}/tree_new.txt" || die "新 manifest 缺 $need"
done
echo "  manifest ok（provider/service/Factory/权限均在）"

echo "[7/8] surgical zip merge（manifest + ${DEX_ADD_NAME}/${DEX_KILLER_NAME} 新增 + ${LOGIN_DEX_ENTRY} 登录改包 + so${LEGACY_MENU_DEX:+ + dex5 重建}）"
"${PY}" - "${BASE_APK}" "${CACHE_CLS5}" "${CACHE_MF}" "${DEX6_SRC}" "${DEX_ADD_NAME}" "${SO_SRC}" "${LIBCXX_SRC}" "${MERGED}" \
  "${WORKW}/${DEX_KILLER_NAME}" "${DEX_KILLER_NAME}" "${WORKW}/login_patched.dex" "${LOGIN_DEX_ENTRY}" "${LEGACY_MENU_DEX}" <<'PY'
import os, sys, zipfile

orig, cls5, mf, dex_src, dex_name, so, libcxx, out, \
    killer_dex, killer_name, login_dex, login_entry, legacy_flag = sys.argv[1:14]
# LEGACY 只能由 VERSION 分支决定（LEGACY_MENU_DEX 非空）。
# 2026-10-29 修：原写法用「CACHE_CLS5 文件是否存在」推断，Windows 树里 1.1.1 时代遗留的
# classes5_nostarry.dex 会让 1.1.2/1.1.3 误判成 LEGACY ⇒ 「底包缺条目 classes5.dex」。
LEGACY = bool(legacy_flag)

REPL = {
    'AndroidManifest.xml': mf,
    login_entry: login_dex,          # A 轨：TapSDK client-SSO action 等长替换后的 classes3.dex
}
ADD = {dex_name: dex_src, killer_name: killer_dex}   # 菜单 dex + B 轨 killer dex
if LEGACY:
    REPL['classes5.dex'] = cls5

def is_sig(name):
    u = name.upper()
    if u == 'META-INF/MANIFEST.MF':
        return True
    return name.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

zin = zipfile.ZipFile(orig, 'r')
names = set(zin.namelist())
for nm in (dex_name, killer_name):
    if nm in names:
        sys.exit('底包已含 %s（重复注入嫌疑）—— 请核对底包或改名' % nm)

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

echo "[8/8] zipalign + apksigner + 校验"
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

"${PY}" - "${FINAL}" "${SO_SRC}" "${LIBCXX_SRC}" "${CACHE_CLS5}" "${DEX_ADD_NAME}" "${AAPT2}" \
  "${DEX_KILLER_NAME}" "${LOGIN_DEX_ENTRY}" "${LOGIN_STR_OLD}" "${LOGIN_STR_NEW}" "${KILLER_CERT_MD5}" "${LEGACY_MENU_DEX}" <<'PY'
import base64, sys, zipfile, hashlib, os, re, subprocess
apk, so, libcxx, cls5, dex_name, aapt2, killer_name, login_entry, login_old, login_new, cert_md5, legacy_flag = sys.argv[1:13]
legacy = bool(legacy_flag)   # 2026-10-29 修：同样改为按 VERSION 分支判定，不看缓存文件是否存在
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

# ---- 2026-10-29 双轨制：B 轨 killer dex 包内回读 ----
names = set(z.namelist())
assert killer_name in names, '%s 未进包（B 轨缺失）' % killer_name
kd = z.read(killer_name)
assert b'bin/mt/signature/KillerApplication193' in kd, '%s 缺 KillerApplication193' % killer_name
assert b'bin.mt.signature.KillerApplication193' in d, '%s 未引用 killer（挂载点缺失）' % dex_name
assert b'SignSpoof' not in d, '%s 仍含 SignSpoof（旧法残留）' % dex_name
i = kd.find(b'MIIC')
assert i > 0, '%s 缺官方证书 base64' % killer_name
j = i
while j < len(kd) and (chr(kd[j]).isalnum() or kd[j] in b'+/=\n'):
    j += 1
der = base64.b64decode(re.sub(rb'[^A-Za-z0-9+/=]', b'', kd[i:j]))
assert hashlib.md5(der).hexdigest() == cert_md5, '%s 证书 DER md5 不符' % killer_name
msg += ' / B 轨 killer=%s（官方证书 %d B md5=%s）' % (killer_name, len(der), cert_md5)

# ---- A 轨：classes3.dex 登录改包回读 ----
ld = z.read(login_entry)
old_b, new_b = login_old.encode(), login_new.encode()
assert ld.count(old_b) == 0, '%s 仍含 "%s"（A 轨未生效）' % (login_entry, login_old)
assert ld.count(new_b) == 1, '%s 未含 "%s"（A 轨未生效）' % (login_entry, login_new)
msg += ' / A 轨 %s "%s"→"%s"' % (login_entry, login_old, login_new)

# appComponentFactory 改写是 2026-10-13「TapTap 签名校验时机」修复的核心；最终包内回读确认。
# 注：二进制 AXML 的字符串池是 UTF-16，不能按 bytes 搜 ASCII —— 用 aapt2 dump 成文本再断言。
os.makedirs('/tmp/hzsj_mfchk', exist_ok=True)
mfx = '/tmp/hzsj_mfchk/mf.zip'
with zipfile.ZipFile(mfx, 'w', zipfile.ZIP_DEFLATED) as wz:
    wi = zipfile.ZipInfo('AndroidManifest.xml')
    wi.compress_type = zipfile.ZIP_DEFLATED
    wz.writestr(wi, z.read('AndroidManifest.xml'))
# aapt2 dump xmltree 只吃 zip/apk（不给裸 axml，会 rc=1）
r = subprocess.run([aapt2, 'dump', 'xmltree', '--file', 'AndroidManifest.xml', mfx],
                   capture_output=True, text=True)
assert r.returncode == 0, 'aapt2 dump 失败: %s' % r.stderr[-300:]
tree = r.stdout
assert 'com.starry.magic.sign.StarryAppComponentFactory' in tree, \
    'manifest 未换 StarryAppComponentFactory（killer 挂载点缺失）'
assert 'com.starry.magic.sign.SignSpoofAppComponentFactory' not in tree, \
    'manifest 仍挂已删的 SignSpoofAppComponentFactory（pre_install_gate 查12 会 FAIL）'
assert 'com.starry.magic.StarryInitProvider' in tree, 'manifest 缺 StarryInitProvider'
msg += ' / manifest Factory=StarryAppFactory'
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