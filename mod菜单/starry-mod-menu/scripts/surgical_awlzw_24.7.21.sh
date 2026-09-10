#!/usr/bin/env bash
# 阿瓦隆之王 24.7.21 TapTap 官包 — 外科组装（官方 dex/arsc/libil2cpp 字节级保留，只增不改）
#
# 产物：08_output/阿瓦隆之王/阿瓦隆之王_24.7.21_surgical-releaseSigned.apk
#
# 与 surgical_es2_4.4.8684.sh 的差异：
#   - 官方 Application = com.funplus.sdk.FunplusApplication（登录/SDK 初始化挂里面）
#     → Manifest application name 改挂 StarryApp，StarryApp.smali 的 .super
#       补丁为 FunplusApplication（super.attachBaseContext/onCreate 链路保留）
#   - TapTap 官包 6 dex（classes~classes5）→ 星光 smali 注入为 classes6.dex（ART 连续规则）
#   - Unity IL2CPP：玩法 hook 在 libstarry_magic.so（RVA 0x8045BD0/0x80927D8/0x8FEA408，见 10_docs）
#   - MuMu 装机默认 127.0.0.1:16384（CEROVED via SERIAL）
#
# 用法:
#   bash scripts/surgical_awlzw_24.7.21.sh            # 只出包
#   STARRY_INSTALL=1 bash scripts/surgical_awlzw_24.7.21.sh   # 出包+装机
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
ORIG_APK="$NIXIANG/02_apks/阿瓦隆之王/阿瓦隆之王_24.7.21_taptap.apk"
MF_CACHE="$NIXIANG/07_patched/阿瓦隆之王/surgical_24.7.21/AndroidManifest.xml"
OUT="$NIXIANG/08_output/阿瓦隆之王"
PKG="com.funplus.kingofavalon.cn"
SIGNED="$OUT/阿瓦隆之王_24.7.21_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/阿瓦隆之王_24.7.21_surgical-debugSigned.apk"
TMP_UNSIGNED="$OUT/.tmp_unsigned.apk"
TMP_ALIGNED="$OUT/.tmp_aligned.apk"
SIGNER="$NIXIANG/01_tools/uber-apk-signer.jar"
SMALI_BIN="$NIXIANG/01_tools/smali/smali"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_CXX="$NDK/toolchains/llvm/prebuilt/darwin-x86_64/sysroot/usr/lib/aarch64-linux-android/libc++_shared.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"

BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then
    BT="$HOME/Library/Android/sdk/build-tools/$v"; break
  fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing"; exit 1; }

command -v python3.11 >/dev/null || { echo "ERROR: python3.11 missing"; exit 1; }
command -v apktool >/dev/null || { echo "ERROR: apktool missing"; exit 1; }
[[ -f "$ORIG_APK" ]] || { echo "ERROR: missing $ORIG_APK"; exit 1; }
[[ -x "$SMALI_BIN" ]] || { echo "ERROR: missing $SMALI_BIN"; exit 1; }

MK="$STARRY_SRC/app/src/main/jni/Android_awlzw.mk"
grep -q 'Games/AwlzwMod.cpp' "$MK" || { echo "ERROR: Android_awlzw.mk missing AwlzwMod.cpp"; exit 1; }
echo "[check] Android_awlzw.mk target=awlzw (Unity IL2CPP)"

AVAIL_GB="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[[ "${AVAIL_GB:-0}" -ge 4 ]] || { echo "ERROR: 磁盘可用 ${AVAIL_GB:-0}G < 4G"; exit 1; }

cleanup() {
  rm -rf "$WORK" 2>/dev/null || true
  rm -rf "$LOCK_DIR" 2>/dev/null || true
  [[ -n "${KEEP_TMP:-}" ]] || { rm -f "$TMP_UNSIGNED" "$TMP_ALIGNED" 2>/dev/null || true; }
}
LOCK_DIR="/tmp/awlzw_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/awlzw_surgical.XXXXXX)"
trap cleanup EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs（NDK_OUT 锁 awlzw 专属目录）"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
AWLZW_NDK_OUT="$STARRY_SRC/app/src/obj_awlzw"
AWLZW_NDK_LIBS="$STARRY_SRC/app/src/libs_awlzw"
rm -rf "$AWLZW_NDK_OUT" "$AWLZW_NDK_LIBS"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_awlzw.mk NDK_APPLICATION_MK=./Application.mk \
  NDK_OUT="$AWLZW_NDK_OUT" NDK_LIBS_OUT="$AWLZW_NDK_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$AWLZW_NDK_LIBS/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
