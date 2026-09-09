#!/usr/bin/env bash
# milthm 6.0.5 — 外科组装（禁止对游戏 apktool b 整包）
#
# 旧 HTML 菜单（assets/web）→ 原生 MenuService + StarryApp + SignSpoof
# - classes4.dex = 新 com/starry
# - classes3.dex = 去掉旧 com/starry（若底包为旧星光包）
# - classes2.dex = UnityMainActivity / Milauncher 生命周期通知
# - 保留 classes.dex / resources.arsc 字节
#
# 产物：08_output/milthm/milthm_6.0.5_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_milthm_6.0.5.sh
#   STARRY_INSTALL=1 bash scripts/surgical_milthm_6.0.5.sh
#   MILTHM_BASE_APK=/path/to/base.apk bash scripts/surgical_milthm_6.0.5.sh
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
PKG="game.taptap.morizero.milthm"
ORIG_APK="${MILTHM_BASE_APK:-$NIXIANG/02_apks/milthm/milthm_6.0.5_orig_vc40060005.apk}"
DEVICE_APK="$NIXIANG/02_apks/milthm/milthm_6.0.5_from_device.apk"
OUT="$NIXIANG/08_output/milthm"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SIGNED="$OUT/milthm_6.0.5_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/milthm_6.0.5_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/milthm/surgical_6.0.5/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing"; exit 1; }

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_MILTHM' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_MILTHM"; exit 1; }
echo "[check] Android.mk target=milthm"

if [[ ! -f "$ORIG_APK" ]]; then
  if [[ -f "$DEVICE_APK" ]]; then
    ORIG_APK="$DEVICE_APK"
    echo "[base] using cached $DEVICE_APK"
  elif adb get-state >/dev/null 2>&1 && adb shell pm path "$PKG" >/dev/null 2>&1; then
    echo "[base] pulling device base.apk → $DEVICE_APK (~700MB, 请稍候)"
    mkdir -p "$(dirname "$DEVICE_APK")"
    REMOTE="$(adb shell pm path "$PKG" 2>/dev/null | head -1 | tr -d '\r' | sed 's/package://')"
    adb pull "$REMOTE" "$DEVICE_APK"
    ORIG_APK="$DEVICE_APK"
  else
    echo "ERROR: missing base APK. 放置官方原包到 $NIXIANG/02_apks/milthm/milthm_6.0.5_orig_vc40060005.apk 或接真机后重跑"
    exit 1
  fi
fi

[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }

LOCK_DIR="/tmp/milthm_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/milthm_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Milthm\]' "$SO_BUILT" || ! grep -a -q 'milthm' "$SO_BUILT"; then
  echo "ERROR: so 不是 MILTHM（可能仍是上一款游戏）"
  strings "$SO_BUILT" | grep -E 'milthm|\[Milthm\]|\[Sc33\]|sc33|\[Gyws\]' | sort -u | head -20
  exit 1
fi
if grep -a -q '\[Sc33\]' "$SO_BUILT" || grep -a -q 'sc33' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 sc33 标签"
  exit 1
fi
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Milthm] verified)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Milthm\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 MILTHM"; exit 1
fi

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] classes4.dex = com/starry (新原生菜单)"
SMALI_ROOT="$WORK/smali_starry"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] classes3.dex = 去掉旧 com/starry（旧 HTML 菜单残留）"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
if [[ -d "$WORK/c3smali/com/starry" ]]; then
  rm -rf "$WORK/c3smali/com/starry"
  echo "  stripped com/starry from classes3"
else
  echo "  classes3 无 com/starry（官方底包）"
fi
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"

echo "[5/7] patch classes2 (ActivityHandler WebLogin + UnityMainActivity / Milauncher)"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import re, sys

root = Path(sys.argv[1])

# 旧版 Tap SDK（com/taptap/sdk/ui）：ActivityHandler.authorize → 客户端 SSO；改签后必失败
ah = root / "com/taptap/sdk/ui/ActivityHandler.smali"
if not ah.is_file():
    raise SystemExit(f"missing {ah}")
ah_t = ah.read_text(encoding="utf-8")
if "starry_force_web_login" not in ah_t:
    FORCE_WEB = """.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .locals 1
    .param p1, "request"    # Lcom/taptap/sdk/LoginRequest;

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string p1, "starry_force_web_login"

    invoke-direct {v0, p1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method"""
    ah2, n = re.subn(
        r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V[\s\S]*?\.end method",
        FORCE_WEB,
        ah_t,
        count=1,
    )
    if n != 1:
        raise SystemExit("ActivityHandler.authorize patch failed")
    ah.write_text(ah2, encoding="utf-8")
    print("  ActivityHandler.authorize → force WebLogin")
else:
    print("  ActivityHandler.authorize ok")

def strip_old_starry_oncreate(body: str) -> str:
    body = re.sub(
        r"\n\s*const-string v\d+, \"starry_magic\"\s*\n\s*invoke-static \{v\d+\}, Ljava/lang/System;->loadLibrary\(Ljava/lang/String;\)V\s*\n",
        "\n",
        body,
    )
    body = re.sub(
        r"\n\s*invoke-static \{p0\}, Lcom/starry/magic/menu/MenuService;->start\(Landroid/content/Context;\)V\s*\n",
        "\n",
        body,
    )
    return body

