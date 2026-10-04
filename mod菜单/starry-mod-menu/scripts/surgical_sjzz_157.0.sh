#!/usr/bin/env bash
# 我在神界刷装备 157.0 — 外科组装（官方 v2-only 底包 + classes3 + starry so + Manifest）
#
# 产物：08_output/我在神界刷装备/我在神界刷装备_157.0_surgical-releaseSigned.apk
# 用法：
#   bash scripts/surgical_sjzz_157.0.sh                         # 构建+签名，不安装
#   STARRY_INSTALL=1 SERIAL=f9a116a2 bash scripts/surgical_sjzz_157.0.sh  # 构建+门禁+安装
#   STARRY_BUILD_ONLY=1 ...                                   # 只构建，等价于默认路径
#   STARRY_INSTALL_ONLY=1 SERIAL=f9a116a2 ...                  # 复用正式包，只门禁+安装
#   STARRY_INSTALL=1 STARRY_UNINSTALL_FIRST=1 STARRY_ALLOW_DATA_LOSS=1 SERIAL=f9a116a2 \
#     bash scripts/surgical_sjzz_157.0.sh
set -euo pipefail

INSTALL_ONLY="${STARRY_INSTALL_ONLY:-0}"
BUILD_ONLY="${STARRY_BUILD_ONLY:-0}"
if [[ "$INSTALL_ONLY" == "1" && "$BUILD_ONLY" == "1" ]]; then
  echo "ERROR: STARRY_INSTALL_ONLY=1 与 STARRY_BUILD_ONLY=1 不能同时使用"; exit 1
fi
INSTALL="${STARRY_INSTALL:-0}"
if [[ "$BUILD_ONLY" == "1" ]]; then
  INSTALL=0
fi
if [[ "$INSTALL_ONLY" == "1" ]]; then
  INSTALL=1
fi

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
ORIG_APK="$NIXIANG/02_apks/我在神界刷装备/com.windforce.rg.offical_157.0_official_base.apk"
ORIG_MD5="f9e1425bd1b4c0e27a362a7cd87c0f39"
OUT="$NIXIANG/08_output/我在神界刷装备"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.windforce.rg.offical"
SERIAL="${SERIAL:-f9a116a2}"
SIGNED="$OUT/我在神界刷装备_157.0_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/我在神界刷装备_157.0_surgical-debugSigned.apk"
VERIFY_SCRIPT="$NIXIANG/04_scripts/我在神界刷装备/sjzz_verify.sh"
MF_CACHE="$NIXIANG/07_patched/我在神界刷装备/surgical/AndroidManifest.xml"
SO_PREBUILT="$STARRY_SRC/libs_sjzz/arm64-v8a/libstarry_magic.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" && \
        -x "$HOME/Library/Android/sdk/build-tools/$v/apksigner" && \
        -x "$HOME/Library/Android/sdk/build-tools/$v/aapt2" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: 未找到可用的 Build Tools（需 apksigner/zipalign/aapt2）"; exit 1; }

run_install() {
  local uninstall_first="${STARRY_UNINSTALL_FIRST:-0}"
  local install_args="-r -d"

  if [[ "$uninstall_first" == "1" ]]; then
    [[ "${STARRY_ALLOW_DATA_LOSS:-0}" == "1" ]] \
      || { echo "ERROR: STARRY_UNINSTALL_FIRST=1 需同时显式授权 STARRY_ALLOW_DATA_LOSS=1"; exit 1; }
    install_args="-d"
  fi

  echo "[install] push + pm install ($install_args) → $SERIAL"
  adb -s "$SERIAL" wait-for-device
  adb -s "$SERIAL" push "$SIGNED" /data/local/tmp/sjzz_surgical.apk >/dev/null
  adb -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true

  if [[ "$uninstall_first" == "1" ]]; then
    echo "[install] uninstall existing package (data loss authorized)"
    if ! adb -s "$SERIAL" shell "pm uninstall $PKG"; then
      adb -s "$SERIAL" shell rm -f /data/local/tmp/sjzz_surgical.apk || true
      echo "ERROR: 卸载失败；设备临时包已清理"
      exit 1
    fi
    if adb -s "$SERIAL" shell "pm path $PKG" 2>/dev/null | grep -q .; then
      adb -s "$SERIAL" shell rm -f /data/local/tmp/sjzz_surgical.apk || true
      echo "ERROR: 卸载后仍能查到 $PKG；设备临时包已清理"
      exit 1
    fi
    echo "[install] uninstall ok"
  fi

  if ! adb -s "$SERIAL" shell "pm install $install_args --user 0 /data/local/tmp/sjzz_surgical.apk"; then
    adb -s "$SERIAL" shell rm -f /data/local/tmp/sjzz_surgical.apk || true
    echo "ERROR: 安装失败；设备临时包已清理"
    exit 1
  fi
  adb -s "$SERIAL" shell rm -f /data/local/tmp/sjzz_surgical.apk
  bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$SIGNED" --su \
    || { echo "ERROR: 装机指纹校验失败"; exit 1; }
  echo "[install] done"
}

