# -*- coding: utf-8 -*-
"""把指定条目的中央目录 CRC 改回官方基线值（TP 基线校验只比对 CD 里的 CRC，不校验内容）

背景：MOD 作者对 libil2cpp.so 就是这么干的（解压后 sha256 变了，但 zip 里的 CRC32 与原始值一致）。
Android 侧读条目（libziparchive）不校验 CRC，所以伪造后仍可正常解析/运行；
而 TP 的基线表比对的是 CD 里的 CRC ⇒ 伪造即通过。

用法: python forge_crc.py <apk> <base_apk> <entry> [<entry> ...]
"""
import struct
import sys
import zipfile


def cd_entries(zf_bytes):
    """返回 {name: (cd_header_offset, crc, csize, usize)}"""
    eocd = zf_bytes.rfind(b"PK\x05\x06")
    if eocd < 0:
        raise SystemExit("找不到 EOCD")
    cd_size, cd_off = struct.unpack_from("<II", zf_bytes, eocd + 12)
    out = {}
    p = cd_off
    while p + 46 <= cd_off + cd_size and zf_bytes[p:p + 4] == b"PK\x01\x02":
        nlen, elen, clen = struct.unpack_from("<HHH", zf_bytes, p + 28)
        crc = struct.unpack_from("<I", zf_bytes, p + 16)[0]
        csize, usize = struct.unpack_from("<II", zf_bytes, p + 20)
        name = zf_bytes[p + 46:p + 46 + nlen].decode("utf-8", "replace")
        out[name] = (p, crc, csize, usize)
        p += 46 + nlen + elen + clen
    return out


def main():
    apk, base = sys.argv[1], sys.argv[2]
    entries = sys.argv[3:]
    a = bytearray(open(apk, "rb").read())
    b = open(base, "rb").read()
    ca, cb = cd_entries(bytes(a)), cd_entries(b)
    for e in entries:
        if e not in ca or e not in cb:
            raise SystemExit(f"条目缺失: {e}")
        pa, crc_a, cs_a, us_a = ca[e]
        pb, crc_b, cs_b, us_b = cb[e]
        print(f"{e}: CRC {crc_a:#010x} → {crc_b:#010x} (原始基线)；size {cs_a} vs 基线 {cs_b}, usize {us_a} vs {us_b}")
        struct.pack_into("<I", a, pa + 16, crc_b)   # 只伪造 CRC；size 必须保持真实，否则 inflate 失败
        # 若 size 也不同，说明改动太大（TP 若比对 size 会失败）——打印出来供判断，不静默改
        if (cs_a, us_a) != (cs_b, us_b):
            print(f"  WARN: 压缩/未压缩大小与基线不同，若 TP 也比对 size 则会失败")
    open(apk, "wb").write(bytes(a))
    # 复核：绕过 CRC 校验，直接按 CD 的 size 解压，确认内容仍可读（Android 读条目不校验 CRC）
    import zlib
    final = open(apk, "rb").read()
    cfin = cd_entries(final)
    for e in entries:
        p, crc, csize, usize = cfin[e]
        nlen, elen, clen = struct.unpack_from("<HHH", final, p + 28)
        lho = struct.unpack_from("<I", final, p + 42)[0]
        # 本地文件头
        lnlen, lelen = struct.unpack_from("<HH", final, lho + 26)
        data_off = lho + 30 + lnlen + lelen
        raw = final[data_off:data_off + csize]
        method = struct.unpack_from("<H", final, lho + 8)[0]
        body = zlib.decompress(raw, -15) if method == 8 else raw
        real_crc = zlib.crc32(body) & 0xFFFFFFFF
        print(f"  复核 {e}: 解压 {len(body)} B 可读；CD 里 CRC={crc:#010x}（伪造），实际 CRC={real_crc:#010x}")
        assert len(body) == usize, f"{e} 解压长度与 CD 不符"


if __name__ == "__main__":
    main()
