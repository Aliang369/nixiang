#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
敢达争锋对决 (gdzfdj) — 原版二进制 AndroidManifest.xml 结构探针（纯 stdlib）

用法:
    python gdzfdj_axml_probe.py <in:AndroidManifest.xml> [--slots]

输出:
    1) 头部 + string pool 元信息（type/flags/count/style_count）
    2) 完整元素树（缩进）+ 每个属性的 name / resId / raw / typed
    3) --slots: 列出可作为「等长槽替换」的 service/activity/provider/receiver 名
       以及长度等于 len("com.starry.magic.menu.MenuService") 的槽位
"""
import struct
import sys

CHUNK_STRING_POOL = 0x0001
CHUNK_XML = 0x0003
CHUNK_RESOURCE_MAP = 0x0180
CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103

TYPE_NAMES = {
    0x00: "NULL", 0x01: "REFERENCE", 0x02: "ATTRIBUTE", 0x03: "STRING",
    0x04: "FLOAT", 0x05: "DIMENSION", 0x06: "FRACTION", 0x07: "DYNAMIC_REF",
    0x08: "DYNAMIC_ATTR", 0x10: "INT_DEC", 0x11: "INT_HEX", 0x12: "INT_BOOL",
    0x13: "INT_COLOR_ARGB8", 0x1C: "INT_COLOR_RGB8",
}

MENU_SERVICE = "com.starry.magic.menu.MenuService"


class Axml:
    def __init__(self, data):
        self.data = data
        self.strings = []
        self.resmap = {}
        self.sp_off = None
        self.sp_size = 0
        self.pool_utf8 = False
        self.style_count = 0
        self._parse_pool()

    def _parse_pool(self):
        data = self.data
        sp = 8
        typ, hdrsz, size = struct.unpack_from("<HHI", data, sp)
        if typ != CHUNK_STRING_POOL:
            raise SystemExit("chunk@8 type=0x%X, expected string pool" % typ)
        self.sp_off = sp
        self.sp_size = size
        count, style_count, flags, str_start, style_start = struct.unpack_from("<IIIII", data, sp + 8)
        self.pool_utf8 = bool(flags & 0x100)
        self.style_count = style_count
        base = sp + str_start
        for i in range(count):
            off = struct.unpack_from("<I", data, sp + 28 + 4 * i)[0]
            p = base + off
            if self.pool_utf8:
                # utf-8: 两段长度（u8/u16 变体）
                n = data[p]
                if n & 0x80:
                    n = ((n & 0x7F) << 8) | data[p + 1]
                    p += 2
                else:
                    p += 1
                ln = data[p]
                if ln & 0x80:
                    ln = ((ln & 0x7F) << 8) | data[p + 1]
                    p += 2
                else:
                    p += 1
                self.strings.append(data[p:p + ln].decode("utf-8", "replace"))
            else:
                ln = struct.unpack_from("<H", data, p)[0]
                p += 2
                if ln & 0x8000:
                    ln = ((ln & 0x7FFF) << 16) | struct.unpack_from("<H", data, p)[0]
                    p += 2 + ln * 2
                    self.strings.append("<utf16-long:%d>" % ln)
                    continue
                self.strings.append(data[p:p + ln * 2].decode("utf-16-le", "replace"))
        # resource map 可能存在（若 style_start 之前即有 chunk 0x0180）
        off = sp + size
        if off + 8 <= len(data):
            t2, h2, s2 = struct.unpack_from("<HHI", data, off)
            if t2 == CHUNK_RESOURCE_MAP:
                n = (s2 - 8) // 4
                for i in range(n):
                    rid = struct.unpack_from("<I", data, off + 8 + 4 * i)[0]
                    if i < len(self.strings):
                        self.resmap[i] = rid

    def s(self, i):
        if i == 0xFFFFFFFF or i >= len(self.strings):
            return None
        return self.strings[i]

    def walk(self, out):
        data = self.data
        off = self.sp_off + self.sp_size
        if off in (self.resmap and None,) :
            pass
        # 跳过 resource map chunk
        while off + 8 <= len(data):
            typ, hdrsz, size = struct.unpack_from("<HHI", data, off)
            if typ != CHUNK_RESOURCE_MAP:
                break
            off += size
        depth = 0
        while off + 8 <= len(data):
            typ, hdrsz, size = struct.unpack_from("<HHI", data, off)
            if size <= 0:
                break
            if typ == CHUNK_START_ELEMENT:
                line, = struct.unpack_from("<I", data, off + 8)
                ns, name = struct.unpack_from("<II", data, off + 16)
                attr_start, attr_size, attr_count = struct.unpack_from("<HHH", data, off + 24)
                aoff = off + 16 + attr_start
                nm = self.s(name) or "<name?>"
                out.append("%sE: %s   @0x%X (line %d, attrs %d)" % ("  " * depth, nm, off, line, attr_count))
                for ai in range(attr_count):
                    a = aoff + ai * attr_size
                    ans, an, araw = struct.unpack_from("<III", data, a)
                    vsize, res0, dtype = struct.unpack_from("<HBB", data, a + 12)
                    vdata, = struct.unpack_from("<I", data, a + 16)
                    aname = self.s(an)
                    rid = self.resmap.get(an, 0) if an < len(self.strings) else 0
                    if dtype == 0x03:
                        val = '"%s"' % (self.s(vdata) or "")
                    elif dtype in (0x10, 0x11):
                        val = "0x%X" % vdata
                    elif dtype == 0x12:
                        val = "true" if vdata else "false"
                    else:
                        val = "%s(0x%X)" % (TYPE_NAMES.get(dtype, "?"), vdata)
                    nsname = self.s(ans) if ans != 0xFFFFFFFF else None
                    out.append("%s  A: %s%s%s = %s  [nameIdx=%d resId=0x%08X raw=%s]" % (
                        "  " * depth,
                        ("[%s]" % nsname) if nsname else "",
                        aname, "" if not aname else "", val, an, rid,
                        '"%s"' % self.s(araw) if araw != 0xFFFFFFFF else "-"))
                depth += 1
            elif typ == CHUNK_END_ELEMENT:
                depth -= 1
                ns, name = struct.unpack_from("<ii", data, off + 16)
                out.append("%s/E: %s" % ("  " * depth, self.s(name)))
            else:
                out.append("%s%s chunk type=0x%X size=%d @0x%X" % (
                    "  " * depth, "RESOURCE_MAP" if typ == CHUNK_RESOURCE_MAP else "?", typ, size, off))
            off += size
        return out


def main():
    if len(sys.argv) < 2:
        raise SystemExit("usage: gdzfdj_axml_probe.py <AndroidManifest.xml> [--slots]")
    path = sys.argv[1]
    show_slots = "--slots" in sys.argv
    data = open(path, "rb").read()
    a = Axml(data)
    print("== file: %s (%d bytes)" % (path, len(data)))
    print("string pool @%d size=%d count=%d styles=%d utf8=%s resmap_entries=%d" % (
        a.sp_off, a.sp_size, len(a.strings), a.style_count, a.pool_utf8, len(a.resmap)))
    out = []
    a.walk(out)
    print("== tree (%d lines)" % len(out))
    print("\n".join(out))
    if show_slots:
        print("== slots ==")
        for want in (MENU_SERVICE, "com.starry.magic.StarryApp", "com.starry.magic.StarryInitProvider"):
            print("  target %-45s len=%d" % (want, len(want)))
        for i, s in enumerate(a.strings):
            if s and ("." in s) and (s.startswith("com.") or ".sdk." in s or ".MainApplication" in s):
                if 20 <= len(s) <= 70:
                    print("  str[%d] len=%d %s" % (i, len(s), s))


if __name__ == "__main__":
    main()
