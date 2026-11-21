#!/usr/bin/env bash
# 冒险与征途 B站渠道 2.0.1 / vc201 — 星光外壳组装（外科 zip，不整包重打包）
#
# 底包：02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk
#       （com.my.mxyzt.bilibili / 2.0.1 / vc201 / arm64-v8a / md5 56128ac55012c7a9d3cd881776777170 / 907,126,398 B）
# 注入（五件，全部为「替换或新增」单条目）：
#   1) classes4.dex                      新增（底包 classes/2/3 已占 ⇒ 最小空闲下标 4）
#   2) lib/arm64-v8a/libstarry_magic.so  新增（libs_mxyztbili/arm64-v8a/，mxyztbili_build_so.sh）
#   3) lib/arm64-v8a/libc++_shared.so    新增（底包无此 so，必须补；NDK 27 那份）
#   5) AndroidManifest.xml               替换（patch_mxyztbili_binary_manifest.py 产出：application@name→StarryApp、
#                                        +StarryInitProvider、+MenuService、+FGS 权限；appComponentFactory 不动）
#
# ★路线（任务书 E）：本包**替换** application@name 为 com.starry.magic.StarryApp，
#   其父类由 dex 侧补丁指向官方 com.hdwj.afk.MainApplication；provider/service 同时保留双保险。
#   （1.5.9/TapTap 当时走 provider-only，二者差异见 patch_mxyztbili_binary_manifest.py 头注释）
#
# 用法：
#   bash 04_scripts/冒险与征途/surgical_mxyztbili_2.0.1.sh            # StarryApp 路线，出包 + 签名
#   SKIP_SIGN=1 bash 04_scripts/冒险与征途/surgical_mxyztbili_2.0.1.sh  # 只出未签名包
#   MXYZTBILI_PROVIDER_ONLY=1 bash 04_scripts/冒险与征途/surgical_mxyztbili_2.0.1.sh
#     ⇒ provider-only 变体（不改 application@name，= 1.5.9 路线），产物名带 -provideronly-
# 产物：
#   08_output/冒险与征途/冒险与征途_2.0.1_bili_starry-unsigned.apk   （zipalign 后、未签名）
#   08_output/冒险与征途/冒险与征途_2.0.1_bili_starry-signed.apk     （apksigner v1/v2/v3）
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=/dev/null
source "$SCRIPT_DIR/../_common/env.sh"

NIXIANG="$NIXIANG_ROOT"
STAR="$NIXIANG/mod菜单/starry-mod-menu"
PKG="com.my.mxyzt.bilibili"
ORIG_APK="$NIXIANG/02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk"
ORIG_SIZE=907126398
ORIG_MD5="56128ac55012c7a9d3cd881776777170"
OUT_DIR="$NIXIANG/08_output/冒险与征途"
# 两条路线并存（2026-11-16 lead 决策②：不覆盖已产出件）：
#   默认            = StarryApp 变体（application@name → com.starry.magic.StarryApp）
#   MXYZTBILI_PROVIDER_ONLY=1 = provider-only 变体（application@name 保持官方 MainApplication，
#                    只补 StarryInitProvider/MenuService/权限 = 1.5.9 已交付并真机验收过的路线）
PO_ONLY="${MXYZTBILI_PROVIDER_ONLY:-0}"
if [ "$PO_ONLY" = "1" ]; then
  ROUTE="provider-only"
  ROUTE_TAG="-provideronly"
  export MXYZTBILI_KEEP_APP=1
else
  ROUTE="starryapp"
  ROUTE_TAG=""
