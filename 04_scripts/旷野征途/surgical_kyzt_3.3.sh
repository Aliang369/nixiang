#!/usr/bin/env bash
# ================================================================
#  旷野征途 (com.xf.kyzt) 3.3 / vc33 — 星光外科组装（只构建，不含装机）
#
#  底包：02_apks/旷野征途/旷野征途_3.3_taptap_official.apk
#        sha256 8b1fceee…303f / 199,384,028 B / 7 dex / signer O=xiefeng
#        （只读拉取产物；取证见 10_docs/旷野征途/底包取证_3.3_20261125.md）
#
#  母本：surgical_kyzt_3.2.sh（只改版本/底包指纹/输入默认路径/工作目录；门禁次序、Windows 三改、python
#        断言原样）。3.3 未出施工记录 ⇒ 依据沿用 3.2 档 10_docs/旷野征途/*_3.2_20261124.md + 静态_Java_Tap改包_3.3_20261125.md。
#
#  与 2.4 版（surgical_kyzt_2.4.sh）的差异（**只改序号与补丁输入，其余门禁原样**）：
#   · 3.3 是 **7 dex 包**（classes.dex..classes7.dex；2.4 是 8 dex）⇒ 改包法**只替换
#     classes6.dex**（2.4 替换 classes6 + classes7）。四个 Tap 锚点全在 classes6.dex：
#     TapTapSdkInitializer / GatekeeperRepository（门闩）、DefaultTapLoginFragmentProvider（登录入口）、
#     TapSignKit.getSign（签名）、com.xiefeng.game2d.PrivacyActivity.getVerifiedVersionCode；
#     3.3 只读复核：四锚点仍全在 classes6.dex、dex 布局与母本同构
#     （10_docs/旷野征途/静态_Java_Tap改包_3.3_20261125.md ③）。
#   · **框架 dex 序号 = classes8.dex**（ART 遇首个缺口即停扫，1..8 必须连续无缺口）。
#     框架 dex 不在模板目录（模板仍是 2.4 的 classes9 记录）⇒ 默认取阶段3a 交付件
#     13_scratch/kyzt33_build/out_classes8.dex，可用 KYZT_FRAMEWORK_DEX 覆盖。
#   · **killer dex 可选**（默认不注入 ⇒ 1..8）：C# 侧反篡改
#     `ET.AndroidPackageIntegrity.VerifyCurrentSigner`（AOT，Java 补丁管不到）比对
#     base64(SHA256(DER)) ⇒ 必须让 Java 侧返回官方 DER，故装机版需给 KYZT_KILLER_DEX，
#     注入为 **classes9.dex**（框架之后，序号连续）。不给 killer 仅用于链路验证。
#   · classes.dex..classes5.dex 与 classes7.dex **逐字节不动**（含 com.xf.kyzt 的 R 类）。
#   · manifest 改动与 2.4 完全相同（StarryInitProvider + MenuService + 3 个权限；
#     **不加 android:name、不换 appComponentFactory**）——3.3 底包 application 同样
#     无 android:name、appComponentFactory=androidx.core.app.CoreComponentFactory
#     （底包取证_3.3_20261125.md §3 实测）。
#   · **输入指纹钉 3.3 默认留空**（classes6 改包产物 / 框架 dex 的 3.3 指纹尚在生成）：
#     未提供时脚本打印 **WARN** 明示「未做 sha256 钉」，**不静默跳过、也不放宽任何其他门禁**
#     （dex 序号连续、官方 dex 逐字节、入包字节==给定文件且≠底包、四锚点断言照旧）。
#   · 默认**只构建**；装机是独立动作：STARRY_INSTALL=1 才会提示 install（默认关）。
#
#  用法：
#    KYZT_CLASSES6_DEX=13_scratch/kyzt33_java/out_classes6.dex bash 04_scripts/旷野征途/surgical_kyzt_3.3.sh
#    KYZT_CLASSES6_SHA256_EXPECT=<sha256> KYZT_FRAMEWORK_SHA256_EXPECT=<sha256> ...   # 指纹就绪后钉死
#    STARRY_INSTALL=1 SERIAL=96f0d883 bash ...                # 构建 + 提示装机（需另行授权）
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "${HERE}/../.." && pwd)"
# shellcheck source=/dev/null
. "${NIXIANG}/04_scripts/_common/env.sh"

STAR="${NIXIANG}/mod菜单/starry-mod-menu"
GAME="旷野征途"
VERSION="3.3"
PKG="com.xf.kyzt"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-96f0d883}"

