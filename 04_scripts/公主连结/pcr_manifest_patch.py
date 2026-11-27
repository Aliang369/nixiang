#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# script-policy: {"purpose":"公主连结 11.7.2（com.bilibili.priconne，bilibili 官包）二进制 AndroidManifest.xml 最小注入：application@name=StarryApp + MenuService(FGS specialUse) + 两条权限","inputs":"官方底包 apk 内 AndroidManifest.xml（zipfile 单条取出）","outputs":"07_patched/公主连结/surgical/AndroidManifest.xml","boundary":"只改 Manifest 字节；不打包不签名；不碰 appComponentFactory；不删改任何官方节点/属性；不注入 StarryInitProvider","prior_art":"04_scripts/追迹迷城/patch_zjmc_binary_manifest_v3.py、04_scripts/999秒幸存者/patch_mxcz999_manifest.py、04_scripts/阿比斯少年冒险团/abssnmxt_manifest_patch.py","difference":"①入口用 application@name（不是 Provider）②字符串池走『原 blob 字节原样搬移 + 尾部追加』而不是整池重编码 ③resmap 支持稀疏 map（本包 map 47/pool 347）④自检含逐元素 diff：官方属性 (ns,name,resId,rawValue) 全等，差异只允许白名单","evidence":"10_docs/公主连结/硬约束.md；mod菜单/starry-mod-menu/app/src/main/AndroidManifest.xml（service 声明原件）；mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryApp.java（onCreate→MenuService.start）"}
"""公主连结（com.bilibili.priconne 11.7.2 bilibili 官包）— 二进制 AndroidManifest.xml 补丁（纯 stdlib）

为什么需要这个脚本：官方底包 Manifest 里 application@name = com.bilibili.priconne.App，
没有任何 com.starry.magic.* 节点。框架 StarryApp.onCreate 会 MenuService.start(ctx)，
未声明的 Service 直接抛 "Unable to start service ... not found"，所以必须同时注入：
  1) <application android:name="com.starry.magic.StarryApp">（super 由 smali patch 指向官方 App）
  2) <service com.starry.magic.menu.MenuService enabled/exported/foregroundServiceType/stopWithTask
        + <property PROPERTY_SPECIAL_USE_FGS_SUBTYPE> MOD 菜单悬浮窗服务…>
  3) uses-permission：SYSTEM_ALERT_WINDOW + FOREGROUND_SERVICE_SPECIAL_USE
     （底包已有 FOREGROUND_SERVICE ⇒ 不重复加）

实现要点（都是踩过的坑）：
  · 字符串池：**不整池重编码**。只把 offsets 数组扩 4*N 字节，原 string data blob 整块原样搬移
    （旧 offset 相对 blob 起点的值不变 ⇒ 旧索引语义与字节零改动），新串追加在 blob 尾部。
  · resource map 语义 = 按字符串索引定位（map[i] = 第 i 条串的 res id）。本包 map 是**稀疏**的
    （47 条 / 347 串，尾部被截断），所以给新属性名补 res id 时必须在 n_ids 与目标索引之间补 0，
    否则 foregroundServiceType / stopWithTask 会读成 0 = 属性等于没写（aapt2 复读照样正常）。
  · application@name 是**改值**不是加属性（官包已有该属性），原地改 rawValue + typedValue 两个
    u32 指向新串，属性条数与顺序完全不动。
  · 自检必须做「逐元素 diff」：只允许 application@name 值变化 + 1 service(含 property) + N 权限，
    其它任何增删改立刻 FAIL，且**不落盘**。

用法: python pcr_manifest_patch.py <in.xml> <out.xml> [--app-class com.starry.magic.StarryApp]
"""
import hashlib
import struct
import sys
from difflib import SequenceMatcher

ANDROID_NS = "http://schemas.android.com/apk/res/android"
DEFAULT_APP_CLASS = "com.starry.magic.StarryApp"
MENU_SERVICE = "com.starry.magic.menu.MenuService"
PERM_WANTED = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000          # FOREGROUND_SERVICE_TYPE_SPECIAL_USE
PROP_NAME = "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
PROP_VALUE = "MOD 菜单悬浮窗服务，需要持续显示悬浮窗供用户操作游戏辅助功能"

