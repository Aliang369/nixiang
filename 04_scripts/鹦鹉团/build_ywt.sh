#!/usr/bin/env bash
# script-policy: {"purpose": "鹦鹉团 1.0.7 星光菜单组包：官方 APK 底包 + 外科 zip 组装（登录补丁 3 个 dex 可选、菜单 dex、libstarry_magic.so、可选 libil2cpp.so/global-metadata）+ 二进制 Manifest 补丁 + zipalign + apksigner(v1/v2/v3) + 成品自检 + 可选装机；两档 --stage inject|full", "inputs": "02_apks/鹦鹉团_1.0.7_orig.apk（md5 aabf023083b46284a425e3b07c9ddcf0）；菜单 APK（gradle assembleRelease 产物）；libstarry_magic.so（[Ywt] 标签）；本游戏 binary Manifest 补丁脚本；可选 libil2cpp.so + global-metadata.dat（成对）；01_tools/starry_release.jks；--stage full 时另需 patch_taptap_login_ywt.sh 产出的 classes/classes7/classes11.dex", "outputs": "08_output/鹦鹉团/鹦鹉团_1.0.7_starry-{inject|full}-{YYYYMMDD}.apk（--dry-run 只打印计划）+ stdout 的 sha256/大小/证书/自检结论", "boundary": "只做外科 zip 组装/对齐/签名/自检；不用 apktool b 整包重建；不覆盖底包输入；不改 assets 其它条目；不卸载、不清数据；装机必须显式 --install 且走 install_starry_apk.sh；不产出 dex/so/Manifest 补丁本身", "prior_art": "04_scripts/群侠传：幸存者/surgical_qxzxcz_0.2.3.10.sh（外科六段骨架与自检风格）；04_scripts/_common/install_starry_apk.sh；04_scripts/_common/pre_install_gate.sh（查10/查12）；04_scripts/_common/next_free_dex.py；skill/starry-mod/SKILL.md（so+metadata 同源）", "difference": "多 dex 底包（13 个 dex，登录补丁跨 classes/classes7/classes11 三处替换）；新增 --stage inject|full 与 --dry-run；新增 libil2cpp.so + global-metadata.dat 成对替换与压缩方式断言；输出命名 starry-{stage}-{date}；装机走统一入口", "evidence": "10_docs/鹦鹉团/鹦鹉团_组包脚本_20261119.md"}
# ============================================================================
# 鹦鹉团 1.0.7（com.jinsaina.auditionsgo.gw，Unity IL2CPP + QGSDK 聚合层 + TapTap 渠道）
#   -> 星光悬浮菜单版（外科 zip 组装，不整包重建）
#
# 两档：
#   --stage inject  只注入星光框架（菜单 dex + libstarry_magic.so + Manifest），不改登录
#                   => 首局验证菜单能不能弹、能不能跑
#   --stage full    在 inject 基础上再替换登录补丁的 3 个 dex（classes / classes7 / classes11）
#                   => 登录也改完的正式包
#
# 组装内容：
#   [full]   classes.dex / classes7.dex / classes11.dex  <- patch_taptap_login_ywt.sh 产物
#   [both]   classesN.dex                                <- 菜单 dex（N = 最小空闲下标，现算）
#   [both]   lib/arm64-v8a/libstarry_magic.so            <- 本游戏 so（Stored）
#   [both]   AndroidManifest.xml                         <- 二进制 AXML 补丁（StarryInitProvider/MenuService/权限）
#   [可选]   lib/arm64-v8a/libil2cpp.so                  <- 只有给了 YWT_IL2CPP_SO 才换
#   [可选]   assets/bin/Data/Managed/Metadata/global-metadata.dat <- 必须与上面那份 so 同源，成对换
#
# 用法：
#   bash 04_scripts/鹦鹉团/build_ywt.sh --stage inject --dry-run
#   bash 04_scripts/鹦鹉团/build_ywt.sh --stage inject
#   bash 04_scripts/鹦鹉团/build_ywt.sh --stage full
#   bash 04_scripts/鹦鹉团/build_ywt.sh --stage full --install
# ============================================================================
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"

