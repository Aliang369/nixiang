#!/usr/bin/env python3
"""Raw-preserving ZIP surgery used by the 幻想武将模拟器 single-APK build.

The script reads a JSON spec:
  {"src": "...", "dst": "...", "changes": {"entry": "/path"}, "drops": []}
It keeps every untouched local record verbatim and only rebuilds changed
entries. The caller must restore any required original CRC fields afterwards.
"""
import json
import os
import struct
import sys
import zlib

LFH = b"PK\x03\x04"
CDH = b"PK\x01\x02"
EOCD = b"PK\x05\x06"


def parse(path):
    data = open(path, "rb").read()
    i = data.rfind(EOCD)
    if i < 0:
        raise SystemExit("no EOCD")
    n_total = struct.unpack("<H", data[i + 10:i + 12])[0]
    cd_size = struct.unpack("<I", data[i + 12:i + 16])[0]
    cd_off = struct.unpack("<I", data[i + 16:i + 20])[0]
    comment = data[i + 22:i + 22 + struct.unpack("<H", data[i + 20:i + 22])[0]]
    if i - cd_size != cd_off:
        print("WARN cd_off mismatch", i - cd_size, cd_off, file=sys.stderr)
    ents, p = [], cd_off
    for _ in range(n_total):
        assert data[p:p + 4] == CDH, (p, data[p:p + 4])
        (vmade, vneed, flags, method, mtime, mdate, crc, csize, usize,
         nlen, elen, clen, dstart, iattr, eattr, lho) = struct.unpack(
            "<HHHHHHIIIHHHHHII", data[p + 4:p + 46])
        name = data[p + 46:p + 46 + nlen]
        extra = data[p + 46 + nlen:p + 46 + nlen + elen]
        cmt = data[p + 46 + nlen + elen:p + 46 + nlen + elen + clen]
        ents.append(dict(
            vmade=vmade, vneed=vneed, flags=flags, method=method, mtime=mtime,
            mdate=mdate, crc=crc, csize=csize, usize=usize, name=name,
            extra=extra, cmt=cmt, iattr=iattr, eattr=eattr, lho=lho,
        ))
        p += 46 + nlen + elen + clen
    return data, ents, cd_off, cd_size, comment


def local_record(data, entry):
    p = entry["lho"]
    assert data[p:p + 4] == LFH
    nlen, elen = struct.unpack("<HH", data[p + 26:p + 30])
    end = p + 30 + nlen + elen + entry["csize"]
    if (entry["flags"] & 0x08) and data[end:end + 4] == b"PK\x07\x08":
        end += 16
    return data[p:end]


def build(src, dst, changes=None, additions=None, drops=None):
    changes = changes or {}
    additions = additions or []
    drops = set(drops or [])
    data, ents, cd_off, cd_size, comment = parse(src)
    out = bytearray()
    new_ents = []
    for entry in ents:
        name = entry["name"].decode("utf-8", "surrogateescape")
        if name in drops:
            continue
        if name in changes:
            payload = changes[name]
            if entry["method"] == 0:
                comp, method = payload, 0
            else:
                co = zlib.compressobj(9, zlib.DEFLATED, -15)
                comp = co.compress(payload) + co.flush()
                method = 8
            crc = zlib.crc32(payload) & 0xffffffff
            off = len(out)
            out += struct.pack(
                "<IHHHHHIIIHH", 0x04034B50, 20, entry["flags"] & ~0x08,
                method, entry["mtime"], entry["mdate"], crc, len(comp),
                len(payload), len(entry["name"]), 0) + entry["name"] + comp
            ne = dict(entry)
            ne.update(method=method, crc=crc, csize=len(comp),
                      usize=len(payload), lho=off, extra=b"",
                      flags=entry["flags"] & ~0x08)
            new_ents.append(ne)
        else:
            off = len(out)
            out += local_record(data, entry)
            ne = dict(entry)
            ne["lho"] = off
            new_ents.append(ne)

    for name, payload in additions:
        nb = name.encode()
        if name == "resources.arsc":
            comp, method = payload, 0
        else:
            co = zlib.compressobj(9, zlib.DEFLATED, -15)
            comp = co.compress(payload) + co.flush()
            method = 8
        crc = zlib.crc32(payload) & 0xffffffff
        off = len(out)
        out += struct.pack("<IHHHHHIIIHH", 0x04034B50, 20, 0, method,
                           0x6000, 0x5200, crc, len(comp), len(payload),
                           len(nb), 0) + nb + comp
        new_ents.append(dict(
            vmade=0x031E, vneed=20, flags=0, method=method, mtime=0x6000,
            mdate=0x5200, crc=crc, csize=len(comp), usize=len(payload),
            name=nb, extra=b"", cmt=b"", iattr=0, eattr=0o100644 << 16,
            lho=off,
        ))

    cd_start = len(out)
    for entry in new_ents:
        out += struct.pack(
            "<IHHHHHHIIIHHHHHII", 0x02014B50, entry["vmade"], entry["vneed"],
            entry["flags"], entry["method"], entry["mtime"], entry["mdate"],
            entry["crc"], entry["csize"], entry["usize"], len(entry["name"]),
            len(entry["extra"]), len(entry["cmt"]), 0, entry["iattr"],
            entry["eattr"], entry["lho"]) + entry["name"] + entry["extra"] + entry["cmt"]
    cd_len = len(out) - cd_start
    n = len(new_ents)
    out += struct.pack("<IHHHHIIH", 0x06054B50, 0, 0, n, n, cd_len,
                       cd_start, len(comment)) + comment
    with open(dst, "wb") as f:
        f.write(out)
    return n, cd_len


def main():
    spec = json.load(open(sys.argv[1], encoding="utf-8"))
    changes = {n: open(p, "rb").read() for n, p in spec.get("changes", {}).items()}
    additions = [(n, open(p, "rb").read()) for n, p in spec.get("adds", [])]
    n, cd_len = build(spec["src"], spec["dst"], changes, additions, spec.get("drops", []))
    print("entries=%d cd=%d out=%d" % (n, cd_len, os.path.getsize(spec["dst"])))


if __name__ == "__main__":
    main()
