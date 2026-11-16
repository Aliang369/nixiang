#!/usr/bin/env bash
# ================================================================
#  旷野征途 (com.xf.kyzt) 2.4 / vc24 — 星光外科组装（只构建，不含装机）
#
#  底包：02_apks/旷野征途/旷野征途_2.4_taptap_official.apk
#        sha256 8d677670…e484e / 194,003,543 B / signer O=xiefeng
#        （只读拉取产物；取证见 10_docs/旷野征途/底包取证_2.4_20261116.md）
#
#  与 2.2 版（surgical_kyzt_2.2.sh）的差异（**只改序号与补丁输入，其余门禁原样**）：
#   · 2.4 是 **8 dex 包**（classes.dex..classes8.dex），TapSDK 锚点分裂到
#     classes6.dex（initializer / gatekeeper）与 classes7.dex（login / kit）
#     ⇒ 改包法要**替换两个官方 dex**（2.2 只替换 classes.dex）。
#   · **框架 dex 序号 = classes9.dex，killer dex 序号 = classes10.dex**
#     （2.2 是 classes3 / classes4；ART 遇缺口即停，1..10 必须连续无缺口）。
#   · classes.dex..classes5.dex 与 classes8.dex **逐字节不动**（含 com.xf.kyzt 的 R 类）。
#   · manifest 改动与 2.2 完全相同（StarryInitProvider + MenuService + 3 个权限；
#     **不加 android:name、不换 appComponentFactory**）——2.4 底包 application 同样
#     无 android:name，killer 由 StarryInitProvider → SignatureKillerBootstrap 的
#     Class.forName(..., initialize=true) 触发，不依赖 manifest 属性。
#   · 默认**只构建**；装机是独立动作：STARRY_INSTALL=1 才会提示 install（默认关）。
#
#  用法：
#    KYZT_CLASSES6_DEX=… KYZT_CLASSES7_DEX=… KYZT_KILLER_DEX=… bash 04_scripts/旷野征途/surgical_kyzt_2.4.sh
#    STARRY_INSTALL=1 SERIAL=f9a116a2 bash ...               # 构建 + 提示装机（需另行授权）
# ================================================================
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "${HERE}/../.." && pwd)"
# shellcheck source=/dev/null
. "${NIXIANG}/04_scripts/_common/env.sh"

STAR="${NIXIANG}/mod菜单/starry-mod-menu"
GAME="旷野征途"
VERSION="2.4"
PKG="com.xf.kyzt"
AUTHORITY="${PKG}.starryinit"
SERIAL="${SERIAL:-f9a116a2}"

BASE_APK="${KYZT_BASE_APK:-${NIXIANG}/02_apks/旷野征途/旷野征途_${VERSION}_taptap_official.apk}"
# 默认钉官方底包；仅在「外层自己生成并把关过中间产物」时用 KYZT_BASE_SHA256_EXPECT 覆盖（会打印告警）
BASE_SHA256_EXPECT="${KYZT_BASE_SHA256_EXPECT:-8d6776704bcaa6bc358e0a4e63eab16d57a38ced518db694485b32f3a17e484e}"

DEX_ADD_NAME="classes9.dex"
TPL_DIR="${STAR}/templates/kyzt_menu_current"
DEX_SRC="${TPL_DIR}/${DEX_ADD_NAME}"
# 可选输入（2.4 改包法：锚点分裂到两个官方 dex；依据
# 10_docs/旷野征途/静态_Java_Tap改包_2.4_20261116.md §②/§⑤）：
#   KYZT_CLASSES6_DEX = 官方 classes6.dex 改包产物（#1 TapTapSdkInitializer / #2 GatekeeperRepository）
#   KYZT_CLASSES7_DEX = 官方 classes7.dex 改包产物（#3 DefaultTapLoginFragmentProvider / #4 TapSignKit）
#   KYZT_KILLER_DEX   = MT 签名杀手 dex（bin.mt.signature.KillerApplication193，回灌官方证书 DER）
KYZT_CLASSES6_DEX="${KYZT_CLASSES6_DEX:-}"
KYZT_CLASSES7_DEX="${KYZT_CLASSES7_DEX:-}"
KYZT_KILLER_DEX="${KYZT_KILLER_DEX:-}"
KILLER_DEX_NAME="${KYZT_KILLER_DEX_NAME:-classes10.dex}"
# so 用「2.2 起沿用的构建输出目录」app/src/libs_kyzt（2.4 就地产出覆盖同名文件），1.8 的 libs_kyzt 原样保留
SO_SRC="${STAR}/app/src/libs_kyzt/arm64-v8a/libstarry_magic.so"
LIBCXX_SRC="${STAR}/libs_kyzt/arm64-v8a/libc++_shared.so"

