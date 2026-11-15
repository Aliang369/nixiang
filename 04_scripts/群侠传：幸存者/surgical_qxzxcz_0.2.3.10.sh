#!/usr/bin/env bash
# script-policy: {"purpose": "群侠传：幸存者 0.2.3.10 单包总装：官方 APK 底包 + 登录补丁 dex（classes.dex 替换）+ 菜单 dex（classes2.dex 追加）+ libstarry_magic.so + 二进制 Manifest 补丁 + 3 个等长 .gdc 替换 + starry_switch 新增 + 对齐签名 + 成品自检 + 可选装机", "inputs": "02_apks/群侠传：幸存者/<VER>_official_from_device.apk；03_decompiled/群侠传：幸存者/<VER>/classes_mod_<VER>.dex（登录补丁产物，通配解析）；菜单 APK app-release.apk；libs_qxzxcz/arm64-v8a/libstarry_magic.so；07_patched/群侠传：幸存者/gdc_menu_<VER>/；patch_qxzxcz_binary_manifest.py；01_tools/starry_release.jks", "outputs": "08_output/群侠传：幸存者/群侠传：幸存者_<VER>_starry-menu-releaseSigned.apk（QXZXCZ_FAKE_DEX=1 干跑落 13_scratch）+ 07_patched/群侠传：幸存者/menu_<VER>/AndroidManifest.xml（Manifest 补丁缓存）", "boundary": "只做外科 zip 组装/对齐/签名/自检/可选装机；不改 assets 其它条目；不覆盖 libc++_shared.so / libgodot_android.so；不产出 dex 与 gdc 本身（分别由 patch_taptap_login_qxzxcz.sh / patch_gdc_qxzxcz.sh 产出）；不卸载、不清数据、不重装异签名", "prior_art": "04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.9.sh（同游戏上一版外科六段骨架）；04_scripts/伊莫/surgical_yimo_1.1.0.sh；04_scripts/_common/pre_install_gate.sh（装机前门禁）", "difference": "底包由现网 mod 包换成官方 from_device APK，并在同一次组装里补上登录 dex 替换（0.2.3.9 是直接吃现成 mod 包）；新增『底包无 classes2.dex』『成品 classes.dex 必须变（查12 会挡）』『成品各 dex 均无 SignSpoof』『其它条目逐字节+压缩方式不变』四类硬断言；新增 QXZXCZ_FAKE_DEX=1 干跑（假 dex 走通全链路，仅跳过『dex 未改』断言）", "evidence": "10_docs/群侠传：幸存者/星光菜单落地.md §4/§7/§8；10_docs/群侠传：幸存者/_state/0.2.3.10_gdc补丁.md"}
# ============================================================================
# 群侠传：幸存者 0.2.3.10（com.nenlystudio.qunxiasurvivor.cn，Godot 4.6.1，TapTap 中国渠道）
#   → 星光悬浮菜单版（单包：登录改包 + 菜单）外科 zip 组装
#
# 底包 = 官方 APK（02_apks/…_official_from_device.apk，md5 7677c9c2fd301b06cc3ab245bb0cced0）
#        官方包只有 classes.dex；登录补丁与菜单 dex 都在本脚本里一次性落位。
#
# 组装内容（6 类）：
#   1) classes.dex                       ← classes_mod_<VER>.dex（登录改包后全量 dex，替换）
#   2) classes2.dex                      ← 菜单 dex（gradle assembleRelease 的 classes.dex，追加）
#   3) lib/arm64-v8a/libstarry_magic.so  ← 本游戏 so（[Qxzxcz] / qxzxcz），Stored + 页对齐
#   4) AndroidManifest.xml 补丁          ← StarryInitProvider（首 provider）+ MenuService + 3 权限
#   5) 三个 .gdc 等长替换                ← health_component / economy_store / combat_hit_pipeline
#   6) 新增 assets/scripts/starry_switch.gdc + .gd.remap（开关桥；新增文件不受等长约束）
#   ⚠️ 不覆盖包内 libc++_shared.so / libgodot_android.so（游戏自带，覆盖=换引擎）
#   ⚠️ .gdc 替换解压后长度必须与原条目完全一致，否则引擎 Error decompressing → GLThread SIGSEGV
#
# 用法：
#   bash 04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.10.sh
#   STARRY_INSTALL=1 SERIAL=f9a116a2 bash …                 # 装机前门禁 + install -r
#   QXZXCZ_SKIP_GDC=1 …                                     # 只出菜单包（不换 gdc，回退用）
#   QXZXCZ_FAKE_DEX=1 …                                     # 干跑：用底包 dex 冒充登录 dex，产物落 13_scratch
#   QXZXCZ_ALLOW_SAME_DEX=1 …                               # YOLO：跳过『成品 classes.dex ≠ 底包』断言
#
# 参数（环境变量）：QXZXCZ_BASE_APK / QXZXCZ_LOGIN_DEX / QXZXCZ_MENU_APK / QXZXCZ_SO /
#                   QXZXCZ_GDC_DIR / SERIAL / SKIP_PRE_INSTALL_GATE / ADB
# ============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="群侠传：幸存者"
VER="0.2.3.10"
PKG="com.nenlystudio.qunxiasurvivor.cn"
TAG="Qxzxcz"
GAME_ID="qxzxcz"
SERIAL="${SERIAL:-f9a116a2}"
STARRY_INSTALL="${STARRY_INSTALL:-0}"
SKIP_GATE="${SKIP_PRE_INSTALL_GATE:-0}"
FAKE_DEX="${QXZXCZ_FAKE_DEX:-0}"
ALLOW_SAME="${QXZXCZ_ALLOW_SAME_DEX:-0}"
if [ "$FAKE_DEX" = "1" ]; then ALLOW_SAME=1; fi

