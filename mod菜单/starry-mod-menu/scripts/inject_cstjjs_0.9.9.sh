#!/usr/bin/env bash
# 星光魔法 → 长生天机降世 0.9.9 原版：构建 + 注入 + TapSign/RootBeer + 签名安装
#
# 用法:
#   bash scripts/inject_cstjjs_0.9.9.sh
#   STARRY_INSTALL=1 bash scripts/inject_cstjjs_0.9.9.sh
#   STARRY_BUILD_TYPE=debug bash scripts/inject_cstjjs_0.9.9.sh
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
NIXIANG="${NIXIANG:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)}"
ORIG_APK="$NIXIANG/02_apks/长生天机降世/长生天机降世_0.9.9_official.apk"
SRC_DECODE="$NIXIANG/03_decompiled/长生天机降世/apktool_0.9.9_orig"
FINAL_PATCH="$NIXIANG/07_patched/长生天机降世/apktool_0.9.9_modmenu"
OUT="$NIXIANG/08_output/长生天机降世"
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

PKG="com.netkuu.changsheng.tjjs"
SIGNED="$OUT/长生天机降世_0.9.9_starry-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/长生天机降世_0.9.9_starry-debugSigned.apk"
# 官方 cert MD5（TapSign / 签名校验）
ORIG_MD5="005b1895be6372f183e7d7f7472430b8"

LOCK_DIR="/tmp/cstjjs_inject.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: another zjmc inject holds $LOCK_DIR (pid $(cat "$LOCK_DIR/pid" 2>/dev/null || echo '?'))"
  exit 1
fi
echo $$ >"$LOCK_DIR/pid"

WORK="$(mktemp -d /tmp/cstjjs_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$FINAL_PATCH"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/cstjjs_starry_unsigned.apk"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
MK_SRC="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -q 'STARRY_GAME_CSTJJS' "$MK_SRC" || { echo "ERROR: Android.mk missing STARRY_GAME_CSTJJS"; exit 1; }
grep -E 'STARRY_GAME_CSTJJS' "$MK_SRC" \
  || { echo "ERROR: Android.mk Android.mk must enable STARRY_GAME_CSTJJS (see SWITCH_GAME.md)"; exit 1; }
echo "[check] Android.mk target=cstjjs (STARRY_GAME_CSTJJS)"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")" "$(dirname "$FINAL_PATCH")"

if [[ ! -d "$SRC_DECODE/smali" && ! -d "$SRC_DECODE/smali_classes2" ]]; then
  echo "[0a] apktool decode orig → $SRC_DECODE (~900MB APK)"
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
# 拆到「下一个连续」classesN（缺中间号时 ART 不加载更高号 → SignSpoof ClassNotFound）。
#
# 硬约束（0.9.9 DlgNetkuuLoginSystem）：包体积 > 官方且 APK 内出现任一
#   classes4.dex / assets/overlay_integrity.dat / libbattlehook.so
# →「警告！检测到非法登录！」+ 退出。禁止产出 classes4.dex。
echo "[0b] split smali_classes2 → smali_classes3 only (no classes4+)"
C2="$PATCH/smali_classes2"
# 原包只有 smali + smali_classes2 → 只能新建 classes3；禁止再开 4/5…
for n in 4 5 6 7 8 9 10; do
  if [[ -d "$PATCH/smali_classes$n" ]]; then
    echo "WARN: remove leftover smali_classes$n (integrity / ART)"
    rm -rf "$PATCH/smali_classes$n"
  fi
done
CSPLIT="$PATCH/smali_classes3"
mkdir -p "$CSPLIT"
echo "split: move bulky pkgs → smali_classes3"
for pkg in kotlin okhttp3 okio io org; do
  if [[ -d "$C2/$pkg" ]]; then
    rm -rf "$CSPLIT/$pkg"
    mv "$C2/$pkg" "$CSPLIT/"
  fi
done
for pkg in tds tapsdk tencent taptap zui vivo taptapshare unicom; do
  if [[ -d "$C2/com/$pkg" ]]; then
    mkdir -p "$CSPLIT/com"
    rm -rf "$CSPLIT/com/$pkg"
    mv "$C2/com/$pkg" "$CSPLIT/com/"
  fi
done

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
# starry 必须进已有最高连续 dex（此处应为 classes3），禁止新建 classes4
SMALI_DST="$PATCH/smali_classes3"
if [[ ! -d "$SMALI_DST" ]]; then
  if [[ -d "$PATCH/smali_classes2" ]]; then
    SMALI_DST="$PATCH/smali_classes2"
  elif [[ -d "$PATCH/smali" ]]; then
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
# 再清一次：避免上次 build/apk 残留或误建
for n in 4 5 6 7 8 9 10; do
  rm -rf "$PATCH/smali_classes$n"
  rm -f "$PATCH/build/apk/classes${n}.dex" 2>/dev/null || true
