#!/usr/bin/env bash
# 王国大作战：前线 1.1.7646 TapTap — 外科 zip 组装（Kiwi VM StubApplication）
#
# - 保留官方 classes2.dex 字节不变；新增 classes5.dex（com/starry + WgdzzAppComponentFact）
# - binary Manifest 劫持 AppComponentFactory（Q1 签名校验前）
# - libstarry_magic.so + assets/certs
#
# 产物：08_output/王国大作战前线/王国大作战前线_1.1.7646_surgical-traceSigned.apk
#
# 用法:
#   STARRY_TRACE=1 bash scripts/surgical_wgdzz_1.1.7646.sh
#   STARRY_TRACE=1 STARRY_INSTALL=1 bash scripts/surgical_wgdzz_1.1.7646.sh
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
ORIG_APK="${WGDZZ_BASE_APK:-$NIXIANG/02_apks/王国大作战前线/base_1.1.7646.apk}"
OUT="$NIXIANG/08_output/王国大作战前线"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
WGDZZ_SO_DELAY_MS="${WGDZZ_SO_DELAY_MS:-15000}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.xr.wgdzztap"
TAG="trace"
[[ "${STARRY_TRACE:-0}" != "1" ]] && TAG="release"
WGDZZ_SWAP_MODE="${WGDZZ_SWAP_MODE:-1}"
[[ "$WGDZZ_SWAP_MODE" == "1" ]] && TAG="${TAG}-swap"
SIGNED="$OUT/王国大作战前线_1.1.7646_surgical-${TAG}Signed.apk"
MF_CACHE="$NIXIANG/07_patched/王国大作战前线/surgical_1.1.7646/AndroidManifest.xml"
[[ "$WGDZZ_SWAP_MODE" == "1" ]] && MF_CACHE="$NIXIANG/07_patched/王国大作战前线/surgical_1.1.7646/AndroidManifest.swap.xml"

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
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_WGDZZ' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_WGDZZ"; exit 1; }
echo "[check] Android.mk target=wgdzz STARRY_TRACE=${STARRY_TRACE:-0}"

LOCK_DIR="/tmp/wgdzz_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/wgdzz_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/6] ndk-build -> jniLibs"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
TRACE_ARG=()
[[ "${STARRY_TRACE:-0}" == "1" ]] && TRACE_ARG=(STARRY_TRACE=1)
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  ${TRACE_ARG+"${TRACE_ARG[@]}"} -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
if ! grep -a -q '\[Wgdzz\]' "$SO_BUILT" || ! grep -a -q 'wgdzz' "$SO_BUILT"; then
  echo "ERROR: so 不是 WGDZZ"
  strings "$SO_BUILT" | grep -E 'Wgdzz|wgdzz|\[Sc33\]' | sort -u | head -20
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
echo "  so -> jniLibs ok ([Wgdzz] verified)"

echo "[1b/6] gradle ${GRADLE_TASK}"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q '\[Wgdzz\]' "$WORK/check.so" || { echo "ERROR: gradle APK so 不是 WGDZZ"; exit 1; }

echo "[2/6] apktool d starry template"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null

echo "[3/6] classes5.dex = com/starry + WgdzzAppComponentFact（官方 classes2 不动）"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -d "$SMALI_ROOT/com/starry" ]] || { echo "ERROR: com/starry smali missing"; exit 1; }
cat > "$SMALI_ROOT/com/starry/magic/WgdzzSoLoaderRunnable.smali" <<SMALI
.class Lcom/starry/magic/WgdzzSoLoaderRunnable;
.super Ljava/lang/Object;
.source "WgdzzSoLoaderRunnable.java"

# interfaces
.implements Ljava/lang/Runnable;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method public static schedule()V
    .locals 2

    new-instance v0, Ljava/lang/Thread;

    new-instance v1, Lcom/starry/magic/WgdzzSoLoaderRunnable;

    invoke-direct {v1}, Lcom/starry/magic/WgdzzSoLoaderRunnable;-><init>()V

    invoke-direct {v0, v1}, Ljava/lang/Thread;-><init>(Ljava/lang/Runnable;)V

    invoke-virtual {v0}, Ljava/lang/Thread;->start()V

    return-void