MENU_ROOT="$NIXIANG/mod菜单/starry-mod-menu"
OFFICIAL_APK="$NIXIANG/02_apks/$GAME/${GAME}_${VER}_official_from_device.apk"
BASE="${QXZXCZ_BASE_APK:-$OFFICIAL_APK}"
BASE_MD5_EXPECT="7677c9c2fd301b06cc3ab245bb0cced0"
BASE_SIZE_EXPECT=590953445
DEC_DIR="$NIXIANG/03_decompiled/$GAME/$VER"
LOGIN_DEX="${QXZXCZ_LOGIN_DEX:-}"
MENU_APK="${QXZXCZ_MENU_APK:-$MENU_ROOT/app/build/outputs/apk/release/app-release.apk}"
SO_SRC="${QXZXCZ_SO:-$MENU_ROOT/libs_qxzxcz/arm64-v8a/libstarry_magic.so}"
GDC_DIR="${QXZXCZ_GDC_DIR:-$NIXIANG/07_patched/$GAME/gdc_menu_$VER}"
BT="${ANDROID_BUILD_TOOLS:-${HOME:-/tmp}/Library/Android/sdk/build-tools/36.0.0}"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PY="${PYTHON:-python3}"
MANIFEST_PATCH="$SCRIPT_DIR/patch_qxzxcz_binary_manifest.py"
SKIP_GDC="${QXZXCZ_SKIP_GDC:-0}"

# 三个 gdc 的原条目解压长度（0.2.3.10 现取：2841 / 35739 / 6630；0.2.3.9 为 2841 / 34169 / 6630）
LEN_HEALTH=2841
LEN_PIPELINE=35739
LEN_ECONOMY=6630
MD5_HEALTH="2cf1bcbdfad58cd88a1ecee5e87bb89c"
MD5_PIPELINE="5d95378d524ba4693977b856e95a25e4"
MD5_ECONOMY="3b1765143ca348d4d510662e958eb77b"
MD5_SWITCH="823f2526f454849fce4e0c9f87ce63b0"
MD5_REMAP="a27119f7d5295a3b2bc703038de83124"

SUFFIX="starry-menu"
if [ "$SKIP_GDC" = "1" ]; then SUFFIX="starry-menu-nogdc"; fi
WORK="$NIXIANG/13_scratch/qxzxcz_menu_$VER"
if [ "$FAKE_DEX" = "1" ]; then
  OUT="$WORK/out"
  SUFFIX="$SUFFIX-dryrun"
else
  OUT="$NIXIANG/08_output/$GAME"
fi
UNSIGNED="$OUT/${GAME}_${VER}_${SUFFIX}-unsigned.apk"
SIGNED="$OUT/${GAME}_${VER}_${SUFFIX}-releaseSigned.apk"
ALIGNED="$WORK/aligned.apk"
MF_DIR="$NIXIANG/07_patched/$GAME/menu_$VER"

ADB="${ADB:-adb}"
if ! command -v "$ADB" >/dev/null 2>&1; then
  for c in /opt/homebrew/bin/adb "${HOME:-/tmp}/Library/Android/sdk/platform-tools/adb" /usr/local/bin/adb; do
    if [ -x "$c" ]; then ADB="$c"; break; fi
  done
fi

