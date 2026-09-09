#!/usr/bin/env bash
# 星光魔法 → 魂坠深境 1.1.0（vc41）官方原版：构建 + 注入 + SignSpoof + PROBE 探针 + 正式签 Root 安装
#
# 用途：1.1.0 invoke 桥 RVA 探针版（STARRY_HZSJ_PROBE），拿到 RVA 后再出正式包
# 用法:
#   bash scripts/inject_hzsj_1.1.0_probe.sh
#   STARRY_INSTALL=1 bash scripts/inject_hzsj_1.1.0_probe.sh
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
# 真机官方原版 1.1.0（vc41，cert 与 1.0.5 相同）
ORIG_APK="$NIXIANG/02_apks/魂坠深境/魂坠深境_1.1.0_device.apk"
SRC_DECODE="$NIXIANG/03_decompiled/魂坠深境/apktool_1.1.0_orig"
FINAL_PATCH="$NIXIANG/07_patched/魂坠深境/apktool_1.1.0_probe_modmenu"
OUT="$NIXIANG/08_output/魂坠深境"
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

PKG="com.chillyroom.containerofsoul.yw"
SIGNED="$OUT/魂坠深境_1.1.0_starry-probe-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/魂坠深境_1.1.0_starry-probe-debugSigned.apk"
# 官方 cert MD5（TapSign / 签名校验）— 1.1.0 实测与 1.0.5 一致
ORIG_MD5="a3e3c07b81aa322215f6315d7e60b151"

LOCK_DIR="/tmp/hzsj_inject.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: another hzsj inject holds $LOCK_DIR (pid $(cat "$LOCK_DIR/pid" 2>/dev/null || echo '?'))"
  exit 1
fi
echo $$ >"$LOCK_DIR/pid"

WORK="$(mktemp -d /tmp/hzsj_inject.XXXXXX)"
STARRY_BUILD="$WORK/starry"
PATCH="$FINAL_PATCH"
OUT_ASCII="$WORK/out"
UNSIGNED="$OUT_ASCII/hzsj_starry_unsigned.apk"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$NDK/ndk-build" ]] || { echo "ERROR: ndk-build missing at $NDK"; exit 1; }
# PROBE 版不要求源码树 mk 已切 HZSJ：rsync 到 /tmp 后用 python 切（不干扰源码树当前游戏）
echo "[check] mk switch happens in /tmp build copy (ENDFIELD→HZSJ+PROBE)"
mkdir -p "$OUT" "$OUT_ASCII" "$(dirname "$SRC_DECODE")" "$(dirname "$FINAL_PATCH")"

if [[ ! -d "$SRC_DECODE/smali" && ! -d "$SRC_DECODE/smali_classes2" ]]; then
  echo "[0a] apktool decode orig → $SRC_DECODE (~958MB APK, 久)"
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

echo "[0c/7] switch /tmp build copy mk: ENDFIELD → HZSJ + PROBE (源码树不动)"
python3.11 <<PY
from pathlib import Path
mk = Path("$STARRY_BUILD/app/src/main/jni/Android.mk")
t = mk.read_text()
t2 = t.replace(
    "LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ENDFIELD",
    "# (probe build) LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_ENDFIELD",
).replace(
    "LOCAL_SRC_FILES := Main.cpp Games/EndfieldMod.cpp Unity/Il2CppApi.cpp",
    "# (probe build) LOCAL_SRC_FILES := Main.cpp Games/EndfieldMod.cpp Unity/Il2CppApi.cpp",
)
assert "# (probe build) LOCAL_SRC_FILES := Main.cpp Games/EndfieldMod.cpp" in t2, "ENDFIELD block not found"
inc = "include " + chr(36) + "(BUILD_SHARED_LIBRARY)"
assert inc in t2, "BUILD_SHARED_LIBRARY include not found"
hzsj = (
    "\\n# 魂坠深境 1.1.0 PROBE 探针版（构建脚本自动切换）\\n"
    "LOCAL_CFLAGS    += -DENABLE_UNITY_HOOK -DSTARRY_GAME_HZSJ -DSTARRY_HZSJ_PROBE\\n"
    "LOCAL_SRC_FILES += Main.cpp Games/HzsjMod.cpp Unity/Il2CppApi.cpp\\n\\n"
)
t2 = t2.replace(inc, hzsj + inc, 1)
mk.write_text(t2)
print("mk switched: -DSTARRY_GAME_HZSJ -DSTARRY_HZSJ_PROBE (before BUILD_SHARED_LIBRARY)")
PY

# apktool 3.x 64k method/string：大包把非钩子包挪到 smali_classesN+
echo "[0b] split bulky smali packages if needed"
python3.11 <<PY
from pathlib import Path
import shutil
PATCH = Path("$PATCH")
cands = sorted(
    [p for p in PATCH.glob("smali_classes*") if p.is_dir()],
    key=lambda p: int(p.name.replace("smali_classes", "") or "0"),
)
if not cands:
    print("split: no smali_classes* — skip")
    raise SystemExit(0)
