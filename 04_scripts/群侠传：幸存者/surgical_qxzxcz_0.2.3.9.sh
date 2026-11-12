#!/usr/bin/env bash
# script-policy: {"purpose": "群侠传：幸存者 0.2.3.9 星光菜单版外科 zip 组装（菜单 dex + 本游戏 so + 二进制 Manifest 补丁 + 3 个等长 .gdc + 新增开关桥脚本）", "inputs": "08_output 现网 mod 包、gradle assembleRelease 的菜单 APK、libs_qxzxcz/arm64-v8a/libstarry_magic.so、07_patched/群侠传：幸存者/gdc_menu_0.2.3.9/ 下的 gdc 与 remap、patch_qxzxcz_binary_manifest.py", "outputs": "08_output/群侠传：幸存者/群侠传：幸存者_0.2.3.9_starry-menu-releaseSigned.apk + 07_patched 下 Manifest 缓存", "boundary": "只做外科组装/签名/自检与可选装机；不改 assets 其它文件、不覆盖 libc++_shared.so 与 libgodot_android.so、不出 gdc 补丁本身（gdc 由 GDRE+pad_gdc 产出）", "prior_art": "04_scripts/伊莫/surgical_yimo_1.1.0.sh（外科六段骨架）；04_scripts/_common/next_free_dex.py（dex 下标）；03_decompiled/群侠传：幸存者/pad_gdc.py（等长填充）", "difference": "底包换成已带登录 smali 的 mod 包；无 killer/签名轨；新增 gdc 等长替换阶段与 starry_switch 新增文件阶段；Manifest 用 provider-only 补丁器", "evidence": "10_docs/群侠传：幸存者/星光菜单落地.md#开关通道设计"}
# ============================================================================
# 群侠传：幸存者 0.2.3.9（com.nenlystudio.qunxiasurvivor.cn，Godot 4.6.1，TapTap 中国渠道）
#   → 星光悬浮菜单版 外科 zip 组装
#
# 底包 = 现网可用 mod 包（08_output/…_mod-releaseSigned.apk：4 处登录 smali 补丁 + 签名已就位）
#
# 组装内容（5 类）：
#   1) classes2.dex                      ← 菜单 dex（gradle assembleRelease 的 classes.dex）
#   2) lib/arm64-v8a/libstarry_magic.so  ← 本游戏 so（[Qxzxcz] / game_id qxzxcz），Stored 供 extractNativeLibs=false 直映射
#   3) AndroidManifest.xml 补丁          ← StarryInitProvider（首 provider）+ MenuService + 3 权限
#   4) 三个 .gdc **等长替换**            ← health_component / economy_store / combat_hit_pipeline
#   5) 新增 assets/scripts/starry_switch.gdc + .gd.remap（开关桥，新增文件不受等长约束）
#   ⚠️ 不覆盖包内 libc++_shared.so / libgodot_android.so（游戏自带）
#
# 用法：
#   bash 04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.9.sh
#   STARRY_INSTALL=1 SERIAL=f9a116a2 bash …            # 装到设备（install -r）
#   QXZXCZ_SKIP_GDC=1 …                                # 只出菜单不换 gdc（回退用）
# 参数：QXZXCZ_BASE_APK / QXZXCZ_MENU_APK / QXZXCZ_SO / QXZXCZ_GDC_DIR / SERIAL
# ============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="群侠传：幸存者"
VER="0.2.3.9"
PKG="com.nenlystudio.qunxiasurvivor.cn"
TAG="Qxzxcz"
GAME_ID="qxzxcz"
SERIAL="${SERIAL:-f9a116a2}"

