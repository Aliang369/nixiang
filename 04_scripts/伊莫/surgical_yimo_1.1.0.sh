#!/usr/bin/env bash
# ============================================================================
# 伊莫 1.1.0（TapTap 渠道原版，com.x.yimoo.official vc7）→ 星空版 外科 zip 组装
# Windows(Git-Bash) 可用版；mac 上同样可跑（路径全走 cygpath/相对）
#
# 底包：雷电模拟器已装的 TapTap 渠道原版（448,687,279 B，md5 9c7c76ac…，vc7）
#       官方签名 CN=FUNPLUS（SHA-256 4657fa90… / MD5 c5fadd98…，与 1.0.1 同一把证书）
#       网易易盾加固：Application = com.netease.nis.wrapper.MyApplication，classes.dex 只有 63 KB 壳桩
#
# 三条轨：
#   ① 玩法 = xLua 层注入（不变）：libstarry_magic.so 走 dlsym(libxlua.so)+指纹后 DobbyHook
#      lua_pcall/lua_gettop，跑 kYimoPatch。六项功能与 1.0.1 完全一致
#      （Toggle_精灵攻击秒杀 / Slider_移速 / Toggle_玩家无敌 / Toggle_无限跳跃 /
#        Toggle_无限体力 / Toggle_瞬移采集）。
#   ② 入口 = ContentProvider（StarryInitProvider，**排在所有其它 provider 之前**）+ 早启动
#      appComponentFactory（com.starry.magic.sign.StarryAppComponentFactory，只委托
#      androidx.core.app.CoreComponentFactory，无签名伪装）。**不动**易盾 MyApplication。
#   ③ 验签 = 运行时签名替换（2026-10-29 双轨制的签名轨）：注入 MT ApkSignatureKillerEx
#      （bin.mt.signature.KillerApplication193），由 StarryAppComponentFactory 在
#      instantiateApplication 里 Class.forName(...,true) 触发 <clinit>
#      （换 PackageInfo.CREATOR + 清 PackageManager.sPackageInfoCache / Parcel.mCreators /
#        sPairedCreators）。killer 的包名与官方证书 base64 由本脚本按**本底包**现取现改。
#      ★为什么不走「改包法」登录轨：本作真实 dex 被易盾加密（classes.dex 仅 63 KB 桩），
#        静态改 ClientLoginUtils/GatekeeperRepository 无从下手；签名轨让 TapTap SDK 读到
#        官方证书，SSO/门闩自然通过。若实测登录仍失败，再按
#        10_docs/伊莫/移植记录_星空版_1.1.0_*.md 的「登录轨备选」处理。
#
# 用法：
#   bash 04_scripts/伊莫/surgical_yimo_1.1.0.sh                  # 构建+签名+自检
#   STARRY_INSTALL=1 bash 04_scripts/伊莫/surgical_yimo_1.1.0.sh # 同上 + 装机（同签覆盖）
#   STARRY_ALLOW_UNINSTALL=1 STARRY_KEEP_DATA=1 STARRY_INSTALL=1 ...  # 异签：uninstall -k 保数据后装
#
# 参数：
#   YIMO_ORIG_APK   底包（默认 09_test/伊莫/orig/yimoo_110_taptap_base.apk）
#   STARRY_MENU_APK 菜单 APK（默认 E:/starry_build/starry-mod-menu/app/build/outputs/apk/release/app-release.apk）
#   YIMO_SO         libstarry_magic.so（默认 09_test/_modlibs_yimo/arm64-v8a/）
#   YIMO_KILLER_SMALI killer smali 目录（默认 07_patched/魂坠深境/surgical/killer_smali）
#   SERIAL          adb 序列号（默认 emulator-5554）
# ============================================================================
set -euo pipefail
export MSYS_NO_PATHCONV=1

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
W() { cygpath -m "$1"; }

GAME="伊莫"
VER="1.1.0"
PKG="com.x.yimoo.official"
TAG="Yimo"
GAME_ID="ym"
SERIAL="${SERIAL:-emulator-5554}"