LC_ALL=C grep -a -q 'Awlzw' "$SO_BUILT" || { echo "ERROR: so 无 [Awlzw] 标签"; exit 1; }
LC_ALL=C grep -a -q 'awlzw' "$SO_BUILT" || { echo "ERROR: so 无 game_id=awlzw"; exit 1; }
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Awlzw] verified)"

echo "[2/7] gradle $GRADLE_TASK (starry 小模板)"
rsync -a --delete \
  --exclude '.gradle/' --exclude 'app/build/' --exclude 'build/' \
  --exclude 'auth-server/.venv/' --exclude 'auth-server/data/' --exclude '.idea/' \
  --exclude 'obj/' \
  "$STARRY_SRC/" "$WORK/starry/"
(cd "$WORK/starry" && ./gradlew "$GRADLE_TASK" --console=plain)
STARRY_APK="$WORK/starry/app/build/outputs/apk/$BUILD_TYPE/$APK_NAME"
[[ -f "$STARRY_APK" ]] || { echo "ERROR: missing $STARRY_APK"; exit 1; }
unzip -p "$STARRY_APK" lib/arm64-v8a/libstarry_magic.so > "$WORK/check.so"
grep -a -q 'Awlzw' "$WORK/check.so" \
  || { echo "ERROR: gradle APK 内 so 不是 Awlzw"; exit 1; }

echo "[3/7] apktool d starry → com/starry smali → classes6.dex（官方 6 dex 不碰；ART 连续规则追加 classes6）"
apktool d -f -o "$WORK/starry_dec" "$STARRY_APK" >/dev/null
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
[[ -f "$SMALI_ROOT/com/starry/magic/menu/MenuService.smali" ]] \
  || { echo "ERROR: MenuService.smali missing"; exit 1; }

# 阿瓦隆专用：StarryApp .super 补丁为官方 FunplusApplication（登录/SDK 初始化链保留）
FP_APP="Lcom/funplus/sdk/FunplusApplication;"
STAR_SMALI="$SMALI_ROOT/com/starry/magic/StarryApp.smali"
if ! grep -q "\.super $FP_APP" "$STAR_SMALI"; then
  sed -i '' \
    -e "s|^.super Landroid/app/Application;$|.super $FP_APP|" \
    -e "s|Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V|$FP_APP->attachBaseContext(Landroid/content/Context;)V|" \
    -e "s|Landroid/app/Application;->onCreate()V|$FP_APP->onCreate()V|" \
    "$STAR_SMALI"
  grep -q "\.super $FP_APP" "$STAR_SMALI" \
    || { echo "ERROR: StarryApp .super patch failed"; exit 1; }
  grep -q "$FP_APP->attachBaseContext" "$STAR_SMALI" \
    || { echo "ERROR: StarryApp invoke-super patch failed"; exit 1; }
  echo "  StarryApp .super + invoke-super → FunplusApplication"
fi
"$SMALI_BIN" assemble -o "$WORK/classes6.dex" "$SMALI_ROOT"
ls -la "$WORK/classes6.dex"

echo "[3b/7] collect lib + certs from starry decode"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
grep -a -q 'Awlzw' "$SO_SRC" || { echo "ERROR: gradle 解包 so 无 [Awlzw] 标签"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] binary AndroidManifest（application 挂 StarryApp + FGS/SAW 权限 + MenuService）"
if [[ -n "${STARRY_BINARY_MANIFEST:-}" && -f "$STARRY_BINARY_MANIFEST" ]]; then
  cp "$STARRY_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using STARRY_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  mini apk（剔除 assets/lib/dex/META-INF）→ apktool 往返"
  python3.11 - "$ORIG_APK" "$WORK/mini.apk" <<'PY'
import sys, zipfile
from pathlib import Path

src, dst = Path(sys.argv[1]), Path(sys.argv[2])
zin = zipfile.ZipFile(src, "r")
zout = zipfile.ZipFile(dst, "w")
kept = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith(("META-INF/", "assets/", "lib/")):
        continue
    if name.startswith("classes") and name.endswith(".dex"):
        continue
    info = zipfile.ZipInfo(item.orig_filename, item.date_time)
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    zout.writestr(info, zin.read(item))
    kept += 1
zout.close()
zin.close()
print(f"  mini apk: {kept} entries")
PY
  apktool d -f -s -o "$WORK/orig_dec" "$WORK/mini.apk" >/dev/null
  python3.11 - "$WORK/orig_dec/AndroidManifest.xml" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8")
