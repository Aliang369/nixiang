#!/usr/bin/env bash
# 炁术行者（com.h5wrapper.app）——「星光版」外科组装出包（框架版）
#
# 产物：08_output/炁术行者/炁术行者_1.0_星光版.apk
#   免 root 直装；含星光原生菜单 + 卡密登录/公告/授权门控 + 页面内 JS 补丁
#
# 本作特殊性（Cocos web-mobile H5 跑在宿主自己的 WebView 里，APK 内零游戏业务逻辑）：
#   · 游戏 classes.dex 只做**一处**改动：MainActivity$2 加 shouldInterceptRequest，
#     把 /assets/main/index.js 与 /src/chunks/bundle.js 换成包内 assets/qsw/*
#     （服务端 JS 是明文可改的，两条扣血口径的静态补丁必须落在这里）
#   · 框架侧新增 classes2.dex（全部 com/starry/**），再挂 Manifest 三条声明
#   · resources.arsc 与 META-INF 之外的其它条目逐字节不变
#   · so 从 libs_qsxz 取（本作专用标签），**不从 starry APK 取**（防旧 so 顶包）
#
# 输入（全部可再生）：底包 02_apks/炁术行者9.15.apk.1 + 服务端 http://43.248.140.201:8888
# 用法：
#   bash 04_scripts/炁术行者/qsw_build_star.sh              # 构建
#   bash 04_scripts/炁术行者/qsw_build_star.sh --install    # 构建并装机（覆盖安装）
#   环境变量：QSW_FETCH=1 重拉服务端 JS；QSW_SKIP_GRADLE=1 复用上次框架产物
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="炁术行者"
TAG="星光版"
PKG="com.h5wrapper.app"
SERVER="http://43.248.140.201:8888"
BASE_APK_MD5="f528339aadb2ba9fcdd6de477a936e06"
# 底包路径解析：用户手动管理的 02_apks/ 里，同一底包可能以 炁术行者9.15.apk
# 或 炁术行者9.15.apk.1 命名（浏览器下载残留）。指纹仍由下面 BASE_APK_MD5 硬校验，
# 文件名不参与判定；也可用 QSW_BASE_APK 显式指定（仍需通过指纹检查）。
BASE_APK="${QSW_BASE_APK:-$NIXIANG/02_apks/炁术行者9.15.apk}"
[[ -f "$BASE_APK" ]] || BASE_APK="$NIXIANG/02_apks/炁术行者9.15.apk.1"
SRC_DIR="$NIXIANG/04_scripts/$GAME"
INJECT_SMALI="$SRC_DIR/inject/MainActivity\$2.smali"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
SO_PREBUILT="$STARRY_SRC/libs_qsxz/arm64-v8a/libstarry_magic.so"
WORK="${QSW_WORK:-/tmp/qsw_build}"
OUT_DIR="$NIXIANG/08_output/$GAME"
OUT="$OUT_DIR/${GAME}_1.0_${TAG}.apk"
KS="$NIXIANG/01_tools/qsw_release.jks"
KS_ALIAS="qsw"
KS_PASS="${QSW_KS_PASS:-qsw123456}"
# 设备上已装包（上一版）的签名证书 SHA-256：换签名会导致覆盖安装失败
CERT_SHA256_WANT="42:6F:A3:FB:6B:14:09:E0:B2:E8:4E:9C:DB:EF:A8:63:A0:87:34:6D:87:88:46:E6:32:80:DB:3C:15:79:2B:88"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"

DO_INSTALL=0
[[ "${1:-}" == "--install" ]] && DO_INSTALL=1

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

say() { printf '\n== %s ==\n' "$1"; }
die() { echo "ERROR: $*" >&2; exit 1; }

PY_BIN="python3.11"; command -v "$PY_BIN" >/dev/null 2>&1 || PY_BIN="python3"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || die "找不到 build-tools/zipalign"

say "0. 输入检查"
[[ -f "$BASE_APK" ]] || die "缺底包 ${BASE_APK}（底包由用户管理，脚本不自动下载）"
[[ "$(md5 -q "$BASE_APK")" == "$BASE_APK_MD5" ]] || die "底包指纹不符（期望 ${BASE_APK_MD5}）"
[[ -f "$INJECT_SMALI" ]] || die "缺注入 smali $INJECT_SMALI"
[[ -f "$KS" ]] || die "缺签名密钥 $KS"
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || die "缺 smali/baksmali（$NIXIANG/01_tools/smali/）"
[[ -f "$SO_PREBUILT" ]] || die "缺 ${SO_PREBUILT}（先跑 04_scripts/炁术行者/qsxz_build_so.sh）"
mkdir -p "$WORK" "$OUT_DIR"

