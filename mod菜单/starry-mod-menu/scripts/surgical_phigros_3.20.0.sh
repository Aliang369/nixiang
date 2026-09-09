#!/usr/bin/env bash
# Phigros 3.20.0 TapTap — 外科组装（官方 classes.dex / resources.arsc / libil2cpp.so 字节级保留，只增不改）
#
# 产物：08_output/phigros/phigros_3.20.0_surgical-releaseSigned.apk
#
# 方案（2026-09-08，骨架复刻 surgical_zhmr_3.6.0.sh + milthm 的 ndk-build 前置）：
#   - Phigros 官方 Manifest 的 application 节点无 android:name（默认 android.app.Application）
#     → StarryApp.smali 保持 .super Landroid/app/Application;，无需官方 App 继承补丁
#   - 官方包仅 1 个 classes.dex → 星光 smali → classes2.dex（ART 连续 dex 规则）
#   - 原包仅 INTERNET / ACCESS_NETWORK_STATE 两条权限 → 注入 SYSTEM_ALERT_WINDOW +
#     FOREGROUND_SERVICE + FOREGROUND_SERVICE_SPECIAL_USE（MenuService FGS 前提）
#   - 二进制 Manifest：迷你 APK（python 流式剔除 assets/lib/dex/META-INF，避免 2.6G
#     decode 落盘）→ apktool d -s → 改文本 → apktool b → 抽取并缓存到
#     07_patched/phigros/surgical_3.20.0/AndroidManifest.xml
#   - zip 合并：python 流式逐条复制（zin.open → copyfileobj → zout.open，2.67G 不整体
#     读内存）；官方 classes.dex / resources.arsc / libil2cpp.so sha256 断言 +
#     全量 CRC/size/compress_type/rawname/utf8-flag 对比
#   - 磁盘：/tmp 与 /Users 同一 APFS 卷；峰值 = unsigned + aligned 两份全包并存
#     ≈5.0GiB → zipalign 完成立即删 unsigned 再签名；可用 <5G 直接报错退出
#   - phigros 为 Unity IL2CPP：无 HybridCLR / 无加固 / 无签名校验绕过需求，
#     不做 milthm 的 TapTap WebLogin 补丁
#
# 用法:
#   bash scripts/surgical_phigros_3.20.0.sh            # 只出包，不装机
#   STARRY_INSTALL=1 bash scripts/surgical_phigros_3.20.0.sh
#
# 依赖：Android.mk 已启用 STARRY_GAME_PHIGROS（Games/PhigrosMod.cpp）；JDK 17；smali-2.5.2.jar
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
ORIG_APK="$NIXIANG/02_apks/phigros/phigros_3.20.0_orig_base.apk"
MF_CACHE="$NIXIANG/07_patched/phigros/surgical_3.20.0/AndroidManifest.xml"
OUT="$NIXIANG/08_output/phigros"
PKG="com.PigeonGames.Phigros"
SIGNED="$OUT/phigros_3.20.0_surgical-releaseSigned.apk"
[[ "$BUILD_TYPE" == "debug" ]] && SIGNED="$OUT/phigros_3.20.0_surgical-debugSigned.apk"
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

MK="$STARRY_SRC/app/src/main/jni/Android.mk"
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DENABLE_UNITY_HOOK\s+-DSTARRY_GAME_PHIGROS' "$MK" \
  || { echo "ERROR: Android.mk must enable STARRY_GAME_PHIGROS (see SWITCH_GAME.md)"; exit 1; }
grep -E '^\s*LOCAL_CFLAGS\s*\+=\s*-DSTARRY_GAME_SELF' "$MK" \
  && { echo "ERROR: STARRY_GAME_SELF still enabled"; exit 1; }
grep -q 'Games/PhigrosMod.cpp' "$MK" || { echo "ERROR: Android.mk missing Games/PhigrosMod.cpp"; exit 1; }
echo "[check] Android.mk target=phigros (Unity IL2CPP hook)"

