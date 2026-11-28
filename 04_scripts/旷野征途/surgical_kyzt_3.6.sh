#!/usr/bin/env bash
# ================================================================
#  旷野征途 (com.xf.kyzt) 3.6 / vc36 — 星光外科组装（只构建，不含装机）
#
#  底包：02_apks/旷野征途/旷野征途_3.6_taptap_official.apk
#        sha256 cdf2ac75…2401 / 177,151,913 B / 2 dex / 8 so / zip 条目 1197
#        （实测指纹；官方 dex 只有 classes.dex + classes2.dex）
#
#  母本：surgical_kyzt_3.3.sh。骨架、门禁次序、日志格式、签名调用（apksigner +
#        01_tools/starry_release.jks alias=starry）原样保留；只改版本常量、dex 布局、
#        两 dex 替换逻辑、四锚点断言与 manifest 的 appComponentFactory 分支。
#
#  3.6 与 3.3 的差异（逐条）：
#   · 官方 dex 集合 = {classes.dex, classes2.dex}（3.3 是 7 dex）。改包法**两个都要换**：
#     KYZT_CLASSES_DEX  -> classes.dex  （门闩 showErrorToast / TapSignKit.getSign）
#     KYZT_CLASSES2_DEX -> classes2.dex （provide 强制 Web / getVerifiedVersionCode）
#     两个都是必填，缺任一即 die。3.3 的 KYZT_CLASSES6_DEX / KYZT_CLASSES6_SHA256_EXPECT
#     在 3.6 不适用，已删除（不留半死代码）。
#   · 框架 dex = classes3.dex（3.3 是 classes8.dex）；killer dex = classes4.dex
#     （3.3 是 classes9.dex，且可选）。ART 遇首个缺口即停扫 ⇒ 序号必须连续 1..4；
#     killer 与框架、与官方 classes.dex/classes2.dex 都不许撞号。
#   · 四锚点断言改为覆盖两个 dex，判据落在 dex 字节码上（python 解析 dex 的
#     string/type/proto/method/class_data/code_item，不靠 smali 文本 grep；任一不符即 die）：
#       classes.dex  GatekeeperRepository.showErrorToast 只剩一条 return-void；
#                    TapSignKit.getSign 只剩 const-string 360c78f5… + return-object；
#       classes2.dex DefaultTapLoginFragmentProvider.provide 唯一 const/4 v2, 0x3
#                    （紧跟 aget ⇒ 恒走 WebLoginFragment 分支）；
#                    PrivacyActivity.getVerifiedVersionCode = const-wide/16 0x24 + return-wide。
#   · manifest 只加三样：StarryInitProvider（authorities=com.xf.kyzt.starryinit、initOrder=100）、
#     MenuService（foregroundServiceType=specialUse + PROPERTY_SPECIAL_USE_FGS_SUBTYPE）、
#     3 个权限。**不注入/不替换 appComponentFactory**（3.3 的注入分支已删）、
#     **不加 application android:name**（底包 <application> 若带 android:name 直接 die）。
#   · 输入指纹默认全部钉死（底包 / 两个改包 dex / 框架 dex / killer），可用同名
#     KYZT_*_SHA256_EXPECT 显式覆盖（覆盖会打印告警）。
#   · 默认**只构建**；STARRY_INSTALL=1 才提示 install（默认关；本脚本不调 adb）。
#
#  用法：
#    KYZT_CLASSES_DEX=13_scratch/kyzt36/out_classes.dex \
#    KYZT_CLASSES2_DEX=13_scratch/kyzt36/out_classes2.dex \
#      bash 04_scripts/旷野征途/surgical_kyzt_3.6.sh
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "${HERE}/../.." && pwd)"
# shellcheck source=/dev/null
. "${NIXIANG}/04_scripts/_common/env.sh"

STAR="${NIXIANG}/mod菜单/starry-mod-menu"
GAME="旷野征途"
VERSION="3.6"
PKG="com.xf.kyzt"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-96f0d883}"

BASE_APK="${KYZT_BASE_APK:-${NIXIANG}/02_apks/旷野征途/旷野征途_${VERSION}_taptap_official.apk}"
# 默认钉官方底包；仅在「外层自己生成并把关过中间产物」时用 KYZT_BASE_SHA256_EXPECT 覆盖（会打印告警）
BASE_SHA256_EXPECT="${KYZT_BASE_SHA256_EXPECT:-cdf2ac75a724f33543ffa736c01874d0d9fd4d5dd344ce41761cb6e2c7052401}"

# 3.6 dex 布局（ART 遇首个缺口即停扫 ⇒ 1..4 连续）
DEX_ADD_NAME="classes3.dex"                       # 框架
KILLER_DEX_NAME="classes4.dex"                    # killer（必须紧跟框架）
DEX_SRC="${KYZT_FRAMEWORK_DEX:-${NIXIANG}/13_scratch/kyzt36/framework/classes3.dex}"
KYZT_FRAMEWORK_SHA256_EXPECT="${KYZT_FRAMEWORK_SHA256_EXPECT:-dfb6b93232f758e4e5752d70cfad035ede67e10fa0c10307418b50c1756f0a51}"

