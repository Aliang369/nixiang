#!/usr/bin/env bash
# 黑暗领域 1.0 — 外科组装（禁止对游戏 apktool b 整包）
#
# - 官方无自定义 Application → StarryApp
# - classes4.dex = com/starry；改 classes2：TapSign / WebLogin / Activity 菜单
# - 保留 classes.dex / classes3.dex / resources.arsc
#
# 产物：08_output/黑暗领域/黑暗领域_1.0_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_haly_1.0.sh
#   STARRY_INSTALL=1 bash scripts/surgical_haly_1.0.sh
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
ORIG_APK="$NIXIANG/02_apks/黑暗领域/黑暗领域_1.0_from_device.apk"
OUT="$NIXIANG/08_output/黑暗领域"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.haly.tap"
SIGNED="$OUT/黑暗领域_1.0_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/黑暗领域_1.0_surgical-debugSigned.apk"
ALIAS_APK="$OUT/黑暗领域_1.0_starry.apk"
MF_CACHE="$NIXIANG/07_patched/黑暗领域/surgical_1.0/AndroidManifest.xml"
ORIG_MD5="d639fb03a75167fbe77bc75e2b4a4304"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing"; exit 1; }

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" && -x "$BAKSMALI_BIN" ]] || { echo "ERROR: smali/baksmali missing"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_COCOS_HOOK\s+-DSTARRY_GAME_HALY' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_HALY"; exit 1; }
echo "[check] Android.mk target=haly"

LOCK_DIR="/tmp/haly_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/haly_surgical.XXXXXX)"
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
if ! grep -a -q '\[Haly\]' "$SO_BUILT" || ! grep -a -q 'haly' "$SO_BUILT"; then
  echo "ERROR: so 不是 HALY"
  strings "$SO_BUILT" | grep -E 'Haly|haly|Gjc2|Dwct|Shhj' | sort -u | head -30
  exit 1
fi
if grep -a -q '\[Gjc2\]' "$SO_BUILT" || grep -a -q 'gjc2intl' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款 gjc2/gjc2intl"
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
echo "  so -> jniLibs ok ([Haly] verified)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Haly\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 HALY"; exit 1
fi

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] classes4.dex = com/starry"
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
"$SMALI_BIN" assemble -o "$WORK/classes4.dex" "$SMALI_ROOT"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] patch classes2 (TapSign / WebLogin / PrivacyGate / AppActivity)"
unzip -p "$ORIG_APK" classes2.dex > "$WORK/classes2_orig.dex"
"$BAKSMALI_BIN" d -o "$WORK/c2smali" "$WORK/classes2_orig.dex"
export ORIG_MD5
python3.11 - "$WORK/c2smali" <<'PY'
from pathlib import Path
import os, re, sys
root = Path(sys.argv[1])
ORIG_MD5 = os.environ["ORIG_MD5"]

def inject_activity(path: Path, tag: str, *, sign_spoof: bool = False):
    if not path.is_file():
        print(f"  skip {tag}")
        return
    s = path.read_text(encoding="utf-8")
    if "MenuService;->start" not in s:
        m = re.search(
            r"(\.method[^\n]*\bonCreate\(Landroid/os/Bundle;\)V\n)([\s\S]*?)(\n\.end method)",
            s,
        )
        if not m:
            print(f"  WARN {tag}.onCreate not found")
        else:
            head, body, end = m.group(1), m.group(2), m.group(3)

            def bump(mm):
                kind = mm.group(1)
                n = int(mm.group(2))
                need = 3 if "registers" in kind else 1
                return kind + str(max(n, need))

            body2, n = re.subn(r"(\.(?:locals|registers) )(\d+)", bump, body, count=1)
            if n == 0:
                print(f"  WARN {tag}.onCreate missing .locals/.registers")
            if sign_spoof and "SignSpoof;->install" not in body2:
                early = "\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n"
                if "invoke-super" in body2:
                    body2 = body2.replace("invoke-super", early + "    invoke-super", 1)
                else:
                    body2 = early + body2
            # insert MenuService.start before final return-void (trailing .line ok)
            rets = list(re.finditer(r"(\n\s*)return-void\b", body2))
            if rets:
                mm = rets[-1]
                body2 = (
                    body2[: mm.start()]
                    + mm.group(1)
                    + "invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V"
                    + mm.group(0)
                    + body2[mm.end() :]
                )
            else:
                body2 = body2 + "\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V\n"
            s = s[: m.start()] + head + body2 + end + s[m.end() :]
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
        print(f"  hook {tag}.onUserLeaveHint")
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
            print(f"  hook {tag}.onResume")
        elif "notifyHostResumed" not in s:
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
                    body_r = body_r + "\n    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V\n"
                s = s[: m2.start()] + head2 + body_r + end2 + s[m2.end() :]
                print(f"  hook {tag}.onResume (inject)")
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
    print("  WARN ClientLoginUtils.smali missing (SdkBridge 登录可能仍可)")
else:
    s = clu.read_text(encoding="utf-8")
    if "force WebLogin" not in s:
        new_icl = """.method public final isClientLogin(Landroid/app/Activity;Lcom/taptap/sdk/login/impl/data/model/Actions;)Z
    .locals 1

    # starry: force WebLogin
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

# PrivacyGate 会 finish，菜单必须挂在 AppActivity；PrivacyActivity 同意页也挂一份
inject_activity(root / "com/cocos/game/AppActivity.smali", "AppActivity", sign_spoof=True)
inject_activity(root / "com/cocos/game/PrivacyGateActivity.smali", "PrivacyGateActivity")
priv = root / "com/cocos/game/PrivacyActivity.smali"
if priv.is_file():
    inject_activity(priv, "PrivacyActivity")
else:
    print("  WARN PrivacyActivity.smali missing")
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
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes2.dex" "$WORK/classes4.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml classes2.dex classes4.dex lib/arm64-v8a/libstarry_magic.so)
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
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zo.read("classes.dex")).hexdigest() == hashlib.sha256(z.read("classes.dex")).hexdigest()
assert hashlib.sha256(zo.read("classes3.dex")).hexdigest() == hashlib.sha256(z.read("classes3.dex")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Haly]" in so
print("  merged ok; arsc/classes.dex/classes3 intact; [Haly] so")
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
cp -f "$SIGNED" "$ALIAS_APK"

python3.11 - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Haly]" in so
assert "classes4.dex" in z.namelist()
print("  signed ok")
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install (不卸载)"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  REMOTE=/data/local/tmp/starry_haly.apk
  adb push "$SIGNED" "$REMOTE"
  ok=0
  if adb install -r -d "$SIGNED" 2>/dev/null; then
    echo "[install] adb install -r -d ok"; ok=1
  elif adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] su pm install ok"; ok=1
  elif adb shell "pm install -r -d --user 0 '$REMOTE'" 2>/dev/null; then
    echo "[install] pm install (no su) ok"; ok=1
  fi
  adb shell "rm -f '$REMOTE'" 2>/dev/null || true
  if [[ "$ok" != "1" ]]; then
    echo "ERROR: 覆盖安装失败。若提示签名不一致，需你明确同意卸载官方包后再装星光包。"
    exit 1
  fi
fi
echo "DONE $SIGNED"
