#!/usr/bin/env bash
# 蛋壳战机 国服 1.2.1 (vc44, taptap) — 星光外科组装。
#
# 原包游戏本体不变：classes.dex..classes4.dex / resources.arsc / assets/** /
# 原 lib so 逐字节保留。仅：
#   - AndroidManifest.xml：appComponentFactory -> StarryAppComponentFactory，
#     增加 StarryInitProvider、MenuService、前台服务权限
#   - 新增 classes5.dex（星光 Java/smali，来自 menu APK 的 com/starry）
#   - 新增 libstarry_magic.so + libc++_shared.so
#
# 与 surgical_dkzj_1.1.8.sh 的差异：底包换 1.2.1(vc44)；so 来自 libs_dkzj121；
# 中间物落 07_patched/蛋壳战机/surgical_1.2.1_cn；md5/rsync 改为跨机可用写法。
#
# 用法：
#   STARRY_MENU_APK=<menu app-release.apk> bash 04_scripts/蛋壳战机/surgical_dkzj_1.2.1_cn.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG="$(cd "$SCRIPT_DIR/../.." && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="蛋壳战机"
VERSION="1.2.1"
PKG="com.survivor.acecn"
ORIG_APK="${DKZJ_BASE_APK:-$NIXIANG/02_apks/$GAME/蛋壳战机_1.2.1_official_vc44_from_emulator.apk}"
ORIG_MD5="${DKZJ_BASE_MD5:-075b9d6a84ad87679301eb94246f8b92}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_starry-unsigned.apk"
STAGE_DIR="$NIXIANG/07_patched/$GAME/surgical_1.2.1_cn"
MF_CACHE="$STAGE_DIR/AndroidManifest.xml"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
APKTOOL="$NIXIANG/01_tools/apktool/apktool"
AUTHORITY="$PKG.starryinit"

md5_of() { if md5 -q /dev/null >/dev/null 2>&1; then md5 -q "$1"; else md5sum "$1" | cut -d' ' -f1; fi; }
PY="$(command -v python3 || command -v python)"

# aapt2：mac 在 ~/Library/Android/sdk，Windows 在 %LOCALAPPDATA%\Android\Sdk
if [ -z "${AAPT2:-}" ]; then
  for c in "$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2" \
           "${LOCALAPPDATA:-}/Android/Sdk/build-tools/36.1.0/aapt2.exe" \
           "/c/Users/Administrator/AppData/Local/Android/Sdk/build-tools/36.1.0/aapt2.exe" \
           "${LOCALAPPDATA:-}/Android/Sdk/build-tools/35.0.0/aapt2.exe"; do
    if [ -x "$c" ]; then AAPT2="$c"; break; fi
  done
fi

# NDK 的 libc++_shared.so（APP_STL=c++_shared ⇒ 必须随包）
NDK="${ANDROID_NDK_HOME:-}"
if [ -z "$NDK" ]; then
  for c in "$HOME/Library/Android/sdk/ndk/27.0.12077973" \
           "${LOCALAPPDATA:-}/Android/Sdk/ndk/27.0.12077973" \
           "/c/Users/Administrator/AppData/Local/Android/Sdk/ndk/27.0.12077973"; do
    if [ -d "$c" ]; then NDK="$c"; break; fi
  done
fi
NDK_CXX=""
if [ -n "$NDK" ]; then
  for c in "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so" \
           "$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-gnu/libc++_shared.so" \
           "$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so" \
           "$NDK/toolchains/llvm/prebuilt/windows-x86_64/sysroot/usr/lib/aarch64-linux-gnu/libc++_shared.so" \
           "$NDK/toolchains/llvm/prebuilt/linux-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so" \
           "$NDK/toolchains/llvm/prebuilt/linux-x86_64/sysroot/usr/lib/aarch64-linux-gnu/libc++_shared.so"; do
    if [ -f "$c" ]; then NDK_CXX="$c"; break; fi
  done
fi
[ -n "$NDK_CXX" ] || echo "WARN: libc++_shared.so 未找到（NDK=${NDK}）——若包内缺该 so 会在启动期 UnsatisfiedLinkError" >&2