die(){ echo "ERROR: $*" >&2; exit 1; }
step(){ echo ""; echo "== $* =="; }
m5(){  # 单文件 md5（mac/win 双机；对 590 MB 底包也能用，R4 只对直接哈希 .apk 报 WARN）
  if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'
  elif command -v md5 >/dev/null 2>&1; then md5 -q "$1"
  else "$PY" -c 'import hashlib,sys
h=hashlib.md5()
with open(sys.argv[1],"rb") as f:
    for c in iter(lambda: f.read(1<<20), b""): h.update(c)
print(h.hexdigest())' "$1"
  fi
}
fsize(){ wc -c < "$1" | tr -d ' '; }

mkdir -p "$WORK" "$OUT" "$MF_DIR"

# ---------------------------------------------------------------- 0/9 输入自检
step "0/9 输入自检"
for f in "$BASE" "$MENU_APK" "$SO_SRC" "$MANIFEST_PATCH" "$KS"; do
  [ -f "$f" ] || die "缺输入 $f"
done
[ -x "$BT/zipalign" ] || die "缺 zipalign ($BT)"
[ -x "$BT/apksigner" ] || die "缺 apksigner ($BT)"

B_SIZE="$(fsize "$BASE")"
[ "$B_SIZE" = "$BASE_SIZE_EXPECT" ] || die "底包大小 ${B_SIZE} B ≠ 记录 ${BASE_SIZE_EXPECT} B（换版本必须重新取数）"
B_MD5="$(m5 "$BASE")"
[ "$B_MD5" = "$BASE_MD5_EXPECT" ] || die "底包 md5 ${B_MD5} ≠ 记录 ${BASE_MD5_EXPECT}（不是官方 0.2.3.10）"
echo "  base   = $BASE"
echo "           ${B_SIZE} B, md5 ${B_MD5}（官方 0.2.3.10）"
echo "  menu   = $MENU_APK ($(fsize "$MENU_APK") B, md5 $(m5 "$MENU_APK"))"
echo "  so     = $SO_SRC ($(fsize "$SO_SRC") B, md5 $(m5 "$SO_SRC"))"
echo "  gdc    = $GDC_DIR (SKIP_GDC=${SKIP_GDC})"
echo "  build-tools = $BT"

LC_ALL=C grep -aq "\[$TAG\]" "$SO_SRC" || die "so 缺 [$TAG] 标签（编错游戏）"
LC_ALL=C grep -aq "$GAME_ID" "$SO_SRC" || die "so 缺 game_id $GAME_ID"
for m in 'Slider_伤害倍率:1:100:1' 'Toggle_无敌' 'Toggle_无限银钱'; do
  n="$(LC_ALL=C grep -ac -- "$m" "$SO_SRC" || true)"
  [ "${n:-0}" -ge 1 ] || die "so 缺菜单串 $m"
done
LC_ALL=C grep -aq 'STARRYSTAMP1:EPOCH=' "$SO_SRC" || die "so 缺构建戳 STARRYSTAMP1:EPOCH=（pre_install_gate 查8 会 FAIL）"
echo "  so 标签/game_id/3 个菜单串/构建戳 均齐备"

if [ "$SKIP_GDC" != "1" ]; then
  for f in health_component.gdc economy_store.gdc combat_hit_pipeline.gdc starry_switch.gdc starry_switch.gd.remap; do
    [ -f "$GDC_DIR/$f" ] || die "缺 gdc 产物 $GDC_DIR/$f"
  done
  for pair in "health_component.gdc:$LEN_HEALTH:$MD5_HEALTH" "economy_store.gdc:$LEN_ECONOMY:$MD5_ECONOMY" \
              "combat_hit_pipeline.gdc:$LEN_PIPELINE:$MD5_PIPELINE" "starry_switch.gdc:1202:$MD5_SWITCH" \
              "starry_switch.gd.remap:48:$MD5_REMAP"; do
    fn="${pair%%:*}"; rest="${pair#*:}"; want_len="${rest%%:*}"; want_md5="${rest#*:}"
    got_len="$(fsize "$GDC_DIR/$fn")"; got_md5="$(m5 "$GDC_DIR/$fn")"
    [ "$got_len" = "$want_len" ] || die "$fn 大小 ${got_len} ≠ 记录 ${want_len}"
    [ "$got_md5" = "$want_md5" ] || die "$fn md5 ${got_md5} ≠ 记录 ${want_md5}（pad_gdc 未对齐/产物不是本版）"
  done
  echo "  gdc 5 个产物：长度与 md5 全部符合 _state/0.2.3.10_gdc补丁.md 记录"
fi

if [ "$FAKE_DEX" = "1" ]; then
  LOGIN_DEX="$WORK/fake_classes.dex"
  "$PY" - "$BASE" "$LOGIN_DEX" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