FP = 'com.funplus.sdk.FunplusApplication'
if "com.starry.magic.StarryApp" in t:
    print("  manifest: application already StarryApp")
else:
    if FP not in t:
        raise SystemExit(f"ERROR: 官方 Application {FP} 不在 Manifest（版本假设失效）")
    t = t.replace(f'android:name="{FP}"', 'android:name="com.starry.magic.StarryApp"', 1)
    if "com.starry.magic.StarryApp" not in t:
        raise SystemExit("ERROR: application name replace failed")
    print("  manifest: application FunplusApplication → StarryApp (.super 补丁保留链路)")
perms = []
if "SYSTEM_ALERT_WINDOW" not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if "FOREGROUND_SERVICE_SPECIAL_USE" not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find("<uses-permission")
    if i < 0:
        raise SystemExit("ERROR: no <uses-permission> anchor in manifest")
    t = t[:i] + "\n".join(perms) + "\n    " + t[i:].lstrip()
    print("  manifest: + SYSTEM_ALERT_WINDOW/FOREGROUND_SERVICE/SPECIAL_USE")
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
if "com.starry.magic.menu.MenuService" not in t:
    t = t.replace("</application>", svc + "\n    </application>", 1)
    print("  manifest: MenuService added")
p.write_text(t, encoding="utf-8")
PY
  rm -rf "$WORK/orig_dec"/smali* 2>/dev/null || true
  apktool b -f "$WORK/orig_dec" -o "$WORK/mf_only.apk" >/dev/null
  unzip -p "$WORK/mf_only.apk" AndroidManifest.xml > "$WORK/AndroidManifest.xml"
  cp "$WORK/AndroidManifest.xml" "$MF_CACHE"
  echo "  cached → $MF_CACHE"
fi
ls -la "$WORK/AndroidManifest.xml"

echo "[5/7] zip surgical merge（流式；官方 dex/arsc/libil2cpp/libunity 字节级保留；星光注入 classes6.dex）"
python3.11 - "$ORIG_APK" "$WORK" "$TMP_UNSIGNED" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c6 = (work / "classes6.dex").read_bytes()
extras = {}
libdir = work / "lib"
if libdir.is_dir():
    for p in sorted(libdir.rglob("*")):
        if p.is_file():
            extras["lib/" + str(p.relative_to(libdir)).replace("\\", "/")] = p.read_bytes()
certs = work / "assets" / "certs"
if certs.is_dir():
    for p in sorted(certs.rglob("*")):
        if p.is_file():
            extras["assets/certs/" + str(p.relative_to(certs)).replace("\\", "/")] = p.read_bytes()
assert "lib/arm64-v8a/libstarry_magic.so" in extras, "libstarry_magic.so not staged"

class RawNameZipInfo(zipfile.ZipInfo):
    _raw_name = None
    _keep_flag = 0
    def _encodeFilenameFlags(self):
        if self._raw_name is not None:
            return self._raw_name, self._keep_flag
        return super()._encodeFilenameFlags()

def make_info(item):
    keep_flag = item.flag_bits & 0x800  # UTF-8 名标志（中文资源条目）
    raw = item.orig_filename.encode("utf-8", "replace") if keep_flag \
        else item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if keep_flag or raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw
        info._keep_flag = 0x800 if keep_flag else 0
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    return info

zin = zipfile.ZipFile(src, "r")
orig_names = set(zin.namelist())
dup = (orig_names & set(extras)) | ({"classes6.dex"} & orig_names)
assert not dup, f"版本假设失效（原包已含注入项）: {sorted(dup)}"

zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue
    info = make_info(item)
    if name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, mf)
    elif name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            import shutil
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            import shutil
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    seen.add(item.orig_filename)
    n += 1

c6info = RawNameZipInfo("classes6.dex")
c6info.compress_type = zipfile.ZIP_DEFLATED
c6info.create_system = 3
zout.writestr(c6info, c6)
seen.add("classes6.dex")

for name, data in sorted(extras.items()):
    einfo = RawNameZipInfo(name)
    einfo.compress_type = zipfile.ZIP_DEFLATED
    einfo.create_system = 3
    zout.writestr(einfo, data)
    seen.add(name)

zout.close()
zin.close()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")

def stream_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

PROTECTED = ["classes.dex", "classes2.dex", "classes3.dex", "classes4.dex",
             "classes5.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so"]
for nm in PROTECTED:
    h1, h2 = stream_sha(zin2, nm), stream_sha(zout2, nm)
    assert h1 == h2, f"PROTECTED MUTATED: {nm}"
    print(f"  sha256 ok: {nm}")