LOCK_DIR="/tmp/qsw_star.lockdir"
mkdir "$LOCK_DIR" 2>/dev/null || die "另一个出包进程在跑（${LOCK_DIR}）"
echo $$ > "$LOCK_DIR/pid"
trap 'rm -rf "$LOCK_DIR"' EXIT

say "1. 取服务端 JS（明文 HTTP，可再生）"
if [[ ! -f "$WORK/main_index.js" || "${QSW_FETCH:-0}" == "1" ]]; then
  curl -fsS "$SERVER/assets/main/index.js" -o "$WORK/main_index.js" || die "拉 main/index.js 失败"
fi
if [[ ! -f "$WORK/bundle.js" || "${QSW_FETCH:-0}" == "1" ]]; then
  curl -fsS "$SERVER/src/chunks/bundle.js" -o "$WORK/bundle.js" || die "拉 bundle.js 失败"
fi
echo "  main_index.js $(wc -c <"$WORK/main_index.js" | tr -d ' ') B / bundle.js $(wc -c <"$WORK/bundle.js" | tr -d ' ') B"

say "2. 生成包内游戏 JS + 静态自检"
mkdir -p "$WORK/assets/qsw" "$WORK/assets/starry"
"$PY_BIN" "$SRC_DIR/qsw_patch_js_star.py" --src "$WORK/main_index.js" --dst "$WORK/assets/qsw/main.js"
cp "$WORK/bundle.js" "$WORK/assets/qsw/bundle.js"
cp "$STARRY_SRC/app/src/main/assets/starry/qsxz_patch.js" "$WORK/assets/starry/qsxz_patch.js"
"$PY_BIN" - "$WORK/assets/qsw/main.js" "$WORK/assets/starry/qsxz_patch.js" <<'PY'
import sys
mj = open(sys.argv[1], encoding='utf-8').read()
pj = open(sys.argv[2], encoding='utf-8').read()
for m in ('QSW-MOD v10s', '__QSW_APPLY', 'getBaseReduceHp=function'):
    assert m in mj, 'main.js 缺 ' + m
for b in ('192.168.10.25', '8899', '/cfg'):
    assert b not in mj, 'main.js 仍残留外部服务痕迹: ' + b
assert mj.count('window.__QSW_DMG(') == 3, 'QSW_DMG 调用点不是 3 处'
# 补丁资产：与城主别慌张同一铁律——不得出现百分号与井号（evaluateJavascript 前身 loadUrl 会截断）
for ch, nm in (('%', '百分号'), ('#', '井号')):
    assert ch not in pj, 'qsxz_patch.js 含' + nm + '（历史坑：URL 解析截断）'
for m in ('getState', '__QSW_APPLY', 'state.live'):
    assert m in pj, 'qsxz_patch.js 缺 ' + m
print('  ok: main.js 无外部服务残留；qsxz_patch.js 无百分号/井号')
PY
if command -v node >/dev/null 2>&1; then
  node --check "$WORK/assets/qsw/main.js" && node --check "$WORK/assets/starry/qsxz_patch.js" \
    && echo "  ok: node --check 两份 JS 语法通过"
fi

say "3. 框架 dex（classes2.dex）"
if [[ "${QSW_SKIP_GRADLE:-0}" != "1" || ! -f "$WORK/classes2.dex" ]]; then
  (cd "$STARRY_SRC" && ./gradlew assembleRelease --console=plain > "$WORK/gradle.log" 2>&1) \
    || { tail -30 "$WORK/gradle.log"; die "gradle assembleRelease 失败"; }
  STARRY_APK="$STARRY_SRC/app/build/outputs/apk/release/app-release.apk"
  [[ -f "$STARRY_APK" ]] || die "缺 $STARRY_APK"
  rm -rf "$WORK/starry_dec" "$WORK/starry_smali"
  apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
  mkdir -p "$WORK/starry_smali"
  for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
    [[ -d "$d/com/starry" ]] || continue
    mkdir -p "$WORK/starry_smali/com"
    cp -R "$d/com/starry" "$WORK/starry_smali/com/"
  done
  for f in StarryApp StarryInitProvider; do
    [[ -f "$WORK/starry_smali/com/starry/magic/$f.smali" ]] || die "缺 $f.smali（框架没编进 starry apk）"
  done
  [[ -f "$WORK/starry_smali/com/starry/magic/h5box/StarryH5.smali" ]] || die "缺 h5box/StarryH5.smali"
  [[ -f "$WORK/starry_smali/com/starry/magic/h5box/StarryH5Provider.smali" ]] || die "缺 h5box/StarryH5Provider.smali"
  "$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/starry_smali"
