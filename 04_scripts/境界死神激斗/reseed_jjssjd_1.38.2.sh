#!/usr/bin/env bash
# 境界-死神激斗 1.38.2（com.cmge.ssjd.x7sy）— 种子重注入（单条目替换 + 重签）
#
# 为什么需要这个脚本（而不是直接跑 surgical_jjssjd_1.38.2.sh）：
#   官方底包 2,466,112,634 B（md5 befa9c606746fe8fa5d084ece3547949）已按工作区口径
#   「APK 不留本地」在 2026-10-02 收尾清理中删除；设备上现在是星光包，adb 拉不回官方；
#   02_apks/ 无本游戏；渠道重下会拿到滚动后的新版（版本/字节都不对）。
#   ⇒ 采用「反推种子」路线（先例：伊莫 2026-09-30 第二次反推救场）：
#     以上一轮**正式交付包**为容器，只替换本轮真正变了的条目，其余条目字节级透传。
#
# 本轮变更集（相对上轮正式包）：
#   lib/arm64-v8a/libstarry_magic.so   ← 新构建（内嵌秒杀补丁 DLL）
#   META-INF/{STARRY.SF,STARRY.RSA,MANIFEST.MF}  ← 丢弃后由 apksigner 重签
#   **classes9.dex 不变**（菜单功能位定义在 native 的 kJjssjd_Features[] 里，dex 无需重建）
#   **AndroidManifest.xml 不变**（ContentProvider 注入与权限都是上轮那份，字节透传）
#
# 产物：08_output/境界死神激斗/境界死神激斗_1.38.2_surgical-releaseSigned.apk
#
# 用法:
#   bash 04_scripts/境界死神激斗/reseed_jjssjd_1.38.2.sh
#   STARRY_INSTALL=1 bash 04_scripts/境界死神激斗/reseed_jjssjd_1.38.2.sh
#
# ⚠️ 本脚本是「底包已删」的兜底通道，不是常规出包路径。底包能拿到时必须走
#    mod菜单/starry-mod-menu/scripts/surgical_jjssjd_1.38.2.sh。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
STARRY_SRC="$NIXIANG/mod菜单/starry-mod-menu"
GAME="境界死神激斗"
VER="1.38.2"
PKG="com.cmge.ssjd.x7sy"
TAG="Jjssjd"
GAME_ID="jjssjd"

OUT="$NIXIANG/08_output/$GAME"
SEED="${STARRY_SEED_APK:-$OUT/${GAME}_${VER}_surgical-releaseSigned.apk}"
SEED_MD5="e3f7de59c83246de32f5956d54df0bd5"
SEED_SHA256="34b5d108d726b75da88fef2d5ba8e338eb32c56a8b2a6b47efbe058b5bd2bd37"
SIGNED="$OUT/${GAME}_${VER}_surgical-releaseSigned.apk"
TMP_NEW="$OUT/.tmp_reseed_new.apk"
TMP_UNSIGNED="$OUT/.tmp_reseed_unsigned.apk"
TMP_ALIGNED="$OUT/.tmp_reseed_aligned.apk"
SO_PREBUILT="$STARRY_SRC/libs_$GAME_ID/arm64-v8a/libstarry_magic.so"
KS="${STARRY_KS:-$NIXIANG/01_tools/starry_release.jks}"
KS_ALIAS="${STARRY_KS_ALIAS:-starry}"
KS_PASS="${STARRY_KS_PASS:-starry2026}"
ADB="${ADB:-adb}"
SERIAL="${ANDROID_SERIAL:-f9a116a2}"
SFN="lib/arm64-v8a/libstarry_magic.so"

export JAVA_HOME="${JAVA_HOME:-$(/usr/libexec/java_home -v 17 2>/dev/null || true)}"
export PATH="${JAVA_HOME:+$JAVA_HOME/bin:}$PATH"
BT=""
for v in 36.0.0 35.0.0 34.0.0; do
  if [[ -x "$HOME/Library/Android/sdk/build-tools/$v/zipalign" ]]; then BT="$HOME/Library/Android/sdk/build-tools/$v"; break; fi
done
[[ -n "$BT" ]] || { echo "ERROR: build-tools zipalign missing"; exit 1; }
command -v python3.11 >/dev/null || { echo "ERROR: python3.11 missing"; exit 1; }

[[ -f "$SEED" ]] || { echo "ERROR: missing seed ${SEED}（正式包也必须留着当反推种子）"; exit 1; }
[[ -f "$KS" ]] || { echo "ERROR: missing keystore $KS"; exit 1; }