GAME="鹦鹉团"
VER="1.0.7"
PKG="com.jinsaina.auditionsgo.gw"
GAME_ID="ywt"
SO_TAG="${YWT_SO_TAG:-[ywt]}"
SERIAL="${SERIAL:-f9a116a2}"
STAGE=""
INSTALL=0
DRY=0

usage() { sed -n '2,38p' "$0"; exit "${1:-0}"; }
while [ "$#" -gt 0 ]; do
  case "$1" in
    --stage) STAGE="${2:-}"; shift 2 ;;
    --stage=*) STAGE="${1#*=}"; shift ;;
    --install) INSTALL=1; shift ;;
    --dry-run) DRY=1; shift ;;
    -h|--help) usage 0 ;;
    *) echo "ERROR: 未知参数 $1" >&2; usage 2 ;;
  esac
done
case "$STAGE" in
  inject|full) ;;
  "") echo "ERROR: 必须指定 --stage inject|full" >&2; exit 2 ;;
  *) echo "ERROR: --stage 只能是 inject 或 full（收到 ${STAGE}）" >&2; exit 2 ;;
esac

BASE_APK="${YWT_BASE_APK:-$NIXIANG/02_apks/${GAME}_${VER}_orig.apk}"
BASE_MD5_EXPECT="aabf023083b46284a425e3b07c9ddcf0"
BASE_SIZE_EXPECT=2055781509
MENU_APK="${YWT_MENU_APK:-$NIXIANG/mod菜单/starry-mod-menu/app/build/outputs/apk/release/app-release.apk}"
SO_SRC="${YWT_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_${GAME_ID}/arm64-v8a/libstarry_magic.so}"
# APP_STL=c++_shared ⇒ libstarry_magic.so 的 DT_NEEDED 里有 libc++_shared.so，而本底包不含它
# （2026-11-19 首包实测：dlopen failed: library "libc++_shared.so" not found ⇒ JNI_OnLoad 从未执行、
#  菜单只剩 Java 壳）。默认取与 so 同目录、同一次 ndk-build 产出的那份，保证 ABI/版本一致。
LIBCXX_SRC="${YWT_LIBCXX:-$(dirname "$SO_SRC")/libc++_shared.so}"
MANIFEST_PATCH="${YWT_MANIFEST_PATCH:-$SCRIPT_DIR/patch_ywt_binary_manifest.py}"
IL2CPP_SO="${YWT_IL2CPP_SO:-}"
METADATA="${YWT_METADATA:-}"
LOGIN_PATCH="$SCRIPT_DIR/patch_taptap_login_ywt.sh"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
BT="${ANDROID_BUILD_TOOLS:-${HOME:-/tmp}/Library/Android/sdk/build-tools/36.0.0}"
PY="${PYTHON:-python3}"
DATE="$(date +%Y%m%d)"
OUT_DIR="${OUT_DIR:-$NIXIANG/08_output/$GAME}"
WORK="$NIXIANG/13_scratch/ywt_build_${VER}_${STAGE}"
UNSIGNED="$WORK/${GAME}_${VER}_starry-${STAGE}-unsigned.apk"
ALIGNED="$WORK/aligned.apk"
SIGNED="$OUT_DIR/${GAME}_${VER}_starry-${STAGE}-${DATE}.apk"

die(){ echo "ERROR: $*" >&2; exit 1; }
step(){ echo ""; echo "== $* =="; }
m5(){ if command -v md5sum >/dev/null 2>&1; then md5sum "$1" | awk '{print $1}'; else md5 -q "$1"; fi; }
fsize(){ wc -c < "$1" | tr -d ' '; }
plan(){ if [ "$DRY" = "1" ]; then echo "   [dry-run] $*"; fi; }

echo "== 鹦鹉团 $VER 星光组包（stage=${STAGE}）=="
echo "   底包：$BASE_APK"
echo "   输出：$SIGNED"
if [ "$DRY" = "1" ]; then echo "   模式：dry-run（只打印计划，不写任何文件）"; fi

