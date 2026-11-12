#!/usr/bin/env python3
# script-policy: {"purpose": "群侠传：幸存者二进制 AndroidManifest.xml 补丁：加 StarryInitProvider（application 首个子元素）+ MenuService（specialUse FGS）+ 3 条权限", "inputs": "底包解出的 AndroidManifest.xml", "outputs": "打完补丁的 AndroidManifest.xml（不改 application@name / appComponentFactory）", "boundary": "只改 Manifest；不打包、不签名、不装机；对已打补丁输入幂等拒绝", "prior_art": "04_scripts/伊莫/patch_yimo_binary_manifest.py（同源 provider-only 实现）", "difference": "本作官方无 application@name 且 appComponentFactory 保持 CoreComponentFactory ⇒ 删除 Application 替换与 Factory 改写两段，只留 provider/service/权限", "evidence": "10_docs/群侠传：幸存者/星光菜单落地.md#菜单 dex / so 落位"}
# 群侠传：幸存者（com.nenlystudio.qunxiasurvivor.cn，0.2.3.9 TapTap 中国渠道）— binary AndroidManifest.xml 补丁
# 纯 stdlib；不依赖 apktool/aapt。
#
# 基线：04_scripts/伊莫/patch_yimo_binary_manifest.py（同源实现，provider-only 分支）
# 本作（Godot 4.6.1）差异：
#   1) 官方 <application> **无 android:name**（默认 Application），不替换 Application；
#   2) 官方 appComponentFactory = androidx.core.app.CoreComponentFactory，**保持不动**
#      （本文走 ContentProvider 入口，不需要早启动工厂）；
#   3) 新增 <provider com.starry.magic.StarryInitProvider>，结构上插在 <application> 首个子元素位
#      （Provider 顺序 = 平台实例化顺序 ⇒ 早于 TapTapKitInitProvider / ThemisProvider / FileProvider）；
#   4) 新增 <service com.starry.magic.menu.MenuService>（foregroundServiceType=specialUse + property）；
#   5) 补齐 uses-permission：SYSTEM_ALERT_WINDOW / FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE。
#
# 用法: python3 patch_qxzxcz_binary_manifest.py <in:AndroidManifest.xml> <out:AndroidManifest.xml>
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
PKG = "com.nenlystudio.qunxiasurvivor.cn"
APP_NAME_KEEP = None   # 官方无 application@name，保持不改
APP_FACTORY_NEW = None  # 保持官方 CoreComponentFactory 不动
MENU_SERVICE = "com.starry.magic.menu.MenuService"
INIT_PROVIDER = "com.starry.magic.StarryInitProvider"
PROVIDER_AUTHORITY = PKG + ".starryinit"
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
VAL_INT_HEX = 0x11
VAL_BOOLEAN = 0x12


def fail(msg):
    raise SystemExit("ERROR: " + msg)


