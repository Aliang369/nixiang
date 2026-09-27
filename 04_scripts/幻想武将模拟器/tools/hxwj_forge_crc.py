#!/usr/bin/env python3
"""Patch a named ZIP entry's CRC field in both local and central records."""
import struct
import sys

LFH = b"PK\x03\x04"
CDH = b"PK\x01\x02"
EOCD = b"PK\x05\x06"


def main():
    path, name, crc = sys.argv[1], sys.argv[2].encode(), int(sys.argv[3], 16)
    data = bytearray(open(path, "rb").read())
    eocd = data.rfind(EOCD)
    total = struct.unpack("<H", data[eocd + 10:eocd + 12])[0]
    cd_off = struct.unpack("<I", data[eocd + 16:eocd + 20])[0]
    p, done = cd_off, 0
    for _ in range(total):
        assert data[p:p + 4] == CDH
        vals = struct.unpack("<HHHHHHIIIHHHHHII", data[p + 4:p + 46])
        nlen, elen, clen, lho = vals[9], vals[10], vals[11], vals[15]
        entry_name = bytes(data[p + 46:p + 46 + nlen])
        if entry_name == name:
            struct.pack_into("<I", data, p + 16, crc)
            assert data[lho:lho + 4] == LFH
            struct.pack_into("<I", data, lho + 14, crc)
            done += 1
        p += 46 + nlen + elen + clen
    if done != 1:
        raise SystemExit("expected exactly one CRC record, got %d" % done)
    with open(path, "wb") as f:
        f.write(data)
    print("forged crc %#x on %s" % (crc, name.decode()))


if __name__ == "__main__":
    main()