# 必填输入（3.6 改包法：两个官方 dex 都被改过 ⇒ 两个都必须给）
#   KYZT_CLASSES_DEX  = 官方 classes.dex  改包产物（门闩 / getSign）
#   KYZT_CLASSES2_DEX = 官方 classes2.dex 改包产物（登录入口 / getVerifiedVersionCode）
KYZT_CLASSES_DEX="${KYZT_CLASSES_DEX:-}"
KYZT_CLASSES2_DEX="${KYZT_CLASSES2_DEX:-}"
KYZT_CLASSES_SHA256_EXPECT="${KYZT_CLASSES_SHA256_EXPECT:-f2474b73acbfdd814348cdd02437abbe316bb95664215059673827acdde91270}"
KYZT_CLASSES2_SHA256_EXPECT="${KYZT_CLASSES2_SHA256_EXPECT:-bceaabfc76ae475d909ae20f9bb1b0e373c7f22002bd3486fd0b5bd1b8504e21}"

# killer dex（C# 侧 ET.AndroidPackageIntegrity 反篡改比对 base64(SHA256(DER)) ⇒
#   必须让 Java 侧返回官方 DER，装机版必需；3.6 默认取已就绪交付件）
KYZT_KILLER_DEX="${KYZT_KILLER_DEX:-${NIXIANG}/13_scratch/kyzt36/killer/kyzt_signature_killer.dex}"
KYZT_KILLER_SHA256_EXPECT="${KYZT_KILLER_SHA256_EXPECT:-35ede19c2884b1e9f8919455bffc265f272966cfd2e2891c61aab64e060cbe5d}"

# 框架 so：3.6 交付件（与 app/src/libs_kyzt 同名文件同一 sha256）；libc++_shared 沿用 3.3 那份
SO_SRC="${KYZT_SO:-${STAR}/app/src/libs_kyzt/arm64-v8a/libstarry_magic.so}"
[ -f "${SO_SRC}" ] || SO_SRC="${STAR}/app/src/libs_kyzt/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STAR}/app/src/libs_kyzt/arm64-v8a/libc++_shared.so"
[ -f "${LIBCXX_SRC}" ] || LIBCXX_SRC="${STAR}/libs_kyzt/arm64-v8a/libc++_shared.so"
# 第三版 so 钉子（2026-11-28）：244,968 B / 含恒通 detour VerifyPackageContents+VerifyCurrentSigner，
#   且 FileOffToVa 加「无 x 段退回只读段」分支（雷电 libhoudini 下 arm64 guest 库整库无 x 段）。
# 前两版 so 是 242,504 B / 85ffa5d5…、244,968 B / 8c05de57…，重跑时若 SO_SRC 还指旧文件必须 die，不许静默装错。
SO_SHA256_EXPECT="${KYZT_SO_SHA256_EXPECT:-e41bce780acf07b70a2126a22051ba376597feb10ecf33cb81333bfae3f74c5b}"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-unsigned.apk"
SIGNED="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"
WORK="${NIXIANG}/07_patched/${GAME}/surgical_3.6"
CACHE_MF="${WORK}/AndroidManifest.xml"
# apktool b 的 aapt2 在 Win/GBK 控制台下打不开含中文的路径 ⇒ 解码目录必须落 ASCII 路径
MF_DEC="${NIXIANG}/07_patched/kyzt36_mf_dec"

PY="${NIXIANG_PYTHON:-python3}"
JAVA="${NIXIANG_JAVA17}/bin/java"
APKTOOL_JAR="${NIXIANG_ROOT:-${NIXIANG}}/01_tools/apktool/apktool.jar"
[ -f "${APKTOOL_JAR}" ] || APKTOOL_JAR="${NIXIANG}/01_tools/apktool/apktool_2.9.3.jar"
[ -f "${APKTOOL_JAR}" ] || APKTOOL_JAR="$(command -v apktool >/dev/null 2>&1 && echo '')"
AXML="${NIXIANG}/04_scripts/_common/axml_inspect.py"

die() { echo "ERROR: $*" >&2; exit 1; }
sha_of() {
  # 双机：mac 只有 shasum，Win Git Bash 只有 sha256sum
  if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}';
  else shasum -a 256 "$1" | awk '{print $1}'; fi
}
md5_of() {
  if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}';
  else md5 -q "$1"; fi
}

# ---------- 输入核对 ----------
[ -f "${BASE_APK}" ] || die "底包不存在: ${BASE_APK}"
BSHA="$(sha_of "${BASE_APK}")"
[ "${BSHA}" = "${BASE_SHA256_EXPECT}" ] || die "底包 sha256 不符（期望 ${BASE_SHA256_EXPECT}，实得 ${BSHA}）"
if [ -n "${KYZT_BASE_SHA256_EXPECT:-}" ]; then
  echo "      ⚠ 底包指纹由 KYZT_BASE_SHA256_EXPECT 覆盖（非官方原包）"
fi

[ "${DEX_ADD_NAME}" = "classes3.dex" ] || die "框架 dex 名必须 classes3.dex（3.6 官方只有 2 dex）：${DEX_ADD_NAME}"
[ "${KILLER_DEX_NAME}" = "classes4.dex" ] || die "killer dex 名必须 classes4.dex：${KILLER_DEX_NAME}"
[ -f "${DEX_SRC}" ] || die "缺框架 dex ${DEX_ADD_NAME}: ${DEX_SRC}"
FSHA="$(sha_of "${DEX_SRC}")"
[ "${FSHA}" = "${KYZT_FRAMEWORK_SHA256_EXPECT}" ] \
  || die "框架 dex sha256 不符（期望 ${KYZT_FRAMEWORK_SHA256_EXPECT}，实得 ${FSHA}）"

