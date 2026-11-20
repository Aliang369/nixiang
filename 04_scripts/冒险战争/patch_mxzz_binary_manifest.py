#!/usr/bin/env python3
# 冒险战争 (com.mxzz.zbml) — binary AndroidManifest.xml 补丁
# 纯 stdlib，不依赖 apktool / aapt2（对 1.4 GB 底包不做整包 apktool d/b）。
#
# 底子：04_scripts/霸海争锋/patch_bhzf_binary_manifest.py（同源引擎、同 Application 类）。
# 本作差异（三处，全部实测得来）：
#   1) **resource map 是稀疏的**：本包 341 strings / map 只有 41 项（尾部零被截断），
#      霸海版脚本按「map 与 pool 等长」假设直接追加 ⇒ 在这里会把 300 个零条目的位置错位。
#      本脚本先把 map 补齐到 len(strings)（中间填 0），再在**新串自身索引**处写 res id。
#   2) 属性 res id 现取现证：foregroundServiceType=0x01010599 / stopWithTask=0x0101036a
#      （命令：aapt2 dump resources <sdk>/platforms/android-35/android.jar | grep 同名；2026-11-19 实测）。
#   3) provider authority 用本包名：com.mxzz.zbml.starryinit。
#
# 不动的部分：<application android:name> 保持 com.unity3d.main.GameApplication
#   （官方 SDK 桥；入口一律走 ContentProvider，其 onCreate 自行 loadLibrary + 拉起 MenuService）。
#
# 用法: python patch_mxzz_binary_manifest.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
APP_NAME = "com.unity3d.main.GameApplication"   # 官方 Application，保持不动
MENU_SERVICE = "com.starry.magic.menu.MenuService"
PROVIDER_NAME = "com.starry.magic.StarryInitProvider"
PROVIDER_AUTH = "com.mxzz.zbml.starryinit"
PERM_WANTED = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000          # FOREGROUND_SERVICE_TYPE_SPECIAL_USE
PROP_NAME = "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
PROP_VALUE = "MOD menu overlay"
ATTR_RES_IDS = {
    "foregroundServiceType": 0x01010599,
    "stopWithTask": 0x0101036A,
}
NO_RAW = 0xFFFFFFFF

CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
CHUNK_RESOURCE_MAP = 0x0180
VAL_STRING = 0x03
VAL_INT_HEX = 0x11
VAL_BOOLEAN = 0x12


def fail(msg):
    raise SystemExit("ERROR: " + msg)