# 追加属性名的平台 res id。真值来源 = 本机 android.jar resources.arsc（见
# 04_scripts/_common/axml_inspect.py KNOWN_ATTR_RES_ID 同表）。写错 = 平台读成 0，静默失效。
ATTR_RES_IDS = {
    "foregroundServiceType": 0x01010599,
    "stopWithTask": 0x0101036A,
}

CHUNK_XML = 0x0003
CHUNK_STRING_POOL = 0x0001
CHUNK_RESOURCE_MAP = 0x0180
CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
VAL_STRING = 0x03
VAL_INT_DEC = 0x10
VAL_INT_HEX = 0x11
VAL_BOOLEAN = 0x12
NO_INDEX = 0xFFFFFFFF


def die(msg):
    raise SystemExit("ERROR: " + msg)


def sha(b):
    return hashlib.sha256(bytes(b)).hexdigest()


class Axml(object):
    """最小 AXML 解析器：串池 + resource map + start/end 元素（带文件偏移，便于原地改写）。"""

    def __init__(self, data, label="axml"):
        self.d = bytes(data)
        self.label = label
        self.pool = []
        self.map = []
        self.elements = []          # start element 记录（按文档顺序）
        self.chunks = []            # (type, off, header_size, chunk_size)
        self.pool_off = None
        self.map_off = None
        self._parse()

    # ---- 解析 -------------------------------------------------------------
    def _parse(self):
        d = self.d
        if len(d) < 8:
            die("%s: 文件太短 (%d B)" % (self.label, len(d)))
        ctype, chdr, csize = struct.unpack_from("<HHI", d, 0)
        if ctype != CHUNK_XML:
            die("%s: 不是 AXML（首 chunk type=0x%04X）" % (self.label, ctype))
        if csize != len(d):
            die("%s: 文件头 size=%d 与文件长度 %d 不自洽" % (self.label, csize, len(d)))
        off = 8
        depth = 0
        while off + 8 <= len(d):
            ct, chs, csz = struct.unpack_from("<HHI", d, off)
            if csz < 8 or off + csz > len(d):
                die("%s: chunk@%d 长度异常 (%d)" % (self.label, off, csz))
            self.chunks.append((ct, off, chs, csz))
            if ct == CHUNK_STRING_POOL:
                self._parse_pool(off)
            elif ct == CHUNK_RESOURCE_MAP:
                self.map_off = off
                n = (csz - 8) // 4
                self.map = list(struct.unpack_from("<%dI" % n, d, off + 8)) if n else []
            elif ct == CHUNK_START_ELEMENT:
                self._parse_start(off, depth)
                depth += 1
            elif ct == CHUNK_END_ELEMENT:
                depth -= 1
            off += csz
        if off != len(d):
            die("%s: chunk 累加 %d != 文件长度 %d" % (self.label, off, len(d)))
        if self.pool_off is None:
            die("%s: 没有字符串池" % self.label)

    def _parse_pool(self, off):
        d = self.d
        self.pool_off = off
        count, style_count, flags, strings_start, _styles_start = struct.unpack_from(
            "<IIIII", d, off + 8)
        self.pool_count = count
        self.style_count = style_count
        self.flags = flags
        self.strings_start = strings_start
        self.utf8 = bool(flags & 0x100)
        if self.utf8:
            die("%s: 串池是 UTF-8，本脚本只处理 UTF-16" % self.label)
        if style_count:
            die("%s: 串池带 style，不支持" % self.label)
        self.pool_off_array = off + 28
        for i in range(count):
            p = off + strings_start + struct.unpack_from("<I", d, off + 28 + 4 * i)[0]
            n = struct.unpack_from("<H", d, p)[0]
            p += 2
            if n & 0x8000:
                n = ((n & 0x7FFF) << 16) | struct.unpack_from("<H", d, p)[0]
                p += 2
            self.pool.append(d[p:p + 2 * n].decode("utf-16-le", "replace"))

    def _parse_start(self, off, depth):
        d = self.d
        _line, _cmt, ns_idx, name_idx = struct.unpack_from("<IIII", d, off + 8)
        attr_start, attr_size, attr_count = struct.unpack_from("<HHH", d, off + 24)
        attrs = []
        abase = off + 16 + attr_start
        for i in range(attr_count):
            a = abase + i * attr_size
            ans, an, raw = struct.unpack_from("<III", d, a)
            dv_size, _res0, dtype, ddata = struct.unpack_from("<HBBI", d, a + 12)
            attrs.append({
                "off": a, "ns_idx": ans, "ns": self.s(ans),
                "name_idx": an, "name": self.s(an),
                "raw_idx": raw, "raw": self.s(raw),
                "dtype": dtype, "data": ddata, "resid": self.resid(an),
                "attr_size": attr_size, "val_size": dv_size,
            })
        self.elements.append({
            "off": off, "depth": depth,
            "ns_idx": ns_idx, "name_idx": name_idx, "name": self.s(name_idx),
            "attr_start": attr_start, "attr_size": attr_size,
            "attr_count": attr_count, "attrs": attrs,
            "size": struct.unpack_from("<I", d, off + 4)[0],
        })

    # ---- 查询辅助 ---------------------------------------------------------
    def s(self, idx):
        if idx == NO_INDEX or idx >= len(self.pool):
            return None
        return self.pool[idx]

    def resid(self, name_idx):
        return self.map[name_idx] if name_idx < len(self.map) else 0

    def fmt(self, a):
        if a["raw_idx"] != NO_INDEX and a["raw"] is not None:
            return a["raw"]
        if a["dtype"] == VAL_BOOLEAN:
            return "true" if a["data"] else "false"
        if a["dtype"] in (VAL_INT_DEC, VAL_INT_HEX):
            return str(a["data"])
        return "0x%08X" % a["data"]

    def attr_sig(self, a):
        """官方属性全等判据：(ns, name, resId, 原始 rawValue, 类型, 值)。"""
        return (a["ns"], a["name"], a["resid"], a["raw"], a["dtype"], a["data"])

    def attr_map(self, e):
        return dict((a["name"], self.fmt(a)) for a in e["attrs"])

    def find(self, name, pred=None):
        for e in self.elements:
            if e["name"] == name and (pred is None or pred(e)):
                return e
        return None

    def find_all(self, name):
        return [e for e in self.elements if e["name"] == name]


