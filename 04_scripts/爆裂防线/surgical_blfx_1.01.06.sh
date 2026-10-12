#!/usr/bin/env bash
# 爆裂防线 1.01.06 (com.muyegame.blfx) — 星光外科组装
#
# 官包零改动：classes.dex / classes2.dex / resources.arsc / assets/** / lib/**.so 字节不变。
# 改动：
#   - 改 AndroidManifest（原 <application> 无 android:name，保持默认 Application；
#     appComponentFactory: androidx.core.app.CoreComponentFactory →
#       com.starry.magic.sign.SignSpoofAppComponentFactory（冷启动即回灌官方签名）；
#     + StarryInitProvider + MenuService + 三条权限）
#   - 新增 classes3.dex（星光 smali：StarryInitProvider / MenuService / SignSpoof /
#     TapLoginForceWeb / ModBridge / OTA）。★本作底包是**双 dex**
#     （classes.dex + classes2.dex）⇒ 星光 dex 必须落 classes3.dex（最小空闲下标）。
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#
# 功能全在 native（Games/BlfxMod.cpp：伤害倍率滑块 / 无敌 / 战斗加速滑块）。
#
# 登录：本作 TapSDK 走**新版**路径（com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity
#   + LoginRequest.loginType + DefaultTapLoginFragmentProvider + fallbackToWeb / embedded_web），
#   包内**不存在**旧版 com.taptap.sdk.ui.TapTapActivity ⇒ 不注入 yntt_smali / YnttLoginRequest。
#   改签后 TapTap 客户端 SSO 会以 signature not match 拒绝 → 必须让星光 dex 里的
#   TapLoginForceWeb 对 com.muyegame.blfx 生效（其 PKG_WHITELIST 必须含本包名）。
#   本脚本对该白名单做**硬门禁**（见 [2/6] 的 whitelist gate），可用
#   BLFX_SKIP_LOGIN_WHITELIST_GATE=1 显式跳过（跳过=接受登录可能失败，须在装机报告写明）。
#
# 产物：08_output/爆裂防线/爆裂防线_1.01.06_surgical-unsigned.apk（未签名）
#       07_patched/爆裂防线/surgical/{AndroidManifest.xml,classes3.dex}（留证 / 门禁用）
#
# 用法: bash 04_scripts/爆裂防线/surgical_blfx_1.01.06.sh
#   BLFX_BASE_APK=/path/to/original.apk 可覆盖底包路径
#   BLFX_SO=/path/to/libstarry_magic.so 可覆盖 so 路径
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="爆裂防线"
VERSION="1.01.06"
PKG="com.muyegame.blfx"
ORIG_APK="${BLFX_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_original_from_device.apk}"
ORIG_MD5="${BLFX_BASE_MD5:-1a57be4509b55582f4aae5475aeaf6c6}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
YNTT_SMALI="$STARRY_SRC/yntt_smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
AUTHORITY="$PKG.starryinit"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
LOGIN_CARRIER="$STARRY_SRC/app/src/main/java/com/starry/magic/sign/TapLoginForceWeb.java"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}（用 BLFX_BASE_APK=... 指定底包）" >&2; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[[ -d "$STARRY_SRC" ]] || { echo "ERROR: missing $STARRY_SRC" >&2; exit 1; }
SO_PREBUILT="${BLFX_SO:-$STARRY_SRC/libs_blfx/arm64-v8a/libstarry_magic.so}"
[[ -f "$SO_PREBUILT" ]] \
  || { echo "ERROR: missing modern so ${SO_PREBUILT}（先跑 04_scripts/爆裂防线/blfx_build_so.sh）" >&2; exit 1; }
echo "[check] modern so = $SO_PREBUILT ($(md5 -q "$SO_PREBUILT"))"
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 底包指纹不符 ${ACTUAL_MD5}（期望 ${ORIG_MD5}）" >&2; exit 1; }
echo "[check] 底包 = $ORIG_APK ($ACTUAL_MD5)"

# ---- TapSDK 路径探测：决定是否需要并入旧版 yntt_smali ------------------------------
# 旧版（TapSDK 3.30.x）= com/taptap/sdk/ui/TapTapActivity + Intent extra "request"
#                       → 必须并入 YnttLoginRequest（yntt_smali）
# 新版（TapSDK 4.x）    = com/taptap/sdk/login/impl/ui/activity/TapTapLoginActivity
#                       → 走 TapLoginForceWeb 反射，不需要 yntt_smali
TAP_PATH="$(python3 - "$ORIG_APK" <<'PY'
import sys, zipfile
OLD = b'com/taptap/sdk/ui/TapTapActivity'
NEW = b'com/taptap/sdk/login/impl/ui/activity/TapTapLoginActivity'
blob = b''
with zipfile.ZipFile(sys.argv[1]) as z:
    for n in z.namelist():
        if n.startswith('classes') and n.endswith('.dex'):
            blob += z.read(n)
if OLD in blob:
    print('old')
elif NEW in blob:
    print('new')
else:
    print('none')
PY
)"
echo "[check] TapSDK 路径 = $TAP_PATH"
case "$TAP_PATH" in
  old)
    [[ -d "$YNTT_SMALI/com/starry/magic/sign" ]] \
      || { echo "ERROR: missing ${YNTT_SMALI}（旧版 TapTap 路径必需）" >&2; exit 1; }
    NEED_YNTT=1 ;;
  new)
    NEED_YNTT=0
    if [[ "${BLFX_SKIP_LOGIN_WHITELIST_GATE:-0}" != "1" ]]; then
      if [[ ! -f "$LOGIN_CARRIER" ]]; then
        echo "ERROR: missing ${LOGIN_CARRIER}（无法核对 TapTap 新路径白名单）" >&2; exit 1
      fi
      if ! grep -qF "\"$PKG\"" "$LOGIN_CARRIER"; then
        cat >&2 <<EOM