BASE_APK="${KYZT_BASE_APK:-${NIXIANG}/02_apks/旷野征途/旷野征途_${VERSION}_taptap_official.apk}"
# 默认钉官方底包；仅在「外层自己生成并把关过中间产物」时用 KYZT_BASE_SHA256_EXPECT 覆盖（会打印告警）
BASE_SHA256_EXPECT="${KYZT_BASE_SHA256_EXPECT:-8b1fceee40e13e15ff9b6864a89eaeaba1923fc30b281f8b5e6c5368f144303f}"

DEX_ADD_NAME="classes8.dex"
# 3.3 框架 dex 不在模板目录（templates/kyzt_menu_current/build_info.txt 仍是 2.4 的 classes9 记录）
DEX_SRC="${KYZT_FRAMEWORK_DEX:-${NIXIANG}/13_scratch/kyzt33_build/out_classes8.dex}"
# 必填输入（3.3 改包法：四个 Tap 锚点全在 classes6.dex；3.3 复核见
# 10_docs/旷野征途/静态_Java_Tap改包_3.3_20261125.md ③（锚点/插入点口径与母本相同））：
#   KYZT_CLASSES6_DEX = 官方 classes6.dex 改包产物（门闩 / 登录入口 / getSign / getVerifiedVersionCode）
KYZT_CLASSES6_DEX="${KYZT_CLASSES6_DEX:-}"
# 可选输入：killer dex（C# 侧 ET.AndroidPackageIntegrity 反篡改 ⇒ 必须让 Java 侧返回官方 DER；
#   注入为 classes9.dex。**不做 sha256 硬钉**——killer 由 smali 汇编，非字节确定）
KYZT_KILLER_DEX="${KYZT_KILLER_DEX:-}"
KILLER_DEX_NAME="${KYZT_KILLER_DEX_NAME:-classes9.dex}"
# 输入指纹钉（3.3：默认**留空**——classes6 改包产物与框架 dex 的 3.3 指纹尚在生成；
#   环境变量显式给出即钉死，给出后不符立即 die；给出时会打印告警）
if [ -n "${KYZT_CLASSES6_SHA256_EXPECT:-}" ] || [ -n "${KYZT_FRAMEWORK_SHA256_EXPECT:-}" ]; then
  echo "      ⚠ 输入指纹钉由环境变量显式提供（非脚本默认钉；3.3 产物指纹尚在生成）"
fi
KYZT_CLASSES6_SHA256_EXPECT="${KYZT_CLASSES6_SHA256_EXPECT:-}"
KYZT_FRAMEWORK_SHA256_EXPECT="${KYZT_FRAMEWORK_SHA256_EXPECT:-}"
# so 用「2.2 起沿用的构建输出目录」app/src/libs_kyzt（2.4 就地产出覆盖同名文件），1.8 的 libs_kyzt 原样保留
SO_SRC="${STAR}/app/src/libs_kyzt/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STAR}/app/src/libs_kyzt/arm64-v8a/libc++_shared.so"
[ -f "$LIBCXX_SRC" ] || LIBCXX_SRC="${STAR}/libs_kyzt/arm64-v8a/libc++_shared.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-unsigned.apk"
SIGNED="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"
WORK="${NIXIANG}/07_patched/${GAME}/surgical_3.3"
CACHE_MF="${WORK}/AndroidManifest.xml"
# apktool b 的 aapt2 在 Win/GBK 控制台下打不开含中文的路径（实测 ERROR: failed to open directory … (2)）
# ⇒ 解码目录必须落在 **ASCII 路径**；缓存体仍在 WORK 下（python 读写中文路径没问题）
MF_DEC="${NIXIANG}/07_patched/kyzt33_mf_dec"

PY="${NIXIANG_PYTHON:-python3}"
JAVA="${NIXIANG_JAVA17}/bin/java"
APKTOOL_JAR="${NIXIANG_ROOT:-$NIXIANG}/01_tools/apktool/apktool.jar"
[ -f "$APKTOOL_JAR" ] || APKTOOL_JAR="${NIXIANG}/01_tools/apktool/apktool_2.9.3.jar"
[ -f "$APKTOOL_JAR" ] || APKTOOL_JAR="$(command -v apktool >/dev/null 2>&1 && echo '')"

