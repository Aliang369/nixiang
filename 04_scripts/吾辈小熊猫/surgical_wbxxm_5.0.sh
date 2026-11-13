#!/usr/bin/env bash
# script-policy: {"purpose": "吾辈小熊猫 5.0 星光菜单版外科 zip 组装（菜单 dex + killer dex + 改包 classes2.dex + 本游戏 so + Manifest 补丁 + main.js 免广告补丁）并重签，含成品自检", "inputs": "02_apks/吾辈小熊猫/吾辈小熊猫_5.0_taptap.apk、gradle assembleRelease 菜单 APK、libs_wbxxm/arm64-v8a/libstarry_magic.so、04_scripts/吾辈小熊猫/assets/wbxxm_signature_killer.dex、07_patched/吾辈小熊猫/{classes2_patched.dex,assets/main.js}、patch_wbxxm_binary_manifest.py", "outputs": "08_output/吾辈小熊猫/吾辈小熊猫_5.0_starry-releaseSigned.apk（+ 07_patched/吾辈小熊猫/menu_5.0/ 下 Manifest 缓存）", "boundary": "只做外科组装/签名/自检；不装机、不清数据；不改其它 assets、不动 libcocos.so 与 SDK so、不重编 jsc/so 本身；index.jsc 保持底包原版（内购删除后不再打 jsc 补丁）", "prior_art": "04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.9.sh（外科六段骨架）；04_scripts/我在神界刷装备/build_killer_dex_sjzz.sh；04_scripts/_common/next_free_dex.py", "difference": "底包 7 dex 无空洞 ⇒ 新增 classes8.dex(killer)+classes9.dex(菜单)；classes2.dex 改包（killer 父类+门闩+网页登录）；so 按底包惯例 Deflate，libc++_shared.so 用 Stored（NDK r27 aarch64）；含 main.js 一处 JS 补丁（免广告）；index.jsc 保持底包原版不改", "evidence": "10_docs/吾辈小熊猫/星光落地.md"}
# ============================================================================
# 吾辈小熊猫 5.0（com.wbxxm.wbxxm，Cocos Creator 3.7.1，TapTap 渠道）
#   → 星光悬浮菜单版 外科 zip 组装（不装机）
#
# 组装内容（8 类）：
#   1) AndroidManifest.xml            ← StarryInitProvider（application 首个子元素）+ MenuService + 3 权限
#                                         application@name / appComponentFactory 保持官方值不动
#   2) classes2.dex                   ← 改包：XiPuSDKApplication 父类→killer；TapSDK 门闩恒放行；
#                                         toast no-op；checkInitialize→true；isClientLogin→false（强制网页登录）
#   3) classes8.dex（新增）            ← killer（bin.mt.signature.KillerApplication193 + 官方证书伪装）
#   4) classes9.dex（新增）            ← 星光菜单 dex（含 games/wbxxm/GameHookImpl）
#   5) lib/arm64-v8a/libstarry_magic.so（新增，Deflate，与底包其它 .so 一致）
#   6) lib/arm64-v8a/libc++_shared.so  ← 新增，**Stored（不压缩）**；libstarry_magic.so 的 C++ 运行时
#                                        依赖。底包未带 ⇒ 缺它 nativeloader 报 library not found、
#                                        starry_magic dlopen 失败、native 全废。源取 NDK r27 的
#                                        aarch64-linux-android 版本（与菜单 so 同一 NDK ABI）
#   7) assets/main.js                 ← JS 运行时补丁（免广告 / 内购(GM) 分流；无限体力已于 2026-11-13 删除）
#   8) assets/assets/main/index.jsc   ← **不再改**（2026-11-13 内购删除 ⇒ 直接用底包原版，包内 md5 == 底包）
#   ⚠️ 不覆盖 libcocos.so / SDK .so / 其它 assets
#
# 用法：
#   bash 04_scripts/吾辈小熊猫/surgical_wbxxm_5.0.sh
#   可选环境变量：WBXXM_BASE_APK / WBXXM_MENU_APK / WBXXM_SO / WBXXM_SKIP_JS=1
#   本脚本不含装机分支：装机走 04_scripts/_common/install_starry_apk.sh
# ============================================================================
set -eo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="吾辈小熊猫"
VER="5.0"
STEM="吾辈小熊猫_5.0"
PKG="com.wbxxm.wbxxm"
TAG="Wbxxm"
GAME_ID="wbxxm"