def insert_bytes(data, pos, chunk):
    data[pos:pos] = chunk
    struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))


def pool_extend(data, new_strings):
    """串池尾部追加字符串：原 blob 字节原样搬移，旧 offset 值不变 ⇒ 旧索引语义不变。"""
    ax = Axml(data, "pool?")
    off, size = ax.pool_off, struct.unpack_from("<I", data, ax.pool_off + 4)[0]
    count = ax.pool_count
    flags = ax.flags
    strings_start = ax.strings_start
    if strings_start != 28 + 4 * count:
        die("串池 offsets 与 string data 之间有 %d 字节空隙，布局不受支持"
            % (strings_start - (28 + 4 * count)))
    blob = bytearray(data[off + strings_start: off + size])

    offsets = [struct.unpack_from("<I", data, off + 28 + 4 * i)[0] for i in range(count)]
    added = []
    for text in new_strings:
        if len(text) >= 0x8000:
            die("字符串过长，需双字长编码: %r" % text[:32])
        offsets.append(len(blob))
        blob += struct.pack("<H", len(text)) + text.encode("utf-16-le") + b"\x00\x00"
        added.append(count + len(added))
    while len(blob) % 4:
        blob += b"\x00"

    new_count = count + len(new_strings)
    new_start = 28 + 4 * new_count
    new_pool = bytearray()
    new_pool += struct.pack("<HHI", CHUNK_STRING_POOL, 28, new_start + len(blob))
    new_pool += struct.pack("<IIIII", new_count, 0, flags, new_start, 0)
    new_pool += b"".join(struct.pack("<I", o) for o in offsets)
    new_pool += blob
    delta = len(new_pool) - size
    data[off:off + size] = new_pool
    struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + delta)
    return count, new_count, size, len(new_pool), added