die() { echo "ERROR: $*" >&2; exit 1; }
sha_of() {
  # 双机：mac 只有 shasum，Win Git Bash 只有 sha256sum（shasum 缺失会让 2.4 版直接崩）
  if command -v sha256sum >/dev/null 2>&1; then sha256sum "$1" | awk '{print $1}';
  else shasum -a 256 "$1" | awk '{print $1}'; fi
}

[ -f "$BASE_APK" ] || die "底包不存在: $BASE_APK"
BSHA="$(sha_of "$BASE_APK")"
[ "$BSHA" = "$BASE_SHA256_EXPECT" ] || die "底包 sha256 不符（期望 ${BASE_SHA256_EXPECT}，实得 ${BSHA}）"
[ -n "${KYZT_BASE_SHA256_EXPECT:-}" ] && echo "      ⚠ 底包指纹由 KYZT_BASE_SHA256_EXPECT 覆盖（非官方原包，用于改包法产物）"
[ -f "$DEX_SRC" ] || die "缺 ${DEX_ADD_NAME}: ${DEX_SRC}（阶段3a 交付件应在 13_scratch/kyzt33_build/）"
FSHA="$(sha_of "$DEX_SRC")"
if [ -n "$KYZT_FRAMEWORK_SHA256_EXPECT" ]; then
  [ "$FSHA" = "$KYZT_FRAMEWORK_SHA256_EXPECT" ] \
    || die "框架 dex sha256 不符（期望 ${KYZT_FRAMEWORK_SHA256_EXPECT}，实得 ${FSHA}）"
else
  echo "      WARN 未提供 KYZT_FRAMEWORK_SHA256_EXPECT（3.3 指纹待生成）⇒ ${DEX_ADD_NAME} 未做 sha256 钉"
  echo "           其余门禁不放宽：dex 序号 1..8 连续 / 官方 dex 逐字节 / 入包字节==给定文件 / 四锚点断言"
fi
# 3.3 改包法只改一个官方 dex ⇒ 必填；缺了就退化成「只带菜单」，查12 会 FAIL，不静默放行
[ -n "$KYZT_CLASSES6_DEX" ] \
  || die "必填 KYZT_CLASSES6_DEX（改包法 classes6.dex 产物，例：13_scratch/kyzt33_java/out_classes6.dex）"
[ -f "$KYZT_CLASSES6_DEX" ] || die "缺改包法 classes6.dex: $KYZT_CLASSES6_DEX"
C6SHA="$(sha_of "$KYZT_CLASSES6_DEX")"
if [ -n "$KYZT_CLASSES6_SHA256_EXPECT" ]; then
  [ "$C6SHA" = "$KYZT_CLASSES6_SHA256_EXPECT" ] \
    || die "classes6.dex sha256 不符（期望 ${KYZT_CLASSES6_SHA256_EXPECT}，实得 ${C6SHA}）"
else
  echo "      WARN 未提供 KYZT_CLASSES6_SHA256_EXPECT（3.3 改包法产物指纹待生成）⇒ classes6.dex 未做 sha256 钉"
  echo "           其余门禁不放宽：[4/5] 仍断言 入包 classes6 == 给定文件 / ≠ 底包原值 / 含四锚点，官方其余 6 dex 逐字节"
fi
KILLER_CLASSES=""
if [ -n "$KYZT_KILLER_DEX" ]; then
  [ -f "$KYZT_KILLER_DEX" ] || die "缺 killer dex: $KYZT_KILLER_DEX"
  case "$KILLER_DEX_NAME" in
    classes*.dex) : ;;
    *) die "KILLER_DEX_NAME 命名非法: $KILLER_DEX_NAME" ;;
  esac
  [ "$KILLER_DEX_NAME" = "$DEX_ADD_NAME" ] && die "killer dex 名与框架 dex 撞号: $KILLER_DEX_NAME"
  case "$KILLER_DEX_NAME" in
    classes.dex|classes[2-7].dex) die "killer dex 名与官方 dex 撞号: $KILLER_DEX_NAME" ;;
  esac
  KILLER_CLASSES="$("$PY" - "$KYZT_KILLER_DEX" <<'PY'
import struct, sys
b = open(sys.argv[1], 'rb').read()
if b[:4] != b'dex\n':
    raise SystemExit('killer 不是 dex 文件: %s' % sys.argv[1])
print(struct.unpack('<I', b[0x60:0x64])[0])
PY
)"
fi
[ -f "$SO_SRC" ] || die "缺 so: ${SO_SRC}（先 ndk-build Android_kyzt.mk）"
[ -f "$LIBCXX_SRC" ] || die "缺 libc++_shared.so: $LIBCXX_SRC"

