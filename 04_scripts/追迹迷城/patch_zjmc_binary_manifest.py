#!/usr/bin/env python3
# 追迹迷城 — binary AndroidManifest.xml 补丁（等长/结构化，禁止对游戏整包 apktool d/b）
#
# 补丁内容（对齐 20260904 定稿机制）：
#   1) 34 字符 service 槽 com.byazt.iu.DownloadHandleService → com.starry.magic.menu.MenuService2（等长 UTF-16 原位替换）
#   2) application 元素插入 android:name="com.starry.magic.StarryApp"（官方无 Application 类，必须声明）
#   3) 该 service 元素插入 android:foregroundServiceType="specialUse"（API34+ startForeground 需要类型）
#   4) 插入 uses-permission：SYSTEM_ALERT_WINDOW（悬浮窗）+ FOREGROUND_SERVICE_SPECIAL_USE
#
# 用法: python3 patch_zjmc_binary_manifest.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
SVC_OLD = "com.byazt.iu.DownloadHandleService"   # 34 chars
SVC_NEW = "com.starry.magic.menu.MenuService2"   # 34 chars
assert len(SVC_OLD) == len(SVC_NEW)

APP_NAME = "com.starry.magic.StarryApp"
PERM_ADD = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000  # ServiceInfo.FOREGROUND_SERVICE_TYPE_SPECIAL_USE

CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
CHUNK_RESOURCE_MAP = 0x0180
VAL_STRING = 0x03
VAL_INT_HEX = 0x11

# 新增属性名字符串（原池不存在时）需要的 attr 资源 ID。
# 权威值取自 aapt2 编译产物资源映射表（星光模板 AndroidManifest），勿凭记忆猜。
ATTR_RES_IDS = {
    "foregroundServiceType": 0x01010599,
}


def fail(msg):
    raise SystemExit(f"ERROR: {msg}")


