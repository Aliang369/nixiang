#!/usr/bin/env bash
# ============================================================
#  无限升级 —— TapTap 去验签 + 网页登录「后处理」脚本（2026-11-18）
#
#  背景（10_docs/无限升级/去tap验签与网页登录_20261118.md）：
#    13.101 / vc1650 底包是网易易盾壳，包内只有 1 个 classes.dex（59,596 B，全是
#    com/netease/nis/wrapper/* 壳类），com/taptap/sdk/*、GatekeeperRepository、TapSignKit
#    一个都搜不到 ⇒ 真实 dex 在壳 payload 里，**静态改包法没有落点**。
#    故 TapTap 侧改两件事，都不碰游戏自己的 dex：
#      1) 登录：框架 SPI games/wxsj/GameHookImpl → TapLoginForceWeb（强制内嵌网页登录）
#      2) 验签：SigReplace 运行时签名替换（PackageInfo 签名 → 官方 cert MD5 40fb0d30…）
#    两者都装在 starry 菜单 dex（classes2.dex）里，本脚本负责外科塞进 APK 并重签。
#
#  用法:
#    bash 04_scripts/无限升级/patch_wxsj_tap_dex.sh <输入.apk> [--dex <dex>] [--out <apk>] [--twice] [--probe]
#
#    默认 --dex = mod菜单/starry-mod-menu/templates/wxsj_menu_current_sig/classes2.dex
#    默认 --out = <输入目录>/<输入名去 .apk>_tapfix-releaseSigned.apk
#
#  --probe 只做「TapSDK 是否 dex 内可达」判定并退出（0=可达 / 3=不可达）
#  --twice 自检：整套流程跑两遍，断言两次产物 classes2.dex 段 md5 一致（幂等）
#
#  固定流程（lead 2026-11-18 定的接口）：删 META-INF → 外科替换/追加 classes2.dex
#    → zipalign -p 4 → apksigner sign(01_tools/starry_release.jks) → apksigner verify --print-certs
#
#  纪律：官方 classes.dex / resources.arsc / libil2cpp.so 逐字节透传（只换 starry 菜单 dex）；
#        不做 apktool b 整包重建；不碰 Manifest。
# ============================================================
set -o pipefail

NIXIANG="$(cd "$(dirname "$0")/../.." && pwd)"

IN_APK=""
DEX="$NIXIANG/mod菜单/starry-mod-menu/templates/wxsj_menu_current_sig/classes2.dex"
OUT_APK=""
DO_TWICE=0
DO_PROBE=0
KS="$NIXIANG/01_tools/starry_release.jks"
KS_ALIAS="starry"
KS_PASS="starry2026"
OFFICIAL_CERT_MD5="40fb0d3074290c0fd60a0dd5303b1757"

if [ -n "$WXSJ_TAP_DEX" ]; then DEX="$WXSJ_TAP_DEX"; fi
if [ -n "$STARRY_KS" ]; then KS="$STARRY_KS"; fi
if [ -n "$STARRY_KS_ALIAS" ]; then KS_ALIAS="$STARRY_KS_ALIAS"; fi
if [ -n "$STARRY_KS_PASS" ]; then KS_PASS="$STARRY_KS_PASS"; fi

while [ $# -gt 0 ]; do
  case "$1" in
    --dex)   DEX="$2"; shift 2 ;;
    --out)   OUT_APK="$2"; shift 2 ;;
    --twice) DO_TWICE=1; shift ;;
    --probe) DO_PROBE=1; shift ;;
    -h|--help) sed -n '2,30p' "$0"; exit 0 ;;
    -*) echo "ERROR: 未知参数 $1"; exit 2 ;;
    *)  if [ -z "$IN_APK" ]; then IN_APK="$1"; else echo "ERROR: 多余参数 $1"; exit 2; fi; shift ;;
  esac
done