ORIG="${YIMO_ORIG_APK:-$NIXIANG/09_test/${GAME}/orig/yimoo_110_taptap_base.apk}"
MENU_APK="${STARRY_MENU_APK:-E:/starry_build/starry-mod-menu/app/build/outputs/apk/release/app-release.apk}"
# 诊断包（STARRY_DIAG=1）：用独立 obj/libs 目录编出的诊断 so，产物名带 -diag，不覆盖正式包
DIAG="${STARRY_DIAG:-0}"
DIAG_SUFFIX=""
if [ "$DIAG" = "1" ]; then DIAG_SUFFIX="-diag"; fi
SO_SRC="${YIMO_SO:-$NIXIANG/09_test/_modlibs_yimo${DIAG_SUFFIX:+_diag}/arm64-v8a/libstarry_magic.so}"
CXX_SRC="$(dirname "$SO_SRC")/libc++_shared.so"
KILLER_SMALI="${YIMO_KILLER_SMALI:-$NIXIANG/07_patched/魂坠深境/surgical/killer_smali}"

OUT="$NIXIANG/08_output/${GAME}"
UNSIGNED="$OUT/${GAME}_${VER}_surgical${DIAG_SUFFIX}-unsigned.apk"
SIGNED="$OUT/${GAME}_${VER}_surgical${DIAG_SUFFIX}-releaseSigned.apk"
WORK="$NIXIANG/07_tmp/yimo_110"
MF_CACHE="$NIXIANG/07_patched/${GAME}/surgical_${VER}/AndroidManifest.xml"

JAVA_BIN="${JAVA_BIN:-java}"
APKTOOL_JAR="$NIXIANG/01_tools/apktool/apktool.jar"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-fat.jar"
BAKSMALI_JAR="$NIXIANG/01_tools/smali/baksmali-fat.jar"
BT="${ANDROID_BUILD_TOOLS:-C:/Users/Administrator/AppData/Local/Android/Sdk/build-tools/36.1.0}"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
PY="${PYTHON:-python}"
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8

die() { echo "ERROR: $*" >&2; exit 1; }
step() { echo ""; echo "== $* =="; }

[ -f "$ORIG" ] || die "缺底包 $ORIG"
[ -f "$MENU_APK" ] || die "缺菜单 APK $MENU_APK"
[ -f "$SO_SRC" ] || die "缺 so ${SO_SRC}（先跑 04_scripts/伊莫/build_so_yimo_win.sh）"
[ -f "$CXX_SRC" ] || die "缺 libc++_shared.so $CXX_SRC"
[ -d "$KILLER_SMALI/bin/mt/signature" ] || die "缺 killer smali $KILLER_SMALI"
[ -f "$APKTOOL_JAR" ] || die "缺 apktool.jar"
[ -f "$SMALI_JAR" ] || die "缺 smali-fat.jar"
[ -f "$BT/zipalign.exe" ] || [ -f "$BT/zipalign" ] || die "缺 zipalign（${BT}）"
[ -f "$BT/apksigner.bat" ] || [ -f "$BT/apksigner" ] || die "缺 apksigner（${BT}）"
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"
W_ORIG="$(W "$ORIG")"; W_MENU="$(W "$MENU_APK")"; W_SO="$(W "$SO_SRC")"; W_CXX="$(W "$CXX_SRC")"
W_WORK="$(W "$WORK")"; W_OUT="$(W "$OUT")"; W_UNSIGNED="$(W "$UNSIGNED")"; W_SIGNED="$(W "$SIGNED")"
W_KS="$(W "$KS")"; W_KILLER="$(W "$KILLER_SMALI")"; W_MFCACHE="$(W "$MF_CACHE")"