MENU_ROOT="$NIXIANG/mod菜单/starry-mod-menu"
BASE="${QXZXCZ_BASE_APK:-$NIXIANG/08_output/${GAME}/${GAME}_${VER}_mod-releaseSigned.apk}"
MENU_APK="${QXZXCZ_MENU_APK:-$MENU_ROOT/app/build/outputs/apk/release/app-release.apk}"
SO_SRC="${QXZXCZ_SO:-$MENU_ROOT/libs_qxzxcz/arm64-v8a/libstarry_magic.so}"
GDC_DIR="${QXZXCZ_GDC_DIR:-$NIXIANG/07_patched/${GAME}/gdc_menu_${VER}}"
BT="${ANDROID_BUILD_TOOLS:-$HOME/Library/Android/sdk/build-tools/36.0.0}"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PY="${PYTHON:-python3}"
MANIFEST_PATCH="$SCRIPT_DIR/patch_qxzxcz_binary_manifest.py"
SKIP_GDC="${QXZXCZ_SKIP_GDC:-0}"

SUFFIX="starry-menu"
[ "$SKIP_GDC" = "1" ] && SUFFIX="starry-menu-nogdc"
OUT="$NIXIANG/08_output/${GAME}"
UNSIGNED="$OUT/${GAME}_${VER}_${SUFFIX}-unsigned.apk"
SIGNED="$OUT/${GAME}_${VER}_${SUFFIX}-releaseSigned.apk"
WORK="$NIXIANG/13_scratch/qxzxcz_menu"
MF_DIR="$NIXIANG/07_patched/${GAME}/menu_${VER}"

die(){ echo "ERROR: $*" >&2; exit 1; }
step(){ echo ""; echo "== $* =="; }

for f in "$BASE" "$MENU_APK" "$SO_SRC" "$MANIFEST_PATCH"; do [ -f "$f" ] || die "缺输入 $f"; done
if [ "$SKIP_GDC" != "1" ]; then
  for f in health_component.gdc economy_store.gdc combat_hit_pipeline.gdc starry_switch.gdc starry_switch.gd.remap; do
    [ -f "$GDC_DIR/$f" ] || die "缺 gdc 产物 $GDC_DIR/$f"
  done
fi
[ -x "$BT/zipalign" ] || die "缺 zipalign ($BT)"
[ -x "$BT/apksigner" ] || die "缺 apksigner ($BT)"
mkdir -p "$OUT" "$WORK" "$MF_DIR"

step "0/7 输入自检"
echo "  base   = $BASE ($(wc -c < "$BASE") B, md5 $(md5sum "$BASE" | awk '{print $1}'))"
echo "  menu   = $MENU_APK ($(wc -c < "$MENU_APK") B)"
echo "  so     = $SO_SRC ($(wc -c < "$SO_SRC") B, md5 $(md5sum "$SO_SRC" | awk '{print $1}'))"
echo "  gdc    = ${GDC_DIR} (SKIP_GDC=$SKIP_GDC)"
LC_ALL=C grep -aq "\[$TAG\]" "$SO_SRC" || die "so 缺 [$TAG] 标签（编错游戏）"
LC_ALL=C grep -aq "$GAME_ID" "$SO_SRC" || die "so 缺 game_id $GAME_ID"
for m in 'Slider_伤害倍率:1:100:1' 'Toggle_无敌' 'Toggle_无限银钱'; do
  n=$(LC_ALL=C grep -ac "$m" "$SO_SRC" || true)
  [ "${n:-0}" -ge 1 ] || die "so 缺菜单串 $m"
done
echo "  so 三项菜单串 + 标签 + game_id 齐备"

step "1/7 解出底包 Manifest 并打补丁"
rm -f "$WORK"/AndroidManifest*.xml "$WORK"/classes2.dex
"$PY" - "$BASE" "$WORK/AndroidManifest.xml" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
open(sys.argv[2], 'wb').write(z.read('AndroidManifest.xml'))
names = z.namelist()
assert 'classes2.dex' not in names, '底包已有 classes2.dex（dex 下标需重新求）'
assert 'lib/arm64-v8a/libstarry_magic.so' not in names, '底包已有 libstarry_magic.so'
print('  底包 Manifest %d B；无 classes2.dex / 无 libstarry_magic.so（可安全追加）' % len(z.read('AndroidManifest.xml')))
PY
"$PY" "$MANIFEST_PATCH" "$WORK/AndroidManifest.xml" "$MF_DIR/AndroidManifest.xml"
cp -f "$MF_DIR/AndroidManifest.xml" "$WORK/AndroidManifest.patched.xml"