[ -n "$IN_APK" ] || { echo "ERROR: 缺输入 APK"; sed -n '2,30p' "$0"; exit 2; }
[ -f "$IN_APK" ] || { echo "ERROR: 输入 APK 不存在: $IN_APK"; exit 2; }

PY=""
for c in python3.11 python3 python; do
  if command -v "$c" >/dev/null 2>&1; then PY="$c"; break; fi
done
[ -n "$PY" ] || { echo "ERROR: 缺 python3（zip 外科需要）"; exit 2; }

# ---------- probe：TapSDK 是否 dex 内可达 ----------
if [ "$DO_PROBE" = "1" ]; then
  WORK_P="$(mktemp -d /tmp/wxsj_probe.XXXXXX)" || exit 2
  trap 'rm -rf "$WORK_P"' EXIT
  echo "== TapSDK 可达性 probe =="
  echo "   APK: $IN_APK"
  "$PY" - "$IN_APK" "$WORK_P" <<'PYEOF'
import sys, zipfile, os, hashlib
apk, out = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
dexes = [n for n in z.namelist() if n.startswith('classes') and n.endswith('.dex')]
print('   dex 数: %d  %s' % (len(dexes), ', '.join(dexes)))
for n in dexes:
    data = z.read(n)
    open(os.path.join(out, n), 'wb').write(data)
    print('   %-16s %9d B  md5=%s' % (n, len(data), hashlib.md5(data).hexdigest()))
PYEOF
  FOUND=0
  for pat in 'Lcom/taptap/sdk' 'GatekeeperRepository' 'TapSignKit' 'InitializerServiceImpl' 'LoginActivityImpl' 'DefaultTapLoginFragmentProvider'; do
    tot=0
    for f in "$WORK_P"/classes*.dex; do
      [ -f "$f" ] || continue
      n=$(LC_ALL=C grep -a -c -- "$pat" "$f" 2>/dev/null)
      n=$((n + 0))
      tot=$((tot + n))
    done
    printf '   %-38s %s\n' "$pat" "$tot"
    if [ "$tot" -gt 0 ]; then FOUND=$((FOUND + 1)); fi
  done
  if [ "$FOUND" = "0" ]; then
    echo "   => UNREACHABLE：6 个 TapSDK 锚点全部 0 命中 ⇒ 真实 dex 在壳 payload 里，改包法无落点"
    echo "   => 走运行时 SPI（games/wxsj/GameHookImpl + TapLoginForceWeb）+ SigReplace 签名轨"
    exit 3
  fi
  echo "   => REACHABLE：$FOUND 组锚点命中 ⇒ 可走静态改包法（skill/re-analyze/login/taptap-web-login.md 四步表）"
  exit 0
fi

