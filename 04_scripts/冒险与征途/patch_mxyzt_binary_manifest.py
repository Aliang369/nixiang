#!/usr/bin/env python3
# 冒险与征途（mxyzt）— binary AndroidManifest.xml 补丁（纯 stdlib；本机无 apktool/aapt）
#
# 路线：**provider-only（分支 B）**——【不替换】官方 application@name 与 appComponentFactory：
#   官方 application@name = com.hdwj.afk.MainApplication（保留）
#   官方 appComponentFactory = androidx.core.app.CoreComponentFactory（保留）
# 补丁内容：
#   1) 字符串池重建（追加新串，旧串索引不变）+ resource map 追加新属性名 res id
#   2) <application> 内末尾新增 <service com.starry.magic.menu.MenuService>（含
#      foregroundServiceType=specialUse + PROPERTY_SPECIAL_USE_FGS_SUBTYPE）
#   3) 新增 <provider com.starry.magic.StarryInitProvider>（authorities=<pkg>.starryinit
#      + initOrder=2147483647：让 SignSpoof 早于登录 SDK 的 provider）
#   4) 补齐 uses-permission（SYSTEM_ALERT_WINDOW 官包已有则不重复；FOREGROUND_SERVICE
#      与 FOREGROUND_SERVICE_SPECIAL_USE 官包没有 → 追加）
# 参考实现：04_scripts/追迹迷城/patch_zjmc_binary_manifest_v2.py（等长槽替换版）；
# 本脚本扩展了「凭空新增元素」（service/provider）与嵌套子元素（property）。
#
# 用法: python patch_mxyzt_binary_manifest.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
PKG = "com.mxyzt.gzmy"
APP_NAME_KEEP = "com.hdwj.afk.MainApplication"
APP_FACTORY_KEEP = "androidx.core.app.CoreComponentFactory"
MENU_SERVICE = "com.starry.magic.menu.MenuService"
INIT_PROVIDER = "com.starry.magic.StarryInitProvider"
PROVIDER_AUTHORITY = PKG + ".starryinit"
INIT_ORDER = 2147483647
PERM_WANTED = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000          # FOREGROUND_SERVICE_TYPE_SPECIAL_USE
ATTR_RES_IDS = {"foregroundServiceType": 0x01010599}

CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
CHUNK_RESOURCE_MAP = 0x0180
VAL_STRING = 0x03
VAL_INT_DEC = 0x10
VAL_INT_HEX = 0x11
VAL_BOOLEAN = 0x12


def fail(msg):
    raise SystemExit("ERROR: " + msg)


class Pool:
    """重建后的字符串池视图：旧索引不变，新串追加在尾部。"""

    def __init__(self, data, sp, sp_size, count, flags, str_start):
        self.data = data
        self.sp = sp
        self.size = sp_size
        self.count = count
        self.flags = flags
        self.str_start = str_start

    def read(self, idx):
        off = struct.unpack_from("<I", self.data, self.sp + 28 + 4 * idx)[0]
        p = self.sp + self.str_start + off
        length = struct.unpack_from("<H", self.data, p)[0]
        p += 2
        if length & 0x8000:
            length2 = struct.unpack_from("<H", self.data, p)[0]
            p += 2
            length = ((length & 0x7FFF) << 16) | length2
        return self.data[p:p + 2 * length].decode("utf-16-le")


