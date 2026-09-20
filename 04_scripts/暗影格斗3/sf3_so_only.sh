#!/usr/bin/env bash
# 暗影格斗3 1.45.3 — so-only 迭代后端（pipeline surgical 阶段用）。
#
# 语义（见 _common/stages.md「调试内循环」）：已有外科底包，只换
# lib/arm64-v8a/libstarry_magic.so 再重签，不碰原版包、不重跑 manifest/dex。
# 适用：仅改 native（本次=HNAccessCore::checkCopyright 版权补丁）。
#
# 底包 = 从真机拉回的上一版正式包（== 已发布 releaseSigned，md5 fcb6c4f3…）。
# 原版 TapTap 包已被清理，故本次不能走完整 surgical_sf3_1.45.3.sh。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="暗影格斗3"
VERSION="1.45.3"
BASE_APK="${SF3_BASE_APK:-$NIXIANG/02_apks/$GAME/${GAME}_${VERSION}_starry_from_device.apk}"
BASE_MD5="${SF3_BASE_MD5:-fcb6c4f32a0a804a38567b142ffa7e93}"
SO="${SF3_SO:-$NIXIANG/mod菜单/starry-mod-menu/libs_sf3/arm64-v8a/libstarry_magic.so}"
OUT="$NIXIANG/08_output/$GAME"
UNSIGNED="$OUT/${GAME}_${VERSION}_surgical-unsigned.apk"

[[ -f "$BASE_APK" ]] || { echo "ERROR: missing base APK: $BASE_APK" >&2; exit 1; }
[[ -f "$SO" ]] || { echo "ERROR: missing so: $SO" >&2; exit 1; }

ACTUAL_MD5="$(md5 -q "$BASE_APK")"
[[ "$ACTUAL_MD5" == "$BASE_MD5" ]] \
  || { echo "ERROR: 底包指纹不符 got=$ACTUAL_MD5 want=$BASE_MD5" >&2; exit 1; }
LC_ALL=C grep -aq "sf3" "$SO" || { echo "ERROR: so 不含 sf3 标签" >&2; exit 1; }

mkdir -p "$OUT"
echo "[so_only] base = $BASE_APK ($ACTUAL_MD5)"
echo "[so_only] so   = $SO ($(md5 -q "$SO"))"

python3 - "$BASE_APK" "$SO" "$UNSIGNED" <<'PY'
import hashlib, struct, sys, zipfile
from pathlib import Path

src, so_path, dst = Path(sys.argv[1]), Path(sys.argv[2]), Path(sys.argv[3])
so = so_path.read_bytes()
STRIP = ('META-INF/MANIFEST.MF',)

# 只替换 starry so：原 dex/arsc/assets/其余 so 逐字节保持不变
with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'w') as zout:
    original_names = zin.namelist()
    for item in zin.infolist():
        name = item.filename
        if name in STRIP or (name.startswith('META-INF/')
                             and name.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if name == 'lib/arm64-v8a/libstarry_magic.so':
            info = zipfile.ZipInfo(name, item.date_time)
            info.compress_type = zipfile.ZIP_STORED
            info.external_attr = item.external_attr
            info.create_system = 3
            zout.writestr(info, so)
            continue
        data = zin.read(item)
        info = zipfile.ZipInfo(name, item.date_time)
        info.compress_type = zipfile.ZIP_STORED if name == 'resources.arsc' else item.compress_type
        info.external_attr = item.external_attr
        info.create_system = 3
        zout.writestr(info, data)

with zipfile.ZipFile(src, 'r') as zin, zipfile.ZipFile(dst, 'r') as zout:
    out_names = set(zout.namelist())
    unchanged = 0
    for n in original_names:
        if n in STRIP or (n.startswith('META-INF/')
                          and n.endswith(('.SF', '.RSA', '.DSA', '.EC'))):
            continue
        if n == 'lib/arm64-v8a/libstarry_magic.so':
            continue
        if hashlib.sha256(zin.read(n)).digest() != hashlib.sha256(zout.read(n)).digest():
            raise SystemExit(f'unchanged entry changed: {n}')
        unchanged += 1
    for n in ('classes.dex', 'classes7.dex', 'classes8.dex', 'resources.arsc',
              'lib/arm64-v8a/libil2cpp.so', 'lib/arm64-v8a/libunity.so',
              'assets/bin/Data/Managed/Metadata/global-metadata.dat'):
        if n not in out_names:
            raise SystemExit(f'{n} missing from output')
    got = zout.read('lib/arm64-v8a/libstarry_magic.so')
    if hashlib.sha256(got).digest() != hashlib.sha256(so).digest():
        raise SystemExit('packaged libstarry_magic.so != build output')
print(f'  so_only verified: {unchanged} 个原始条目字节不变 + 替换 libstarry_magic.so')

# >2G 包归一化（照搬 surgical_sf3_1.45.3.sh 已实证方案）：
# python zipfile 的 ZIP64_LIMIT=2^31-1，2.7G 输出会写 zip64 EOCD + 条目 zip64 extra，
# zipalign(36.0.0) 对 >2G 走 zip64 路径报 "Unable to open as zip archive"。
def normalize_zip(path):
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
    print(f'  normalized: zip64 extras fixed={fixed}, classic EOCD ok')


normalize_zip(str(dst))
PY

echo "[so_only] unsigned → $UNSIGNED"
echo "unsigned_md5=$(md5 -q "$UNSIGNED")"
echo "unsigned_size=$(stat -f '%z' "$UNSIGNED")"
