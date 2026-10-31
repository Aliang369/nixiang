#!/usr/bin/env bash
# 我本布衣：世家录 — classes2.dex（TapSDK 所在 dex）验签/登录改包（A 轨）
#
# 产出：mod菜单/starry-mod-menu/templates/wbjy_menu_current/classes2_loginpatched.dex
#       （surgical_wbjy_1.1.66.sh 用它替换官方 classes2.dex）
#
# 为什么改 classes2.dex 整个回填：TapSDK 全在 classes2.dex（GatekeeperRepository /
# InitializerServiceImpl / Actions$CN），官方其它 dex 保持逐字节不变。
#
# 补丁内容见 patch_wbjy_tapsdk_login.py 头注（3 处；不含 checkInitialize 恒真——已实测否证）。
#
# 用法：bash 04_scripts/我本布衣世家录/wbjy_patch_login_dex.sh
set -uo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NIXIANG="$(cd "$HERE/../.." && pwd)"
PY="${NIXIANG_PYTHON:-python3}"
STAR="$NIXIANG/mod菜单/starry-mod-menu"
TPL="$STAR/templates/wbjy_menu_current"
BASE="${WBJY_BASE_APK:-$NIXIANG/.workbuddy/scratch/wbjy_base/我本布衣世家录_1.1.66_vc94_base.apk}"
ENTRY="classes2.dex"
OUT="$TPL/classes2_loginpatched.dex"
WORK="$NIXIANG/.workbuddy/scratch/wbjy_login"

die() { echo "ERROR: $*" >&2; exit 1; }

BAKSMALI="$NIXIANG/01_tools/smali/baksmali-fat.jar"
SMALI="$NIXIANG/01_tools/smali/smali-fat.jar"
[[ -f "$BAKSMALI" ]] || die "缺 $BAKSMALI"
[[ -f "$SMALI" ]]    || die "缺 $SMALI"
[[ -f "$BASE" ]]     || die "缺底包 $BASE"

mkdir -p "$TPL" "$WORK"
rm -rf "$WORK/smali" "$WORK/in.dex" "$WORK/out.dex"
rm -f "$OUT"

echo "[1/5] 取官方 $ENTRY"
unzip -p "$BASE" "$ENTRY" > "$WORK/in.dex" || die "unzip 失败"
echo "      $(wc -c <"$WORK/in.dex" | tr -d ' ') B  md5=$(md5sum "$WORK/in.dex" | awk '{print $1}')"

echo "[2/5] baksmali → $WORK/smali"
java -Xmx3g -jar "$BAKSMALI" d "$WORK/in.dex" -o "$WORK/smali" || die "baksmali 失败"

echo "[3/5] 打补丁"
"$PY" "$HERE/patch_wbjy_tapsdk_login.py" "$WORK/smali" || die "smali 补丁失败"

echo "[4/5] smali → $OUT"
java -Xmx3g -jar "$SMALI" a -o "$WORK/out.dex" "$WORK/smali" || die "smali 汇编失败"
[[ -f "$WORK/out.dex" ]] || die "未产出 dex"
cp -f "$WORK/out.dex" "$OUT"
echo "      in=$(wc -c <"$WORK/in.dex" | tr -d ' ') B out=$(wc -c <"$OUT" | tr -d ' ') B"

echo "[5/5] 回读断言"
"$PY" - "$OUT" <<'PY' || die "回读断言失败"
import sys
b = open(sys.argv[1], 'rb').read()
need = [b'com/taptap/sdk/initializer/repository/GatekeeperRepository',
        b'com/taptap/sdk/initializer/service/InitializerServiceImpl',
        b'com/taptap/sdk/login/impl/data/model/Actions$CN']
for n in need:
    assert n in b, 'missing ' + n.decode()
assert b'com.taptap.sdk.act1on' in b, '登录常量替换未生效'
assert b'com.taptap.sdk.action' not in b, '旧登录常量仍在'
print('  dex 回读 OK（TapSDK 三类在；action 常量已换）')
PY

echo "OK → $OUT"
echo "sha256=$(sha256sum "$OUT" | awk '{print $1}')"