def main():
    if len(sys.argv) != 3:
        fail("usage: patch_zjmc_binary_manifest.py <in.xml> <out.xml>")
    data = bytearray(open(sys.argv[1], "rb").read())

    # ---- string pool (file header 8B → RES_STRING_POOL_TYPE 0x0001) ----
    SP = 8
    sp_type, sp_hdrsz, sp_size = struct.unpack_from("<HHI", data, SP)
    if sp_type != 0x0001:
        fail(f"chunk@8 type=0x{sp_type:X}, expected string pool")
    count, style_count, flags, str_start, style_start = struct.unpack_from(
        "<IIIII", data, SP + 8)
    utf8 = bool(flags & 0x100)
    if style_count != 0:
        fail("string pool has styles; unsupported")
    if utf8:
        fail("utf-8 pool; this script targets utf-16 pools")

    def read_str(idx):
        off = struct.unpack_from("<I", data, SP + 28 + 4 * idx)[0]
        p = SP + str_start + off
        l = struct.unpack_from("<H", data, p)[0]
        p += 2
        if l & 0x8000:
            l2 = struct.unpack_from("<H", data, p)[0]
            p += 2
            l = ((l & 0x7FFF) << 16) | l2
        return data[p:p + 2 * l].decode("utf-16-le")

    strings = [read_str(i) for i in range(count)]
    print(f"[pool] count={count} flags=0x{flags:X}")

    def ensure(s):
        if s in strings:
            return strings.index(s)
        strings.append(s)
        return len(strings) - 1

    # 1) 等长槽替换（pool 内原位换内容，index 不变）
    if SVC_OLD in strings:
        strings[strings.index(SVC_OLD)] = SVC_NEW
        print(f"[pool] slot swap idx={strings.index(SVC_NEW)}: {SVC_OLD} → {SVC_NEW}")
    else:
        fail(f"service slot {SVC_OLD} not found")

    idx_ns = ensure(ANDROID_NS)
    idx_name = ensure("name")
    idx_fgst = ensure("foregroundServiceType")
    idx_special = ensure("specialUse")
    idx_appname = ensure(APP_NAME)
    idx_perms = [ensure(p) for p in PERM_ADD]
    idx_usesperm = ensure("uses-permission")

    # ---- rebuild pool chunk (append-only; index 稳定) ----
    offsets = []
    blob = bytearray()
    for s in strings:
        offsets.append(len(blob))
        blob += struct.pack("<H", len(s)) + s.encode("utf-16-le") + b"\x00\x00"
    while len(blob) % 4:
        blob += b"\x00\x00"
    str_start_new = 28 + 4 * len(strings)
    pool = bytearray()
    pool += struct.pack("<HHI", 0x0001, 28, str_start_new + len(blob))
    pool += struct.pack("<IIIII", len(strings), 0, flags, str_start_new, 0)
    pool += b"".join(struct.pack("<I", o) for o in offsets)
    pool += blob
    delta = len(pool) - sp_size
    data[SP:SP + sp_size] = pool
    total = struct.unpack_from("<I", data, 4)[0]
    struct.pack_into("<I", data, 4, total + delta)
    print(f"[pool] rebuilt {count}→{len(strings)} strings, chunk {sp_size}→{len(pool)} B")

    # ---- 资源映射表扩展：新追加且用作属性名的字符串须补 attr 资源 ID ----
    # RES_XML_RESOURCE_MAP 按「字符串池 index」对应 attr ID；平台按 ID 解析属性，
    # 映射缺失 → attr ID 读作 0（aapt 按名字渲染所以静态校验看不出来）。
    p = SP + len(pool)
    t, hs, sz = struct.unpack_from("<HHI", data, p)
    if t == CHUNK_RESOURCE_MAP:
        n_ids = (sz - hs) // 4
        extra_ids = []
        for i in range(n_ids, len(strings)):
            extra_ids.append(ATTR_RES_IDS.get(strings[i], 0))
        if any(extra_ids):
            chunk = b"".join(struct.pack("<I", v) for v in extra_ids)
            insert_pos = p + sz
            data[insert_pos:insert_pos] = chunk
            total = struct.unpack_from("<I", data, 4)[0]
            struct.pack_into("<I", data, 4, total + len(chunk))
            struct.pack_into("<I", data, p + 4, sz + len(chunk))
            named = {f"idx{n_ids + k}({strings[n_ids + k]})": hex(v)
                     for k, v in enumerate(extra_ids) if v}
            print(f"[map] resource map +{len(extra_ids)} entries: {named}")
        else:
            print("[map] no new attr names, map untouched")
    else:
        print(f"[map] WARN chunk after pool type=0x{t:X}, no resource map")

    # ---- walk XML body chunks ----
    def find_element(target_name_idx, after=0):
        p = SP + len(pool)
        while p < len(data):
            t, hs, sz = struct.unpack_from("<HHI", data, p)
            if t == CHUNK_START_ELEMENT:
                ns, name = struct.unpack_from("<II", data, p + 16)
                if name == target_name_idx:
                    if p >= after:
                        return p
            p += sz
        return None

    def elem_attr_count(p):
        return struct.unpack_from("<H", data, p + 16 + 12)[0]

    def insert_bytes(pos, chunk):
        data[pos:pos] = chunk
        total = struct.unpack_from("<I", data, 4)[0]
        struct.pack_into("<I", data, 4, total + len(chunk))

    def add_attr(elem_pos, attr_name_idx, raw_idx, dtype, ddata):
        cnt = elem_attr_count(elem_pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, elem_pos + 16 + 8)
        arr = elem_pos + 16 + attr_start
        chunk = struct.pack(
            "<IIIHBBI", idx_ns, attr_name_idx, raw_idx, 8, 0, dtype, ddata)
        insert_pos = arr + cnt * attr_size
        insert_bytes(insert_pos, chunk)
        struct.pack_into("<H", data, elem_pos + 16 + 12, cnt + 1)
        sz = struct.unpack_from("<I", data, elem_pos + 4)[0]
        struct.pack_into("<I", data, elem_pos + 4, sz + attr_size)
        return insert_pos + attr_size  # 元素整体后移 20B

    app_pos = find_element(ensure("application"))
    if app_pos is None:
        fail("application element not found")
    # 2) application android:name
    after = add_attr(app_pos, idx_name, idx_appname, VAL_STRING, idx_appname)
    print(f"[axml] application +android:name (attrIdx={elem_attr_count(app_pos) - 1})")

    svc_pos = find_element(ensure("service"))
    # 找到被换名的 MenuService2 所在 service 元素：按 name 属性过滤
    svc_pos = None
    p = SP + len(pool)
    while p < len(data):
        t, hs, sz = struct.unpack_from("<HHI", data, p)
        if t == CHUNK_START_ELEMENT:
            ns, name = struct.unpack_from("<II", data, p + 16)
            if name == ensure("service"):
                cnt = elem_attr_count(p)
                attr_start, attr_size = struct.unpack_from("<HH", data, p + 16 + 8)
                for i in range(cnt):
                    ans, an, *_ = struct.unpack_from("<III", data,
                                                     p + 16 + attr_start + i * attr_size)
                    if an == idx_name:
                        raw = struct.unpack_from(
                            "<I", data, p + 16 + attr_start + i * attr_size + 8)[0]
                        if raw < len(strings) and strings[raw] == SVC_NEW:
                            svc_pos = p
                            break
        if svc_pos is not None:
            break
        p += sz
    if svc_pos is None:
        fail("MenuService2 service element not found")
    # 3) service foregroundServiceType（int-hex flags）
    add_attr(svc_pos, idx_fgst, idx_special, VAL_INT_HEX, FGS_TYPE_DATA)
    print(f"[axml] service +foregroundServiceType=specialUse")

    # 4) uses-permission 节点插到 application 元素之前
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
    for pi in reversed(idx_perms):
        insert_bytes(app_pos, usesperm_chunk(pi))
    print(f"[axml] +{len(idx_perms)} uses-permission before application")

    open(sys.argv[2], "wb").write(data)
    print(f"[done] {sys.argv[2]} ({len(data)} B)")


if __name__ == "__main__":
    main()
