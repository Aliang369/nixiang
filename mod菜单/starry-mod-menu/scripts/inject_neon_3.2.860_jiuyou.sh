#!/usr/bin/env bash
# 星光魔法 → 霓虹深渊 3.2.860 九游包：构建 + 注入 + SignSpoof(9Game) + 签名安装
# 功能/RVA 与 TapTap 同版 NeonMod 一致；包名/证书不同。
#
# 用法:
#   bash scripts/inject_neon_3.2.860_jiuyou.sh
#   STARRY_INSTALL=1 bash scripts/inject_neon_3.2.860_jiuyou.sh
#   STARRY_BUILD_TYPE=debug bash scripts/inject_neon_3.2.860_jiuyou.sh
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
ORIG_APK="$NIXIANG/02_apks/霓虹深渊/霓虹深渊_3.2.860_jiuyou_vc104.apk"
SRC_DECODE="$NIXIANG/03_decompiled/霓虹深渊/apktool_3.2.860_jiuyou"
FINAL_PATCH="$NIXIANG/07_patched/霓虹深渊/apktool_3.2.860_jiuyou_modmenu"
OUT="$NIXIANG/08_output/霓虹深渊"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
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

PKG="com.yhhy.neon.aligames"
SIGNED="$OUT/霓虹深渊_3.2.860_jiuyou_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/霓虹深渊_3.2.860_jiuyou_starry-debugSigned.apk"
# 官方 cert MD5（九游 9Game DER；SignSpoof / 偶发 TapSign）
ORIG_MD5="232126f8add3e1e307bfb8b8f95d1013"

LOCK_DIR="/tmp/neon_jiuyou_inject.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: another neon inject holds $LOCK_DIR (pid $(cat "$LOCK_DIR/pid" 2>/dev/null || echo '?'))"
  exit 1
fi
echo $$ >"$LOCK_DIR/pid"

WORK="$(mktemp -d /tmp/neon_jiuyou_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$FINAL_PATCH"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/neon_jiuyou_starry_unsigned.apk"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
MK_SRC="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_NEON' "$MK_SRC" || { echo "ERROR: Android.mk missing STARRY_GAME_NEON"; exit 1; }
grep -E 'STARRY_GAME_NEON' "$MK_SRC" \
  || { echo "ERROR: Android.mk Android.mk must enable STARRY_GAME_NEON (see SWITCH_GAME.md)"; exit 1; }
echo "[check] Android.mk target=neon (STARRY_GAME_NEON)"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")" "$(dirname "$FINAL_PATCH")"

if [[ ! -d "$SRC_DECODE/smali" && ! -d "$SRC_DECODE/smali_classes2" ]]; then
  echo "[0a] apktool decode orig → $SRC_DECODE (1.3G APK，可能需十几分钟)"
  rm -rf "$SRC_DECODE"
  apktool d -f -o "$SRC_DECODE" "$ORIG_APK"
fi

echo "[0/7] rsync starry → /tmp + sync decode → 07_patched"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$STARRY_BUILD/"
rsync -a --delete \
  --exclude '/build/' \
  "$SRC_DECODE/" "$PATCH/"