MENU_ROOT="$NIXIANG/mod菜单/starry-mod-menu"
BASE="$NIXIANG/02_apks/$GAME/""$STEM""_taptap.apk"
MENU_APK="$MENU_ROOT/app/build/outputs/apk/release/app-release.apk"
SO_SRC="$MENU_ROOT/libs_wbxxm/arm64-v8a/libstarry_magic.so"
KILLER_DEX="$SCRIPT_DIR/assets/wbxxm_signature_killer.dex"
CLASSES2="$NIXIANG/07_patched/$GAME/classes2_patched.dex"
MAINJS="$NIXIANG/07_patched/$GAME/assets/main.js"
BT="$ANDROID_BUILD_TOOLS"
if [ -z "$BT" ] && [ -n "$ANDROID_HOME" ]; then
  BT="$(ls -d "$ANDROID_HOME"/build-tools/* 2>/dev/null | sort -V | tail -1)"
fi
if [ -z "$BT" ] || [ ! -x "$BT/apksigner" ]; then
  BT="$(ls -d "$HOME"/Library/Android/sdk/build-tools/* 2>/dev/null | sort -V | tail -1)"
fi
[ -x "$BT/apksigner" ] || { echo "ERROR: 找不到 build-tools（设 ANDROID_BUILD_TOOLS=<dir>）" >&2; exit 1; }
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="starry"
KS_PASS="starry2026"
KS_CN_EXPECT="StarryMagic"
PY="python3"
SKIP_JS="0"
[ -n "$WBXXM_BASE_APK" ] && BASE="$WBXXM_BASE_APK"
[ -n "$WBXXM_MENU_APK" ] && MENU_APK="$WBXXM_MENU_APK"
[ -n "$WBXXM_SO" ] && SO_SRC="$WBXXM_SO"
[ -n "$WBXXM_KILLER_DEX" ] && KILLER_DEX="$WBXXM_KILLER_DEX"
[ -n "$WBXXM_CLASSES2" ] && CLASSES2="$WBXXM_CLASSES2"
[ -n "$WBXXM_SKIP_JS" ] && SKIP_JS="$WBXXM_SKIP_JS"

MANIFEST_PATCH="$SCRIPT_DIR/patch_wbxxm_binary_manifest.py"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/""$STEM""_starry-unsigned.apk"
SIGNED="$OUT/""$STEM""_starry-releaseSigned.apk"
WORK="$NIXIANG/13_scratch/wbxxm_menu"
MF_DIR="$NIXIANG/07_patched/$GAME/menu_$VER"

die(){ echo "ERROR: $*" >&2; exit 1; }
step(){ echo ""; echo "== $* =="; }
hash1(){ "$PY" -c 'import hashlib,sys;print(hashlib.md5(open(sys.argv[1],"rb").read()).hexdigest())' "$1"; }
fsize(){ wc -c < "$1" | tr -d ' '; }

# libc++_shared.so：libstarry_magic.so 的 C++ 运行时依赖；底包没带，必须补进包，
# 否则 nativeloader "library \"libc++_shared.so\" not found" → starry_magic 加载失败、native 全废。
# 取 NDK r27（libs_wbxxm 里那颗 so 的构建 NDK）的 aarch64-linux-android 版；可用 WBXXM_CXX_SO 覆盖。
CXX_SRC="${WBXXM_CXX_SO:-}"
if [ -z "$CXX_SRC" ]; then
  NDK_ROOT="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk}"
  for nd in "$NDK_ROOT/27.0.12077973" "$NDK_ROOT"/*; do
    if [ -n "$CXX_SRC" ]; then break; fi
    for cand in "$nd"/toolchains/llvm/prebuilt/*/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so; do
      if [ -f "$cand" ]; then CXX_SRC="$cand"; break; fi
    done
  done