b = z.read('classes.dex')
open(sys.argv[2], 'wb').write(b)
print('  DRY-RUN: 假 dex = 底包 classes.dex 副本（%d B, md5 %s）' % (len(b), hashlib.md5(b).hexdigest()))
print('    => 仅用于验证脚本骨架；本产物不得装机、不得当交付物')
PY
else
  if [ -z "$LOGIN_DEX" ]; then
    NC=0
    for f in "$DEC_DIR"/classes_mod*.dex; do
      if [ -e "$f" ]; then NC=$((NC+1)); LOGIN_DEX="$f"; fi
    done
    [ "$NC" -ge 1 ] || die "找不到登录补丁 dex：$DEC_DIR/classes_mod*.dex（先跑 patch_taptap_login_qxzxcz.sh）"
    [ "$NC" -eq 1 ] || die "$DEC_DIR 下有 ${NC} 个 classes_mod*.dex（产物不唯一，请用 QXZXCZ_LOGIN_DEX 指定）"
  fi
  [ -f "$LOGIN_DEX" ] || die "登录 dex 不存在：$LOGIN_DEX"
  echo "  login  = $LOGIN_DEX ($(fsize "$LOGIN_DEX") B, md5 $(m5 "$LOGIN_DEX"))"
fi

# ------------------------------------------------- 1/9 底包清单 + Manifest 补丁
step "1/9 底包清单 + 二进制 Manifest 补丁"
"$PY" - "$BASE" "$WORK/AndroidManifest.xml" "$WORK/base_classes.md5" <<'PY'
import sys, zipfile, hashlib
base, mfout, md5out = sys.argv[1:4]
z = zipfile.ZipFile(base)
names = z.namelist()
dexes = sorted(n for n in names if n.startswith('classes') and n.endswith('.dex'))
assert dexes == ['classes.dex'], '底包 dex 布局异常：%r（期望只有 classes.dex；多 dex 则 classes2 下标需重求）' % dexes
assert 'lib/arm64-v8a/libstarry_magic.so' not in names, '底包已有 libstarry_magic.so'
for e in ('assets/scripts/starry_switch.gdc', 'assets/scripts/starry_switch.gd.remap'):
    assert e not in names, '底包已有 %s（新增项会冲突）' % e
info = z.getinfo('classes.dex')
data = z.read('classes.dex')
man = z.read('AndroidManifest.xml')
open(mfout, 'wb').write(man)
md5 = hashlib.md5(data).hexdigest()
open(md5out, 'w').write('%s %d %d\n' % (md5, info.file_size, info.compress_type))
print('  底包条目 %d；classes.dex %d B（compress_type=%d, md5 %s）' % (len(names), info.file_size, info.compress_type, md5))
print('  底包无 classes2.dex / 无 libstarry_magic.so / 无 starry_switch.*（可安全替换与追加）')
print('  Manifest %d B（头 %s）' % (len(man), man[:4].hex()))
PY
"$PY" "$MANIFEST_PATCH" "$WORK/AndroidManifest.xml" "$MF_DIR/AndroidManifest.xml"
cp -f "$MF_DIR/AndroidManifest.xml" "$WORK/AndroidManifest.patched.xml"
"$PY" - "$WORK/AndroidManifest.patched.xml" <<'PY'
import sys
b = open(sys.argv[1], 'rb').read()
assert b[:4] == b'\x03\x00\x08\x00', 'Manifest 头 %r 不是真 AXML（查7 会 FAIL）' % b[:4]
flat = b.replace(b'\x00', b'')          # AXML 字符串池可能是 UTF-16LE
need = ['com.starry.magic.StarryInitProvider', 'com.starry.magic.menu.MenuService',
        'android.permission.SYSTEM_ALERT_WINDOW', 'android.permission.FOREGROUND_SERVICE',
        'android.permission.FOREGROUND_SERVICE_SPECIAL_USE']
miss = [s for s in need if s.encode() not in flat]
assert not miss, '补丁后 Manifest 缺：%r' % miss
assert b'SignSpoof' not in flat, 'Manifest 仍挂 SignSpoof（查12 会 FAIL）'
print('  Manifest 补丁 OK：%d B，AXML 头 03000800，含 StarryInitProvider + MenuService + 3 权限，无 SignSpoof' % len(b))
PY