# so 由本脚本自己编（构建产物 obj/libs 不入库，收尾会被清掉 ⇒ 不能假设 SO_PREBUILT 已存在）
echo "[0/4] ndk-build → libs_$GAME_ID"
NDK="${ANDROID_NDK_HOME:-$HOME/Library/Android/sdk/ndk/27.0.12077973}"
NDK_BUILD="$NDK/ndk-build"
[[ -x "$NDK_BUILD" ]] || { echo "ERROR: missing $NDK_BUILD"; exit 1; }
(cd "$STARRY_SRC/app/src/main/jni" && \
  "$NDK_BUILD" NDK_PROJECT_PATH=../.. APP_BUILD_SCRIPT="./Android_$GAME_ID.mk" \
    NDK_APPLICATION_MK=./Application.mk \
    NDK_OUT="$STARRY_SRC/obj_$GAME_ID" NDK_LIBS_OUT="$STARRY_SRC/libs_$GAME_ID" \
    -j"$(sysctl -n hw.ncpu 2>/dev/null || echo 4)" >/tmp/jjssjd1382_reseed_ndk.log 2>&1) \
  || { echo "ERROR: ndk-build 失败"; tail -30 /tmp/jjssjd1382_reseed_ndk.log; exit 1; }
[[ -f "$SO_PREBUILT" ]] || { echo "ERROR: missing ${SO_PREBUILT}"; exit 1; }
LC_ALL=C grep -aq "[$TAG]" "$SO_PREBUILT" || { echo "ERROR: so 无 [$TAG] 标签（可能仍是上一款游戏）"; exit 1; }
LC_ALL=C grep -aq "$GAME_ID" "$SO_PREBUILT" || { echo "ERROR: so 无 game_id=$GAME_ID"; exit 1; }
LC_ALL=C grep -aq "秒杀" "$SO_PREBUILT" || { echo "ERROR: so 无菜单串（秒杀）—— 本次变更未编进去"; exit 1; }
echo "  so ok ($(ls -l "$SO_PREBUILT" | awk '{print $5}') B, [$TAG] + 秒杀 verified)"

# 种子身份复核（防拿错种子把上一版的注入物又叠一层）
GOT_MD5="$(md5 -q "$SEED")"
echo "[check] seed md5 = $GOT_MD5"
if [[ "$GOT_MD5" != "$SEED_MD5" ]]; then
  echo "ERROR: 种子 md5 与登记值不符（got=$GOT_MD5 want=${SEED_MD5}）"
  echo "      若已确认这是新版正式包，请同步更新本脚本 SEED_MD5/SEED_SHA256 后再跑。"
  exit 1
fi
echo "[check] seed identity ok ($SEED_SHA256)"

AVAIL_GB="$(df -g "$NIXIANG" | awk 'NR==2{print $4}')"
[[ "${AVAIL_GB:-0}" -ge 8 ]] || { echo "ERROR: 磁盘可用 ${AVAIL_GB:-0}G < 8G（峰值 = 种子+新包+对齐包三份并存）"; exit 1; }
echo "[check] disk avail=${AVAIL_GB}G ok"

LOCK_DIR="/tmp/jjssjd1382_reseed.lockdir"
mkdir "$LOCK_DIR" 2>/dev/null || { echo "ERROR: lock $LOCK_DIR held"; exit 1; }
echo $$ >"$LOCK_DIR/pid"
cleanup() {
  rm -rf "$LOCK_DIR" 2>/dev/null || true
  rm -f "$TMP_NEW" "$TMP_UNSIGNED" "$TMP_ALIGNED" 2>/dev/null || true
}
trap cleanup EXIT

echo "[1/4] stream merge（种子逐条字节透传，仅换 ${SFN}）"
python3.11 - "$SEED" "$TMP_NEW" "$SO_PREBUILT" <<'PY'
import sys, zipfile, hashlib, shutil, struct
from pathlib import Path

src, dst, so_path = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
SO_NAME = "lib/arm64-v8a/libstarry_magic.so"
so_data = so_path.read_bytes()

class RawNameZipInfo(zipfile.ZipInfo):
    """按原始字节写条目名（flag 不置 0x800）。
    Unity APK 里部分 AssetBundle 名是「UTF-8 字节但不带 UTF-8 flag」；python zipfile
    重写时会把名字重编码并强设 0x800 → Unity/Android zip 读取器路径对不上 →
    "Unable to open archive file" → 加载条卡死。"""
    _raw_name = b""
    _out_flags = 0x800
    def _encodeFilenameFlags(self):
        if self._raw_name:
            return self._raw_name, self._out_flags
        name, flags = super()._encodeFilenameFlags()
        return name, flags | self._out_flags