fi
[ -n "$CXX_SRC" ] && [ -f "$CXX_SRC" ] || die "找不到 aarch64 libc++_shared.so（设 WBXXM_CXX_SO=<path> 或 ANDROID_NDK_HOME）"

for f in "$BASE" "$MENU_APK" "$SO_SRC" "$CXX_SRC" "$KILLER_DEX" "$CLASSES2" "$MANIFEST_PATCH" "$BT/zipalign" "$BT/apksigner"; do
  [ -e "$f" ] || die "缺输入 $f"
done
if [ "$SKIP_JS" != "1" ]; then
  [ -f "$MAINJS" ] || die "缺 JS 补丁产物 $MAINJS"
fi
mkdir -p "$OUT" "$WORK" "$MF_DIR"

step "0/7 输入自检（so 身份 / killer 身份）"
echo "  base    = $BASE ($(fsize "$BASE") B, md5 $(hash1 "$BASE"))"
echo "  menu    = $MENU_APK ($(fsize "$MENU_APK") B)"
echo "  so      = $SO_SRC ($(fsize "$SO_SRC") B, md5 $(hash1 "$SO_SRC"))"
echo "  libc++  = $CXX_SRC ($(fsize "$CXX_SRC") B, md5 $(hash1 "$CXX_SRC"))"
echo "  killer  = $KILLER_DEX ($(fsize "$KILLER_DEX") B, md5 $(hash1 "$KILLER_DEX"))"
echo "  classes2= $CLASSES2 ($(fsize "$CLASSES2") B, md5 $(hash1 "$CLASSES2"))"
LC_ALL=C grep -aqF "[$TAG]" "$SO_SRC" || die "so 缺 [$TAG] 标签（编错游戏）"
LC_ALL=C grep -aqF "$GAME_ID" "$SO_SRC" || die "so 缺 game_id $GAME_ID"
for m in 'Toggle_免广告'; do
  n=$(LC_ALL=C grep -acF -- "$m" "$SO_SRC" || true)
  [ "$n" -ge 1 ] || die "so 缺菜单串 $m"
done
for bad in '[Qxzxcz]' 'qxzxcz' 'nenlystudio' 'windforce'; do
  n=$(LC_ALL=C grep -acF -- "$bad" "$SO_SRC" || true)
  [ "$n" -eq 0 ] || die "so 里残留上一款游戏的串 $bad"
done
LC_ALL=C grep -aqF 'bin/mt/signature/KillerApplication193' "$KILLER_DEX" || die "killer dex 缺 KillerApplication193"
LC_ALL=C grep -aqF "$PKG" "$KILLER_DEX" || die "killer dex 未烘焙本包名"
LC_ALL=C grep -aqF 'MIIDczCCAlugAwIBAgIEB8L/CjANBgkqhkiG9w0BAQsFADBp' "$KILLER_DEX" || die "killer dex 未烘焙官方证书（首段不匹配）"
n=$(LC_ALL=C grep -acF -- '@@' "$KILLER_DEX" || true)
[ "$n" -eq 0 ] || die "killer dex 残留 @@ 占位符"
echo "  so/killer 身份 + 菜单串 + 无他包残留：OK"

step "1/7 底包结构断言 + Manifest 补丁（从底包现取）"
"$PY" - "$BASE" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = z.namelist()
for n in ('classes8.dex', 'classes9.dex', 'lib/arm64-v8a/libstarry_magic.so',
          'lib/arm64-v8a/libc++_shared.so'):
    assert n not in names, '底包已存在 %s（dex 下标/落位需重新求）' % n
