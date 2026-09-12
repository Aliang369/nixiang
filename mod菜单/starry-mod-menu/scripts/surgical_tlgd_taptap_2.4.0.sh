#!/usr/bin/env bash
# 弹力果冻 2.4.0 **TapTap/官服渠道**（com.ginicat.sweet，vc4409）— 外科组装
#   官方 classes.dex / resources.arsc 字节不变（组装后脚本内断言 sha256 相等）
#
# 本版 = 5 功能数据型包：零 hook / 零 invoke，只读写 ObscuredFloat 字段（日志前缀 [TLGDF]）。
#
# 与 4399 版（surgical_tlgd_2.4.0.sh）的差异：
#   - 底包 / 包名 / 产物名 / Manifest 缓存目录全部独立（不覆盖 4399 产物）
#   - **不再自己跑 ndk-build**：消费 build_so 阶段产物 libs_tlgd/.../libstarry_magic.so
#   - 走私有 mk Android_tlgd.mk（modern 注册表 TlgdTapTap），不查公共 Android.mk 的 STARRY_GAME_TLGD
#
# 用法:
#   bash scripts/surgical_tlgd_taptap_2.4.0.sh
#   STARRY_INSTALL=1 bash scripts/surgical_tlgd_taptap_2.4.0.sh
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
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/弹力果冻/弹力果冻_2.4.0_taptap_official.apk"
OUT="$NIXIANG/08_output/弹力果冻"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.ginicat.sweet"
TAG_OUT="taptap"
SIGNED="$OUT/弹力果冻_2.4.0_${TAG_OUT}-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/弹力果冻_2.4.0_${TAG_OUT}-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/弹力果冻/surgical_taptap/AndroidManifest.xml"
BUILT_SO="$STARRY_SRC/libs_tlgd/arm64-v8a/libstarry_magic.so"
MK="$STARRY_SRC/app/src/main/jni/Android_tlgd.mk"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }
[[ -f "$MK" ]] || { echo "ERROR: missing $MK (modern 私有 mk)"; exit 1; }
echo "[check] 底包 = $ORIG_APK"
echo "[check] 私有 mk = Android_tlgd.mk (modern 注册表 TlgdTapTap)"

LOCK_DIR="/tmp/tlgd_taptap_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/tlgd_taptap.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] 消费 build_so 产物 -> jniLibs（本脚本不重编 native）"
[[ -f "$BUILT_SO" ]] || { echo "ERROR: missing $BUILT_SO —— 先跑管线 build_so 阶段"; exit 1; }
# ⚠ macOS BSD grep 在 zh_CN.UTF-8 下匹配不了二进制里的 UTF-8 模式 → 必须 LC_ALL=C
if ! LC_ALL=C grep -a -q '\[Tlgd\]' "$BUILT_SO" || ! LC_ALL=C grep -a -q '\[TLGDF\]' "$BUILT_SO" \
   || ! LC_ALL=C grep -a -q 'Category_战斗' "$BUILT_SO"; then
  echo "ERROR: $BUILT_SO 不是 TLGD 功能版（缺 [Tlgd]/[TLGDF]/Category_战斗）"
  strings "$BUILT_SO" | grep -E '\[Dwct\]|\[Tlgd\]|\[TLGDF\]|dwct|tlgd' | sort -u | head -20
  exit 1
fi
if grep -a -q '\[Dwct\]' "$BUILT_SO"; then
  echo "ERROR: $BUILT_SO 含 [Dwct]，jniLibs 未刷新"; exit 1
fi
BUILT_SO_MD5="$(md5 -q "$BUILT_SO")"
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$BUILT_SO" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok (tlgd 探针版, md5=$BUILT_SO_MD5)"

echo "[1b/6] gradle ${GRADLE_TASK} (pack refreshed jniLibs so)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! LC_ALL=C grep -a -q '\[TLGDF\]' "$WORK/check.so" || ! LC_ALL=C grep -a -q 'Category_战斗' "$WORK/check.so"; then
  echo "ERROR: APK 内 so 不是 TLGD 功能版 —— 菜单会错成上一款游戏"
  strings "$WORK/check.so" | grep -E '\[Dwct\]|\[Tlgd\]|\[TLGDF\]|dwct|tlgd' | sort -u | head -20
  exit 1
fi
echo "  APK 内 so verified [TLGDF]/Category_战斗"

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes2.dex (官方仅 classes.dex)"
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
if ! grep -q '^\.super Landroid/app/Application;' "$SMALI_ROOT/com/starry/magic/StarryApp.smali"; then
  echo "ERROR: 官方无自定义 Application，StarryApp 必须 .super Landroid/app/Application;"
  exit 1
fi
# TapTap 渠道专属：官服包名必须在 SignSpoof 表内（否则签名伪装落空）
# 注意：smali 里字符串常量保留点号（const-string vX, "com.ginicat.sweet"），不是类型描述符 com/ginicat/sweet
grep -qE 'com[./]ginicat[./]sweet' "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" \
  || { echo "ERROR: SignSpoof.smali 未含官服包名 com.ginicat.sweet"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] binary AndroidManifest (mini apktool，不对 2G 原包 apktool b)"
if [[ -n "${TLGD_BINARY_MANIFEST:-}" && -f "$TLGD_BINARY_MANIFEST" ]]; then
  cp "$TLGD_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
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
    info = zin.getinfo(name)
    data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
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
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if 'android:name="com.starry.magic.StarryApp"' not in t:
    t = t.replace("<application ", '<application android:name="com.starry.magic.StarryApp" ', 1)
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
print("  text Manifest patched")
PY
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge (keep official classes.dex + arsc)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()

def is_protected(name: str) -> bool:
    return name in ("classes.dex", "resources.arsc")

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == "classes2.dex":
        continue
    if name == "AndroidManifest.xml":
        data = mf
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

info = zipfile.ZipInfo("classes2.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c2)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in zin2.namelist():
    if is_protected(name):
        assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes2.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes2={len(c2)}")
zin2.close(); zout2.close()
PY

# ---- 旧 so 混包防线：包内 so 必须等于 build_so 产物 ----
PKG_SO_MD5="$(unzip -p "$WORK/unsigned.apk" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
[[ "$PKG_SO_MD5" == "$BUILT_SO_MD5" ]] \
  || { echo "ERROR: 包内 so md5=$PKG_SO_MD5 != build_so 产物 $BUILT_SO_MD5"; exit 1; }
echo "  [5b/6] 包内 so md5 == build_so 产物 ($PKG_SO_MD5)"

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
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*"$TAG_OUT"*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
ls -lh "$SIGNED"
echo "artifact_md5=$(md5 -q "$SIGNED")"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "NOTE: 装机由管线 install 阶段负责（含设备包 md5 复核）；此处不自动 adb install"
fi

echo "DONE: $SIGNED"
