#!/usr/bin/env bash
# 山河旅探 1.4.38 — 外科组装（禁止对游戏 apktool b 整包）
#
# 产物：08_output/山河旅探/山河旅探_1.4.38_surgical-releaseSigned.apk
# 覆盖安装，禁止卸载（userdata + Android/data 很大）
#
# 用法:
#   bash scripts/surgical_shlt_1.4.38.sh
#   STARRY_INSTALL=1 bash scripts/surgical_shlt_1.4.38.sh
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
ORIG_APK="$NIXIANG/02_apks/山河旅探/山河旅探_1.4.38_official.apk"
OUT="$NIXIANG/08_output/山河旅探"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.itaotuo.shanhelvtan"
SIGNED="$OUT/山河旅探_1.4.38_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/山河旅探_1.4.38_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/山河旅探/surgical_1.4.38/AndroidManifest.xml"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
[[ -n "$BT" ]] || { echo "ERROR: zipalign/apksigner missing"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_SHLT' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_SHLT"; exit 1; }
grep -E 'Games/ShltMod\.cpp' "$MK" \
  || { echo "ERROR: Android.mk must compile ShltMod.cpp"; exit 1; }
echo "[check] Android.mk target=shlt"

LOCK_DIR="/tmp/shlt_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/shlt_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/8] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Shlt\]' "$SO_BUILT" || ! grep -a -q 'shlt' "$SO_BUILT"; then
  echo "ERROR: so 不是 SHLT"
  strings "$SO_BUILT" | grep -E 'Shlt|shlt|Ltzj|Dwct|Gjc2' | sort -u | head -30
  exit 1
fi
if grep -a -q '\[Ltzj\]' "$SO_BUILT" || grep -a -q '\[Dwct\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款游戏标签"
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
echo "  so -> jniLibs ok ([Shlt] verified)"

echo "[1b/8] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Shlt\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 SHLT"; exit 1
fi

echo "[2/8] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/8] classes3.dex = com/starry (StarryApp extends AomiApplication)"
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
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/itaotuo/shanhelvtan/AomiApplication;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/itaotuo/shanhelvtan/AomiApplication;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/itaotuo/shanhelvtan/AomiApplication;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/itaotuo/shanhelvtan/AomiApplication;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
        print(f"  patch: {a[:52]}")
if "Lcom/itaotuo/shanhelvtan/AomiApplication;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing AomiApplication")
if "Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still references Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.smali AomiApplication wiring ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/8] patch classes.dex ActivityHandler → force WebLogin"
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c1smali" "$WORK/classes_orig.dex"
python3.11 - "$WORK/c1smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
ah = root / "com/taptap/sdk/ui/ActivityHandler.smali"
t = ah.read_text(encoding="utf-8")
if "starry_force_web_login" in t:
    print("  already patched")
else:
    FORCE = """.method public authorize(Lcom/taptap/sdk/LoginRequest;)V
    .registers 4
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO
    new-instance v0, Landroid/content/ActivityNotFoundException;

    const-string p1, "starry_force_web_login"

    invoke-direct {v0, p1}, Landroid/content/ActivityNotFoundException;-><init>(Ljava/lang/String;)V

    throw v0
.end method"""
    t2, n = re.subn(
        r"\.method public authorize\(Lcom/taptap/sdk/LoginRequest;\)V[\s\S]*?\.end method",
        FORCE, t, count=1)
    if n != 1:
        raise SystemExit("ActivityHandler.authorize patch failed")
    ah.write_text(t2, encoding="utf-8")
    print("  ActivityHandler.authorize → force WebLogin")
PY
"$SMALI_BIN" assemble -o "$WORK/classes.dex" "$WORK/c1smali"

echo "[4b/8] patch classes2 PrivacyActivity / AomiActivity MenuService"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])

def inject_activity(path: Path, tag: str):
    if not path.is_file():
        print(f"  skip {tag}")
        return
    s = path.read_text(encoding="utf-8")
    changed = False
    if "SignSpoof;->install" not in s:
        m = re.search(
            r"(\.method protected onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n.end method)",
            s,
        )
        if not m:
            m = re.search(
                r"(\.method public onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n.end method)",
                s,
            )
        if m:
            head = m.group(1)
            head2, _ = re.subn(
                r"(\.locals )(\d+)",
                lambda mm: mm.group(1) + str(max(int(mm.group(2)), 1)),
                head, count=1,
            )
            early = """
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V
"""
            if "invoke-super" in head2:
                head2 = head2.replace("invoke-super", early + "\n    invoke-super", 1)
            else:
                head2 = head2 + early
            inject = """
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
            s = s[: m.start(1)] + head2 + inject + s[m.start(2) :]
            changed = True
            print(f"  hook {tag}.onCreate")
        else:
            print(f"  WARN {tag}.onCreate not found")
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
    if "MenuService;->notifyHostResumed" not in s:
        if ".method protected onResume()V" not in s and ".method public onResume()V" not in s:
            s += """
.method protected onResume()V
    .locals 0
    invoke-super {p0}, Landroid/app/Activity;->onResume()V
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
    return-void
