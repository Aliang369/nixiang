#!/usr/bin/env bash
# 正中靶心2 1.9 — 星空外科组装（2026-11-02 双轨制版）。
#
# 与 2026-09-26 版（surgical_zzbx2_1.9.sh）的差别：只改 TapTap 这条链，玩法侧零改动。
#   签名/完整性校验 → B 轨「运行时签名替换」：注入 MT ApkSignatureKillerEx 形态的
#     bin.mt.signature.KillerApplication193（classes6.dex，已烘焙官方证书与包名），
#     由 StarryAppComponentFactory.instantiateApplication 提前 Class.forName(clinit) 挂载
#     ⇒ TapSignKit.getSign 里的 pm.getPackageInfo(GET_SIGNATURES) 自然返回官方签名。
#     **不再静态改 TapSignKit.getSign**（旧版改法；同一链条禁止双修）。
#   登录方式/渠道分支 → A 轨「改包法」：classes3.dex 里
#     ClientLoginUtils.isClientLogin(Activity, Actions) 恒 false ⇒ 落内嵌网页登录。
#
# 依据：AGENTS.md §3「渠道包是 TapTap/TapSDK」条目 + skill/re-analyze/login/taptap-web-login.md
#       「现行口径（2026-10-29 双轨制）」+ 10_docs/_templates/经验_运行时签名替换挂载点与dex等长改包_20261029.md
#
# 底包唯一合法输入：官方 1.9 原包（md5 3b126e42a931d712ceef85b0c9b8ec05）。
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
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
VER="${ZZBX2_VER:-1.9}"
ORIG_APK="${ZZBX2_BASE_APK:-$NIXIANG/02_apks/正中靶心2/正中靶心2_${VER}_official.apk}"
ORIG_MD5="${ZZBX2_BASE_MD5:-3b126e42a931d712ceef85b0c9b8ec05}"
OUT_DIR="$NIXIANG/07_patched/正中靶心2"
UNSIGNED="$OUT_DIR/正中靶心2_${VER}_starry-unsigned.apk"
PATCH_JS="$SCRIPT_DIR/patch_main.js"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PKG="com.openew.monster"
SO_PREBUILT="${ZZBX2_SO:-$STARRY_SRC/libs_zzbx2/arm64-v8a/libstarry_magic.so}"
KILLER_ASSET="$SCRIPT_DIR/assets/zzbx2_signature_killer.dex"
TAP_GATE_DEX="classes3.dex"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing base APK $ORIG_APK" >&2; exit 1; }
[[ -f "$PATCH_JS" ]] || { echo "ERROR: missing $PATCH_JS" >&2; exit 1; }
[[ -f "$KILLER_ASSET" ]] || { echo "ERROR: missing killer dex $KILLER_ASSET" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing starry so $SO_PREBUILT" >&2; exit 1; }
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared at $NDK_CXX" >&2; exit 1; }

ACTUAL_MD5="$(md5 -q "$ORIG_APK")"
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: base APK fingerprint mismatch: $ACTUAL_MD5" >&2; exit 1; }

# 官方底包自检：不得已含星空外挂物（避免在改包上二次组装）
python3 - "$ORIG_APK" <<'PYCHK'
import sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z:
    names = set(z.namelist())
    bad = [n for n in ('classes5.dex', 'classes6.dex', 'lib/arm64-v8a/libstarry_magic.so') if n in names]
if bad:
    raise SystemExit('ERROR: base APK is not official (already contains %s)' % bad)
print('  base APK is official (no starry extras)')
PYCHK

LOCK="/tmp/zzbx2_surgical.lockdir"
if ! mkdir "$LOCK" 2>/dev/null; then echo "ERROR: lock held: $LOCK" >&2; exit 1; fi
echo $$ > "$LOCK/pid"
WORK="$(mktemp -d /tmp/zzbx2_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK"' EXIT
mkdir -p "$OUT_DIR" "$WORK"

echo "[1/8] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_zzbx2/' --exclude 'libs_zzbx2/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: starry apk missing" >&2; exit 1; }