# ------------------------------------------------ 2/9 登录 dex（classes.dex）校验
step "2/9 登录 dex 校验（成品 classes.dex）"
"$PY" - "$LOGIN_DEX" "$WORK/base_classes.md5" "$ALLOW_SAME" <<'PY'
import sys, hashlib
dex, md5file, allow_same = sys.argv[1], sys.argv[2], sys.argv[3] == '1'
b = open(dex, 'rb').read()
assert b[:4] == b'dex\n', '不是 dex 文件（魔数 %r）：%s' % (b[:4], dex)
assert len(b) > 5000000, '登录 dex 只有 %d B，疑似截断/空 dex' % len(b)
md5 = hashlib.md5(b).hexdigest()
base_md5 = open(md5file).read().split()[0]
need = [b'com/taptap/sdk/initializer/repository/GatekeeperRepository', b'com/nenlystudio/']
miss = [s.decode() for s in need if s not in b]
assert not miss, '登录 dex 缺 %r（不是本游戏全量 dex？）' % miss
assert b'Lcom/starry/magic/menu/MenuService;' not in b, '登录 dex 里已含菜单类（把菜单 dex 混进来了）'
assert b'SignSpoof' not in b, '登录 dex 含 SignSpoof（该类 2026-10-28 已删除，禁复活）'
print('  %s：%d B，md5 %s' % (dex.split('/')[-1], len(b), md5))
if md5 == base_md5:
    if allow_same:
        print('  WARN YOLO/干跑：成品 classes.dex 与底包逐字节相同（QXZXCZ_ALLOW_SAME_DEX=1 已放行）')
        print('    => 正式出包禁止此状态：pre_install_gate 查12 会以「没改包」直接 FAIL')
    else:
        raise SystemExit('ERROR: 登录 dex md5 == 底包 classes.dex md5（%s）=> 登录补丁没生效或拿错文件' % md5)
else:
    print('  OK 与底包 classes.dex 不同（底包 %s -> 补丁 %s）' % (base_md5[:12], md5[:12]))
PY

# --------------------------------------------------- 3/9 菜单 dex（classes2.dex）
step "3/9 菜单 dex 提取（成品 classes2.dex）"
"$PY" - "$MENU_APK" "$WORK/classes2.dex" <<'PY'
import sys, zipfile, hashlib
z = zipfile.ZipFile(sys.argv[1])
names = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')]
assert len(names) == 1, '菜单 APK 期望单 dex，实得 %r' % names
b = z.read(names[0])
open(sys.argv[2], 'wb').write(b)
need = [b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService',
        b'com/starry/magic/core/GameHooks', b'starry_state.txt']
miss = [n.decode() for n in need if n not in b]
assert not miss, '菜单 dex 缺类: %r' % miss
assert b'SignSpoof' not in b, '菜单 dex 含 SignSpoof（禁复活）'
print('  %s %d B md5 %s（StarryInitProvider/MenuService/GameHooks + starry_state.txt 均在）'
      % (names[0], len(b), hashlib.md5(b).hexdigest()))
PY

# --------------------------------------------------- 4/9 gdc 等长 + 内容一致校验
step "4/9 gdc 等长自检（换长度=引擎 Error decompressing -> 崩）"
if [ "$SKIP_GDC" != "1" ]; then
"$PY" - "$BASE" "$GDC_DIR" "$LEN_HEALTH" "$LEN_ECONOMY" "$LEN_PIPELINE" <<'PY'
import sys, os, zipfile, hashlib
base, gdir = sys.argv[1], sys.argv[2]
EXPECT = {
    'assets/scripts/components/health_component.gdc': ('health_component.gdc', int(sys.argv[3])),
    'assets/scripts/core/model/economy_store.gdc': ('economy_store.gdc', int(sys.argv[4])),
    'assets/scripts/systems/combat_hit_pipeline.gdc': ('combat_hit_pipeline.gdc', int(sys.argv[5])),
}
z = zipfile.ZipFile(base)
for entry, (fn, n) in EXPECT.items():
    orig = z.getinfo(entry).file_size
    assert orig == n, '底包 %s 实际 %d，与记录 %d 不符（换版本必须重新取数）' % (entry, orig, n)
    got = os.path.getsize(os.path.join(gdir, fn))
    assert got == n, '%s 长 %d != 原条目 %d（pad_gdc 未对齐）' % (fn, got, n)
    print('  OK %s = %d B（解压后等长）' % (entry, got))
for fn, md5want in (('health_component.gdc', '2cf1bcbdfad58cd88a1ecee5e87bb89c'),
                    ('combat_hit_pipeline.gdc', '5d95378d524ba4693977b856e95a25e4'),
                    ('economy_store.gdc', '3b1765143ca348d4d510662e958eb77b')):
    got = hashlib.md5(open(os.path.join(gdir, fn), 'rb').read()).hexdigest()
    assert got == md5want, '%s md5 %s != 本版记录 %s' % (fn, got, md5want)