fi
[[ -f "$WORK/classes2.dex" ]] || die "缺 classes2.dex"
echo "  classes2.dex $(wc -c <"$WORK/classes2.dex" | tr -d ' ') B"

say "4. 游戏 classes.dex 重建（只换 MainActivity\$2 受击拦截器）"
rm -rf "$WORK/orig_smali"
"$BAKSMALI_BIN" d "$BASE_APK" -o "$WORK/orig_smali" >/dev/null 2>&1 || \
  "$BAKSMALI_BIN" d "$WORK/classes.dex.orig" -o "$WORK/orig_smali" >/dev/null
cp "$INJECT_SMALI" "$WORK/orig_smali/com/h5wrapper/app/MainActivity\$2.smali"
"$SMALI_BIN" assemble -o "$WORK/classes.dex" "$WORK/orig_smali"
[[ -f "$WORK/classes.dex" ]] || die "游戏 dex 重建失败"
echo "  classes.dex $(wc -c <"$WORK/classes.dex" | tr -d ' ') B"

say "5. lib + binary Manifest"
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so" || die "缺 libc++_shared.so（${NDK_CXX}）"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"; cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi
MF_CACHE="$WORK/AndroidManifest.bin"
if [[ ! -f "$MF_CACHE" ]]; then
  echo "  首次生成 binary Manifest（apktool d -s 文本改 + b 抽回）"
  apktool d -f -s -o "$WORK/orig_dec" "$BASE_APK" >/dev/null
  "$PY_BIN" - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1]); t = p.read_text(encoding='utf-8')
PKG = 'com.h5wrapper.app'
perms = []
for perm in ('android.permission.SYSTEM_ALERT_WINDOW',
             'android.permission.FOREGROUND_SERVICE',
             'android.permission.FOREGROUND_SERVICE_SPECIAL_USE'):
    if perm not in t:
        perms.append('    <uses-permission android:name="%s"/>' % perm)
if perms:
    i = t.find('<uses-permission')
    if i < 0:
        i = t.find('<application')
    t = t[:i] + '\n'.join(perms) + '\n' + t[i:]
    print('  +permissions: %d' % len(perms))
if 'com.starry.magic.menu.MenuService' not in t:
    svc = ('\n        <service\n'
           '            android:name="com.starry.magic.menu.MenuService"\n'
           '            android:enabled="true"\n'
           '            android:exported="false"\n'
           '            android:foregroundServiceType="specialUse"\n'
           '            android:stopWithTask="false">\n'
           '            <property\n'
           '                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"\n'
           '                android:value="MOD menu overlay" />\n'
           '        </service>\n')
    t = t.replace('</application>', svc + '\n    </application>', 1)
    print('  +MenuService')
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = ('\n        <provider\n'
            '            android:name="com.starry.magic.StarryInitProvider"\n'
            '            android:authorities="%s.starryinit"\n'
            '            android:exported="false"\n'
            '            android:initOrder="100" />\n' % PKG)
    t = t.replace('</application>', prov + '\n    </application>', 1)
    print('  +StarryInitProvider')
if 'com.starry.magic.h5box.StarryH5Provider' not in t:
    prov = ('\n        <provider\n'
            '            android:name="com.starry.magic.h5box.StarryH5Provider"\n'
            '            android:authorities="%s.starryh5"\n'
            '            android:exported="false"\n'
            '            android:initOrder="101" />\n' % PKG)
    t = t.replace('</application>', prov + '\n    </application>', 1)
    print('  +StarryH5Provider')