# 3.6 改包法两个官方 dex 都改 ⇒ 两个都必填；缺一个就退化成「只带菜单」，查12 会 FAIL，不静默放行
[ -n "${KYZT_CLASSES_DEX}" ]  || die "必填 KYZT_CLASSES_DEX（官方 classes.dex 改包产物，例：13_scratch/kyzt36/out_classes.dex）"
[ -n "${KYZT_CLASSES2_DEX}" ] || die "必填 KYZT_CLASSES2_DEX（官方 classes2.dex 改包产物，例：13_scratch/kyzt36/out_classes2.dex）"
[ -f "${KYZT_CLASSES_DEX}" ]  || die "缺改包法 classes.dex: ${KYZT_CLASSES_DEX}"
[ -f "${KYZT_CLASSES2_DEX}" ] || die "缺改包法 classes2.dex: ${KYZT_CLASSES2_DEX}"
C1SHA="$(sha_of "${KYZT_CLASSES_DEX}")"
C2SHA="$(sha_of "${KYZT_CLASSES2_DEX}")"
[ "${C1SHA}" = "${KYZT_CLASSES_SHA256_EXPECT}" ] \
  || die "classes.dex sha256 不符（期望 ${KYZT_CLASSES_SHA256_EXPECT}，实得 ${C1SHA}）"
[ "${C2SHA}" = "${KYZT_CLASSES2_SHA256_EXPECT}" ] \
  || die "classes2.dex sha256 不符（期望 ${KYZT_CLASSES2_SHA256_EXPECT}，实得 ${C2SHA}）"

KILLER_CLASSES=""
[ -f "${KYZT_KILLER_DEX}" ] || die "缺 killer dex: ${KYZT_KILLER_DEX}"
KSHA="$(sha_of "${KYZT_KILLER_DEX}")"
[ "${KSHA}" = "${KYZT_KILLER_SHA256_EXPECT}" ] \
  || die "killer dex sha256 不符（期望 ${KYZT_KILLER_SHA256_EXPECT}，实得 ${KSHA}）"
KILLER_CLASSES="$("$PY" -c 'import struct,sys;print(struct.unpack_from("<I", open(sys.argv[1],"rb").read(), 0x60)[0])' "${KYZT_KILLER_DEX}")" \
  || die "读 killer dex class_defs_size 失败: ${KYZT_KILLER_DEX}"
grep -qaF 'bin/mt/signature/KillerApplication193' "${KYZT_KILLER_DEX}" \
  || die "killer dex 缺主类 bin/mt/signature/KillerApplication193"
grep -qaF 'com.xf.kyzt' "${KYZT_KILLER_DEX}" \
  || die "killer dex 未烘死包名 com.xf.kyzt"
[ "${KILLER_CLASSES}" -gt 0 ] || die "killer dex 类数为 0"

[ -f "${SO_SRC}" ] || die "缺 so: ${SO_SRC}（先 ndk-build Android_kyzt.mk）"
[ -f "${LIBCXX_SRC}" ] || die "缺 libc++_shared.so: ${LIBCXX_SRC}"
if [ -n "${SO_SHA256_EXPECT}" ] && [ "$(sha_of "${SO_SRC}")" != "${SO_SHA256_EXPECT}" ]; then
  die "so sha256 不符（期望 ${SO_SHA256_EXPECT}，实得 $(sha_of "${SO_SRC}")）"
fi

echo "[0/5] 输入核对"
echo "      底包 ${BASE_APK}"
echo "      sha256 ${BSHA} (OK)"
echo "      dex  classes.dex  $(wc -c <"${KYZT_CLASSES_DEX}" | tr -d ' ') B ${C1SHA}（改包法）"
echo "      dex  classes2.dex $(wc -c <"${KYZT_CLASSES2_DEX}" | tr -d ' ') B ${C2SHA}（改包法）"
echo "      dex  ${DEX_ADD_NAME}  $(wc -c <"${DEX_SRC}" | tr -d ' ') B ${FSHA}（框架）"
echo "      kill ${KILLER_DEX_NAME} $(wc -c <"${KYZT_KILLER_DEX}" | tr -d ' ') B ${KSHA} ${KILLER_CLASSES} 个类（C# 反篡改 DER 回灌）"
echo "      so   $(wc -c <"${SO_SRC}" | tr -d ' ') B $(sha_of "${SO_SRC}")"
echo "      cxx  $(wc -c <"${LIBCXX_SRC}" | tr -d ' ') B $(sha_of "${LIBCXX_SRC}")"
echo "      dex 序号 1..4（classes.dex / classes2.dex / ${DEX_ADD_NAME} / ${KILLER_DEX_NAME}）"

mkdir -p "${OUT_DIR}" "${WORK}"

echo "[1/5] 官方 dex 基线（组装后逐字节复核用）"
"$PY" - "${BASE_APK}" "${WORK}/base_dex.json" <<'PY'
import hashlib, json, sys, zipfile
apk, out = sys.argv[1], sys.argv[2]
d = {}
with zipfile.ZipFile(apk) as z:
    names = z.namelist()
    for n in names:
        if n.startswith('classes') and n.endswith('.dex'):
            d[n] = hashlib.sha256(z.read(n)).hexdigest()
    # 3.6 官方 dex 集合硬断言：只有 classes.dex + classes2.dex（布局变了必须改脚本，不静默）
    if set(d) != set(['classes.dex', 'classes2.dex']):
        raise SystemExit('FAIL 官方 dex 集合不是 {classes.dex, classes2.dex}（实际 %s）' % sorted(d))
open(out, 'w').write(json.dumps(d, indent=2, sort_keys=True))
print('      ' + ' / '.join('%s=%s' % (k, v[:12]) for k, v in sorted(d.items())))
print('      zip 条目 %d / so %d 个（基线）'
      % (len(names), len([n for n in names if n.startswith('lib/arm64-v8a/') and n.endswith('.so')])))
