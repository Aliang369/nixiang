#!/usr/bin/env bash
# 公主连结 11.7.2 — 外科组装（不动官方 classes0–12 / resources.arsc）
#
# 产物：08_output/公主连结/公主连结_11.7.2_surgical-releaseSigned.apk
# （不覆盖旧的 inject 整包 starry-releaseSigned.apk）
#
# 用法:
#   bash scripts/surgical_pcr_11.7.2.sh
#   STARRY_INSTALL=1 bash scripts/surgical_pcr_11.7.2.sh
#
# 依赖：Android.mk 已启用 STARRY_GAME_PCR；JDK 17；smali-2.5.2.jar
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
ORIG_APK="$NIXIANG/02_apks/公主连结/公主连结_11.7.2_official.apk"
DEC_MF="$NIXIANG/03_decompiled/公主连结/apktool_11.7.2_orig/AndroidManifest.xml"
OUT="$NIXIANG/08_output/公主连结"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
SMALI_JAR="$NIXIANG/01_tools/smali/smali-2.5.2.jar"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"
KS="$NIXIANG/01_tools/starry_release.jks"
PKG="com.bilibili.priconne"
SIGNED="$OUT/公主连结_11.7.2_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/公主连结_11.7.2_surgical-debugSigned.apk"

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
MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_PCR' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_PCR (see SWITCH_GAME.md)"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DSTARRY_GAME_SELF' "$MK" \
  && { echo "ERROR: STARRY_GAME_SELF still enabled"; exit 1; }
echo "[check] Android.mk target=pcr"

LOCK_DIR="/tmp/pcr_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/pcr_surgical.XXXXXX)"
trap 'rm -rf "$WORK"; rm -rf "$LOCK_DIR"' EXIT
mkdir -p "$OUT" "$WORK"

echo "[1/6] gradle $GRADLE_TASK + ndk (starry)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  "$STARRY_SRC/" "$WORK/starry/"
# CI-friendly: so from ndk into jniLibs if inject script pattern needed
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }

echo "[2/6] apktool d starry → smali + so + certs"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
[[ -d "$WORK/starry_dec/smali" ]] || [[ -d "$WORK/starry_dec/smali_classes2" ]] \
  || { echo "ERROR: no smali in starry decode"; exit 1; }

echo "[3/6] collect com/starry smali → classes13.dex"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
# apktool 可能把类拆到多个 smali*
shopt -s nullglob
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
  [[ -d "$d/com/starry" ]] || continue
  rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
shopt -u nullglob
[[ -d "$SMALI_ROOT/com/starry" ]] || { echo "ERROR: com/starry smali missing"; exit 1; }
[[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ]] \
  || { echo "ERROR: StarryApp.smali missing — rebuild after adding StarryApp.java"; exit 1; }
# 运行时必须继承官方 App，否则 PermissionActivity/GSC Router 未 init 秒崩
python3.11 - "$SMALI_ROOT/com/starry/magic/StarryApp.smali" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
repls = [
    (".super Landroid/app/Application;", ".super Lcom/bilibili/priconne/App;"),
    (
        "invoke-direct {p0}, Landroid/app/Application;-><init>()V",
        "invoke-direct {p0}, Lcom/bilibili/priconne/App;-><init>()V",
    ),
    (
        "invoke-super {p0, p1}, Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V",
        "invoke-super {p0, p1}, Lcom/bilibili/priconne/App;->attachBaseContext(Landroid/content/Context;)V",
    ),
    (
        "invoke-super {p0}, Landroid/app/Application;->onCreate()V",
        "invoke-super {p0}, Lcom/bilibili/priconne/App;->onCreate()V",
    ),
]
changed = 0
for a, b in repls:
    if a in t:
        t = t.replace(a, b)
        changed += 1
        print(f"  patch: {a[:48]}…")
if "Lcom/bilibili/priconne/App;" not in t:
    raise SystemExit("ERROR: StarryApp.smali missing official App references")
if "Landroid/app/Application;" in t:
    raise SystemExit("ERROR: StarryApp.smali still references Landroid/app/Application;")
p.write_text(t, encoding="utf-8")
print(f"  StarryApp.smali official App wiring ok ({changed} edits)")
PY
"$SMALI_BIN" assemble -o "$WORK/classes13.dex" "$SMALI_ROOT"
ls -la "$WORK/classes13.dex"

SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/6] binary AndroidManifest"
# 默认用已校验 ID 稳定的预编译 Manifest（避免每次 apktool b 1.8G）
DEFAULT_MF="$NIXIANG/07_patched/公主连结/surgical/AndroidManifest.xml"
if [[ -n "${PCR_BINARY_MANIFEST:-}" && -f "$PCR_BINARY_MANIFEST" ]]; then
  cp "$PCR_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using PCR_BINARY_MANIFEST=$PCR_BINARY_MANIFEST"
