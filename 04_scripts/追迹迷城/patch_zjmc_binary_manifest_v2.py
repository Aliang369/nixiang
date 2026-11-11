#!/usr/bin/env python3
# 追迹迷城 — binary AndroidManifest.xml 补丁（结构化，禁止对游戏整包 apktool d/b）
#
# 补丁内容：
#   1) 33/34 字符广告 service 槽等长替换成 MenuService/MenuService2
#   2) application 元素插入 android:name="com.starry.magic.StarryApp"
#   3) 被替换的 service 插入 android:foregroundServiceType="specialUse"
#   4) 按需插入 SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE_SPECIAL_USE
#
# 用法: python3 patch_zjmc_binary_manifest_v2.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
SLOT_PAIRS = [
    ("com.byazt.iu.DownloadHandleService", "com.starry.magic.menu.MenuService2"),
    ("com.byazt.b.DownloadHandleService", "com.starry.magic.menu.MenuService"),
]
for _old, _new in SLOT_PAIRS:
    assert len(_old) == len(_new), (_old, _new)

APP_NAME = "com.starry.magic.StarryApp"
PERM_WANTED = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000
ATTR_RES_IDS = {"foregroundServiceType": 0x01010599}

CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
CHUNK_RESOURCE_MAP = 0x0180
VAL_STRING = 0x03
VAL_INT_HEX = 0x11


def fail(msg):
    raise SystemExit(f"ERROR: {msg}")


