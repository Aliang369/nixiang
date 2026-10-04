#!/usr/bin/env bash
# 偶像梦幻祭2 (es2) 4.4.8684 TapTap 官包 — 外科组装（官方 dex/arsc/libil2cpp 字节级保留，只增不改）
#
# 产物：08_output/偶像梦幻2/es2_4.4.8684_surgical-releaseSigned.apk
#
# 与 surgical_phigros_3.20.0.sh 的差异：
#   - 官方 Application = com.happyelements.hei.android.HeSDKApplication（有名字，登录/SDK 初始化挂里面）
#     → Manifest application name 改挂 StarryApp，且 StarryApp.smali 的 .super
#       补丁为 HeSDKApplication（super.attachBaseContext/onCreate 链路保留）
#   - TapTap 官包仅 classes.dex + classes2.dex → 星光 smali 注入为 classes3.dex（ART 连续规则）
#   - 1.12GB < 2G：无 zip64（normalize_zip 保留作防御，无 zip64 时零改动通过）
#   - 真机安装默认目标 96f0d883（STARRY_DEVICE 可覆盖）
#
# 用法:
#   bash scripts/surgical_es2_4.4.8684.sh            # 只出包
#   STARRY_INSTALL=1 bash scripts/surgical_es2_4.4.8684.sh   # 出包+装真机
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
ORIG_APK="$NIXIANG/02_apks/偶像梦幻2/es2_4.4.8684_orig_taptap.apk"
MF_CACHE="$NIXIANG/07_patched/偶像梦幻2/surgical_4.4.8684/AndroidManifest.xml"
OUT="$NIXIANG/08_output/偶像梦幻2"
PKG="com.happyelements.es2"
SIGNED="$OUT/es2_4.4.8684_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/es2_4.4.8684_surgical-debugSigned.apk"
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

MK="$STARRY_SRC/app/src/main/jni/Android_es2.mk"  # es2 私有 mk（公共 Android.mk 可能被并行会话切到别的游戏）
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_ES2' "$MK" \
  || { echo "ERROR: Android_es2.mk must enable STARRY_GAME_ES2"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DSTARRY_GAME_SELF' "$MK" \
  && { echo "ERROR: STARRY_GAME_SELF still enabled"; exit 1; }
grep -q 'Games/Es2Mod.cpp' "$MK" || { echo "ERROR: Android_es2.mk missing Games/Es2Mod.cpp"; exit 1; }
echo "[check] Android_es2.mk target=es2 oxmhj2 (Unity IL2CPP probe)"

AVAIL_GB="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[[ "${AVAIL_GB:-0}" -ge 4 ]] || {
  echo "ERROR: 磁盘可用 ${AVAIL_GB:-0}G < 4G（1.12G 包对齐期两份并存）"; exit 1
}
echo "[check] disk avail=${AVAIL_GB}G ok"

cleanup() {
  rm -rf "$WORK" 2>/dev/null || true
  rm -rf "$LOCK_DIR" 2>/dev/null || true
  [[ -n "${KEEP_TMP:-}" ]] || { rm -f "$TMP_UNSIGNED" "$TMP_ALIGNED" 2>/dev/null || true; }
}
LOCK_DIR="/tmp/es2_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/es2_surgical.XXXXXX)"
trap cleanup EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs（NDK_OUT 锁 es2 专属目录：并行会话各编各的，防陈旧 obj 链接污染）"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
ES2_NDK_OUT="$STARRY_SRC/app/src/obj_es2"
ES2_NDK_LIBS="$STARRY_SRC/app/src/libs_es2"
rm -rf "$ES2_NDK_OUT" "$ES2_NDK_LIBS"
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android_es2.mk NDK_APPLICATION_MK=./Application.mk \
  NDK_OUT="$ES2_NDK_OUT" NDK_LIBS_OUT="$ES2_NDK_LIBS" \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$ES2_NDK_LIBS/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
grep -a -q '\[Es2\]' "$SO_BUILT" || { echo "ERROR: so 无 [Es2] 标签"; exit 1; }
grep -a -q 'oxmhj2' "$SO_BUILT" || { echo "ERROR: so 无 game_id=oxmhj2"; exit 1; }
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Es2] verified)"

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
grep -a -q '\[Es2\]' "$WORK/check.so" \
  || { echo "ERROR: gradle APK 内 so 不是 ES2"; exit 1; }

echo "[3/7] apktool d starry → com/starry smali → classes3.dex（官方 2 dex 不碰；ART 连续规则追加 classes3）"
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