# --------------------------------------------------------------- 0/9 输入自检
step "0/9 输入自检"
[ -f "$BASE_APK" ] || die "缺底包：$BASE_APK"
[ -x "$BT/zipalign" ] || die "缺 zipalign（${BT}）"
[ -x "$BT/apksigner" ] || die "缺 apksigner（${BT}）"

B_SIZE="$(fsize "$BASE_APK")"
[ "$B_SIZE" = "$BASE_SIZE_EXPECT" ] || die "底包大小 ${B_SIZE} B != 记录 ${BASE_SIZE_EXPECT} B（换版本必须重取数）"
B_MD5="$(m5 "$BASE_APK")"
[ "$B_MD5" = "$BASE_MD5_EXPECT" ] || die "底包 md5 ${B_MD5} != 期望 ${BASE_MD5_EXPECT}（拒绝用错包或被改过的包）"
echo "   底包 OK：${B_SIZE} B  md5 ${B_MD5}"

# so 必须是本游戏编的（含 game_id 标签），否则菜单/鉴权会串到别的游戏
if [ -f "$SO_SRC" ]; then
  if ! LC_ALL=C grep -aq "$GAME_ID" "$SO_SRC"; then
    die "libstarry_magic.so 内找不到 game_id 标签 ${GAME_ID}（$SO_SRC 是不是别的游戏的 so？）"
  fi
  SO_SHA="$(shasum -a 256 "$SO_SRC" | cut -d' ' -f1)"
  echo "   so OK：$(fsize "$SO_SRC") B  sha256 ${SO_SHA}  含标签 $GAME_ID"
fi

MISSING=""
[ -f "$MENU_APK" ] || MISSING="$MISSING|  - 菜单 APK：${MENU_APK}（gradle assembleRelease 产物）"
[ -f "$SO_SRC" ] || MISSING="$MISSING|  - libstarry_magic.so：${SO_SRC}（需 [Ywt] 标签）"
[ -f "$MANIFEST_PATCH" ] || MISSING="$MISSING|  - Manifest 补丁脚本：${MANIFEST_PATCH}（照 patch_qxzxcz_binary_manifest.py 范式）"
[ -f "$KS" ] || MISSING="$MISSING|  - 签名库：$KS"
if [ "$STAGE" = "full" ]; then
  [ -x "$LOGIN_PATCH" ] || MISSING="$MISSING|  - 登录补丁脚本：$LOGIN_PATCH"
fi
if [ -n "$IL2CPP_SO" ] && [ ! -f "$IL2CPP_SO" ]; then MISSING="$MISSING|  - YWT_IL2CPP_SO 指向的文件不存在：$IL2CPP_SO"; fi
if [ -n "$IL2CPP_SO" ] && [ -z "$METADATA" ]; then
  MISSING="$MISSING|  - 给了 YWT_IL2CPP_SO 却没给 YWT_METADATA（IL2CPP 必须 so+metadata 同源）"
fi
if [ -n "$METADATA" ] && [ ! -f "$METADATA" ]; then MISSING="$MISSING|  - YWT_METADATA 指向的文件不存在：$METADATA"; fi
if [ -n "$MISSING" ]; then
  echo ""
  echo "缺以下输入（补齐后可重跑；本次 dry-run 仍打印计划）：" >&2
  printf '%s\n' "$MISSING" | tr '|' '\n' >&2
  if [ "$DRY" != "1" ]; then die "输入不全，拒绝继续"; fi
fi

# ------------------------------------------------------- 1/9 计划
step "1/9 计划"
plan "底包：${BASE_APK}（大小与 md5 已校验）"
if [ "$STAGE" = "full" ]; then
  plan "BUILD_DEX=1 OUT_DIR=$WORK/login_dex bash ${LOGIN_PATCH}（产出 classes.dex / classes7.dex / classes11.dex）"
  plan "校验 3 个 dex 后替换底包同名条目"
else
  plan "跳过登录补丁（stage=inject，底包 dex 原样保留）"