.end method
"""
            changed = True
    if changed:
        path.write_text(s, encoding="utf-8")

for name in ("PrivacyActivity.smali", "AomiActivity.smali"):
    hits = list(root.rglob(name))
    if not hits:
        print(f"  WARN {name} missing")
    for p in hits:
        inject_activity(p, str(p.relative_to(root)))
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"

echo "[5/8] binary AndroidManifest (StarryApp + MenuService)"
if [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached Manifest"
else
  mkdir -p "$WORK/mini"
  unzip -p "$ORIG_APK" AndroidManifest.xml > "$WORK/mini/AndroidManifest.xml"
  unzip -p "$ORIG_APK" resources.arsc > "$WORK/mini/resources.arsc"
  unzip -p "$ORIG_APK" classes.dex > "$WORK/mini/classes.dex"
  (cd "$WORK/mini" && zip -q "$WORK/mini.apk" AndroidManifest.xml resources.arsc classes.dex)
  apktool d -f -s -o "$WORK/mini_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/mini_dec/AndroidManifest.xml" <<'PY'
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
old = 'android:name="com.itaotuo.shanhelvtan.AomiApplication"'
new = 'android:name="com.starry.magic.StarryApp"'
if old in t:
    t = t.replace(old, new, 1)
elif 'android:name="com.starry.magic.StarryApp"' not in t:
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
  apktool b -f "$WORK/mini_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached Manifest"
fi

echo "[6/8] patch ILRuntime hotfix bundle"
HOTFIX_OUT="$WORK/hotfix"
mkdir -p "$HOTFIX_OUT"
unzip -p "$ORIG_APK" assets/yoo/HotfixDll/hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle \
  > "$WORK/hotfix_in.bundle"
unzip -p "$ORIG_APK" assets/yoo/HotfixDll/PackageManifest_HotfixDll_1.4.38.bytes \
  > "$WORK/manifest_in.bytes"
python3.11 "$SCRIPT_DIR/shlt_patch_hotfix.py" \
  "$WORK/hotfix_in.bundle" "$WORK/manifest_in.bytes" "$HOTFIX_OUT"

echo "[7/8] zip surgical merge"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a" "$STAGE/assets/yoo/HotfixDll"
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes.dex" "$WORK/classes2.dex" "$WORK/classes3.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
cp -f "$HOTFIX_OUT/hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle" \
  "$STAGE/assets/yoo/HotfixDll/"
cp -f "$HOTFIX_OUT/PackageManifest_HotfixDll_1.4.38.bytes" "$STAGE/assets/yoo/HotfixDll/"
cp -f "$HOTFIX_OUT/PackageManifest_HotfixDll_1.4.38.hash" "$STAGE/assets/yoo/HotfixDll/"
ZIP_ARGS=(
  AndroidManifest.xml classes.dex classes2.dex classes3.dex
  lib/arm64-v8a/libstarry_magic.so
  assets/yoo/HotfixDll/hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle
  assets/yoo/HotfixDll/PackageManifest_HotfixDll_1.4.38.bytes
  assets/yoo/HotfixDll/PackageManifest_HotfixDll_1.4.38.hash
)
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
assert "classes3.dex" in names
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Shlt]" in so and b"[Ltzj]" not in so and b"[Dwct]" not in so
assert b"starry_force_web_login" in z.read("classes.dex")
print("  merged ok; arsc intact; [Shlt] so; WebLogin patch")
PY

echo "[8/8] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -8

python3.11 - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Shlt]" in so
assert "classes3.dex" in z.namelist()
print("  signed ok")
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install (no uninstall)"
  REMOTE=/data/local/tmp/starry_shlt.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""; then
    echo "ERROR: pm install failed（签名不一致也禁止卸载，需用户明确同意才能卸）"
    exit 1
  fi
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
  echo "[install] push patched YooAsset HotfixDll cache (must chown to app uid)"
  CACHE="/sdcard/Android/data/$PKG/files/yoo/HotfixDll"
  adb push "$HOTFIX_OUT/hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle" /data/local/tmp/shlt_hf.bundle
  adb push "$HOTFIX_OUT/PackageManifest_HotfixDll_1.4.38.bytes" /data/local/tmp/shlt_hf.bytes
  adb push "$HOTFIX_OUT/PackageManifest_HotfixDll_1.4.38.hash" /data/local/tmp/shlt_hf.hash
  adb shell "su -c \"
    APP_UID=\\\$(stat -c %u /data/data/$PKG)
    mkdir -p '$CACHE'
    cp -f /data/local/tmp/shlt_hf.bundle '$CACHE/hotfixdll_assets_hotfixdll_hotfix_project_dll.bundle'
    cp -f /data/local/tmp/shlt_hf.bytes '$CACHE/PackageManifest_HotfixDll_1.4.38.bytes'
    cp -f /data/local/tmp/shlt_hf.hash '$CACHE/PackageManifest_HotfixDll_1.4.38.hash'
    rm -f /data/local/tmp/shlt_hf.bundle /data/local/tmp/shlt_hf.bytes /data/local/tmp/shlt_hf.hash
    chown -R \\\$APP_UID:ext_data_rw /sdcard/Android/data/$PKG/files/yoo
    find /sdcard/Android/data/$PKG/files/yoo -type d -exec chmod 2775 {} \\;
    find /sdcard/Android/data/$PKG/files/yoo -type f -exec chmod 664 {} \\;
  \""
fi
echo "DONE $SIGNED"