elif [[ -f "$DEFAULT_MF" ]]; then
  cp "$DEFAULT_MF" "$WORK/AndroidManifest.xml"
  echo "  using cached $DEFAULT_MF"
else
  echo "ERROR: missing binary Manifest. Generate once:"
  echo "  （已有流程）改 decode 的 AndroidManifest → apktool b → unzip AndroidManifest.xml"
  echo "  → $DEFAULT_MF"
  exit 1
fi
ls -la "$WORK/AndroidManifest.xml"
echo "[5/6] zip surgical merge (keep official dex0-12 + arsc byte-identical)"
python3.11 - "$ORIG_APK" "$WORK" "$WORK/unsigned.apk" <<'PY'
import sys, zipfile, os
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c13 = (work / "classes13.dex").read_bytes()
extras = {}
for p in (work / "lib").rglob("*"):
    if p.is_file():
        extras["lib/" + str(p.relative_to(work / "lib")).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in certs.rglob("*"):
        if p.is_file():
            rel = "assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")
            extras[rel] = p.read_bytes()

PROTECTED_PREFIX = tuple(f"classes{i}.dex" for i in range(0, 13))  # classes.dex .. classes12.dex
# classes.dex has no number
def is_protected_dex(name: str) -> bool:
    if name == "classes.dex":
        return True
    if name.startswith("classes") and name.endswith(".dex"):
        # classes2..classes12
        mid = name[len("classes"):-len(".dex")]
        if mid.isdigit() and 2 <= int(mid) <= 12:
            return True
    return False

zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue  # replace below
    if name == "AndroidManifest.xml":
        data = mf
    elif name == "classes13.dex":
        continue  # write fresh below
    else:
        data = zin.read(item)
        # 完整性：受保护 dex / arsc 原样写入
        if name == "resources.arsc" or is_protected_dex(name):
            pass
    info = zipfile.ZipInfo(name, item.date_time)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
    elif name == "AndroidManifest.xml":
        # 官方多为 DEFLATED；保持可压
        info.compress_type = zipfile.ZIP_DEFLATED
    else:
        info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, data)
    seen.add(name)
    n += 1

# classes13
info = zipfile.ZipInfo("classes13.dex")
info.compress_type = zipfile.ZIP_DEFLATED
info.create_system = 3
zout.writestr(info, c13)
seen.add("classes13.dex")

for name, data in sorted(extras.items()):
    info = zipfile.ZipInfo(name)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 3
    zout.writestr(info, data)
    seen.add(name)
    n += 1

zin.close()
zout.close()
# 自检：官方受保护文件哈希一致
import hashlib
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
for name in list(zin2.namelist()):
    if name == "resources.arsc" or is_protected_dex(name):
        h1 = hashlib.sha256(zin2.read(name)).hexdigest()
        h2 = hashlib.sha256(zout2.read(name)).hexdigest()
        assert h1 == h2, f"PROTECTED MUTATED: {name}"
print(f"  merged ok; entries≈{n}; classes13={len(c13)} bytes; protected sha256 match")
zin2.close()
zout2.close()
PY

echo "[6/6] zipalign + sign → $SIGNED"
ALIGNED="$WORK/aligned.apk"
if [[ -n "$BT" ]]; then
  "$BT/zipalign" -f -p 4 "$WORK/unsigned.apk" "$ALIGNED"
else
  cp "$WORK/unsigned.apk" "$ALIGNED"
fi
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
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
ls -lh "$SIGNED"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  # 默认覆盖安装。公主连结卸载会清掉约 10GB 热更数据，勿默认 uninstall。
  # 仅当用户明确同意时：STARRY_CLEAN_INSTALL=1
  REMOTE="/data/local/tmp/pcr_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  if [[ "${STARRY_CLEAN_INSTALL:-0}" == "1" ]]; then
    echo "[install] CLEAN uninstall (user opted in via STARRY_CLEAN_INSTALL=1)"
    adb shell "pm uninstall $PKG" 2>/dev/null || true
  else
    echo "[install] cover install -r -d (no uninstall; preserves game data)"
    adb shell am force-stop "$PKG" 2>/dev/null || true
  fi
  adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
    || adb install -r "$SIGNED"
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Pcr:V MenuService:V StarrySignSpoof:V"
fi

echo "DONE: $SIGNED"
echo "NOTE: do NOT use inject_pcr_11.7.2.sh for TSS-safe builds."