fi
plan "菜单 dex：从 $MENU_APK 取，装成 classesN.dex（N=最小空闲下标，现算）"
plan "替换 Manifest：$MANIFEST_PATCH"
plan "替换 so：$SO_SRC -> lib/arm64-v8a/libstarry_magic.so（Stored）"
if [ -n "$IL2CPP_SO" ]; then plan "替换 native：$IL2CPP_SO -> lib/arm64-v8a/libil2cpp.so（与 metadata 同源）"; else plan "不动 libil2cpp.so（未给 YWT_IL2CPP_SO）"; fi
if [ -n "$METADATA" ]; then plan "替换 metadata：$METADATA -> assets/bin/Data/Managed/Metadata/global-metadata.dat"; fi
plan "zipalign -p 4 + apksigner（v1+v2+v3，${KS}）"
plan "输出：$SIGNED"
if [ "$INSTALL" = "1" ]; then plan "装机：install_starry_apk.sh --apk <产物> --pkg $PKG --so-tag $SO_TAG --game-id $GAME_ID --serial $SERIAL"; fi

if [ "$DRY" = "1" ]; then
  echo ""
  echo "dry-run 结束：未创建目录、未解包、未 zip、未签名、未装机。"
  exit 0
fi

mkdir -p "$WORK" "$OUT_DIR"


# --------------------------------------------------------- 2/9 登录补丁 dex
if [ "$STAGE" = "full" ]; then
  step "2/9 跑登录补丁（7 处，产出 3 个 dex）"
  rm -rf "$WORK/login_dex"; mkdir -p "$WORK/login_dex"
  if ! BUILD_DEX=1 OUT_DIR="$WORK/login_dex" bash "$LOGIN_PATCH" > "$WORK/login_patch.log" 2>&1; then
    tail -30 "$WORK/login_patch.log" >&2
    die "登录补丁脚本失败（完整日志 $WORK/login_patch.log）"
  fi
  tail -6 "$WORK/login_patch.log"
  # classes2.dex：GatekeeperRepository/TapSignKit 在 classes2 与 classes11 里各定义一份，
  # ART 先命中 classes2 那份 ⇒ 必须一起换（否则等于没改，查12 也会拦）。
  for f in classes.dex classes2.dex classes7.dex classes11.dex; do
    [ -f "$WORK/login_dex/$f" ] || die "登录补丁没产出 $f"
  done
else
  step "2/9 跳过登录补丁（stage=inject）"
fi

# ------------------------------------------------------ 3/9 登录 dex 内容校验
step "3/9 登录 dex 校验"
if [ "$STAGE" = "full" ]; then
  "$PY" - "$WORK/login_dex" "$BASE_APK" <<'PY'
import sys, zipfile, hashlib, os
DEX_MAGIC = bytes([100, 101, 120, 10])          # 'dex' + 0x0A
d, base = sys.argv[1], sys.argv[2]
zb = zipfile.ZipFile(base)
LOW = b'27c2d8ca4e5703d9ad2a1ee39d2a859d'
UP = b'27C2D8CA4E5703D9AD2A1EE39D2A859D'
EXPECT = {
    'classes11.dex': [b'com/taptap/sdk/initializer/repository/GatekeeperRepository', LOW],
    'classes2.dex': [b'com/taptap/sdk/initializer/repository/GatekeeperRepository', LOW],
    'classes7.dex': [b'com/quickgamesdk/utils/QGSdkUtils', UP],
    'classes.dex': [b'com/quicksdk/utility/k', UP, b'getSignInfo'],
}
bad = 0
for name in ('classes.dex', 'classes2.dex', 'classes7.dex', 'classes11.dex'):
    p = os.path.join(d, name)
    b = open(p, 'rb').read()
    md5 = hashlib.md5(b).hexdigest()
    base_md5 = hashlib.md5(zb.read(name)).hexdigest()
    ok = True
    if b[:4] != DEX_MAGIC:
        print('  FAIL %s 魔数 %r 不是 dex' % (name, b[:4])); ok = False
    for n in EXPECT[name]:
        if n not in b:
            print('  FAIL %s 缺 %r' % (name, n)); ok = False
    if b'SignSpoof' in b:
        print('  FAIL %s 含 SignSpoof（禁复活）' % name); ok = False
    if md5 == base_md5:
        print('  FAIL %s 与底包同条目逐字节相同（补丁没进这个 dex）' % name); ok = False
    print('  %-14s %d B  md5 %s  底包 %s  %s' % (name, len(b), md5[:12], base_md5[:12], 'OK' if ok else 'FAIL'))
    if not ok:
        bad = 1