.end method


# virtual methods
.method public run()V
    .locals 3

    :try_start_0
    const-wide/16 v0, ${WGDZZ_SO_DELAY_MS}L

    invoke-static {v0, v1}, Ljava/lang/Thread;->sleep(J)V
    :try_end_0
    .catch Ljava/lang/InterruptedException; {:try_start_0 .. :try_end_0} :catch_0

    :catch_0
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    const-string v0, "WgdzzSoLoader"

    const-string v1, "delayed loadLibrary ok (${WGDZZ_SO_DELAY_MS}ms)"

    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    return-void
.end method
SMALI
if [[ "$WGDZZ_SWAP_MODE" != "1" ]]; then
if [[ "${STARRY_TRACE:-0}" == "1" ]]; then
cat > "$SMALI_ROOT/com/starry/magic/WgdzzAppComponentFact.smali" <<'SMALI'
.class public Lcom/starry/magic/WgdzzAppComponentFact;
.super Landroidx/core/app/CoreComponentFactory;
.source "WgdzzAppComponentFact.java"


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Landroidx/core/app/CoreComponentFactory;-><init>()V

    return-void
.end method


# virtual methods
.method public instantiateApplication(Ljava/lang/ClassLoader;Ljava/lang/String;)Landroid/app/Application;
    .locals 2

    :try_start_0
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :catchall_0
    const-string v0, "starry_magic"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    const-string v0, "WgdzzAppComponentFact"

    const-string v1, "instantiateApplication SignSpoof+early-so"

    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    invoke-super {p0, p1, p2}, Landroidx/core/app/CoreComponentFactory;->instantiateApplication(Ljava/lang/ClassLoader;Ljava/lang/String;)Landroid/app/Application;

    move-result-object p1

    return-object p1
.end method
SMALI
else
cat > "$SMALI_ROOT/com/starry/magic/WgdzzAppComponentFact.smali" <<'SMALI'
.class public Lcom/starry/magic/WgdzzAppComponentFact;
.super Landroidx/core/app/CoreComponentFactory;
.source "WgdzzAppComponentFact.java"


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Landroidx/core/app/CoreComponentFactory;-><init>()V

    return-void
.end method


# virtual methods
.method public instantiateApplication(Ljava/lang/ClassLoader;Ljava/lang/String;)Landroid/app/Application;
    .locals 2

    :try_start_0
    invoke-static {}, Lcom/starry/magic/sign/SignSpoof;->install()V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :catchall_0
    invoke-static {}, Lcom/starry/magic/WgdzzSoLoaderRunnable;->schedule()V

    const-string v0, "WgdzzAppComponentFact"

    const-string v1, "instantiateApplication SignSpoof+delayed-so"

    invoke-static {v0, v1}, Landroid/util/Log;->i(Ljava/lang/String;Ljava/lang/String;)I

    invoke-super {p0, p1, p2}, Landroidx/core/app/CoreComponentFactory;->instantiateApplication(Ljava/lang/ClassLoader;Ljava/lang/String;)Landroid/app/Application;

    move-result-object p1

    return-object p1
.end method
SMALI
fi
else
  echo "  swap mode: skip WgdzzAppComponentFact (sidecar DexInstaller only)"
fi
echo "  WgdzzAppComponentFact.smali skipped=$([[ "$WGDZZ_SWAP_MODE" == "1" ]] && echo yes || echo no)"
"$SMALI_BIN" assemble -o "$WORK/classes5.dex" "$SMALI_ROOT"
ls -la "$WORK/classes5.dex"
ORIG_C2_SZ=$(unzip -l "$ORIG_APK" classes2.dex | awk '/classes2\.dex/{print $1}')
echo "  official classes2.dex kept byte-identical (${ORIG_C2_SZ} bytes)"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"

