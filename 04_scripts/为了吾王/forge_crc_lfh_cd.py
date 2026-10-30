# -*- coding: utf-8 -*-
"""forge_crc 的本地文件头版：同时改写 LFH(+14) 与 CD(+16) 的 CRC32 为官方基线值。

为什么需要：apksigner 会校验「LFH CRC 与 CD CRC 必须一致」，只改 CD 会直接 ZipFormatException。
破解者包能过 TP 说明 TP 比对的是 zip 字段值而非真实内容 CRC。
"""
import struct
import sys
import zlib


def parse(zf):
    eocd = zf.rfind(b"PK\x05\x06")
    cd_size, cd_off = struct.unpack_from("<II", zf, eocd + 12)
    out = {}
    p = cd_off
    while p + 46 <= cd_off + cd_size and zf[p:p + 4] == b"PK\x01\x02":
        nlen, elen, clen = struct.unpack_from("<HHH", zf, p + 28)
        name = zf[p + 46:p + 46 + nlen].decode("utf-8", "replace")
        lho = struct.unpack_from("<I", zf, p + 42)[0]
        out[name] = {"cd": p, "crc": struct.unpack_from("<I", zf, p + 16)[0],
                     "csize": struct.unpack_from("<I", zf, p + 20)[0],
                     "usize": struct.unpack_from("<I", zf, p + 24)[0], "lfh": lho}
        p += 46 + nlen + elen + clen
    return out


def main():
    apk, base = sys.argv[1], sys.argv[2]
    entries = sys.argv[3:]
    a = bytearray(open(apk, "rb").read())
    b = open(base, "rb").read()
    ca, cb = parse(bytes(a)), parse(b)
    for e in entries:
        ea, eb = ca[e], cb[e]
        print(f"{e}: CRC {ea['crc']:#010x} → {eb['crc']:#010x}；"
              f"csize {ea['csize']}/{eb['csize']} usize {ea['usize']}/{eb['usize']} "
              f"(usize 必须相等，否则解压长度不符)")
        if ea["usize"] != eb["usize"]:
            raise SystemExit("usize 不同 ⇒ 改动太大，本探针不适用")
        struct.pack_into("<I", a, ea["cd"] + 16, eb["crc"])     # 中央目录
        struct.pack_into("<I", a, ea["lfh"] + 14, eb["crc"])    # 本地文件头
    open(apk, "wb").write(bytes(a))
    fin = open(apk, "rb").read()
    cf = parse(fin)
    for e in entries:
        e_ = cf[e]
        lho = e_["lfh"]
        nlen, elen = struct.unpack_from("<HH", fin, lho + 26)
        data_off = lho + 30 + nlen + elen
        raw = fin[data_off:data_off + e_["csize"]]
        method = struct.unpack_from("<H", fin, lho + 8)[0]
        body = zlib.decompress(raw, -15) if method == 8 else raw
        print(f"  复核 {e}: 解压 {len(body)} B 可读；CD/LFH 记的 CRC={e_['crc']:#010x}，"
              f"真实 CRC={zlib.crc32(body) & 0xFFFFFFFF:#010x}")


if __name__ == "__main__":
    main()