echo "[0/5] 输入核对"
echo "      底包 $BASE_APK"
echo "      sha256 $BSHA (OK)"
echo "      dex  $DEX_ADD_NAME $(wc -c <"$DEX_SRC" | tr -d ' ') B $FSHA"
echo "      c6   classes6.dex $(wc -c <"$KYZT_CLASSES6_DEX" | tr -d ' ') B $C6SHA"
echo "      so   $(wc -c <"$SO_SRC" | tr -d ' ') B $(sha_of "$SO_SRC")"
if [ -n "$KYZT_KILLER_DEX" ]; then
  echo "      kill $KILLER_DEX_NAME $(wc -c <"$KYZT_KILLER_DEX" | tr -d ' ') B $(sha_of "$KYZT_KILLER_DEX") ${KILLER_CLASSES} 个类（不作指纹门禁）"
else
  echo "      kill 未注入（KYZT_KILLER_DEX 空 ⇒ 1..8，装机版会缺 Java 侧 DER 回灌）"
fi

mkdir -p "$OUT_DIR" "$WORK"

echo "[1/5] 官方 dex 基线（组装后逐字节复核用）"
"$PY" - "$BASE_APK" "$WORK/base_dex.json" <<'PY'
import hashlib, json, sys, zipfile
apk, out = sys.argv[1], sys.argv[2]
d = {}
with zipfile.ZipFile(apk) as z:
    for n in z.namelist():
        if n.startswith('classes') and n.endswith('.dex'):
            d[n] = hashlib.sha256(z.read(n)).hexdigest()
open(out, 'w').write(json.dumps(d, indent=2, sort_keys=True))
print('      ' + ' / '.join('%s=%s' % (k, v[:12]) for k, v in sorted(d.items())))
PY

echo "[2/5] Manifest 注入（二进制；缓存 ${CACHE_MF}）"
if [ ! -f "$CACHE_MF" ]; then
  rm -rf "$MF_DEC"
  "$JAVA" -jar "$APKTOOL_JAR" d -f -s --no-assets -o "$MF_DEC" "$BASE_APK" >"$WORK/apktool_d.log" 2>&1 \
    || { tail -20 "$WORK/apktool_d.log" >&2; die "apktool d 失败"; }
  "$PY" - "$MF_DEC/AndroidManifest.xml" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); authority = sys.argv[2]
t = p.read_text(encoding='utf-8')
FACTORY = 'com.starry.magic.sign.StarryAppComponentFactory'
if 'android:appComponentFactory=' in t:
    # 2.2 底包自带 androidx.core.app.CoreComponentFactory（1.8 没有）。
    # 2026-11-14 口径：不注入、不替换任何 appComponentFactory（既不丢 androidx 语义，
    # 也不引入 com.starry.magic.sign.* 签名相关组件）。初始化改由 StarryInitProvider 完成。
    print('      keep base appComponentFactory (no inject)')
else:
    i = t.find('<application'); j = t.find('>', i)
    if i < 0 or j < 0:
        raise SystemExit('no <application>')
    t = t[:j] + '\n        android:appComponentFactory="%s"' % FACTORY + t[j:]
    print('      appComponentFactory injected')
perms = []
for perm, key in (('SYSTEM_ALERT_WINDOW', 'android.permission.SYSTEM_ALERT_WINDOW'),
                  ('FOREGROUND_SERVICE', 'android.permission.FOREGROUND_SERVICE'),
                  ('FOREGROUND_SERVICE_SPECIAL_USE', 'android.permission.FOREGROUND_SERVICE_SPECIAL_USE')):
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
p.write_text(t, encoding='utf-8')
print('      manifest patched')
PY
  rm -f "$WORK/mf_only.apk"   # Win: apktool 回编末尾 rename 到已存在文件会报 Unable to rename temporary file
  "$JAVA" -jar "$APKTOOL_JAR" b -f "$MF_DEC" -o "$WORK/mf_only.apk" >"$WORK/apktool_b.log" 2>&1 \
    || { tail -20 "$WORK/apktool_b.log" >&2; die "apktool b 失败"; }
  "$PY" - "$WORK/mf_only.apk" "$CACHE_MF" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
data = z.read('AndroidManifest.xml')
open(sys.argv[2], 'wb').write(data)
print('      manifest %d B' % len(data))
PY
else
  echo "      缓存命中 $CACHE_MF ($(wc -c <"$CACHE_MF" | tr -d ' ') B)"
fi