fi
# 版本后缀（2026-11-16 v2 起）：MXYZTBILI_OUT_SUFFIX=_v2 ⇒ 产新包、不覆盖旧包
OUT_SUFFIX="${MXYZTBILI_OUT_SUFFIX:-}"
UNSIGNED="$OUT_DIR/冒险与征途_2.0.1_bili_starry${ROUTE_TAG}${OUT_SUFFIX}-unsigned.apk"
SIGNED="$OUT_DIR/冒险与征途_2.0.1_bili_starry${ROUTE_TAG}${OUT_SUFFIX}-signed.apk"
TPL_DIR="$STAR/templates/mxyztbili_menu_current"
DEX_NAME="classes4.dex"
DEX_SRC="$TPL_DIR/$DEX_NAME"
SO_SRC="$STAR/libs_mxyztbili/arm64-v8a/libstarry_magic.so"
CXX_SRC="$STAR/libs_mxyztbili/arm64-v8a/libc++_shared.so"
if [ "$PO_ONLY" = "1" ]; then
  MF_OUT="$NIXIANG/07_patched/冒险与征途bili/surgical_provideronly/AndroidManifest.xml"
else
  MF_OUT="$NIXIANG/07_patched/冒险与征途bili/surgical/AndroidManifest.xml"
fi
SCRATCH="$NIXIANG/13_scratch/mxyztbili_build"
PATCHER="$NIXIANG/04_scripts/冒险与征途/patch_mxyztbili_binary_manifest.py"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
CERT_SHA256="83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d"
CERT_SHA1="821c92e404ab5fdac8dbf157c2e467e71440917d"
PY="${NIXIANG_PYTHON:-python3}"
ZIPALIGN="${NIXIANG_ZIPALIGN:-$HOME/Library/Android/sdk/build-tools/36.0.0/zipalign}"
APKSIGNER="${NIXIANG_APKSIGNER:-$HOME/Library/Android/sdk/build-tools/36.0.0/apksigner}"
AAPT2="$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2"

die() { echo "ERROR: $*" >&2; exit 1; }
say() { printf '%s\n' "$*"; }
md5f() { md5 -q "$1"; }
sha256f() { shasum -a 256 "$1" | awk '{print $1}'; }

say "[1/8] 输入校验 route=$ROUTE"
say "      ★Manifest 路线：${ROUTE}（provider-only = 不改 application@name；starryapp = 改成 StarryApp）"
[ -f "$ORIG_APK" ] || die "缺底包 $ORIG_APK"
osize="$(wc -c <"$ORIG_APK" | tr -d ' ')"
[ "$osize" = "$ORIG_SIZE" ] || die "底包体积不符：期望 $ORIG_SIZE 实得 $osize"
omd5="$(md5f "$ORIG_APK")"
[ "$omd5" = "$ORIG_MD5" ] || die "底包 md5 不符：期望 $ORIG_MD5 实得 $omd5"
[ -f "$DEX_SRC" ] || die "缺菜单 dex ${DEX_SRC}（先跑 mxyztbili_build_dex.sh）"
[ -f "$SO_SRC" ]  || die "缺 so ${SO_SRC}（先跑 mxyztbili_build_so.sh）"
[ -f "$CXX_SRC" ] || die "缺 libc++_shared.so ${CXX_SRC}（mxyztbili_build_so.sh 会装到 libs_mxyztbili）"
[ -f "$ZIPALIGN" ] || die "缺 zipalign $ZIPALIGN"
[ -f "$APKSIGNER" ] || [ -x "$APKSIGNER" ] || die "缺 apksigner $APKSIGNER"
say "      底包 $osize B md5=$omd5"
say "      dex  $(wc -c <"$DEX_SRC" | tr -d ' ') B sha256=$(sha256f "$DEX_SRC")"
say "      so   $(wc -c <"$SO_SRC" | tr -d ' ') B md5=$(md5f "$SO_SRC")"
say "      cxx  $(wc -c <"$CXX_SRC" | tr -d ' ') B"

# 菜单 dex 新鲜度（查10 同口径）
bash "$NIXIANG/04_scripts/_common/menu_tpl_gate.sh" --tpl "$TPL_DIR" --dex-name "$DEX_NAME" \
  || die "菜单模板过期 ⇒ 先跑 bash 04_scripts/冒险与征途/mxyztbili_build_dex.sh"