# 磁盘预检：峰值 unsigned+aligned 两份 2.5GiB 全包并存，/tmp 与 /Users 同卷
AVAIL_GB="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[[ "${AVAIL_GB:-0}" -ge 5 ]] || {
  echo "ERROR: 磁盘可用 ${AVAIL_GB:-0}G < 5G（合并/对齐/签名峰值需两份全包并存 ~5GiB），先清理后重试"
  exit 1
}
echo "[check] disk avail=${AVAIL_GB}G ok"

cleanup() {
  rm -rf "$WORK" 2>/dev/null || true
  rm -rf "$LOCK_DIR" 2>/dev/null || true
  [[ -n "${KEEP_TMP:-}" ]] || { rm -f "$TMP_UNSIGNED" "$TMP_ALIGNED" 2>/dev/null || true; }
}
LOCK_DIR="/tmp/phigros320_surgical.lockdir"
if ! mkdir "$LOCK_DIR" 2>/dev/null; then
  echo "ERROR: lock $LOCK_DIR held"; exit 1
fi
echo $$ >"$LOCK_DIR/pid"
WORK="$(mktemp -d /tmp/phigros320_surgical.XXXXXX)"
trap cleanup EXIT
mkdir -p "$OUT" "$WORK" "$(dirname "$MF_CACHE")"

echo "[1/7] ndk-build -> jniLibs（so 必须是本游戏，防 jniLibs 旧 so 污染）"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT=./Android.mk NDK_APPLICATION_MK=./Application.mk \
  -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)")
SO_BUILT="$STARRY_SRC/app/src/libs/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_BUILT" ]] || { echo "ERROR: missing $SO_BUILT"; exit 1; }
grep -a -q '\[Phigros\]' "$SO_BUILT" || { echo "ERROR: so 无 [Phigros] 标签（可能仍是上一款游戏）"; exit 1; }
grep -a -q 'phigros' "$SO_BUILT" || { echo "ERROR: so 无 game_id=phigros"; exit 1; }
for dest in \
  "$STARRY_SRC/app/src/main/jniLibs/arm64-v8a" \
  "$STARRY_SRC/app/src/main/libs/arm64-v8a" \
  "$STARRY_SRC/app/libs/arm64-v8a"
do
  mkdir -p "$dest"
  cp -f "$SO_BUILT" "$dest/libstarry_magic.so"
done
echo "  so -> jniLibs ok ([Phigros] verified)"

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
grep -a -q '\[Phigros\]' "$WORK/check.so" \
  || { echo "ERROR: gradle APK 内 so 不是 PHIGROS"; exit 1; }

echo "[3/7] apktool d starry → com/starry smali → classes2.dex"
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
"$SMALI_BIN" assemble -o "$WORK/classes2.dex" "$SMALI_ROOT"
ls -la "$WORK/classes2.dex"

echo "[3b/7] collect lib + certs from starry decode"
SO_SRC="$WORK/starry_dec/lib/arm64-v8a/libstarry_magic.so"
[[ -f "$SO_SRC" ]] || { echo "ERROR: missing libstarry_magic.so"; exit 1; }
grep -a -q '\[Phigros\]' "$SO_SRC" || { echo "ERROR: gradle 解包 so 无 [Phigros] 标签"; exit 1; }
mkdir -p "$WORK/lib/arm64-v8a"
cp "$SO_SRC" "$WORK/lib/arm64-v8a/"
[[ -f "$NDK_CXX" ]] && cp "$NDK_CXX" "$WORK/lib/arm64-v8a/libc++_shared.so"
if [[ -d "$WORK/starry_dec/assets/certs" ]]; then
  mkdir -p "$WORK/assets/certs"
  cp -R "$WORK/starry_dec/assets/certs/." "$WORK/assets/certs/"
fi

echo "[4/7] binary AndroidManifest（StarryApp + FGS/SAW 权限 + MenuService；迷你 APK 往返）"
if [[ -n "${STARRY_BINARY_MANIFEST:-}" && -f "$STARRY_BINARY_MANIFEST" ]]; then
  cp "$STARRY_BINARY_MANIFEST" "$WORK/AndroidManifest.xml"
  echo "  using STARRY_BINARY_MANIFEST=$STARRY_BINARY_MANIFEST"
