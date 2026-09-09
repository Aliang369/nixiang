#!/usr/bin/env bash
# 星光魔法 → 消消英雄2 1.2.26：构建 + 注入 + SignSpoof(原厂) + TapSign + 签名安装
#
# 注意：Gradle/apktool/aapt2 在含中文的路径下会把目录弄成 ?? 并失败。
# 本脚本把 native/gradle/apktool 都放到 /tmp ASCII 路径完成，再拷回 08_output。
#
# 用法:
#   bash scripts/inject_xxh2_1.2.26.sh
#   STARRY_BUILD_TYPE=debug bash scripts/inject_xxh2_1.2.26.sh
#   STARRY_SKIP_INSTALL=1 bash scripts/inject_xxh2_1.2.26.sh
#   STARRY_INSTALL=1 bash scripts/inject_xxh2_1.2.26.sh   # 同默认会装机
set -euo pipefail

BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [[ "$BUILD_TYPE" != "release" && "$BUILD_TYPE" != "debug" ]]; then
  echo "ERROR: STARRY_BUILD_TYPE must be release or debug (got: $BUILD_TYPE)"
  exit 1
fi
if [[ "$BUILD_TYPE" == "release" ]]; then
  GRADLE_TASK="assembleRelease"
  APK_NAME="app-release.apk"
else
  GRADLE_TASK="assembleDebug"
  APK_NAME="app-debug.apk"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STARRY_SRC="$(cd "$SCRIPT_DIR/.." && pwd)"
NIXIANG="/Users/aliang/Documents/nixiang"
ORIG_APK="$NIXIANG/02_apks/消消英雄2/消消英雄2_1.2.26_orig.apk"
SRC_DECODE="$NIXIANG/03_decompiled/消消英雄2/apktool_1.2.26_orig"
FINAL_PATCH="$NIXIANG/07_patched/消消英雄2/apktool_1.2.26_modmenu"
OUT="$NIXIANG/08_output/消消英雄2"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
# 原厂证书 MD5（DER），与 SignSpoof / TapSignKit 一致
ORIG_MD5="55d94651f0dc17f082a530f98582d78e"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
BT_ZIPALIGN=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT_ZIPALIGN="$HOME/Library/Android/sdk/build-tools/$v/zipalign"
    break
  fi
done
export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

PKG="com.blina.match2.tt"
SIGNED="$OUT/消消英雄2_1.2.26_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/消消英雄2_1.2.26_starry-debugSigned.apk"

WORK="$(mktemp -d /tmp/xxh2_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$WORK/apktool_modmenu"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/xxh2_starry_unsigned.apk"
trap 'rm -rf "$WORK"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing ${ORIG_APK}"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
MK_SRC="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_MATCH2' "$MK_SRC" || { echo "ERROR: Android.mk missing STARRY_GAME_MATCH2"; exit 1; }
grep -E 'STARRY_GAME_MATCH2' "$MK_SRC" \
  || { echo "ERROR: Android.mk Android.mk must enable STARRY_GAME_MATCH2"; exit 1; }
echo "[check] Android.mk target=xxh2"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")"

if [[ ! -d "$SRC_DECODE/smali" && ! -d "$SRC_DECODE/smali_classes2" ]]; then
  echo "[0a] apktool decode orig → $SRC_DECODE"
  rm -rf "$SRC_DECODE"
  apktool d -f -o "$SRC_DECODE" "$ORIG_APK"
fi

echo "[0/7] rsync starry → ASCII work + copy decode"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
cp -a "$SRC_DECODE" "$PATCH"

# Gradle 在中文路径下无法 spawn ndk-build：预编译 so → jniLibs，并关掉 externalNativeBuild
APP="$STARRY_BUILD/app"
echo "[1/7] ndk-build + assemble ($GRADLE_TASK) in $STARRY_BUILD"
"$NDK/ndk-build" -C "$APP/src/main/jni" \
  NDK_PROJECT_PATH="$APP" \
  NDK_APPLICATION_MK="$APP/src/main/jni/Application.mk" \
  APP_BUILD_SCRIPT="$APP/src/main/jni/Android.mk" \
  APP_ABI=arm64-v8a \
  NDK_OUT="$APP/build/intermediates/ndk/obj" \
  NDK_LIBS_OUT="$APP/build/intermediates/ndk/libs" \
  -j8