# so 菜单串 + 渠道 tag（字节级）
for s in 'Toggle_秒杀小怪' 'Toggle_无敌' 'Toggle_全模式技能伤害增加' 'Toggle_免广告'; do
  c="$(LC_ALL=C grep -ac "$s" "$SO_SRC" || true)"
  [ "$c" = "1" ] || die "so 缺菜单串 ${s}（grep -c=${c}）"
done
LC_ALL=C grep -aq 'mxyztbili' "$SO_SRC" || die "so 缺 mxyztbili 标签"
LC_ALL=C grep -aq 'libil2cpp.so' "$SO_SRC" || die "so 缺 libil2cpp.so 等待库名"

say "[2/8] 生成补丁 Manifest（从底包现解，幂等可重跑）"
mkdir -p "$SCRATCH" "$(dirname "$MF_OUT")"
"$PY" - "$ORIG_APK" "$SCRATCH/manifest_raw.xml" <<'PY'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z:
    open(sys.argv[2], 'wb').write(z.read('AndroidManifest.xml'))
print("      raw AndroidManifest.xml -> %s" % sys.argv[2])
PY
"$PY" "$PATCHER" "$SCRATCH/manifest_raw.xml" "$MF_OUT" || die "Manifest 补丁失败"

say "[3/8] 组装（python zipfile；官方条目字节保真，丢弃旧签名条目）"
mkdir -p "$OUT_DIR"
WORK="$SCRATCH/assemble"; rm -rf "$WORK"; mkdir -p "$WORK"
"$PY" - "$ORIG_APK" "$WORK/merged.apk" "$MF_OUT" "$DEX_SRC" "$SO_SRC" "$CXX_SRC" "$DEX_NAME" <<'PY'
import sys, zipfile, os
orig, out, mf, dex, so, cxx, dexname = sys.argv[1:8]
REPL = {
    'AndroidManifest.xml': mf,
    'classes4.dex': dex,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}
def is_sig(name):
    u = name.upper()
    if u == 'META-INF/MANIFEST.MF': return True
    return name.startswith('META-INF/') and u.endswith(('.SF', '.RSA', '.DSA', '.EC'))

mfb = open(mf, 'rb').read()
def has(s): return s.encode('utf-16-le') in mfb
keep_app = os.environ.get('MXYZTBILI_KEEP_APP', '') in ('1', 'true', 'yes')
app_req = 'com.hdwj.afk.MainApplication' if keep_app else 'com.starry.magic.StarryApp'
for s in (app_req, 'com.starry.magic.StarryInitProvider',
          'com.starry.magic.menu.MenuService', 'androidx.core.app.CoreComponentFactory'):
    if not has(s): sys.exit('manifest assertion failed: missing %s' % s)
print("[merge] route=%s application@name 期望=%s" % ('provider-only' if keep_app else 'starryapp', app_req))

zin = zipfile.ZipFile(orig, 'r')
names = zin.namelist()
for n in ('classes4.dex',):
    if n in names: sys.exit('底包已存在 %s（dex 下标判断错）' % n)
for n in REPL:
    pass
n_sig = 0; copied = []; replaced = []; added = []
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, allowZip64=True) as zout:
    for item in zin.infolist():
        n = item.filename
        if is_sig(n):
            n_sig += 1; continue
        zi = zipfile.ZipInfo(n, date_time=item.date_time)
        zi.external_attr = item.external_attr; zi.internal_attr = item.internal_attr
        zi.create_system = item.create_system; zi.comment = item.comment
        zi.compress_type = item.compress_type
        if n in REPL:
            zout.writestr(zi, open(REPL[n], 'rb').read()); replaced.append(n); continue
        zout.writestr(zi, zin.read(n)); copied.append(n)
    for n, p in sorted(REPL.items()):
        if n in names: continue
        zi = zipfile.ZipInfo(n, date_time=(2026, 11, 16, 0, 0, 0))
        zi.external_attr = 0o100644 << 16; zi.create_system = 3
        zi.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(zi, open(p, 'rb').read()); added.append(n)