step "0/7 输入自检"
echo "  orig      = $W_ORIG ($(wc -c < "$ORIG") B, md5 $(md5sum "$ORIG" | awk '{print $1}'))"
echo "  menu apk  = $W_MENU ($(wc -c < "$MENU_APK") B)"
echo "  so        = $W_SO ($(wc -c < "$SO_SRC") B, md5 $(md5sum "$SO_SRC" | awk '{print $1}'))"
echo "  killer    = $W_KILLER"
LC_ALL=C grep -aq "\[$TAG\]" "$SO_SRC" || die "so 缺 [$TAG] 标签（仍是上一款游戏）"
for m in 'Toggle_精灵攻击秒杀' 'Slider_移速' 'Toggle_玩家无敌' 'Toggle_无限跳跃' 'Toggle_无限体力' 'Toggle_瞬移采集'; do
  N=$(LC_ALL=C grep -ac "$m" "$SO_SRC" || true)
  [ "${N:-0}" -ge 1 ] || die "so 缺菜单串 $m"
done
echo "  so 六项菜单串齐全 + [$TAG] 标签在"

step "1/7 官方证书 DER（喂给 killer 的 killPM）"
rm -f "$WORK"/official.* "$WORK"/c2ce*
"$PY" - "$W_ORIG" "$W_WORK" <<'PY'
import sys, zipfile, os
apk, work = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
rsa = [n for n in z.namelist() if n.startswith('META-INF/') and n.upper().endswith('.RSA')]
assert len(rsa) == 1, '期望恰有 1 个 v1 签名块 .RSA，实得 %r' % rsa
open(os.path.join(work, 'sig.rsa'), 'wb').write(z.read(rsa[0]))
print('  v1 签名块 =', rsa[0], z.getinfo(rsa[0]).file_size, 'B')
PY
# ★openssl 是 mingw 原生程序，必须喂 Windows 形式路径（MSYS_NO_PATHCONV=1 下 /e/... 传不过去）
openssl pkcs7 -inform DER -in "$W_WORK/sig.rsa" -print_certs -out "$W_WORK/cert.pem" 2>/dev/null \
  || die "openssl 解 pkcs7 失败"
openssl x509 -in "$W_WORK/cert.pem" -outform DER -out "$W_WORK/official.der" || die "导出 DER 失败"
DER_MD5=$(md5sum "$WORK/official.der" | awk '{print $1}')
SIGNER_MD5=$("$BT/apksigner.bat" verify --print-certs "$W_ORIG" 2>/dev/null | sed -n 's/^Signer #1 certificate MD5 digest: //p' | head -1)
echo "  官方证书 DER md5 = ${DER_MD5}（apksigner 报 ${SIGNER_MD5}）"
if [ -n "$SIGNER_MD5" ] && [ "$DER_MD5" != "$SIGNER_MD5" ]; then
  die "证书 DER md5 与 apksigner 不一致（$DER_MD5 vs ${SIGNER_MD5}）—— killer 喂错证书会静默不生效"
fi
# 与 1.0.1 记录同一把证书（SHA-256 4657fa90…）：仅提示，不作硬门
openssl x509 -in "$W_WORK/cert.pem" -noout -fingerprint -sha256 | sed 's/^/  /'

step "2/7 classes2.dex ← 菜单 dex（星光框架类）"
rm -rf "$WORK/menu" "$WORK/classes2.dex"
mkdir -p "$WORK/menu"
unzip -o -q "$W_MENU" classes.dex -d "$WORK/menu"
mv -f "$WORK/menu/classes.dex" "$WORK/classes2.dex"
"$PY" - "$W_WORK/classes2.dex" <<'PY'
import sys
d = open(sys.argv[1], 'rb').read()
for bad in (b"Lcom/starry/magic/sign/SignSpoof;", b"SignSpoofAppComponentFactory"):
    assert bad not in d, '菜单 dex 仍含已删类 %r' % bad
for need in (b"com/starry/magic/StarryInitProvider", b"com/starry/magic/menu/MenuService",
             b"com/starry/magic/sign/StarryAppComponentFactory"):
    assert need in d, '菜单 dex 缺 %r' % need
print('  classes2.dex =', len(d), 'B（无 SignSpoof；含 Provider/MenuService/AppComponentFactory）')
PY