ERROR: TapTap 新路径登录白名单缺本包名（登录会在改签后被 SSO 以 signature not match 拒绝）。

需要把 "$PKG" 加入：
  $LOGIN_CARRIER
  private static final java.util.Set<String> PKG_WHITELIST = new java.util.HashSet<>(...)

这是共享文件（多游戏共用），本脚本不代改；由会话负责人决定后加一行，例如：
  "com.muyegame.blfx"));   // 爆裂防线：TapSDK 新版路径，改签包固定走 embedded_web

确认「本轮不需要 TapTap 登录修复」时可显式跳过：
  BLFX_SKIP_LOGIN_WHITELIST_GATE=1 bash 04_scripts/爆裂防线/surgical_blfx_1.01.06.sh
EOM
        exit 1
      fi
      echo "[check] TapTap 登录白名单含 $PKG ✓"
    else
      echo "[check] WARN: 已跳过 TapTap 登录白名单门禁（登录可能失败，须在装机报告写明）"
    fi ;;
  none)
    echo "WARN: 底包未检出 TapTap 登录入口（既不旧也不新）——本作可能未启用 TapTap 登录" >&2
    NEED_YNTT=0 ;;
esac

# dex 落位名：★必须是 base.apk 里「最小的空闲 classesN.dex 下标」。
# ART 的 BaseDexClassLoader 从 classes.dex 起顺序扫描 classes2/3/…，遇到第一个缺口就停；
# 本作底包是 classes.dex + classes2.dex ⇒ 星光 dex 必须叫 classes3.dex。
# 若错写成 classes8.dex（底包无 classes3..7），classes8 永远不会被加载 →
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

LOCK_DIR="/tmp/blfx_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held" >&2; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/blfx_surgical.XXXXXX)"
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

echo "[2/6] apktool d starry → com/starry smali → $DEX_NAME"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
# 旧版 TapTap 路径才需要 YnttLoginRequest（本作实测为新版 ⇒ 通常不并入）
if [[ "${NEED_YNTT:-0}" == "1" ]]; then
  rsync -a "$YNTT_SMALI/com/" "$SMALI_ROOT/com/"
  echo "  merged yntt_smali（旧版 TapTap 路径）"
fi
SMALI_NEED=(
  "com/starry/magic/StarryInitProvider.smali"
  "com/starry/magic/menu/MenuService.smali"
  "com/starry/magic/sign/SignSpoof.smali"
  "com/starry/magic/sign/SignSpoofAppComponentFactory.smali"
  "com/starry/magic/sign/TapLoginForceWeb.smali"
)
if [[ "${NEED_YNTT:-0}" == "1" ]]; then
  SMALI_NEED+=(
    "com/starry/magic/sign/YnttLoginRequest.smali"
    "com/starry/magic/sign/YnttLoginRequest\$1.smali"
  )
fi
for need in "${SMALI_NEED[@]}"; do
  [[ -f "$SMALI_ROOT/$need" ]] || { echo "ERROR: $need missing（Java 桥没编进 starry app？）" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/$DEX_NAME" "$SMALI_ROOT"
ls -la "$WORK/$DEX_NAME"
if [[ "${NEED_YNTT:-0}" == "1" ]]; then
  LC_ALL=C grep -aqF "com/taptap/sdk/LoginRequest" "$WORK/$DEX_NAME" \
    || { echo "ERROR: $DEX_NAME 未含 YnttLoginRequest（super=LoginRequest 缺失）" >&2; exit 1; }
fi

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

STAGE_DIR="$NIXIANG/07_patched/$GAME/surgical"
mkdir -p "$STAGE_DIR"
cp -f "$WORK/$DEX_NAME" "$STAGE_DIR/$DEX_NAME"
echo "  $DEX_NAME → $STAGE_DIR/$DEX_NAME ($(md5 -q "$STAGE_DIR/$DEX_NAME"))"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] || { echo "ERROR: missing libc++_shared.so at $NDK_CXX" >&2; exit 1; }
cp "$NDK_CXX" "$WORK/lib/arm64-v8a/"

echo "[3/6] binary AndroidManifest（默认 Application；换 appComponentFactory + provider/service/权限）"
if [[ -n "${BLFX_BINARY_MANIFEST:-}" && -f "${BLFX_BINARY_MANIFEST:-}" ]]; then
  cp "$BLFX_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $BLFX_BINARY_MANIFEST"
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
new_factory = 'android:appComponentFactory="com.starry.magic.sign.SignSpoofAppComponentFactory"'
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
  grep -q "com.starry.magic.sign.SignSpoofAppComponentFactory" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 未替换 appComponentFactory" >&2; exit 1; }
  # 本作官方 launcher activity = com.unity3d.player.MyMainActivity（团结引擎，非 PrivacyActivity）
  grep -q "com.unity3d.player.MyMainActivity" "$WORK/tree_new.txt" \
    || { echo "ERROR: 新 manifest 丢了官方 launcher activity（MyMainActivity）" >&2; exit 1; }
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
    for n in ('classes.dex', 'classes2.dex', 'resources.arsc',
              'lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libunity.so',
              'lib/arm64-v8a/libtuanjie.so',
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
