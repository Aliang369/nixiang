#!/usr/bin/env bash
# 我的勇者 7.6.5 (com.rsg.wdyz, vc 214, TapTap 渠道) — 外科 zip 组装
# - 注入方式：ContentProvider（StarryInitProvider），【不替换 Application】
#   （官方 Application=com.netease.nis.wrapper.MyApplication 易盾壳，替换会触发校验）
# - classes2.dex：com/starry smali（StarryApp/StarryInitProvider/MenuService…）
# - classes3.dex：A 轨签名 Killer（bin.mt.signature.KillerApplication193，13 类），由
#   04_scripts/我的勇者/build_killer_dex_wdyz.sh 从 smali 烘焙（官方 CN=rn 证书 DER，865 B）。
#   StarryInitProvider → SignatureKillerBootstrap 触发其 <clinit> ⇒ TapSDK 走 PackageManager
#   取到的签名被回灌为官方值（真机星光谱 161fca32… → 官方 60908df9…）。
# - lib/arm64-v8a：libstarry_magic.so（WDYZ：无敌 HeroSettle/基类己方归零 + 五倍攻击 敌 core ×5）+ libc++_shared.so
# - 官方 classes.dex / resources.arsc 原样保留（易盾壳主 dex 不动）
#
# 底包：02_apks/我的勇者/我的勇者_7.6.5_base.apk（2026-11-24 取得，vc 214 / versionName 7.6.5，
#   1,525,433,900 B，sha256 e6601dc2…）
#   包内 libil2cpp.so sha256 259622aa…（142,698,280 B；base 配对：同包 global-metadata.dat sha256 5ba8f5f6…，不换 so）
#   挂点：WdyzMod.cpp §② 7.6.5 表（Hero 0x48805A4 / Base 0x486078C / Monster 0x488AD44 /
#     settleMp 0x48806E0 / 无CD 入口 0x48F90F8，补丁点 = 入口+0x60；证据见 10_docs/我的勇者/路线锁定_7.6.5.md）
#
# 产物：08_output/我的勇者/我的勇者_7.6.5_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_wdyz_7.6.5.sh
#   STARRY_INSTALL=1 bash scripts/surgical_wdyz_7.6.5.sh       （装机）
#   STARRY_ALLOW_UNINSTALL=1 ...                               （签名不一致需卸载重装时显式给）
#   WDYZ_KILLER_DEX=/path/classes3.dex                         （A 轨 killer dex；默认 build 产物）
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug"; exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
VER="7.6.5"
ORIG_APK="$NIXIANG/02_apks/我的勇者/我的勇者_7.6.5_base.apk"
# NEW_SO：7.6.5 **不需要**（无 357 那类可嵌热更 so；mod 偏移已按 base so 回填）。
#   默认 STARRY_SKIP_SO_SWAP=1 保留底包 so（base 配对）；仅显式置 0 时才要求本变量指向 so。
NEW_SO="${STARRY_NEW_SO:-}"
# STARRY_OUT_DIR：可把产物写到别处（默认 08_output/我的勇者）。
#   用于「验证包先落临时目录、确认能用再进 08_output」，避免覆盖已交付/已归档的正式包。
OUT="${STARRY_OUT_DIR:-$NIXIANG/08_output/我的勇者}"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
# A 轨：签名 Killer dex（装成 classes3.dex）。默认用 build_killer_dex_wdyz.sh 的产物；
#   缺失即报错退出——**不允许**静默回退成「无 killer」包（那样 TapSDK 仍读到星光签名）。
KILLER_DEX="${WDYZ_KILLER_DEX:-$NIXIANG/04_scripts/我的勇者/assets/wdyz_signature_killer.dex}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.rsg.wdyz"
OFFICIAL_APP="Lcom/netease/nis/wrapper/MyApplication;"
# STARRY_APK_TAG：产物名加标记（如 fix1）⇒ 出包不覆盖既有正式包（原包保留）。
SIGNED="$OUT/我的勇者_${VER}${STARRY_APK_TAG:+_${STARRY_APK_TAG}}_surgical-${BUILD_TYPE}Signed.apk"
ALIAS_APK="$OUT/我的勇者_${VER}${STARRY_APK_TAG:+_${STARRY_APK_TAG}}_starry.apk"
MF_CACHE="$NIXIANG/07_patched/我的勇者/surgical_${VER}/AndroidManifest.xml"