step "3/7 classes3.dex ← MT killer（包名/证书按本底包改写）"
rm -rf "$WORK/killer_smali" "$WORK/classes3.dex"
cp -r "$KILLER_SMALI" "$WORK/killer_smali"
rm -f "$WORK/killer_smali/bin/mt/signature/SgKillerApp.smali"   # 本作不换 Application，挂载走 appComponentFactory
"$PY" - "$W_WORK/killer_smali" "$W_WORK/official.der" "$PKG" <<'PY'
import base64, io, os, re, sys
root, der_path, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
b64 = base64.b64encode(open(der_path, 'rb').read()).decode()
wrapped = '\\n'.join(b64[i:i + 76] for i in range(0, len(b64), 76))
p = os.path.join(root, 'bin/mt/signature/KillerApplication193.smali')
t = io.open(p, encoding='utf-8').read()
need = '.super Landroid/app/Application;'
assert need in t, 'killer 父类不是 android.app.Application（先规范化 killer_smali）'
# 包名：<clinit> 里第一条 const-string v1, "..."
t, n = re.subn(r'const-string v1, "[^"]*"', 'const-string v1, "%s"' % pkg, t, count=1)
assert n == 1, 'killer 包名锚点未命中'
# 证书：<clinit> 里以 MI 开头的证书 base64（★在包里名 const-string **之前**出现，必须从头找）
j = t.find('const-string v0, "MI')
assert j >= 0, 'killer 证书锚点未命中'
k = t.find('"', j + len('const-string v0, '))
k2 = t.find('"', k + 1)
t = t[:j] + 'const-string v0, "%s"' % wrapped + t[k2 + 1:]
io.open(p, 'w', encoding='utf-8', newline='\n').write(t)
print('  killer: 包名=%s，证书 base64 %d 字符' % (pkg, len(b64)))
PY
"$JAVA_BIN" -jar "$(W "$SMALI_JAR")" assemble -o "$W_WORK/classes3.dex" "$W_WORK/killer_smali"
rm -rf "$WORK/c3_verify"
"$JAVA_BIN" -jar "$(W "$BAKSMALI_JAR")" d "$W_WORK/classes3.dex" -o "$W_WORK/c3_verify" >/dev/null
grep -q "const-string v1, \"${PKG}\"" "$WORK/c3_verify/bin/mt/signature/KillerApplication193.smali" || die "回读：killer 包名未改"
grep -q '^\.super Landroid/app/Application;' "$WORK/c3_verify/bin/mt/signature/KillerApplication193.smali" || die "回读：killer 父类异常"
echo "  classes3.dex = $(wc -c < "$WORK/classes3.dex") B"

step "4/7 二进制 Manifest（AXML 直改：Provider 首位 + appComponentFactory + MenuService）"
# 为什么不用 apktool：本作 resources.arsc 被资源混淆，apktool 重建 res 会因
# 「values-anydpi-v26/mipmaps.xml: invalid value for type 'mipmap'」失败（2026-10-29 实测）
# ⇒ 用 04_scripts/伊莫/patch_yimo_binary_manifest.py（纯 stdlib AXML 直改，改完自检不落盘）
if [ -f "$MF_CACHE" ] && [ "${STARRY_REUSE_MF:-0}" = "1" ]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  用缓存 ${W_MFCACHE}（STARRY_REUSE_MF=1）"
else
  unzip -p "$W_ORIG" AndroidManifest.xml > "$WORK/AndroidManifest.orig.xml" \
    || die "从底包取 AndroidManifest.xml 失败"
  "$PY" "$(W "$NIXIANG/04_scripts/伊莫/patch_yimo_binary_manifest.py")" \
    "$W_WORK/AndroidManifest.orig.xml" "$W_WORK/AndroidManifest.xml" \
    || die "manifest AXML 补丁失败（见上面 [selfcheck]）"
  cp -f "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  缓存 → $W_MFCACHE"
fi
echo "  binary manifest = $(wc -c < "$WORK/AndroidManifest.xml") B"

step "5/7 zip 外科合并 → $W_UNSIGNED"
"$PY" - "$W_ORIG" "$W_WORK" "$W_UNSIGNED" "$W_SO" "$W_CXX" "$W_MENU" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path
src, work, dst, so, cxx, menu = (Path(p) for p in sys.argv[1:7])
mf = (work / 'AndroidManifest.xml').read_bytes()
c2 = (work / 'classes2.dex').read_bytes()
c3 = (work / 'classes3.dex').read_bytes()