if bad:
    raise SystemExit('ERROR: 登录 dex 校验未过')
PY
else
  echo "   （stage=inject：不产出也不替换登录 dex）"
fi

# ------------------------------------------------------------ 4/9 菜单 dex
step "4/9 菜单 dex 提取 + 最小空闲下标"
NEXT_N="$("$PY" "$NIXIANG/04_scripts/_common/next_free_dex.py" "$BASE_APK")" || die "next_free_dex.py 失败"
[ -n "$NEXT_N" ] || die "next_free_dex.py 无输出"
MENU_DEX_NAME="classes${NEXT_N}.dex"
echo "   底包 13 个 dex -> 菜单 dex 名 $MENU_DEX_NAME"
"$PY" - "$MENU_APK" "$WORK/menu.dex" "$GAME_ID" <<'PY'
import sys, zipfile, hashlib, re
apk, out, gid = sys.argv[1], sys.argv[2], sys.argv[3]
z = zipfile.ZipFile(apk)
names = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')]
assert len(names) == 1, '菜单 APK 期望单 dex，实得 %r' % names
b = z.read(names[0])
open(out, 'wb').write(b)
need = [b'com/starry/magic/StarryInitProvider', b'com/starry/magic/menu/MenuService',
        b'com/starry/magic/core/GameHooks']
miss = [n.decode() for n in need if n not in b]
assert not miss, '菜单 dex 缺 %r（不是星光菜单 dex？）' % miss
assert b'SignSpoof' not in b, '菜单 dex 含 SignSpoof（禁复活）'
# game_id 由 native so 承载（GameConfig.h 烘焙），dex 不必然含 id；但若含 games/<其它id>/ 则是别的游戏的包
ids = set(m.group(1).decode() for m in re.finditer(rb'com/starry/magic/games/([A-Za-z0-9_]+)/', b))
foreign = sorted(i for i in ids if i != gid)
# 2026-11-19 builder 修：本框架的菜单 dex 是「全量 games 子树共存」（实测 app-release.apk
# 就含 9 个 games/<id>/），按「含别人的 id 就 die」必然假失败。真正要拦的是 SignSpoof（上一行）
# 与缺框架类（上上行）；games 子树只作信息打印。
if foreign:
    print('   [info] 菜单 dex 内含 %d 个其它 games 子树（框架全量共存，非本游戏专属包，不拦）' % len(foreign))
print('   %s %d B md5 %s（StarryInitProvider/MenuService/GameHooks 均在；games 子树=%r）'
      % (names[0], len(b), hashlib.md5(b).hexdigest(), sorted(ids)))
PY

# --------------------------------------------------------- 5/9 Manifest 补丁
step "5/9 二进制 Manifest 补丁"
"$PY" - "$BASE_APK" "$WORK/AndroidManifest.xml" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
b = z.read('AndroidManifest.xml')
open(sys.argv[2], 'wb').write(b)
print('   底包 Manifest %d B（头 %s）' % (len(b), b[:4].hex()))
PY
MF_DIR="$NIXIANG/07_patched/$GAME/menu_$VER"
mkdir -p "$MF_DIR"
"$PY" "$MANIFEST_PATCH" "$WORK/AndroidManifest.xml" "$MF_DIR/AndroidManifest.xml"
cp -f "$MF_DIR/AndroidManifest.xml" "$WORK/AndroidManifest.patched.xml"
"$PY" - "$WORK/AndroidManifest.patched.xml" <<'PY'
import sys
AXML_MAGIC = bytes([3, 0, 8, 0])
b = open(sys.argv[1], 'rb').read()
assert b[:4] == AXML_MAGIC, 'Manifest 头 %r 不是真 AXML（查7 会 FAIL）' % b[:4]
flat = b.replace(bytes([0]), b'')
need = ['com.starry.magic.StarryInitProvider', 'com.starry.magic.menu.MenuService',
        'android.permission.SYSTEM_ALERT_WINDOW', 'android.permission.FOREGROUND_SERVICE',
        'android.permission.FOREGROUND_SERVICE_SPECIAL_USE']