# 7.6.5 首次构建不沿用 7.6.4 的注入缓存，避免官方 Manifest 结构变化时错配。
# MF_RECIPE：Manifest 注入配方版本。配方变了（如 A 轨「StarryInitProvider 置首」）就换名字/
#   换标记，强制重新生成——否则旧缓存会把旧 provider 顺序带进新包（缓存只在文本步骤生效）。
MF_RECIPE="killer-first-provider-v1"
MF_CACHE="/tmp/wdyz765_no_manifest_cache_killer"
MF_RECIPE_FILE="$MF_CACHE.recipe"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

# STARRY_SKIP_SO_SWAP=1：**不替换** lib/arm64-v8a/libil2cpp.so（用底包自带 so）。
#   为什么需要（2026-10-18 首装闪退实测，见 10_docs/我的勇者/崩溃复现_首装闪退_20261018.md）：
#   把 so 换成热更 build 357、但 global-metadata.dat 仍是底包那份；游戏自带 UnityPatch 在
#   「没热更过」的干净机器上找不到补丁副本（NewPatchSoDir==false → LoadLibrary.false）就回退到
#   包内 so ⇒ so/metadata 混搭 ⇒ il2cpp 互递归爆栈闪退。只有已热更 357 的机器（走 files/update/current）才正常。
#   跳过换 so 时 mod 走 WdyzMod.cpp 的「RVA 序言不符 → 按名兜底」路径。
# 7.6.5 默认 =1：base 配对（包内 so sha256 259622aa… 与 mod 的 base Off 同源），不换 so。
SKIP_SO_SWAP="${STARRY_SKIP_SO_SWAP:-1}"
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
if [[ "$SKIP_SO_SWAP" == "1" ]]; then
  echo "[check] STARRY_SKIP_SO_SWAP=1 ⇒ 保留底包 libil2cpp.so（RVA 序言校验，不符则按名兜底）"
else
  [[ -n "$NEW_SO" && -f "$NEW_SO" ]] || { echo "ERROR: STARRY_SKIP_SO_SWAP=0 但 NEW_SO 未配置/不存在：'$NEW_SO'（7.6.5 默认不换 so）"; exit 1; }
fi
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
[[ -n "$KILLER_DEX" && -f "$KILLER_DEX" ]] || {
  echo "ERROR: 缺少签名 Killer dex：'$KILLER_DEX'"
  echo "  生成：bash 04_scripts/我的勇者/build_killer_dex_wdyz.sh"
  echo "  或显式指定：WDYZ_KILLER_DEX=/path/to/killer.dex bash $0"
  exit 1
}
[[ "$(head -c 3 "$KILLER_DEX")" == "dex" ]] || { echo "ERROR: $KILLER_DEX 不是 dex（magic 不符）"; exit 1; }
grep -a -q 'KillerApplication193' "$KILLER_DEX" || { echo "ERROR: killer dex 内缺 KillerApplication193"; exit 1; }
grep -a -q 'com.rsg.wdyz' "$KILLER_DEX" || { echo "ERROR: killer dex 内缺包名 com.rsg.wdyz"; exit 1; }
if grep -a -q 'Lcom/starry/magic/sign/SignSpoof;' "$KILLER_DEX"; then
  echo "ERROR: killer dex 内含 SignSpoof（已禁用，禁止复活）"; exit 1
fi
echo "[check] killer dex = $KILLER_DEX ($(stat -f '%z' "$KILLER_DEX") B, md5 $(md5 -q "$KILLER_DEX"))"
MK="$STARRY_SRC/app/src/main/jni/Android_wdyz.mk"
[[ -f "$MK" ]] || { echo "ERROR: missing $MK"; exit 1; }
grep -E '^\s*LOCAL_SRC_FILES\s*:=\s*MainModern\.cpp\s+Games/WdyzMod\.cpp\s+Games/WdyzEntry\.cpp' "$MK" \
  || { echo "ERROR: Android_wdyz.mk LOCAL_SRC_FILES must be MainModern.cpp Games/WdyzMod.cpp Games/WdyzEntry.cpp"; exit 1; }
[[ -f "$STARRY_SRC/app/src/main/jni/Games/WdyzEntry.cpp" ]] \
  || { echo "ERROR: missing Games/WdyzEntry.cpp"; exit 1; }