extras = {
    'classes2.dex': c2,
    'classes3.dex': c3,
    'lib/arm64-v8a/libstarry_magic.so': so.read_bytes(),
    'lib/arm64-v8a/libc++_shared.so': cxx.read_bytes(),
}
# assets/certs（历史兼容；证书已烘焙进 so）
with zipfile.ZipFile(menu) as zm:
    for n in zm.namelist():
        if n.startswith('assets/certs/'):
            extras[n] = zm.read(n)

def is_v1sig(name):
    if not name.startswith('META-INF/'):
        return False
    up = name.upper()
    return up.endswith(('.SF', '.RSA', '.DSA', '.EC')) or name == 'META-INF/MANIFEST.MF'

zin = zipfile.ZipFile(src)
names = set(zin.namelist())
for n in ('AndroidManifest.xml', 'classes.dex', 'resources.arsc'):
    assert n in names, '底包缺 %s' % n
for n in extras:
    assert n not in names, '底包已含 %s（dex 落位冲突）' % n

zout = zipfile.ZipFile(dst, 'w')
for item in zin.infolist():
    n = item.filename
    if is_v1sig(n):
        continue
    data = mf if n == 'AndroidManifest.xml' else zin.read(item)
    zi = zipfile.ZipInfo(n, item.date_time)
    zi.compress_type = zipfile.ZIP_STORED if n == 'resources.arsc' else item.compress_type
    zi.external_attr = item.external_attr
    zi.create_system = 3
    zout.writestr(zi, data)
for n, data in extras.items():
    zi = zipfile.ZipInfo(n, (2026, 10, 29, 0, 0, 0))
    zi.compress_type = zipfile.ZIP_STORED if n.endswith('.so') else zipfile.ZIP_DEFLATED
    zi.create_system = 3
    zout.writestr(zi, data)
zout.close()

# 逐条目审计：官方条目必须逐字节透传（resources.arsc 尤其）
zi1 = zipfile.ZipFile(src); zi2 = zipfile.ZipFile(dst)
assert hashlib.sha256(zi1.read('resources.arsc')).digest() == hashlib.sha256(zi2.read('resources.arsc')).digest(), 'arsc 被改'
n_same = n_chk = 0
for item in zi1.infolist():
    n = item.filename
    if is_v1sig(n) or n == 'AndroidManifest.xml' or n in extras:
        continue
    n_chk += 1
    if hashlib.sha256(zi1.read(n)).digest() == hashlib.sha256(zi2.read(n)).digest():
        n_same += 1
print('  透传 %d/%d 条目；注入 %r' % (n_same, n_chk, sorted(extras)))
assert n_same == n_chk, '官方条目 hash 不一致（外科透传被破坏）'
assert 'resources.arsc' in zi2.namelist()
PY
echo "  unsigned = $(wc -c < "$UNSIGNED") B"

step "6/7 zipalign + apksigner"
rm -f "$WORK/aligned.apk"
"$BT/zipalign.exe" -f -p 4 "$W_UNSIGNED" "$W_WORK/aligned.apk" 2>/dev/null \
  || "$BT/zipalign" -f -p 4 "$W_UNSIGNED" "$W_WORK/aligned.apk"
rm -f "$SIGNED"
"$BT/apksigner.bat" sign --ks "$W_KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$W_SIGNED" "$W_WORK/aligned.apk" 2>/dev/null \
  || "$BT/apksigner" sign --ks "$W_KS" --ks-key-alias "$KS_ALIAS" \
       --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
       --out "$W_SIGNED" "$W_WORK/aligned.apk"
"$BT/apksigner.bat" verify --print-certs "$W_SIGNED" 2>/dev/null | head -6 || true
echo "  signed = $(wc -c < "$SIGNED") B"