# es2 专用：StarryApp .super 补丁为官方 HeSDKApplication（登录/SDK 初始化链保留）
# invoke-super 常量同步替换：否则 attachBaseContext/onCreate 走 Application 实现跳过 HeSDK override
HE_APP="Lcom/happyelements/hei/android/HeSDKApplication;"
STAR_SMALI="$SMALI_ROOT/com/starry/magic/StarryApp.smali"
if ! grep -q "\.super $HE_APP" "$STAR_SMALI"; then
  sed -i '' \
    -e "s|^.super Landroid/app/Application;$|.super $HE_APP|" \
    -e "s|Landroid/app/Application;->attachBaseContext(Landroid/content/Context;)V|$HE_APP->attachBaseContext(Landroid/content/Context;)V|" \
    -e "s|Landroid/app/Application;->onCreate()V|$HE_APP->onCreate()V|" \
    "$STAR_SMALI"
  grep -q "\.super $HE_APP" "$STAR_SMALI" \
    || { echo "ERROR: StarryApp .super patch failed"; exit 1; }
  grep -q "$HE_APP->attachBaseContext" "$STAR_SMALI" \
    || { echo "ERROR: StarryApp invoke-super patch failed"; exit 1; }
  echo "  StarryApp .super + invoke-super → HeSDKApplication"
fi
"$SMALI_BIN" assemble -o "$WORK/classes3.dex" "$SMALI_ROOT"
ls -la "$WORK/classes3.dex"

echo "[3b/7] collect lib + certs from starry decode"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
grep -a -q '\[Es2\]' "$SO_SRC" || { echo "ERROR: gradle 解包 so 无 [Es2] 标签"; exit 1; }
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
HE = 'com.happyelements.hei.android.HeSDKApplication'
if "com.starry.magic.StarryApp" in t:
    print("  manifest: application already StarryApp")
else:
    if HE not in t:
        raise SystemExit(f"ERROR: 官方 Application {HE} 不在 Manifest（版本假设失效）")
    # StarryApp 继承 HeSDKApplication → 入口换星光，官方链路经 super 保留
    t = t.replace(f'android:name="{HE}"', 'android:name="com.starry.magic.StarryApp"', 1)
    if "com.starry.magic.StarryApp" not in t:
        raise SystemExit("ERROR: application name replace failed")
    print("  manifest: application HeSDKApplication → StarryApp (.super 补丁保留链路)")
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

echo "[5/7] zip surgical merge（流式；官方 2 dex/arsc/libil2cpp/libunity/libxlua 字节级保留；星光注入 classes3.dex）"
python3.11 - "$ORIG_APK" "$WORK" "$TMP_UNSIGNED" <<'PY'
import sys, zipfile, hashlib
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c6 = (work / "classes3.dex").read_bytes()
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
    """按原始字节写条目名（flag 不置 0x800）；zhmr 3.5.0 实锤 28 条 Unity 资产坑。"""
    _raw_name = b""
    def _encodeFilenameFlags(self):
        if self._raw_name:
            return self._raw_name, 0
        return super()._encodeFilenameFlags()

def make_info(item):
    raw = item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if (item.flag_bits & 0x800) == 0 and raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    return info

zin = zipfile.ZipFile(src, "r")
orig_names = set(zin.namelist())
dup = (orig_names & set(extras)) | ({"classes3.dex"} & orig_names)
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

c6info = RawNameZipInfo("classes3.dex")
c6info.compress_type = zipfile.ZIP_DEFLATED
c6info.create_system = 3
zout.writestr(c6info, c6)
seen.add("classes3.dex")

for name, data in sorted(extras.items()):
    einfo = RawNameZipInfo(name)
    einfo.compress_type = zipfile.ZIP_DEFLATED
    einfo.create_system = 3
    zout.writestr(einfo, data)
    seen.add(name)

zout.close()
zin.close()