echo "[2/8] collect starry smali -> classes5.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] || { echo "ERROR: StarryApp missing" >&2; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/cocos/Zzbx2Hooks.smali" ]] || { echo "ERROR: Zzbx2Hooks missing from Gradle build" >&2; exit 1; }

# API 24-27 会忽略 manifest 的 appComponentFactory，入口同时改 StarryApp 父类兜底。
# （本类不再安装任何签名伪装；签名校验走 classes6.dex 的运行时 killer）
python3 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/openew/game/OpenewApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/openew/game/OpenewApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/openew/game/OpenewApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/openew/game/OpenewApplication;->onCreate()V",
    ),
]
changed = 0
for old, new in repls:
    if old in t:
        t = t.replace(old, new)
        changed += 1
if ".super Lcom/openew/game/OpenewApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali did not switch super to OpenewApplication")
for bad in (
    "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
    "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
    "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
):
    if bad in t:
        raise SystemExit("ERROR: StarryApp.smali still has old super call: " + bad)
p.write_text(t, encoding="utf-8")
print("  StarryApp -> OpenewApplication ok (%d edits)" % changed)
PY

"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -lh "$WORK/classes5.dex"

echo "[3/8] A 轨改包法：$TAP_GATE_DEX 的 ClientLoginUtils.isClientLogin -> false（登录恒走内嵌网页）"
python3 - "$ORIG_APK" "$WORK" "$BAKSMALI_BIN" "$SMALI_BIN" "$TAP_GATE_DEX" <<'PY'
from pathlib import Path
import re
import shutil
import subprocess
import sys
import zipfile

apk = Path(sys.argv[1])
work = Path(sys.argv[2])
baksmali = sys.argv[3]
smali = sys.argv[4]
dex_name = sys.argv[5]

with zipfile.ZipFile(apk) as z:
    source_dex = work / ("original_" + dex_name)
    source_dex.write_bytes(z.read(dex_name))

unpacked = work / "tap_smali"
if unpacked.exists():
    shutil.rmtree(unpacked)
subprocess.run([baksmali, "d", "-o", str(unpacked), str(source_dex)], check=True)

target = unpacked / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not target.is_file():
    raise SystemExit("missing " + str(target))
text = target.read_text(encoding="utf-8")
pattern = re.compile(
    r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z"
    r".*?\.end method",
    re.S,
)
replacement = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    const/4 v0, 0x0

    return v0