PY

echo "[2/5] Manifest 注入（二进制；缓存 ${CACHE_MF}）"
if [ ! -f "${CACHE_MF}" ]; then
  rm -rf "${MF_DEC}"
  "$JAVA" -jar "${APKTOOL_JAR}" d -f -s --no-assets -o "${MF_DEC}" "${BASE_APK}" >"${WORK}/apktool_d.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_d.log" >&2; die "apktool d 失败"; }
  "$PY" - "${MF_DEC}/AndroidManifest.xml" "${AUTHORITY}" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
t = p.read_text(encoding='utf-8')
# 3.6 硬约束：不注入、不替换 appComponentFactory（3.3 的注入分支已删）
if 'android:appComponentFactory=' in t:
    print('      keep base appComponentFactory（不注入 / 不替换）')
else:
    print('      base 无 appComponentFactory（保持无，不注入）')
i = t.find('<application'); j = t.find('>', i)
if i < 0 or j < 0:
    raise SystemExit('no <application>')
if 'android:name' in t[i:j]:
    raise SystemExit('底包 <application> 已带 android:name（3.6 预期无）：%s' % t[i:j][:200])
perms = []
for key in ('android.permission.SYSTEM_ALERT_WINDOW',
            'android.permission.FOREGROUND_SERVICE',
            'android.permission.FOREGROUND_SERVICE_SPECIAL_USE'):
    if key not in t:
        perms.append('    <uses-permission android:name="%s"/>' % key)
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
# 只加三样：不许出现已删除的签名伪装相关类
for bad in ('StarryAppComponentFactory', 'SignSpoof'):
    if bad in t:
        raise SystemExit('manifest 出现已删除类 %s（3.6 禁止）' % bad)
i = t.find('<application'); j = t.find('>', i)
if 'android:name' in t[i:j]:
    raise SystemExit('application 被加了 android:name（3.6 禁止）')
p.write_text(t, encoding='utf-8')
print('      manifest patched（StarryInitProvider + MenuService + 3 权限；appComponentFactory 未动）')
PY
  rm -f "${WORK}/mf_only.apk"   # Win: apktool 回编末尾 rename 到已存在文件会报错
  "$JAVA" -jar "${APKTOOL_JAR}" b -f "${MF_DEC}" -o "${WORK}/mf_only.apk" >"${WORK}/apktool_b.log" 2>&1 \
    || { tail -20 "${WORK}/apktool_b.log" >&2; die "apktool b 失败"; }
  "$PY" - "${WORK}/mf_only.apk" "${CACHE_MF}" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
data = z.read('AndroidManifest.xml')
open(sys.argv[2], 'wb').write(data)
if data[:4] != b'\x03\x00\x08\x00':
    raise SystemExit('FAIL 回编 manifest 不是 AXML（头 %s）' % data[:4].hex())
print('      manifest %d B' % len(data))
PY
else
  echo "      缓存命中 ${CACHE_MF} ($(wc -c <"${CACHE_MF}" | tr -d ' ') B)"
fi

# manifest 回读断言：只加三样 + 组件计数（provider +1 / service +1 / 权限 +3，activity/receiver 不变）
BASE_MF_XML="${WORK}/base_mf_axml.xml"
unzip -p "${BASE_APK}" AndroidManifest.xml > "${BASE_MF_XML}" || die "取底包 manifest 失败"
cnt_of() { PYTHONUTF8=1 PYTHONIOENCODING=utf-8 "$PY" "${AXML}" "$1" 2>/dev/null | grep -c "^E: $2" || true; }
b_prov="$(cnt_of "${BASE_MF_XML}" provider)"
f_prov="$(cnt_of "${CACHE_MF}" provider)"
b_svc="$(cnt_of "${BASE_MF_XML}" service)"
f_svc="$(cnt_of "${CACHE_MF}" service)"
b_act="$(cnt_of "${BASE_MF_XML}" activity)"
f_act="$(cnt_of "${CACHE_MF}" activity)"
b_rcv="$(cnt_of "${BASE_MF_XML}" receiver)"
f_rcv="$(cnt_of "${CACHE_MF}" receiver)"
b_perm="$(cnt_of "${BASE_MF_XML}" uses-permission)"
f_perm="$(cnt_of "${CACHE_MF}" uses-permission)"
echo "      回读组件: provider ${b_prov}->${f_prov} / service ${b_svc}->${f_svc} / activity ${b_act}->${f_act} / receiver ${b_rcv}->${f_rcv} / permission ${b_perm}->${f_perm}"
[ "${f_prov}" -eq "$((b_prov + 1))" ] || die "manifest provider 计数异常（期望 ${b_prov}+1，实得 ${f_prov}）"
[ "${f_svc}" -eq "$((b_svc + 1))" ] || die "manifest service 计数异常（期望 ${b_svc}+1，实得 ${f_svc}）"
[ "${f_act}" -eq "${b_act}" ] || die "manifest activity 计数变了（${b_act}->${f_act}）：3.6 只许加 provider/service/权限"
[ "${f_rcv}" -eq "${b_rcv}" ] || die "manifest receiver 计数变了（${b_rcv}->${f_rcv}）"
[ "${f_perm}" -eq "$((b_perm + 3))" ] || die "manifest 权限计数异常（期望 ${b_perm}+3，实得 ${f_perm}）"

# AXML 属性值级断言（不含文本 grep 的语义判断）：authorities / initOrder=0x64 / specialUse=0x40000000 / property 名
PYTHONUTF8=1 PYTHONIOENCODING=utf-8 "$PY" "${AXML}" "${CACHE_MF}" > "${WORK}/mf_tree.txt" 2>&1 \
  || die "axml_inspect 解析成品 manifest 失败（见 ${WORK}/mf_tree.txt）"
