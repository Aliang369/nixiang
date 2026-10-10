#!/usr/bin/env python3
# 追迹迷城（zjmc）1.0.20261009 — binary AndroidManifest.xml 补丁（纯 stdlib，结构化）
#
# 为什么有 v3（v2 作废的原因）：
#   1.0.20260912 及更早的包 Manifest 里存在穿山甲（CSJ）33/34 字符 service 槽
#   `com.byazt.b.DownloadHandleService` / `com.byazt.iu.DownloadHandleService`，
#   v2 走「等长槽替换」。**1.0.20261009 起这些串在 Manifest 与全部 dex 里彻底消失**
#   （实测 classes*.dex 中 com/byazt|bytedance|toutiao|pangle|csj 零命中），
#   包内现存的 6 个 service 全是 androidx.room / androidx.work / google datatransport
#   的真实使用者，不能牺牲。⇒ 改为「凭空新增 <service> 元素」（沿用
#   04_scripts/冒险与征途/patch_mxyzt_binary_manifest.py 已验证的实现）。
#
# 补丁内容：
#   1) 字符串池重建（追加新串，旧串索引不变）+ resource map 追加新属性名 res id
#   2) <application> 追加 android:name="com.starry.magic.StarryApp"（官包无该属性）
#   3) </application> 之前新增 <service com.starry.magic.menu.MenuService>
#      （enabled=true / exported=false / foregroundServiceType=specialUse /
#        stopWithTask=true + <property PROPERTY_SPECIAL_USE_FGS_SUBTYPE>）
#   4) 补齐 uses-permission（FOREGROUND_SERVICE 官包已有；SYSTEM_ALERT_WINDOW 与
#      FOREGROUND_SERVICE_SPECIAL_USE 官包没有 → 追加）
#   5) 改完立刻复读自检，失败不落盘；幂等（已打过补丁的输入直接拒绝）
#
# 用法: python3 patch_zjmc_binary_manifest_v3.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
APP_NAME = "com.starry.magic.StarryApp"
MENU_SERVICE = "com.starry.magic.menu.MenuService"
PERM_WANTED = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000          # FOREGROUND_SERVICE_TYPE_SPECIAL_USE
PROP_NAME = "android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
PROP_VALUE = "MOD menu overlay"
# 追加属性名的平台 res id。resource map 语义 = **按字符串索引稠密排列**（map[i] = 字符串池第 i 条的
# res id；官方文件实测 label#1→0x1010001 / name#3→0x1010003 / exported#7→0x1010010 /
# extractNativeLibs#34→0x10104ea / appComponentFactory#39→0x101057a 全部吻合）。
# 少写就等于该属性对平台完全无效：v3 首版把基址写成 (sz-16)//4（差 2 位）⇒ foregroundServiceType
# 与 stopWithTask 的 res id 落成了 0，平台看不到 specialUse 前台类型。
ATTR_RES_IDS = {
    "foregroundServiceType": 0x01010599,
    "stopWithTask": 0x01010281,
}
# 入口点：**不用 Application 替换**（1.0.20261009 实测 <application android:name> 平台不采用：
# 该属性记录经 aapt2 复读合法（name#3 resid=0x1010003、type=string、值正确），但运行时
# StarryApp 从未实例化（logcat 无任何 StarryApp 行、/proc/<pid>/maps 无 libstarry_magic.so、
# 且若 className 生效而类缺失必然 "Unable to instantiate application" 崩溃 —— 未崩）。
# 改用框架既有 Provider 入口（见 StarryInitProvider.java 类注释：终末地 ACE 同款场景）。
PROVIDER_NAME = "com.starry.magic.StarryInitProvider"
PROVIDER_AUTH = "com.TeaMelo.DungeonChasers.starryinit"

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
        fail("usage: patch_zjmc_binary_manifest_v3.py <in.xml> <out.xml>")
    data = bytearray(open(sys.argv[1], "rb").read())

    sp = 8
    sp_type, _sp_hdrsz, sp_size = struct.unpack_from("<HHI", data, sp)
    if sp_type != 0x0001:
        fail("chunk@8 type=0x%X, expected string pool" % sp_type)
    count, style_count, flags, str_start, _style_start = struct.unpack_from(
        "<IIIII", data, sp + 8)
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
    idx_special = ensure("specialUse")
    idx_service = ensure("service")
    idx_property = ensure("property")
    idx_usesperm = ensure("uses-permission")
    idx_application = ensure("application")
    idx_provider = ensure("provider")
    idx_authorities = ensure("authorities")
    idx_provider_name = ensure(PROVIDER_NAME)
    idx_provider_auth = ensure(PROVIDER_AUTH)
    idx_appname = ensure(APP_NAME)
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

    # ---- resource map 追加新属性名 res id -----------------------------------
    p = sp + len(new_pool)
    t, _hs, sz = struct.unpack_from("<HHI", data, p)
    if t == CHUNK_RESOURCE_MAP:
        n_ids = (sz - 8) // 4
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

    def add_attr(elem_pos, attr_name_idx, raw_idx, dtype, dtype_data):
        cnt = elem_attr_count(elem_pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, elem_pos + 16 + 8)
        arr = elem_pos + 16 + attr_start
        chunk = struct.pack("<IIIHBBI", idx_ns, attr_name_idx, raw_idx, 8, 0,
                            dtype, dtype_data)
        insert_bytes(arr + cnt * attr_size, chunk)
        struct.pack_into("<H", data, elem_pos + 16 + 12, cnt + 1)
        sz_now = struct.unpack_from("<I", data, elem_pos + 4)[0]
        struct.pack_into("<I", data, elem_pos + 4, sz_now + attr_size)

    # ---- 0) 幂等：已打过补丁的清单直接拒绝 ----------------------------------
    for pos, t2, en in walk():
        if t2 == CHUNK_START_ELEMENT and en == idx_service:
            if find_string_attr(pos, idx_name) == MENU_SERVICE:
                fail("manifest already patched (MenuService present); "
                     "请从官方底包重新解 Manifest")
    app_now = find_element(idx_application)
    if app_now is None:
        fail("application element not found")
    if find_string_attr(app_now, idx_name) == APP_NAME:
        fail("manifest already patched (application@name==StarryApp)")

    # ---- 1) uses-permission 补齐（插到 <application> 之前）------------------
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
    added_perms = []
    for wanted, pidx in zip(PERM_WANTED, idx_perm):
        if wanted not in existing_perms:
            chunk = usesperm_chunk(pidx)
            insert_bytes(app_start, chunk)
            app_start += len(chunk)
            added_perms.append(wanted)
    print("[axml] uses-permission: existing=%d, added=%s" % (
        len(existing_perms), added_perms))

    # ---- 2) <application> 追加 android:name=StarryApp -----------------------
    app_now = find_element(idx_application)
    add_attr(app_now, idx_name, idx_appname, VAL_STRING, idx_appname)
    print("[axml] application +android:name=%s" % APP_NAME)

    # ---- 2b) 入口点：<application> 首个子元素插入 <provider StarryInitProvider>
    # 必须排在其它 Provider 之前（TapTap 的 TapTapKitInitProvider/TapUpdateFileProvider 等
    # 在应用启动早期就会读签名），Provider 顺序 = 平台实例化顺序 ⇒ 先回灌官方证书再让 SDK 读。
    def elem_attrs_end(elem_pos):
        cnt = elem_attr_count(elem_pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, elem_pos + 16 + 8)
        return elem_pos + 16 + attr_start + cnt * attr_size

    provider_xml = start_elem(idx_provider, [
        (idx_name, idx_provider_name, VAL_STRING, idx_provider_name),
        (idx_authorities, idx_provider_auth, VAL_STRING, idx_provider_auth),
        (idx_exported, 0xFFFFFFFF, VAL_BOOLEAN, 0),
        (idx_enabled, 0xFFFFFFFF, VAL_BOOLEAN, 1),
    ]) + end_elem(idx_provider)
    insert_bytes(elem_attrs_end(app_now), provider_xml)
    print("[axml] +<provider %s authorities=%s> (%d B)"
          % (PROVIDER_NAME, PROVIDER_AUTH, len(provider_xml)))

    # ---- 3) </application> 之前新增 <service MenuService> -------------------
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
    insert_bytes(app_end_pos(), service_xml)
    print("[axml] +<service %s> (%d B)" % (MENU_SERVICE, len(service_xml)))

    # ---- 4) 改完立刻复读自检（失败不落盘）-----------------------------------
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
        for need in ("name", "enabled", "exported", "foregroundServiceType",
                     "stopWithTask"):
            if need not in names:
                errs.append("MenuService 缺属性 " + need)
        if not any(t2 == CHUNK_START_ELEMENT and en == idx_property
                   for _p, t2, en in walk()):
            errs.append("MenuService 缺 <property> 子元素")

    app_now = find_element(idx_application)
    if find_string_attr(app_now, idx_name) != APP_NAME:
        errs.append("application@name != " + APP_NAME)

    # 入口点必须真的在清单里（provider 是本包唯一可用的自动执行入口）
    prov = [p for p in start_positions(idx_provider)
            if find_string_attr(p, idx_name) == PROVIDER_NAME]
    if not prov:
        errs.append("StarryInitProvider element missing after patch")
    elif find_string_attr(prov[0], idx_authorities) != PROVIDER_AUTH:
        errs.append("provider@authorities != " + PROVIDER_AUTH)

    # ⚠ 决定性自检：resource map 按「字符串索引稠密」语义回读，值必须等于期望 res id。
    # （v3 首版就是把基址算错 2 位，导致 foregroundServiceType/stopWithTask 平台读成 0，
    #   aapt2 复读却完全正常 —— 只靠 aapt2 看不出这个缺陷。）
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
    for an, want in sorted(ATTR_RES_IDS.items()):
        i = strings.index(an)
        got = rmap[i] if i < len(rmap) else 0
        if got != want:
            errs.append("resmap[%d]=%s 得到 0x%X，期望 0x%X" % (i, an, got, want))
    for an, want in (("label", 0x01010001), ("name", 0x01010003),
                     ("exported", 0x01010010), ("appComponentFactory", 0x0101057A)):
        i = strings.index(an)
        got = rmap[i] if i < len(rmap) else 0
        if got != want:
            errs.append("resmap[%d]=%s 被破坏（0x%X != 0x%X）" % (i, an, got, want))

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
    print("[selfcheck] StarryApp + MenuService(FGS specialUse) + StarryInitProvider + resmap + 权限齐备")

    open(sys.argv[2], "wb").write(data)
    print("[done] %s (%d B)" % (sys.argv[2], len(data)))


if __name__ == "__main__":
    main()