mkdir -p "$APP/src/main/jniLibs/arm64-v8a"
cp -f "$APP/build/intermediates/ndk/libs/arm64-v8a/"*.so "$APP/src/main/jniLibs/arm64-v8a/"
python3.11 - <<PY
from pathlib import Path
import re
p = Path("$APP/build.gradle")
t = p.read_text()
t2 = re.sub(
    r"\n    externalNativeBuild \{[\s\S]*?\n    \}\n",
    "\n    // externalNativeBuild disabled (ASCII CI): so from jniLibs\n",
    t,
    count=1,
)
p.write_text(t2)
PY
(cd "$STARRY_BUILD" && ./gradlew "$GRADLE_TASK" --console=plain)
APK="$STARRY_BUILD/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$APK" ]] || { echo "ERROR: missing $APK"; exit 1; }
echo "using: $APK"

echo "[2/7] decode starry"
apktool d -f -o "$WORK/starry_dec" "$APK" >/dev/null

echo "[3/7] inject so/assets/smali"
mkdir -p "$PATCH/lib/arm64-v8a"
cp "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$PATCH/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$PATCH/lib/arm64-v8a/libc++_shared.so"
# 原生菜单：不再拷贝 assets/web；清掉旧包残留 HTML
rm -rf "$PATCH/assets/web" 2>/dev/null || true
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$PATCH/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$PATCH/assets/certs/"
fi
rm -rf "$PATCH/smali_classes3/com/starry"
mkdir -p "$PATCH/smali_classes3/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$PATCH/smali_classes3/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$PATCH/smali_classes3/com/"
done
[[ -d "$PATCH/smali_classes3/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }

echo "[4/7] patch Manifest / FlyApplication SignSpoof / PolicyActivity / TapSign"
python3.11 <<PY
from pathlib import Path
import re

PATCH = Path(r"$PATCH")
ORIG_MD5 = "$ORIG_MD5"

manifest = PATCH / "AndroidManifest.xml"
t = manifest.read_text(encoding="utf-8")
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
    print("manifest: perms")
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
    print("manifest: MenuService")
manifest.write_text(t, encoding="utf-8")

# 原厂证书伪装：FlyApplication 最早 install SignSpoof（PackageInfo.CREATOR）
for app_smali in PATCH.glob("smali*/cn/fly/FlyApplication.smali"):
    s = app_smali.read_text(encoding="utf-8")
    if "SignSpoof;->install" in s:
        print(f"sign: FlyApplication SignSpoof ok ({app_smali.relative_to(PATCH)})")
        continue
    if ".method static constructor <clinit>()V" in s:
        s2, n = re.subn(
            r"(\.method static constructor <clinit>\(\)V\n(?:    \.locals \d+\n)?)",
            r"\1\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n",
            s,
            count=1,
        )
    else:
        clinit = """
.method static constructor <clinit>()V
    .locals 0

    # starry_sign_spoof — 原厂 Ranhao Kang 证书
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    return-void
.end method
"""
        s2, n = re.subn(r"(\n# (?:direct|virtual) methods\n)", clinit + r"\1", s, count=1)
        if n == 0:
            s2 = s + "\n" + clinit
            n = 1
    if n:
        # attachBaseContext / onCreate 再挂一次（MultiDex 之后）
        for needle in (
            "invoke-static {p1}, Landroidx/multidex/MultiDex;->install(Landroid/content/Context;)V\n",
            "invoke-static {p0}, Landroidx/multidex/MultiDex;->install(Landroid/content/Context;)V\n",
        ):
            inject = needle + "\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n"
            if needle in s2 and "SignSpoof;->install()V" in s2:
                # already have at least clinit; add after MultiDex if present
                if inject not in s2 and needle in s2:
                    s2 = s2.replace(needle, inject, 1)
        if s2.count("SignSpoof;->install()V") < 1:
            raise SystemExit("FlyApplication SignSpoof inject failed")
        app_smali.write_text(s2, encoding="utf-8")
        print(f"sign: FlyApplication ← SignSpoof ({app_smali.relative_to(PATCH)})")
    else:
        print(f"sign: WARN FlyApplication patch failed ({app_smali.relative_to(PATCH)})")

tap = next(PATCH.glob("smali*/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"), None)
if tap is None:
    raise SystemExit("TapSignKit.smali missing")
s = tap.read_text(encoding="utf-8")
if ORIG_MD5 not in s or "starry_sign_bypass" not in s:
    new_get = f""".method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .locals 1

    # starry_sign_bypass
    const-string v0, "{ORIG_MD5}"

    return-object v0
.end method"""
    s2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        new_get, s, count=1)
    if n != 1:
        raise SystemExit("TapSignKit.getSign patch failed")
    tap.write_text(s2, encoding="utf-8")
    print(f"sign: TapSignKit.getSign → {ORIG_MD5}")
else:
    print("sign: TapSignKit ok")

clu = next(PATCH.glob("smali*/com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"), None)
if clu is None:
    raise SystemExit("ClientLoginUtils.smali missing")
s = clu.read_text(encoding="utf-8")
if "force WebLogin" not in s:
    new_icl = """.method public final isClientLogin(Landroid/app/Activity;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
    s2, n = re.subn(
        r"\.method public final isClientLogin\(Landroid/app/Activity;\)Z[\s\S]*?\.end method",
        new_icl, s, count=1)
    if n != 1:
        raise SystemExit("ClientLoginUtils patch failed")
    clu.write_text(s2, encoding="utf-8")
    print("sign: ClientLoginUtils")
else:
    print("sign: ClientLoginUtils ok")

clh = next(PATCH.glob("smali*/com/taptap/sdk/login/internal/handlers/client/ClientLoginHandler.smali"), None)
if clh is None:
    raise SystemExit("ClientLoginHandler.smali missing")
s = clh.read_text(encoding="utf-8")
if "force WebLogin" not in s:
    new_ch = """.method public canHandle(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Z
    .locals 1

    # starry: force WebLogin — 禁止跳转 TapTap 客户端 SSO（改签后必失败）
    const/4 v0, 0x0

    return v0
.end method"""
    s2, n = re.subn(
        r"\.method public canHandle\(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;\)Z[\s\S]*?\.end method",
        new_ch, s, count=1)
    if n != 1:
        raise SystemExit("ClientLoginHandler.canHandle patch failed")
    clh.write_text(s2, encoding="utf-8")
    print("sign: ClientLoginHandler → force WebLogin")
else:
    print("sign: ClientLoginHandler ok")

act = next(PATCH.glob("smali*/com/alina/PolicyActivity.smali"), None)
if act is None:
    raise SystemExit("PolicyActivity.smali missing")
s = act.read_text(encoding="utf-8")
if "MenuService;->start" not in s:
    m = re.search(
        r"(\.method protected onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n.end method)",
        s)
    if not m:
        raise SystemExit("PolicyActivity.onCreate not found")
    inject = """
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
    s = s[: m.start(2)] + inject + s[m.start(2) :]
    print("hook: PolicyActivity.onCreate")
if "MenuService;->notifyHostLeaving" not in s:
    s += """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method
"""
if "MenuService;->notifyHostResumed" not in s:
    s += """
.method protected onResume()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onResume()V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->notifyHostResumed(Landroid/app/Activity;)V

    return-void
.end method
"""
act.write_text(s, encoding="utf-8")

pz = next(PATCH.glob("smali*/com/alina/PuzzleActivity.smali"), None)
if pz is None:
    raise SystemExit("PuzzleActivity.smali missing")
ps = pz.read_text(encoding="utf-8")
if "starry_magic" not in ps:
    m = re.search(
        r"(\.method protected onCreate\(Landroid/os/Bundle;\)V[\s\S]*?)(\n    return-void\n.end method)",
        ps)
    if not m:
        raise SystemExit("PuzzleActivity.onCreate not found")
    inject = """
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
    ps = ps[: m.start(2)] + inject + ps[m.start(2) :]
    pz.write_text(ps, encoding="utf-8")
    print("hook: PuzzleActivity.onCreate")
print("hooks done")
PY

echo "[5/7] (no extra asset gate)"
echo "[6/7] apktool rebuild (ASCII)"
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + install"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
mkdir -p "$OUT"
cp -f "$UNSIGNED" "$SIGNED"
# sync patched tree back for笔记对照
rm -rf "$FINAL_PATCH"
mkdir -p "$(dirname "$FINAL_PATCH")"
cp -a "$PATCH" "$FINAL_PATCH"
ls -lh "$SIGNED"

if [[ "${STARRY_SKIP_INSTALL:-0}" == "1" ]]; then
  echo "skip install"
  echo "done → $SIGNED"
  exit 0
fi

# 默认装机；可用 STARRY_INSTALL=0 跳过
if [[ "${STARRY_INSTALL:-1}" != "1" ]]; then
  echo "STARRY_INSTALL!=1 skip install"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_xxh2_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
if ! adb shell su -c "pm install -r -d --user 0 '$REMOTE_APK'"; then
  echo "signature mismatch? uninstall + reinstall"
  adb uninstall "$PKG" 2>/dev/null || true
  adb install --no-incremental "$SIGNED"
fi
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED"
echo "log: adb logcat -s StarrySignSpoof:V StarryMagic:V Mod_Menu:V"