z2 = zipfile.ZipFile(out, 'r')
bad = [n for n in copied if zin.getinfo(n).CRC != z2.getinfo(n).CRC or zin.getinfo(n).file_size != z2.getinfo(n).file_size]
if bad: sys.exit('passthrough mismatch (%d), first=%s' % (len(bad), bad[0]))
print("[merge] orig=%d copied=%d replaced=%d added=%d dropped_sig=%d" % (len(names), len(copied), len(replaced), len(added), n_sig))
print("[merge] passthrough CRC verified: %d entries" % len(copied))
print("[merge] replaced: %s" % ','.join(replaced))
print("[merge] added: %s" % ','.join(added))
print("[merge] out size = %d B" % os.path.getsize(out))
PY

say "[4/8] zipalign -f -p 4 → unsigned"
rm -f "$UNSIGNED"
"$ZIPALIGN" -f -p 4 "$WORK/merged.apk" "$UNSIGNED" || die "zipalign 失败"
say "      $(wc -c <"$UNSIGNED" | tr -d ' ') B  md5=$(md5f "$UNSIGNED")"

if [ "${SKIP_SIGN:-0}" = "1" ]; then
  say "[5/8] SKIP_SIGN=1 ⇒ 跳过签名，产物：$UNSIGNED"
  exit 0
fi

say "[5/8] apksigner（CN=StarryMagic，v1/v2/v3）"
[ -f "$KS" ] || die "缺签名密钥 ${KS}（不得新建/替换密钥）"
rm -f "$SIGNED"
"$APKSIGNER" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$UNSIGNED" || die "apksigner 失败"

say "[6/8] 验签"
CERTINFO="$("$APKSIGNER" verify --print-certs "$SIGNED" | head -8)"
printf '%s\n' "$CERTINFO"
printf '%s' "$CERTINFO" | LC_ALL=C grep -qi "$CERT_SHA256" || die "证书 SHA-256 不符"
printf '%s' "$CERTINFO" | LC_ALL=C grep -qi "$CERT_SHA1" || die "证书 SHA-1 不符"

say "[7/8] 结构自检（APK 内）"
"$PY" - "$SIGNED" "$NIXIANG" <<'PY'
import sys, zipfile, hashlib, re
apk, root = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
names = z.namelist()
base = zipfile.ZipFile(root + '/02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk')
def md5(b): return hashlib.md5(b).hexdigest()
for n in ('classes.dex', 'classes2.dex', 'classes3.dex'):
    a, b = md5(base.read(n)), md5(z.read(n))
    print("  %-14s base=%s apk=%s %s" % (n, a[:12], b[:12], 'OK' if a == b else 'CHANGED!'))
    if a != b: sys.exit('底包 dex 被改动: ' + n)
dexes = sorted([n for n in names if re.fullmatch(r'classes\d*\.dex', n)])
print("  dex 条目:", dexes)
so = z.read('lib/arm64-v8a/libstarry_magic.so')
print('  libstarry_magic.so %d B md5=%s' % (len(so), md5(so)))
print('  mxyztbili 命中:', len(re.findall(b'mxyztbili', so)))
print('  libc++_shared.so', len(z.read('lib/arm64-v8a/libc++_shared.so')), 'B')
print('  classes4.dex', len(z.read('classes4.dex')), 'B md5=%s' % md5(z.read('classes4.dex')))
print('  total entries', len(names))
PY

say "[8/8] aapt2 badging（包名/版本不得被改坏）"
"$AAPT2" dump badging "$SIGNED" | head -3

say "DONE"
say "  unsigned: $UNSIGNED"
say "  signed  : $SIGNED  ($(wc -c <"$SIGNED" | tr -d ' ') B)"
say "  md5     : $(md5f "$SIGNED")"
say "  sha256  : $(sha256f "$SIGNED")"
