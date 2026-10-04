#!/usr/bin/env bash
# 追迹迷城 1.0.20260821 — 外科组装（禁止对游戏 apktool b 整包）
#
# 产物：08_output/追迹迷城/追迹迷城_1.0.20260821_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_zjmc_1.0.20260821.sh
#   STARRY_INSTALL=1 bash scripts/surgical_zjmc_1.0.20260821.sh
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
ORIG_APK="$NIXIANG/02_apks/追迹迷城/追迹迷城_1.0.20260821_official.apk"
OUT="$NIXIANG/08_output/追迹迷城"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.TeaMelo.DungeonChasers"
SIGNED="$OUT/追迹迷城_1.0.20260821_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/追迹迷城_1.0.20260821_surgical-debugSigned.apk"
MF_CACHE="$NIXIANG/07_patched/追迹迷城/surgical_1.0.20260821/AndroidManifest.xml"
ORIG_MD5="4684a2b2249e978c74230f8895635894"

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
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ZJMC' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_ZJMC"; exit 1; }
echo "[check] Android.mk target=zjmc"

LOCK_DIR="/tmp/zjmc_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/zjmc_surgical.XXXXXX)"
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
if ! grep -a -q '\[Zjmc\]' "$SO_BUILT" || ! grep -a -q 'zjmc' "$SO_BUILT"; then
  echo "ERROR: so 不是 ZJMC"
  strings "$SO_BUILT" | grep -E 'Zjmc|zjmc|Cstjjs|Pcr|Gjc2' | sort -u | head -30
  exit 1
fi
if grep -a -q '\[Cstjjs\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 [Cstjjs]"
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
echo "  so -> jniLibs ok ([Zjmc] verified)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Zjmc\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 ZJMC"; exit 1
fi

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] classes7.dex = com/starry"
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
"$SMALI_BIN" assemble -o "$WORK/classes7.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] patch classes5 (TapSign / WebLogin / PrivacyActivity)"
unzip -p "$ORIG_APK" classes5.dex > "$WORK/classes5_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c5smali" "$WORK/classes5_orig.dex"
export ORIG_MD5
python3.11 - "$WORK/c5smali" <<'PY'
from pathlib import Path
import os, re, sys
root = Path(sys.argv[1])
ORIG_MD5 = os.environ["ORIG_MD5"]

def inject_activity(path: Path, tag: str):
    if not path.is_file():
        print(f"  skip {tag}")
        return
    s = path.read_text(encoding="utf-8")
    if "MenuService;->start" not in s:
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
            if "SignSpoof;->install" not in head2:
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
        print(f"  hook {tag}.onUserLeaveHint")
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
            print(f"  hook {tag}.onResume")
    path.write_text(s, encoding="utf-8")

taps = list(root.glob("com/taptap/sdk/kit/internal/utils/TapSignKit.smali"))
TAP_GETSIGN = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1
    .param p1    # Landroid/content/Context;
        .annotation build Lorg/jetbrains/annotations/NotNull;
        .end annotation
    .end param

    # starry_sign_bypass
    const-string v0, "{ORIG_MD5}"

    return-object v0
.end method"""
if not taps:
    print("  WARN TapSignKit.smali not found")
for tap in taps:
    ts = tap.read_text(encoding="utf-8")
    if ORIG_MD5 in ts and "starry_sign_bypass" in ts:
        print("  TapSignKit ok")
        continue
    ts2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        TAP_GETSIGN, ts, count=1,
    )
    if n != 1:
        raise SystemExit(f"TapSignKit patch failed: {tap}")
    tap.write_text(ts2, encoding="utf-8")
    print(f"  TapSignKit.getSign → {ORIG_MD5}")

clu = next(root.glob("com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"), None)
if clu is None:
    clu = next(root.glob("com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"), None)
if clu is None:
    raise SystemExit("ClientLoginUtils.smali missing")
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
        new_icl, s, count=1,
    )
    if n != 1:
        new_icl_old = """.method public final isClientLogin(Landroid/app/Activity;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
        s2, n = re.subn(
            r"\.method public final isClientLogin\(Landroid/app/Activity;\)Z[\s\S]*?\.end method",
            new_icl_old, s, count=1,
        )
    if n != 1:
        raise SystemExit("ClientLoginUtils.isClientLogin patch failed")
    clu.write_text(s2, encoding="utf-8")
    print("  ClientLoginUtils → force WebLogin")
else:
    print("  ClientLoginUtils ok")

priv = next(root.rglob("PrivacyActivity.smali"), None)
if priv:
    inject_activity(priv, str(priv.relative_to(root)))
else:
    print("  WARN PrivacyActivity.smali not in classes5")
PY
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$WORK/c5smali"

echo "[4b/7] patch classes2 Unity/GameActivity"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import re, sys
root = Path(sys.argv[1])

def inject_activity(path: Path, tag: str):
    if not path.is_file():
        return
    s = path.read_text(encoding="utf-8")
    changed = False
    if "MenuService;->start" not in s:
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
            inject = """
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
            s = s[: m.start(1)] + head2 + inject + s[m.start(2) :]
            changed = True
            print(f"  hook {tag}.onCreate")
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
    if changed:
        path.write_text(s, encoding="utf-8")

for name in ("GameActivity.smali", "UnityPlayerActivity.smali", "TPFSplashActivity.smali"):
    for p in root.rglob(name):
        inject_activity(p, str(p.relative_to(root)))
PY
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$WORK/c2smali"

echo "[5/7] binary AndroidManifest (StarryApp + MenuService)"
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
# 官方无自定义 Application → StarryApp
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
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached Manifest"
fi

echo "[6/7] zip surgical merge"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes2.dex" "$WORK/classes5.dex" "$WORK/classes7.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml classes2.dex classes5.dex classes7.dex lib/arm64-v8a/libstarry_magic.so)
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
assert "classes7.dex" in names
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zo.read("classes.dex")).hexdigest() == hashlib.sha256(z.read("classes.dex")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Zjmc]" in so and b"[Cstjjs]" not in so
print("  merged ok; arsc/classes.dex intact; [Zjmc] so")
PY

echo "[7/7] zipalign + sign → $SIGNED"
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
assert b"[Zjmc]" in so
assert "classes7.dex" in z.namelist()
print("  signed ok")
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install"
  REMOTE=/data/local/tmp/starry_zjmc.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""; then
    echo "ERROR: pm install failed (多半签名不一致，需用户明确同意卸载后再装)"
    exit 1
  fi
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
fi
echo "DONE $SIGNED"