.end method"""
text, count = pattern.subn(replacement, text, count=1)
if count != 1:
    raise SystemExit("ClientLoginUtils.isClientLogin method not found")
target.write_text(text, encoding="utf-8")

# 同一链条禁双修：验签走运行时 killer，TapSignKit 必须保持官方原样
sign_kit = unpacked / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
if not sign_kit.is_file():
    raise SystemExit("missing TapSignKit.smali")
if "b9c07db608080d40b04254d32ac3fc5c" in sign_kit.read_text(encoding="utf-8"):
    raise SystemExit("TapSignKit.getSign 仍被静态改成官方 MD5 —— 违反双轨制同一链条禁双修")

subprocess.run([smali, "assemble", "-o", str(work / "tap_gate.dex"), str(unpacked)], check=True)
print("  patched ClientLoginUtils.isClientLogin -> false；TapSignKit 保持官方")
PY
[[ -f "$WORK/tap_gate.dex" ]] || { echo "ERROR: classes3.dex 改包法失败" >&2; exit 1; }

echo "[4/8] B 轨签名轨：killer dex -> classes6.dex（烘焙 $PKG + 官方证书）"
cp "$KILLER_ASSET" "$WORK/classes6.dex"
python3 - "$WORK/classes6.dex" <<'PY'
import sys
from pathlib import Path
data = Path(sys.argv[1]).read_bytes()
checks = {
    'KillerApplication193': b'KillerApplication193',
    'bin/mt/signature': b'bin/mt/signature/',
    'package com.openew.monster': b'com.openew.monster',
    'official cert base64': b'MIID',
}
for k, needle in checks.items():
    if needle not in data:
        raise SystemExit('ERROR: killer dex 缺少 ' + k)
if b'com.leiting' in data:
    raise SystemExit('ERROR: killer dex 仍残留模板包名 com.leiting')
print('  killer dex ok (%d B, cert baked, pkg com.openew.monster)' % len(data))
PY
ls -lh "$WORK/classes6.dex"

echo "[5/8] create Manifest patch"
apktool d -f -s -o "$WORK/base_dec" "$ORIG_APK" >/dev/null
python3 - "$WORK/base_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and "android.permission.FOREGROUND_SERVICE/" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no uses-permission node")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

old_app = 'android:name="com.openew.game.OpenewApplication"'
new_app = 'android:name="com.starry.magic.StarryApp"'
if new_app not in t:
    if old_app not in t:
        raise SystemExit("expected OpenewApplication not found in manifest")
    t = t.replace(old_app, new_app, 1)

old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.StarryAppComponentFactory"'
if new_factory not in t:
    if old_factory not in t:
        raise SystemExit("expected CoreComponentFactory not found")
    t = t.replace(old_factory, new_factory, 1)

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

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.openew.monster.starryinit"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("manifest patched: provider + MenuService + StarryApp + StarryAppComponentFactory")
PY
rm -rf "$WORK/base_dec"/smali* 2>/dev/null || true
apktool b -f "$WORK/base_dec" -o "$WORK/manifest_only.apk" >/dev/null
unzip -p "$WORK/manifest_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"

echo "[6/8] surgical zip merge（原 assets/main.js 保持字节不变）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" "$TAP_GATE_DEX" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx = Path(sys.argv[5]).read_bytes()
tap_dex_name = sys.argv[6]
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes5 = (work / 'classes5.dex').read_bytes()
classes6 = (work / 'classes6.dex').read_bytes()
tap_gate = (work / 'tap_gate.dex').read_bytes()

replace = {
    'AndroidManifest.xml': manifest,
    tap_dex_name: tap_gate,
}
extras = {
    'classes5.dex': classes5,
    'classes6.dex': classes6,
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
    for name, data in extras.items():
        info = zipfile.ZipInfo(name)
        info.compress_type = zipfile.ZIP_STORED if name.startswith('lib/') and name.endswith('.so') else zipfile.ZIP_DEFLATED
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    for n in original_names:
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit('unchanged entry changed: ' + n)
    for need in ('classes5.dex', 'classes6.dex'):
        if need not in out_names:
            raise SystemExit(need + ' missing')
    if hashlib.sha256(zin.read('assets/main.js')).digest() != hashlib.sha256(zout.read('assets/main.js')).digest():
        raise SystemExit('assets/main.js changed unexpectedly')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
    if zout.read(tap_dex_name) == zin.read(tap_dex_name):
        raise SystemExit(tap_dex_name + ' 未被改包（TapTap 方案关会判 FAIL）')
print('surgical zip verified: assets unchanged + classes3 改包 + classes5/classes6 + starry so')
PY

echo "[7/8] verify patched TapSDK dex"
unzip -p "$UNSIGNED" "$TAP_GATE_DEX" > "$WORK/verify_tap.dex"
"$BAKSMALI_BIN" d -o "$WORK/verify_tap_smali" "$WORK/verify_tap.dex"
python3 - "$WORK/verify_tap_smali/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali" \
          "$WORK/verify_tap_smali/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" <<'PY'
from pathlib import Path
import sys

cli = Path(sys.argv[1]).read_text(encoding="utf-8")
kit = Path(sys.argv[2]).read_text(encoding="utf-8")
if "const/4 v0, 0x0" not in cli:
    raise SystemExit("ClientLoginUtils.isClientLogin verification failed")
if "getPackageInfo(Ljava/lang/String;I)Landroid/content/pm/PackageInfo;" not in kit:
    raise SystemExit("TapSignKit.getSign 已被改写（应保持官方实现，签名由运行时 killer 负责）")
print("  packaged isClientLogin -> false；TapSignKit 仍走 PackageManager（killer 覆盖）")
PY

echo "[8/8] unsigned output -> $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