[ -f "$ORIG_APK" ] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[ -x "$SMALI_BIN" ] || { echo "ERROR: missing $SMALI_BIN" >&2; exit 1; }
[ -f "$APKTOOL" ] || { echo "ERROR: missing $APKTOOL" >&2; exit 1; }
SO_PREBUILT="${DKZJ_SO:-$STARRY_SRC/libs_dkzj121/arm64-v8a/libstarry_magic.so}"
[ -f "$SO_PREBUILT" ] || { echo "ERROR: missing ${SO_PREBUILT}（先跑 dkzj_build_so_1.2.1_cn.sh）" >&2; exit 1; }
STARRY_MENU_APK="${STARRY_MENU_APK:-$STARRY_SRC/app/build/outputs/apk/release/app-release.apk}"
[ -f "$STARRY_MENU_APK" ] || { echo "ERROR: missing menu APK $STARRY_MENU_APK" >&2; exit 1; }
# TapTap「改包法」（用户 2026-10-28 定案，取代运行时签名伪装）：修补后的 classes4.dex
PATCHED_DEX4="${DKZJ_PATCHED_DEX4:-$NIXIANG/07_patched/$GAME/tapsdk_patch/classes4.dex}"
[ -f "$PATCHED_DEX4" ] || { echo "ERROR: missing ${PATCHED_DEX4}（先跑 dkzj_tapsdk_dexpatch_1.2.1_cn.py）" >&2; exit 1; }

echo "[check] base  = $ORIG_APK"
ACTUAL_MD5="$(md5_of "$ORIG_APK")"
[ "$ACTUAL_MD5" = "$ORIG_MD5" ] \
  || { echo "ERROR: 原包指纹不符 ${ACTUAL_MD5}（期望 ${ORIG_MD5}）" >&2; exit 1; }
echo "[check] so    = $SO_PREBUILT ($(md5_of "$SO_PREBUILT"))"
echo "[check] menu  = $STARRY_MENU_APK ($(md5_of "$STARRY_MENU_APK"))"

WORK="$(mktemp -d "${TMPDIR:-/tmp}/dkzj_surgical_cn.XXXXXX")"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$WORK" "$STAGE_DIR"

echo "[1/6] menu APK → com/starry smali → classes5.dex"
"$APKTOOL" d -f -o "$WORK/menu_dec" "$STARRY_MENU_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
for d in "$WORK/menu_dec"/smali "$WORK/menu_dec"/smali_classes*; do
  [ -d "$d/com/starry" ] || continue
  mkdir -p "$SMALI_ROOT/com"
  cp -a "$d/com/starry" "$SMALI_ROOT/com/"
done
for need in \
  "com/starry/magic/StarryInitProvider.smali" \
  "com/starry/magic/menu/MenuService.smali" \
  "com/starry/magic/sign/StarryAppComponentFactory.smali" \
  "com/starry/magic/sign/TapLoginForceWeb.smali"; do
  [ -f "$SMALI_ROOT/$need" ] || { echo "ERROR: $need missing" >&2; exit 1; }
done

# 【改包法】运行时签名伪装已于 2026-10-28 从源码整体删除（SignSpoof 类不再存在）。
# 本段保留为"兜底断言"：万一 smali 树里又出现 SignSpoof.install() 生效调用（例如回滚到旧 Java），
# 就地注释掉并告警；0 处是正常状态。
"$PY" - "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" <<'PY'
import sys
from pathlib import Path
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
needle = "Lcom/starry/magic/sign/SignSpoof;->install()V"
n = t.count(needle)
if n == 0:
    print("  StarryInitProvider 无 SignSpoof.install()（源码已删，正常）")
    sys.exit(0)
if n > 1:
    raise SystemExit(f"StarryInitProvider 里 SignSpoof.install() 出现 {n} 处（异常，人工核对）")
out = []
for line in t.splitlines():
    if needle in line and line.lstrip().startswith("invoke-static"):
        out.append("    # 【改包法】运行时签名伪装已删除（用户 2026-10-28 定案）：" + line.strip())
        continue
    out.append(line)
p.write_text("\n".join(out) + "\n", encoding="utf-8")
print("  ⚠ 检测到遗留 SignSpoof.install() 调用，已就地注释（源码应为新版）")
PY
if LC_ALL=C grep -aqE '^[[:space:]]*invoke-static.*SignSpoof;->install\(\)V' "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali"; then
  echo "ERROR: SignSpoof.install() 仍在生效位置（应只剩注释）" >&2
  exit 1