grep -qF 'com.starry.magic.StarryInitProvider' "${WORK}/mf_tree.txt" || die "manifest 缺 StarryInitProvider"
awk -v a=android:authorities -v b="${AUTHORITY}" 'index($0,a) && index($0,b){f=1} END{exit !f}' "${WORK}/mf_tree.txt" \
  || die "StarryInitProvider authorities 不是 ${AUTHORITY}"
awk -v a=android:initOrder 'index($0,a) && index($0,"0x00000064"){f=1} END{exit !f}' "${WORK}/mf_tree.txt" \
  || die "StarryInitProvider initOrder 不是 100"
awk -v a=android:foregroundServiceType 'index($0,a) && index($0,"0x40000000"){f=1} END{exit !f}' "${WORK}/mf_tree.txt" \
  || die "MenuService foregroundServiceType 不是 specialUse(0x40000000)"
grep -qF 'android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE' "${WORK}/mf_tree.txt" || die "MenuService 缺 PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
for p in android.permission.SYSTEM_ALERT_WINDOW android.permission.FOREGROUND_SERVICE android.permission.FOREGROUND_SERVICE_SPECIAL_USE; do
  grep -qF "$p" "${WORK}/mf_tree.txt" || die "manifest 缺权限 $p"
done
if grep -qF 'StarryAppComponentFactory' "${WORK}/mf_tree.txt"; then die "manifest 出现 StarryAppComponentFactory（3.6 禁止）"; fi
echo "      manifest 结构断言 OK（三样改动就位；appComponentFactory 未动、application 无 android:name）"

echo "[3/5] 外科组装（其余条目字节保真）"
"$PY" - "${BASE_APK}" "${FINAL}" "${CACHE_MF}" "${DEX_SRC}" "${SO_SRC}" "${LIBCXX_SRC}" \
        "${KYZT_CLASSES_DEX}" "${KYZT_CLASSES2_DEX}" "${DEX_ADD_NAME}" "${KYZT_KILLER_DEX}" "${KILLER_DEX_NAME}" <<'PY'