OUT_DIR="${NIXIANG}/08_output/${GAME}"
FINAL="${OUT_DIR}/${GAME}_${VERSION}_starry-unsigned.apk"
SIGNED="${OUT_DIR}/${GAME}_${VERSION}_starry-releaseSigned.apk"
WORK="${NIXIANG}/07_patched/${GAME}/surgical_2.4"
CACHE_MF="${WORK}/AndroidManifest.xml"

PY="${NIXIANG_PYTHON:-python3}"
JAVA="${NIXIANG_JAVA17}/bin/java"
APKTOOL_JAR="${NIXIANG_ROOT:-$NIXIANG}/01_tools/apktool/apktool.jar"
[ -f "$APKTOOL_JAR" ] || APKTOOL_JAR="${NIXIANG}/01_tools/apktool/apktool_2.9.3.jar"
[ -f "$APKTOOL_JAR" ] || APKTOOL_JAR="$(command -v apktool >/dev/null 2>&1 && echo '')"

die() { echo "ERROR: $*" >&2; exit 1; }
sha_of() { (shasum -a 256 "$1" | awk '{print $1}'); }

[ -f "$BASE_APK" ] || die "底包不存在: $BASE_APK"
BSHA="$(sha_of "$BASE_APK")"
[ "$BSHA" = "$BASE_SHA256_EXPECT" ] || die "底包 sha256 不符（期望 ${BASE_SHA256_EXPECT}，实得 ${BSHA}）"
[ -n "${KYZT_BASE_SHA256_EXPECT:-}" ] && echo "      ⚠ 底包指纹由 KYZT_BASE_SHA256_EXPECT 覆盖（非官方原包，用于改包法产物）"
[ -f "$DEX_SRC" ] || die "缺 ${DEX_ADD_NAME}（先跑 KYZT_DEX_NAME=${DEX_ADD_NAME} bash 04_scripts/旷野征途/kyzt_build_dex.sh）"
if [ -n "$KYZT_CLASSES6_DEX" ]; then
  [ -f "$KYZT_CLASSES6_DEX" ] || die "缺改包法 classes6.dex: $KYZT_CLASSES6_DEX"
fi
if [ -n "$KYZT_CLASSES7_DEX" ]; then
  [ -f "$KYZT_CLASSES7_DEX" ] || die "缺改包法 classes7.dex: $KYZT_CLASSES7_DEX"
fi
if [ -n "$KYZT_KILLER_DEX" ]; then
  [ -f "$KYZT_KILLER_DEX" ] || die "缺 killer dex: $KYZT_KILLER_DEX"
  case "$KILLER_DEX_NAME" in
    classes*.dex) : ;;
    *) die "KILLER_DEX_NAME 命名非法: $KILLER_DEX_NAME" ;;
  esac
  [ "$KILLER_DEX_NAME" = "$DEX_ADD_NAME" ] && die "killer dex 名与菜单 dex 撞号"
  case "$KILLER_DEX_NAME" in
    classes6.dex|classes7.dex) die "killer dex 名与官方改包 dex 撞号: $KILLER_DEX_NAME" ;;
  esac
fi
[ -f "$SO_SRC" ] || die "缺 so: ${SO_SRC}（先 ndk-build Android_kyzt.mk）"
[ -f "$LIBCXX_SRC" ] || die "缺 libc++_shared.so: $LIBCXX_SRC"

