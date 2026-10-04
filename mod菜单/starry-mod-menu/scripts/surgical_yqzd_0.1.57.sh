#!/usr/bin/env bash
# 遗弃之地 0.1.57 (com.hortor.yqzd) TapTap — 外科组装
# （官方 classes.dex~classes3.dex / resources.arsc 字节不变；ACE 在 classes.dex，严禁触碰）
#
# 改动：
#   - 改 AndroidManifest（保留官方 GameApplication；+StarryInitProvider + MenuService + 悬浮窗权限）
#   - 新增 classes5.dex（星光 smali；官方自带 classes4.dex=ACE 壳 loader dex 严禁覆盖）
#   - 新增 lib/arm64-v8a/libstarry_magic.so + libc++_shared.so
#   - JS 功能全走 CocosJsBridge.evalOnGLThread 运行时 wrap，无需改任何官方 dex/jsc
#
# 用法:
#   bash scripts/surgical_yqzd_0.1.57.sh
#   STARRY_INSTALL=1 bash scripts/surgical_yqzd_0.1.57.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="$NIXIANG/02_apks/遗弃之地/yqzd_0.1.57_taptap_base.apk"
OUT="$NIXIANG/08_output/遗弃之地"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.hortor.yqzd"
SIGNED="$OUT/遗弃之地_0.1.57_surgical-releaseSigned.apk"
MF_CACHE="$NIXIANG/07_patched/遗弃之地/surgical_0.1.57/AndroidManifest.xml"

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
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_YQZD' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_YQZD"; exit 1; }
echo "[check] Android.mk target=yqzd"

LOCK_DIR="/tmp/yqzd_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/yqzd_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] gradle assembleRelease (starry) — clean rebuild"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew clean assembleRelease --console=plain --rerun-tasks)
STARRY_APK="$WORK/starry/app/build/outputs/apk/release/app-release.apk"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
SO_CHK="$(find "$WORK/starry/app/build" -name libstarry_magic.so -path '*/arm64-v8a/*' | head -1)"
[[ -f "$SO_CHK" ]] || SO_CHK="$(find "$WORK/starry/app/build" -name libstarry_magic.so | head -1)"
[[ -f "$SO_CHK" ]] || { echo "ERROR: libstarry_magic.so not found after build"; exit 1; }
# 以最终 APK 内的 so 为准（中间产物可能有旧缓存）
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/so_in_apk.so" 2>/dev/null || {
  echo "ERROR: APK 内无 lib/arm64-v8a/libstarry_magic.so"; unzip -l "$STARRY_APK" | grep '\.so' | head; exit 1; }
echo "[diag] so_in_apk=$(wc -c <"$WORK/so_in_apk.so") bytes; tags=$(LC_ALL=C strings -a "$WORK/so_in_apk.so" | LC_ALL=C tr '\0' '\n' | LC_ALL=C grep -x 'yqzd\|dwct\|wdyz\|cwjh\|starry_self' | head -3 | tr '\n' ' ')"
if ! LC_ALL=C strings -a "$WORK/so_in_apk.so" | LC_ALL=C tr '\0' '\n' | LC_ALL=C grep -qx 'yqzd'; then
  echo "ERROR: APK 内 so 缺少 game_id yqzd — jniLibs 未同步？"; exit 1
fi
if LC_ALL=C strings -a "$WORK/so_in_apk.so" | LC_ALL=C tr '\0' '\n' | LC_ALL=C grep -qx 'dwct'; then
  echo "ERROR: APK 内 so 仍含 dwct — NDK 缓存污染，拒绝出包"; exit 1
fi
echo "[check] APK so game_id=yqzd ok ($SO_CHK)"

echo "[2/6] apktool d starry → smali"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] collect com/starry smali → classes5.dex (.super→GameApplication)"
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
# 官方 Application = com.hortorgames.gamesdk.GameApplication（hortor SDK 初始化链必须保留）
STARRY_APP_SMALI="$SMALI_ROOT/com/starry/magic/StarryApp.smali"
python3.11 - "$STARRY_APP_SMALI" <<'PY'
from pathlib import Path
import sys, re
p = Path(sys.argv[1])
s = p.read_text(encoding="utf-8")
s2, n = re.subn(r"^\.super Landroid/app/Application;$",
                ".super Lcom/hortorgames/gamesdk/GameApplication;", s, count=1, flags=re.M)
if n != 1:
    raise SystemExit("StarryApp .super patch failed (pattern not found)")
p.write_text(s2, encoding="utf-8")
print("  StarryApp.super → com.hortorgames.gamesdk.GameApplication")
PY
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -la "$WORK/classes5.dex"

echo "[4/6] collect libs"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"

echo "[5/6] binary AndroidManifest"
if [[ -n "${YQZD_BINARY_MANIFEST:-}" && -f "$YQZD_BINARY_MANIFEST" ]]; then
  cp "$YQZD_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
# ACE 壳（com.ace.gshell.AceApplication）：Application 类名不可替换，否则
# AceApplication.initialize 在 libtprt 信号量上毒丸挂起（无限升级/终末地同坑）。
# 走 StarryInitProvider 注入（晚于 ACE initialize 的安全窗口）。
if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.hortor.yqzd.starryinit"
            android:enabled="true"
            android:exported="false"
            android:initOrder="100" />
'''
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
print("  text Manifest patched (provider 注入, 保留官方 GameApplication)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/6] zip surgical merge + sign"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c4 = (work / "classes5.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
skip = set(extras)  # 官方 classes4.dex（ACE 壳 loader dex, 720B）必须原样保留，星光走 classes5.dex
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in skip:
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
    n += 1

info = zipfile.ZipInfo("classes5.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c4)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

# 字节级校验：官方 dex（含 ACE 壳的 classes4.dex）/ arsc 不得有任何改动
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "resources.arsc"):
    assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes5.dex" in zout2.namelist()
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
assert "lib/arm64-v8a/libcocos.so" in zout2.namelist()
print(f"  merged ok; entries≈{n}; classes5={len(c4)}")
zin2.close(); zout2.close()
PY

ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT/.prev"
for old in "$OUT"/遗弃之地_0.1.57_surgical*Signed.apk; do
  [[ -f "$old" ]] || continue
  mv -f "$old" "$OUT/.prev/$(basename "$old").$(date +%Y%m%d%H%M%S)" 2>/dev/null || true
done
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
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/yqzd_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "pm install -r -d --user 0 \"$REMOTE\"" 2>/tmp/yqzd_pm_err.txt; then
    echo "[install] cover failed (likely signature mismatch)."
    cat /tmp/yqzd_pm_err.txt 2>/dev/null || true
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      adb shell "pm uninstall $PKG" 2>/dev/null || adb uninstall "$PKG" || true
      adb install -r -d "$SIGNED"
    else
      echo "ERROR: set STARRY_ALLOW_UNINSTALL=1 to replace official signature."
      exit 1
    fi
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  # shellcheck source=/dev/null
  source "$NIXIANG/04_scripts/_common/starry_post_install.sh"
  starry_post_install "$PKG" || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Yqzd:V MenuService:V"
fi

echo "DONE: $SIGNED"
