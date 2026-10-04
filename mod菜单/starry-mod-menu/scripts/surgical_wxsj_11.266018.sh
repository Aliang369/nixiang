#!/usr/bin/env bash
# 无限升级 11.266018 (vc1637) — 外科组装（禁止对游戏 apktool b 整包）
# 方案：starry 菜单(classes3 合并) + WxsjMod(热更 dll 字节替换 hook)
# 产物：08_output/无限升级/无限升级_11.266018_surgical-releaseSigned.apk
#
# 用法:
#   bash scripts/surgical_wxsj_11.266018.sh
#   STARRY_INSTALL=1 bash scripts/surgical_wxsj_11.266018.sh
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
# 原包已清理时可指向设备拉取的已装包（如 /tmp/wxsj_v2/installed_base.apk）；
# 注意：换 so 时若 so 侧 native 接口有演进（如证书烘焙 19/19），必须让脚本走 gradle
# 模板重合入 com/starry smali（本脚本本来就会做），保证 so/Java 同源。
ORIG_APK="${WXSJ_ORIG_APK:-$NIXIANG/02_apks/无限升级/无限升级_11.266018_vc1637_base.apk}"
OUT="$NIXIANG/08_output/无限升级"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.xingzuodaoshi.infinityUpgrade"
SIGNED="$OUT/无限升级_11.266018_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/无限升级_11.266018_surgical-debugSigned.apk"
MF_BIN="$NIXIANG/07_patched/无限升级/surgical_11.266018/AndroidManifest.xml"
ORIG_MD5="40fb0d3074290c0fd60a0dd5303b1757"   # 官方包 cert MD5（aapt2/apksigner 实测）

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done

[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -f "$MF_BIN" ]] || { echo "ERROR: missing $MF_BIN"; exit 1; }
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_WXSJ' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_WXSJ"; exit 1; }
echo "[check] Android.mk target=wxsj"

LOCK_DIR="/tmp/wxsj_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wxsj_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/7] ndk-build -> jniLibs"
NDK_BUILD="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Wxsj\]' "$SO_BUILT"; then
  echo "ERROR: so 不是 WXSJ"; exit 1
fi
if grep -a -qE '\[Cwjh\]|\[Endfield\]|\[Zhmr\]' "$SO_BUILT"; then
  echo "ERROR: so 仍含上一款游戏标签"; exit 1
fi
# 嵌入 dll 校验
DLL_BIN="$STARRY_SRC/app/src/main/jni/targetfuntional_dll.bin"
[[ -f "$DLL_BIN" ]] || { echo "ERROR: missing $DLL_BIN (objcopy 源)"; exit 1; }
cmp -s "$DLL_BIN" "$NIXIANG/07_patched/无限升级/targetfuntional.dll" \
  || { echo "ERROR: 嵌入 dll 与 07_patched 产物不一致"; exit 1; }
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Wxsj] + embedded dll verified)"

echo "[1b/7] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
if ! grep -a -q '\[Wxsj\]' "$WORK/check.so"; then
  echo "ERROR: gradle APK 内 so 不是 WXSJ"; exit 1
fi

echo "[2/7] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/7] collect starry smali (merge into classes3)"
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
echo "  starry smali collected ($(find "$SMALI_ROOT" -name '*.smali' | wc -l | tr -d ' ') files)"
mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so" "$WORK/lib/arm64-v8a/"
NDK_CXX="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
[[ -f "$NDK_CXX" ]] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] patch classes3 (SignSpoof/MenuService/TapSign)"
unzip -p "$ORIG_APK" classes3.dex > "$WORK/classes3_orig.dex"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
"$BAKSMALI_BIN" d -o "$WORK/c3smali" "$WORK/classes3_orig.dex"
export ORIG_MD5
python3 - "$WORK/c3smali" <<'PY'
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
        print(f"  WARN TapSignKit patch failed: {tap}")
    else:
        tap.write_text(ts2, encoding="utf-8")
        print(f"  TapSignKit.getSign → {ORIG_MD5}")

# TapSDK 4.8.1: handler 选择器路由——禁掉 client SSO，落到 embedded web 登录
cli = root.glob("com/taptap/sdk/login/internal/handlers/client/ClientLoginHandler.smali")
for ch in cli:
    cs = ch.read_text(encoding="utf-8")
    if "starry: disable client SSO" in cs:
        print("  ClientLoginHandler ok")
        continue
    new_ch = """.method public canHandle(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;)Z
    .locals 1

    # starry: disable client SSO → fall through to embedded web login (改签包 SSO 签名校验必失败)
    const/4 v0, 0x0

    return v0
.end method"""
    cs2, n = re.subn(
        r"\.method public canHandle\(Landroid/app/Activity;Lcom/taptap/sdk/login/internal/bean/LoginRequest;\)Z[\s\S]*?\.end method",
        new_ch, cs, count=1,
    )
    if n != 1:
        raise SystemExit(f"ClientLoginHandler.canHandle patch failed: {ch}")
    ch.write_text(cs2, encoding="utf-8")
    print("  ClientLoginHandler.canHandle → false (web 登录兜底)")