def inject_activity(rel: str, tag: str):
    path = root / rel
    if not path.is_file():
        print(f"  skip {tag} (missing {rel})")
        return
    s = path.read_text(encoding="utf-8")
    changed = False

    m = re.search(
        r"(\.method[^\n]*\bonCreate\(Landroid/os/Bundle;\)V\n)([\s\S]*?)(\n\.end method)",
        s,
    )
    if m:
        head, body, end = m.group(1), m.group(2), m.group(3)
        body2 = strip_old_starry_oncreate(body)
        if body2 != body:
            s = s[: m.start()] + head + body2 + end + s[m.end() :]
            changed = True
            print(f"  {tag}.onCreate: removed legacy loadLibrary/MenuService.start (StarryApp owns)")

    if "MenuService;->notifyHostLeaving" not in s:
        s += """
.method protected onUserLeaveHint()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
        changed = True
        print(f"  +{tag}.onUserLeaveHint")

    if "MenuService;->notifyHostResumed" not in s:
        if not re.search(r"\.method[^\n]*\bonResume\(\)V", s):
            s += """
.method protected onResume()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onResume()V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
"""
            changed = True
            print(f"  +{tag}.onResume")
        else:
            m2 = re.search(
                r"(\.method[^\n]*\bonResume\(\)V\n)([\s\S]*?)(\n\.end method)",
                s,
            )
            if m2 and "notifyHostResumed" not in m2.group(0):
                head2, body_r, end2 = m2.group(1), m2.group(2), m2.group(3)
                rets = list(re.finditer(r"(\n\s*)return-void\b", body_r))
                if rets:
                    mm = rets[-1]
                    body_r = (
                        body_r[: mm.start()]
                        + mm.group(1)
                        + "invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V"
                        + mm.group(0)
                        + body_r[mm.end() :]
                    )
                else:
                    body_r += "\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n"
                s = s[: m2.start()] + head2 + body_r + end2 + s[m2.end() :]
                changed = True
                print(f"  +{tag}.onResume (inject)")

    if changed:
        path.write_text(s, encoding="utf-8")
    else:
        print(f"  {tag} ok")

inject_activity("com/morizero/milthm/UnityMainActivity.smali", "UnityMainActivity")
inject_activity("com/morizero/milthm/Milauncher.smali", "Milauncher")
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"

echo "[6/7] binary AndroidManifest (StarryApp + MenuService)"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached Manifest"
else
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
if "<application" in t and 'android:name="com.starry.magic.StarryApp"' not in t:
    t = t.replace("<application", '<application android:name="com.starry.magic.StarryApp"', 1)
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
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi

echo "[7/7] zip surgical merge + sign"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null 2>&1 || true
zip -d "$WORK/unsigned.apk" 'assets/web/*' >/dev/null 2>&1 || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes2.dex" "$WORK/classes3.dex" "$WORK/classes4.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml classes2.dex classes3.dex classes4.dex lib/arm64-v8a/libstarry_magic.so)
[[ -f "$STAGE/lib/arm64-v8a/libc++_shared.so" ]] && ZIP_ARGS+=(lib/arm64-v8a/libc++_shared.so)
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
  while IFS= read -r -d '' f; do
    ZIP_ARGS+=("${f#"$STAGE/"}")
  done < <(find "$STAGE/assets/certs" -type f -print0)
fi
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" "${ZIP_ARGS[@]}")

python3.11 - "$WORK/unsigned.apk" "$ORIG_APK" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
dst, src = Path(sys.argv[1]), Path(sys.argv[2])
z = zipfile.ZipFile(dst)
names = set(z.namelist())
assert "lib/arm64-v8a/libstarry_magic.so" in names
assert "classes4.dex" in names
assert not any(n.startswith("assets/web/") for n in names), "assets/web 未清干净"
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zo.read("classes.dex")).hexdigest() == hashlib.sha256(z.read("classes.dex")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Milthm]" in so or b"milthm" in so
c2 = z.read("classes2.dex")
assert b"starry_force_web_login" in c2, "classes2 missing TapTap WebLogin patch"
print("  merged ok; arsc/classes.dex intact; no assets/web; [Milthm] so; WebLogin patch ok")
PY

ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8 || true
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] 覆盖安装（不卸载）"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  REMOTE=/data/local/tmp/starry_milthm_surgical.apk
  adb push "$SIGNED" "$REMOTE"
  ok=0
  if adb install -r -d "$SIGNED" 2>/dev/null; then
    echo "[install] adb install -r -d ok"; ok=1
  elif adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] su pm install ok"; ok=1
  elif adb shell "pm install -r -d --user 0 '$REMOTE'" 2>/dev/null; then
    echo "[install] pm install ok"; ok=1
  fi
  adb shell "rm -f '$REMOTE'" 2>/dev/null || true
  if [[ "$ok" != "1" ]]; then
    echo "ERROR: 覆盖安装失败（签名不一致时需你明确同意卸载后再装）"
    exit 1
  fi
  _NIXIANG="$(cd "$SCRIPT_DIR/../../.." && pwd)"
  # shellcheck source=/dev/null
  source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
  starry_post_install "$PKG"
  adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Milthm Mod_Menu MenuService:V"
fi

echo "DONE: $SIGNED"