meta_mismatch = raw_mismatch = flag_mismatch = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name == "AndroidManifest.xml":
        continue
    if name in extras or (name.startswith("classes") and name.endswith(".dex") and name != "classes6.dex"):
        continue
    m = zout2.getinfo(name)
    if item.CRC != m.CRC or item.file_size != m.file_size or item.compress_type != m.compress_type:
        meta_mismatch += 1
    if item.orig_filename.encode("cp437", "replace") != m.orig_filename.encode("cp437", "replace"):
        raw_mismatch += 1
    if (item.flag_bits & 0x800) != (m.flag_bits & 0x800):
        flag_mismatch += 1
assert meta_mismatch == 0 and raw_mismatch == 0 and flag_mismatch == 0, \
    f"meta={meta_mismatch} rawname={raw_mismatch} flag={flag_mismatch}"
missing = [x for x in ("classes6.dex", "lib/arm64-v8a/libstarry_magic.so") if x not in seen]
assert not missing, f"missing injectables: {missing}"
assert "AndroidManifest.xml" in zout2.namelist()
print(f"  merged ok; official entries={n}; classes6.dex={len(c6)}B; +{len(extras)} extras")
print(f"  全量 CRC/size/ctype 一致; name-bytes & utf8-flag 全量一致（mismatch=0）")
zin2.close()
zout2.close()
PY

echo "[6/7] zipalign + sign → $SIGNED"
"$BT/zipalign" -f -p 4 "$TMP_UNSIGNED" "$TMP_ALIGNED"
"$BT/zipalign" -c 4 "$TMP_ALIGNED" || { echo "ERROR: aligned APK failed zipalign -c"; exit 1; }
rm -f "$TMP_UNSIGNED"
KEEP_TMP=""
if [[ -x "$BT/apksigner" && -f "$KS" ]]; then
  "$BT/apksigner" sign \
    --ks "$KS" --ks-key-alias "$KS_ALIAS" \
    --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
    --out "$SIGNED" "$TMP_ALIGNED"
  "$BT/apksigner" verify --print-certs "$SIGNED" 2>&1 | head -6 || true
else
  java -jar "$SIGNER" -a "$TMP_ALIGNED" --out "$OUT" --allowResign \
    --ks "$KS" --ksAlias "$KS_ALIAS" --ksPass "$KS_PASS" --ksKeyPass "$KS_PASS"
  CAND="$(ls -t "$OUT"/*surgical*Signed*.apk 2>/dev/null | grep -v '\.tmp_' | head -1 || true)"
  [[ -n "${CAND:-}" ]] && cp -f "$CAND" "$SIGNED"
fi
KEEP_TMP=""
rm -f "$TMP_ALIGNED"
[[ -f "$SIGNED" ]] || { echo "ERROR: signing produced no $SIGNED"; exit 1; }
echo "  size : $(ls -lh "$SIGNED" | awk '{print $5}')"
echo "  sha256: $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "  md5  : $(md5 -q "$SIGNED")"

# ---- so 闭环校验：包内 so 必须与 build_so 产物字节一致（防旧 so 混包直装） ----
IN_SO_MD5="$(unzip -p "$SIGNED" lib/arm64-v8a/libstarry_magic.so | md5 -q)"
SRC_SO_MD5="$(md5 -q "$SO_SRC")"
if [[ "$IN_SO_MD5" != "$SRC_SO_MD5" ]]; then
  echo "ERROR: 包内 so ($IN_SO_MD5) != 构建产物 ($SRC_SO_MD5)——旧 so 混包，禁止交付" >&2
  exit 1
fi
echo "[so-verify] 包内 so md5 == 构建产物 ($IN_SO_MD5) ✓"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  DEVICE="${STARRY_DEVICE:-127.0.0.1:16384}"
  echo "[7/7] install → $DEVICE"
  REMOTE="/data/local/tmp/awlzw_surgical.apk"
  adb -s "$DEVICE" push "$SIGNED" "$REMOTE"
  adb -s "$DEVICE" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$DEVICE" shell pm install -r "$REMOTE" \
    || { echo "[install] 覆盖装失败（签名冲突）；请人工处理" >&2; exit 1; }
  adb -s "$DEVICE" shell "rm -f $REMOTE" 2>/dev/null || true
  echo "log: adb -s $DEVICE logcat -s StarryApp:V StarryMagic:V Awlzw:V MenuService:V"
fi

echo "DONE: $SIGNED"