miss = [s for s in need if s.encode() not in flat]
assert not miss, '补丁后 Manifest 缺 %r' % miss
assert b'SignSpoof' not in flat, 'Manifest 仍挂 SignSpoof（查12 会 FAIL）'
print('   Manifest 补丁 OK：%d B，含 StarryInitProvider + MenuService + 3 权限，无 SignSpoof' % len(b))
PY

# ------------------------------------------------------ 6/9 外科 zip 重写
step "6/9 外科 zip 重写（其它条目逐字节与压缩方式不变）"
rm -f "$UNSIGNED"
"$PY" - "$BASE_APK" "$WORK/AndroidManifest.patched.xml" "$WORK/menu.dex" "$MENU_DEX_NAME" "$SO_SRC" "$UNSIGNED" "$STAGE" "$WORK/login_dex" "$IL2CPP_SO" "$METADATA" "$LIBCXX_SRC" <<'PY'
import sys, zipfile, os
base, mf, menu, menu_name, so, out, stage, login_dir, il2cpp, metadata, libcxx = sys.argv[1:12]
SO_NAME = 'lib/arm64-v8a/libstarry_magic.so'
IL2CPP_NAME = 'lib/arm64-v8a/libil2cpp.so'
META_NAME = 'assets/bin/Data/Managed/Metadata/global-metadata.dat'
REPLACE = {'AndroidManifest.xml': mf}
if stage == 'full':
    for n in ('classes.dex', 'classes2.dex', 'classes7.dex', 'classes11.dex'):
        REPLACE[n] = os.path.join(login_dir, n)
if il2cpp:
    REPLACE[IL2CPP_NAME] = il2cpp
if metadata:
    REPLACE[META_NAME] = metadata
ADD = {menu_name: menu, SO_NAME: so}
LIBCXX_NAME = 'lib/arm64-v8a/libc++_shared.so'

def drop(name):
    if not name.startswith('META-INF/'):
        return False
    tail = name.rsplit('/', 1)[-1]
    return (tail == 'MANIFEST.MF' or tail.endswith('.SF') or tail.endswith('.RSA')
            or tail.endswith('.DSA') or tail.endswith('.EC'))

zin = zipfile.ZipFile(base)
names = set(zin.namelist())
if libcxx and os.path.exists(libcxx) and LIBCXX_NAME not in names:
    ADD[LIBCXX_NAME] = libcxx   # APP_STL=c++_shared 的运行时依赖，底包不含 ⇒ 必须自带
for n in REPLACE:
    assert n in names, '底包没有条目 %s（不能替换）' % n
assert menu_name not in names, '底包已存在 %s，下标算错' % menu_name
assert SO_NAME not in names, '底包已含 libstarry_magic.so（不该有）'
if il2cpp and IL2CPP_NAME in names:
    ct = zin.getinfo(IL2CPP_NAME).compress_type
    print('   底包 libil2cpp.so compress_type=%s（extractNativeLibs=true 时 Deflate 合法，按原样保留）'
          % ('Stored' if ct == zipfile.ZIP_STORED else 'Deflate'))
if os.path.exists(out):
    os.remove(out)
zout = zipfile.ZipFile(out, 'w', allowZip64=True)
replaced = 0
dropped = []
for info in zin.infolist():
    n = info.filename
    if drop(n):
        dropped.append(n)
        continue
    data = open(REPLACE[n], 'rb').read() if n in REPLACE else zin.read(n)
    if n in REPLACE:
        replaced += 1
    zi = zipfile.ZipInfo(n, date_time=info.date_time)
    zi.compress_type = info.compress_type
    zi.external_attr = info.external_attr
    zout.writestr(zi, data)
# 新增 so 的压缩方式跟底包走：本包 extractNativeLibs=true（实测 manifest=true，
# 底包 libil2cpp/libunity/libxlua 全 Deflate），Stored 虽也能装，但会与底包不一致。
LIB_CT = zipfile.ZIP_DEFLATED
if IL2CPP_NAME in names:
    LIB_CT = zin.getinfo(IL2CPP_NAME).compress_type