# ---------- 后处理：换 starry 菜单 dex + 重签 ----------
[ -f "$DEX" ] || { echo "ERROR: 缺 starry 菜单 dex: ${DEX}（先跑 wxsj_refresh_menu_tpl.sh 与 build_menu_dex_sigreplace_1210701.sh）"; exit 1; }
[ -f "$KS" ] || { echo "ERROR: 缺签名库 $KS"; exit 1; }

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [ -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[ -n "$BT" ] || { echo "ERROR: 找不到 Android build-tools（apksigner/zipalign）"; exit 1; }

if [ -z "$OUT_APK" ]; then
  IN_DIR="$(cd "$(dirname "$IN_APK")" && pwd)"
  IN_STEM="$(basename "$IN_APK" .apk)"
  OUT_APK="$IN_DIR/$IN_STEM""_tapfix-releaseSigned.apk"
fi
mkdir -p "$(dirname "$OUT_APK")"

WORK="$(mktemp -d /tmp/wxsj_tapfix.XXXXXX)" || exit 1
trap 'rm -rf "$WORK"' EXIT

DEX_MD5="$(md5 -q "$DEX")"
echo "== 无限升级 TapTap 后处理 =="
echo "   输入 APK : $IN_APK"
echo "   菜单 dex : $DEX"
echo "              $DEX_MD5  ($(stat -f %z "$DEX") B)"
echo "   输出 APK : $OUT_APK"
echo "   签名库   : $KS (alias=$KS_ALIAS)"

LC_ALL=C grep -aq 'Lcom/starry/magic/games/wxsj/GameHookImpl' "$DEX" || { echo "ERROR: 菜单 dex 缺 games/wxsj/GameHookImpl（模板太旧？）"; exit 1; }
LC_ALL=C grep -aq 'Lcom/starry/magic/core/GameHooks' "$DEX" || { echo "ERROR: 菜单 dex 缺框架 SPI 装载器 GameHooks（模板太旧？）"; exit 1; }
LC_ALL=C grep -aq 'Lcom/starry/magic/sign/SigReplace' "$DEX" || { echo "ERROR: 菜单 dex 缺 SigReplace 签名轨（要用 wxsj_menu_current_sig）"; exit 1; }
LC_ALL=C grep -aq 'nativeSetStaticField' "$DEX" || { echo "ERROR: 菜单 dex 缺 nativeSetStaticField 引用"; exit 1; }
echo "   dex 断言: GameHookImpl / GameHooks / SigReplace / nativeSetStaticField 全命中 OK"

build_once() {
  local out="$1"
  local stage="$WORK/stage.zip"
  "$PY" - "$IN_APK" "$stage" "$DEX" <<'PYEOF'
import sys, zipfile, hashlib
src, dst, newdex = sys.argv[1], sys.argv[2], sys.argv[3]
dex_bytes = open(newdex, 'rb').read()
zin = zipfile.ZipFile(src)
names = zin.namelist()
have_c2 = 'classes2.dex' in names
if have_c2:
    old = zin.read('classes2.dex')
    print('   / 输入 classes2.dex: %d B md5=%s' % (len(old), hashlib.md5(old).hexdigest()))
else:
    print('   / 输入无 classes2.dex（将追加）')
print('   / 替换为           : %d B md5=%s' % (len(dex_bytes), hashlib.md5(dex_bytes).hexdigest()))
written = []
with zipfile.ZipFile(dst, 'w') as zout:
    for info in zin.infolist():
        n = info.filename
        if n.startswith('META-INF/'):
            continue
        if n == 'classes2.dex':
            continue
        data = zin.read(n)
        zi = zipfile.ZipInfo(n, date_time=(1981, 1, 1, 1, 1, 0))
        zi.compress_type = info.compress_type
        zi.external_attr = info.external_attr
        zout.writestr(zi, data)
        written.append(n)
        if n == 'classes.dex':
            zi2 = zipfile.ZipInfo('classes2.dex', date_time=(1981, 1, 1, 1, 1, 0))
            zi2.compress_type = zipfile.ZIP_DEFLATED
            zout.writestr(zi2, dex_bytes)
            written.append('classes2.dex')
    if 'classes2.dex' not in written:
        zi2 = zipfile.ZipInfo('classes2.dex', date_time=(1981, 1, 1, 1, 1, 0))
        zi2.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(zi2, dex_bytes)
print('   / 写出 %d 个条目（META-INF 已剔除，classes2.dex 就位）' % len(written))
PYEOF
  [ -s "$stage" ] || { echo "ERROR: 组包失败"; return 1; }
  "$BT/zipalign" -f -p 4 "$stage" "$WORK/aligned.apk" || { echo "ERROR: zipalign 失败"; return 1; }
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" --out "$out" "$WORK/aligned.apk" || { echo "ERROR: apksigner sign 失败"; return 1; }
  return 0
}

echo "[1/4] 外科组包 + zipalign + apksigner sign"
build_once "$OUT_APK" || exit 1

echo "[2/4] 校验产物"
"$BT/apksigner" verify --print-certs "$OUT_APK" > "$WORK/verify.txt" 2>&1
grep -E 'Signer #1 certificate (DN|MD5)' "$WORK/verify.txt" | sed 's/^/   /'
OUT_DEX_MD5="$("$PY" - "$OUT_APK" <<'PYEOF'
import sys, zipfile, hashlib
print(hashlib.md5(zipfile.ZipFile(sys.argv[1]).read('classes2.dex')).hexdigest())
PYEOF
)"
echo "   产物 classes2.dex md5 = $OUT_DEX_MD5"
[ "$OUT_DEX_MD5" = "$DEX_MD5" ] || { echo "ERROR: 产物菜单 dex 与模板不一致（$OUT_DEX_MD5 != ${DEX_MD5}）"; exit 1; }
"$PY" - "$IN_APK" "$OUT_APK" <<'PYEOF'
import sys, zipfile, hashlib
a, b = zipfile.ZipFile(sys.argv[1]), zipfile.ZipFile(sys.argv[2])
guard = ['classes.dex', 'resources.arsc', 'lib/arm64-v8a/libil2cpp.so']
for n in guard:
    if n not in a.namelist():
        print('   ! 输入缺 %s（跳过该断言）' % n)
        continue
    ha = hashlib.sha256(a.read(n)).hexdigest()
    hb = hashlib.sha256(b.read(n)).hexdigest() if n in b.namelist() else 'MISSING'
    assert ha == hb, '官方件被改动: %s' % n
    print('   / 透传 OK %s  sha256=%s...' % (n, ha[:16]))
