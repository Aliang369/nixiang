#!/usr/bin/env bash
# 圣翼传说 7.5.10 (com.aiyue.wscq) TapTap — 星光外科组装
#
# 官包零改动：classes.dex / resources.arsc / assets/** / lib/**.so 字节不变。
# 改动：
#   - 改 AndroidManifest（原 <application> 无 android:name，保持默认 Application；
#     appComponentFactory: androidx.core.app.CoreComponentFactory →
#       com.starry.magic.sign.StarryAppComponentFactory（冷启动即回灌官方签名）；
#     + StarryInitProvider + MenuService + 三条权限）
#   - 新增 classes2.dex（星光 smali：StarryInitProvider / MenuService / StarryAppComponentFactory /
#     TapLoginForceWeb / ModBridge / OTA + yntt_smali 的 YnttLoginRequest）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#
# 功能全在 native（Games/WscqMod.cpp：伤害倍率 / 玩家无敌 / 全屏攻击；20261026 收窄）。
# 登录：本作是 TapSDK 旧版路径（com.taptap.sdk.ui.TapTapActivity）→ 必须同时做
#   ① TapLoginForceWeb 白名单加 com.aiyue.wscq ② yntt_smali 注入（YnttLoginRequest）。
#
# 产物：08_output/圣翼传说/圣翼传说_7.5.10_surgical-unsigned.apk（未签名）
#       07_patched/圣翼传说/surgical/{AndroidManifest.xml,classes2.dex}（留证 / 门禁用）
#
# 用法: bash 04_scripts/圣翼传说/surgical_wscq_7.5.10.sh
#   WSCQ_BASE_APK=/path/to/official.apk 可覆盖底包路径
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="圣翼传说"
VERSION="${WSCQ_VERSION:-7.5.10}"
PKG="com.aiyue.wscq"
ORIG_APK="${WSCQ_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_taptap_official_from_device.apk}"
ORIG_MD5="${WSCQ_BASE_MD5:-66b979b2bef80dce1bcc6223f57cc4ed}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="${WSCQ_MF_CACHE:-$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml}"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
YNTT_SMALI="$STARRY_SRC/yntt_smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（用 WSCQ_BASE_APK=... 指定底包）" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -d "$YNTT_SMALI/com/starry/magic/sign" ]] \
  || { echo "ERROR: missing ${YNTT_SMALI}（TapTap 旧版路径必需）" >&2; exit 1; }
SO_PREBUILT="${WSCQ_SO:-$STARRY_SRC/libs_wscq/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 04_scripts/圣翼传说/wscq_build_so.sh）" >&2; exit 1; }
echo "[check] modern so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 ${ACTUAL_MD5}（期望 ${ORIG_MD5}）" >&2; exit 1; }
echo "[check] 官包 = $ORIG_APK ($ACTUAL_MD5)"

# dex 落位名：★必须是 base.apk 里「最小的空闲 classesN.dex 下标」。
# ART 的 BaseDexClassLoader 从 classes.dex 起顺序扫描 classes2/3/…，遇到第一个缺口就停；
# 官包是单 dex（只有 classes.dex）→ 星光 dex 必须叫 classes2.dex。
# 若错写成 classes8.dex（官包无 classes2..7），classes8 永远不会被加载 →
# Manifest 里的 StarryInitProvider 一启动就 ClassNotFoundException（2026-09-24 实测踩坑）。
DEX_NAME="$(python3 - "$ORIG_APK" <<'PY'
import re, sys, zipfile
with zipfile.ZipFile(sys.argv[1]) as z:
    used = set()
    for n in z.namelist():
        m = re.fullmatch(r'classes(\d*)\.dex', n)
        if m:
            used.add(int(m.group(1)) if m.group(1) else 1)
i = 2
while i in used:
    i += 1
print('classes%d.dex' % i)
PY
)"
echo "[check] 星光 dex 落位 = $DEX_NAME"

LOCK_DIR="/tmp/wscq_surgical_${VERSION}.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wscq_surgical.XXXXXX)"
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