print('  新增脚本：starry_switch.gdc %d B + .gd.remap %d B（新增文件无长度约束）'
      % (os.path.getsize(os.path.join(gdir, 'starry_switch.gdc')),
         os.path.getsize(os.path.join(gdir, 'starry_switch.gd.remap'))))
PY
fi

# ------------------------------------- 5/9 外科 zip 重写（保留其它条目字节与压缩方式）
step "5/9 外科 zip 重写（classes.dex 替换 + classes2.dex/so/switch 追加；.so 走 Stored）"
if [ -f "$UNSIGNED" ]; then rm -f "$UNSIGNED"; fi
"$PY" - "$BASE" "$WORK/AndroidManifest.patched.xml" "$LOGIN_DEX" "$WORK/classes2.dex" "$SO_SRC" \
        "$GDC_DIR" "$SKIP_GDC" "$UNSIGNED" <<'PY'
import sys, zipfile, os
base, mf, login, menu, so, gdir, skip_gdc, out = sys.argv[1:9]
SO_NAME = 'lib/arm64-v8a/libstarry_magic.so'
REPLACE = {'AndroidManifest.xml': mf, 'classes.dex': login}
ADD = {'classes2.dex': menu, SO_NAME: so}
if skip_gdc != '1':
    REPLACE['assets/scripts/components/health_component.gdc'] = os.path.join(gdir, 'health_component.gdc')
    REPLACE['assets/scripts/core/model/economy_store.gdc'] = os.path.join(gdir, 'economy_store.gdc')
    REPLACE['assets/scripts/systems/combat_hit_pipeline.gdc'] = os.path.join(gdir, 'combat_hit_pipeline.gdc')
    ADD['assets/scripts/starry_switch.gdc'] = os.path.join(gdir, 'starry_switch.gdc')
    ADD['assets/scripts/starry_switch.gd.remap'] = os.path.join(gdir, 'starry_switch.gd.remap')

def drop(name):
    if not name.startswith('META-INF/'):
        return False
    tail = name.rsplit('/', 1)[-1]
    return tail == 'MANIFEST.MF' or tail.endswith('.SF') or tail.endswith('.RSA') \
        or tail.endswith('.DSA') or tail.endswith('.EC')

zin = zipfile.ZipFile(base)
if os.path.exists(out):
    os.remove(out)
zout = zipfile.ZipFile(out, 'w', allowZip64=True)
replaced = 0
dropped = []
for info in zin.infolist():
    n = info.filename
    if drop(n):
        dropped.append(n); continue
    data = open(REPLACE[n], 'rb').read() if n in REPLACE else zin.read(n)
    if n in REPLACE:
        replaced += 1
    zi = zipfile.ZipInfo(n, date_time=info.date_time)
    zi.compress_type = info.compress_type
    zi.external_attr = info.external_attr
    zout.writestr(zi, data)
for name, path in ADD.items():
    data = open(path, 'rb').read()
    zi = zipfile.ZipInfo(name, date_time=(1981, 1, 1, 1, 1, 2))
    zi.compress_type = zipfile.ZIP_STORED if name.endswith('.so') else zipfile.ZIP_DEFLATED
    zi.external_attr = 0o644 << 16
    zout.writestr(zi, data)
    print('  + %s (%d B, %s)' % (name, len(data), 'Stored' if zi.compress_type == 0 else 'Deflate'))
zout.close()
print('  去掉签名文件 %d 个：%s' % (len(dropped), ', '.join(dropped) if dropped else '（无）'))
print('  条目 %d -> %d；替换 %d 个（Manifest/classes.dex/gdc 若干）'
      % (len(zin.infolist()), len(zipfile.ZipFile(out).infolist()), replaced))
PY

# --------------------------------------------------------- 6/9 zipalign + 签名
step "6/9 zipalign -p 4 + apksigner（v1/v2/v3）"
if [ -f "$SIGNED" ]; then rm -f "$SIGNED"; fi
if [ -f "$ALIGNED" ]; then rm -f "$ALIGNED"; fi
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
"$BT/zipalign" -c -p 4 "$ALIGNED" || die "zipalign 校验未过"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
  --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true \
  --out "$SIGNED" "$ALIGNED"
rm -f "$ALIGNED" "$UNSIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -4