echo "[check] modern target=wdyz (${VER})"
echo "[check] orig apk = $ORIG_APK"

LOCK_DIR="/tmp/wdyz765_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wdyz765_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_wdyz.mk NDK_APPLICATION_MK=./Application.mk \
  NDK_OUT="$STARRY_SRC/app/src/obj_wdyz" NDK_LIBS_OUT="$STARRY_SRC/app/src/libs_wdyz" \
  APP_ABI=arm64-v8a -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs_wdyz/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Wdyz\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 WDYZ（可能仍是上一款游戏）。strings 检查失败。"
  exit 1
fi
for stale in '\[Zjmc\]' '\[Wxsj\]' '\[Cwjh\]'; do
  if grep -a -q "$stale" "$SO_BUILT"; then
    echo "ERROR: so 仍含上一款游戏标签 $stale"
    exit 1
  fi
done
# 证书自检（2026-11-21 策略反转）：服务器换发官方证书（auth.xgmfyx.cn，Let's Encrypt）后，
#   CertPinned.h 已清空 ⇒ so 内**不再烘焙**任何 PEM（nativeGetPinnedCert 仅留符号，保 RegisterNatives
#   整表注册）。因此本版自检改为「不得含 PEM」，与 7.6.4 及更早的「必须含」相反。
CERT_N=$(grep -a -c 'BEGIN CERTIFICATE' "$SO_BUILT" || true)
[[ "${CERT_N:-0}" -eq 0 ]] || { echo "ERROR: so 内仍有烘焙证书（BEGIN CERTIFICATE=${CERT_N}），与 2026-11-21 策略不符"; exit 1; }
# 中文菜单串按字节搜（strings 默认 ASCII 不认 UTF-8 中文）
# STARRY_MENU_FEATS=2 ⇒ 2 技能精简包（无敌/五倍攻击）；默认 4 项完整包
if [[ "${STARRY_MENU_FEATS:-4}" == "2" ]]; then
  MENU_CHECK_LIST=('无敌' '五倍攻击')
else
  MENU_CHECK_LIST=('无敌' '五倍攻击' '无限技能' '伤害倍率')
fi
for m in "${MENU_CHECK_LIST[@]}"; do
  N=$(LC_ALL=C grep -a -c "$m" "$SO_BUILT" || true)
  [[ "${N:-0}" -ge 1 ]] || { echo "ERROR: so 内缺菜单串 $m"; exit 1; }
done
echo "  so selfcheck ok (wdyz tag / cert=$CERT_N / 中文菜单串 ok)"
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok (wdyz verified)"

echo "[2/6] gradle ${GRADLE_TASK}"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Wdyz\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 WDYZ"
  exit 1
fi
if [[ "$(md5 -q "$WORK/check.so")" != "$(md5 -q "$SO_BUILT")" ]]; then
  echo "ERROR: gradle APK 内 so 与本次构建产物 md5 不一致（旧 so 混包）"
  exit 1
fi
echo "  gradle APK so verified [Wdyz] + md5 == 构建产物"

echo "[3/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[4/6] 收集 com/starry smali → classes2.dex（StarryApp.super → 官方 MyApplication，Manifest 不走它）"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" "$OFFICIAL_APP" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
app = sys.argv[2]
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", f".super {app}"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        f"invoke-direct {{p0}}, {app}-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        f"invoke-super {{p0, p1}}, {app}->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        f"invoke-super {{p0}}, {app}->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
if app not in t:
    raise SystemExit(f"ERROR: StarryApp.smali missing {app}")