fi
echo "  校验：StarryInitProvider 内已无生效的 SignSpoof.install() 调用"

"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
cp -f "$WORK/classes5.dex" "$STAGE_DIR/classes5.dex"
echo "  classes5.dex → $STAGE_DIR/classes5.dex ($(md5_of "$STAGE_DIR/classes5.dex"))"

# Java / jni 注册面交叉校验
MB_JAVA="$STARRY_SRC/app/src/main/java/com/starry/magic/bridge/ModBridge.java"
[ -f "$MB_JAVA" ] || { echo "ERROR: missing $MB_JAVA" >&2; exit 1; }
NATIVE_TOTAL=0
NATIVE_MISS=0
for m in $(LC_ALL=C grep -oE '\bnative[A-Za-z0-9_]+' "$MB_JAVA" | sort -u); do
  NATIVE_TOTAL=$((NATIVE_TOTAL + 1))
  LC_ALL=C grep -aqF "$m" "$WORK/classes5.dex" || {
    echo "ERROR: classes5.dex 缺 native 声明 $m" >&2
    NATIVE_MISS=$((NATIVE_MISS + 1))
  }
done
[ "$NATIVE_MISS" -eq 0 ] \
  || { echo "ERROR: classes5.dex 与 ModBridge.java 声明不一致（$NATIVE_MISS/$NATIVE_TOTAL 缺失）" >&2; exit 1; }
echo "  dex↔ModBridge native 声明校验通过（$NATIVE_TOTAL 项）"

# 证书/包名映射（SignSpoof）
LC_ALL=C grep -aq "com.survivor.acecn" "$WORK/classes5.dex" \
  || { echo "ERROR: classes5.dex 缺本包名签名映射" >&2; exit 1; }
LC_ALL=C grep -aq "MIICojCCAYqgAwIBAgIEbs5L8zAN" "$WORK/classes5.dex" \
  || { echo "ERROR: classes5.dex 缺 gorilla 官方证书" >&2; exit 1; }
echo "  SignSpoof 包名 + gorilla 证书串命中"

echo "[2/6] binary AndroidManifest"
if [ -n "${DKZJ_BINARY_MANIFEST:-}" ] && [ -f "${DKZJ_BINARY_MANIFEST:-}" ]; then
  cp "$DKZJ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using $DKZJ_BINARY_MANIFEST"
elif [ -f "$MF_CACHE" ]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once（apktool d base → 文本改写 → apktool b）"
  "$APKTOOL" d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  "$PY" - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" "$AUTHORITY" <<'PY'
from pathlib import Path
import sys

p = Path(sys.argv[1])
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

# 【改包法 2026-10-28】不再把 appComponentFactory 换成 StarryAppComponentFactory：
# 该类自身的 instantiateApplication 会调 SignSpoof.install()（实测小米上 StarrySignSpoof 66 行、exemptHiddenApis 失败）。
# 保留官方 androidx.core.app.CoreComponentFactory，注入入口只用 StarryInitProvider（其 install() 调用已在 dex 级注释）。
old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
if old_factory not in t:
    raise SystemExit("expected official CoreComponentFactory to be preserved")

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
print("  text Manifest patched (Application preserved)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  "$APKTOOL" b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  "$PY" -c "import sys,zipfile;z=zipfile.ZipFile(sys.argv[1]);open(sys.argv[2],'wb').write(z.read('AndroidManifest.xml'))" \
    "$WORK/mf_only.apk" "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[3/6] binary Manifest 回读校验"
if [ -n "${AAPT2:-}" ] && [ -x "${AAPT2:-}" ]; then
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$ORIG_APK" > "$WORK/tree_orig.txt"
  "$PY" - "$WORK/AndroidManifest.xml" "$WORK/mf_check.apk" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[2], "w")