# --------------------------------------------------------------- 7/9 成品自检
step "7/9 成品自检（包内 md5 / 无 SignSpoof / 其它条目逐字节不变）"
"$PY" - "$SIGNED" "$BASE" "$LOGIN_DEX" "$WORK/classes2.dex" "$SO_SRC" "$GDC_DIR" "$SKIP_GDC" "$ALLOW_SAME" <<'PY'
import sys, zipfile, hashlib
apk, base, login, menu, so, gdir, skip_gdc, allow_same = sys.argv[1:9]
SO_NAME = 'lib/arm64-v8a/libstarry_magic.so'
GDC = {'assets/scripts/components/health_component.gdc': 'health_component.gdc',
       'assets/scripts/core/model/economy_store.gdc': 'economy_store.gdc',
       'assets/scripts/systems/combat_hit_pipeline.gdc': 'combat_hit_pipeline.gdc'}
NEW = {'classes2.dex', SO_NAME}
if skip_gdc != '1':
    NEW |= {'assets/scripts/starry_switch.gdc', 'assets/scripts/starry_switch.gd.remap'}

def h(b):
    return hashlib.md5(b).hexdigest()

def drop(name):
    if not name.startswith('META-INF/'):
        return False
    tail = name.rsplit('/', 1)[-1]
    return tail == 'MANIFEST.MF' or tail.endswith('.SF') or tail.endswith('.RSA') \
        or tail.endswith('.DSA') or tail.endswith('.EC')

zb, zo = zipfile.ZipFile(base), zipfile.ZipFile(apk)
bmap = {i.filename: i for i in zb.infolist()}
omap = {i.filename: i for i in zo.infolist()}

# 7.1 条目集合：底包条目（去掉签名文件）+ 4 个新增，不多不少（v1 签名文件由 apksigner 重新生成）
bkeep = {n for n in bmap if not drop(n)}
okeep = {n for n in omap if not drop(n)}
missing = sorted(bkeep - okeep)
extra = sorted(okeep - bkeep - NEW)
assert not missing, '成品缺底包条目 %r' % missing[:5]
assert not extra, '成品多出未预期条目 %r' % extra[:5]
for n in NEW:
    assert n in omap, '成品缺新增条目 %s' % n
# 官方签名证书文件（.SF/.RSA/.DSA/.EC）必须消失；META-INF/MANIFEST.MF 由 apksigner 重新生成，允许存在
oldsig = [n for n in omap if drop(n) and n != 'META-INF/MANIFEST.MF' and n in bmap and bmap[n] is not None
          and h(zb.read(n)) == h(zo.read(n))]
assert not oldsig, '成品仍含官方旧签名文件（与底包逐字节相同）%r' % oldsig
newsig = sorted(n for n in omap if drop(n))
assert 'META-INF/MANIFEST.MF' in newsig, '成品缺 v1 MANIFEST.MF（apksigner v1 未生效）'

# 7.2 未被替换的条目：解压后逐字节相同 + 压缩方式相同
changed = {'AndroidManifest.xml', 'classes.dex'} | (set(GDC) if skip_gdc != '1' else set())
bad = []
for n in bkeep:
    if n in changed:
        continue
    if bmap[n].compress_type != omap[n].compress_type:
        bad.append('%s:压缩方式 %d->%d' % (n, bmap[n].compress_type, omap[n].compress_type)); continue
    if h(zb.read(n)) != h(zo.read(n)):
        bad.append('%s:内容变了' % n)
assert not bad, '未替换条目被改动（%d 条）：%r' % (len(bad), bad[:5])

# 7.3 替换项逐一核对
assert h(zo.read('classes.dex')) == h(open(login, 'rb').read()), 'classes.dex 与登录补丁 dex 不一致'
assert h(zo.read('classes2.dex')) == h(open(menu, 'rb').read()), 'classes2.dex 与菜单 dex 不一致'
info_so = omap[SO_NAME]
assert info_so.compress_type == zipfile.ZIP_STORED, '.so 必须 Stored（extractNativeLibs=false 直映射）'
so_md5 = h(zo.read(SO_NAME))
assert so_md5 == h(open(so, 'rb').read()), '包内 so md5 与构建产物不一致'
if skip_gdc != '1':
    for entry, fn in GDC.items():
        got = h(zo.read(entry))
        want = h(open(gdir + '/' + fn, 'rb').read())
        assert got == want, '%s 包内内容与补丁产物不一致' % entry
    for n in ('assets/scripts/starry_switch.gdc', 'assets/scripts/starry_switch.gd.remap'):
        assert h(zo.read(n)) == h(open(gdir + '/' + n.rsplit('/', 1)[-1], 'rb').read()), '%s 内容不一致' % n

