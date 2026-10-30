#!/usr/bin/env bash
# 宝石战争 6.0.013 外科组装：官方 dex/arsc/IL2CPP/第三方 so 字节保留，
# 只新增星光 dex/so，完整替换 HotUpdate 资源，并仅覆盖注入后的 System.Network.dll.bytes。
# 6.0.013 与 6.0.012 的差别：热更版本 1.0.0.01 -> 1.0.0.04（官方 Network 声明 hash 变为
# 3e49c9876a6c4b394c552f71adbebf9d），底包换为 6.0.013 官方 base APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/../../mod菜单/starry-mod-menu" && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
GAME="宝石战争"
VERSION="6.0.013"
PKG="com.acing.gow.adv"
ORIG_APK="${BSZZ_SOURCE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_official_from_device.apk}"
[[ "$ORIG_APK" = /* ]] || ORIG_APK="$NIXIANG/$ORIG_APK"
ORIG_MD5="${BSZZ_SOURCE_APK_MD5:-cca417cc6bb0e0f19574fbbbd7e51bfe}"
EXPECT_HOTUPDATE_VERSION="${BSZZ_EXPECT_HOTUPDATE_VERSION:-1.0.0.04}"
EXPECT_NETWORK_HASH="${BSZZ_EXPECT_NETWORK_HASH:-3e49c9876a6c4b394c552f71adbebf9d}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
PATCHED_ROOT="$NIXIANG/07_patched/$GAME/$VERSION"
PATCHED_ASSETS="$PATCHED_ROOT/assets/Android"
FULL_HOTUPDATE_ROOT="${BSZZ_HOTUPDATE_FULL_ROOT:-$PATCHED_ROOT/official_hotupdate}"
SO_PREBUILT="${BSZZ_SO:-$STARRY_SRC/libs_bszz/arm64-v8a/libstarry_magic.so}"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical-$VERSION/AndroidManifest.xml"
# TapTap 验签（双轨制 B 轨，2026-10-29 起现行）：MT ApkSignatureKillerEx 形态
# bin.mt.signature.KillerApplication193 打成 classes4.dex，由 manifest 的
# appComponentFactory=com.starry.magic.sign.StarryAppComponentFactory 在
# instantiateApplication（早于真实 Application）触发其 <clinit>，换 PackageInfo.CREATOR
# 并清 Parcel/PackageManager 缓存 ⇒ TapSDK 读到官方证书。官方 dex 一个字节不改。
KILLER_SMALI="${BSZZ_KILLER_SMALI:-$PATCHED_ROOT/surgical/killer_smali}"
OFFICIAL_CERT_DER="${BSZZ_OFFICIAL_CERT_DER:-$PATCHED_ROOT/surgical/official.der}"
EXPECT_CERT_SHA256="${BSZZ_EXPECT_CERT_SHA256:-8943b31609712c8c7fdf583ce26b204ad8b01dad8eb09f4c7d1f276458985bb4}"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK" >&2; exit 1; }
[[ "$(md5 -q "$ORIG_APK")" == "$ORIG_MD5" ]] || { echo "ERROR: 底包指纹不符" >&2; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing" >&2; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing $SO_PREBUILT" >&2; exit 1; }
for f in Dlls/System.Network.dll.bytes manifest.json RuntimeAssembliesReceipt.json; do
  [[ -f "$PATCHED_ASSETS/$f" ]] || { echo "ERROR: missing patched asset $f" >&2; exit 1; }
done
[[ -f "$FULL_HOTUPDATE_ROOT/Android/manifest.json" ]] || {
  echo "ERROR: missing full HotUpdate manifest: $FULL_HOTUPDATE_ROOT/Android/manifest.json" >&2
  exit 1
}
[[ -f "$FULL_HOTUPDATE_ROOT/Android/RuntimeAssembliesReceipt.json" ]] || {
  echo "ERROR: missing full HotUpdate receipt" >&2
  exit 1
}

# 注入 IL 的 P/Invoke 入口名必须与 so 导出符号一致，否则运行期 dlsym 失败抛
# EntryPointNotFoundException，战斗协程被打断（2026-09-16 宝石战争回合结束卡死实锤）。
PATCH_SIDECAR="$PATCHED_ROOT/System.Network.patched.pinvoke.json"
[[ -f "$PATCH_SIDECAR" ]] || { echo "ERROR: missing ${PATCH_SIDECAR}（patch_dll 未产出入口名清单）" >&2; exit 1; }
NM_BIN="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-nm"
[[ -x "$NM_BIN" ]] || { echo "ERROR: llvm-nm missing" >&2; exit 1; }
while read -r sym; do
  [[ -n "$sym" ]] || continue
  if ! "$NM_BIN" -D --defined-only "$SO_PREBUILT" | grep -q " $sym$"; then
    echo "ERROR: 注入 IL 声明了 P/Invoke 入口名 ${sym}，但 $SO_PREBUILT 没有该导出符号（dlsym 必失败）" >&2
    exit 1
  fi
done < <(python3 -c 'import json,sys; print("\n".join(json.load(open(sys.argv[1]))["entry_points"]))' "$PATCH_SIDECAR")
echo "[preflight] P/Invoke 入口名与 so 导出符号一致: $(python3 -c 'import json,sys; print(len(json.load(open(sys.argv[1]))["entry_points"]))' "$PATCH_SIDECAR") 项"

LOCK_DIR="/tmp/bszz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then echo "ERROR: lock $LOCK_DIR held" >&2; exit 1; fi
echo $$ > "$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/bszz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease (starry -> dex)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew assembleRelease --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK" >&2; exit 1; }

echo "[2/6] starry com/starry -> classes3.dex"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
for cls in StarryInitProvider MenuService ModBridge StarryAppComponentFactory TapLoginForceWeb; do
  find "$SMALI_ROOT/com" -type f -name "$cls.smali" -print -quit | grep -q . \
    || { echo "ERROR: starry class missing: $cls" >&2; exit 1; }
done
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$SMALI_ROOT"

echo "[2b/6] MT killer -> classes4.dex（TapTap 验签 B 轨：运行时签名替换）"
[[ -d "$KILLER_SMALI" ]] || { echo "ERROR: 缺 killer smali 源目录 $KILLER_SMALI" >&2; exit 1; }
[[ -f "$OFFICIAL_CERT_DER" ]] || { echo "ERROR: 缺官方证书 DER $OFFICIAL_CERT_DER" >&2; exit 1; }
CERT_SHA256="$(shasum -a 256 "$OFFICIAL_CERT_DER" | awk '{print $1}')"
[[ "$CERT_SHA256" == "$EXPECT_CERT_SHA256" ]] || {
  echo "ERROR: 官方证书 DER 指纹不符（实际 ${CERT_SHA256}，期望 ${EXPECT_CERT_SHA256}）" >&2
  echo "       killer 喂错证书会静默不生效，拒绝打包" >&2
  exit 1
}
rm -rf "$WORK/killer_smali" "$WORK/c4_check"
cp -r "$KILLER_SMALI" "$WORK/killer_smali"
rm -f "$WORK/killer_smali/bin/mt/signature/SgKillerApp.smali"
python3 - "$WORK/killer_smali" "$OFFICIAL_CERT_DER" "$PKG" <<'PY'
import base64, io, os, re, sys
root, der_path, pkg = sys.argv[1], sys.argv[2], sys.argv[3]
b64 = base64.b64encode(open(der_path, 'rb').read()).decode()
wrapped = '\\n'.join(b64[i:i + 76] for i in range(0, len(b64), 76))
p = os.path.join(root, 'bin/mt/signature/KillerApplication193.smali')
t = io.open(p, encoding='utf-8').read()
assert '.super Landroid/app/Application;' in t, 'killer 父类不是 android.app.Application（先规范化 killer_smali）'
t, n = re.subn(r'const-string v1, "[^"]*"', 'const-string v1, "%s"' % pkg, t, count=1)
assert n == 1, 'killer 包名锚点未命中'
j = t.find('const-string v0, "MI')
assert j >= 0, 'killer 证书锚点未命中'
k = t.find('"', j + len('const-string v0, '))
k2 = t.find('"', k + 1)
t = t[:j] + 'const-string v0, "%s"' % wrapped + t[k2 + 1:]
io.open(p, 'w', encoding='utf-8', newline='\n').write(t)
print('  killer: 包名=%s，证书 base64 %d 字符' % (pkg, len(b64)))
PY
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$WORK/killer_smali"
"$BAKSMALI_BIN" d -o "$WORK/c4_check" "$WORK/classes4.dex" >/dev/null
K4="$WORK/c4_check/bin/mt/signature/KillerApplication193.smali"
[[ -f "$K4" ]] || { echo "ERROR: 回读失败：classes4.dex 缺 KillerApplication193" >&2; exit 1; }
grep -q '^.super Landroid/app/Application;' "$K4" || { echo "ERROR: 回读失败：killer 父类异常" >&2; exit 1; }
grep -q "const-string v1, \"${PKG}\"" "$K4" || { echo "ERROR: 回读失败：killer 包名未改" >&2; exit 1; }
python3 - "$WORK/classes4.dex" "$OFFICIAL_CERT_DER" <<'PY'
import base64, sys
dex = open(sys.argv[1], 'rb').read()
b64 = base64.b64encode(open(sys.argv[2], 'rb').read()).decode()
head = b64[:48].encode()
assert head in dex, 'classes4.dex 缺官方证书 base64（喂错证书 killer 静默失效）'
print('  classes4.dex killer 证书锚点 ✓（%d B）' % len(dex))
PY
rm -rf "$WORK/c4_check"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"

echo "[3/6] binary AndroidManifest（保留 U8Application）"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
else
  apktool d -f -s --no-assets -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3 - "$WORK/orig_dec/AndroidManifest.xml" "$PKG" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
pkg = sys.argv[2]
t = p.read_text(encoding="utf-8")
perms = []
for perm in ("SYSTEM_ALERT_WINDOW", "FOREGROUND_SERVICE", "FOREGROUND_SERVICE_SPECIAL_USE"):
    if f"android.permission.{perm}" not in t:
        perms.append(f'    <uses-permission android:name="android.permission.{perm}"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("no uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

# TapTap 验签 B 轨挂载点：appComponentFactory 必须在真实 Application 之前跑起来
# （底包无该属性 ⇒ 直接补；U8Application 与既有属性一字不动）。
factory = "com.starry.magic.sign.StarryAppComponentFactory"
if 'android:appComponentFactory="' in t:
    raise SystemExit("底包已有 appComponentFactory，先人工确认挂载点再改本脚本")
i = t.find("<application")
if i < 0:
    raise SystemExit("no <application>")
j = i + len("<application")
t = t[:j] + ' android:appComponentFactory="%s"' % factory + t[j:]
if "com.starry.magic.StarryInitProvider" not in t:
    provider = f'''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="{pkg}.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace("</application>", provider + "\n    </application>", 1)
if "com.starry.magic.menu.MenuService" not in t:
    service = '''
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
    t = t.replace("</application>", service + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
fi

echo "[4/6] manifest 回读校验"
AAPT2="${AAPT2:-$HOME/Library/Android/sdk/build-tools/36.0.0/aapt2}"
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
  "$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/mf_check.apk" > "$WORK/tree_new.txt" 2>/dev/null
  grep -q 'com.u8.sdk.U8Application' "$WORK/tree_new.txt" || { echo "ERROR: lost U8Application" >&2; exit 1; }
  grep -q 'com.starry.magic.sign.StarryAppComponentFactory' "$WORK/tree_new.txt" || { echo "ERROR: appComponentFactory 未挂上（TapTap 验签 B 轨会失效）" >&2; exit 1; }
  grep -q 'com.starry.magic.StarryInitProvider' "$WORK/tree_new.txt" || { echo "ERROR: provider missing" >&2; exit 1; }
  grep -q 'com.starry.magic.menu.MenuService' "$WORK/tree_new.txt" || { echo "ERROR: service missing" >&2; exit 1; }
fi

echo "[5/6] zip surgical merge -> $UNSIGNED"
python3 - "$ORIG_APK" "$WORK" "$PATCHED_ROOT" "$FULL_HOTUPDATE_ROOT" "$UNSIGNED" <<'PY'
import hashlib, json, sys, zipfile
from pathlib import Path
src, work, patched_root, full_hotupdate_root, dst = map(Path, sys.argv[1:6])
mf = (work / "AndroidManifest.xml").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
c4 = (work / "classes4.dex").read_bytes()
official_manifest_path = full_hotupdate_root / "Android/manifest.json"
official_manifest_bytes = official_manifest_path.read_bytes()
official_manifest = json.loads(official_manifest_bytes)
asset_overrides = {}
for asset in official_manifest["files"]:
    source = full_hotupdate_root / asset["path"]
    assert source.is_file(), f"missing HotUpdate asset: {source}"
    data = source.read_bytes()
    assert len(data) == asset["size"], f"size mismatch: {asset['path']}"
    assert hashlib.md5(data).hexdigest() == asset["hash"], f"md5 mismatch: {asset['path']}"
    asset_overrides["assets/" + asset["path"]] = data
patched_network_key = "assets/Android/Dlls/System.Network.dll.bytes"
patched_network_bytes = (patched_root / patched_network_key).read_bytes()
asset_overrides[patched_network_key] = patched_network_bytes
asset_overrides["assets/Android/manifest.json"] = official_manifest_bytes
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
SIG_NAMES = {"META-INF/MANIFEST.MF"}
untouched = [
    "classes.dex", "classes2.dex", "resources.arsc",
    "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
    "lib/arm64-v8a/libmain.so", "lib/arm64-v8a/libthemis.so",
    "assets/bin/Data/Managed/Metadata/global-metadata.dat",
]
zin = zipfile.ZipFile(src, "r")
names = set(zin.namelist())
for n in untouched:
    assert n in names, f"orig apk missing {n}"

def is_sig(name):
    if name in SIG_NAMES:
        return True
    return name.startswith("META-INF/") and name.upper().endswith((".SF", ".RSA", ".DSA", ".EC"))
kept_meta = [n for n in names if n.startswith("META-INF/") and not is_sig(n)]
zout = zipfile.ZipFile(dst, "w")
for item in zin.infolist():
    name = item.filename
    if (
        is_sig(name)
        or name.startswith("assets/Android/")
        or name in extras
        or name in ("classes3.dex", "classes4.dex")
    ):
        continue
    data = mf if name == "AndroidManifest.xml" else zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
for name, data in sorted(asset_overrides.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
info = zipfile.ZipInfo("classes3.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c3)
info = zipfile.ZipInfo("classes4.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c4)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), f"changed: {name}"
for name, data in asset_overrides.items():
    assert hashlib.sha256(zout2.read(name)).hexdigest() == hashlib.sha256(data).hexdigest(), f"asset override mismatch: {name}"
expected_android = {f"assets/{asset['path']}" for asset in official_manifest["files"]}
expected_android.add("assets/Android/manifest.json")
actual_android = {name for name in zout2.namelist() if name.startswith("assets/Android/")}
assert actual_android == expected_android, (
    f"assets/Android mismatch: missing={sorted(expected_android - actual_android)}, "
    f"extra={sorted(actual_android - expected_android)}"
)
for asset in official_manifest["files"]:
    name = "assets/" + asset["path"]
    data = zout2.read(name)
    if name == patched_network_key:
        continue
    assert len(data) == asset["size"], f"packaged size mismatch: {name}"
    assert hashlib.md5(data).hexdigest() == asset["hash"], f"packaged md5 mismatch: {name}"
assert zout2.read("assets/Android/manifest.json") == official_manifest_bytes
assert len(patched_network_bytes) > 0
for name in kept_meta:
    assert name in zout2.namelist(), f"lost META-INF entry: {name}"
assert "classes3.dex" in zout2.namelist()
assert "classes4.dex" in zout2.namelist()
assert zout2.read("classes4.dex") == c4, "classes4.dex 与工作副本不一致"
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
assert not any(n.upper().endswith((".SF", ".RSA", ".DSA", ".EC")) for n in zout2.namelist())
print("  merged ok; complete HotUpdate assets + patched DLL + classes3/classes4(so+killer) verified")
zin2.close(); zout2.close()
PY

echo "[6/6] 产物校验"
IN_SO_MD5="$(unzip -p "$UNSIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_PREBUILT")"
[[ "$IN_SO_MD5" == "$SRC_SO_MD5" ]] || { echo "ERROR: 包内 so 与构建产物不一致" >&2; exit 1; }
IN_NET_MD5="$(unzip -p "$UNSIGNED" assets/Android/Dlls/System.Network.dll.bytes | md5 -q)"
SRC_NET_MD5="$(md5 -q "$PATCHED_ASSETS/Dlls/System.Network.dll.bytes")"
[[ "$IN_NET_MD5" == "$SRC_NET_MD5" ]] || { echo "ERROR: 包内 System.Network 与 patched 产物不一致" >&2; exit 1; }
IN_MANIFEST_VERSION="$(unzip -p "$UNSIGNED" assets/Android/manifest.json | jq -r '.version')"
[[ "$IN_MANIFEST_VERSION" == "$EXPECT_HOTUPDATE_VERSION" ]] || { echo "ERROR: 包内 manifest 版本异常: ${IN_MANIFEST_VERSION}（期望 ${EXPECT_HOTUPDATE_VERSION}）" >&2; exit 1; }
IN_MANIFEST_NET_HASH="$(unzip -p "$UNSIGNED" assets/Android/manifest.json | jq -r '.files[] | select(.path=="Android/Dlls/System.Network.dll.bytes") | .hash')"
[[ "$IN_MANIFEST_NET_HASH" == "$EXPECT_NETWORK_HASH" ]] || {
  echo "ERROR: 包内 manifest 未保留官方 Network hash: $IN_MANIFEST_NET_HASH" >&2
  exit 1
}
rm -rf "$WORK/c3_check"
unzip -p "$UNSIGNED" classes3.dex > "$WORK/classes3.check.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3_check" "$WORK/classes3.check.dex" >/dev/null
[[ -f "$WORK/c3_check/com/starry/magic/StarryInitProvider.smali" ]] || { echo "ERROR: classes3 missing provider" >&2; exit 1; }
[[ -f "$WORK/c3_check/com/starry/magic/sign/StarryAppComponentFactory.smali" ]] || { echo "ERROR: classes3 missing StarryAppComponentFactory" >&2; exit 1; }
unzip -p "$UNSIGNED" classes4.dex > "$WORK/classes4.check.dex"
"$BAKSMALI_BIN" d -o "$WORK/c4_pkg_check" "$WORK/classes4.check.dex" >/dev/null
[[ -f "$WORK/c4_pkg_check/bin/mt/signature/KillerApplication193.smali" ]] || { echo "ERROR: 成品包 classes4.dex 缺 KillerApplication193（TapTap 验签会继续失败）" >&2; exit 1; }

ls -lh "$UNSIGNED"
md5 -q "$UNSIGNED" | sed 's/^/md5=/'
shasum -a 256 "$UNSIGNED" | awk '{print "sha256="$1}'