def map_extend(data, strings, first_new_index):
    """给新属性名补 res id。稀疏 map（尾部截断）必须补 0 到目标索引，不能直接接在末尾。"""
    ax = Axml(data, "map?")
    if ax.map_off is None:
        die("没有 resource map chunk，无法补 res id")
    off = ax.map_off
    size = struct.unpack_from("<I", data, off + 4)[0]
    n_ids = (size - 8) // 4
    want = dict((i, ATTR_RES_IDS[strings[i]])
                for i in range(first_new_index, len(strings))
                if strings[i] in ATTR_RES_IDS)
    if not want:
        return 0
    last = max(want)
    if last < n_ids:
        return 0
    chunk = b"".join(struct.pack("<I", want.get(i, 0)) for i in range(n_ids, last + 1))
    data[off + size: off + size] = chunk
    struct.pack_into("<I", data, off + 4, size + len(chunk))
    struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))
    return len(chunk) // 4


def start_elem(idx_ns, name_idx, attrs):
    size = 16 + 20 + 20 * len(attrs)
    chunk = bytearray(struct.pack(
        "<HHIIIIIHHHHHH",
        CHUNK_START_ELEMENT, 16, size,
        1, NO_INDEX,            # lineNumber / comment
        NO_INDEX, name_idx,     # ns / name
        20, 20, len(attrs), 0, 0, 0))
    for an, raw_i, dtype, ddata in attrs:
        chunk += struct.pack("<IIIHBBI", idx_ns, an, raw_i, 8, 0, dtype, ddata)
    return bytes(chunk)


