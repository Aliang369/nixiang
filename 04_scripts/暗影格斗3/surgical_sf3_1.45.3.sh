#!/usr/bin/env bash
# 暗影格斗3 1.45.3 (com.wanda.sf3.wd) TapTap — 星光外科组装
#
# 官包零改动：classes.dex..classes7.dex / resources.arsc / assets/** / lib/**.so 字节不变。
# 改动：
#   - 改 AndroidManifest（保留原 Application=com.accesssdkwrapper.AccessSDKApplicationWrapper；
#     appComponentFactory → com.starry.magic.sign.StarryAppComponentFactory（冷启动即回灌官方签名）；
#     +StarryInitProvider +MenuService +FOREGROUND_SERVICE 两条权限）
#   - 新增 classes8.dex（星光 smali：StarryInitProvider / MenuService / SignSpoof / TapLoginForceWeb / OTA）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
# 功能全在 native（Games/Sf3Mod.cpp：伤害倍率 / 上帝模式 / 战斗速度 / 无限体力 / 无限货币）。
#
# 产物：08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-unsigned.apk（未签名）
#       签名由 04_scripts/暗影格斗3/sf3_sign.sh（pipeline sign 阶段）负责
#       07_patched/暗影格斗3/surgical/{AndroidManifest.xml,classes8.dex}（留证 / pre_install_gate 门禁用）
#
# 用法: bash 04_scripts/暗影格斗3/surgical_sf3_1.45.3.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="暗影格斗3"
VERSION="1.45.3"
PKG="com.wanda.sf3.wd"
ORIG_APK="${SF3_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_taptap_official_from_device.apk}"
ORIG_MD5="${SF3_BASE_MD5:-7ac7591436e92136f22b715c91b5b930}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
SO_PREBUILT="${SF3_SO:-$STARRY_SRC/libs_sf3/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 sf3_build_so.sh）" >&2; exit 1; }
echo "[check] modern so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 $ACTUAL_MD5" >&2; exit 1; }