dex = sorted((n for n in names if n.startswith('classes') and n.endswith('.dex')),
             key=lambda s: 1 if s == 'classes.dex' else int(s[7:-4]))
assert dex == ['classes.dex'] + ['classes%d.dex' % i for i in range(2, 8)], '底包 dex 布局变了：%r' % dex
print('  底包 7 dex 连续无空洞；classes8/9、libstarry_magic.so、libc++_shared.so 均空闲')
PY
"$PY" - "$BASE" "$WORK/AndroidManifest.xml" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
data = z.read('AndroidManifest.xml')
open(sys.argv[2], 'wb').write(data)
print('  底包 Manifest %d B' % len(data))
PY
"$PY" "$MANIFEST_PATCH" "$WORK/AndroidManifest.xml" "$MF_DIR/AndroidManifest.xml"
cp -f "$MF_DIR/AndroidManifest.xml" "$WORK/AndroidManifest.patched.xml"

step "2/7 取菜单 dex（classes9.dex）"
"$PY" - "$MENU_APK" "$WORK/classes9.dex" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
names = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')]
assert len(names) == 1, '菜单 APK 期望单 dex，实得 %r' % names
b = z.read(names[0])
open(sys.argv[2], 'wb').write(b)
need = [b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService',
        b'com/starry/magic/core/GameHooks', b'com/starry/magic/games/wbxxm/GameHookImpl',
        b'starry_state.txt']
miss = [n.decode() for n in need if n not in b]
assert not miss, '菜单 dex 缺类/串: %r' % miss
print('  %s %d B md5 %s（InitProvider/MenuService/GameHooks/GameHookImpl/starry_state 均在）'
      % (names[0], len(b), hashlib.md5(b).hexdigest()))
PY

step "3/7 改包 classes2.dex 与 JS 补丁复读"
"$PY" - "$CLASSES2" "$MAINJS" "$SKIP_JS" <<'PY'
import sys, hashlib
c2, mainjs, skip = sys.argv[1], sys.argv[2], sys.argv[3]
b = open(c2, 'rb').read()
assert b'bin/mt/signature/KillerApplication193' in b, 'classes2.dex 未含 killer 父类引用'
assert b'com/taptap/sdk/login/impl/utils/ClientLoginUtils' in b, 'classes2.dex 缺 ClientLoginUtils'
assert b'com/taptap/sdk/initializer/service/InitializerServiceImpl' in b, 'classes2.dex 缺 InitializerServiceImpl'
print('  classes2.dex %d B md5 %s（killer 父类 / ClientLoginUtils / InitializerServiceImpl 均在）'
      % (len(b), hashlib.md5(b).hexdigest()))
if skip != '1':
    t = open(mainjs, 'r', encoding='utf-8').read()
    assert 'starry-mod:wbxxm:v1' in t, 'main.js 补丁块缺失'
    assert 'whenReady' in t and 'EVENT_AFTER_SCENE_LAUNCH' in t, 'main.js 未用 whenReady 延后挂载（黑屏回归）'
    assert 'VideoAdMgr' in t and 'FEAT_AD' in t and 'starry_state.txt' in t, 'main.js 补丁内容不完整'
    for _bad in ('FEAT_GM', 'FEAT_ENERGY', '内购', '无限体力', 'showGMLogin', 'GameStartMgr'):
        assert _bad not in t, 'main.js 仍含已删除功能（内购/无限体力）：%s' % _bad
    assert "System.import('./src/application.js')" in t, 'main.js 原启动链被破坏'
    print('  main.js %d B（补丁块 1 处，原启动链完好）' % len(t.encode()))
PY

