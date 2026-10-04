#!/usr/bin/env bash
# 长生天机降世 1.0.2 — 外科组装（禁止 classes4 / apktool b 整包）
#
# 硬约束（热更 Code.dll DlgNetkuuLoginSystem）：
#   包体积 > 阈值 且 出现任一 classes4.dex / overlay_integrity.dat / libbattlehook.so
#   →「非法登录」。官方 1.0.2 为 classes3 结构（实测无 classes4/overlay_integrity/battlehook），星光必须并入 classes3。
#
# 产物：08_output/长生天机降世/长生天机降世_1.0.2_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_cstjjs_1.0.2.sh
#   STARRY_INSTALL=1 bash scripts/surgical_cstjjs_1.0.2.sh
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
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="$NIXIANG/02_apks/长生天机降世/长生天机降世_1.0.2_official.apk"
OUT="$NIXIANG/08_output/长生天机降世"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.netkuu.changsheng.tjjs"
SIGNED="$OUT/长生天机降世_1.0.2_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/长生天机降世_1.0.2_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/长生天机降世/surgical_1.0.2/AndroidManifest.xml"
ORIG_MD5="005b1895be6372f183e7d7f7472430b8"

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
[[ -x "$BAKSMALI_BIN" ]] || { echo "ERROR: missing $BAKSMALI_BIN"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_CSTJJS' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_CSTJJS"; exit 1; }
echo "[check] Android.mk target=cstjjs"

LOCK_DIR="/tmp/cstjjs102_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/cstjjs102_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs (Gradle packs jniLibs so only)"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Cstjjs\]' "$SO_BUILT" || ! grep -a -q 'cstjjs' "$SO_BUILT"; then
  echo "ERROR: so 不是 CSTJJS"
  strings "$SO_BUILT" | grep -E 'Cstjjs|cstjjs|Ltzj|Pcr|Gjc2|Dwct' | sort -u | head -30
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
echo "  so -> jniLibs ok ([Cstjjs] verified)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Cstjjs\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 CSTJJS"
  exit 1
fi
echo "  gradle APK so verified [Cstjjs]"

echo "[2/7] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] merge starry into official classes3 (NO classes4); patch UnityApplication in-place"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$WORK/c3smali/com/"
done
shopt -u nullglob
[[ -d "$WORK/c3smali/com/starry/magic" ]] \
  || { echo "ERROR: com/starry/magic missing after merge"; exit 1; }

# UnityApplication 是 final，不能 StarryApp 继承；直接注入 SignSpoof / so / MenuService
python3.11 - "$WORK/c3smali/com/unity3d/player/UnityApplication.smali" <<'PY'
from pathlib import Path
import re, sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
if "SignSpoof;->install" not in t:
    # 新增 attachBaseContext
    attach = """
.method protected attachBaseContext(Landroid/content/Context;)V
    .registers 2
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V
    invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V
    invoke-static {p0}, Lcom/starry/magic/ota/StarryNativeLoad;->ensureLoaded(Landroid/content/Context;)Z
    return-void
.end method
"""
    t = t.replace(".method public onCreate()V", attach + "\n.method public onCreate()V", 1)
    print("  +UnityApplication.attachBaseContext → SignSpoof + ensureLoaded")
if "MenuService;->start" not in t:
    m = re.search(
        r"(\.method public onCreate\(\)V[\s\S]*?)(\n\s*return-void\n\.end method)",
        t,
    )
    if not m:
        raise SystemExit("UnityApplication.onCreate not found")
    inject = """
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
    t = t[: m.start(1)] + m.group(1) + inject + m.group(2) + t[m.end(2) :]
    print("  inject UnityApplication.onCreate → MenuService.start")
p.write_text(t, encoding="utf-8")
PY

# Unity 主活动生命周期 → MenuService（classes3 内；onResume 已存在须注入）
python3.11 - "$WORK/c3smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
main = root / "com/unity3d/player/UnityPlayerActivity.smali"
if not main.is_file():
    raise SystemExit(f"missing {main}")
t = main.read_text(encoding="utf-8")
if "notifyHostResumed" not in t:
    m = re.search(
        r"(\.method protected onResume\(\)V[\s\S]*?)(\n\s*return-void\n\.end method)",
        t,
    )
    if not m:
        raise SystemExit("UnityPlayerActivity.onResume not found")
    inject = """
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V
"""
    t = t[: m.start(1)] + m.group(1) + inject + m.group(2) + t[m.end(2) :]
    print("  inject UnityPlayerActivity.onResume → notifyHostResumed")
if "notifyHostLeaving" not in t:
    t += """
.method protected onUserLeaveHint()V
    .registers 1
    invoke-super {p0}, Landroidx/appcompat/app/AppCompatActivity;->onUserLeaveHint()V
    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V
    return-void
.end method
"""
    print("  +UnityPlayerActivity.onUserLeaveHint → notifyHostLeaving")
main.write_text(t, encoding="utf-8")
PY

"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"
ls -la "$WORK/classes3.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] patch classes.dex: TapSign MD5 + force WebLogin"
unzip -p "$ORIG_APK" classes.dex > "$WORK/classes_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c1smali" "$WORK/classes_orig.dex"
python3.11 - "$WORK/c1smali" "$ORIG_MD5" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])
md5 = sys.argv[2]
tap = root / "com/taptap/sdk/kit/internal/utils/TapSignKit.smali"
clu = root / "com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"
if not tap.is_file():
    raise SystemExit(f"missing {tap}")
if not clu.is_file():
    raise SystemExit(f"missing {clu}")

TAP_GETSIGN = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{md5}"

    return-object v0
.end method"""
ts = tap.read_text(encoding="utf-8")
ts2, n = re.subn(
    r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
    TAP_GETSIGN,
    ts,
    count=1,
)
if n != 1:
    raise SystemExit("TapSignKit.getSign patch failed")