done

echo "[4/7] patch Manifest / Unity activities / TapSign / RootBeer"
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
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V

    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
"""
            if "invoke-super" in head2:
                head2 = head2.replace("invoke-super", early + "\n    invoke-super", 1)
            else:
                head2 = head2 + early
        elif "SignSpoof;->install" not in head2:
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

# Unity 主活动
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

# 新版 TapLogin：装有 TapTap 客户端时 isClientLogin=true → Client SSO。
# 改签后客户端 SSO 必挂；强制走 WebLogin（与消消英雄2同思路，路径/签名不同）。
clu = next(PATCH.glob("smali*/com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"), None)
if clu is None:
    clu = next(PATCH.glob("smali*/com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"), None)
if clu is None:
    raise SystemExit("ClientLoginUtils.smali missing (need force WebLogin)")
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
        # 旧 SDK：isClientLogin(Activity)Z
        new_icl_old = """.method public final isClientLogin(Landroid/app/Activity;)Z
    .locals 1

    # starry: force WebLogin
    const/4 v0, 0x0

    return v0
.end method"""
        s2, n = re.subn(
            r"\.method public final isClientLogin\(Landroid/app/Activity;\)Z[\s\S]*?\.end method",
            new_icl_old,
            s,
            count=1,
        )
    if n != 1:
        raise SystemExit(f"ClientLoginUtils.isClientLogin patch failed: {clu}")
    clu.write_text(s2, encoding="utf-8")
    print(f"sign: ClientLoginUtils → force WebLogin ({clu.relative_to(PATCH)})")
else:
    print("sign: ClientLoginUtils ok (force WebLogin)")

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

print("hooks done")
PY

echo "[5/7] (cstjjs: RoleValue_Health trampoline in native)"
echo "[6/7] apktool rebuild (ASCII, large)"
rm -f "$PATCH"/build/apk/classes*.dex 2>/dev/null || true
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + optional install"
# 默认 starry_release.jks（勿用 Debug：ColorOS 报高风险拦安装）
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
UBER_ARGS=(-a "$UNSIGNED" --overwrite --allowResign)
[[ -n "$BT_ZIPALIGN" ]] && UBER_ARGS+=(--zipAlignPath "$BT_ZIPALIGN")
if [[ -f "$KS" ]]; then
  UBER_ARGS+=(--ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS")
  echo "sign: RELEASE_CUSTOM $KS alias=$KS_ALIAS"
else
  echo "WARN: missing $KS — debug fallback (ColorOS 可能报高风险)"
fi
java -jar "$SIGNER" "${UBER_ARGS[@]}"
mkdir -p "$OUT"
SIGNED_SRC=""
for c in \
  "${UNSIGNED%.apk}-aligned-releaseSigned.apk" \
  "${UNSIGNED%.apk}-releaseSigned.apk" \
  "${UNSIGNED%.apk}-aligned-debugSigned.apk" \
  "${UNSIGNED%.apk}-debugSigned.apk" \
  "$UNSIGNED"; do
  if [[ -f "$c" ]]; then SIGNED_SRC="$c"; break; fi
done
cp -f "$SIGNED_SRC" "$SIGNED"
ls -lh "$SIGNED"
# 自检：0.9.9 登录完整性黑名单
python3.11 - <<PY
import zipfile, sys
from pathlib import Path
p = Path("$SIGNED")
z = zipfile.ZipFile(p)
bad = [
    "classes4.dex",
    "assets/overlay_integrity.dat",
    "lib/arm64-v8a/libbattlehook.so",
]
hit = [b for b in bad if b in z.namelist()]
dex = sorted(n for n in z.namelist() if n.startswith("classes") and n.endswith(".dex"))
print("verify: size", p.stat().st_size, "dex", dex)
if hit:
    print("ERROR: integrity artifacts present:", hit)
    sys.exit(1)
print("verify: no login-integrity artifacts")
PY
echo "patched tree → $FINAL_PATCH"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_cstjjs_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
adb shell am force-stop "$PKG" 2>/dev/null || true
if ! adb shell "su -c \"pm install -r -d -g --user 0 '$REMOTE_APK'\""; then
  echo "install failed → uninstall + reinstall"
  adb shell "su -c \"pm uninstall --user 0 '$PKG'\"" 2>/dev/null || adb uninstall "$PKG" 2>/dev/null || true
  adb shell "su -c \"pm install -r -d -g --user 0 '$REMOTE_APK'\""     || adb install --no-incremental "$SIGNED"
fi
adb shell "su -c \"rm -f '$REMOTE_APK'\"" 2>/dev/null || true
# 全游戏卡密种子 + 悬浮窗（勿删；换游戏 403 多因此）
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic Cstjjs Mod_Menu MenuService"