step "4/7 外科 zip 重写（保留各条目压缩方式）"
"$PY" - "$BASE" "$WORK/AndroidManifest.patched.xml" "$WORK/classes9.dex" "$SO_SRC" "$CLASSES2" "$KILLER_DEX" "$MAINJS" "$SKIP_JS" "$CXX_SRC" "$UNSIGNED" <<'PY'
import sys, zipfile, os
base, mf, menu_dex, so, c2, killer, mainjs, skip, cxx, out = sys.argv[1:11]
REPLACE = {'AndroidManifest.xml': mf, 'classes2.dex': c2}
if skip != '1':
    REPLACE['assets/main.js'] = mainjs
    # index.jsc 不再替换：内购删除后 jsc 保持底包原版（自检段核 md5 == 底包）
ADD = [('classes8.dex', killer, zipfile.ZIP_DEFLATED),
       ('classes9.dex', menu_dex, zipfile.ZIP_DEFLATED),
       ('lib/arm64-v8a/libstarry_magic.so', so, zipfile.ZIP_DEFLATED),
       # libc++_shared.so 必须 Stored：loader 直接 mmap 未压缩页（zipalign -p 4 会按页对齐）
       ('lib/arm64-v8a/libc++_shared.so', cxx, zipfile.ZIP_STORED)]
zin = zipfile.ZipFile(base)
if os.path.exists(out):
    os.remove(out)
zout = zipfile.ZipFile(out, 'w', allowZip64=True)
swapped, dropped = 0, 0
for info in zin.infolist():
    n = info.filename
    if n.startswith('META-INF/') and (n.endswith('.SF') or n.endswith('.RSA') or n.endswith('.DSA')
                                      or n.endswith('.EC') or n.endswith('/MANIFEST.MF')):
        dropped += 1
        continue
    zi = zipfile.ZipInfo(n, date_time=info.date_time)
    zi.compress_type = info.compress_type
    zi.external_attr = info.external_attr
    if n in REPLACE:
        data = open(REPLACE[n], 'rb').read()
        swapped += 1
        print('  ~ %s (%d B, 替换)' % (n, len(data)))
    else:
        data = zin.read(n)
    zout.writestr(zi, data)
for name, path, ctype in ADD:
    data = open(path, 'rb').read()
    zi = zipfile.ZipInfo(name, date_time=(1981, 1, 1, 1, 1, 2))
    zi.compress_type = ctype
    zi.external_attr = 0o644 << 16
    zout.writestr(zi, data)
    print('  + %s (%d B, %s)' % (name, len(data), 'Stored' if ctype == 0 else 'Deflate'))
zout.close()
print('  条目 %d -> %d；替换 %d；去掉旧签名 %d' %
      (len(zin.infolist()), len(zipfile.ZipFile(out).infolist()), swapped, dropped))
PY

step "5/7 zipalign -p 4 + apksigner（v1+v2+v3，v2 必须开）"
rm -f "$SIGNED" "$OUT/.aligned.apk"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$OUT/.aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true --v4-signing-enabled false --out "$SIGNED" "$OUT/.aligned.apk"
rm -f "$OUT/.aligned.apk" "$UNSIGNED" "$SIGNED.idsig"