tap.write_text(ts2, encoding="utf-8")
print(f"  TapSignKit.getSign → {md5}")

s = clu.read_text(encoding="utf-8")
if "force WebLogin" not in s:
    new_icl = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO（改签后必失败）
    const/4 v0, 0x0

    return v0
.end method"""
    s2, n = re.subn(
        r"\.method public final isClientLogin\(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;\)Z[\s\S]*?\.end method",
        new_icl,
        s,
        count=1,
    )
    if n != 1:
        raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
    clu.write_text(s2, encoding="utf-8")
    print("  ClientLoginUtils.isClientLogin → false (force WebLogin)")
else:
    print("  ClientLoginUtils already forced")
PY
"$SMALI_BIN" assemble -o "$WORK/classes.dex" "$WORK/c1smali"
ls -la "$WORK/classes.dex"

echo "[5/7] binary AndroidManifest (StarryApp + MenuService + perms)"
if [[ -n "${CSTJJS_BINARY_MANIFEST:-}" && -f "$CSTJJS_BINARY_MANIFEST" ]]; then
  cp "$CSTJJS_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
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
# 保留官方 UnityApplication（final，不可 StarryApp 替换）；只加权限 + MenuService
t = t  # noqa: keep UnityApplication
if 'android:name="com.starry.magic.StarryApp"' in t:
    t = t.replace(
        'android:name="com.starry.magic.StarryApp"',
        'android:name="com.unity3d.player.UnityApplication"',
        1,
    )
    print("  revert StarryApp → UnityApplication")
if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="true">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
p.write_text(t, encoding="utf-8")
print("  text Manifest patched (UnityApplication kept)")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[6/7] zip surgical merge (cp + zip -u; keep classes2 + resources.arsc; NO classes4)"
# 2GB+ 包用 Python zipfile 易坏；官方包整份拷贝再定点替换
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes.dex" "$WORK/classes3.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml classes.dex classes3.dex lib/arm64-v8a/libstarry_magic.so)
[[ -f "$STAGE/lib/arm64-v8a/libc++_shared.so" ]] && ZIP_ARGS+=(lib/arm64-v8a/libc++_shared.so)
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
  while IFS= read -r -d '' f; do
    ZIP_ARGS+=("${f#"$STAGE/"}")
  done < <(find "$STAGE/assets/certs" -type f -print0)
fi
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" "${ZIP_ARGS[@]}")
zip -d "$WORK/unsigned.apk" classes4.dex assets/overlay_integrity.dat \
  lib/arm64-v8a/libbattlehook.so 2>/dev/null || true

python3.11 - "$WORK/unsigned.apk" "$ORIG_APK" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
dst, src = Path(sys.argv[1]), Path(sys.argv[2])
print(f"  unsigned size={dst.stat().st_size}")
z = zipfile.ZipFile(dst)
names = set(z.namelist())
for bad in ("classes4.dex", "assets/overlay_integrity.dat", "lib/arm64-v8a/libbattlehook.so"):
    if bad in names:
        raise SystemExit(f"ERROR: still has {bad}")
assert "lib/arm64-v8a/libstarry_magic.so" in names
assert "classes.dex" in names and "classes2.dex" in names and "classes3.dex" in names
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zo.read("classes2.dex")).hexdigest() == hashlib.sha256(z.read("classes2.dex")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Cstjjs]" in so
print("  merged ok; no classes4; arsc/classes2 intact; [Cstjjs] so present")
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
mkdir -p "$OUT"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
fi

# 成品完整性自检
python3.11 - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
names = set(z.namelist())
bad = [n for n in ("classes4.dex", "assets/overlay_integrity.dat", "lib/arm64-v8a/libbattlehook.so") if n in names]
if bad:
    raise SystemExit(f"ERROR: signed apk still has {bad}")
so = z.read("lib/arm64-v8a/libstarry_magic.so")
if b"[Cstjjs]" not in so:
    raise SystemExit("ERROR: signed apk so missing [Cstjjs]")
from pathlib import Path
print("  signed integrity ok (no blacklist; [Cstjjs] present)")
print(f"  size={Path(sys.argv[1]).stat().st_size}")
PY

ls -lh "$SIGNED"
echo "DONE: $SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] via root pm"
  REMOTE="/data/local/tmp/cstjjs_1.0.2_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c \"pm install -r -d -g --user 0 '$REMOTE'\""; then
    if [[ "${STARRY_ALLOW_UNINSTALL:-0}" == "1" ]]; then
      echo "cover install failed → uninstall + reinstall (STARRY_ALLOW_UNINSTALL=1)"
      adb shell "su -c \"pm uninstall --user 0 '$PKG'\"" 2>/dev/null || adb uninstall "$PKG" || true
      adb shell "su -c \"pm install -r -d -g --user 0 '$REMOTE'\"" \
        || { echo "ERROR: install failed"; exit 1; }
    else
      echo "ERROR: signature mismatch. Re-run with STARRY_ALLOW_UNINSTALL=1（须用户明确同意）"
      exit 1
    fi
  fi
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
  _NIXIANG="$NIXIANG"
  # shellcheck source=/dev/null
  source "$_NIXIANG/04_scripts/_common/starry_post_install.sh" 2>/dev/null || true
  if declare -F starry_post_install >/dev/null 2>&1; then
    starry_post_install "$PKG" || true
  fi
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Cstjjs:V MenuService:V StarrySignSpoof:V"
fi