elif [[ -f "$MF_CACHE" ]]; then
  cp "$MF_CACHE" "$WORK/AndroidManifest.xml"
  echo "  using cached $MF_CACHE"
else
  echo "  mini apk（剔除 assets/lib/dex/META-INF，只留 res+arsc+manifest）→ apktool 往返"
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
if "com.starry.magic.StarryApp" in t:
    print("  manifest: application already StarryApp")
else:
    # 官方 application 无 android:name（默认 android.app.Application）→ 直接挂 StarryApp
    if "<application" not in t:
        raise SystemExit("ERROR: <application> not found in manifest")
    t = t.replace("<application", '<application android:name="com.starry.magic.StarryApp"', 1)
    print("  manifest: application → com.starry.magic.StarryApp")
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

echo "[5/7] zip surgical merge（流式逐条复制，官方 dex/arsc/libil2cpp 字节级保留）"
python3.11 - "$ORIG_APK" "$WORK" "$TMP_UNSIGNED" <<'PY'
import sys, zipfile, hashlib, shutil
from pathlib import Path

src, work, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
mf = (work / "AndroidManifest.xml").read_bytes()
c2 = (work / "classes2.dex").read_bytes()
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
    """按原始字节写条目名（flag 不置 0x800）。
    Unity APK 里部分 AssetBundle 名是「UTF-8 字节但不带 UTF-8 flag」；python zipfile
    重写时会把名字重编码并强设 0x800，Unity/Android zip 读取器路径对不上 →
    "Unable to open archive file" → 加载条卡死（zhmr 3.5.0 实锤 28 条）。"""
    _raw_name = b""
    def _encodeFilenameFlags(self):
        if self._raw_name:
            return self._raw_name, 0
        return super()._encodeFilenameFlags()

def make_info(item):
    raw = item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if (item.flag_bits & 0x800) == 0 and raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw  # 名字字节原样保留，flag 保持 0
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    return info

zin = zipfile.ZipFile(src, "r")
orig_names = set(zin.namelist())
dup = (orig_names & set(extras)) | ({"classes2.dex"} & orig_names)
assert not dup, f"版本假设失效（原包已含注入项）: {sorted(dup)}"

zout = zipfile.ZipFile(dst, "w")
seen = set()
n = 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):
        continue
    if name in extras:
        continue  # 新增项在下方统一写入
    info = make_info(item)
    if name == "AndroidManifest.xml":
        info.compress_type = zipfile.ZIP_DEFLATED
        zout.writestr(info, mf)
    elif name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED  # Android 11+ 要求 arsc 未压缩
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)
    else:
        with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
            shutil.copyfileobj(fsrc, fdst, 1 << 20)  # 流式：大条目不整体读内存
    seen.add(item.orig_filename)
    n += 1

c2info = RawNameZipInfo("classes2.dex")
c2info.compress_type = zipfile.ZIP_DEFLATED
c2info.create_system = 3
zout.writestr(c2info, c2)
seen.add("classes2.dex")

for name, data in sorted(extras.items()):
    einfo = RawNameZipInfo(name)
    einfo.compress_type = zipfile.ZIP_DEFLATED  # extractNativeLibs=true，DEFLATED 可安装
    einfo.create_system = 3
    zout.writestr(einfo, data)
    seen.add(name)

zout.close()
zin.close()