step "2/7 取菜单 dex（classes2.dex）"
"$PY" - "$MENU_APK" "$WORK/classes2.dex" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
names = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')]
assert len(names) == 1, '菜单 APK 期望单 dex，实得 %r' % names
b = z.read(names[0])
open(sys.argv[2], 'wb').write(b)
# StateMirror 会被 R8 改名（不被 Manifest/native 按名引用），改判「状态文件名串在」
need = [b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService',
        b'com/starry/magic/core/GameHooks', b'starry_state.txt']
miss = [n.decode() for n in need if n not in b]
assert not miss, '菜单 dex 缺类: %r' % miss
print('  %s %d B md5 %s（StarryInitProvider/MenuService/GameHooks + starry_state.txt 均在）'
      % (names[0], len(b), hashlib.md5(b).hexdigest()))
PY

step "3/7 gdc 等长自检（换长度=引擎 Error decompressing → 崩）"
if [ "$SKIP_GDC" != "1" ]; then
"$PY" - "$BASE" "$GDC_DIR" <<'PY'
import sys, os, zipfile
base, gdir = sys.argv[1], sys.argv[2]
EXPECT = {
    'assets/scripts/components/health_component.gdc': ('health_component.gdc', 2841),
    'assets/scripts/core/model/economy_store.gdc': ('economy_store.gdc', 6630),
    'assets/scripts/systems/combat_hit_pipeline.gdc': ('combat_hit_pipeline.gdc', 34169),
}
z = zipfile.ZipFile(base)
for entry, (fn, n) in EXPECT.items():
    orig = z.getinfo(entry).file_size
    assert orig == n, '底包 %s 实际 %d，与记录 %d 不符（换版本必须重新取数）' % (entry, orig, n)
    got = os.path.getsize(os.path.join(gdir, fn))
    assert got == n, '%s 长 %d ≠ 原条目 %d（pad_gdc 未对齐）' % (fn, got, n)
    print('  OK %s = %d B（解压后等长）' % (entry, got))
print('  新增脚本：starry_switch.gdc %d B + .gd.remap（新增文件无长度约束）'
      % os.path.getsize(os.path.join(gdir, 'starry_switch.gdc')))
PY
fi

step "4/7 外科 zip 重写（保留各条目压缩方式；.so 走 Stored）"
"$PY" - "$BASE" "$WORK/AndroidManifest.patched.xml" "$WORK/classes2.dex" "$SO_SRC" "$GDC_DIR" "$SKIP_GDC" "$UNSIGNED" <<'PY'
import sys, zipfile, os
base, mf, dex, so, gdir, skip_gdc, out = sys.argv[1:8]
SO_NAME = 'lib/arm64-v8a/libstarry_magic.so'
REPLACE = {}
if skip_gdc != '1':
    for entry, fn in (('assets/scripts/components/health_component.gdc', 'health_component.gdc'),
                      ('assets/scripts/core/model/economy_store.gdc', 'economy_store.gdc'),
                      ('assets/scripts/systems/combat_hit_pipeline.gdc', 'combat_hit_pipeline.gdc')):
        REPLACE[entry] = os.path.join(gdir, fn)
ADD = {}
if skip_gdc != '1':
    ADD['assets/scripts/starry_switch.gdc'] = os.path.join(gdir, 'starry_switch.gdc')
    ADD['assets/scripts/starry_switch.gd.remap'] = os.path.join(gdir, 'starry_switch.gd.remap')
zin = zipfile.ZipFile(base)
if os.path.exists(out):
    os.remove(out)