class Pool:
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
        fail("usage: patch_yimo_binary_manifest.py <in.xml> <out.xml>")
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
    # ★必须在这里 ensure：字符串池是**在下面一次性序列化**的，之后再 ensure 的新串不会进池
    #   （2026-10-29 踩过：appComponentFactory 写成池外索引 ⇒ aapt2 复读成 ""）
    idx_factory = ensure("appComponentFactory")
    idx_factory_new = -1   # 本作不改 appComponentFactory，占位不用（勿 ensure，否则 None 进池）
    idx_perm = [ensure(p) for p in PERM_WANTED]

    # ---- 字符串池重建（旧索引不变，新串追加）--------------------------------
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

    # ---- resource map 追加（新属性名 → android attr res id）-----------------
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
            out.append((ns_i, name_i, raw_i, dtype, ddata, arr + i * attr_size))
        return out

    def find_string_attr(pos, name_idx):
        for _ns, an, raw_i, dtype, _d, _off in elem_attrs(pos):
            if an == name_idx:
                if dtype != VAL_STRING:
                    return None
                return strings[raw_i] if raw_i < len(strings) else None
        return None

    def set_string_attr(pos, name_idx, str_idx):
        """把已存在属性的值改成字符串池第 str_idx 条（原地改，不动 chunk 长度）。"""
        for _ns, an, _raw, dtype, _d, off in elem_attrs(pos):
            if an == name_idx:
                if dtype != VAL_STRING:
                    fail("属性 %r 不是字符串类型，拒绝原地改" % strings[name_idx])
                struct.pack_into("<I", data, off + 8, str_idx)    # rawValue
                struct.pack_into("<I", data, off + 16, str_idx)   # typed data
                return True
        return False

    def walk():
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

    # ---- 幂等：已打过补丁 ⇒ 拒绝 ------------------------------------------
    for pos, t, en in walk():
        if t == CHUNK_START_ELEMENT and en in (idx_service, idx_provider):
            nm = find_string_attr(pos, idx_name)
            if nm in (MENU_SERVICE, INIT_PROVIDER):
                fail("manifest already patched (found %s); 请从官方底包重新解 Manifest" % nm)

    # ---- 0) 前置断言：<application> 存在，且不替换 Application / appComponentFactory ----
    app_start = find_element(idx_application)
    if app_start is None:
        fail("application element not found")
    cur_name = find_string_attr(app_start, idx_name)
    if cur_name:
        fail("官方包意外已有 application@name=%s，本脚本按「无 name」实现，需人工确认" % cur_name)
    cur_factory = find_string_attr(app_start, idx_factory)
    print("[axml] application@name=<none>（不改）；appComponentFactory=%s（不改）" % cur_factory)

    # ---- 2) uses-permission 补齐（插到 <application> 之前）-----------------
    existing_perms = set()
    for pos, t, en in walk():
        if t == CHUNK_START_ELEMENT and en == idx_usesperm:
            v = find_string_attr(pos, idx_name)
            if v:
                existing_perms.add(v)

    def usesperm_chunk(perm_idx):
        return start_elem(idx_usesperm, [(idx_name, perm_idx, VAL_STRING, perm_idx)]) \
            + end_elem(idx_usesperm)

    added_perms = []
    for wanted, pidx in zip(PERM_WANTED, idx_perm):
        if wanted not in existing_perms:
            app_start = find_element(idx_application)
            insert_bytes(app_start, usesperm_chunk(pidx))
            added_perms.append(wanted)
    print("[axml] uses-permission: existing=%d, added=%s" % (len(existing_perms), added_perms))

    # ---- 3) <provider> 插到 <application> 第一个子元素位（顺序铁律）---------
    app_start = find_element(idx_application)
    hdr_sz = struct.unpack_from("<H", data, app_start + 2)[0]
    insert_at = app_start + hdr_sz + 20 + 20 * elem_attr_count(app_start)
    provider_xml = start_elem(idx_provider, [
        (idx_name, idx_prov_name, VAL_STRING, idx_prov_name),
        (idx_authorities, idx_authority, VAL_STRING, idx_authority),
        (idx_exported, 0xFFFFFFFF, VAL_BOOLEAN, 0),
    ]) + end_elem(idx_provider)
    insert_bytes(insert_at, provider_xml)
    print("[axml] +<provider %s> (%d B, 位于 application 首个子元素)" % (INIT_PROVIDER, len(provider_xml)))

    # ---- 4) <service> 插到 </application> 之前 -----------------------------
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

    # ---- 5) 自检（改完立刻复读；失败不落盘）--------------------------------
    errs = []
    app_now = find_element(idx_application)
    if app_now is None:
        errs.append("application 元素丢失")
    else:
        if find_string_attr(app_now, idx_name):
            errs.append("application@name 被意外写入（本作必须保持无 name）")
        if find_string_attr(app_now, idx_factory) != cur_factory:
            errs.append("appComponentFactory 被意外改动")

    provs = [pos for pos, t, en in walk()
             if t == CHUNK_START_ELEMENT and en == idx_provider
             and find_string_attr(pos, idx_name) == INIT_PROVIDER]
    if not provs:
        errs.append("StarryInitProvider 缺失")
    else:
        if find_string_attr(provs[0], idx_authorities) != PROVIDER_AUTHORITY:
            errs.append("StarryInitProvider authorities 不等于 " + PROVIDER_AUTHORITY)
        # 顺序：本 provider 必须是第一个出现的 <provider>
        first_prov = next((pos for pos, t, en in walk()
                           if t == CHUNK_START_ELEMENT and en == idx_provider), None)
        if first_prov != provs[0]:
            errs.append("StarryInitProvider 不是第一个 provider（顺序铁律）")

    svcs = [pos for pos, t, en in walk()
            if t == CHUNK_START_ELEMENT and en == idx_service
            and find_string_attr(pos, idx_name) == MENU_SERVICE]
    if not svcs:
        errs.append("MenuService 缺失")
    else:
        names = [strings[an] for _ns, an, *_ in elem_attrs(svcs[0])]
        if "foregroundServiceType" not in names:
            errs.append("MenuService 缺 foregroundServiceType")
        if not any(t == CHUNK_START_ELEMENT and en == idx_property for _p, t, en in walk()):
            errs.append("MenuService 缺 <property> 子元素")

    perms_now = set()
    for pos, t, en in walk():
        if t == CHUNK_START_ELEMENT and en == idx_usesperm:
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
    print("[selfcheck] Application/appComponentFactory 未改；Provider(首位)/Service/权限 齐备")

    open(sys.argv[2], "wb").write(data)
    print("[done] %s (%d B)" % (sys.argv[2], len(data)))


if __name__ == "__main__":
    main()