def make_info(item):
    raw = item.orig_filename.encode("cp437", "replace")
    info = RawNameZipInfo(item.orig_filename, item.date_time)
    if (item.flag_bits & 0x800) == 0 and raw != item.orig_filename.encode("ascii", "replace"):
        info._raw_name = raw
    info.compress_type = item.compress_type
    info.external_attr = item.external_attr
    info.create_system = 3
    # 0x08(data descriptor) 是有意归一化掉的位（我侧全部写真实 local header）
    info._out_flags = item.flag_bits & ~0x08
    return info

zin = zipfile.ZipFile(src, "r")
seed_names = set(zin.namelist())
assert SO_NAME in seed_names, f"种子内没有 {SO_NAME}（选错种子？）"
assert "AndroidManifest.xml" in seed_names and "resources.arsc" in seed_names

zout = zipfile.ZipFile(dst, "w")
seen, n = set(), 0
for item in zin.infolist():
    name = item.filename
    if name.startswith("META-INF/"):   # 旧签名整组丢弃，交 apksigner 重签
        continue
    if name == SO_NAME:                # 本轮唯一替换对象
        continue
    info = make_info(item)
    if name == "resources.arsc":
        info.compress_type = zipfile.ZIP_STORED   # Android 11+ 要求 arsc 未压缩
    with zin.open(item) as fsrc, zout.open(info, "w") as fdst:
        shutil.copyfileobj(fsrc, fdst, 1 << 20)
    seen.add(item.orig_filename)
    n += 1

sinfo = RawNameZipInfo(SO_NAME)
sinfo.compress_type = zipfile.ZIP_DEFLATED   # extractNativeLibs=true，DEFLATED 可安装
sinfo.create_system = 3
sinfo._out_flags = 0x800
zout.writestr(sinfo, so_data)
seen.add(SO_NAME)
zout.close(); zin.close()

def normalize_zip(path):
    """剥离 python zipfile 写入的 zip64 残留（ZIP64_LIMIT=2^31-1 会在 >2G 产物上触发）：
    还原逐条目真实 hoff（uint32 合法域）、vneed 回填本地头版本（消 BadPack header mismatch）、
    尾部改写纯经典 EOCD。不做这步 zipalign 36.0.0 走 zip64 路径会 "Unable to open"。"""
    f = open(path, "r+b")
    f.seek(0, 2); size = f.tell()
    f.seek(max(0, size - 65536)); tail = f.read()
    e = tail.rfind(b"PK\x05\x06")
    assert e >= 0, "EOCD not found"
    _, _, _, _, n2, cdsize, cdoff, clen = struct.unpack("<IHHHHIIH", tail[e:e+22])
    assert clen == 0, "unexpected EOCD comment"
    if cdoff == 0xFFFFFFFF or cdsize == 0xFFFFFFFF or n2 == 0xFFFF:
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
                if bid == 1: real = payload
                else: blocks.append((bid, payload))
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
    f.truncate(); f.close()
    print(f"  normalized: zip64 extras stripped/vneed fixed={fixed}, classic EOCD ok")

normalize_zip(dst)

def stream_sha(z, name):
    h = hashlib.sha256()
    with z.open(name) as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()

zin2 = zipfile.ZipFile(src, "r")
zout2 = zipfile.ZipFile(dst, "r")
out_names = set(zout2.namelist())

# 门禁 1：替换件 = 本次构建的 so，逐字节
so_want = hashlib.sha256(so_data).hexdigest()
so_got = stream_sha(zout2, SO_NAME)
assert so_want == so_got, f"so 不一致 want={so_want} got={so_got}"
print(f"  so ok: {SO_NAME} sha256={so_got[:16]}… ({len(so_data)} B)")

# 门禁 2：所有未触碰条目 meta / 名字字节 / flag 全量一致（字节级透传证明）
meta = raw = flag = 0
for item in zin2.infolist():
    name = item.orig_filename
    if name.startswith("META-INF/") or name == SO_NAME:
        continue
    assert name in out_names, f"seed entry lost: {name}"
    m = zout2.getinfo(name)
    if item.CRC != m.CRC or item.file_size != m.file_size or item.compress_type != m.compress_type:
        meta += 1
    if item.orig_filename.encode("cp437", "replace") != m.orig_filename.encode("cp437", "replace"):
        raw += 1
    if (item.flag_bits & ~0x08) != (m.flag_bits & ~0x08) or (m.flag_bits & 0x08):
        flag += 1
        if flag <= 3:
            print(f"    flagmismatch {name!r}: seed={item.flag_bits:#06x} new={m.flag_bits:#06x}")