KW_SRC="${WGDZZ_KWPROTECT_SO:-}"
if [[ -n "$KW_SRC" && -f "$KW_SRC" ]]; then
  DLOPEN_PY="$SCRIPT_DIR/kwprotect_k1_starry_dlopen_patch.py"
  DLOPEN_SO="$WORK/libKwProtectSDK_dlopen.so"
  if [[ -f "$DLOPEN_PY" ]]; then
    echo "[3b] KwProtect K1 + dlopen(libstarry_magic) patch"
    python3.11 "$DLOPEN_PY" "$KW_SRC" "$DLOPEN_SO"
    export WGDZZ_KWPROTECT_SO="$DLOPEN_SO"
  fi
fi

if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] binary AndroidManifest (mini apktool)"
if [[ "$WGDZZ_SWAP_MODE" == "1" ]]; then
  rm -f "$MF_CACHE"
  python3.11 - "$ORIG_APK" "$WORK/AndroidManifest.xml" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
with zipfile.ZipFile(src) as zin:
    dst.write_bytes(zin.read("AndroidManifest.xml"))
print("  swap mode: official AndroidManifest.xml (CoreComponentFactory, no WgdzzAppComponentFact)")
PY
elif [[ -n "${WGDZZ_BINARY_MANIFEST:-}" && -f "$WGDZZ_BINARY_MANIFEST" ]]; then
  cp "$WGDZZ_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path
src, dst = Path(sys.argv[1]), Path(sys.argv[2])
keep = ("AndroidManifest.xml", "resources.arsc", "classes.dex")
zin = zipfile.ZipFile(src)
zout = zipfile.ZipFile(dst, "w")
for name in keep:
    info = zin.getinfo(name)
    data = zin.read(name)
    zi = zipfile.ZipInfo(name)
    zi.compress_type = zipfile.ZIP_STORED if name == "resources.arsc" else zipfile.ZIP_DEFLATED
    zout.writestr(zi, data)
zin.close(); zout.close()
print(f"  mini apk {dst.stat().st_size} bytes")
PY
  if apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null 2>&1; then
    python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
if 'android:name="com.starry.magic.StarryApp"' not in t:
    if 'android:name="com.kiwivm.security.StubApplication"' in t:
        t = t.replace(
            'android:name="com.kiwivm.security.StubApplication"',
            'android:name="com.starry.magic.StarryApp"',
            1,
        )
    else:
        t = t.replace("<application ", '<application android:name="com.starry.magic.StarryApp" ', 1)
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
print("  text Manifest patched (StarryApp + MenuService)")
PY
    apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
    unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  else
    echo "  apktool mini failed — Kiwi VM binary Manifest UTF-16 patch"
    python3.11 - "$ORIG_APK" "$WORK/AndroidManifest.xml" <<'PY'
import sys, zipfile
from pathlib import Path

src, dst = Path(sys.argv[1]), Path(sys.argv[2])
# Kiwi StubApplication 的 attachBaseContext/onCreate 均为 final native，不能子类化。
# 保持 Application=StubApplication；劫持 AppComponentFactory（Q1 channel 读签前加载）
old_acf = "androidx.core.app.CoreComponentFactory"
new_acf = "com.starry.magic.WgdzzAppComponentFact"
if len(new_acf) != len(old_acf):
    raise SystemExit(
        f"ERROR: AppComponentFactory name must be same length ({len(new_acf)} != {len(old_acf)})"
    )

def patch_utf16_once(data: bytearray, old: str, new: str, label: str) -> None:
    ob = old.encode("utf-16-le")
    nb = new.encode("utf-16-le")
    if len(nb) != len(ob):
        raise SystemExit(f"ERROR: {label} length mismatch {len(nb)} != {len(ob)}")
    idx = data.find(ob)
    if idx < 0:
        raise SystemExit(f"ERROR: {label} utf-16 not found")
    data[idx:idx + len(ob)] = nb
    print(f"  binary Manifest: {old} -> {new}")

with zipfile.ZipFile(src) as zin:
    data = bytearray(zin.read("AndroidManifest.xml"))
patch_utf16_once(data, old_acf, new_acf, "AppComponentFactory")
dst.write_bytes(data)
print("  NOTE: Application 保持 StubApplication；MenuService 未写入 binary Manifest")
PY
  fi
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/6] zip surgical merge"
export WGDZZ_SWAP_MODE
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, hashlib, os
from pathlib import Path