clu = next(root.glob("com/taptap/sdk/login/impl/utils/ClientLoginUtils.smali"), None)
if clu is None:
    clu = next(root.glob("com/taptap/sdk/login/internal/util/ClientLoginUtils.smali"), None)
if clu is not None:
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
            # 本游戏 TapSDK 为旧版单 Activity 签名（internal/util/ClientLoginUtils）
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
            print("  WARN ClientLoginUtils patch failed (两版签名均未命中)")
        else:
            clu.write_text(s2, encoding="utf-8")
            print("  ClientLoginUtils → force WebLogin")
else:
    print("  ClientLoginUtils 不存在（本游戏主登录走自有 TCP，无需 patch）")

for p in root.rglob("UnityPlayerActivity.smali"):
    inject_activity(p, str(p.relative_to(root)))
PY
echo "[4b/7] merge starry smali into classes3"
rsync -a "$SMALI_ROOT/" "$WORK/c3smali/"
[[ -d "$WORK/c3smali/com/starry/magic" ]] || { echo "ERROR: starry merge failed"; exit 1; }
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$WORK/c3smali"

echo "[5/7] binary AndroidManifest"
cp "$MF_BIN" "$WORK/AndroidManifest.xml"
ls -la "$WORK/AndroidManifest.xml"

echo "[6/7] zip surgical merge"
cp -f "$ORIG_APK" "$WORK/unsigned.apk"
zip -d "$WORK/unsigned.apk" 'META-INF/*' >/dev/null || true
STAGE="$WORK/stage"
rm -rf "$STAGE"
mkdir -p "$STAGE/lib/arm64-v8a"
cp -f "$WORK/AndroidManifest.xml" "$WORK/classes3.dex" "$STAGE/"
cp -f "$WORK/lib/arm64-v8a/"* "$STAGE/lib/arm64-v8a/"
ZIP_ARGS=(AndroidManifest.xml classes3.dex lib/arm64-v8a/libstarry_magic.so)
[[ -f "$STAGE/lib/arm64-v8a/libc++_shared.so" ]] && ZIP_ARGS+=(lib/arm64-v8a/libc++_shared.so)
if [[ -d "$WORK/assets/certs" ]]; then
  mkdir -p "$STAGE/assets/certs"
  cp -R "$WORK/assets/certs/." "$STAGE/assets/certs/"
  while IFS= read -r -d '' f; do
    ZIP_ARGS+=("${f#"$STAGE/"}")
  done < <(find "$STAGE/assets/certs" -type f -print0)
fi
(cd "$STAGE" && zip -u "$WORK/unsigned.apk" "${ZIP_ARGS[@]}")

python3 - "$WORK/unsigned.apk" "$ORIG_APK" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path
dst, src = Path(sys.argv[1]), Path(sys.argv[2])
z = zipfile.ZipFile(dst)
names = set(z.namelist())
assert "lib/arm64-v8a/libstarry_magic.so" in names
c3 = z.read("classes3.dex")
assert b"com/starry/magic/StarryApp" in c3
zo = zipfile.ZipFile(src)
assert hashlib.sha256(zo.read("resources.arsc")).hexdigest() == hashlib.sha256(z.read("resources.arsc")).hexdigest()
assert hashlib.sha256(zo.read("classes.dex")).hexdigest() == hashlib.sha256(z.read("classes.dex")).hexdigest()
assert hashlib.sha256(zo.read("classes2.dex")).hexdigest() == hashlib.sha256(z.read("classes2.dex")).hexdigest()
assert hashlib.sha256(zo.read("lib/arm64-v8a/libil2cpp.so")).hexdigest() == hashlib.sha256(z.read("lib/arm64-v8a/libil2cpp.so")).hexdigest()
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Wxsj]" in so
print("  merged ok; arsc/classes1-2/libil2cpp intact; [Wxsj] so")
PY

echo "[7/7] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
"$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
rm -f "$WORK/unsigned.apk"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
"$BT/apksigner" sign \
  --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --out "$SIGNED" "$ALIGNED"
"$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -4

python3 - "$SIGNED" <<'PY'
import sys, zipfile
z = zipfile.ZipFile(sys.argv[1])
so = z.read("lib/arm64-v8a/libstarry_magic.so")
assert b"[Wxsj]" in so
c3 = z.read("classes3.dex")
assert b"com/starry/magic/menu/MenuService" in c3
print("  signed ok; starry merged in classes3")
PY
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  echo "[install] cover install"
  REMOTE=/data/local/tmp/starry_wxsj.apk
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if ! adb shell "su -c \"pm install -r -d --user 0 '$REMOTE'\""; then
    echo "ERROR: pm install failed (多半签名不一致，需用户明确同意卸载后再装)"
    exit 1
  fi
  adb shell "su -c \"rm -f '$REMOTE'\"" 2>/dev/null || true
fi
echo "DONE $SIGNED"