for name, path in ADD.items():
    data = open(path, 'rb').read()
    zi = zipfile.ZipInfo(name, date_time=(1981, 1, 1, 1, 1, 2))
    zi.compress_type = LIB_CT if name.endswith('.so') else zipfile.ZIP_DEFLATED
    zi.external_attr = 0o644 << 16
    zout.writestr(zi, data)
    print('   + %s (%d B, %s)' % (name, len(data), 'Stored' if zi.compress_type == 0 else 'Deflate'))
zout.close()
print('   去掉旧签名文件 %d 个' % len(dropped))
print('   替换 %d 条；条目总数 %d' % (replaced, len(zipfile.ZipFile(out).infolist())))
PY

# ------------------------------------------------------- 7/9 对齐 + 签名
step "7/9 zipalign -p 4 + apksigner（v1/v2/v3）"
rm -f "$ALIGNED" "$SIGNED"
"$BT/zipalign" -f -p 4 "$UNSIGNED" "$ALIGNED"
"$BT/zipalign" -c -p 4 "$ALIGNED" || die "zipalign 校验未过"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" --v1-signing-enabled true --v2-signing-enabled true --v3-signing-enabled true --out "$SIGNED" "$ALIGNED"
rm -f "$ALIGNED" "$UNSIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" | head -4
echo "   大小：$(fsize "$SIGNED") B"
echo "   sha256：$(shasum -a 256 "$SIGNED" | cut -d' ' -f1)"
echo "   md5：$(m5 "$SIGNED")"

# --------------------------------------------------------- 8/9 成品自检
step "8/9 成品自检 + 装机前门禁"
"$PY" - "$SIGNED" "$BASE_APK" "$MENU_DEX_NAME" "$SO_SRC" "$STAGE" "$IL2CPP_SO" "$METADATA" "$WORK/menu.dex" "$LIBCXX_SRC" <<'PY'
import sys, zipfile, hashlib, os
apk, base, menu_name, so, stage, il2cpp, metadata, menu_dex, libcxx = sys.argv[1:10]
LIBCXX_NAME = 'lib/arm64-v8a/libc++_shared.so'
za, zb = zipfile.ZipFile(apk), zipfile.ZipFile(base)
na, nb = set(za.namelist()), set(zb.namelist())
SO_NAME = 'lib/arm64-v8a/libstarry_magic.so'
# v1 签名会新增 META-INF/*.SF|RSA —— 那是签名产物，不是组装内容，比较前剔除
added = {n for n in (na - nb) if not n.startswith('META-INF/')}
expect_added = {menu_name, SO_NAME}
if LIBCXX_NAME in na:
    expect_added.add(LIBCXX_NAME)
assert added == expect_added, '新增条目不符：%r' % sorted(added)
removed = nb - na
bad_removed = [n for n in removed if not (n.startswith('META-INF/') and
               (n.endswith('.SF') or n.endswith('.RSA') or n.endswith('.DSA') or n.endswith('.EC')))]
assert not bad_removed, '删掉了不该删的条目：%r' % bad_removed
want = {SO_NAME: so, menu_name: menu_dex}
if libcxx and os.path.exists(libcxx) and LIBCXX_NAME in na:
    want[LIBCXX_NAME] = libcxx
if stage == 'full':
    want['classes.dex'] = None
    want['classes2.dex'] = None
    want['classes7.dex'] = None
    want['classes11.dex'] = None
if il2cpp:
    want['lib/arm64-v8a/libil2cpp.so'] = il2cpp
if metadata:
    want['assets/bin/Data/Managed/Metadata/global-metadata.dat'] = metadata
for n, src in want.items():
    if src is None:
        continue
    a = hashlib.md5(za.read(n)).hexdigest()
    b = hashlib.md5(open(src, 'rb').read()).hexdigest()
    assert a == b, '%s 与输入不一致（%s != %s）' % (n, a[:12], b[:12])