def end_elem(name_idx):
    return struct.pack("<HHIIIII", CHUNK_END_ELEMENT, 16, 24, 1,
                       NO_INDEX, NO_INDEX, name_idx)


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")

    argv = sys.argv[1:]
    app_class = DEFAULT_APP_CLASS
    positional = []
    i = 0
    while i < len(argv):
        if argv[i] == "--app-class":
            if i + 1 >= len(argv):
                die("--app-class 缺值")
            app_class = argv[i + 1]
            i += 2
            continue
        positional.append(argv[i])
        i += 1
    if len(positional) != 2:
        die("usage: pcr_manifest_patch.py <in.xml> <out.xml> [--app-class X]")
    src, dst = positional
    if "." not in app_class or not all(c.isalnum() or c in "._$" for c in app_class):
        die("--app-class 不像 Java 类名: %r" % app_class)

    raw = open(src, "rb").read()
    data = bytearray(raw)
    orig = Axml(raw, "orig")
    print("[in ] %s (%d B, sha256=%s...)" % (src, len(raw), sha(raw)[:16]))
    print("[in ] pool=%d strings map=%d entries elements=%d"
          % (len(orig.pool), len(orig.map), len(orig.elements)))

    # ---- 0) 幂等 / 前置条件 -------------------------------------------------
    if MENU_SERVICE in orig.pool:
        die("manifest 已含 %s（已打过补丁）；请从底包重新取 Manifest" % MENU_SERVICE)
    if app_class in orig.pool:
        die("manifest 串池已含 %s：要么已打过补丁，要么 --app-class 与现有串重名；"
            "请从底包重新取 Manifest 或换类名" % app_class)
    app = orig.find("application")
    if app is None:
        die("找不到 <application>")
    if app["depth"] != 1:
        die("<application> 深度 = %d（期望 1）" % app["depth"])
    orig_app = app
    app_name_val = None
    for a in app["attrs"]:
        if a["name"] == "name" and a["ns"] == ANDROID_NS:
            app_name_val = a
    print("[in ] application@name=%s / appComponentFactory=%s"
          % (app_name_val["raw"] if app_name_val else "<无>",
             orig.attr_map(app).get("appComponentFactory", "<无>")))

    # ---- 1) 串池追加 -------------------------------------------------------
    wanted = [app_class, MENU_SERVICE, PROP_NAME, PROP_VALUE] + list(PERM_WANTED)
    for nm in ("name", "enabled", "exported", "foregroundServiceType", "specialUse",
               "stopWithTask", "service", "property", "uses-permission", "value"):
        if nm not in orig.pool:
            wanted.append(nm)
    n_old, n_new, old_sz, new_sz, added_idx = pool_extend(data, wanted)
    print("[pool] %d -> %d strings (chunk %d -> %d B)" % (n_old, n_new, old_sz, new_sz))

    ax = Axml(data, "p1")
    need = [app_class, MENU_SERVICE, PROP_NAME, PROP_VALUE] + list(PERM_WANTED) + [
        "name", "enabled", "exported", "foregroundServiceType", "specialUse",
        "stopWithTask", "service", "property", "uses-permission", "value"]
    missing = [t for t in need if t not in ax.pool]
    if missing:
        die("串池补入后仍缺: %r" % missing)
    idx = dict((t, ax.pool.index(t)) for t in need)
    idx_ns = ax.pool.index(ANDROID_NS)

    # ---- 2) resource map 补 res id ----------------------------------------
    n_map = map_extend(data, ax.pool, n_old)
    ax = Axml(data, "p2")
    for nm in ("foregroundServiceType", "stopWithTask"):
        got = ax.resid(ax.pool.index(nm))
        print("[map ] %-20s idx=%d resid=0x%08X %s"
              % (nm, ax.pool.index(nm), got, "OK" if got == ATTR_RES_IDS[nm] else "FAIL"))
        if got != ATTR_RES_IDS[nm]:
            die("resmap 补写失败: %s" % nm)
    print("[map ] 追加 %d 条 res id 条目（map %d -> %d）" % (n_map, len(orig.map), len(ax.map)))

    # ---- 3) uses-permission 补齐（<application> 之前）----------------------
    existing = set()
    for e in ax.find_all("uses-permission"):
        v = ax.attr_map(e).get("name")
        if v:
            existing.add(v)
    added_perms = [p for p in PERM_WANTED if p not in existing]
    if added_perms:
        chunk = b""
        for p in added_perms:
            pi = idx[p]
            chunk += start_elem(idx_ns, idx["uses-permission"],
                                [(idx["name"], pi, VAL_STRING, pi)]) \
                + end_elem(idx["uses-permission"])
        app = ax.find("application")
        insert_bytes(data, app["off"], chunk)
        print("[axml] uses-permission: 已有 %d 条, 追加 %d 条 %s"
              % (len(existing), len(added_perms), added_perms))
    else:
        print("[axml] uses-permission: 已齐备，无需追加")
    ax = Axml(data, "p3")

    # ---- 4) application@name -> app_class（改值，不增属性）-----------------
    app = ax.find("application")
    target = idx[app_class]
    hit = None
    for a in app["attrs"]:
        if a["name"] == "name" and a["ns"] == ANDROID_NS:
            hit = a
    if hit is not None:
        if hit["dtype"] != VAL_STRING:
            die("application@name 类型不是 string (0x%02X)" % hit["dtype"])
        old_val = hit["raw"]
        struct.pack_into("<I", data, hit["off"] + 8, target)
        struct.pack_into("<I", data, hit["off"] + 16, target)
        print("[axml] application@name: %s -> %s（原地改值，属性条数不变）" % (old_val, app_class))
    else:
        a_off = app["off"] + 16 + app["attr_start"] + app["attr_count"] * app["attr_size"]
        data[a_off:a_off] = struct.pack("<IIIHBBI", idx_ns, idx["name"], target, 8, 0,
                                        VAL_STRING, target)
        struct.pack_into("<H", data, app["off"] + 28, app["attr_count"] + 1)
        struct.pack_into("<I", data, app["off"] + 4, app["size"] + app["attr_size"])
        struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + app["attr_size"])
        print("[axml] application: 无 android:name，追加一条 -> %s" % app_class)
    ax = Axml(data, "p4")

    # ---- 5) </application> 之前插入 <service MenuService>（含 property）----
    app = ax.find("application")
    # chunk 扫描找 </application>（同名 end element）的文件偏移
    end_off = None
    d0 = 0
    p = app["off"]
    while p + 8 <= len(data):
        ct, _chs, csz = struct.unpack_from("<HHI", data, p)
        if ct == CHUNK_START_ELEMENT:
            d0 += 1
        elif ct == CHUNK_END_ELEMENT:
            d0 -= 1
            if d0 == 0:
                end_off = p
                break
        p += csz
    if end_off is None:
        die("找不到 </application>")

    svc = start_elem(idx_ns, idx["service"], [
        (idx["name"], idx[MENU_SERVICE], VAL_STRING, idx[MENU_SERVICE]),
        (idx["enabled"], NO_INDEX, VAL_BOOLEAN, 1),
        (idx["exported"], NO_INDEX, VAL_BOOLEAN, 0),
        (idx["foregroundServiceType"], idx["specialUse"], VAL_INT_HEX, FGS_TYPE_DATA),
        (idx["stopWithTask"], NO_INDEX, VAL_BOOLEAN, 0),
    ]) + start_elem(idx_ns, idx["property"], [
        (idx["name"], idx[PROP_NAME], VAL_STRING, idx[PROP_NAME]),
        (idx["value"], idx[PROP_VALUE], VAL_STRING, idx[PROP_VALUE]),
    ]) + end_elem(idx["property"]) + end_elem(idx["service"])
    insert_bytes(data, end_off, svc)
    print("[axml] +<service %s> (+<property %s>) %d B" % (MENU_SERVICE, PROP_NAME, len(svc)))

    # ---- 6) 复读自检（失败不落盘）----------------------------------------
    out = Axml(data, "out")
    errs = []

    # (a) application@name
    app = out.find("application")
    if app is None:
        errs.append("<application> 丢失")
    elif out.attr_map(app).get("name") != app_class:
        errs.append("application@name = %r != %r" % (out.attr_map(app).get("name"), app_class))

    # (b) service 节点（属性齐全 + property 子元素 + 父节点是 application）
    svc_e = None
    for e in out.elements:
        if e["name"] == "service" and out.attr_map(e).get("name") == MENU_SERVICE:
            svc_e = e
    if svc_e is None:
        errs.append("MenuService 未插入")
    else:
        m = out.attr_map(svc_e)
        for k, want in (("name", MENU_SERVICE), ("enabled", "true"), ("exported", "false"),
                        ("foregroundServiceType", "specialUse"), ("stopWithTask", "false")):
            if m.get(k) != want:
                errs.append("MenuService@%s = %r != %r" % (k, m.get(k), want))
        if svc_e["depth"] != 2:
            errs.append("MenuService 深度 = %d（应为 2，即 application 直接子节点）" % svc_e["depth"])
        nxt = None
        for k, e in enumerate(out.elements):
            if e is svc_e:
                nxt = out.elements[k + 1] if k + 1 < len(out.elements) else None
                break
        if nxt is None or nxt["name"] != "property" or nxt["depth"] != 3:
            errs.append("MenuService 的 <property> 子元素缺失或层级不对")
        else:
            pm = out.attr_map(nxt)
            if pm.get("name") != PROP_NAME:
                errs.append("property@name = %r" % pm.get("name"))
            if pm.get("value") != PROP_VALUE:
                errs.append("property@value = %r != %r" % (pm.get("value"), PROP_VALUE))

    # (c) 逐元素 diff：官方节点/属性只许多不许少、不许改（application@name 例外）
    def key(e):
        return (e["depth"], e["name"])

    o_keys = [key(e) for e in orig.elements]
    p_keys = [key(e) for e in out.elements]
    sm = SequenceMatcher(None, o_keys, p_keys, autojunk=False)
    value_changes, added_elems, lost, changed = [], [], [], []
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag == "equal":
            for oi, pj in zip(range(i1, i2), range(j1, j2)):
                oe, pe = orig.elements[oi], out.elements[pj]
                oa, pa = oe["attrs"], pe["attrs"]
                if len(oa) != len(pa):
                    changed.append("%s 属性条数 %d -> %d" % (oe["name"], len(oa), len(pa)))
                    continue
                for x, y in zip(oa, pa):
                    if orig.attr_sig(x) == out.attr_sig(y):
                        continue
                    same_slot = (x["ns"], x["name"], x["resid"], x["dtype"]) == \
                                (y["ns"], y["name"], y["resid"], y["dtype"])
                    if oe is orig_app and same_slot and x["name"] == "name":
                        value_changes.append((oe, x, y))
                    else:
                        changed.append("<%s>@%s: %r -> %r"
                                       % (oe["name"], x["name"], orig.fmt(x), out.fmt(y)))
        elif tag == "insert":
            for pj in range(j1, j2):
                added_elems.append(out.elements[pj])
        else:
            for oi in range(i1, i2):
                lost.append(orig.elements[oi])
            for pj in range(j1, j2):
                added_elems.append(out.elements[pj])

    # 允许的差异白名单
    allow_added = set()
    for p in added_perms:
        allow_added.add(("uses-permission", p))
    allow_added.add(("service", MENU_SERVICE))
    allow_added.add(("property", PROP_NAME))
    for e in added_elems:
        m = out.attr_map(e)
        tag = (e["name"], m.get("name"))
        if tag not in allow_added:
            errs.append("多出的元素不在白名单: <%s %s>" % tag)
    if len(value_changes) != 1:
        errs.append("application@name 值变化次数 = %d（应为 1）" % len(value_changes))
    else:
        oe, x, y = value_changes[0]
        if oe["name"] != "application" or y["raw"] != app_class:
            errs.append("值变化不是 application@name -> app_class")
    for s in changed:
        errs.append("官方属性被改动: " + s)
    for e in lost:
        errs.append("官方节点丢失: <%s> depth=%d" % (e["name"], e["depth"]))

    n_orig_attrs = sum(len(e["attrs"]) for e in orig.elements)
    n_out_attrs = sum(len(e["attrs"]) for e in out.elements)
    if n_out_attrs < n_orig_attrs:
        errs.append("属性总数减少 %d -> %d" % (n_orig_attrs, n_out_attrs))

    # (d) 结构自洽：头 size / chunk 长度 / 每个元素的 attrExt
    total = struct.unpack_from("<I", data, 4)[0]
    if total != len(data):
        errs.append("文件头 size=%d != 实际 %d" % (total, len(data)))
    walk_sum = 8
    p = 8
    while p + 8 <= len(data):
        ct, chs, csz = struct.unpack_from("<HHI", data, p)
        walk_sum += csz
        if ct == CHUNK_START_ELEMENT:
            ast, asz, acnt = struct.unpack_from("<HHH", data, p + 24)
            if chs != 16 or ast != 20 or asz != 20:
                errs.append("元素@%d header 异常 (%d/%d/%d)" % (p, chs, ast, asz))
            if csz != 16 + 20 + 20 * acnt:
                errs.append("元素@%d size=%d != %d (attrs=%d)" % (p, csz, 16 + 20 + 20 * acnt, acnt))
        elif ct == CHUNK_END_ELEMENT and (chs != 16 or csz != 24):
            errs.append("end 元素@%d header 异常 (%d/%d)" % (p, chs, csz))
        p += csz
    if walk_sum != len(data) or p != len(data):
        errs.append("chunk 累加 %d != 文件长度 %d" % (walk_sum, len(data)))

    # 串池：旧串逐条不变、resmap 前缀不变
    if out.pool[:len(orig.pool)] != orig.pool:
        errs.append("串池旧索引内容被改动")
    if out.map[:len(orig.map)] != orig.map:
        errs.append("resource map 前缀被改动")

    # ---- 差异清单 ---------------------------------------------------------
    print("[diff] 元素 %d -> %d（+%d）" % (len(orig.elements), len(out.elements),
                                          len(out.elements) - len(orig.elements)))
    print("[diff] 属性 %d -> %d（+%d）" % (n_orig_attrs, n_out_attrs, n_out_attrs - n_orig_attrs))
    for oe, x, y in value_changes:
        print("[diff] 值变更 <application>@android:name: %r -> %r" % (orig.fmt(x), out.fmt(y)))
    for e in added_elems:
        m = out.attr_map(e)
        order = [a["name"] for a in e["attrs"]]
        print("[diff] +<%s%s depth=%d>" % (e["name"], " " + " ".join(
            "%s=%r" % (k, m[k]) for k in order), e["depth"]))
    print("[diff] 官方节点被删=0 官方属性被改=0 白名单外新增=0（实际 %d/%d/%d）"
          % (len(lost), len(changed), len(added_elems)))

    if errs:
        for e in errs:
            print("  FAIL " + e)
        die("%d 处自检失败，未写出文件" % len(errs))

    open(dst, "wb").write(bytes(data))
    print("[done] %s (%d B, sha256=%s)" % (dst, len(data), sha(data)))


if __name__ == "__main__":
    main()