# 归一化：python zipfile 的 ZIP64_LIMIT=2^31-1（非 4G），2.67G 包会被写上 zip64
# EOCD + 晚期条目 CD zip64 extra（hoff=0xFFFFFFFF, vneed=45）。官方工具链
# （AGP/zipflinger）对 2-4G 包写经典记录；zipalign(36.0.0) 的 libziparchive
# 走 zip64 EOCD 路径对 >2G 文件 "Unable to open"（实测），且 CD vneed=45 与
# 本地头 ver=20 不一致触发 BadPack 防护 "header mismatch"（实测 23 条）。
# 修复：剥离逐条目 zip64 extra 还原真实 hoff、vneed 回填本地头版本、尾部改写
# 纯经典 EOCD（cdoff 2.49GiB < 4G，uint32 合法，与原包 EOCD 形态一致）。
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
        # 经典 EOCD 字段饱和 → 从 EOCD64 取真实值
        e64 = e - 76
        assert e64 >= 0 and tail[e64:e64+4] == b"PK\x06\x06", "expected EOCD64 before EOCD"
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
            struct.pack_into("<H", rec, 6, lver)       # vneed = LFH version（消 header mismatch）
            struct.pack_into("<H", rec, 30, len(new_extra))
            struct.pack_into("<I", rec, 42, real_off)  # 真实 hoff（uint32）
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
    # 纯经典 EOCD（与原包形态一致；2-4G 包 cdoff uint32 合法）
    f.write(struct.pack("<IHHHHIIH", 0x06054b50, 0, 0, n2, n2, new_cdsize, cdoff, 0))
    f.truncate()
    f.close()
    print(f"  normalized: zip64 extras stripped/vneed fixed={fixed}, classic EOCD ok")

normalize_zip(dst)

# 自检 1：官方关键条目 sha256（外科铁律：dex / arsc / libil2cpp 字节不动）
zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")

def stream_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

for nm in ("classes.dex", "resources.arsc", "lib/arm64-v8a/libil2cpp.so"):
    h1, h2 = stream_sha(zin2, nm), stream_sha(zout2, nm)
    assert h1 == h2, f"PROTECTED MUTATED: {nm}"
    print(f"  sha256 ok: {nm}")

# 自检 2：全量条目 meta 对比（CRC/size/compress_type 零读取成本）+ 名字字节与 utf8 flag
meta_mismatch = raw_mismatch = flag_mismatch = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name == "AndroidManifest.xml":
        continue
    if name in extras or (name.startswith("classes") and name.endswith(".dex") and name != "classes.dex"):
        continue  # 注入项原包不应有（前 assert 已保证）
    m = zout2.getinfo(name)
    if item.CRC != m.CRC or item.file_size != m.file_size or item.compress_type != m.compress_type:
        meta_mismatch += 1
    if item.orig_filename.encode("cp437", "replace") != m.orig_filename.encode("cp437", "replace"):
        raw_mismatch += 1
    if (item.flag_bits & 0x800) != (m.flag_bits & 0x800):
        flag_mismatch += 1
assert meta_mismatch == 0 and raw_mismatch == 0 and flag_mismatch == 0, \
    f"meta={meta_mismatch} rawname={raw_mismatch} flag={flag_mismatch}"
missing = [x for x in ("classes2.dex", "lib/arm64-v8a/libstarry_magic.so") if x not in seen]
assert not missing, f"missing injectables: {missing}"
assert "AndroidManifest.xml" in zout2.namelist()
print(f"  merged ok; official entries={n}; classes2.dex={len(c2)}B; +{len(extras)} extras")
print(f"  全量 CRC/size/ctype 一致; name-bytes & utf8-flag 全量一致（mismatch=0）")
zin2.close()
zout2.close()
PY

echo "[6/7] zipalign + sign → $SIGNED"
"$BT/zipalign" -f -p 4 "$TMP_UNSIGNED" "$TMP_ALIGNED"
"$BT/zipalign" -c 4 "$TMP_ALIGNED" || { echo "ERROR: aligned APK failed zipalign -c"; exit 1; }
rm -f "$TMP_UNSIGNED"  # 对齐完成即删，压低磁盘峰值（两份全包并存只发生在对齐期）
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
  # 默认不装机；STARRY_INSTALL=1 时覆盖安装（禁卸载：清数据 = Mod + 存档双杀）
  REMOTE="/data/local/tmp/phigros320_surgical.apk"
  adb push "$SIGNED" "$REMOTE"
  adb shell am force-stop "$PKG" 2>/dev/null || true
  adb shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
    || adb install -r "$SIGNED"
  echo "log: adb logcat -s StarryApp:V StarryMagic:V Phigros:V MenuService:V"
fi

echo "DONE: $SIGNED"