assert meta == 0 and raw == 0 and flag == 0, f"meta={meta} rawname={raw} flag={flag}"
print(f"  passthrough ok: {n} 条种子条目逐字节透传（CRC/size/ctype/name-bytes/flag 全量一致）")

# 门禁 3：dex 集合 / arsc / 注入物齐备
seed_dex = sorted(x for x in seed_names if x.startswith("classes") and x.endswith(".dex"))
out_dex = sorted(x for x in out_names if x.startswith("classes") and x.endswith(".dex"))
assert seed_dex == out_dex, f"dex set changed: {seed_dex} -> {out_dex}"
assert zout2.getinfo("resources.arsc").compress_type == zipfile.ZIP_STORED, "arsc not stored"
assert "classes9.dex" in out_names, "classes9.dex（星光 smali）缺失"
zsig = sorted(x for x in out_names if x.startswith("META-INF/"))
assert zsig == [], f"新包不应含 META-INF（交重签）: {zsig}"
print(f"  dex ok: {len(out_dex)} 个 {out_dex}; arsc=STORED; 待重签")
zin2.close(); zout2.close()
PY

echo "[2/4] zipalign"
"$BT/zipalign" -f -p 4 "$TMP_NEW" "$TMP_ALIGNED"
"$BT/zipalign" -c 4 "$TMP_ALIGNED" || { echo "ERROR: aligned APK failed zipalign -c"; exit 1; }
rm -f "$TMP_NEW"

echo "[3/4] sign → $SIGNED"
"$BT/apksigner" sign --ks "$KS" --ks-key-alias "$KS_ALIAS" \
  --ks-pass "pass:$KS_PASS" --key-pass "pass:$KS_PASS" \
  --v4-signing-enabled false --out "$TMP_UNSIGNED" "$TMP_ALIGNED"
rm -f "$TMP_ALIGNED"
"$BT/apksigner" verify --print-certs "$TMP_UNSIGNED" 2>&1 | head -6 || true

echo "[4/4] 产物自检"
python3.11 - "$TMP_UNSIGNED" <<'PY'
import sys, zipfile
apk = sys.argv[1]
z = zipfile.ZipFile(apk)
names = set(z.namelist())
assert "lib/arm64-v8a/libstarry_magic.so" in names, "so missing"
assert "classes9.dex" in names, "classes9.dex missing"
assert z.getinfo("resources.arsc").compress_type == zipfile.ZIP_STORED, "arsc not stored"
sig = sorted(n for n in names if n.startswith("META-INF/"))
assert not any(n.endswith(("X7GAME.SF", "X7GAME.RSA", "X7GAME.DSA", "X7GAME.EC")) for n in sig), \
    f"官方旧签名残留: {sig}"
assert any(n.endswith(".SF") for n in sig) and any(n.endswith(".RSA") for n in sig), \
    f"缺重签后的 v1 签名: {sig}"
dexes = sorted(n for n in names if n.startswith("classes") and n.endswith(".dex"))
print(f"  dex: {dexes}")
print(f"  entries={len(names)} arsc=STORED v1sig={sig}")
PY
"$BT/zipalign" -c 4 "$TMP_UNSIGNED" >/dev/null && echo "  zipalign -c 4 ok"

# 移植前最后一步才覆盖旧正式包 —— 它同时是本轮的种子，必须活到门禁全过
mv -f "$TMP_UNSIGNED" "$SIGNED"
echo "  size  : $(ls -lh "$SIGNED" | awk '{print $5}')"
echo "  sha256: $(shasum -a 256 "$SIGNED" | awk '{print $1}')"
echo "  md5   : $(md5 -q "$SIGNED")"

if [[ "${STARRY_INSTALL:-0}" == "1" ]]; then
  # 覆盖安装（禁卸载：清数据 = Mod + 存档双杀）。装机后必须对设备包 md5 == 本地 APK
  REMOTE="/data/local/tmp/jjssjd1382_reseed.apk"
  "$ADB" -s "$SERIAL" push "$SIGNED" "$REMOTE"
  "$ADB" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
  "$ADB" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\" && rm -f \"$REMOTE\"'" \
    || "$ADB" -s "$SERIAL" install -r "$SIGNED"
  echo "log: $ADB -s $SERIAL logcat -s Mod_Menu:V StarrySignSpoof:V MeatSdk:V"
fi

echo "DONE: $SIGNED"