def main():
    if len(sys.argv) != 3:
        fail("usage: patch_mxyzt_binary_manifest.py <in.xml> <out.xml>")
    data = bytearray(open(sys.argv[1], "rb").read())

    sp = 8
    sp_type, _sp_hdrsz, sp_size = struct.unpack_from("<HHI", data, sp)
    if sp_type != 0x0001:
        fail("chunk@8 type=0x%X, expected string pool" % sp_type)
    count, style_count, flags, str_start, _style_start = struct.unpack_from("<IIIII", data, sp + 8)
    if style_count:
        fail("string pool has styles; unsupported")
    if flags & 0x100:
        fail("utf-8 pool; this script targets utf-16 pools")

    pool = Pool(data, sp, sp_size, count, flags, str_start)
    strings = [pool.read(i) for i in range(count)]
    print("[pool] count=%d flags=0x%X" % (count, flags))

    def ensure(text):
        if text in strings:
            return strings.index(text)
        strings.append(text)
        return len(strings) - 1

    idx_ns = ensure(ANDROID_NS)
    idx_name = ensure("name")
    idx_value = ensure("value")
    idx_exported = ensure("exported")
    idx_enabled = ensure("enabled")
    idx_authorities = ensure("authorities")
    idx_init_order = ensure("initOrder")
    idx_stop_with_task = ensure("stopWithTask")
    idx_fgst = ensure("foregroundServiceType")
    idx_special = ensure("specialUse")
    idx_service = ensure("service")
    idx_provider = ensure("provider")
    idx_property = ensure("property")
    idx_usesperm = ensure("uses-permission")
    idx_application = ensure("application")
    idx_svc_name = ensure(MENU_SERVICE)
    idx_prov_name = ensure(INIT_PROVIDER)
    idx_authority = ensure(PROVIDER_AUTHORITY)
    idx_prop_name = ensure("android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE")
    idx_prop_value = ensure("MOD menu overlay")
    idx_perm = [ensure(p) for p in PERM_WANTED]

    # ---- 字符串池重建 -------------------------------------------------------
    offsets = []
    blob = bytearray()
    for text in strings:
        offsets.append(len(blob))
        blob += struct.pack("<H", len(text)) + text.encode("utf-16-le") + b"\x00\x00"
    while len(blob) % 4:
        blob += b"\x00\x00"
    str_start_new = 28 + 4 * len(strings)
    new_pool = bytearray()
    new_pool += struct.pack("<HHI", 0x0001, 28, str_start_new + len(blob))
    new_pool += struct.pack("<IIIII", len(strings), 0, flags, str_start_new, 0)
    new_pool += b"".join(struct.pack("<I", off) for off in offsets)
    new_pool += blob
    delta = len(new_pool) - sp_size
    data[sp:sp + sp_size] = new_pool
    struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + delta)
    print("[pool] rebuilt %d->%d strings, chunk %d->%d B" % (count, len(strings), sp_size, len(new_pool)))

    # ---- resource map 追加 --------------------------------------------------
    p = sp + len(new_pool)
    t, _hs, sz = struct.unpack_from("<HHI", data, p)
    if t == CHUNK_RESOURCE_MAP:
        n_ids = (sz - 16) // 4
        extra_ids = [ATTR_RES_IDS.get(strings[i], 0) for i in range(n_ids, len(strings))]
        if any(extra_ids):
            chunk = b"".join(struct.pack("<I", v) for v in extra_ids)
            data[p + sz:p + sz] = chunk
            struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))
            struct.pack_into("<I", data, p + 4, sz + len(chunk))
            named = {strings[n_ids + k]: hex(v) for k, v in enumerate(extra_ids) if v}
            print("[map] resource map +%d entries: %s" % (len(extra_ids), named))
        else:
            print("[map] no new attr names with res ids, map untouched")
    else:
        fail("chunk after pool type=0x%X, expected resource map" % t)

    xml_start = sp + len(new_pool)

    # ---- chunk 遍历工具 ------------------------------------------------------
    def elem_attr_count(pos):
        return struct.unpack_from("<H", data, pos + 16 + 12)[0]

    def elem_attrs(pos):
        cnt = elem_attr_count(pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, pos + 16 + 8)
        arr = pos + 16 + attr_start
        out = []
        for i in range(cnt):
            ns_i, name_i, raw_i, _s, _r, dtype, ddata = struct.unpack_from(
                "<IIIHBBI", data, arr + i * attr_size)
            out.append((ns_i, name_i, raw_i, dtype, ddata))
        return out

    def find_string_attr(pos, name_idx):
        for _ns, an, raw_i, dtype, _d in elem_attrs(pos):
            if an == name_idx:
                if dtype != VAL_STRING:
                    return None
                return strings[raw_i] if raw_i < len(strings) else None
        return None

    def walk():
        """产 (chunk_pos, type, name_idx) 序列。"""
        pos = xml_start
        while pos + 8 <= len(data):
            t, _hs, sz = struct.unpack_from("<HHI", data, pos)
            if sz <= 0:
                break
            if t in (CHUNK_START_ELEMENT, CHUNK_END_ELEMENT):
                _ns, elem_name = struct.unpack_from("<II", data, pos + 16)
                yield pos, t, elem_name
            pos += sz

    def find_element(name_idx, nth=0):
        seen = 0
        for pos, t, en in walk():
            if t == CHUNK_START_ELEMENT and en == name_idx:
                if seen == nth:
                    return pos
                seen += 1
        return None

    def app_end_pos():
        """<application> 的配对 </application> chunk 位置。"""
        start = find_element(idx_application)
        if start is None:
            fail("application element not found")
        depth = 0
        for pos, t, en in walk():
            if pos < start:
                continue
            if t == CHUNK_START_ELEMENT and en == idx_application:
                depth += 1
            elif t == CHUNK_END_ELEMENT and en == idx_application:
                depth -= 1
                if depth == 0:
                    return pos
        fail("</application> not found")

    def insert_bytes(pos, chunk):
        data[pos:pos] = chunk
        struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))
        return len(chunk)

    def start_elem(name_idx, attrs):
        """attrs: list of (attr_name_idx, raw_idx, dtype, data)"""
        size = 16 + 20 + 20 * len(attrs)
        chunk = bytearray(struct.pack("<HHIIIIIHHHHHH",
                                      CHUNK_START_ELEMENT, 16, size,
                                      1, 0xFFFFFFFF,
                                      0xFFFFFFFF, name_idx,
                                      20, 20, len(attrs), 0, 0, 0))
        for an, raw_i, dtype, ddata in attrs:
            chunk += struct.pack("<IIIHBBI", idx_ns, an, raw_i, 8, 0, dtype, ddata)
        return bytes(chunk)

    def end_elem(name_idx):
        return struct.pack("<HHIIIII", CHUNK_END_ELEMENT, 16, 24, 1,
                           0xFFFFFFFF, 0xFFFFFFFF, name_idx)

    # ---- 自带幂等：已打过补丁的清单直接拒绝（避免二次叠加）----------------
    for pos, t, en in walk():
        if t == CHUNK_START_ELEMENT and en in (idx_service, idx_provider):
            nm = find_string_attr(pos, idx_name)
            if nm in (MENU_SERVICE, INIT_PROVIDER):
                fail("manifest already patched (found %s); 请从官方底包重新解 Manifest" % nm)

    # ---- 1) uses-permission 补齐（插到 <application> 之前）------------------
    existing_perms = set()
    for pos, t, en in walk():
        if t == CHUNK_START_ELEMENT and en == idx_usesperm:
            v = find_string_attr(pos, idx_name)
            if v:
                existing_perms.add(v)
    app_start = find_element(idx_application)

    def usesperm_chunk(perm_idx):
        return start_elem(idx_usesperm, [(idx_name, perm_idx, VAL_STRING, perm_idx)]) \
            + end_elem(idx_usesperm)

    added_perms = []
    for wanted, pidx in zip(PERM_WANTED, idx_perm):
        if wanted not in existing_perms:
            insert_bytes(app_start, usesperm_chunk(pidx))
            added_perms.append(wanted)
            app_start += len(usesperm_chunk(pidx))   # 位置随插入右移
    print("[axml] uses-permission: existing=%d, added=%s" % (len(existing_perms), added_perms))

    # ---- 2) 新增 <service> 与 <provider>（插到 </application> 之前）--------
    service_xml = start_elem(idx_service, [
        (idx_name, idx_svc_name, VAL_STRING, idx_svc_name),
        (idx_enabled, 0xFFFFFFFF, VAL_BOOLEAN, 1),
        (idx_exported, 0xFFFFFFFF, VAL_BOOLEAN, 0),
        (idx_fgst, idx_special, VAL_INT_HEX, FGS_TYPE_DATA),
        (idx_stop_with_task, 0xFFFFFFFF, VAL_BOOLEAN, 0),
    ]) + start_elem(idx_property, [
        (idx_name, idx_prop_name, VAL_STRING, idx_prop_name),
        (idx_value, idx_prop_value, VAL_STRING, idx_prop_value),
    ]) + end_elem(idx_property) + end_elem(idx_service)

    provider_xml = start_elem(idx_provider, [
        (idx_name, idx_prov_name, VAL_STRING, idx_prov_name),
        (idx_authorities, idx_authority, VAL_STRING, idx_authority),
        (idx_exported, 0xFFFFFFFF, VAL_BOOLEAN, 0),
        (idx_init_order, 0xFFFFFFFF, VAL_INT_DEC, INIT_ORDER),
    ]) + end_elem(idx_provider)

    end_app = app_end_pos()
    insert_bytes(end_app, service_xml)
    end_app += len(service_xml)
    insert_bytes(end_app, provider_xml)
    print("[axml] +<service %s> (%d B) +<provider %s> (%d B)" % (
        MENU_SERVICE, len(service_xml), INIT_PROVIDER, len(provider_xml)))

    # ---- 3) 自检（改完立刻复读；失败不落盘）--------------------------------
    def start_positions(name_idx):
        return [pos for pos, t, en in walk() if t == CHUNK_START_ELEMENT and en == name_idx]

    errs = []
    svc = [p for p in start_positions(idx_service)
           if find_string_attr(p, idx_name) == MENU_SERVICE]
    if not svc:
        errs.append("MenuService element missing after patch")
    else:
        names = [strings[an] for _ns, an, *_ in elem_attrs(svc[0])]
        if "foregroundServiceType" not in names:
            errs.append("MenuService 缺 foregroundServiceType")
        if not any(t == CHUNK_START_ELEMENT and en == idx_property for _p, t, en in walk()):
            errs.append("MenuService 缺 <property> 子元素")

    prov = [p for p in start_positions(idx_provider)
            if find_string_attr(p, idx_name) == INIT_PROVIDER]
    if not prov:
        errs.append("StarryInitProvider element missing after patch")
    else:
        if find_string_attr(prov[0], idx_authorities) != PROVIDER_AUTHORITY:
            errs.append("StarryInitProvider authorities 不等于 " + PROVIDER_AUTHORITY)
        an_names = [strings[an] for _ns, an, *_ in elem_attrs(prov[0])]
        if "initOrder" not in an_names:
            errs.append("StarryInitProvider 缺 initOrder")

    perms_now = set()
    for pos, t, en in walk():
        if t == CHUNK_START_ELEMENT and en == idx_usesperm:
            v = find_string_attr(pos, idx_name)
            if v:
                perms_now.add(v)
    for wanted in PERM_WANTED:
        if wanted not in perms_now:
            errs.append("权限缺失: " + wanted)

    app_now = find_element(idx_application)
    if find_string_attr(app_now, idx_name) != APP_NAME_KEEP:
        errs.append("application@name 被改动（必须保持 %s）" % APP_NAME_KEEP)
    idx_factory = strings.index("appComponentFactory") if "appComponentFactory" in strings else None
    if idx_factory is not None and find_string_attr(app_now, idx_factory) != APP_FACTORY_KEEP:
        errs.append("appComponentFactory 被改动（必须保持 %s）" % APP_FACTORY_KEEP)

    if errs:
        for e in errs:
            print("  FAIL " + e)
        fail("%d 处自检失败，未写出文件" % len(errs))
    print("[selfcheck] application@name/appComponentFactory 未改；service/provider/权限齐备")

    open(sys.argv[2], "wb").write(data)
    print("[done] %s (%d B)" % (sys.argv[2], len(data)))


if __name__ == "__main__":
    main()