p.write_text(t, encoding='utf-8')
print('  text Manifest patched（保留原 Application）')
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$MF_CACHE"
fi
echo "  AndroidManifest.xml $(wc -c <"$MF_CACHE" | tr -d ' ') B"

say "6. zip 外科合并（除 classes.dex 与 Manifest 外零改动；arsc 与 so 不压缩）"
"$PY_BIN" - "$BASE_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib, shutil
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
new_dex = (work / "classes.dex").read_bytes()
new_c2 = (work / "classes2.dex").read_bytes()
new_mf = (work / "AndroidManifest.bin").read_bytes()

extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p
for sub in ("qsw", "starry", "certs"):
    base = work / "assets" / sub
    if base.is_dir():
        for p in base.rglob("*"):
            if p.is_file():
                extras["assets/%s/%s" % (sub, str(p.relative_to(base)).replace("\\", "/"))] = p

# 只换这两件；其余条目逐字节搬运
replace = {"AndroidManifest.xml": new_mf, "classes.dex": new_dex}
untouched = ["resources.arsc"]

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue                       # 旧签名必须剔除
    if name in extras or name in ("classes2.dex",):
        continue                       # 由新增块写入
    info = zipfile.ZipInfo(name, item.date_time)
    info.external_attr = item.external_attr
    info.create_system = 3
    # resources.arsc 与 .so 必须不压缩（targetSdk>=30 + extractNativeLibs 兼容）
    if name == "resources.arsc" or name.endswith(".so"):
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    if name in replace:
        zout.writestr(info, replace[name])
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    n += 1

for nm, data, store in (("classes2.dex", new_c2, False),):
    info = zipfile.ZipInfo(nm)
    info.compress_type = zipfile.ZIP_STORED if store else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data); n += 1

for name in sorted(extras):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, extras[name].read_bytes()); n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r"); zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == \
           hashlib.sha256(zout2.read(name)).hexdigest(), "changed: " + name
names = zout2.namelist()
for need in ("classes.dex", "classes2.dex", "lib/arm64-v8a/libstarry_magic.so",
             "lib/arm64-v8a/libc++_shared.so", "assets/qsw/main.js",
             "assets/qsw/bundle.js", "assets/starry/qsxz_patch.js", "AndroidManifest.xml"):
    assert need in names, "missing: " + need
assert hashlib.sha256(zin2.read("AndroidManifest.xml")).hexdigest() != \
       hashlib.sha256(zout2.read("AndroidManifest.xml")).hexdigest(), "manifest not patched"
print("  merged ok; 条目≈%d；resources.arsc 字节不变" % n)
zin2.close(); zout2.close()
PY

say "7. zipalign + sign + 闭环校验"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$WORK/aligned.apk"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$OUT" "$WORK/aligned.apk"
"$BT/apksigner" verify --print-certs "$OUT" >/dev/null
GOT_SHA=$("$BT/apksigner" verify --print-certs "$OUT" | grep -i 'SHA-256' | head -1 | awk '{print $NF}' | tr -d ':' | tr 'A-Z' 'a-z')
WANT_SHA=$(echo "$CERT_SHA256_WANT" | tr -d ':' | tr 'A-Z' 'a-z')
echo "  signer SHA-256: $GOT_SHA"
[[ "$GOT_SHA" == "$WANT_SHA" ]] || die "签名证书与设备已装包不一致（${GOT_SHA}）⇒ 无法覆盖安装"
unzip -p "$OUT" lib/arm64-v8a/libstarry_magic.so > "$WORK/pkg.so"
[[ "$(md5 -q "$SO_PREBUILT")" == "$(md5 -q "$WORK/pkg.so")" ]] || die "包内 so != 构建产物（旧 so 顶包）"
echo "  包内 so md5 = $(md5 -q "$WORK/pkg.so")"
LC_ALL=C grep -ac 'SeekBar_' "$WORK/pkg.so" >/dev/null
echo "  package : $("$BT/aapt2" dump packagename "$OUT" 2>/dev/null || echo "$PKG")"
echo "  体积    : $(ls -lh "$OUT" | awk '{print $5}')"
echo "  md5     : $(md5 -q "$OUT")"

if [[ "$DO_INSTALL" == "1" ]]; then
  say "8. 装机（覆盖安装，保留数据）"
  adb install -r "$OUT"
fi

say "DONE"
echo "$OUT"