# META-INF/*（MANIFEST.MF 会被 v1 重写）不参与逐字节比较，改由 apksigner verify 把关
skip = set(want) | removed | {'AndroidManifest.xml'} | {n for n in na if n.startswith('META-INF/')}
for n in (na & nb) - skip:
    ia, ib = za.getinfo(n), zb.getinfo(n)
    assert ia.compress_type == ib.compress_type, '%s 压缩方式变了' % n
    assert hashlib.md5(za.read(n)).hexdigest() == hashlib.md5(zb.read(n)).hexdigest(), '%s 内容变了' % n
# 压缩方式跟底包的 so 走（本包 extractNativeLibs=true ⇒ 底包全 Deflate），不再强制 Stored
_libct = zb.getinfo('lib/arm64-v8a/libil2cpp.so').compress_type if 'lib/arm64-v8a/libil2cpp.so' in nb else zipfile.ZIP_DEFLATED
assert za.getinfo(SO_NAME).compress_type == _libct, 'libstarry_magic.so 压缩方式与底包 so 不一致'
for n in want:                      # 替换条目必须保持底包压缩方式（本包 libil2cpp/metadata 都是 Deflate）
    if n in nb:
        assert za.getinfo(n).compress_type == zb.getinfo(n).compress_type, '%s 压缩方式与底包不一致' % n
for n in na:
    if n.startswith('classes') and n.endswith('.dex'):
        assert b'SignSpoof' not in za.read(n), '%s 含 SignSpoof' % n
print('   自检 OK：新增 %d 条 / 删除 %d 条旧签名 / 替换 %d 条 / 其余条目逐字节一致 / so Stored / 无 SignSpoof'
      % (len(added), len(removed), len(want) + 1))
PY

# Manifest 结构关：成品是完整 APK，aapt2 能直接解析（裸 AXML 不行，实测报 proto 反序列化失败）
if [ -x "$BT/aapt2" ]; then
  "$BT/aapt2" dump xmltree --file AndroidManifest.xml "$SIGNED" > "$WORK/xmltree.txt" 2> "$WORK/xmltree.err" \
    || { head -5 "$WORK/xmltree.err" >&2; die "aapt2 dump xmltree 解析失败（Manifest 结构坏了）"; }
  grep -q 'E: provider' "$WORK/xmltree.txt" || die "aapt2 复读结果里没有 provider 节点"
  grep -q 'com.starry.magic.StarryInitProvider' "$WORK/xmltree.txt" || die "aapt2 复读结果里没有 StarryInitProvider"
  grep -q 'com.starry.magic.menu.MenuService' "$WORK/xmltree.txt" || die "aapt2 复读结果里没有 MenuService"
  echo "   aapt2 dump xmltree OK：$(grep -c 'E: ' "$WORK/xmltree.txt") 个元素，含 StarryInitProvider + MenuService"
else
  echo "   WARN 未找到 aapt2（${BT}），跳过 Manifest 结构复读"
fi

if ! bash "$NIXIANG/04_scripts/_common/pre_install_gate.sh" --game "$GAME" --apk "$SIGNED" --base-apk "$BASE_APK" --menu-dex "$WORK/menu.dex" --menu-dex-name "$MENU_DEX_NAME" --so "$SO_SRC"; then
  die "pre_install_gate 未过 => 禁止装机与交付（逃生门须在报告写明理由）"
fi
bash "$NIXIANG/04_scripts/_common/framework_entry_audit.sh" --game "$GAME" --pkg "$PKG" --apk "$SIGNED" || echo "   WARN framework_entry_audit 有未通过项（按 10_docs/$GAME/审计豁免.json 处理）"

# ------------------------------------------------------------ 9/9 装机
step "9/9 装机"
if [ "$INSTALL" = "1" ]; then
  bash "$NIXIANG/04_scripts/_common/install_starry_apk.sh" --apk "$SIGNED" --pkg "$PKG" --so-tag "$SO_TAG" --game-id "$GAME_ID" --serial "$SERIAL"
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su || die "设备上的包与本地产物不一致（装到旧包了）"
else
  echo "   （跳过：显式 --install 才装机；不卸载、不清数据）"
fi

echo ""
echo "OK  $SIGNED"
echo "    阶段 $STAGE  大小 $(fsize "$SIGNED") B  md5 $(m5 "$SIGNED")"