print('   / META-INF 条目: 旧=%d 新=%d' % (
    len([n for n in a.namelist() if n.startswith('META-INF/')]),
    len([n for n in b.namelist() if n.startswith('META-INF/')])))
PYEOF
[ $? -eq 0 ] || { echo "ERROR: 官方件透传断言失败"; exit 1; }

if [ "$DO_TWICE" = "1" ]; then
  echo "[3/4] 幂等自检（整套流程重跑一遍，比 classes2.dex 段 md5）"
  TMP_OUT="$WORK/second.apk"
  build_once "$TMP_OUT" || exit 1
  A="$("$PY" - "$OUT_APK" <<'PYEOF'
import sys, zipfile, hashlib
print(hashlib.md5(zipfile.ZipFile(sys.argv[1]).read('classes2.dex')).hexdigest())
PYEOF
)"
  B="$("$PY" - "$TMP_OUT" <<'PYEOF'
import sys, zipfile, hashlib
print(hashlib.md5(zipfile.ZipFile(sys.argv[1]).read('classes2.dex')).hexdigest())
PYEOF
)"
  echo "   第 1 次 dex md5 = $A"
  echo "   第 2 次 dex md5 = $B"
  [ "$A" = "$B" ] || { echo "ERROR: 两次 classes2.dex 段 md5 不一致 => 不幂等"; exit 1; }
  APK_A="$(md5 -q "$OUT_APK")"
  APK_B="$(md5 -q "$TMP_OUT")"
  echo "   整包 md5: $APK_A / $APK_B"
  if [ "$APK_A" = "$APK_B" ]; then echo "   OK 幂等：两遍整包字节完全一致"; else echo "   OK 幂等：dex 段一致；整包 md5 不同（签名块非确定性）"; fi
else
  echo "[3/4] 跳过幂等自检（加 --twice 才跑）"
fi

echo "[4/4] 完成"
echo "   APK   : $OUT_APK"
echo "   大小  : $(stat -f %z "$OUT_APK") B"
echo "   md5   : $(md5 -q "$OUT_APK")"
echo "   sha256: $(shasum -a 256 "$OUT_APK" | awk '{print $1}')"
echo "   官方 cert MD5（回灌目标，须与 SigReplace 常量一致）: $OFFICIAL_CERT_MD5"
echo "   装机判据: logcat 见 game hook 已装载: com.starry.magic.games.wxsj.GameHookImpl"
echo "             logcat 见 StarrySigReplace: jni static-field write: ok=true"
echo "             logcat 无 TapSDK 包名/签名报错；登录页为内嵌 WebView（非 TapTap 客户端）"