os.environ.setdefault("WGDZZ_SWAP_MODE", "1")

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c5 = (work / "classes5.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
kw_so = os.environ.get("WGDZZ_KWPROTECT_SO", "").strip()
if kw_so:
    kwp = Path(kw_so)
    if kwp.is_file():
        extras["lib/arm64-v8a/libKwProtectSDK.so"] = kwp.read_bytes()
        print(f"  override libKwProtectSDK.so <- {kwp} ({len(extras['lib/arm64-v8a/libKwProtectSDK.so'])} B)")

def is_protected(name: str) -> bool:
    return name in ("classes.dex", "classes2.dex", "classes3.dex", "classes4.dex", "resources.arsc")

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras or name == "classes5.dex":
        continue
    if name == "AndroidManifest.xml":
        data = mf
    else:
        data = zin.read(item)
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    n += 1

info = zipfile.ZipInfo("classes5.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c5)
for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    n += 1
zin.close(); zout.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in zout2.namelist():
    if is_protected(name):
        assert hashlib.sha256(zin2.read(name)).hexdigest() == hashlib.sha256(zout2.read(name)).hexdigest(), name
assert "classes5.dex" in zout2.namelist()
assert b"SignSpoof" in zout2.read("classes5.dex")
assert "lib/arm64-v8a/libstarry_magic.so" in zout2.namelist()
if os.environ.get("WGDZZ_SWAP_MODE", "1") != "1":
    assert b"WgdzzAppComponentFact" in zout2.read("classes5.dex")
else:
    print("  swap mode: classes5 may omit WgdzzAppComponentFact")
print(f"  merged ok; entries≈{n}; classes5={len(c5)}; dex2-4+arsc intact")
zin2.close(); zout2.close()
PY

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
if [[ -f "$KS" && -n "$BT" && -x "$BT/apksigner" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND=$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | head -1 || true)
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi

# A 路线：补全官方渠道 SigningBlock 0x42726577（1600B）并重签
if [[ "${WGDZZ_PRESERVE_CHANNEL:-0}" == "1" ]]; then
  PATCH_PY="$NIXIANG/work/wgdzz_b_route/preserve_channel_signblock.py"
  CHANNEL_OUT="$WORK/channel_extended.apk"
  if [[ -f "$PATCH_PY" ]]; then
    echo "[6b] preserve official channel block → re-sign"
    python3.11 "$PATCH_PY" --donor "$ORIG_APK" -i "$SIGNED" -o "$CHANNEL_OUT"
    "$BT/apksigner" sign \
      --ks "$KS" --ks-key-alias "$KS_ALIAS" \
      --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
      --out "$SIGNED" "$CHANNEL_OUT"
    python3.11 "$PATCH_PY" --donor "$ORIG_APK" -i "$SIGNED" -o "$WORK/ch_verify.apk" 2>&1 | tail -6
  else
    echo "WARN: missing $PATCH_PY, skip channel preserve"
  fi
fi

ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  REMOTE="/data/local/tmp/wgdzz_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  if adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'" 2>/dev/null; then
    echo "[install] cover ok"
    echo "[install] 清 jiagu 缓存（Manifest/dex 变更须 pm clear）"
    adb shell pm clear "$PKG" 2>/dev/null || true
  else
    echo "ERROR: 覆盖安装失败（签名与 TapTap 官方包不一致时常见）。"
    echo "  本游戏 ~509MB，默认禁止卸载。若必须换签，请聊天确认「可以卸载」后再装。"
    adb shell "rm -f $REMOTE" 2>/dev/null || true
    exit 2
  fi
  adb shell "rm -f $REMOTE" 2>/dev/null || true
  if [[ "${WGDZZ_POST_SWAP:-1}" == "1" ]]; then
    echo "[install] post-install swap official base.apk (root)"
    WGDZZ_SURGICAL_APK="$SIGNED" bash "$SCRIPT_DIR/wgdzz_post_install_swap.sh" || {
      echo "WARN: wgdzz_post_install_swap failed — run manually after install"
    }
  fi
  echo "log: adb logcat -c && adb logcat -s StarryTrace StarryMagic:V WgdzzDex:V MenuService:V"
fi

echo "DONE $SIGNED"
