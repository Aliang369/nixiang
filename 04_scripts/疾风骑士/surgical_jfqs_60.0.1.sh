#!/usr/bin/env bash
# =============================================================================
# 疾风骑士 60.0.1 — 外科组装（官方 classes*.dex / resources.arsc / lib*.so 字节不变）
#
# 目标包：com.emay.wxsy.topon（TapTap + MAR SDK + TopOn 渠道，Unity IL2CPP）
# 注入路线：(c) application@name → com.starry.magic.StarryApp extends
#           com.mar.sdk.MARApplication（同 MAR SDK 家族先例 surgical_bjdw_1.0.1.sh）
#
# dex 落位（官方 15 dex: classes.dex .. classes14.dex，最小空闲下标 = 15）：
#   classes9.dex  ← 改写（MarMainActivity onResume / onUserLeaveHint → MenuService）
#   classes14.dex ← 改写（TapSignKit.getSign + ClientLoginUtils.isClientLogin）
#   classes15.dex ← 新增（com/starry 菜单 smali）
#   其余全部 dex + resources.arsc + lib/** 字节保留
#
# 用法：
#   bash 04_scripts/疾风骑士/surgical_jfqs_60.0.1.sh
#   STARRY_INSTALL=1 bash 04_scripts/疾风骑士/surgical_jfqs_60.0.1.sh
# =============================================================================
set -euo pipefail

GAME="疾风骑士"
VER="60.0.1"
TAG="Jfqs"
GAME_ID="jfqs"
PKG="com.emay.wxsy.topon"

NEW_DEX="classes15.dex"
PATCH_DEX_TAP="classes14.dex"
PATCH_DEX_ACT="classes9.dex"

# 受保护 dex（除上面两个改写 dex 外全部官方 dex）
PROTECTED_DEX="classes.dex classes2.dex classes3.dex classes4.dex classes5.dex \
classes6.dex classes7.dex classes8.dex classes10.dex classes11.dex classes12.dex \
classes13.dex"

NIXIANG="/Users/aliang/Documents/nixiang"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"

ORIG_APK="${STARRY_ORIG_APK:-$NIXIANG/02_apks/疾风骑士_60.0.1_原版.apk}"
ORIG_MD5="033c55fec862c39d2d1c5723e56377d0"
SIGN_MD5="e28a5f390ab1a7590a032f05b3fde72a"

OUT="$NIXIANG/08_output/$GAME"
MF_CACHE="$NIXIANG/07_patched/$GAME/surgical_$VER/AndroidManifest.xml"
SO_PREBUILT="${JFQS_SO:-$STARRY_SRC/libs_$GAME_ID/arm64-v8a/libstarry_magic.so}"

SMALI_BIN="$NIXIANG/01_tools/smali/smali"
BAKSMALI_BIN="$NIXIANG/01_tools/smali/baksmali"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
SIGNER_SMALI="$NIXIANG/04_scripts/$GAME/patch_jfqs_smali.py"

NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_BUILD="$NDK/ndk-build"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"

SERIAL="${ANDROID_SERIAL:-f9a116a2}"
BUILD_TYPE="${STARRY_BUILD_TYPE:-release}"
if [ "$BUILD_TYPE" = "debug" ]; then GRADLE_TASK="assembleDebug"; APK_NAME="app-debug.apk"; SUF="debug"
else GRADLE_TASK="assembleRelease"; APK_NAME="app-release.apk"; SUF="release"; fi
SIGNED="$OUT/${GAME}_${VER}_surgical-${SUF}Signed.apk"
TMP_UNSIGNED="$OUT/.tmp_unsigned.apk"
TMP_ALIGNED="$OUT/.tmp_aligned.apk"

LOCK_DIR="/tmp/jfqs_surgical.lockdir"