echo "[0/5] 输入核对"
echo "      底包 $BASE_APK"
echo "      sha256 $BSHA (OK)"
echo "      dex  $DEX_ADD_NAME $(wc -c <"$DEX_SRC" | tr -d ' ') B $(sha_of "$DEX_SRC")"
echo "      so   $(wc -c <"$SO_SRC" | tr -d ' ') B $(sha_of "$SO_SRC")"

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
  "$JAVA" -jar "$APKTOOL_JAR" d -f -s --no-assets -o "$WORK/mf_dec" "$BASE_APK" >"$WORK/apktool_d.log" 2>&1 \
    || { tail -20 "$WORK/apktool_d.log" >&2; die "apktool d 失败"; }
  "$PY" - "$WORK/mf_dec/AndroidManifest.xml" "$AUTHORITY" <<'PY'
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
  "$JAVA" -jar "$APKTOOL_JAR" b -f "$WORK/mf_dec" -o "$WORK/mf_only.apk" >"$WORK/apktool_b.log" 2>&1 \
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
"$PY" - "$BASE_APK" "$FINAL" "$CACHE_MF" "$DEX_SRC" "$SO_SRC" "$LIBCXX_SRC" "$KYZT_CLASSES6_DEX" "$KYZT_CLASSES7_DEX" "$KYZT_KILLER_DEX" "$KILLER_DEX_NAME" <<'PY'
import sys, zipfile, shutil
base, out, mf, dex, so, cxx, c6, c7, killer, killer_name = sys.argv[1:11]
OFFICIAL_DEX = set(["classes.dex"] + ["classes%d.dex" % i for i in range(2, 9)])  # 2.4 底包 8 dex
ADD = {
    'AndroidManifest.xml': mf,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
import os
dex_name = os.path.basename(dex)
if dex_name in OFFICIAL_DEX:
    raise SystemExit('拒绝覆盖官方 dex: %s' % dex_name)
ADD[dex_name] = dex
if c6:
    ADD['classes6.dex'] = c6      # 改包法 #1/#2（TapTapSdkInitializer / GatekeeperRepository）
if c7:
    ADD['classes7.dex'] = c7      # 改包法 #3/#4（DefaultTapLoginFragmentProvider / TapSignKit）
if killer:
    if killer_name in OFFICIAL_DEX or killer_name == dex_name:
        raise SystemExit('killer dex 名非法/撞号: %s' % killer_name)
    ADD[killer_name] = killer                  # B 轨：MT 签名杀手（官方证书 DER）
with zipfile.ZipFile(base) as zi, zipfile.ZipFile(out, 'w', compresslevel=9) as zo:
    names = zi.namelist()
    for n in names:
        info = zi.getinfo(n)
        data = ADD[n] and open(ADD[n], 'rb').read() if n in ADD else zi.read(n)
        # 保留底包原始压缩方式（2.2 底包 extractNativeLibs=true，官方 so 是 deflate；
        # 强行 STORED 会把 65 MB libil2cpp 从 18 MB 撑到 65 MB，实测多出 75 MB）
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

echo "[4/5] 校验：官方 dex 逐字节未变 / 序号连续 / 新增条目就位"
"$PY" - "$BASE_APK" "$FINAL" "$WORK/base_dex.json" "$DEX_ADD_NAME" "$KYZT_CLASSES6_DEX" "$KYZT_CLASSES7_DEX" "$KYZT_KILLER_DEX" "$KILLER_DEX_NAME" <<'PY'
import hashlib, json, sys, zipfile
base, out, basejson, dexname, c6, c7, killer, killer_name = sys.argv[1:9]
want = json.load(open(basejson))
with zipfile.ZipFile(out) as z:
    got = {n: hashlib.sha256(z.read(n)).hexdigest()
           for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')}
    names = set(z.namelist())
    so = z.read('lib/arm64-v8a/libstarry_magic.so')
    mf = z.read('AndroidManifest.xml')
# dex 序号必须连续 1..N（ART 遇缺口即停）：classes.dex 记 1
nums = sorted([1] + [int(n[7:-4]) for n in got if n != 'classes.dex'])
if nums != list(range(1, len(nums) + 1)):
    raise SystemExit('FAIL dex 序号不连续（实际 %s）' % nums)
patched = {}
for name, path in (('classes6.dex', c6), ('classes7.dex', c7)):
    if path:
        patched[name] = open(path, 'rb').read()
for n, h in want.items():
    if n in patched:
        continue          # 改包法：这两个官方 dex 是刻意改的，改由下面的补丁断言把关
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
if 'classes6.dex' in patched and b'GatekeeperRepository' not in patched['classes6.dex']:
    raise SystemExit('FAIL classes6.dex 内无 GatekeeperRepository（装错 dex？）')
if 'classes7.dex' in patched:
    for nb in (b'DefaultTapLoginFragmentProvider', b'TapSignKit'):
        if nb not in patched['classes7.dex']:
            raise SystemExit('FAIL classes7.dex 缺 %s（装错 dex？）' % nb.decode())
if killer:
    if killer_name not in got:
        raise SystemExit('FAIL 缺 killer dex: %s' % killer_name)
    k = open(killer, 'rb').read()
    if got.get(killer_name) != hashlib.sha256(k).hexdigest():
        raise SystemExit('FAIL killer dex 字节不符')
    for need_b in (b'bin/mt/signature/KillerApplication193', b'com.xf.kyzt'):
        if need_b not in k:
            raise SystemExit('FAIL killer dex 缺 %s' % need_b.decode())
    if b'nxtOVAnjgXtvKSmcPLPj' in k:
        raise SystemExit('FAIL killer dex 仍指向 1.4 混淆父类（须 android/app/Application）')
    print('      OK killer dex %s: %s' % (killer_name, hashlib.sha256(k).hexdigest()[:16]))
if b'KYZT' not in so:
    raise SystemExit('FAIL so 内无 KYZT 标签（装错 so？）')
if not (b'StarryInitProvider' in mf or 'StarryInitProvider'.encode('utf-16-le') in mf):
    raise SystemExit('FAIL manifest 缺 StarryInitProvider')
print('      OK 官方 dex 未变: %s' % ', '.join(sorted(n for n in want if n not in patched)))
print('      OK dex 序号连续 1..%d（%s）' % (len(nums), ' '.join(sorted(got, key=lambda s: 1 if s == 'classes.dex' else int(s[7:-4])))))
print('      OK 替换/新增: %s%s%s + 2 个 so' % (
    ('classes6 ' if 'classes6.dex' in patched else ''),
    dexname + ' ',
    killer_name if killer else ''))
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