step "7/7 成品自检"
"$PY" - "$W_SIGNED" "$W_SO" "$PKG" <<'PY'
import hashlib, sys, zipfile
apk, so, pkg = sys.argv[1:4]
z = zipfile.ZipFile(apk)
names = z.namelist()
man = z.read('AndroidManifest.xml')
so_in = z.read('lib/arm64-v8a/libstarry_magic.so')
so_disk = open(so, 'rb').read()
assert hashlib.md5(so_in).digest() == hashlib.md5(so_disk).digest(), '包内 so 与本次构建产物 md5 不同（旧 so 混包）'
dexes = sorted(n for n in names if n.startswith('classes') and n.endswith('.dex'))
print('  dex 布局 =', dexes)
for need in ('classes2.dex', 'classes3.dex'):
    assert need in names, '缺 %s' % need
assert b'libc++_shared.so' in b'\n'.join(x.encode() for x in names) or 'lib/arm64-v8a/libc++_shared.so' in names
c2 = z.read('classes2.dex'); c3 = z.read('classes3.dex')
assert b'Lcom/starry/magic/sign/SignSpoof;' not in c2, 'classes2 仍含已删 SignSpoof'
assert b'bin/mt/signature/KillerApplication193' in c3, 'classes3 不是 killer'
def has(s):
    # ★AXML 字符串池是 UTF-16LE：直接搜 utf-8 字节会假失败（2026-10-29 踩过）
    return s.encode('utf-8') in man or s.encode('utf-16-le') in man
assert not has('SignSpoofAppComponentFactory'), 'manifest 仍挂已删的 SignSpoofAppComponentFactory'
assert has('com.netease.nis.wrapper.MyApplication'), 'manifest 丢了易盾 Application（被误改）'
assert has('com.starry.magic.sign.StarryAppComponentFactory'), 'manifest 未挂 StarryAppComponentFactory'
assert has('com.starry.magic.StarryInitProvider'), 'manifest 缺 StarryInitProvider'
assert has('com.starry.magic.menu.MenuService'), 'manifest 缺 MenuService'
print('  manifest/dex/so 断言全过')
PY
shasum -a 256 "$SIGNED" 2>/dev/null || sha256sum "$SIGNED"

if [ "${STARRY_INSTALL:-0}" = "1" ]; then
  step "装机（${SERIAL}）"
  # ★装机姿势（2026-10-29 伊莫 1.1.0 实测两条坑）：
  #   ① 必须用 `adb install`（streamed session）；`pm install <设备内路径>` 在本机
  #      会报 INSTALL_FAILED_MEDIA_UNAVAILABLE: Failed to restorecon（包是从
  #      /data/local/tmp 复制的、context 不对），adbd 流式安装则一次成功。
  #   ② ★换签名场景**禁止 pm uninstall -k**：会残留带旧签名的 PackageSetting，
  #      之后 install 永远 INSTALL_FAILED_UPDATE_INCOMPATIBLE，且 pm uninstall
  #      只回 "not installed for 0"（僵尸记录）。标准解（已实测）：
  #      用**官方原包** adb install -r -d 覆盖僵尸记录 → pm uninstall（不带 -k）
  #      → adb install 星光包。数据目录如需保留，先 root mv 出去再搬回（见文档）。
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  if adb -s "$SERIAL" install -r -d --user 0 "$W_SIGNED" 2>&1 | grep -q Success; then
    echo "[install] 同签覆盖 OK（adb install）"
  else
    if [ "${STARRY_ALLOW_UNINSTALL:-0}" = "1" ]; then
      echo "[install] 异签 → 官方原包装回记录 → pm uninstall（不带 -k）→ adb install"
      adb -s "$SERIAL" install -r -d --user 0 "$W_ORIG" 2>&1 | tail -1
      adb -s "$SERIAL" shell "su -c 'pm uninstall --user 0 $PKG'" || true
      adb -s "$SERIAL" install -r -d --user 0 "$W_SIGNED" || die "重装失败"
    else
      die "签名与设备原版不一致，覆盖安装失败。确认后加 STARRY_ALLOW_UNINSTALL=1（并先按文档搬数据）"
    fi
  fi
  echo "[install] 取日志："
  echo "  adb -s $SERIAL logcat -d | grep -E '\\[Yimo\\]|Mod_Menu|MenuService|StarryAppFactory|StarryDiag'"
fi
echo "[done] $W_SIGNED"