import sys, zipfile, os
base, out, mf, dex, so, cxx, c1, c2, dex_name, killer, killer_name = sys.argv[1:12]
OFFICIAL_DEX = set(["classes.dex", "classes2.dex"])   # 3.6 底包 2 dex
ADD = {
    'AndroidManifest.xml': mf,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
for dname, path in (('classes.dex', c1), ('classes2.dex', c2)):
    if not os.path.isfile(path):
        raise SystemExit('缺改包 dex %s: %s' % (dname, path))
    ADD[dname] = path                 # 改包法：两个官方 dex 都替换
for name in (dex_name, killer_name):
    if name in OFFICIAL_DEX:
        raise SystemExit('拒绝覆盖官方 dex: %s' % name)
if dex_name == killer_name:
    raise SystemExit('框架 dex 与 killer 撞号: %s' % dex_name)
if os.path.basename(dex) != dex_name:
    print('      note: 源 dex %s -> 入包名 %s' % (os.path.basename(dex), dex_name))
ADD[dex_name] = dex                  # 框架 classes3.dex
if not os.path.isfile(killer):
    raise SystemExit('缺 killer dex: %s' % killer)
ADD[killer_name] = killer            # killer classes4.dex
with zipfile.ZipFile(base) as zi, zipfile.ZipFile(out, 'w', compresslevel=9) as zo:
    names = zi.namelist()
    for n in names:
        info = zi.getinfo(n)
        data = open(ADD[n], 'rb').read() if n in ADD else zi.read(n)
        # 保留底包原始压缩方式（底包 extractNativeLibs=true，官方 so 是 deflate）
        comp = info.compress_type
        ni = zipfile.ZipInfo(n, date_time=info.date_time)
        ni.compress_type = comp
        ni.external_attr = info.external_attr
        zo.writestr(ni, data)
    for n, path in sorted(ADD.items()):
        if n in names:
            continue
        comp = zipfile.ZIP_STORED if n.endswith('.so') else zipfile.ZIP_DEFLATED
        ni = zipfile.ZipInfo(n, date_time=(1981, 1, 1, 1, 1, 2))
        ni.compress_type = comp
        zo.writestr(ni, open(path, 'rb').read())
print('      assembled %s' % out)
PY

echo "[4/5] 校验：两 dex 替换就位 / 序号连续 1..4 / killer / so / manifest"
"$PY" - "${BASE_APK}" "${FINAL}" "${WORK}/base_dex.json" "${DEX_ADD_NAME}" \
        "${KYZT_CLASSES_DEX}" "${KYZT_CLASSES2_DEX}" "${KYZT_KILLER_DEX}" "${KILLER_DEX_NAME}" "${DEX_SRC}" <<'PY'
import hashlib, json, struct, sys, zipfile
base, out, basejson, dexname, c1, c2, killer, killer_name, fwpath = sys.argv[1:10]
want = json.load(open(basejson))
with zipfile.ZipFile(out) as z:
    got = {n: hashlib.sha256(z.read(n)).hexdigest()
           for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')}
    names = set(z.namelist())
    entries = len(z.namelist())
    so = z.read('lib/arm64-v8a/libstarry_magic.so')
    mf = z.read('AndroidManifest.xml')
    kblob = z.read(killer_name)

# 1) dex 集合与序号：必须恰为 classes.dex..classes4.dex（ART 遇首个缺口即停扫）
expect = set(['classes.dex', 'classes2.dex', dexname, killer_name])
if set(got) != expect:
    raise SystemExit('FAIL dex 集合不符（期望 %s，实际 %s）' % (sorted(expect), sorted(got)))
nums = sorted([1] + [int(n[7:-4]) for n in got if n != 'classes.dex'])
if nums != [1, 2, 3, 4]:
    raise SystemExit('FAIL dex 序号不是 1..4（实际 %s）' % nums)

# 2) 两个官方 dex 都是给定改包产物，且都不等于底包（补丁生效）
patched = {'classes.dex': open(c1, 'rb').read(), 'classes2.dex': open(c2, 'rb').read()}
for name, d in sorted(patched.items()):
    if name not in got:
        raise SystemExit('FAIL 缺 %s' % name)
    if got[name] != hashlib.sha256(d).hexdigest():
        raise SystemExit('FAIL %s 不是给定改包法产物（字节不符）' % name)
    if got[name] == want.get(name):
        raise SystemExit('FAIL %s 与底包逐字节相同（补丁没生效）' % name)
    print('      OK 改包法 %s: %d B sha256 %s' % (name, len(d), hashlib.sha256(d).hexdigest()))

# 3) 框架 dex（不入底包，故与底包无「逐字节相同」可比；比入包字节 == 给定文件）
fw = open(fwpath, 'rb').read()
if got[dexname] != hashlib.sha256(fw).hexdigest():
    raise SystemExit('FAIL 框架 dex 字节不符（入包内容 ≠ 给定文件）')
print('      OK 框架 %s: %d B %s（字节与给定文件一致）'
      % (dexname, len(fw), hashlib.sha256(fw).hexdigest()[:16]))

# 4) killer：字节 / 类数 / 主类 / 包名
k = open(killer, 'rb').read()
if got[killer_name] != hashlib.sha256(k).hexdigest():
    raise SystemExit('FAIL killer dex 字节不符（入包内容 ≠ 给定文件）')
kcls = struct.unpack_from('<I', kblob, 0x60)[0]
if kcls != struct.unpack_from('<I', k, 0x60)[0]:
    raise SystemExit('FAIL killer dex 类数不符')
for nd in (b'bin/mt/signature/KillerApplication193', b'com.xf.kyzt'):
    if nd not in kblob:
        raise SystemExit('FAIL killer dex 缺 %s' % nd.decode())
print('      OK killer %s: %d B %d 类 sha256 %s（主类 bin.mt.signature.KillerApplication193）'
      % (killer_name, len(k), kcls, hashlib.sha256(k).hexdigest()[:16]))

# 5) so / manifest
for need in ('lib/arm64-v8a/libstarry_magic.so', 'lib/arm64-v8a/libc++_shared.so'):
    if need not in names:
        raise SystemExit('FAIL 缺 %s' % need)
if b'KYZT' not in so:
    raise SystemExit('FAIL so 内无 KYZT 标签（装错 so？）')
for badsym in (b'libwbmod', b'libtoolChecker'):
    if badsym in so:
        raise SystemExit('FAIL so 内含 %s（旧版 so 装错）' % badsym.decode())
if not (b'StarryInitProvider' in mf or 'StarryInitProvider'.encode('utf-16-le') in mf):
    raise SystemExit('FAIL manifest 缺 StarryInitProvider')
if not (b'MenuService' in mf or 'MenuService'.encode('utf-16-le') in mf):
    raise SystemExit('FAIL manifest 缺 MenuService')

# 6) 条目守恒：底包条目 + 4（classes3/classes4 + 2 个 so），其余一律原样
with zipfile.ZipFile(base) as zb:
    bnames = set(zb.namelist())
if entries != len(bnames) + 4:
    raise SystemExit('FAIL zip 条目数异常（底包 %d + 4 ≠ 成品 %d）' % (len(bnames), entries))
if names - bnames != expect - set(['classes.dex', 'classes2.dex']) | set(['lib/arm64-v8a/libstarry_magic.so', 'lib/arm64-v8a/libc++_shared.so']):
    raise SystemExit('FAIL 新增条目集合不符: %s' % sorted(names - bnames))

print('      OK 官方 dex 全部由改包法替换（classes.dex / classes2.dex）；其余条目字节保真')
print('      OK dex 序号连续 1..4（%s）' % ' '.join(sorted(got, key=lambda t: 1 if t == 'classes.dex' else int(t[7:-4]))))
print('      OK zip 条目 %d（底包 %d + 2 dex + 2 so）' % (entries, len(bnames)))
print('      OK manifest: StarryInitProvider + MenuService 就位')
PY

echo "[4/5] 四锚点断言（python 解析 dex 字节码；失败即 die）"
"$PY" - "${KYZT_CLASSES_DEX}" "${KYZT_CLASSES2_DEX}" <<'PY' || die "四锚点断言失败（见上）"
import struct, sys

def uleb(b, o):
    r = 0; s = 0
    while True:
        x = b[o]; o += 1
        r |= (x & 0x7f) << s
        if not x & 0x80:
            return r, o
        s += 7

def mutf8(b, o):
    n, o = uleb(b, o)          # 跳过 utf16 长度
    e = b.index(0, o)
    return b[o:e].decode('utf-8', 'replace'), e + 1

# 指令长度（16-bit 单元）；payload 伪指令在 decode 里单独处理
SZ = [1] * 256
def _r(a, z, n):
    for i in range(a, z + 1):
        SZ[i] = n
_r(0x01, 0x01, 1); _r(0x02, 0x02, 2); _r(0x03, 0x03, 3)
_r(0x04, 0x06, 1); _r(0x07, 0x09, 2); _r(0x0a, 0x12, 1)
_r(0x13, 0x13, 2); _r(0x14, 0x14, 3); _r(0x15, 0x15, 2)
_r(0x16, 0x16, 2); _r(0x17, 0x17, 3); _r(0x18, 0x18, 5); _r(0x19, 0x19, 2)
_r(0x1a, 0x1c, 2); _r(0x1d, 0x1e, 1); _r(0x1f, 0x20, 2)
_r(0x21, 0x21, 1); _r(0x22, 0x23, 2); _r(0x24, 0x24, 3); _r(0x25, 0x25, 3)
_r(0x26, 0x26, 3); _r(0x27, 0x27, 1); _r(0x28, 0x28, 1); _r(0x29, 0x29, 2)
_r(0x2a, 0x2a, 3); _r(0x2b, 0x2c, 3); _r(0x2d, 0x3d, 2); _r(0x3e, 0x43, 1)
_r(0x44, 0x51, 2); _r(0x52, 0x6d, 2)
_r(0x6e, 0x72, 3); _r(0x73, 0x73, 1); _r(0x74, 0x78, 3)
_r(0x79, 0x7a, 1); _r(0x7b, 0x8f, 1); _r(0x90, 0xaf, 2); _r(0xb0, 0xcf, 1)
_r(0xd0, 0xd7, 2); _r(0xd8, 0xe2, 2); _r(0xe3, 0xf9, 1)
_r(0xfa, 0xfb, 4); _r(0xfc, 0xfd, 3); _r(0xfe, 0xff, 2)

class Dex:
    def __init__(self, path):
        self.b = open(path, 'rb').read()
        b = self.b
        if b[:4] != b'dex\n':
            raise SystemExit('不是 dex 文件: %s' % path)
        self.so = struct.unpack_from('<I', b, 0x3c)[0]
        self.to = struct.unpack_from('<I', b, 0x44)[0]
        self.po = struct.unpack_from('<I', b, 0x4c)[0]
        self.mo = struct.unpack_from('<I', b, 0x5c)[0]
        self.cs = struct.unpack_from('<I', b, 0x60)[0]
        self.co = struct.unpack_from('<I', b, 0x64)[0]
        self._s = {}; self._t = {}
    def string(self, i):
        if i not in self._s:
            o = struct.unpack_from('<I', self.b, self.so + 4 * i)[0]
            self._s[i], _ = mutf8(self.b, o)
        return self._s[i]
    def typ(self, i):
        if i not in self._t:
            self._t[i] = self.string(struct.unpack_from('<I', self.b, self.to + 4 * i)[0])
        return self._t[i]
    def proto(self, i):
        shorty, ret, poff = struct.unpack_from('<III', self.b, self.po + 12 * i)
        ps = []
        if poff:
            n = struct.unpack_from('<I', self.b, poff)[0]
            ps = [self.typ(struct.unpack_from('<H', self.b, poff + 4 + 2 * k)[0]) for k in range(n)]
        return self.typ(ret), ps
    def method(self, i):
        c, p, n = struct.unpack_from('<HHI', self.b, self.mo + 8 * i)
        ret, ps = self.proto(p)
        return 'L%s;' % self.typ(c), self.string(n), ret, ps
    def classes(self):
        for k in range(self.cs):
            off = self.co + 32 * k
            yield self.typ(struct.unpack_from('<I', self.b, off)[0]), struct.unpack_from('<I', self.b, off + 24)[0]
    def methods_of(self, cdata):
        b = self.b; o = cdata
        sf, o = uleb(b, o); inf, o = uleb(b, o); dm, o = uleb(b, o); vm, o = uleb(b, o)
        idx = 0
        for _ in range(sf):
            _, o = uleb(b, o); idx, o = uleb(b, o)
        idx = 0
        for _ in range(inf):
            _, o = uleb(b, o); idx, o = uleb(b, o)
        out = []
        for cnt in (dm, vm):
            idx = 0
            for _ in range(cnt):
                d, o = uleb(b, o); _flags, o = uleb(b, o); co, o = uleb(b, o)
                idx += d
                out.append((idx, co))
        return out
    def code(self, off):
        n = struct.unpack_from('<I', self.b, off + 12)[0]
        return struct.unpack_from('<%dH' % n, self.b, off + 16)
    def decode(self, units):
        out = []; i = 0; n = len(units)
        while i < n:
            u = units[i]; op = u & 0xff
            if op == 0 and (u >> 8) in (1, 2, 3):
                kind = u >> 8
                if kind == 1:
                    size = 4 + 2 * units[i + 1]
                elif kind == 2:
                    size = 2 + 4 * units[i + 1]
                else:
                    size = 4 + (units[i + 1] * units[i + 2] + 1) // 2
                out.append((-1, i, units[i:i + size])); i += size; continue
            sz = SZ[op]
            out.append((op, i, units[i:i + sz])); i += sz
        return out

def code_of(d, cls, name, nparams, ret):
    for c, cd in d.classes():
        if c != cls or cd == 0:
            continue
        for mi, co in d.methods_of(cd):
            cc, nm, rt, ps = d.method(mi)
            if nm == name and len(ps) == nparams and rt == ret:
                if co == 0:
                    raise SystemExit('FAIL %s.%s 无代码体' % (cls, name))
                return d.decode(d.code(co))
    raise SystemExit('FAIL 找不到方法 %s.%s（参数 %d，返回 %s）' % (cls, name, nparams, ret))

d1 = Dex(sys.argv[1])
d2 = Dex(sys.argv[2])
FAIL = []

# 锚点1：门闩 showErrorToast 只剩 return-void
ops1 = code_of(d1, 'Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;',
               'showErrorToast', 1, 'V')
if [x[0] for x in ops1] != [0x0e]:
    FAIL.append('showErrorToast 不是只剩 return-void（指令 %s）' % [x[0] for x in ops1])
else:
    print('      PASS classes.dex GatekeeperRepository.showErrorToast 只剩 return-void（1 条指令）')

# 锚点2：TapSignKit.getSign 只剩常量串 + return-object
SIGN = '360c78f563dd471fa6df02cc68045a31'
ops2 = code_of(d1, 'Lcom/taptap/sdk/kit/internal/utils/TapSignKit;', 'getSign', 1, 'Ljava/lang/String;')
ok2 = (len(ops2) == 2 and ops2[0][0] == 0x1a and ops2[1][0] == 0x11)
if ok2:
    sidx = ops2[0][2][1]
    ok2 = (d1.string(sidx) == SIGN and ((ops2[0][2][0] >> 8) & 0xff) == ((ops2[1][2][0] >> 8) & 0xff))
if not ok2:
    FAIL.append('getSign 不是「const-string %s + return-object 同寄存器」' % SIGN)
else:
    print('      PASS classes.dex TapSignKit.getSign 只剩 const-string %s + return-object（2 条指令）' % SIGN)

# 锚点3：provide 唯一 const/4 v2, 0x3，且紧跟 aget（强制走 WebLoginFragment 分支）
ops3 = code_of(d2, 'Lcom/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider;',
               'provide', 1, 'Landroid/app/Fragment;')
hits = [k for k, x in enumerate(ops3)
        if x[0] == 0x12 and ((x[2][0] >> 8) & 0xf) == 2 and ((x[2][0] >> 12) & 0xf) == 3]
if len(hits) != 1:
    FAIL.append('provide 中 const/4 v2, 0x3 出现 %d 次（须唯一）' % len(hits))
elif hits[0] == 0 or ops3[hits[0] - 1][0] != 0x44:
    FAIL.append('provide 中 const/4 v2, 0x3 不在 aget 之后（位置 %d，前一条 op=0x%02x）'
                % (hits[0], ops3[hits[0] - 1][0] if hits[0] else -1))
else:
    print('      PASS classes2.dex DefaultTapLoginFragmentProvider.provide 唯一 const/4 v2, 0x3（紧跟 aget）')

# 锚点4：getVerifiedVersionCode = const-wide/16 0x24 + return-wide
ops4 = code_of(d2, 'Lcom/xiefeng/game2d/PrivacyActivity;', 'getVerifiedVersionCode', 1, 'J')
ok4 = (len(ops4) == 2 and ops4[0][0] == 0x16 and ops4[1][0] == 0x10
       and ((ops4[0][2][0] >> 8) & 0xf) == 0 and ops4[0][2][1] == 0x24)
if not ok4:
    FAIL.append('getVerifiedVersionCode 不是 const-wide/16 v0, 0x24 + return-wide')
else:
    print('      PASS classes2.dex PrivacyActivity.getVerifiedVersionCode = const-wide/16 0x24 + return-wide')

if FAIL:
    for f in FAIL:
        print('      FAIL ' + f)
    sys.exit(1)
print('      PASS 四锚点断言 4/4（classes.dex 2 + classes2.dex 2）')
PY

echo "[5/5] 签名（starry_release.jks / alias=starry，与 gradle_cycle.sh 同源口径）"
KS="${NIXIANG}/01_tools/starry_release.jks"
KS_ALIAS="${KS_ALIAS:-starry}"
KS_PASS="${KS_PASS:-starry2026}"
if [ -n "${NIXIANG_APKSIGNER:-}" ] && [ -f "${KS}" ]; then
  "${NIXIANG_APKSIGNER}" sign --ks "${KS}" --ks-key-alias "${KS_ALIAS}" \
    --ks-pass "pass:${KS_PASS}" --key-pass "pass:${KS_PASS}" \
    --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
    --out "${SIGNED}" "${FINAL}" >"${WORK}/sign.log" 2>&1 \
    || { tail -10 "${WORK}/sign.log" >&2; die "签名失败"; }
  "${NIXIANG_APKSIGNER}" verify "${SIGNED}" >/dev/null || die "apksigner verify 失败"
  echo "      signed OK"
else
  echo "      WARN 无 apksigner/缺 ks，仅输出未签名包"
  cp -f "${FINAL}" "${SIGNED}"
fi

ZIPN="$("$PY" -c 'import sys,zipfile;print(len(zipfile.ZipFile(sys.argv[1]).namelist()))' "${SIGNED}")"
echo "DONE"
echo "  signed  : ${SIGNED}"
echo "  size    : $(wc -c <"${SIGNED}" | tr -d ' ') B"
echo "  md5     : $(md5_of "${SIGNED}")"
echo "  sha256  : $(sha_of "${SIGNED}")"
echo "  zip     : ${ZIPN} 条目"
echo "  dex     : classes.dex classes2.dex ${DEX_ADD_NAME} ${KILLER_DEX_NAME}（1..4 连续）"
echo "  unsigned: ${FINAL} ($(wc -c <"${FINAL}" | tr -d ' ') B, sha256 $(sha_of "${FINAL}"))"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  echo "装机：请显式执行（本脚本默认不装，不调 adb）"
  echo "  bash 04_scripts/_common/install_starry_apk.sh --serial ${SERIAL} --apk ${SIGNED} --pkg ${PKG}"
fi