def main():
    if len(sys.argv) != 3:
        fail("usage: patch_zjmc_binary_manifest_v2.py <in.xml> <out.xml>")
    data = bytearray(open(sys.argv[1], "rb").read())

    sp = 8
    sp_type, _sp_hdrsz, sp_size = struct.unpack_from("<HHI", data, sp)
    if sp_type != 0x0001:
        fail(f"chunk@8 type=0x{sp_type:X}, expected string pool")
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
    print(f"[pool] count={count} flags=0x{flags:X}")

    def ensure(text):
        if text in strings:
            return strings.index(text)
        strings.append(text)
        return len(strings) - 1

    slot = None
    for old, new in SLOT_PAIRS:
        if old in strings:
            slot = (old, new)
            break
    if slot is None:
        fail("known 33/34-char DownloadHandleService slot not found")
    old_svc, new_svc = slot
    strings[strings.index(old_svc)] = new_svc
    print(f"[pool] slot swap idx={strings.index(new_svc)}: {old_svc} -> {new_svc}")

    idx_ns = ensure(ANDROID_NS)
    idx_name = ensure("name")
    idx_fgst = ensure("foregroundServiceType")
    idx_special = ensure("specialUse")
    idx_appname = ensure(APP_NAME)
    idx_usesperm = ensure("uses-permission")
    idx_perm_wanted = [ensure(p) for p in PERM_WANTED]

    offsets = []
    blob = bytearray()
    for text in strings:
        offsets.append(len(blob))
        blob += struct.pack("<H", len(text)) + text.encode("utf-16-le") + b"\x00\x00"
    while len(blob) % 4:
        blob += b"\x00\x00"
    str_start_new = 28 + 4 * len(strings)
    pool = bytearray()
    pool += struct.pack("<HHI", 0x0001, 28, str_start_new + len(blob))
    pool += struct.pack("<IIIII", len(strings), 0, flags, str_start_new, 0)
    pool += b"".join(struct.pack("<I", off) for off in offsets)
    pool += blob
    delta = len(pool) - sp_size
    data[sp:sp + sp_size] = pool
    struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + delta)
    print(f"[pool] rebuilt {count}->{len(strings)} strings, chunk {sp_size}->{len(pool)} B")

    p = sp + len(pool)
    t, _hs, sz = struct.unpack_from("<HHI", data, p)
    if t == CHUNK_RESOURCE_MAP:
        n_ids = (sz - 16) // 4
        extra_ids = [ATTR_RES_IDS.get(strings[i], 0) for i in range(n_ids, len(strings))]
        if any(extra_ids):
            chunk = b"".join(struct.pack("<I", value) for value in extra_ids)
            insert_pos = p + sz
            data[insert_pos:insert_pos] = chunk
            struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))
            struct.pack_into("<I", data, p + 4, sz + len(chunk))
            named = {f"idx{n_ids + k}({strings[n_ids + k]})": hex(v)
                     for k, v in enumerate(extra_ids) if v}
            print(f"[map] resource map +{len(extra_ids)} entries: {named}")
        else:
            print("[map] no new attr names, map untouched")
    else:
        print(f"[map] WARN chunk after pool type=0x{t:X}, no resource map")

    xml_start = sp + len(pool)

    def elem_attr_count(pos):
        return struct.unpack_from("<H", data, pos + 16 + 12)[0]

    def elem_attrs(pos):
        cnt = elem_attr_count(pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, pos + 16 + 8)
        arr = pos + 16 + attr_start
        for i in range(cnt):
            _ns_idx, name_idx, raw_idx, _size, _res0, dtype, dtype_data = struct.unpack_from(
                "<IIIHBBI", data, arr + i * attr_size)
            yield name_idx, raw_idx, dtype, dtype_data

    def find_string_attr(pos, name_idx):
        for an, raw_idx, dtype, _dtype_data in elem_attrs(pos):
            if an == name_idx:
                if dtype != VAL_STRING:
                    return None
                return strings[raw_idx] if raw_idx < len(strings) else None
        return None

    def find_element(name_idx):
        pos = xml_start
        while pos < len(data):
            t, _hs, sz = struct.unpack_from("<HHI", data, pos)
            if t == CHUNK_START_ELEMENT:
                _ns, elem_name = struct.unpack_from("<II", data, pos + 16)
                if elem_name == name_idx:
                    return pos
            if sz <= 0:
                break
            pos += sz
        return None

    def insert_bytes(pos, chunk):
        data[pos:pos] = chunk
        struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))

    def add_attr(elem_pos, attr_name_idx, raw_idx, dtype, dtype_data):
        cnt = elem_attr_count(elem_pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, elem_pos + 16 + 8)
        arr = elem_pos + 16 + attr_start
        chunk = struct.pack("<IIIHBBI", idx_ns, attr_name_idx, raw_idx, 8, 0, dtype, dtype_data)
        insert_bytes(arr + cnt * attr_size, chunk)
        struct.pack_into("<H", data, elem_pos + 16 + 12, cnt + 1)
        sz = struct.unpack_from("<I", data, elem_pos + 4)[0]
        struct.pack_into("<I", data, elem_pos + 4, sz + attr_size)
        return elem_pos + attr_size

    app_pos = find_element(ensure("application"))
    if app_pos is None:
        fail("application element not found")
    if find_string_attr(app_pos, idx_name) != APP_NAME:
        add_attr(app_pos, idx_name, idx_appname, VAL_STRING, idx_appname)
        print("[axml] application +android:name")
    else:
        print("[axml] application android:name already present")

    svc_pos = None
    pos = xml_start
    while pos < len(data):
        t, _hs, sz = struct.unpack_from("<HHI", data, pos)
        if t == CHUNK_START_ELEMENT:
            _ns, elem_name = struct.unpack_from("<II", data, pos + 16)
            if elem_name == ensure("service") and find_string_attr(pos, idx_name) == new_svc:
                svc_pos = pos
                break
        if sz <= 0:
            break
        pos += sz
    if svc_pos is None:
        fail(f"replacement service element not found: {new_svc}")
    existing_attr_names = {an for an, *_ in elem_attrs(svc_pos)}
    if idx_fgst not in existing_attr_names:
        add_attr(svc_pos, idx_fgst, idx_special, VAL_INT_HEX, FGS_TYPE_DATA)
        print("[axml] service +foregroundServiceType=specialUse")
    else:
        print("[axml] service foregroundServiceType already present")

    existing_perms = set()
    pos = xml_start
    while pos < len(data):
        t, _hs, sz = struct.unpack_from("<HHI", data, pos)
        if t == CHUNK_START_ELEMENT:
            _ns, elem_name = struct.unpack_from("<II", data, pos + 16)
            if elem_name == idx_usesperm:
                value = find_string_attr(pos, idx_name)
                if value:
                    existing_perms.add(value)
        if sz <= 0:
            break
        pos += sz

    perms_to_add = []
    for wanted, perm_idx in zip(PERM_WANTED, idx_perm_wanted):
        if wanted not in existing_perms:
            perms_to_add.append((wanted, perm_idx))

    def usesperm_chunk(perm_idx):
        start = struct.pack(
            "<HHIIIIIHHHHHH",
            CHUNK_START_ELEMENT, 16, 16 + 20 + 20,
            1, 0xFFFFFFFF,
            0xFFFFFFFF, idx_usesperm, 20, 20, 1, 0, 0, 0)
        start += struct.pack("<IIIHBBI", idx_ns, idx_name, perm_idx, 8, 0,
                             VAL_STRING, perm_idx)
        end = struct.pack(
            "<HHIIIII", CHUNK_END_ELEMENT, 16, 24, 1, 0xFFFFFFFF,
            0xFFFFFFFF, idx_usesperm)
        return start + end

    app_pos = find_element(ensure("application"))
    for _wanted, perm_idx in reversed(perms_to_add):
        insert_bytes(app_pos, usesperm_chunk(perm_idx))
    print(f"[axml] +{len(perms_to_add)} uses-permission, existing={len(existing_perms)}")

    open(sys.argv[2], "wb").write(data)
    print(f"[done] {sys.argv[2]} ({len(data)} B)")


if __name__ == "__main__":
    main()