i = zipfile.ZipInfo("AndroidManifest.xml")
i.compress_type = zipfile.ZIP_DEFLATED
z.writestr(i, open(sys.argv[1], "rb").read())
z.close()
PY
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null \
    || { echo "ERROR: aapt2 无法解析新 manifest" >&2; exit 1; }
  for need in \
    "com.starry.magic.StarryInitProvider" \
    "com.starry.magic.menu.MenuService" \
    "androidx.core.app.CoreComponentFactory" \
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"; do
    grep -q "$need" "$WORK/tree_new.txt" \
      || { echo "ERROR: 新 manifest 缺 $need" >&2; exit 1; }
  done
  LC_ALL=C grep -q "com.starry.magic.sign.StarryAppComponentFactory" "$WORK/tree_new.txt" && {
    echo "ERROR: manifest 仍挂着 StarryAppComponentFactory（改包法要求摘掉）" >&2; exit 1; }
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_orig.txt" > "$WORK/o.norm"
  sed -E 's/\(line=[0-9]+\)//' "$WORK/tree_new.txt" > "$WORK/n.norm"
  "$PY" - "$WORK/o.norm" "$WORK/n.norm" <<'PY'
import sys
from collections import Counter
from pathlib import Path

def canon(path):
    stack, out = [], []
    for ln in Path(path).read_text(encoding='utf-8', errors='replace').splitlines():
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
unexpected = {
    k: v for k, v in dropped.items()
    if 'androidx.core.app.CoreComponentFactory' not in k
    and not ('/intent-filter/data|' in k)
}
if unexpected:
    for k, v in list(unexpected.items())[:10]:
        print(f'  LOST x{v}: {k}', file=sys.stderr)
    raise SystemExit(f'manifest 重建立即失败：丢失官方节点（{sum(unexpected.values())} 项）')
if not (18 <= added <= 22):
    raise SystemExit(f'manifest 新增节点数异常（{added} 项）')
print(f'  manifest ok (新增 {added} 项 / 只丢弃旧 factory)')
PY
else
  echo "  WARN: aapt2 未找到，跳过 manifest 回读校验" >&2
fi

echo "[4/6] surgical zip merge（TapTap 改包法：只换 classes4.dex + Manifest + 星光 dex/so，其余字节不变）"
"$PY" - "$ORIG_APK" "$WORK" "$UNSIGNED" "$SO_PREBUILT" "${NDK_CXX:-}" "$PATCHED_DEX4" <<'PY'
import hashlib, sys, zipfile
from pathlib import Path

src = Path(sys.argv[1])
work = Path(sys.argv[2])
dst = Path(sys.argv[3])
so = Path(sys.argv[4]).read_bytes()
cxx_path = Path(sys.argv[5]) if len(sys.argv) > 5 and sys.argv[5] else None
cxx = cxx_path.read_bytes() if cxx_path and cxx_path.is_file() else b""
patched4 = Path(sys.argv[6]).read_bytes() if len(sys.argv) > 6 and sys.argv[6] else b""
manifest = (work / 'AndroidManifest.xml').read_bytes()
classes5 = (work / 'classes5.dex').read_bytes()

replace = {'AndroidManifest.xml': manifest}
if patched4:
    replace['classes4.dex'] = patched4
extras = {
    'classes5.dex': classes5,
    'lib/arm64-v8a/libstarry_magic.so': so,
}
if cxx:
    extras['lib/arm64-v8a/libc++_shared.so'] = cxx

GUARD = {
    'classes.dex', 'classes2.dex', 'classes3.dex',
    'resources.arsc', 'lib/arm64-v8a/libil2cpp.so',
    'lib/arm64-v8a/libtapsdkcore.so', 'lib/arm64-v8a/libunity.so',
    'lib/arm64-v8a/libmain.so',
    'assets/bin/Data/Managed/Metadata/global-metadata.dat',
}
if not patched4:
    GUARD.add('classes4.dex')

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    if 'classes5.dex' in original_names:
        raise SystemExit('base 已含 classes5.dex —— dex 落位下标需重算')
    for item in zin.infolist():
        name = item.filename
        if name.startswith('META-INF/') and (name == 'META-INF/MANIFEST.MF'
                                             or name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
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
        if n.startswith('META-INF/') and (n == 'META-INF/MANIFEST.MF'
                                          or n.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if n in replace or n in extras:
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        changed += 1
    for n in GUARD:
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'protected entry changed: {n}')
    if 'classes5.dex' not in out_names:
        raise SystemExit('classes5.dex missing')
    pkg_so = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(pkg_so).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'surgical zip verified: {changed} 个原始条目字节不变 + classes5.dex + starry so')
PY

echo "[5/6] surgical done"
echo "[surgical] unsigned = $UNSIGNED"
echo "[surgical] md5 = $(md5_of "$UNSIGNED")"
ls -lh "$UNSIGNED"