echo "[3/5] 外科组装（其余条目字节保真）"
"$PY" - "$BASE_APK" "$FINAL" "$CACHE_MF" "$DEX_SRC" "$SO_SRC" "$LIBCXX_SRC" "$KYZT_CLASSES6_DEX" "$DEX_ADD_NAME" "$KYZT_KILLER_DEX" "$KILLER_DEX_NAME" <<'PY'
import sys, zipfile, os
base, out, mf, dex, so, cxx, c6, dex_name, killer, killer_name = sys.argv[1:11]   # dex_name 由脚本钉死（源文件名可以是 out_classes8.dex）
OFFICIAL_DEX = set(["classes.dex"] + ["classes%d.dex" % i for i in range(2, 8)])  # 3.3 底包 7 dex（.dex..classes7）
ADD = {
    'AndroidManifest.xml': mf,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
if os.path.basename(dex) != dex_name:
    print('      note: 源 dex %s → 入包名 %s' % (os.path.basename(dex), dex_name))
if dex_name in OFFICIAL_DEX:
    raise SystemExit('拒绝覆盖官方 dex: %s' % dex_name)
ADD[dex_name] = dex          # 框架（classes8.dex）
ADD['classes6.dex'] = c6      # 改包法：3.3 四个 Tap 锚点全在这一个官方 dex 里
if killer:                     # C# 侧反篡改（VerifyCurrentSigner）⇒ 装机版需要 Java 侧回灌官方 DER
    if killer_name in OFFICIAL_DEX or killer_name == dex_name:
        raise SystemExit('killer dex 名非法/撞号: %s' % killer_name)
    ADD[killer_name] = killer
with zipfile.ZipFile(base) as zi, zipfile.ZipFile(out, 'w', compresslevel=9) as zo:
    names = zi.namelist()
    for n in names:
        info = zi.getinfo(n)
        data = ADD[n] and open(ADD[n], 'rb').read() if n in ADD else zi.read(n)
        # 保留底包原始压缩方式（3.3 底包 extractNativeLibs=true，官方 so 是 deflate；
        # 强行 STORED 会把 66 MB libil2cpp 撑回原尺寸，实测多出 70 MB 量级）
        comp = info.compress_type
        ni = zipfile.ZipInfo(n, date_time=info.date_time)
        ni.compress_type = comp
        ni.external_attr = info.external_attr
        zo.writestr(ni, data)
    for n, path in ADD.items():
        if n in names:
            continue
        comp = zipfile.ZIP_STORED if n.endswith('.so') else zipfile.ZIP_DEFLATED
        ni = zipfile.ZipInfo(n, date_time=(1981, 1, 1, 1, 1, 2))
        ni.compress_type = comp
        zo.writestr(ni, open(path, 'rb').read())
print('      assembled %s' % out)
PY

echo "[4/5] 校验：官方 dex 逐字节未变 / 序号连续 1..8（或 1..9，视 killer）/ 新增条目就位"
"$PY" - "$BASE_APK" "$FINAL" "$WORK/base_dex.json" "$DEX_ADD_NAME" "$KYZT_CLASSES6_DEX" "$KYZT_KILLER_DEX" "$KILLER_DEX_NAME" <<'PY'
import hashlib, json, sys, zipfile
base, out, basejson, dexname, c6, killer, killer_name = sys.argv[1:8]
want = json.load(open(basejson))
with zipfile.ZipFile(out) as z:
    got = {n: hashlib.sha256(z.read(n)).hexdigest()
           for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')}
    names = set(z.namelist())
    so = z.read('lib/arm64-v8a/libstarry_magic.so')
    mf = z.read('AndroidManifest.xml')
# dex 序号必须连续（ART 遇缺口即停）：classes.dex 记 1；末位 = 8（无 killer）/ 9（有 killer）
nums = sorted([1] + [int(n[7:-4]) for n in got if n != 'classes.dex'])
want_last = 9 if killer else 8
if nums != list(range(1, len(nums) + 1)):
    raise SystemExit('FAIL dex 序号不连续（实际 %s）' % nums)
if nums != list(range(1, want_last + 1)):
    raise SystemExit('FAIL dex 序号不是 1..%d（实际 1..%d）⇒ 官方 7 dex + 框架 classes8.dex%s'
                         % (want_last, nums[-1], ' + killer classes9.dex' if killer else '（未注入 killer）'))
patched = {'classes6.dex': open(c6, 'rb').read()}
for n, h in want.items():
    if n in patched:
        continue          # 改包法：这个官方 dex 是刻意改的，改由下面的补丁断言把关
    if got.get(n) != h:
        raise SystemExit('FAIL 官方 dex 被改动: %s' % n)
if dexname not in got:
    raise SystemExit('FAIL 缺新增 dex: %s' % dexname)
for need in ('lib/arm64-v8a/libstarry_magic.so', 'lib/arm64-v8a/libc++_shared.so'):
    if need not in names:
        raise SystemExit('FAIL 缺 %s' % need)
for name, d in sorted(patched.items()):
    if got.get(name) != hashlib.sha256(d).hexdigest():
        raise SystemExit('FAIL %s 不是给定改包法产物（字节不符）' % name)
    if got[name] == want.get(name):
        raise SystemExit('FAIL %s 与底包逐字节相同（补丁没生效）' % name)
    print('      OK 改包法 %s: %s' % (name, hashlib.sha256(d).hexdigest()[:16]))
for nb in (b'GatekeeperRepository', b'DefaultTapLoginFragmentProvider', b'TapSignKit', b'PrivacyActivity'):
    if nb not in patched['classes6.dex']:
        raise SystemExit('FAIL classes6.dex 缺 %s（装错 dex？）' % nb.decode())
if killer:
    if killer_name not in got:
        raise SystemExit('FAIL 缺 killer dex: %s' % killer_name)
    k = open(killer, 'rb').read()
    if got[killer_name] != hashlib.sha256(k).hexdigest():
        raise SystemExit('FAIL killer dex 字节不符（入包内容 ≠ 给定文件）')
    if b'bin/mt/signature/KillerApplication193' not in k:
        raise SystemExit('FAIL killer dex 缺 bin/mt/signature/KillerApplication193')
    if b'com.xf.kyzt' not in k:
        raise SystemExit('FAIL killer dex 未烘死包名 com.xf.kyzt')
    print('      OK killer %s: %d B sha256 %s（字节与给定文件一致；sha256 不作门禁判据）'
          % (killer_name, len(k), hashlib.sha256(k).hexdigest()))
if b'KYZT' not in so:
    raise SystemExit('FAIL so 内无 KYZT 标签（装错 so？）')
for bad in (b'libwbmod', b'libtoolChecker'):
    if bad in so:
        raise SystemExit('FAIL so 内含 %s（旧版 so 装错）' % bad.decode())
if not (b'StarryInitProvider' in mf or 'StarryInitProvider'.encode('utf-16-le') in mf):
    raise SystemExit('FAIL manifest 缺 StarryInitProvider')
print('      OK 官方 dex 未变: %s' % ', '.join(sorted(n for n in want if n not in patched)))
print('      OK dex 序号连续 1..%d（%s）' % (want_last, ' '.join(sorted(got, key=lambda t: 1 if t == 'classes.dex' else int(t[7:-4])))))
print('      OK 替换/新增: classes6.dex（改包法）+ %s（框架）+ 2 个 so%s'
      % (dexname, ' + killer %s' % killer_name if killer else '（未注入 killer dex）'))
PY

echo "[5/5] 签名（starry_release.jks / alias=starry，与 gradle_cycle.sh 同源口径）"
KS="${NIXIANG}/01_tools/starry_release.jks"
KS_ALIAS="${KS_ALIAS:-starry}"
KS_PASS="${KS_PASS:-starry2026}"
if [ -n "${NIXIANG_APKSIGNER:-}" ] && [ -f "$KS" ]; then
  "${NIXIANG_APKSIGNER}" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
    --out "$SIGNED" "$FINAL" >"$WORK/sign.log" 2>&1 \
    || { tail -10 "$WORK/sign.log" >&2; die "签名失败"; }
  "${NIXIANG_APKSIGNER}" verify "$SIGNED" >/dev/null && echo "      signed OK"
else
  echo "      WARN 无 apksigner/缺 ks，仅输出未签名包"
  cp -f "$FINAL" "$SIGNED"
fi

echo "DONE"
echo "  unsigned: $FINAL ($(wc -c <"$FINAL" | tr -d ' ') B, sha256 $(sha_of "$FINAL"))"
[ -f "$SIGNED" ] && echo "  signed  : $SIGNED ($(wc -c <"$SIGNED" | tr -d ' ') B, sha256 $(sha_of "$SIGNED"))"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  echo "装机：请显式执行（本脚本默认不装）"
  echo "  bash 04_scripts/_common/install_starry_apk.sh --serial $SERIAL --apk $SIGNED --pkg $PKG"
fi