if [[ "$INSTALL_ONLY" == "1" ]]; then
  echo "[install-only] 复用正式包: $SIGNED"
  [[ -f "$VERIFY_SCRIPT" ]] || { echo "ERROR: missing $VERIFY_SCRIPT"; exit 1; }
  bash "$VERIFY_SCRIPT" "$SIGNED"
  run_install
  exit 0
fi

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing modern so $SO_PREBUILT"; exit 1; }
ACTUAL_MD5=$(md5 -q "$ORIG_APK")
[[ "$ACTUAL_MD5" == "$ORIG_MD5" ]] || { echo "ERROR: 官包指纹不符 $ACTUAL_MD5"; exit 1; }

LOCK_DIR="/tmp/sjzz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/sjzz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle $GRADLE_TASK (starry → classes3 + assets)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry → smali"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes3.dex"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryInitProvider.smali" ]] \
  || { echo "ERROR: StarryInitProvider.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/sign/SignSpoofAppComponentFactory.smali" ]] \
  || { echo "ERROR: SignSpoofAppComponentFactory.smali missing"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$SMALI_ROOT"
ls -la "$WORK/classes3.dex"

mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"

# 只复制星光自有资产，避开游戏原有 assets/certs 和 assets/starry 命名空间。
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  rsync -a --exclude '.DS_Store' "$WORK/starry_dec/assets/certs/" "$WORK/assets/certs/"
fi
if [[ -d "$WORK/starry_dec/assets/starry" ]]; then
  mkdir -p "$WORK/assets/starry"
  rsync -a --exclude '.DS_Store' "$WORK/starry_dec/assets/starry/" "$WORK/assets/starry/"
fi

echo "[4/6] binary AndroidManifest（原 Application 不变；工厂接管签名回灌）"
if [[ -n "${SJZZ_BINARY_MANIFEST:-}" && -f "$SJZZ_BINARY_MANIFEST" ]]; then
  cp "$SJZZ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  generate binary Manifest once (apktool d -s + b extract)"
  apktool d -f -s -o "$WORK/orig_dec" "$ORIG_APK" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
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
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]

old_factory = 'android:appComponentFactory="androidx.core.app.CoreComponentFactory"'
new_factory = 'android:appComponentFactory="com.starry.magic.sign.SignSpoofAppComponentFactory"'
if old_factory in t:
    t = t.replace(old_factory, new_factory, 1)
elif "com.starry.magic.sign.SignSpoofAppComponentFactory" not in t:
    raise SystemExit("expected androidx.core.app.CoreComponentFactory not found")

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
            android:authorities="com.windforce.rg.offical.starryinit"
            android:exported="false"
            android:initOrder="2147483647" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)

p.write_text(t, encoding="utf-8")
print("  text Manifest patched")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c3 = (work / "classes3.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
for root_name in ("certs", "starry"):
    root = work / "assets" / root_name
    if root.is_dir():
        for p in root.rglob("*"):
            if p.is_file():
                extras["assets/" + root_name + "/" + str(p.relative_to(root)).replace("\\", "/")] = p.read_bytes()

replace = {"AndroidManifest.xml": mf}
untouched = ["classes.dex", "classes2.dex", "resources.arsc"]
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    if name in replace:
        data = replace[name]
    else:
        data = zin.read(item)
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
    n += 1

info = zipfile.ZipInfo("classes3.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c3)
n += 1

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in untouched:
    assert hashlib.sha256(zin2.read(name)).hexdigest() == \
           hashlib.sha256(zout2.read(name)).hexdigest(), f"changed: {name}"
assert "classes3.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
assert "lib/arm64-v8a/libc++_shared.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; c3={len(c3)}; untouched 3/3 verified")
zin2.close(); zout2.close()
PY

AAPT2="${BT:+$BT/aapt2}"
[[ -x "$AAPT2" ]] || { echo "ERROR: missing aapt2"; exit 1; }
MF_DUMP="$("$AAPT2" dump xmltree --file AndroidManifest.xml "$WORK/unsigned.apk")"
for needle in \
  'com.starry.magic.sign.SignSpoofAppComponentFactory' \
  'com.starry.magic.StarryInitProvider' \
  'com.starry.magic.menu.MenuService'; do
  grep -Fq "$needle" <<<"$MF_DUMP" || { echo "ERROR: Manifest missing $needle"; exit 1; }
done
echo "  manifest xmltree ok (factory/provider/service)"

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" --ks-pass "pass:$KS_PASS" \
    --key-pass "pass:$KS_PASS" --out "$SIGNED" "$ALIGNED"
else
  java -jar "$SIGNER" -a "$ALIGNED" --ks "$KS" --ksAlias "$KS_ALIAS" --ksKeyPass "$KS_PASS" \
    --ksPass "$KS_PASS"
  mv "$OUT/$(basename "$ALIGNED")" "$SIGNED" 2>/dev/null || true
fi
MD5_SIGNED=$(md5 -q "$SIGNED")
echo "[6/6] done → $SIGNED (md5 $MD5_SIGNED)"
[[ -f "$VERIFY_SCRIPT" ]] || { echo "ERROR: missing $VERIFY_SCRIPT"; exit 1; }
bash "$VERIFY_SCRIPT" "$SIGNED"

if [[ "$INSTALL" == "1" ]]; then
  run_install
fi