src = None
for p in reversed(cands):
    # prefer largest-looking class folder that has kotlin/okhttp
    if (p / "kotlin").is_dir() or (p / "okhttp3").is_dir() or (p / "com" / "bytedance").is_dir():
        src = p
        break
if src is None:
    src = cands[-1]
maxn = max(int(p.name.replace("smali_classes", "") or "0") for p in cands)
dst = PATCH / f"smali_classes{maxn + 1}"
moved = 0
for pkg in ("kotlin", "okhttp3", "okio", "io", "org"):
    s = src / pkg
    if s.is_dir():
        dst.mkdir(parents=True, exist_ok=True)
        t = dst / pkg
        if t.exists():
            shutil.rmtree(t)
        shutil.move(str(s), str(t))
        moved += 1
        print(f"split: {src.name}/{pkg} → {dst.name}/")
for pkg in ("bytedance", "ss", "kwad", "qq", "taptap", "tapsdk", "tencent", "alipay", "tradplus", "sigmob"):
    s = src / "com" / pkg
    if s.is_dir():
        (dst / "com").mkdir(parents=True, exist_ok=True)
        t = dst / "com" / pkg
        if t.exists():
            shutil.rmtree(t)
        shutil.move(str(s), str(t))
        moved += 1
        print(f"split: {src.name}/com/{pkg} → {dst.name}/com/")
print(f"split: moved={moved} from {src.name}")
PY

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
# 真机已改签包可能残留多份 com/starry，必须全部清掉再拷新菜单
shopt -s nullglob
for d in "$PATCH"/smali/com/starry "$PATCH"/smali_classes*/com/starry; do
  rm -rf "$d"
done
# 旧菜单 res 残留（去资源化后不再需要）
find "$PATCH/res" -name 'magic_*' -type f -delete 2>/dev/null || true
rm -rf "$PATCH/assets/web" 2>/dev/null || true
shopt -u nullglob
mkdir -p "$SMALI_DST/com"
[[ -d "$WORK/starry_dec/smali/com/starry" ]] && cp -R "$WORK/starry_dec/smali/com/starry" "$SMALI_DST/com/"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali_classes*/com/starry; do
  cp -R "$d" "$SMALI_DST/com/"
done
shopt -u nullglob
[[ -d "$SMALI_DST/com/starry/magic" ]] || { echo "ERROR: starry smali missing"; exit 1; }
echo "inject: starry → $(basename "$SMALI_DST")"

echo "[4/7] patch Manifest / BasePrivacy+BasePlayer / TapSign / RootBeer"
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

    invoke-static {p0}, Lcom/starry/magic/ota/StarryNativeLoad;->ensureLoaded(Landroid/content/Context;)Z
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

# 冷餐 / 隐私页 / Unity 主活动（寒舍 SDK）
names = (
    "BasePrivacyActivity.smali",
    "BasePlayerActivity.smali",
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
    raise SystemExit("no BasePrivacy/BasePlayer/Unity* activity smali found")
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

# 装有 TapTap 客户端时 isClientLogin=true → 客户端 SSO；改签后必挂。强制 WebLogin。
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

echo "[5/7] (hzsj: HybridCLR GenerateDamage in native)"
echo "[6/7] apktool rebuild (ASCII, large)"
rm -f "$PATCH"/build/apk/classes*.dex 2>/dev/null || true
apktool b -f "$PATCH" -o "$UNSIGNED"

echo "[7/7] sign + optional install"
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
echo "patched tree → $FINAL_PATCH"

if [[ "${STARRY_INSTALL:-0}" != "1" ]]; then
  echo "skip install (STARRY_INSTALL=1 to install)"
  echo "done → $SIGNED"
  exit 0
fi

REMOTE_APK="/data/local/tmp/starry_hzsj_install.apk"
adb push "$SIGNED" "$REMOTE_APK"
adb shell am force-stop "$PKG" 2>/dev/null || true
# 勿用 -g：避免授予 READ_LOGS（官方 Manifest 有声明）
if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE_APK'\""; then
  echo "install failed → uninstall + reinstall"
  adb shell "su -c \"pm uninstall --user 0 '$PKG'\"" 2>/dev/null || adb uninstall "$PKG" 2>/dev/null || true
  adb shell "su -c \"pm install -r -d --user 0 '$REMOTE_APK'\"" \
    || adb install --no-incremental "$SIGNED"
fi
adb shell "su -c \"pm revoke $PKG android.permission.READ_LOGS\"" 2>/dev/null || true
adb shell "su -c \"rm -f '$REMOTE_APK'\"" 2>/dev/null || true
_NIXIANG="$(cd "$(dirname "$0")/../../.." && pwd)"
# shellcheck source=/dev/null
source "$_NIXIANG/04_scripts/_common/starry_post_install.sh"
starry_post_install "$PKG"
adb shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
echo "done → $SIGNED"
echo "log: adb logcat -s StarryMagic Hzsj Mod_Menu MenuService"