zout = zipfile.ZipFile(out, 'w', allowZip64=True)
swapped = 0
for info in zin.infolist():
    n = info.filename
    if n == 'AndroidManifest.xml':
        zi = zipfile.ZipInfo(n, date_time=info.date_time); zi.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(zi, open(mf, 'rb').read()); continue
    if n.startswith('META-INF/') and (n.endswith('.SF') or n.endswith('.RSA') or n.endswith('.DSA')
                                      or n.endswith('.EC') or n.endswith('/MANIFEST.MF')):
        continue
    data = open(REPLACE[n], 'rb').read() if n in REPLACE else zin.read(n)
    if n in REPLACE:
        swapped += 1
    zi = zipfile.ZipInfo(n, date_time=info.date_time)
    zi.compress_type = info.compress_type
    zi.external_attr = info.external_attr
    zout.writestr(zi, data)
for name, path in ((SO_NAME, so), ('classes2.dex', dex)):
    data = open(path, 'rb').read()
    zi = zipfile.ZipInfo(name, date_time=(1981, 1, 1, 1, 1, 2))
    zi.compress_type = zipfile.ZIP_STORED if name.endswith('.so') else zipfile.ZIP_DEFLATED
    zi.external_attr = 0o644 << 16
    zout.writestr(zi, data)
    print('  + %s (%d B, %s)' % (name, len(data), 'Stored' if zi.compress_type == 0 else 'Deflate'))
for name, path in ADD.items():
    data = open(path, 'rb').read()
    zi = zipfile.ZipInfo(name, date_time=(1981, 1, 1, 1, 1, 2))
    zi.compress_type = zipfile.ZIP_DEFLATED
    zi.external_attr = 0o644 << 16
    zout.writestr(zi, data)
    print('  + %s (%d B, Deflate, 新增)' % (name, len(data)))
zout.close()
print('  条目 %d -> %d；gdc 等长替换 %d 个' % (len(zin.infolist()), len(zipfile.ZipFile(out).infolist()), swapped))
PY

step "5/7 zipalign + apksigner"
rm -f "$SIGNED"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$OUT/.aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$OUT/.aligned.apk"
rm -f "$OUT/.aligned.apk" "$UNSIGNED"

step "6/7 成品自检"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -4
"$PY" - "$SIGNED" "$SO_SRC" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
info_so = z.getinfo('lib/arm64-v8a/libstarry_magic.so')
assert info_so.compress_type == zipfile.ZIP_STORED, '.so 必须 Stored'
a = hashlib.md5(z.read('lib/arm64-v8a/libstarry_magic.so')).hexdigest()
b = hashlib.md5(open(sys.argv[2], 'rb').read()).hexdigest()
assert a == b, '包内 so md5 与构建产物不一致'
dex = z.read('classes2.dex'); assert b'StateMirror' in dex or b'starry_state.txt' in dex
mf = z.read('AndroidManifest.xml'); assert b'com.starry.magic.StarryInitProvider' in mf.replace(bytes([0]), b'')
names = set(z.namelist())
print('  包内 so md5 == 构建产物 ==', a)
print('  classes2.dex %d B；Manifest 含 StarryInitProvider' % len(dex))
if 'assets/scripts/starry_switch.gdc' in names:
    print('  starry_switch.gdc %d B + remap 已入包' % z.getinfo('assets/scripts/starry_switch.gdc').file_size)
    for e in ('assets/scripts/components/health_component.gdc','assets/scripts/core/model/economy_store.gdc','assets/scripts/systems/combat_hit_pipeline.gdc'):
        print('  %s = %d B' % (e, z.getinfo(e).file_size))
PY
echo "  产物：$SIGNED ($(wc -c < "$SIGNED") B, md5 $(md5sum "$SIGNED" | awk '{print $1}'))"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  step "7/7 装机（install -r，同签名覆盖）"
  adb -s "$SERIAL" install -r -d "$SIGNED"
else
  echo ""; echo "（跳过装机：STARRY_INSTALL=1 才装）"
fi