def main():
    if len(sys.argv) != 3:
        fail("usage: patch_mxzz_binary_manifest.py <in.xml> <out.xml>")
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

    def read_str(idx):
        off = struct.unpack_from("<I", data, sp + 28 + 4 * idx)[0]
        p = sp + str_start + off
        length = struct.unpack_from("<H", data, p)[0]
        p += 2
        if length & 0x8000:
            length2 = struct.unpack_from("<H", data, p)[0]
            p += 2
            length = ((length & 0x7FFF) << 16) | length2
        return data[p:p + 2 * length].decode("utf-16-le")

    strings = [read_str(i) for i in range(count)]
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
    idx_stop_with_task = ensure("stopWithTask")
    idx_fgst = ensure("foregroundServiceType")
    idx_service = ensure("service")
    idx_property = ensure("property")
    idx_usesperm = ensure("uses-permission")
    idx_application = ensure("application")
    idx_provider = ensure("provider")
    idx_authorities = ensure("authorities")
    idx_provider_name = ensure(PROVIDER_NAME)
    idx_provider_auth = ensure(PROVIDER_AUTH)
    idx_svc_name = ensure(MENU_SERVICE)
    idx_prop_name = ensure(PROP_NAME)
    idx_prop_value = ensure(PROP_VALUE)
    idx_perm = [ensure(p) for p in PERM_WANTED]

    # ---- 字符串池重建（旧索引不变，新串追加尾部）----------------------------
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
    print("[pool] rebuilt %d->%d strings, chunk %d->%d B" % (
        count, len(strings), sp_size, len(new_pool)))

    # ---- resource map：稀疏表补零到 len(strings)，再给新属性名写 res id --------
    p = sp + len(new_pool)
    t, _hs, sz = struct.unpack_from("<HHI", data, p)
    if t != CHUNK_RESOURCE_MAP:
        fail("chunk after pool type=0x%X, expected resource map" % t)
    n_old = (sz - 8) // 4
    old_ids = [struct.unpack_from("<I", data, p + 8 + i * 4)[0] for i in range(n_old)]
    print("[map] 原始 %d 项 / 字符串 %d 条（稀疏：尾部零被截断）" % (n_old, count))
    want = list(old_ids) + [0] * (len(strings) - n_old)
    for an, rid in ATTR_RES_IDS.items():
        want[strings.index(an)] = rid
    growth = len(want) * 4 + 8 - sz
    data[p:p + sz] = struct.pack("<HHI", CHUNK_RESOURCE_MAP, 8, 8 + len(want) * 4) + \
                     b"".join(struct.pack("<I", v) for v in want)
    struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + growth)
    print("[map] 补齐为 %d 项（+%d B）；新属性 res id: %s" % (
        len(want), growth, {k: hex(v) for k, v in ATTR_RES_IDS.items()}))

    xml_start = sp + len(new_pool)

    # ---- chunk 遍历工具 -----------------------------------------------------
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
        pos = xml_start
        while pos + 8 <= len(data):
            t2, _hs2, sz2 = struct.unpack_from("<HHI", data, pos)
            if sz2 <= 0:
                break
            if t2 in (CHUNK_START_ELEMENT, CHUNK_END_ELEMENT):
                _ns, elem_name = struct.unpack_from("<II", data, pos + 16)
                yield pos, t2, elem_name
            pos += sz2

    def find_element(name_idx, nth=0):
        seen = 0
        for pos, t2, en in walk():
            if t2 == CHUNK_START_ELEMENT and en == name_idx:
                if seen == nth:
                    return pos
                seen += 1
        return None

    def app_end_pos():
        start = find_element(idx_application)
        if start is None:
            fail("application element not found")
        depth = 0
        for pos, t2, en in walk():
            if pos < start:
                continue
            if t2 == CHUNK_START_ELEMENT and en == idx_application:
                depth += 1
            elif t2 == CHUNK_END_ELEMENT and en == idx_application:
                depth -= 1
                if depth == 0:
                    return pos
        fail("</application> not found")

    def insert_bytes(pos, chunk):
        data[pos:pos] = chunk
        struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))
        return len(chunk)

    def start_elem(name_idx, attrs):
        size = 16 + 20 + 20 * len(attrs)
        chunk = bytearray(struct.pack(
            "<HHIIIIIHHHHHH",
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

    # ---- 0) 幂等：已打过补丁的清单直接拒绝 --------------------------------
    for pos, t2, en in walk():
        if t2 == CHUNK_START_ELEMENT and en == idx_service:
            if find_string_attr(pos, idx_name) == MENU_SERVICE:
                fail("manifest already patched (MenuService present); 请从官方底包重新解 Manifest")
    for p2, t2, e2 in walk():
        if t2 == CHUNK_START_ELEMENT and e2 == idx_provider and \
                find_string_attr(p2, idx_name) == PROVIDER_NAME:
            fail("manifest already patched (StarryInitProvider present)；请从官方底包重新解 Manifest")

    # ---- 1) uses-permission 补齐（插到 <application> 之前）----------------
    existing_perms = set()
    for pos, t2, en in walk():
        if t2 == CHUNK_START_ELEMENT and en == idx_usesperm:
            v = find_string_attr(pos, idx_name)
            if v:
                existing_perms.add(v)

    def usesperm_chunk(perm_idx):
        return start_elem(idx_usesperm, [(idx_name, perm_idx, VAL_STRING, perm_idx)]) \
            + end_elem(idx_usesperm)

    app_start = find_element(idx_application)
    if app_start is None:
        fail("application element not found")
    added_perms = []
    for wanted, pidx in zip(PERM_WANTED, idx_perm):
        if wanted not in existing_perms:
            chunk = usesperm_chunk(pidx)
            insert_bytes(app_start, chunk)
            app_start += len(chunk)
            added_perms.append(wanted)
    print("[axml] uses-permission: existing=%d, added=%s" % (len(existing_perms), added_perms))

    # ---- 2) 入口点：<application> 首个子元素插入 <provider StarryInitProvider>
    def elem_attrs_end(elem_pos):
        cnt = elem_attr_count(elem_pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, elem_pos + 16 + 8)
        return elem_pos + 16 + attr_start + cnt * attr_size

    app_now = find_element(idx_application)   # 前面插过权限，必须重新定位
    provider_xml = start_elem(idx_provider, [
        (idx_name, idx_provider_name, VAL_STRING, idx_provider_name),
        (idx_authorities, idx_provider_auth, VAL_STRING, idx_provider_auth),
        (idx_exported, NO_RAW, VAL_BOOLEAN, 0),
        (idx_enabled, NO_RAW, VAL_BOOLEAN, 1),
    ]) + end_elem(idx_provider)
    provider_at = elem_attrs_end(app_now)
    insert_bytes(provider_at, provider_xml)
    print("[axml] +<provider %s authorities=%s> (%d B)"
          % (PROVIDER_NAME, PROVIDER_AUTH, len(provider_xml)))
    _app_s = find_element(idx_application)
    _app_e = app_end_pos()
    if not (_app_s < provider_at < _app_e):
        fail("provider 插入点 %d 不在 <application> [%d,%d) 内" % (provider_at, _app_s, _app_e))

    # ---- 3) </application> 之前新增 <service MenuService> ------------------
    service_xml = start_elem(idx_service, [
        (idx_name, idx_svc_name, VAL_STRING, idx_svc_name),
        (idx_enabled, NO_RAW, VAL_BOOLEAN, 1),
        (idx_exported, NO_RAW, VAL_BOOLEAN, 0),
        (idx_fgst, NO_RAW, VAL_INT_HEX, FGS_TYPE_DATA),
        (idx_stop_with_task, NO_RAW, VAL_BOOLEAN, 0),
    ]) + start_elem(idx_property, [
        (idx_name, idx_prop_name, VAL_STRING, idx_prop_name),
        (idx_value, idx_prop_value, VAL_STRING, idx_prop_value),
    ]) + end_elem(idx_property) + end_elem(idx_service)
    insert_bytes(app_end_pos(), service_xml)
    print("[axml] +<service %s> (%d B)" % (MENU_SERVICE, len(service_xml)))

    # ---- 4) 改完立刻复读自检（失败不落盘）--------------------------------
    def start_positions(name_idx):
        return [pos for pos, t2, en in walk()
                if t2 == CHUNK_START_ELEMENT and en == name_idx]

    errs = []
    svc = [p for p in start_positions(idx_service)
           if find_string_attr(p, idx_name) == MENU_SERVICE]
    if not svc:
        errs.append("MenuService element missing after patch")
    else:
        names = [strings[an] for _ns, an, *_ in elem_attrs(svc[0])]
        for need in ("name", "enabled", "exported", "foregroundServiceType", "stopWithTask"):
            if need not in names:
                errs.append("MenuService 缺属性 " + need)
        if not any(t2 == CHUNK_START_ELEMENT and en == idx_property
                   for _p, t2, en in walk()):
            errs.append("MenuService 缺 <property> 子元素")

    app_now = find_element(idx_application)
    _appname_now = find_string_attr(app_now, idx_name)
    if _appname_now != APP_NAME:
        errs.append("application@name 被改动（%r != %r）—— 本包禁止替换 Application"
                    % (_appname_now, APP_NAME))

    prov = [p for p in start_positions(idx_provider)
            if find_string_attr(p, idx_name) == PROVIDER_NAME]
    if not prov:
        errs.append("StarryInitProvider element missing after patch")
    elif find_string_attr(prov[0], idx_authorities) != PROVIDER_AUTH:
        errs.append("provider@authorities != " + PROVIDER_AUTH)

    # 决定性自检：resource map 按「字符串索引」回读，新属性名与既有属性名都要对
    def resource_map():
        p2 = sp
        while p2 + 8 <= len(data):
            t3, _h3, sz3 = struct.unpack_from("<HHI", data, p2)
            if t3 == CHUNK_RESOURCE_MAP:
                return [struct.unpack_from("<I", data, p2 + 8 + i * 4)[0]
                        for i in range((sz3 - 8) // 4)]
            if sz3 <= 0:
                break
            p2 += sz3
        return []

    rmap = resource_map()
    for an, want_id in sorted(ATTR_RES_IDS.items()):
        i = strings.index(an)
        got = rmap[i] if i < len(rmap) else 0
        if got != want_id:
            errs.append("resmap[%d]=%s 得到 0x%X，期望 0x%X" % (i, an, got, want_id))
    for an, want_id in (("label", 0x01010001), ("name", 0x01010003),
                        ("exported", 0x01010010)):
        if an not in strings:
            continue
        i = strings.index(an)
        got = rmap[i] if i < len(rmap) else 0
        if got != want_id:
            errs.append("resmap[%d]=%s 被破坏（0x%X != 0x%X）" % (i, an, got, want_id))

    perms_now = set()
    for pos, t2, en in walk():
        if t2 == CHUNK_START_ELEMENT and en == idx_usesperm:
            v = find_string_attr(pos, idx_name)
            if v:
                perms_now.add(v)
    for wanted in PERM_WANTED:
        if wanted not in perms_now:
            errs.append("权限缺失: " + wanted)

    if errs:
        for e in errs:
            print("  FAIL " + e)
        fail("%d 处自检失败，未写出文件" % len(errs))
    print("[selfcheck] Application 保留 + MenuService(FGS specialUse) + StarryInitProvider + resmap + 权限齐备")

    open(sys.argv[2], "wb").write(data)
    print("[done] %s (%d B)" % (sys.argv[2], len(data)))


if __name__ == "__main__":
    main()