# 7.4 Manifest 形态 + 无 SignSpoof + dex 必须变
mf = zo.read('AndroidManifest.xml')
flat = mf.replace(b'\x00', b'')
assert mf[:4] == b'\x03\x00\x08\x00', '包内 Manifest 不是真 AXML'
assert b'com.starry.magic.StarryInitProvider' in flat, '包内 Manifest 缺 StarryInitProvider'
assert b'com.starry.magic.menu.MenuService' in flat, '包内 Manifest 缺 MenuService'
assert b'SignSpoofAppComponentFactory' not in flat, 'Manifest 仍挂 SignSpoofAppComponentFactory（查12 FAIL）'
dex_names = sorted(n for n in omap if n.startswith('classes') and n.endswith('.dex'))
assert dex_names == ['classes.dex', 'classes2.dex'], '成品 dex 布局异常：%r' % dex_names
spoof = [n for n in dex_names if b'SignSpoof' in zo.read(n)]
assert not spoof, '成品 dex 仍含 SignSpoof：%r' % spoof
base_dex_md5 = h(zb.read('classes.dex'))
prod_dex_md5 = h(zo.read('classes.dex'))
if base_dex_md5 == prod_dex_md5:
    assert allow_same, '成品 classes.dex == 底包 classes.dex（登录补丁没进包；查12 会拦）'
    print('  WARN YOLO/干跑：成品 classes.dex 与底包相同（已按 QXZXCZ_ALLOW_SAME_DEX 放行）')

print('  包内 classes.dex %d B md5 %s（底包 %s）' % (omap['classes.dex'].file_size, prod_dex_md5, base_dex_md5[:12]))
print('  包内 classes2.dex %d B md5 %s' % (omap['classes2.dex'].file_size, h(zo.read('classes2.dex'))))
print('  包内 so md5 == 构建产物 == %s（%d B, Stored）' % (so_md5, info_so.file_size))
if skip_gdc != '1':
    for entry in GDC:
        print('  %s = %d B（等长替换）' % (entry, omap[entry].file_size))
    print('  starry_switch.gdc %d B + .gd.remap %d B（新增）'
          % (omap['assets/scripts/starry_switch.gdc'].file_size, omap['assets/scripts/starry_switch.gd.remap'].file_size))
else:
    print('  （QXZXCZ_SKIP_GDC=1：未替换 gdc、未新增 starry_switch）')
print('  其它 %d 个条目：字节与压缩方式均与底包一致' % (len(bkeep) - len(changed)))
print('  v1 签名文件：%s' % ', '.join(newsig))
print('  成品 dex 均无 SignSpoof；Manifest AXML 头 03000800 + StarryInitProvider + MenuService')
PY
S_SIZE="$(fsize "$SIGNED")"; S_MD5="$(m5 "$SIGNED")"
echo "  产物：$SIGNED"
echo "        ${S_SIZE} B, md5 ${S_MD5}"
if [ "$FAKE_DEX" = "1" ]; then
  echo "  WARN 干跑产物（假 dex）：仅供骨架自检，禁止装机/交付"
fi

# ---------------------------------------------------------------- 8/9 装机
if [ "$STARRY_INSTALL" = "1" ]; then
  step "8/9 装机前门禁 + install -r（不卸载、不清数据）"
  if [ "$FAKE_DEX" = "1" ]; then die "干跑产物不得装机（QXZXCZ_FAKE_DEX=0 后才可）"; fi
  if [ "$SKIP_GATE" != "1" ]; then
    bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" \
      --apk "$SIGNED" --base-apk "$BASE" \
      --menu-dex "$SIGNED" --menu-dex-name classes2.dex \
      --needle 'com/starry/magic/StarryInitProvider' \
      --needle 'com/starry/magic/menu/MenuService' \
      --needle 'starry_state.txt' \
      --needle 'com/taptap/sdk/initializer/repository/GatekeeperRepository' \
      || die "pre_install_gate 未过 => 禁止装机（逃生门 SKIP_PRE_INSTALL_GATE=1 须在报告写明理由）"
  else
    echo "  WARN 已按 SKIP_PRE_INSTALL_GATE=1 跳过装机前门禁（必须在装机报告写明理由）"
  fi
  "$ADB" -s "$SERIAL" install -r -d "$SIGNED"
  echo "  装机返回码 0 => 已覆盖安装（未卸载、未清数据）"
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" \
    || echo "  WARN 设备包 md5 校验未过（非 root 机读不到 /data/app 时属预期，改用 dumpsys 组件指纹）"
else
  echo ""
  echo "（跳过装机：STARRY_INSTALL=1 才装）"
fi