if ".super Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still .super Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.super -> {app} ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"
# classes3.dex = A 轨签名 Killer（底包只有 classes.dex，classes3 空位可用）
cp -f "$KILLER_DEX" "$WORK/classes3.dex"
echo "  classes3.dex (killer) <- $KILLER_DEX"
ls -la "$WORK/classes3.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[5/6] binary AndroidManifest（ContentProvider 注入；不替换 Application）"
if [[ -f "$MF_CACHE" && -f "$MF_RECIPE_FILE" && "$(cat "$MF_RECIPE_FILE" 2>/dev/null || true)" == "$MF_RECIPE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE (recipe $MF_RECIPE)"
else
  echo "  generate binary Manifest once via mini zip"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src)
zout = zipfile.ZipFile(dst, "w")
for name in keep:
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, zin.read(name))
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
# 官方 Application=MyApplication（易盾 wrapper），【不替换】；改由 StarryInitProvider 自动初始化。
# 【A 轨时序】StarryInitProvider 必须排在**所有官方 provider 之前**（Manifest 顺序 = 进程内
#   ContentProvider 安装顺序）⇒ 直接紧跟 <application> 开标签插入，确保
#   KillerApplication193.<clinit> 早于 com.taptap.sdk.kit.internal.TapTapKitInitProvider 的 onCreate，
#   缩小「TapSDK 先取签名、killer 后生效」的窗口。只调顺序，不改 StarryInitProvider 逻辑。
if 'com.starry.magic.StarryInitProvider' not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.rsg.wdyz.starryinit"
            android:exported="false" />'''
    m = t.find("<application")
    if m < 0:
        raise SystemExit("ERROR: text Manifest 内找不到 <application")
    gt = t.find(">", m)
    if gt < 0:
        raise SystemExit("ERROR: <application> 开标签无 '>'")
    t = t[:gt + 1] + prov + t[gt + 1:]
# 断言：首位 provider 必须是 StarryInitProvider，且早于 TapTap 的 provider
import re
names = re.findall(r'<provider[^>]*?android:name="([^"]+)"', t, re.S)
if not names or names[0] != "com.starry.magic.StarryInitProvider":
    raise SystemExit("ERROR: StarryInitProvider 不是首个 provider: " + repr(names[:3]))
TAP = "com.taptap.sdk.kit.internal.TapTapKitInitProvider"
print("  provider[0]=StarryInitProvider, total=%d, TapTapKitInitProvider idx=%s"
      % (len(names), names.index(TAP) if TAP in names else "absent"))
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (StarryInitProvider + MenuService)")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  printf '%s\n' "$MF_RECIPE" > "$MF_RECIPE_FILE"
  echo "  cached → $MF_CACHE (recipe $MF_RECIPE)"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" "$NEW_SO" "$SKIP_SO_SWAP" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
skip_so = (sys.argv[5] == "1")
new_so = None if skip_so else Path(sys.argv[4]).read_bytes()
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name == "classes2.dex":
        continue
    if name in ("classes3.dex", "classes4.dex", "classes5.dex", "classes6.dex", "classes7.dex", "classes8.dex", "classes9.dex"):
        continue  # 官方只有 classes.dex；防御性跳过不存在项
    if name == "AndroidManifest.xml":
        data = mf
    elif name == "lib/arm64-v8a/libil2cpp.so" and new_so is not None:
        data = new_so  # 仅 STARRY_SKIP_SO_SWAP=0 且 NEW_SO 已配置时替换（7.6.5 默认不换）
    else:
        data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)

info = zipfile.ZipInfo("classes2.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c2)

info = zipfile.ZipInfo("classes3.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c3)

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_STORED if name.endswith(".so") else zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
assert hashlib.sha256(zin2.read("resources.arsc")).hexdigest() == hashlib.sha256(zout2.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zin2.read("classes.dex")).hexdigest() == hashlib.sha256(zout2.read("classes.dex")).hexdigest()
assert "classes2.dex" in zout2.namelist()
assert "classes3.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
# ---- A 轨：classes3.dex（killer）内容与指纹校验 ----
c3_out = zout2.read("classes3.dex")
assert hashlib.sha256(c3_out).hexdigest() == hashlib.sha256(c3).hexdigest(), "classes3.dex 与输入 killer dex 不一致"
assert b"KillerApplication193" in c3_out, "classes3.dex 内缺 KillerApplication193"
assert b"com.rsg.wdyz" in c3_out, "classes3.dex 内缺包名 com.rsg.wdyz"
assert b"Lcom/starry/magic/sign/SignSpoof;" not in c3_out, "classes3.dex 内含 SignSpoof（禁用）"
# ---- Manifest 注入项集合校验（二进制 AXML，字符串池可能 UTF-8 或 UTF-16LE）----
mf_out = zout2.read("AndroidManifest.xml")
def _axml_has(blob, s):
    return s.encode("utf-8") in blob or s.encode("utf-16-le") in blob
for need in ("com.starry.magic.StarryInitProvider", "com.rsg.wdyz.starryinit",
             "com.starry.magic.menu.MenuService"):
    assert _axml_has(mf_out, need), "AndroidManifest 注入缺失: " + need
assert hashlib.sha256(mf_out).hexdigest() != hashlib.sha256(zin2.read("AndroidManifest.xml")).hexdigest(), "AndroidManifest 未被注入"
print("  classes3.dex killer ok (sha256 %s…)" % hashlib.sha256(c3_out).hexdigest()[:16])
print("  manifest 注入项齐: StarryInitProvider + authorities + MenuService")
if new_so is not None:
    assert hashlib.sha256(zout2.read("lib/arm64-v8a/libil2cpp.so")).hexdigest() == hashlib.sha256(new_so).hexdigest()
    print(f"  embedded libil2cpp.so sha256 = {hashlib.sha256(new_so).hexdigest()[:16]}… (embedded)")
    bak_so_sha = None
else:
    bak_so_sha = hashlib.sha256(zout2.read("lib/arm64-v8a/libil2cpp.so")).hexdigest()
    assert bak_so_sha == hashlib.sha256(zin2.read("lib/arm64-v8a/libil2cpp.so")).hexdigest()
    print(f"  keep base libil2cpp.so sha256 = {bak_so_sha[:16]}… (未替换)")
# 官方条目逐字节透传（排除注入项、签名块；替换 so 时另排除 so）
inj = set(extras) | {"classes2.dex", "classes3.dex", "AndroidManifest.xml"}
if new_so is not None:
    inj.add("lib/arm64-v8a/libil2cpp.so")
n_same = n_checked = 0
for item in zin2.infolist():
    name = item.filename
    if name.startswith("META-INF/") or name in inj:
        continue
    n_checked += 1
    if hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest():
        n_same += 1
print(f"  merged ok; arsc & classes.dex 原样（hash 一致）")
print(f"  官方条目透传校验: {n_same}/{n_checked}")
if n_same != n_checked:
    raise SystemExit("ERROR: 官方条目 hash 不一致（外科透传破坏）")
zin2.close(); zout2.close()
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
cp -f "$SIGNED" "$ALIAS_APK"
ls -lh "$SIGNED" "$ALIAS_APK"

# 成品闭环自检：包内 so == 构建产物
unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so > "$WORK/final.so"
if [[ "$(md5 -q "$WORK/final.so")" != "$(md5 -q "$SO_BUILT")" ]]; then
  echo "ERROR: 成品包内 so 与本次构建产物 md5 不一致"
  exit 1
fi
echo "  final apk so md5 == 构建产物 ok"
# A 轨闭环自检：成品包 classes3.dex == 本次 killer dex，含包名/类名，无 SignSpoof
python3.11 - "$SIGNED" "$KILLER_DEX" <<'PY'
import hashlib, sys, zipfile
apk = zipfile.ZipFile(sys.argv[1])
names = apk.namelist()
for need in ("classes.dex", "classes2.dex", "classes3.dex"):
    assert need in names, "成品包缺 " + need
c3 = apk.read("classes3.dex")
want = hashlib.sha256(open(sys.argv[2], "rb").read()).hexdigest()
got = hashlib.sha256(c3).hexdigest()
assert got == want, "成品 classes3.dex sha256 != killer dex: %s vs %s" % (got, want)
assert b"KillerApplication193" in c3, "成品 classes3.dex 缺 KillerApplication193"
assert b"com.rsg.wdyz" in c3, "成品 classes3.dex 缺 com.rsg.wdyz"
assert b"Lcom/starry/magic/sign/SignSpoof;" not in c3, "成品含 SignSpoof（禁用）"
print("  final classes3.dex sha256 = " + got + " (killer ok)")
PY
shasum -a 256 "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/wdyz_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
  else
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      echo "[install] signature mismatch → uninstall + reinstall (ALLOW_UNINSTALL=1)"
      adb shell "pm uninstall $PKG" 2>/dev/null || adb uninstall "$PKG" || true
      adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" || adb install -r "$SIGNED"
    else
      echo "ERROR: 签名与真机原版不一致，覆盖安装失败。"
      echo "  需卸载原版才能装星光包。确认后设 STARRY_ALLOW_UNINSTALL=1 再跑。"
      exit 2
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Wdyz:V MenuService:V"
fi
echo "[done] $SIGNED"