export JAVA_HOME="$(/usr/libexec/java_home -v 17)"
export PATH="$JAVA_HOME/bin:$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
    if [ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done

NPROC="$(sysctl -n hw.ncpu 2>/dev/null || echo 4)"

# ---------------------------------------------------------------- preflight ---
# 注意：$SO_PREBUILT 不在此列 —— 它由 [1/6] ndk-build 产出，其后（L121）再校验。
for f in "$ORIG_APK" "$SMALI_BIN" "$BAKSMALI_BIN" "$SIGNER_SMALI" "$MF_CACHE"; do
    [ -e "$f" ] || { echo "ERROR: missing $f" >&2; exit 1; }
done
command -v python3.11 >/dev/null || { echo "ERROR: python3.11 required" >&2; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool required" >&2; exit 1; }
[ -n "$BT" ] || { echo "ERROR: no build-tools with zipalign" >&2; exit 1; }

MK="$STARRY_SRC/app/src/main/jni/Android_$GAME_ID.mk"
[ -f "$MK" ] || { echo "ERROR: $MK not found" >&2; exit 1; }
grep -q "Games/${TAG}Mod.cpp" "$MK" || { echo "ERROR: $MK 未引用 Games/${TAG}Mod.cpp" >&2; exit 1; }

GOT_MD5="$(md5 -q "$ORIG_APK")"
[ "$GOT_MD5" = "$ORIG_MD5" ] || { echo "ERROR: ORIG_APK md5=$GOT_MD5 want $ORIG_MD5" >&2; exit 1; }

FREE_DEX="classes$(python3.11 "$NIXIANG/04_scripts/_common/next_free_dex.py" "$ORIG_APK").dex"
[ "$FREE_DEX" = "$NEW_DEX" ] || { echo "ERROR: free dex=$FREE_DEX want $NEW_DEX" >&2; exit 1; }

AVAIL_G="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[ "$AVAIL_G" -ge 6 ] || { echo "ERROR: 磁盘可用 ${AVAIL_G}G < 6G" >&2; exit 1; }

if ! mkdir "$LOCK_DIR" 2>/dev/null; then
    echo "ERROR: 另一实例在跑（$LOCK_DIR，pid $(cat "$LOCK_DIR/pid" 2>/dev/null)）" >&2; exit 1
fi
echo $$ > "$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/jfqs_surgical.XXXXXX)"
cleanup() { rm -rf "$WORK" "$LOCK_DIR"; }
trap cleanup EXIT

mkdir -p "$OUT"
echo "[jfqs] ORIG=$ORIG_APK ($ORIG_MD5)"
echo "[jfqs] NEW_DEX=$NEW_DEX  PATCH: $PATCH_DEX_ACT $PATCH_DEX_TAP  WORK=$WORK"

# ------------------------------------------------------------------ [1/6] ----
echo "[1/6] ndk-build -> libs_$GAME_ID"
( cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT="./Android_$GAME_ID.mk" \
    NDK_APPLICATION_MK=./Application.mk \
    NDK_OUT="$STARRY_SRC/obj_$GAME_ID" NDK_LIBS_OUT="$STARRY_SRC/libs_$GAME_ID" -j"$NPROC" ) \
  2>&1 | grep -v '^fcntl()' || true

[ -f "$SO_PREBUILT" ] || { echo "ERROR: 构建未产出 $SO_PREBUILT" >&2; exit 1; }
LC_ALL=C grep -aq '\[jfqs\]' "$SO_PREBUILT" || { echo "ERROR: so 不含 [jfqs]" >&2; exit 1; }
LC_ALL=C grep -aq 'jfqs' "$SO_PREBUILT" || { echo "ERROR: so 不含 jfqs" >&2; exit 1; }
LC_ALL=C grep -aq '百分百改造成功' "$SO_PREBUILT" || { echo "ERROR: so 不含菜单门禁串" >&2; exit 1; }
if LC_ALL=C grep -aq '\[Bjdw\]' "$SO_PREBUILT"; then echo "ERROR: so 残留上一款游戏 [Bjdw]" >&2; exit 1; fi
mkdir -p "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" "$STARRY_SRC/app/src/main/libs/arm64-v8a" "$STARRY_SRC/app/libs/arm64-v8a"
cp -f "$SO_PREBUILT" "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a/libstarry_magic.so"
cp -f "$SO_PREBUILT" "$STARRY_SRC/app/src/main/libs/arm64-v8a/libstarry_magic.so"
cp -f "$SO_PREBUILT" "$STARRY_SRC/app/libs/arm64-v8a/libstarry_magic.so"
echo "    so ok: $(md5 -q "$SO_PREBUILT")"

# ----------------------------------------------------------------- [1b/6] ----
echo "[1b/6] gradle $GRADLE_TASK"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj_*/' --exclude 'libs_*/' \
  "$STARRY_SRC/" "$WORK/starry/"
( cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain -q ) 2>&1 | tail -20 || true
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[ -f "$STARRY_APK" ] || { echo "ERROR: gradle 未产出 $STARRY_APK" >&2; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/so_check.bin" 2>/dev/null || true
LC_ALL=C grep -aq '\[jfqs\]' "$WORK/so_check.bin" || { echo "ERROR: gradle APK 内 so 不是 jfqs" >&2; exit 1; }
echo "    starry apk ok"

# ------------------------------------------------------------------ [2/6] ----
echo "[2/6] apktool d starry → smali + so"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null 2>&1

# ------------------------------------------------------------------ [3/6] ----
echo "[3/6] collect com/starry smali → ${NEW_DEX}（route (c): StarryApp.super → MARApplication）"
SMALI_ROOT="$WORK/smali_in"
mkdir -p "$SMALI_ROOT"
for d in "$WORK/starry_dec"/smali "$WORK/starry_dec"/smali_classes*; do
    [ -d "$d/com/starry" ] || continue
    rsync -a "$d/com/starry" "$SMALI_ROOT/com/"
done
[ -f "$SMALI_ROOT/com/starry/magic/StarryApp.smali" ] || { echo "ERROR: StarryApp.smali missing" >&2; exit 1; }
[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ] || { echo "ERROR: MenuService.smali missing" >&2; exit 1; }
[ -f "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" ] || { echo "ERROR: SignSpoof.smali missing" >&2; exit 1; }
grep -q "$PKG" "$SMALI_ROOT/com/starry/magic/sign/SignSpoof.smali" || { echo "ERROR: SignSpoof 不含 $PKG" >&2; exit 1; }

python3.11 - "$SMALI_ROOT" <<'PY'
import sys, pathlib
root = pathlib.Path(sys.argv[1])
sa = root / "com/starry/magic/StarryApp.smali"
t = sa.read_text(encoding="utf-8", errors="surrogateescape")
if "Lcom/mar/sdk/MARApplication;" not in t:
    n = t.count(".super Landroid/app/Application;")
    if n != 1:
        raise SystemExit("StarryApp.super Application count=%d (want 1)" % n)
    t = t.replace(".super Landroid/app/Application;", ".super Lcom/mar/sdk/MARApplication;", 1)
    t = t.replace("Landroid/app/Application;->", "Lcom/mar/sdk/MARApplication;->")
    sa.write_text(t, encoding="utf-8", errors="surrogateescape")
    print("    StarryApp.super → Lcom/mar/sdk/MARApplication;")
else:
    print("    StarryApp.super already MARApplication")
c = sa.read_text(encoding="utf-8", errors="surrogateescape")
if "Landroid/app/Application;" in c:
    raise SystemExit("StarryApp 残留 Landroid/app/Application;")
if "Lcom/mar/sdk/MARApplication;" not in c:
    raise SystemExit("StarryApp 缺 Lcom/mar/sdk/MARApplication;")
PY

"$SMALI_BIN" assemble -o "$WORK/$NEW_DEX" "$SMALI_ROOT" || { echo "ERROR: smali assemble 失败" >&2; exit 1; }
[ -f "$WORK/$NEW_DEX" ] || { echo "ERROR: $NEW_DEX 未产出" >&2; exit 1; }
echo "    $NEW_DEX = $(stat -f%z "$WORK/$NEW_DEX") bytes"

mkdir -p "$WORK/lib/arm64-v8a"
cp -f "$SO_PREBUILT" "$WORK/lib/arm64-v8a/libstarry_magic.so"
[ -f "$NDK_CXX" ] && cp -f "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"

# ----------------------------------------------------------------- [3b/6] ----
echo "[3b/6] patch $PATCH_DEX_TAP : TapSign MD5 + force Tap WebLogin"
mkdir -p "$WORK/tap"
"$BAKSMALI_BIN" d "$ORIG_APK" -o "$WORK/tap" 2>/dev/null || true
# 官方 15 dex：逐个试解，找到含 TapSignKit 的那个（预期 classes14）
DEC_TAP=""
for i in $(seq 1 15); do
    d="$WORK/tap$i"
    mkdir -p "$d"
    if unzip -p "$ORIG_APK" "classes$i.dex" > "$WORK/c$i.dex" 2>/dev/null && [ -s "$WORK/c$i.dex" ]; then
        "$BAKSMALI_BIN" d "$WORK/c$i.dex" -o "$d" >/dev/null 2>&1 || true
        if [ -f "$d/com/taptap/sdk/kit/internal/utils/TapSignKit.smali" ]; then DEC_TAP="$d"; TAP_IDX="$i"; break; fi
    fi
done
[ -n "$DEC_TAP" ] || { echo "ERROR: TapSignKit.smali not found in any dex" >&2; exit 1; }
echo "    TapSignKit in classes$TAP_IDX.dex"
python3.11 "$SIGNER_SMALI" tap "$DEC_TAP" "$SIGN_MD5"
"$SMALI_BIN" assemble -o "$WORK/$PATCH_DEX_TAP" "$DEC_TAP" || { echo "ERROR: assemble $PATCH_DEX_TAP" >&2; exit 1; }

# ----------------------------------------------------------------- [3c/6] ----
echo "[3c/6] patch $PATCH_DEX_ACT : MarMainActivity onResume/onUserLeaveHint → MenuService"
DEC_ACT=""
for i in $(seq 1 15); do
    d="$WORK/act$i"
    mkdir -p "$d"
    if [ -f "$WORK/c$i.dex" ]; then
        "$BAKSMALI_BIN" d "$WORK/c$i.dex" -o "$d" >/dev/null 2>&1 || true
        if [ -f "$d/com/mar/sdk/MarMainActivity.smali" ]; then DEC_ACT="$d"; ACT_IDX="$i"; break; fi
    fi
done
[ -n "$DEC_ACT" ] || { echo "ERROR: MarMainActivity.smali not found" >&2; exit 1; }
echo "    MarMainActivity in classes$ACT_IDX.dex"
python3.11 "$SIGNER_SMALI" activity "$DEC_ACT"
"$SMALI_BIN" assemble -o "$WORK/$PATCH_DEX_ACT" "$DEC_ACT" || { echo "ERROR: assemble $PATCH_DEX_ACT" >&2; exit 1; }

# ------------------------------------------------------------------ [4/6] ----
echo "[4/6] binary AndroidManifest"
cp -f "$MF_CACHE" "$WORK/AndroidManifest.xml"
echo "    manifest md5=$(md5 -q "$WORK/AndroidManifest.xml")"

# ------------------------------------------------------------------ [5/6] ----
echo "[5/6] zip surgical merge"
python3.11 - "$ORIG_APK" "$WORK" "$TMP_UNSIGNED" "$NEW_DEX" "$PATCH_DEX_TAP" "$PATCH_DEX_ACT" "$PROTECTED_DEX" "$SIGN_MD5" <<'PY'
import hashlib, os, shutil, sys, zipfile

ORIG, WORK, OUTP, NEW_DEX, PTAP, PACT, PROT_STR, SIGN_MD5 = sys.argv[1:9]
PROTECTED = [x for x in PROT_STR.split() if x]
REPLACED = {PTAP, PACT}

def sha(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()

def stream_sha(zf, name):
    h = hashlib.sha256()
    with zf.open(name) as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()

mf = open(os.path.join(WORK, "AndroidManifest.xml"), "rb").read()
dex = {}
for n in (NEW_DEX, PTAP, PACT):
    p = os.path.join(WORK, n)
    dex[n] = open(p, "rb").read()

extras = {}
for base, _, files in os.walk(os.path.join(WORK, "lib")):
    for f in files:
        p = os.path.join(base, f)
        extras[os.path.relpath(p, WORK)] = p
acerts = os.path.join(WORK, "assets", "certs")
if os.path.isdir(acerts):
    for base, _, files in os.walk(acerts):
        for f in files:
            p = os.path.join(base, f)
            extras[os.path.relpath(p, WORK)] = p

class RawNameZipInfo(zipfile.ZipInfo):
    """保留官方条目名原始字节（Unity AssetBundle 名常是 UTF-8 字节但不带 0x800 flag）。"""
    def __init__(self, item):
        super().__init__(item.filename, item.date_time)
        self._raw_name = item.orig_filename.encode("cp437", "replace")
        self._out_flags = item.flag_bits & ~0x08
        self.compress_type = item.compress_type
        self.external_attr = item.external_attr
        self.internal_attr = item.internal_attr
        self.create_system = 3
    def _encodeFilenameFlags(self):
        return self._raw_name, self._out_flags

src_sha = {}
with zipfile.ZipFile(ORIG, "r") as zin:
    for nm in PROTECTED + ["resources.arsc"]:
        src_sha[nm] = stream_sha(zin, nm)

names = set()
n_out = 0
with zipfile.ZipFile(ORIG, "r") as zin, zipfile.ZipFile(OUTP, "w", zipfile.ZIP_DEFLATED) as zout:
    for item in zin.infolist():
        nm = item.filename
        if nm.startswith("META-INF/"):
            continue
        if nm in extras or nm in dex or nm in REPLACED:
            continue
        if nm == "AndroidManifest.xml":
            info = RawNameZipInfo(item)
            info.compress_type = zipfile.ZIP_DEFLATED
            zout.writestr(info, mf)
            names.add(nm); n_out += 1
            continue
        if nm == "resources.arsc":
            info = RawNameZipInfo(item)
            info.compress_type = zipfile.ZIP_STORED
            zout.writestr(info, zin.read(nm))
            names.add(nm); n_out += 1
            continue
        if nm in names:
            raise SystemExit("版本假设失效（重名条目 %s）" % nm)
        info = RawNameZipInfo(item)
        with zin.open(nm) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
        names.add(nm); n_out += 1

    for nm in sorted(dex):
        info = zipfile.ZipInfo(nm, (2026, 10, 2, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        info.external_attr = 0o100644 << 16
        zout.writestr(info, dex[nm])
        names.add(nm); n_out += 1

    for nm in sorted(extras):
        info = zipfile.ZipInfo(nm, (2026, 10, 2, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        info.create_system = 3
        info.external_attr = 0o100644 << 16
        zout.writestr(info, open(extras[nm], "rb").read())
        names.add(nm); n_out += 1

# --- 自检 -------------------------------------------------------------------
with zipfile.ZipFile(OUTP, "r") as zf:
    for nm, want in src_sha.items():
        got = stream_sha(zf, nm)
        if got != want:
            raise SystemExit("PROTECTED MUTATED: %s" % nm)
    for nm in (NEW_DEX, PTAP, PACT):
        if nm not in zf.namelist():
            raise SystemExit("missing %s in output" % nm)
    if "AndroidManifest.xml" not in zf.namelist():
        raise SystemExit("missing AndroidManifest.xml")
    if zf.getinfo("resources.arsc").compress_type != zipfile.ZIP_STORED:
        raise SystemExit("resources.arsc not STORED")
    b5 = zf.read(NEW_DEX)
    if b"SignSpoof" not in b5:
        raise SystemExit("SignSpoof missing in %s" % NEW_DEX)
    if b"MenuService" not in b5:
        raise SystemExit("MenuService missing in %s" % NEW_DEX)
    if b"notifyHostResumed" not in zf.read(PACT):
        raise SystemExit("notifyHostResumed missing in %s" % PACT)
    # 注意：smali 的 `# ...` 注释不会被汇编进 dex（只进 .rodata 之前的文本层），
    # 所以不能用 "# starry_sign_bypass" 当门禁串 —— 必须验真实字面量：
    # TapSignKit.getSign 被改成 const-string v0, "<SIGN_MD5>"，该 ASCII 串一定落在 dex 字符串池。
    if SIGN_MD5.encode() not in zf.read(PTAP):
        raise SystemExit("SIGN_MD5 %s missing in %s (TapSignKit patch 未落进 dex)" % (SIGN_MD5, PTAP))

print("    merged ok; entries=%d; %s=%d %s=%d %s=%d"
      % (n_out, NEW_DEX, len(dex[NEW_DEX]), PTAP, len(dex[PTAP]), PACT, len(dex[PACT])))
PY

# ------------------------------------------------------------------ [6/6] ----
echo "[6/6] zipalign + sign → $SIGNED"
"$BT/zipalign" -f -p 4 "$TMP_UNSIGNED" "$TMP_ALIGNED"
"$BT/zipalign" -c 4 "$TMP_ALIGNED" || { echo "ERROR: zipalign -c failed" >&2; exit 1; }
rm -f "$TMP_UNSIGNED"

if "$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
        --ks-pass "pass:$KS_PASS" --v4-signing-enabled false \
        --out "$SIGNED" "$TMP_ALIGNED" 2>/dev/null; then
    :
else
    echo "    apksigner 不可用，回退 uber-apk-signer"
    java -jar "$SIGNER" -a "$TMP_ALIGNED" --allowResign -o "$OUT" >/dev/null
    mv -f "$OUT/${GAME}_${VER}_surgical-${SUF}Signed-aligned-signed.apk" "$SIGNED" 2>/dev/null || true
fi
rm -f "$TMP_ALIGNED"
[ -f "$SIGNED" ] || { echo "ERROR: 签名产物缺失" >&2; exit 1; }

echo "    signed: $SIGNED"
echo "    size=$(stat -f%z "$SIGNED")  md5=$(md5 -q "$SIGNED")"

# --- 产物自检 ---------------------------------------------------------------
python3.11 - "$SIGNED" "$NEW_DEX" <<'PY'
import sys, zipfile, hashlib
apk, new_dex = sys.argv[1], sys.argv[2]
with zipfile.ZipFile(apk) as zf:
    n = zf.namelist()
    assert new_dex in n, "missing " + new_dex
    assert "lib/arm64-v8a/libstarry_magic.so" in n, "missing starry so"
    assert zf.getinfo("resources.arsc").compress_type == zipfile.ZIP_STORED, "arsc not STORED"
    bad = [x for x in n if x.startswith("META-INF/") and x.rsplit(".", 1)[-1].upper() in ("SF", "RSA", "DSA", "EC")]
    print("    dex:", sorted(x for x in n if x.startswith("classes") and x.endswith(".dex")))
    print("    entries=%d  meta-inf-sigs=%s" % (len(n), bad))
h = hashlib.sha256(open(apk, "rb").read()).hexdigest()
print("    sha256=%s" % h)
PY

# --- 装机（STARRY_INSTALL=1） ----------------------------------------------
if [ "${STARRY_INSTALL:-0}" = "1" ]; then
    echo "[install] adb push + pm install -r -d"
    adb -s "$SERIAL" push "$SIGNED" "/data/local/tmp/jfqs_surgical.apk"
    adb -s "$SERIAL" shell am force-stop "$PKG" || true
    if ! adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 /data/local/tmp/jfqs_surgical.apk'" 2>&1 | tail -3; then
        if [ "${STARRY_ALLOW_UNINSTALL:-0}" = "1" ]; then
            echo "    签名不符 → 卸载重装（会清数据）"
            adb -s "$SERIAL" shell "su -c 'pm uninstall --user 0 $PKG'"
            adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 /data/local/tmp/jfqs_surgical.apk'"
        else
            echo "    签名不符；需显式 STARRY_ALLOW_UNINSTALL=1 才卸载（禁止清数据）" >&2
        fi
    fi
    echo "    logcat: adb -s $SERIAL logcat -s Mod_Menu:V StarrySignSpoof:V Jfqs:V MenuService:V"
fi

echo "[jfqs] done → $SIGNED"