step "6/7 成品自检（签名 / 条目完整性 / 功能性字节）"
SIGOUT="$("$BT/apksigner" verify --print-certs "$SIGNED")"
echo "$SIGOUT" | head -5
"$BT/apksigner" verify --verbose "$SIGNED" | grep -E 'Verified using v[123] scheme' || die "签名方案复核失败"
echo "$SIGOUT" | grep -q "CN=$KS_CN_EXPECT" || die "签名主体不是 CN=$KS_CN_EXPECT (keystore 用错?)"
"$PY" - "$SIGNED" "$SO_SRC" "$KILLER_DEX" "$CLASSES2" "$WORK/classes9.dex" "$SKIP_JS" "$CXX_SRC" "$BASE" <<'PY'
import sys, zipfile, hashlib, os
apk, so, killer, c2, menu_dex, skip, cxx, base = sys.argv[1:9]
z = zipfile.ZipFile(apk)
names = z.namelist()
def md5b(b): return hashlib.md5(b).hexdigest()
def md5f(p): return md5b(open(p, 'rb').read())
a = md5b(z.read('lib/arm64-v8a/libstarry_magic.so'))
assert a == md5f(so), '包内 so md5 %s != 构建产物' % a
assert md5b(z.read('classes8.dex')) == md5f(killer), 'classes8.dex 与 killer 源不一致'
assert md5b(z.read('classes9.dex')) == md5f(menu_dex), 'classes9.dex 与菜单 dex 源不一致'
assert md5b(z.read('classes2.dex')) == md5f(c2), 'classes2.dex 与改包源不一致'
assert 'lib/arm64-v8a/libc++_shared.so' in names, '包内缺 lib/arm64-v8a/libc++_shared.so（native 必崩）'
cxx_info = z.getinfo('lib/arm64-v8a/libc++_shared.so')
assert cxx_info.compress_type == zipfile.ZIP_STORED, 'libc++_shared.so 不是 Stored（compress_type=%d）' % cxx_info.compress_type
assert md5b(z.read('lib/arm64-v8a/libc++_shared.so')) == md5f(cxx), '包内 libc++_shared.so 与 NDK 源不一致'
mf = z.read('AndroidManifest.xml')
flat = mf.replace(b'\x00', b'')
for need in (b'com.starry.magic.StarryInitProvider', b'com.starry.magic.menu.MenuService',
             b'com.xipu.msdk.XiPuSDKApplication', b'com.wbxxm.wbxxm'):
    assert need in flat, 'Manifest 缺 %r' % need
assert b'SignSpoof' not in flat, 'Manifest 出现已删除的 SignSpoof'
if skip != '1':
    _mj = z.read('assets/main.js')
    assert b'starry-mod:wbxxm:v1' in _mj, 'main.js 补丁块缺失'
    assert b'whenReady' in _mj and b'EVENT_AFTER_SCENE_LAUNCH' in _mj, '包内 main.js 不是 whenReady 延后版（黑屏回归）'
    # 内购删除后 jsc 必须 == 底包原版（不再打 IsGmLogin 补丁）
    _jsc = z.read('assets/assets/main/index.jsc')
    _base_jsc = zipfile.ZipFile(base).read('assets/assets/main/index.jsc')
    assert md5b(_jsc) == md5b(_base_jsc), '包内 index.jsc 与底包原版不一致（jsc 补丁未撤干净）'
    assert _jsc == _base_jsc, 'index.jsc 字节级不等于底包原版'
    print('  包内 index.jsc == 底包原版（%d B, md5 %s）' % (len(_jsc), md5b(_jsc)))
sigs = sorted(n for n in names if n.startswith('META-INF/') and (n.endswith('.SF') or n.endswith('.RSA')))
assert sigs == ['META-INF/STARRY.RSA', 'META-INF/STARRY.SF'], '签名块不是本机 keystore 产物（或官方签名残留）：%r' % sigs
print('  包内 so md5 == 构建产物 == %s' % a)
print('  classes2/classes8/classes9 与输入同 md5；libc++_shared.so 存在、Stored、与 NDK 源同 md5；Manifest 含 InitProvider/MenuService/宿主 Application，无 SignSpoof')
print('  条目数 %d；APK %d B' % (len(names), os.path.getsize(apk)))
PY
echo "  产物：$SIGNED ($(fsize "$SIGNED") B, md5 $(hash1 "$SIGNED"))"

step "7/7 出包门禁（check_release_apk / audit_with_waivers）"
FAILED=0
bash "$NIXIANG/04_scripts/_common/check_release_apk.sh" --apk "$SIGNED" --game "$GAME" --expect-vc 5 --expect-vn 5.0 || FAILED=1
bash "$NIXIANG/04_scripts/_common/audit_with_waivers.sh" --game "$GAME" --pkg "$PKG" --apk "$SIGNED" || FAILED=1
if [ "$FAILED" = "0" ]; then echo "  门禁全过"; else echo "  ⚠️ 有门禁未过（见上），不要交付 / 装机"; fi

echo ""
echo "完成：$SIGNED"