LOCK_DIR="/tmp/sf3_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/sf3_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease (starry → smali + certs)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] apktool d starry → com/starry smali → classes8.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for need in \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/StarryAppComponentFactory.smali" \
  "com/starry/magic/sign/TapLoginForceWeb.smali"; do
  [[ -f "$SMALI_ROOT/$need" ]] || { echo "ERROR: $need missing（Java 桥没编进 starry app）" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/classes8.dex" "$SMALI_ROOT"
ls -la "$WORK/classes8.dex"

# dex↔Java JNI 声明交叉校验：ModBridge 声明的 native 方法名必须进 dex 字符串池
# （防"so 注册 N 个方法、dex 只声明 M<N 个"→ 装机即 UnsatisfiedLinkError）
MB_JAVA="$STARRY_SRC/app/src/main/java/com/starry/magic/bridge/ModBridge.java"
[[ -f "$MB_JAVA" ]] || { echo "ERROR: missing $MB_JAVA" >&2; exit 1; }
NATIVE_TOTAL=0
NATIVE_MISS=0
for m in $(LC_ALL=C grep -oE '\bnative[A-Za-z0-9_]+' "$MB_JAVA" | sort -u); do
  NATIVE_TOTAL=$((NATIVE_TOTAL + 1))
  LC_ALL=C grep -aqF "$m" "$WORK/classes8.dex" || {
    echo "ERROR: classes8.dex 缺 native 声明 $m" >&2
    NATIVE_MISS=$((NATIVE_MISS + 1))
  }
done
[[ "$NATIVE_MISS" -eq 0 ]] \
  || { echo "ERROR: classes8.dex 与 ModBridge.java 声明不一致（$NATIVE_MISS/$NATIVE_TOTAL 缺失）" >&2; exit 1; }
echo "  dex↔ModBridge native 声明校验通过（$NATIVE_TOTAL 项）"

# 留证：装机前门禁（pre_install_gate --dex）与复查都要用这个 dex
STAGE_DIR="$NIXIANG/07_patched/$GAME/surgical"
mkdir -p "$STAGE_DIR"
cp -f "$WORK/classes8.dex" "$STAGE_DIR/classes8.dex"
echo "  classes8.dex → $STAGE_DIR/classes8.dex ($(md5 -q "$STAGE_DIR/classes8.dex"))"

# ---- 改包法（2026-10-28 定案·斌哥包对照结论）：TapTap 登录只改一个 action 常量 ----
# 真机对照样本（CN=binge2.keystore）把
#   com/taptap/sdk/login/internal/handlers/client/IntentHelper 里的
#   "com.taptap.sdk.action" 改成 "com.tap12tap.sdk.action"：
# 唤起 TapTap 客户端的 intent 永远匹配不到 ⇒ 登录入口自动落到网页(H5)登录，
# 也就避开了 TapTap 客户端侧的调用方签名校验。改动面比换 WebHandler 更小、不动 handler 语义。
# 签名校验（hoolai / C# 常量 / 服务端）不在这里处理——由 classes8.dex 里的
# com.starry.magic.sign.SignatureKiller 在 StarryInitProvider 阶段换 PackageInfo.CREATOR 解决。
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN" >&2; exit 1; }
echo "[2.5/6] 游戏 dex 补丁（TapTap 强制网页登录；classes.dex / IntentHelper action 改名）"
unzip -o -q "$ORIG_APK" classes.dex -d "$WORK/dexsrc1"
cp -f "$WORK/dexsrc1/classes.dex" "$STAGE_DIR/orig_classes.dex"
DEX1_SMALI="$WORK/dex1_smali"
mkdir -p "$DEX1_SMALI"
"$BAKSMALI_BIN" d "$WORK/dexsrc1/classes.dex" -o "$DEX1_SMALI"
INTENT_HELPER="$DEX1_SMALI/com/taptap/sdk/login/internal/handlers/client/IntentHelper.smali"
[[ -f "$INTENT_HELPER" ]] || { echo "ERROR: IntentHelper.smali 缺失（TapSDK 版本漂移，锚点须现取）" >&2; exit 1; }
python3 - "$INTENT_HELPER" <<'PATCH_INTENT_PY'
import sys
p = sys.argv[1]
t = open(p, encoding='utf-8').read()
OLD = 'const-string p1, "com.taptap.sdk.action"'
NEW = 'const-string p1, "com.tap12tap.sdk.action"'
n = t.count(OLD)
if n != 1:
    raise SystemExit('补丁锚点漂移：com.taptap.sdk.action 出现 %d 次（期望 1 次）' % n)
open(p, 'w', encoding='utf-8').write(t.replace(OLD, NEW, 1))
print('  smali 补丁完成：IntentHelper action → com.tap12tap.sdk.action（强制网页登录）')
PATCH_INTENT_PY
"$SMALI_BIN" assemble -o "$WORK/classes.dex" "$DEX1_SMALI"
VERIFY1_DIR="$WORK/dex1_verify"
mkdir -p "$VERIFY1_DIR"
"$BAKSMALI_BIN" d "$WORK/classes.dex" -o "$VERIFY1_DIR"
V_INTENT="$VERIFY1_DIR/com/taptap/sdk/login/internal/handlers/client/IntentHelper.smali"
grep -q 'com.tap12tap.sdk.action' "$V_INTENT" \
  || { echo "ERROR: 回读校验失败：IntentHelper action 未改名" >&2; exit 1; }
if grep -q 'com.taptap.sdk.action' "$V_INTENT"; then
  echo "ERROR: 回读校验失败：仍残留原 action 常量" >&2; exit 1
fi
rm -rf "$VERIFY1_DIR"
cp -f "$WORK/classes.dex" "$STAGE_DIR/classes.dex"
echo "  classes.dex(patched) → $STAGE_DIR/classes.dex ($(md5 -q "$STAGE_DIR/classes.dex"))"
# ---- 签名校验：直接采用斌哥包里的「MT ApkSignatureKillerEx」（改父类注入，真机连续 3 次启动实测通过）----
# 2026-10-29 实测教训：自研 SignatureKiller（只换 PackageInfo.CREATOR）只能撑住「首启」，
#   第二次启动 hoolai SDK 在 Application 阶段就取签名（frida 实测 appSignatures 仍返回星空证书
#   SHA-1 82:1C:92:E4:…），说明 CREATOR 替换被 Parcel 的 creator 缓存/时机吃掉；
#   而斌哥包（CN=binge2）连续 3 次启动都 InitSuccessCallBack。
# 于是照抄其机制：
#   ① 把 androidx/multidex/MultiDexApplication 的父类改成 bin.mt.signature.KillerApplication193
#      （游戏 Application = com.accesssdkwrapper.AccessSDKApplicationWrapper → MultiDexApplication，
#        改父类即可在 Application 构造期装好签名替换器，Manifest 一点不用动）；
#   ② KillerApplication193 的静态块 killPM(包名, 官方证书 base64) + killOpen(...)，
#      内部还清 PackageManager.sPackageInfoCache 与 Parcel.mCreators / sPairedCreators
#      —— 这正是我们自己那版缺的一环；
#   ③ 该类原父类是斌哥菜单里的壳 com.wanda.sf3.wd.PkjNluikmwxGUJv，我们改成 android.app.Application。
# 产物 dex 名用 classes9.dex（classes8.dex 已被星光菜单占用）。
echo "[2.6/6] 签名校验（MT ApkSignatureKillerEx 改父类；classes5.dex + classes9.dex）"
unzip -o -q "$ORIG_APK" classes5.dex -d "$WORK/dexsrc5"
cp -f "$WORK/dexsrc5/classes5.dex" "$STAGE_DIR/orig_classes5.dex"
DEX5_SMALI="$WORK/dex5_smali"
mkdir -p "$DEX5_SMALI"
"$BAKSMALI_BIN" d "$WORK/dexsrc5/classes5.dex" -o "$DEX5_SMALI"
MULTIDEX="$DEX5_SMALI/androidx/multidex/MultiDexApplication.smali"
[[ -f "$MULTIDEX" ]] || { echo "ERROR: MultiDexApplication.smali 缺失（底包结构漂移）" >&2; exit 1; }
python3 - "$MULTIDEX" <<'PATCH_MULTIDEX_PY'
import sys
p = sys.argv[1]
t = open(p, encoding='utf-8').read()
OLD = '.super Landroid/app/Application;'
NEW = '.super Lbin/mt/signature/KillerApplication193;'
n = t.count(OLD)
if n != 1:
    raise SystemExit('补丁锚点漂移：MultiDexApplication 的 .super 出现 %d 次（期望 1 次）' % n)
open(p, 'w', encoding='utf-8').write(t.replace(OLD, NEW, 1))
print('  smali 补丁完成：MultiDexApplication.super → bin.mt.signature.KillerApplication193')
PATCH_MULTIDEX_PY
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$DEX5_SMALI"
VERIFY5_DIR="$WORK/dex5_verify"
mkdir -p "$VERIFY5_DIR"
"$BAKSMALI_BIN" d "$WORK/classes5.dex" -o "$VERIFY5_DIR"
grep -q 'Lbin/mt/signature/KillerApplication193;' "$VERIFY5_DIR/androidx/multidex/MultiDexApplication.smali" \
  || { echo "ERROR: 回读校验失败：MultiDexApplication 父类未改" >&2; exit 1; }
rm -rf "$VERIFY5_DIR"
cp -f "$WORK/classes5.dex" "$STAGE_DIR/classes5.dex"
echo "  classes5.dex(patched) → $STAGE_DIR/classes5.dex ($(md5 -q "$STAGE_DIR/classes5.dex"))"

# Killer dex：源码来自斌哥包内 classes8.dex（MT ApkSignatureKillerEx），父类改回 android.app.Application
KILLER_SMALI="$NIXIANG/07_patched/$GAME/surgical/killer_smali"
[[ -d "$KILLER_SMALI" ]] || { echo "ERROR: 缺 killer smali 源目录 $KILLER_SMALI" >&2; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes9.dex" "$KILLER_SMALI"
VERIFY9_DIR="$WORK/dex9_verify"
mkdir -p "$VERIFY9_DIR"
"$BAKSMALI_BIN" d "$WORK/classes9.dex" -o "$VERIFY9_DIR"
K9="$VERIFY9_DIR/bin/mt/signature/KillerApplication193.smali"
[[ -f "$K9" ]] || { echo "ERROR: 回读校验失败：classes9.dex 缺 KillerApplication193" >&2; exit 1; }
grep -q '^.super Landroid/app/Application;' "$K9" || { echo "ERROR: KillerApplication193 父类非 Application" >&2; exit 1; }
LC_ALL=C grep -aq 'MIIDSTCCAjGgAwIBAgIEJhYcejAN' "$WORK/classes9.dex" \
  || { echo "ERROR: classes9.dex 缺官方证书 base64" >&2; exit 1; }
rm -rf "$VERIFY9_DIR"
cp -f "$WORK/classes9.dex" "$STAGE_DIR/classes9.dex"
echo "  classes9.dex(killer) → $STAGE_DIR/classes9.dex ($(md5 -q "$STAGE_DIR/classes9.dex"))"


echo "[3/6] binary AndroidManifest（保留原 Application；换 appComponentFactory + provider/service/权限）"
if [[ -n "${SF3_BINARY_MANIFEST:-}" && -f "${SF3_BINARY_MANIFEST:-}" ]]; then
  cp "$SF3_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $SF3_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s --no-assets)"
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
pkg = sys.argv[2]
authority = sys.argv[3]
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no <uses-permission> in manifest")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# 冷启动即回灌官方证书（TapSDK 读签名早于 Provider.onCreate 的场景）
old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"'
if new_factory not in t:
    if old_factory not in t:
        raise SystemExit("expected CoreComponentFactory not found")
    t = t.replace(old_factory, new_factory, 1)

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="%s"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
''' % authority
    t = t.replace("</application>", prov + "\n    </application>", 1)

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
print("  text Manifest patched (保留原 Application)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验（provider/service/权限/factory + 官方属性不变）"
if [[ -x "$AAPT2" ]]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  python3 - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  grep -q "com.starry.magic.StarryInitProvider" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 StarryInitProvider" >&2; exit 1; }
  grep -q "com.starry.magic.menu.MenuService" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 MenuService" >&2; exit 1; }
  grep -q "android.permission.FOREGROUND_SERVICE_SPECIAL_USE" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 缺 FOREGROUND_SERVICE_SPECIAL_USE" >&2; exit 1; }
  grep -q "com.starry.magic.sign.StarryAppComponentFactory" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 未替换 appComponentFactory" >&2; exit 1; }
  grep -q "com.accesssdkwrapper.AccessSDKApplicationWrapper" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 丢了官方 Application" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  # 结构化比对：按「元素路径 + 属性全值」做多重集，忽略 aapt2 输出顺序。
  # （apktool 重排会把 uses-sdk 挪到 manifest 开头，逐行 diff 会误报丢节点；不再是该口径）
  python3 - "$WORK/o.norm" "$WORK/n.norm" <<'PY'
import sys
from collections import Counter
from pathlib import Path


def canon(path):
    stack, out = [], []
    for ln in Path(path).read_text(encoding='utf-8').splitlines():
        if not ln.strip():
            continue
        ind = len(ln) - len(ln.lstrip(' '))
        core = ln.strip()
        if core.startswith('N:'):
            continue
        # aapt2 缩进：元素级 L 在 4L-2 列，属性在 4L 列（L 从 1 开始）
        if core.startswith('E:'):
            lv = (ind + 2) // 4
            stack = stack[:lv - 1]
            stack.append(core[2:].strip().split(' ')[0])
            out.append('/'.join(stack))
        elif core.startswith('A:') and stack:
            out.append('/'.join(stack[:ind // 4]) + '|' + core)
    return Counter(out)


o, n = canon(sys.argv[1]), canon(sys.argv[2])
dropped = o - n
added = sum((n - o).values())
unexpected = {k: v for k, v in dropped.items()
              if not ('appComponentFactory' in k
                      and 'androidx.core.app.CoreComponentFactory' in k)}
if unexpected:
    for k, v in list(unexpected.items())[:10]:
        print(f'  LOST x{v}: {k}', file=sys.stderr)
    raise SystemExit(f'manifest 重建丢失官方节点（{sum(unexpected.values())} 项）')
if not (15 <= added <= 30):
    raise SystemExit(f'manifest 新增节点数异常（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 丢弃 {sum(dropped.values())} 项=旧 factory，顺序无关)')
PY
else
  echo "  WARN: aapt2 不在 ${AAPT2}，跳过 manifest 回读校验" >&2
fi

echo "[5/6] surgical zip merge（原 dex/arsc/assets/lib 字节不变）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx = Path(sys.argv[5]).read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes8 = (work / 'classes8.dex').read_bytes()

replace = {'AndroidManifest.xml': manifest}
dex1_path = work / 'classes.dex'
if dex1_path.exists():
    replace['classes.dex'] = dex1_path.read_bytes()
dex5_path = work / 'classes5.dex'
if dex5_path.exists():
    replace['classes5.dex'] = dex5_path.read_bytes()
classes9 = (work / 'classes9.dex')
classes9 = classes9.read_bytes() if classes9.exists() else None
extras = {
    'classes8.dex': classes8,
    'lib/arm64-v8a/libstarry_magic.so': so,
    'lib/arm64-v8a/libc++_shared.so': cxx,
}

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name == 'META-INF/MANIFEST.MF' or (name.startswith('META-INF/') and name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if name in extras:
            continue
        data = replace.get(name)
        if data is None:
            data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == 'resources.arsc' else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)
    if classes9 is not None:
        extras['classes9.dex'] = classes9
    for name, data in extras.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name.startswith('lib/') and name.endswith('.so') else zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    changed = 0
    for n in original_names:
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        changed += 1
    for n in ('classes.dex', 'classes7.dex', 'resources.arsc',
              'lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libunity.so',
              'assets/bin/Data/Managed/Metadata/global-metadata.dat'):
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
    if 'classes8.dex' not in out_names:
        raise SystemExit('classes8.dex missing')
    if 'classes5.dex' in replace:
        pkg_d5 = zout.read('classes5.dex')
        if hashlib.sha256(pkg_d5).digest() != hashlib.sha256(replace['classes5.dex']).digest():
            raise SystemExit('packaged classes5.dex != patched classes5.dex')
        if b'bin/mt/signature/KillerApplication193' not in pkg_d5:
            raise SystemExit('packaged classes5.dex missing KillerApplication super patch')
    if classes9 is not None:
        pkg_d9 = zout.read('classes9.dex')
        if hashlib.sha256(pkg_d9).digest() != hashlib.sha256(classes9).digest():
            raise SystemExit('packaged classes9.dex != killer dex')
        if b'bin/mt/signature/KillerApplication193' not in pkg_d9:
            raise SystemExit('packaged classes9.dex missing KillerApplication193')
    if 'classes.dex' in replace:
        pkg_d1 = zout.read('classes.dex')
        if hashlib.sha256(pkg_d1).digest() != hashlib.sha256(replace['classes.dex']).digest():
            raise SystemExit('packaged classes.dex != patched classes.dex')
        if b'com.tap12tap.sdk.action' not in pkg_d1:
            raise SystemExit('packaged classes.dex missing IntentHelper action patch')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'surgical zip verified: {changed} 个原始条目字节不变 + classes8.dex + starry so')

# >2G 包归一化（照搬 Phigros 3.20.0 已实证方案，见 scripts/surgical_phigros_3.20.0.sh）：
# python zipfile 的 ZIP64_LIMIT=2^31-1（非 4G），2.67G 输出会被写上 zip64 EOCD +
# 晚期条目 CD zip64 extra（hoff=0xFFFFFFFF / vneed=45）。官方工具链对 2-4G 包写的是
# 经典记录；zipalign(36.0.0) 走 zip64 路径对 >2G 文件报 "Unable to open as zip archive"
# （本次实测），且 CD vneed=45 与本地头 ver=20 不一致会触发 BadPack 防护。
# 修复：剥离逐条目 zip64 extra 还原真实 hoff、vneed 回填本地头版本、尾部改纯经典 EOCD
# （cdoff < 4G，uint32 合法，形态与原包一致）。
def normalize_zip(path):
    import struct
    f = open(path, "r+b")
    f.seek(0, 2); size = f.tell()
    f.seek(max(0, size - 65536)); tail = f.read()
    e = tail.rfind(b"PK\x05\x06")
    assert e >= 0, "EOCD not found"
    _, _, _, _, n2, cdsize, cdoff, clen = struct.unpack("<IHHHHIIH", tail[e:e+22])
    assert clen == 0, "unexpected EOCD comment"
    if cdoff == 0xFFFFFFFF or cdsize == 0xFFFFFFFF or n2 == 0xFFFF:
        e64 = e - 76
        assert e64 >= 0 and tail[e64:e64+4] == b"PK\x06\x06", "expected EOCD64 before EOCD"
        count64, = struct.unpack("<Q", tail[e64+32:e64+40])
        cdsize64, cdoff64 = struct.unpack("<QQ", tail[e64+40:e64+56])
        n2, cdsize, cdoff = count64, cdsize64, cdoff64
    f.seek(cdoff); cd = f.read(cdsize)
    out = bytearray(); pos = fixed = 0
    while pos < len(cd):
        assert cd[pos:pos+4] == b"PK\x01\x02", f"bad CD sig @{pos}"
        nlen, elen, clen2 = struct.unpack_from("<HHH", cd, pos+28)
        hoff = struct.unpack_from("<I", cd, pos+42)[0]
        vneed = struct.unpack_from("<H", cd, pos+6)[0]
        total = 46 + nlen + elen + clen2
        rec = bytearray(cd[pos:pos+total])
        if hoff == 0xFFFFFFFF:
            name = cd[pos+46:pos+46+nlen]
            extra = cd[pos+46+nlen:pos+46+nlen+elen]
            comment = cd[pos+46+nlen+elen:pos+total]
            real = None; blocks = []; q = 0
            while q + 4 <= len(extra):
                bid, bsz = struct.unpack_from("<HH", extra, q)
                payload = extra[q+4:q+4+bsz]
                if bid == 1:
                    real = payload
                else:
                    blocks.append((bid, payload))
                q += 4 + bsz
            assert real is not None and len(real) == 8, "unexpected zip64 payload"
            real_off = struct.unpack("<Q", real)[0]
            assert real_off < 0xFFFFFFFF, "entry offset exceeds classic uint32"
            f.seek(real_off + 4); lver = struct.unpack("<H", f.read(2))[0]
            new_extra = b"".join(struct.pack("<HH", bid, len(pl)) + pl for bid, pl in blocks)
            rec = bytearray(cd[pos:pos+46])
            struct.pack_into("<H", rec, 6, lver)       # vneed = LFH version（消 header mismatch）
            struct.pack_into("<H", rec, 30, len(new_extra))
            struct.pack_into("<I", rec, 42, real_off)  # 真实 hoff（uint32）
            out += rec + name + new_extra + comment
            fixed += 1
        elif vneed == 45:
            f.seek(hoff + 4); lver = struct.unpack("<H", f.read(2))[0]
            struct.pack_into("<H", rec, 6, lver)
            out += rec
            fixed += 1
        else:
            out += rec
        pos += total
    new_cdsize = len(out)
    f.seek(cdoff); f.write(out)
    f.seek(cdoff + new_cdsize)
    f.write(struct.pack("<IHHHHIIH", 0x06054b50, 0, 0, n2, n2, new_cdsize, cdoff, 0))
    f.truncate()
    f.close()
    print(f'  normalized: zip64 extras stripped/vneed fixed={fixed}, classic EOCD ok')


normalize_zip(str(dst))
PY

echo "[6/6] unsigned output → $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