echo "[2/6] apktool d starry → com/starry smali + yntt_smali → $DEX_NAME"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
# TapTap 旧版强制网页登录：YnttLoginRequest 不是 gradle 产物，必须显式并入
rsync -a "$YNTT_SMALI/com/" "$SMALI_ROOT/com/"
for need in \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/StarryAppComponentFactory.smali" \
  "com/starry/magic/sign/TapLoginForceWeb.smali" \
  "com/starry/magic/sign/YnttLoginRequest.smali" \
  "com/starry/magic/sign/YnttLoginRequest\$1.smali"; do
  [[ -f "$SMALI_ROOT/$need" ]] || { echo "ERROR: $need missing（Java 桥没编进 starry app？）" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/$DEX_NAME" "$SMALI_ROOT"
ls -la "$WORK/$DEX_NAME"
LC_ALL=C grep -aqF "com/taptap/sdk/LoginRequest" "$WORK/$DEX_NAME" \
  || { echo "ERROR: $DEX_NAME 未含 YnttLoginRequest（super=LoginRequest 缺失）" >&2; exit 1; }

# dex↔Java JNI 声明交叉校验：防「so 注册 N 个方法、dex 只声明 M<N 个」→ UnsatisfiedLinkError
MB_JAVA="$STARRY_SRC/app/src/main/java/com/starry/magic/bridge/ModBridge.java"
[[ -f "$MB_JAVA" ]] || { echo "ERROR: missing $MB_JAVA" >&2; exit 1; }
NATIVE_TOTAL=0
NATIVE_MISS=0
for m in $(LC_ALL=C grep -oE '\bnative[A-Za-z0-9_]+' "$MB_JAVA" | sort -u); do
  NATIVE_TOTAL=$((NATIVE_TOTAL + 1))
  LC_ALL=C grep -aqF "$m" "$WORK/$DEX_NAME" || {
    echo "ERROR: $DEX_NAME 缺 native 声明 $m" >&2
    NATIVE_MISS=$((NATIVE_MISS + 1))
  }
done
[[ "$NATIVE_MISS" -eq 0 ]] \
  || { echo "ERROR: $DEX_NAME 与 ModBridge.java 声明不一致（$NATIVE_MISS/$NATIVE_TOTAL 缺失）" >&2; exit 1; }
echo "  dex↔ModBridge native 声明校验通过（$NATIVE_TOTAL 项）"

STAGE_DIR="${WSCQ_STAGE_DIR:-$NIXIANG/07_patched/$GAME/surgical}"
mkdir -p "$STAGE_DIR"
cp -f "$WORK/$DEX_NAME" "$STAGE_DIR/$DEX_NAME"
echo "  $DEX_NAME → $STAGE_DIR/$DEX_NAME ($(md5 -q "$STAGE_DIR/$DEX_NAME"))"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared.so at $NDK_CXX" >&2; exit 1; }
cp "$NDK_CXX" "$WORK/lib/arm64-v8a/"

echo "[3/6] binary AndroidManifest（默认 Application；换 appComponentFactory + provider/service/权限）"
if [[ -n "${WSCQ_BINARY_MANIFEST:-}" && -f "${WSCQ_BINARY_MANIFEST:-}" ]]; then
  cp "$WSCQ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $WSCQ_BINARY_MANIFEST"
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
print("  text Manifest patched（原 <application> 无 android:name，保持默认 Application）")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[4/6] binary Manifest 回读校验（provider/service/权限/factory + 官方节点不变）"
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
  grep -q "com.unity3d.player.PrivacyActivity" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 丢了官方 launcher activity" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  # 结构化比对：按「元素路径 + 属性全值」做多重集，忽略 aapt2 输出顺序
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
if not (15 <= added <= 40):
    raise SystemExit(f'manifest 新增节点数异常（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 丢弃 {sum(dropped.values())} 项=旧 factory，顺序无关)')
PY
else
  echo "  WARN: aapt2 不在 ${AAPT2}，跳过 manifest 回读校验" >&2
fi

echo "[5/6] surgical zip merge（原 dex/arsc/assets/lib 字节不变）"
python3 - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "$NDK_CXX" "$DEX_NAME" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx = Path(sys.argv[5]).read_bytes()
manifest = (work / 'AndroidManifest.xml').read_bytes()
dex_name = sys.argv[6]
dex_new = (work / dex_name).read_bytes()

replace = {'AndroidManifest.xml': manifest}
extras = {
    dex_name: dex_new,
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
    changed = 0
    for n in original_names:
        if n == 'META-INF/MANIFEST.MF' or (n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))) or n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        changed += 1
    for n in ('classes.dex', 'resources.arsc',
              'lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libunity.so',
              'assets/bin/Data/Managed/Metadata/global-metadata.dat'):
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
    if dex_name not in out_names:
        raise SystemExit(f'{dex_name} missing')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'surgical zip verified: {changed} 个原始条目字节不变 + {dex_name} + starry so')
PY

echo "[6/6] unsigned output → $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