# apktool 3.x cannot round-trip original smali_classes2 (method/string refs >65535).
# Move bulky non-hook packages into smali_classes5 (ART loads contiguous classesN.dex).
echo "[0b] split smali_classes2 → next contiguous smali_classesN (64k dex limit)"
C2="$PATCH/smali_classes2"
# ART 只按 classes2,3,4… 连续探测；跳号（如仅有 classes5）会导致整包 secondary 不加载
next_n=3
if [[ -d "$PATCH/smali_classes2" ]]; then
  max_n=2
  for d in "$PATCH"/smali_classes*; do
    [[ -d "$d" ]] || continue
    base=$(basename "$d")
    n=${base#smali_classes}
    [[ "$n" =~ ^[0-9]+$ ]] || continue
    (( n > max_n )) && max_n=$n
  done
  next_n=$((max_n + 1))
fi
C5="$PATCH/smali_classes${next_n}"
echo "split target → $(basename "$C5")"
mkdir -p "$C5"
for pkg in kotlin okhttp3 okio io org; do
  if [[ -d "$C2/$pkg" ]]; then
    rm -rf "$C5/$pkg"
    mv "$C2/$pkg" "$C5/"
  fi
done
for pkg in tds tapsdk tencent taptap zui vivo taptapshare unicom alibaba uc ucweb ninegame ali; do
  if [[ -d "$C2/com/$pkg" ]]; then
    mkdir -p "$C5/com"
    rm -rf "$C5/com/$pkg"
    mv "$C2/com/$pkg" "$C5/com/"
  fi
done
# 勿整包挪 cn/：九游 UC 宿主 R/类与动态插件 ClassLoader 敏感


APP="$STARRY_BUILD/app"
echo "[1/7] ndk-build + assemble ($GRADLE_TASK)"
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
SMALI_DST=""
max_n=1
for d in "$PATCH"/smali_classes*; do
  [[ -d "$d" ]] || continue
  base=$(basename "$d")
  n=${base#smali_classes}
  [[ "$n" =~ ^[0-9]+$ ]] || continue
  if (( n > max_n )); then
    max_n=$n
    SMALI_DST="$d"
  fi
done
if [[ -z "$SMALI_DST" ]]; then
  if [[ -d "$PATCH/smali" ]]; then
    SMALI_DST="$PATCH/smali"
  else
    echo "ERROR: no smali/ or smali_classes* in PATCH"; exit 1
  fi
fi
rm -rf "$SMALI_DST/com/starry"
mkdir -p "$SMALI_DST/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$SMALI_DST/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$SMALI_DST/com/"
done
shopt -u nullglob
[[ -d "$SMALI_DST/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }
echo "inject: starry → $(basename "$SMALI_DST")"

echo "[4/7] patch Manifest / Unity activities / SignSpoof(TPFApplication) / TapSign / RootBeer"
PATCH="$PATCH" ORIG_MD5="$ORIG_MD5" python3.11 <<'PY'
from pathlib import Path
import re, os

PATCH = Path(os.environ["PATCH"])
ORIG_MD5 = os.environ["ORIG_MD5"]

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

def inject_activity(path: Path, tag: str):
    if not path.is_file():
        print(f"hook: skip {tag} (missing)")
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
        if not m:
            print(f"hook: WARN {tag}.onCreate not found")
            return
        head = m.group(1)
        head2, _ = re.subn(
            r"(\.locals )(\d+)",
            lambda mm: mm.group(1) + str(max(int(mm.group(2)), 1)),
            head,
            count=1,
        )
        if "starry_magic" not in head2:
            early = """
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
            if "invoke-super" in head2:
                head2 = head2.replace("invoke-super", early + "\n    invoke-super", 1)
            else:
                head2 = head2 + early
        inject = """
    invoke-static {p0}, Lcom/starry/magic/menu/MenuService;->start(Landroid/content/Context;)V
"""
        s = s[: m.start(1)] + head2 + inject + s[m.start(2) :]
        print(f"hook: {tag}.onCreate")
    if "MenuService;->notifyHostLeaving" not in s:
        s += """
.method protected onUserLeaveHint()V
    .locals 0

    invoke-super {p0}, Landroid/app/Activity;->onUserLeaveHint()V

    invoke-static {}, Lcom/starry/magic/menu/MenuService;->notifyHostLeaving()V

    return-void
.end method
"""
        print(f"hook: {tag}.onUserLeaveHint")
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
            print(f"hook: {tag}.onResume")
    path.write_text(s, encoding="utf-8")

# TPF / Unity 主活动（360 StubApp 壳下真正进游戏的入口）
names = (
    "GameActivity.smali",
    "TPFSplashActivity.smali",
    "UnityPlayerActivity.smali",
    "UnityActivity.smali",
)
seen = set()
cands = []
for name in names:
    for p in PATCH.rglob(name):
        key = str(p.resolve())
        if key in seen:
            continue
        seen.add(key)
        cands.append(p)
if not cands:
    raise SystemExit("no GameActivity/TPFSplash/Unity* activity smali found")
for p in cands:
    inject_activity(p, str(p.relative_to(PATCH)))

# 九游：无 TapSign 时靠 SignSpoof 回灌 CN=9Game；勿放 <clinit>（secondary dex 可能尚未就绪）
for app_smali in PATCH.glob("smali*/com/tpf/sdk/TPFApplication.smali"):
    s = app_smali.read_text(encoding="utf-8")
    # 去掉仅含 SignSpoof 的 clinit（历史误注入）
    s = re.sub(
        r"\n\.method static constructor <clinit>\(\)V\n(?:.|\n)*?SignSpoof;->install\(\)V\n(?:.|\n)*?\.end method\n",
        "\n",
        s,
        count=1,
    )
    if "SignSpoof;->install" in s and "attachBaseContext" in s:
        app_smali.write_text(s, encoding="utf-8")
        print(f"sign: TPFApplication SignSpoof ok ({app_smali.relative_to(PATCH)})")
        continue
    s2, n = re.subn(
        r"(\.method public attachBaseContext\(Landroid/content/Context;\)V\n(?:    \.locals \d+\n)?)",
        r"\1\n    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V\n",
        s,
        count=1,
    )
    if n != 1:
        print(f"sign: WARN TPFApplication.attachBaseContext patch failed ({app_smali.relative_to(PATCH)})")
        continue
    app_smali.write_text(s2, encoding="utf-8")
    print(f"sign: TPFApplication.attachBaseContext ← SignSpoof ({app_smali.relative_to(PATCH)})")
apps = list(PATCH.glob("smali*/com/tpf/sdk/TPFApplication.smali"))
if not apps:
    print("sign: WARN TPFApplication.smali not found")

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
taps = list(PATCH.glob("smali*/com/taptap/sdk/kit/internal/utils/TapSignKit.smali"))
if not taps:
    print("sign: WARN TapSignKit.smali not found (ok if no TapTap sign check)")
for tap in taps:
    ts = tap.read_text(encoding="utf-8")
    if ORIG_MD5 in ts and "starry_sign_bypass" in ts:
        print("sign: TapSignKit ok")
        continue
    ts2, n = re.subn(
        r"\.method public final getSign\(Landroid/content/Context;\)Ljava/lang/String;[\s\S]*?\.end method",
        TAP_GETSIGN,
        ts,
        count=1,
    )
    if n != 1:
        print(f"sign: WARN TapSignKit patch failed: {tap}")
        continue
    tap.write_text(ts2, encoding="utf-8")
    print(f"sign: TapSignKit.getSign → {ORIG_MD5}")

for rb in PATCH.rglob("RootBeerNative.smali"):
    s = rb.read_text(encoding="utf-8")
    if "starry_rootbeer_bypass" in s:
        print("sign: RootBeerNative ok")
        continue
    s2, n = re.subn(
        r"\.method public native checkForRoot\(\[Ljava/lang/String;\)Z[\s\S]*?\.end method",
        """.method public checkForRoot([Ljava/lang/String;)Z
    .locals 1

    # starry_rootbeer_bypass
    const/4 v0, 0x0

    return v0
.end method""",
        s,
        count=1,
    )
    if n == 0:
        s2, n = re.subn(
            r"\.method public checkForRoot\(\[Ljava/lang/String;\)Z[\s\S]*?\.end method",
            """.method public checkForRoot([Ljava/lang/String;)Z
    .locals 1

    # starry_rootbeer_bypass
    const/4 v0, 0x0

    return v0
.end method""",
            s,
            count=1,
        )
    if n == 1:
        rb.write_text(s2, encoding="utf-8")
        print(f"sign: RootBeerNative.checkForRoot bypass ({rb.relative_to(PATCH)})")
    else:
        print(f"sign: WARN RootBeerNative pattern miss ({rb})")

print(f"hooks done ORIG_MD5={ORIG_MD5} pkg=com.yhhy.neon.aligames")
PY

echo "[5/7] (neon: DeductHP/过检 in native)"
echo "[6/7] apktool rebuild (ASCII, large)"
rm -f "$PATCH"/build/apk/classes*.dex 2>/dev/null || true
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + optional install"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
java -jar "$SIGNER" "${UBER_ARGS[@]}"
mkdir -p "$OUT"
SIGNED_SRC=""
for c in \
  "${UNSIGNED%.apk}-aligned-debugSigned.apk" \
  "${UNSIGNED%.apk}-aligned-releaseSigned.apk" \
  "${UNSIGNED%.apk}-debugSigned.apk" \
  "${UNSIGNED%.apk}-releaseSigned.apk" \
  "$UNSIGNED"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"
echo "patched tree → $FINAL_PATCH"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_neon_jiuyou_install.apk"
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
echo "log: adb logcat -s StarryMagic Neon Mod_Menu MenuService DeviceFingerprint AuthClient"
