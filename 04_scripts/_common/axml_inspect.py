#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""AXML（二进制 AndroidManifest.xml）结构检查器 + resource map 断言。

为什么需要它：`aapt2 dump xmltree` 只能证明 AXML **结构合法**，
不能证明平台按 res id 取到了属性值 —— resource map 追加错位时
aapt2 照样正常复读，平台却读到 res id = 0（属性等于没写）。
本工具补上这一环：把 map 里每个属性名对应的 res id 读出来做断言。

用法：
  python3 04_scripts/_common/axml_inspect.py <AndroidManifest.xml> [选项]

  --filter a,b,c    只详细打印名字命中（逗号分隔，子串匹配）的元素；缺省全打
  --expect n=0xID   断言属性名 n 的 res id 等于 0xID（可重复给多次）
  --assert-map      android 命名空间下的属性必须带非 0 res id，
                    且已知属性名必须等于 AOSP 固定 id（防 resource map 错位）
  --no-dump         只做断言，不打印元素树
  --quiet           只打印 pool/map 摘要与断言结果

退出码：0 通过 / 1 断言失败 / 2 用法或解析错误
"""

import struct
import sys

CHUNK_STRING_POOL = 0x0001
CHUNK_XML = 0x0003
CHUNK_RES_MAP = 0x0180
START_ELEMENT = 0x0102
ANDROID_NS = "http://schemas.android.com/apk/res/android"

# AOSP 固定 res id（用于 --assert-map 的强校验：只要该属性名出现在池里就比对这些值）。
# ⚠ 表值必须来自真值来源，禁止凭记忆写（2026-10-10 教训：初版凭记忆写 7 条错了 5 条，
#   导致 6 个别的游戏的既有 Manifest 全被误判 FAIL）。真值唯一来源 = 本机 android.jar 的
#   resources.arsc，再生成命令：
#     aapt2=$ANDROID_HOME/build-tools/<ver>/aapt2
#     $aapt2 dump resources $ANDROID_HOME/platforms/android-<api>/android.jar \
#       | grep -E "resource 0x0101[0-9a-f]{4} attr/" \
#       | sed -E 's/.*resource (0x0101[0-9a-f]{4}) attr\/([A-Za-z_0-9]+).*/\2 \1/' | sort -u
KNOWN_ATTR_RES_ID = {
    "theme": 0x01010000,
    "label": 0x01010001,
    "icon": 0x01010002,
    "name": 0x01010003,
    "permission": 0x01010006,
    "enabled": 0x0101000E,
    "debuggable": 0x0101000F,
    "exported": 0x01010010,
    "process": 0x01010011,
    "authorities": 0x01010018,
    "launchMode": 0x0101001D,
    "screenOrientation": 0x0101001E,
    "configChanges": 0x0101001F,
    "value": 0x01010024,
    "noHistory": 0x0101022D,
    "minSdkVersion": 0x0101020C,
    "targetSdkVersion": 0x01010270,
    "allowBackup": 0x01010280,
    "hardwareAccelerated": 0x010102D3,
    "largeHeap": 0x0101035A,
    "stopWithTask": 0x0101036A,
    "supportsRtl": 0x010103AF,
    "banner": 0x010103F2,
    "extractNativeLibs": 0x010104EA,
    "usesCleartextTraffic": 0x010104EC,
    "resizeableActivity": 0x010104F6,
    "directBootAware": 0x01010505,
    "networkSecurityConfig": 0x01010527,
    "roundIcon": 0x0101052C,
    "appComponentFactory": 0x0101057A,
    "foregroundServiceType": 0x01010599,
    "requestLegacyExternalStorage": 0x01010603,
}


def _u16(b, o):
    return struct.unpack_from("<H", b, o)[0]


def _u32(b, o):
    return struct.unpack_from("<I", b, o)[0]


class Axml(object):
    def __init__(self, data):
        self.d = data
        self.pool = []
        self.map = []
        self.elements = []
        self.utf8 = True
        self._parse()

    # ---------- public helpers ----------
    def s(self, idx):
        if idx == 0xFFFFFFFF or idx < 0 or idx >= len(self.pool):
            return None
        return self.pool[idx]

    def res_id(self, name_idx):
        if name_idx < len(self.map):
            return self.map[name_idx]
        return 0

    # ---------- parsing ----------
    def _parse(self):
        d = self.d
        if len(d) < 8:
            raise ValueError("file too short (%d bytes)" % len(d))
        if _u16(d, 0) != CHUNK_XML:
            raise ValueError("not an AXML: first chunk type 0x%04x (expect 0x0003)" % _u16(d, 0))
        total = _u32(d, 4)
        if total > len(d):
            raise ValueError("declared size %d > file size %d" % (total, len(d)))
        off = 8
        while off + 8 <= len(d):
            ctype = _u16(d, off)
            # header size is at off+2 (u16), chunk size at off+4 (u32)
            csize = _u32(d, off + 4)
            if csize < 8 or off + csize > len(d):
                break
            if ctype == CHUNK_STRING_POOL:
                self._parse_pool(off)
            elif ctype == CHUNK_RES_MAP:
                n = (csize - 8) // 4
                self.map = list(struct.unpack_from("<%dI" % n, d, off + 8)) if n else []
            elif ctype == START_ELEMENT:
                self._parse_start_element(off)
            off += csize

    def _parse_pool(self, off):
        d = self.d
        cnt = _u32(d, off + 8)
        flags = _u32(d, off + 16)
        strings_start = _u32(d, off + 20)
        self.utf8 = (flags & 0x100) != 0
        base = off + strings_start
        out = []
        for i in range(cnt):
            p = base + _u32(d, off + 28 + 4 * i)
            try:
                if self.utf8:
                    n = d[p]
                    if n & 0x80:
                        n = ((n & 0x7F) << 8) | d[p + 1]
                        p += 2
                    else:
                        p += 1
                    out.append(d[p:p + n].decode("utf-8", "replace"))
                else:
                    n = _u16(d, p)
                    if n & 0x8000:
                        n = ((n & 0x7FFF) << 16) | _u16(d, p + 2)
                        p += 4
                    else:
                        p += 2
                    out.append(d[p:p + 2 * n].decode("utf-16-le", "replace"))
            except Exception:
                out.append("<decode-error@%d>" % i)
        self.pool = out

    def _parse_start_element(self, off):
        d = self.d
        line = _u32(d, off + 8)
        ns_idx = _u32(d, off + 16)
        name_idx = _u32(d, off + 20)
        attr_start = _u16(d, off + 24) or 20
        attr_size = _u16(d, off + 26) or 20
        attr_count = _u16(d, off + 28)
        attrs = []
        abase = off + 16 + attr_start
        for i in range(attr_count):
            a = abase + i * attr_size
            if a + attr_size > len(d):
                break
            ans = _u32(d, a)
            an = _u32(d, a + 4)
            raw = _u32(d, a + 8)
            dtype = d[a + 15]
            data = _u32(d, a + 16)
            attrs.append({
                "ns_idx": ans, "ns": self.s(ans),
                "name_idx": an, "name": self.s(an),
                "raw_idx": raw, "raw": self.s(raw),
                "dtype": dtype, "data": data,
            })
        self.elements.append({
            "line": line, "ns": self.s(ns_idx), "name": self.s(name_idx),
            "name_idx": name_idx, "attrs": attrs,
        })


def fmt_value(a):
    if a["raw"] is not None:
        return '"%s"' % a["raw"]
    t = a["dtype"]
    if t == 0x12:  # boolean
        return "true" if a["data"] else "false"
    if t == 0x10:
        return "0x%08x" % a["data"] if a["data"] > 9 else "%d" % a["data"]
    if t == 0x11:
        return "0x%08x" % a["data"]
    if t == 0x01:
        return "#%08x" % a["data"]
    return "0x%08x(type=0x%02x)" % (a["data"], t)


def main(argv):
    args = [a for a in argv[1:]]
    if not args:
        sys.stderr.write(__doc__)
        return 2
    path = args[0]
    filt = None
    expects = []
    assert_map = False
    dump = True
    quiet = False
    i = 1
    while i < len(args):
        a = args[i]
        if a == "--filter":
            i += 1
            filt = [x for x in args[i].split(",") if x]
        elif a == "--expect":
            i += 1
            k, _, v = args[i].partition("=")
            expects.append((k, int(v, 0)))
        elif a == "--assert-map":
            assert_map = True
        elif a == "--no-dump":
            dump = False
        elif a == "--quiet":
            quiet = True
            dump = False
        else:
            sys.stderr.write("unknown option: %s\n" % a)
            return 2
        i += 1

    try:
        with open(path, "rb") as fh:
            data = fh.read()
        ax = Axml(data)
    except Exception as e:
        print("[axml] ERROR %s: %s" % (path, e))
        return 2

    print("[axml] %s (%d B)" % (path, len(data)))
    print("[pool] %d strings (enc=%s)" % (len(ax.pool), "utf8" if ax.utf8 else "utf16"))
    print("[map ] %d entries (%s)"
          % (len(ax.map), "dense" if len(ax.map) == len(ax.pool) else "sparse: map/pool = %d/%d" % (len(ax.map), len(ax.pool))))
    print("[tree] %d start-element(s)" % len(ax.elements))

    if dump:
        for e in ax.elements:
            if filt and not any(f in (e["name"] or "") for f in filt):
                continue
            print("E: %s (line %d)" % (e["name"], e["line"]))
            for a in e["attrs"]:
                rid = ax.res_id(a["name_idx"])
                ns = "android:" if a["ns"] == ANDROID_NS else ("%s:" % a["ns"] if a["ns"] else "")
                print("    %s%-24s = %-28s {resid=0x%08x idx=%d}"
                      % (ns, a["name"], fmt_value(a), rid, a["name_idx"]))

    failures = []
    # --- 断言 1：--expect 显式期望值 ---
    for k, want in expects:
        if k not in ax.pool:
            failures.append("expect %s=0x%08x 但该属性名不在字符串池里" % (k, want))
            continue
        idx = ax.pool.index(k)
        got = ax.res_id(idx)
        if got != want:
            failures.append("expect %s: res id 期望 0x%08x 实际 0x%08x（idx=%d）" % (k, want, got, idx))
        else:
            print("[expect] OK %s (idx=%d) -> 0x%08x" % (k, idx, got))

    # --- 断言 2：android 命名空间属性必须带非 0 res id（防 map 错位） ---
    if assert_map:
        checked = 0
        seen = set()
        for e in ax.elements:
            for a in e["attrs"]:
                if a["ns"] != ANDROID_NS or a["name"] is None:
                    continue
                key = (a["name"], a["name_idx"])
                if key in seen:
                    continue
                seen.add(key)
                checked += 1
                got = ax.res_id(a["name_idx"])
                if got == 0:
                    failures.append("res id == 0: <android:%s> (idx=%d, 元素 <%s> line %d) —— resource map 错位或未重建"
                                    % (a["name"], a["name_idx"], e["name"], e["line"]))
                want = KNOWN_ATTR_RES_ID.get(a["name"])
                if want is not None and got != want:
                    failures.append("<android:%s> res id 期望 0x%08x 实际 0x%08x（idx=%d）—— map 语义应为「按字符串索引稠密排列」"
                                    % (a["name"], want, got, a["name_idx"]))
        print("[assert-map] 检查了 %d 个 android 命名空间属性" % checked)
        if len(ax.map) < len(ax.pool):
            print("[assert-map] NOTE map(%d) < pool(%d)：元素名/文本串不需要 res id，通常无害；"
                  "但补丁后新增的属性名必须落在 map 覆盖范围内" % (len(ax.map), len(ax.pool)))

    if failures:
        for f in failures:
            print("[FAIL] " + f)
        print("[result] FAIL (%d)" % len(failures))
        return 1
    if not quiet:
        print("[result] PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