# 防御性 zip64 归一化（1.12G 无 zip64；非 zip64 时 vneed!=45 条目原样通过）
def normalize_zip(path):
    import struct
    f = open(path, "r+b")
    f.seek(0, 2); size = f.tell()
    f.seek(max(0, size - 65536)); tail = f.read()
    e = tail.rfind(b"PK\x05\x06")
    assert e >= 0, "EOCD not found"
    _, _, _, _, n2, cdsize, cdoff, clen = struct.unpack("<IHHHHIIH", tail[e:e+22])
    assert clen == 0, "unexpected EOCD comment"
    if cdoff == 0xFFFFFFFF or cdsize == 0xFFFFFFFF or n2 == 0xFFFF:
        e64 = e - 76
        assert e64 >= 0 and tail[e64:e64+4] == b"PK\x06\x06", "expected EOCD64"
        count64, = struct.unpack("<Q", tail[e64+32:e64+40])
        cdsize64, cdoff64 = struct.unpack("<QQ", tail[e64+40:e64+56])
        n2, cdsize, cdoff = count64, cdsize64, cdoff64
    f.seek(cdoff); cd = f.read(cdsize)
    out = bytearray(); pos = fixed = 0
    while pos < len(cd):
        assert cd[pos:pos+4] == b"PK\x01\x02", f"bad CD sig @{pos}"
        nlen, elen, clen2 = struct.unpack_from("<HHH", cd, pos+28)
        hoff = struct.unpack_from("<I", cd, pos+42)[0]
        vneed = struct.unpack_from("<H", cd, pos+6)[0]
        total = 46 + nlen + elen + clen2
        rec = bytearray(cd[pos:pos+total])
        if hoff == 0xFFFFFFFF:
            name = cd[pos+46:pos+46+nlen]
            extra = cd[pos+46+nlen:pos+46+nlen+elen]
            comment = cd[pos+46+nlen+elen:pos+total]
            real = None; blocks = []; q = 0
            while q + 4 <= len(extra):
                bid, bsz = struct.unpack_from("<HH", extra, q)
                payload = extra[q+4:q+4+bsz]
                if bid == 1:
                    real = payload
                else:
                    blocks.append((bid, payload))
                q += 4 + bsz
            assert real is not None and len(real) == 8, "unexpected zip64 payload"
            real_off = struct.unpack("<Q", real)[0]
            assert real_off < 0xFFFFFFFF, "entry offset exceeds classic uint32"
            f.seek(real_off + 4); lver = struct.unpack("<H", f.read(2))[0]
            new_extra = b"".join(struct.pack("<HH", bid, len(pl)) + pl for bid, pl in blocks)
            rec = bytearray(cd[pos:pos+46])
            struct.pack_into("<H", rec, 6, lver)
            struct.pack_into("<H", rec, 30, len(new_extra))
            struct.pack_into("<I", rec, 42, real_off)
            out += rec + name + new_extra + comment
            fixed += 1
        elif vneed == 45:
            f.seek(hoff + 4); lver = struct.unpack("<H", f.read(2))[0]
            struct.pack_into("<H", rec, 6, lver)
            out += rec
            fixed += 1
        else:
            out += rec
        pos += total
    new_cdsize = len(out)
    f.seek(cdoff); f.write(out)
    f.seek(cdoff + new_cdsize)
    f.write(struct.pack("<IHHHHIIH", 0x06054b50, 0, 0, n2, n2, new_cdsize, cdoff, 0))
    f.truncate()
    f.close()
    print(f"  normalized: zip64 extras stripped/vneed fixed={fixed}")

normalize_zip(dst)

# 自检：官方关键条目 sha256（外科铁律）
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")

def stream_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

PROTECTED = ["classes.dex", "classes2.dex", "resources.arsc",
             "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
             "lib/arm64-v8a/libxlua.so"]
for nm in PROTECTED:
    h1, h2 = stream_sha(zin2, nm), stream_sha(zout2, nm)
    assert h1 == h2, f"PROTECTED MUTATED: {nm}"
    print(f"  sha256 ok: {nm}")

meta_mismatch = raw_mismatch = flag_mismatch = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name == "AndroidManifest.xml":
        continue
    if name in extras or (name.startswith("classes") and name.endswith(".dex") and name != "classes3.dex"):
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
missing = [x for x in ("classes3.dex", "lib/arm64-v8a/libstarry_magic.so") if x not in seen]
assert not missing, f"missing injectables: {missing}"
assert "AndroidManifest.xml" in zout2.namelist()
print(f"  merged ok; official entries={n}; classes3.dex={len(c6)}B; +{len(extras)} extras")
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

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  DEVICE="${STARRY_DEVICE:-96f0d883}"
  echo "[7/7] install → $DEVICE"
  REMOTE="/data/local/tmp/es2_surgical.apk"
  adb -s "$DEVICE" push "$SIGNED" "$REMOTE"
  adb -s "$DEVICE" shell am force-stop "$PKG" 2>/dev/null || true
  adb -s "$DEVICE" install -r "$SIGNED" \
    || adb -s "$DEVICE" shell pm install -r "$REMOTE"
  adb -s "$DEVICE" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
  echo "log: adb -s $DEVICE logcat -s StarryApp:V StarryMagic:V Es2:V MenuService:V"
fi

echo "DONE: $SIGNED"
