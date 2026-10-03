#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
敢达争锋对决 (gdzfdj) — 二进制 AndroidManifest.xml 补丁（纯 stdlib，本机无 apktool/aapt 时的唯一路线）

路线：**provider-only**（原版 <application> 没有任何 android:name，故完全不动 Application / appComponentFactory）
  1) 字符串池重建：旧串索引不变，新串追加尾部；resource map 为新增的 android: 属性名补 res id
  2) 追加 2 条 uses-permission（FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE；
     SYSTEM_ALERT_WINDOW 官包已有）
  3) <application> 末尾新增 <service com.starry.magic.menu.MenuService>（foregroundServiceType=specialUse
     + <property PROPERTY_SPECIAL_USE_FGS_SUBTYPE>）
  4) <application> 末尾新增 <provider com.starry.magic.StarryInitProvider>
     （authorities=<pkg>.starryinit + exported=false + initOrder=2147483647，让 SignSpoof 早于其它 provider）

用法:
    python patch_gdzfdj_manifest.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
"""
import struct
import sys

PKG = "com.kingnet.gundam.mg.normal"
MENU_SERVICE = "com.starry.magic.menu.MenuService"
INIT_PROVIDER = "com.starry.magic.StarryInitProvider"
PROVIDER_AUTHORITY = PKG + ".starryinit"
INIT_ORDER = 2147483647
FGS_TYPE_SPECIAL_USE = 0x40000000
FGS_SUBTYPE_PROP = "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
FGS_SUBTYPE_VALUE = "MOD menu overlay"
PERM_WANTED = [
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]

ANDROID_NS = "http://schemas.android.com/apk/res/android"
CHUNK_STRING_POOL = 0x0001
CHUNK_XML = 0x0003
CHUNK_RESOURCE_MAP = 0x0180
CHUNK_START_NS = 0x0100
CHUNK_END_NS = 0x0101
CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
NO_INDEX = 0xFFFFFFFF

# android 属性名 res id（用于 resmap）
ATTR_RES_IDS = {
    "name": 0x01010003,
    "exported": 0x01010010,
    "authorities": 0x01010018,
    "value": 0x01010024,
    "enabled": 0x0101000E,
    "initOrder": 0x01010036,
    "stopWithTask": 0x0101036A,
    "foregroundServiceType": 0x01010599,
}

VAL_STRING = 0x03
VAL_INT_DEC = 0x10
VAL_INT_HEX = 0x11
VAL_BOOLEAN = 0x12


def fail(msg):
    raise SystemExit("[gdzfdj-manifest] ERROR: " + msg)


class Doc:
    """AXML 文档：header / string pool / resource map / 节点序列。"""

    def __init__(self, data):
        self.data = data
        self.strings = []
        self.flags = 0
        self.resmap = {}          # strIdx -> resId
        self.resmap_len = 0
        self.nodes = []           # 见 parse()
        self._parse()

    # ---------- 解析 ----------
    def _parse(self):
        data = self.data
        if struct.unpack_from("<H", data, 0)[0] != CHUNK_XML:
            fail("file header is not 0x0003 (XML)")
        sp = 8
        typ, hdrsz, size = struct.unpack_from("<HHI", data, sp)
        if typ != CHUNK_STRING_POOL:
            fail("chunk@8 type=0x%X, expected string pool" % typ)
        count, style_count, flags, str_start, style_start = struct.unpack_from("<IIIII", data, sp + 8)
        if style_count:
            fail("string pool has styles; unsupported")
        if flags & 0x100:
            fail("utf-8 pool; this script targets utf-16 pools")
        self.flags = flags
        base = sp + str_start
        for i in range(count):
            off = struct.unpack_from("<I", data, sp + 28 + 4 * i)[0]
            p = base + off
            ln = struct.unpack_from("<H", data, p)[0]
            if ln & 0x8000:
                fail("long string in pool; unsupported")
            p += 2
            self.strings.append(data[p:p + ln * 2].decode("utf-16-le", "replace"))
        off = sp + size
        t2, h2, s2 = struct.unpack_from("<HHI", data, off)
        if t2 == CHUNK_RESOURCE_MAP:
            n = (s2 - 8) // 4
            self.resmap_len = n
            for i in range(n):
                rid = struct.unpack_from("<I", data, off + 8 + 4 * i)[0]
                if rid:
                    self.resmap[i] = rid
            off += s2
        # 节点遍历
        depth = 0
        stack = []
        while off + 8 <= len(data):
            typ, hdrsz, size = struct.unpack_from("<HHI", data, off)
            if size <= 0 or off + size > len(data):
                break
            if typ in (CHUNK_START_NS, CHUNK_END_NS):
                line, = struct.unpack_from("<I", data, off + 8)
                prefix, uri = struct.unpack_from("<II", data, off + 16)
                self.nodes.append({
                    "t": "ns_start" if typ == CHUNK_START_NS else "ns_end",
                    "line": line, "prefix": prefix, "uri": uri,
                })
            elif typ == CHUNK_START_ELEMENT:
                line, = struct.unpack_from("<I", data, off + 8)
                ns, name = struct.unpack_from("<II", data, off + 16)
                attr_start, attr_size, attr_count = struct.unpack_from("<HHH", data, off + 24)
                ididx, classidx, styleidx = struct.unpack_from("<HHH", data, off + 30)
                aoff = off + 16 + attr_start
                attrs = []
                for ai in range(attr_count):
                    a = aoff + ai * attr_size
                    ans, an, araw = struct.unpack_from("<III", data, a)
                    vsize, res0, dtype = struct.unpack_from("<HBB", data, a + 12)
                    vdata, = struct.unpack_from("<I", data, a + 16)
                    attrs.append([ans, an, araw, dtype, vdata])
                node = {
                    "t": "elem_start", "line": line, "ns": ns, "name": name,
                    "attrs": attrs, "id": ididx, "cls": classidx, "style": styleidx,
                    "depth": depth, "children": [], "end": None,
                }
                self.nodes.append(node)
                stack.append(node)
                depth += 1
            elif typ == CHUNK_END_ELEMENT:
                depth -= 1
                line, = struct.unpack_from("<I", data, off + 8)
                ns, name = struct.unpack_from("<II", data, off + 16)
                endnode = {"t": "elem_end", "line": line, "ns": ns, "name": name, "depth": depth}
                self.nodes.append(endnode)
                if stack:
                    stack.pop()["end"] = endnode
            else:
                self.nodes.append({"t": "raw", "raw": data[off:off + size]})
            off += size

    # ---------- 工具 ----------
    def sidx(self, s):
        try:
            return self.strings.index(s)
        except ValueError:
            self.strings.append(s)
            return len(self.strings) - 1

    def res_of(self, idx):
        return self.resmap.get(idx, 0)

    # ---------- 修改 ----------
    def ensure_permissions(self, want):
        have = set()
        for nd in self.nodes:
            if nd["t"] == "elem_start" and self.strings[nd["name"]] == "uses-permission":
                for a in nd["attrs"]:
                    if self.strings[a[1]] == "name" and a[3] == VAL_STRING:
                        have.add(self.strings[a[4]])
        missing = [p for p in want if p not in have]
        if not missing:
            print("[gdzfdj-manifest] permissions already present")
            return 0
        anchor = None
        for nd in self.nodes:
            if nd["t"] == "elem_start" and self.strings[nd["name"]] == "uses-permission":
                anchor = nd
                break
        if anchor is None:
            fail("no <uses-permission> anchor")
        new_nodes = []
        for p in missing:
            new_nodes.extend(self._mk_elem("uses-permission", [("name", VAL_STRING, self.sidx(p))]))
        pos = self.nodes.index(anchor)
        self.nodes[pos:pos] = new_nodes
        print("[gdzfdj-manifest] +permissions %s" % missing)
        return len(missing)

    def _mk_elem(self, name, attrs, ns_attr=True, children=(), line=0, depth=0):
        """构造 elem_start / elem_end 节点对（attrs: (strName, dtype, data) 或 (strName, dtype, data, rawStr)）。"""
        ns_idx = self.sidx(ANDROID_NS) if ns_attr else NO_INDEX
        rows = []
        for a in attrs:
            aname, dtype, data = a[0], a[1], a[2]
            ai = self.sidx(aname)
            if dtype == VAL_STRING:
                raw = self.sidx(self.strings[data]) if a[3:] == () else self.sidx(a[3])
            else:
                raw = NO_INDEX
            rows.append([ns_idx, ai, raw, dtype, data])
        start = {"t": "elem_start", "line": line, "ns": ns_idx, "name": self.sidx(name),
                 "attrs": rows, "id": 0, "cls": 0, "style": 0, "depth": depth, "children": [], "end": None}
        end = {"t": "elem_end", "line": line, "ns": ns_idx, "name": self.sidx(name), "depth": depth}
        start["end"] = end
        out = [start]
        for c in children:
            out.extend(c)
        out.append(end)
        return out

    def inject_components(self):
        app = None
        for nd in self.nodes:
            if nd["t"] == "elem_start" and self.strings[nd["name"]] == "application":
                app = nd
                break
        if app is None:
            fail("no <application> element")
        if any(n.get("t") == "elem_start" and self.strings[n["name"]] == "service"
               and any(self.strings[a[4]] == MENU_SERVICE for a in n["attrs"]
                       if a[3] == VAL_STRING) for n in self.nodes):
            print("[gdzfdj-manifest] already patched, skip")
            return False

        prop = self._mk_elem("property", [
            ("name", VAL_STRING, self.sidx(FGS_SUBTYPE_PROP)),
            ("value", VAL_STRING, self.sidx(FGS_SUBTYPE_VALUE)),
        ])
        svc = self._mk_elem("service", [
            ("name", VAL_STRING, self.sidx(MENU_SERVICE)),
            ("exported", VAL_BOOLEAN, 0),
            ("foregroundServiceType", VAL_INT_HEX, FGS_TYPE_SPECIAL_USE),
        ], children=[prop])
        prov = self._mk_elem("provider", [
            ("name", VAL_STRING, self.sidx(INIT_PROVIDER)),
            ("authorities", VAL_STRING, self.sidx(PROVIDER_AUTHORITY)),
            ("exported", VAL_BOOLEAN, 0),
            ("initOrder", VAL_INT_DEC, INIT_ORDER),
        ])
        # 插到 </application> 之前
        end_idx = self.nodes.index(app["end"])
        self.nodes[end_idx:end_idx] = svc + prov
        print("[gdzfdj-manifest] +MenuService +StarryInitProvider")
        return True

    # ---------- 序列化 ----------
    def build(self):
        out = bytearray()
        # XML 文件头 chunk：type=0x0003 headerSize=8，size = **整个文件长度**（AOSP ResXMLTree
        # 以该 size 作为数据总长去遍历 chunk；填 8 会导致 "failed to initialize ResXMLTree"）
        out += struct.pack("<HHI", CHUNK_XML, 8, 0)
        # 字符串池
        count = len(self.strings)
        str_data = bytearray()
        offsets = []
        for s in self.strings:
            offsets.append(len(str_data))
            b = s.encode("utf-16-le")
            str_data += struct.pack("<H", len(s)) + b + b"\x00\x00"
            while len(str_data) % 4:
                str_data += b"\x00"
        str_start = 28 + 4 * count
        pool_size = str_start + len(str_data)
        out += struct.pack("<HHI", CHUNK_STRING_POOL, 28, pool_size)
        out += struct.pack("<IIIII", count, 0, self.flags, str_start, 0)
        for o in offsets:
            out += struct.pack("<I", o)
        out += str_data
        # resource map（长度覆盖所有新属性名索引）
        need = self.resmap_len
        for i in range(len(self.strings)):
            if i in self.resmap:
                need = max(need, i + 1)
        rmap = bytearray()
        for i in range(need):
            rmap += struct.pack("<I", self.resmap.get(i, 0))
        out += struct.pack("<HHI", CHUNK_RESOURCE_MAP, 8, 8 + len(rmap))
        out += rmap
        # 节点
        for nd in self.nodes:
            t = nd["t"]
            if t == "ns_start" or t == "ns_end":
                out += struct.pack("<HHIII", CHUNK_START_NS if t == "ns_start" else CHUNK_END_NS,
                                   16, 24, nd["line"], NO_INDEX)
                out += struct.pack("<II", nd["prefix"], nd["uri"])
            elif t == "elem_start":
                n = len(nd["attrs"])
                size = 16 + 20 + 20 * n
                out += struct.pack("<HHIII", CHUNK_START_ELEMENT, 16, size, nd["line"], NO_INDEX)
                out += struct.pack("<II", nd["ns"], nd["name"])
                out += struct.pack("<HHHHHH", 20, 20, n, nd["id"], nd["cls"], nd["style"])
                for a in nd["attrs"]:
                    out += struct.pack("<III", a[0], a[1], a[2])
                    out += struct.pack("<HBB", 8, 0, a[3])
                    out += struct.pack("<I", a[4])
            elif t == "elem_end":
                out += struct.pack("<HHIII", CHUNK_END_ELEMENT, 16, 24, nd["line"], NO_INDEX)
                out += struct.pack("<II", nd["ns"], nd["name"])
            else:
                out += nd["raw"]
        struct.pack_into("<I", out, 4, len(out))   # 回填整文件长度
        return bytes(out)

    def finalize_resmap(self):
        """把新增 android: 属性名的 res id 写进 resmap。"""
        for i, s in enumerate(self.strings):
            if i < self.resmap_len:
                continue
            if s in ATTR_RES_IDS:
                self.resmap[i] = ATTR_RES_IDS[s]
        # 旧串若原本就在 resmap 里，保持不动


def main():
    if len(sys.argv) != 3:
        fail("usage: patch_gdzfdj_manifest.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>")
    src, dst = sys.argv[1], sys.argv[2]
    doc = Doc(open(src, "rb").read())
    print("[gdzfdj-manifest] parsed: strings=%d resmap=%d nodes=%d utf16 flags=0x%X" % (
        len(doc.strings), doc.resmap_len, len(doc.nodes), doc.flags))
    doc.ensure_permissions(PERM_WANTED)
    doc.inject_components()
    doc.finalize_resmap()
    blob = doc.build()
    open(dst, "wb").write(blob)
    print("[gdzfdj-manifest] wrote %s (%d bytes)" % (dst, len(blob)))

    # 自检：重新解析并核对
    chk = Doc(blob)
    names = [chk.strings[n["name"]] for n in chk.nodes if n["t"] == "elem_start"]
    for need in ("uses-permission", "service", "provider", "property"):
        if need not in names:
            fail("selfcheck: <%s> missing" % need)
    flat = "\n".join(chk.strings)
    for need in (MENU_SERVICE, INIT_PROVIDER, PROVIDER_AUTHORITY, FGS_SUBTYPE_PROP, FGS_SUBTYPE_VALUE,
                 "android.permission.FOREGROUND_SERVICE", "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"):
        if need not in flat:
            fail("selfcheck: string missing: %s" % need)
    # 新增属性名必须带 res id
    for nd in chk.nodes:
        if nd["t"] != "elem_start":
            continue
        for a in nd["attrs"]:
            an = chk.strings[a[1]]
            if an in ATTR_RES_IDS and a[0] != NO_INDEX:
                if chk.res_of(a[1]) != ATTR_RES_IDS[an]:
                    fail("selfcheck: attr %s resId=0x%08X want 0x%08X (idx %d)" % (
                        an, chk.res_of(a[1]), ATTR_RES_IDS[an], a[1]))
    if chk.strings[:len(doc.strings) - 0] and False:
        pass
    print("[gdzfdj-manifest] OK (selfcheck passed)")


if __name__ == "__main__":
    main()
