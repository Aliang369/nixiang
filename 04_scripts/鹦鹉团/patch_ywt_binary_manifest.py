#!/usr/bin/env python3
# script-policy: {"purpose": "鹦鹉团 1.0.7 二进制 AndroidManifest.xml 补丁：加 StarryInitProvider（application 首个子元素，保证排在 Facebook/TapTapKit 等 12 个 provider 之前）+ MenuService（foregroundServiceType=specialUse + property）+ 3 条权限；不改 application@name 与 appComponentFactory", "inputs": "从 APK 内直接取出的二进制 AndroidManifest.xml（不是 apktool 解出的文本 XML）；--dry-run 只打印将插入的节点", "outputs": "打完补丁的二进制 AndroidManifest.xml（--dry-run 不写文件）+ stdout 的 pool/map/自检摘要", "boundary": "只改 Manifest；不打包、不签名、不装机；不改 application@name / appComponentFactory / 既有 provider 顺序；对已打补丁输入幂等跳过", "prior_art": "04_scripts/群侠传：幸存者/patch_qxzxcz_binary_manifest.py（同源 AXML 改写实现）；04_scripts/_common/axml_inspect.py（resid 断言）", "difference": "本作 application **已有** android:name=com.qk.game.GameApplication（qxzxcz 是无 name，遇 name 直接 fail）⇒ 改为断言后保持原样；新增 --dry-run 与幂等跳过（qxzxcz 是已打过就 fail）；新增结构完整性自检（chunk 走到底、pool 偏移、属性索引全部在池内）", "evidence": "10_docs/鹦鹉团/鹦鹉团_组包脚本_20261119.md 附节"}
# ============================================================================
# 鹦鹉团 1.0.7（com.jinsaina.auditionsgo.gw）—— 二进制 AXML Manifest 补丁
#
# 本作 anchor（从 APK 内 52,768 B 的二进制 Manifest 现取，2026-11-19）：
#   <application android:name="com.qk.game.GameApplication"
#                android:appComponentFactory="androidx.core.app.CoreComponentFactory"
#                android:extractNativeLibs="true" ...>
#   provider 共 12 个，首个是 com.facebook.internal.FacebookInitProvider；
#   com.taptap.sdk.kit.internal.TapTapKitInitProvider 出现 2 次（原包现状，不动）
#   权限已含 SYSTEM_ALERT_WINDOW；缺 FOREGROUND_SERVICE / FOREGROUND_SERVICE_SPECIAL_USE
#
# 做三件事（顺序语义不动）：
#   1) <provider com.starry.magic.StarryInitProvider authorities=PKG.starryinit exported=false>
#      插到 <application> 的**第一个子元素位** —— Provider 顺序 = 平台实例化顺序，
#      必须早于 Facebook / TapTapKit / Themis / QGFileProvider 等；
#   2) <service com.starry.magic.menu.MenuService>（enabled + exported=false +
#      foregroundServiceType=specialUse + <property PROPERTY_SPECIAL_USE_FGS_SUBTYPE>）插到 </application> 前；
#   3) 补 uses-permission 三条（已有则跳过）。
#
# 用法：
#   python3 patch_ywt_binary_manifest.py <in.xml> <out.xml>
#   python3 patch_ywt_binary_manifest.py <in.xml> <out.xml> --dry-run
# 退出码：0 成功 / 1 自检失败 / 2 用法或输入不合法
# ============================================================================
import os
import struct
import sys

ANDROID_NS = "http://schemas.android.com/apk/res/android"
PKG = "com.jinsaina.auditionsgo.gw"
APP_NAME_EXPECT = "com.qk.game.GameApplication"
FACTORY_EXPECT = None            # 只打印，不校验具体值（本作保留 aix CoreComponentFactory）
MENU_SERVICE = "com.starry.magic.menu.MenuService"
INIT_PROVIDER = "com.starry.magic.StarryInitProvider"
AUTHORITY = PKG + ".starryinit"
PERMS_NEED = [
    "android.permission.SYSTEM_ALERT_WINDOW",
    "android.permission.FOREGROUND_SERVICE",
    "android.permission.FOREGROUND_SERVICE_SPECIAL_USE",
]
FGS_TYPE_DATA = 0x40000000        # FOREGROUND_SERVICE_TYPE_SPECIAL_USE
ATTR_RES_IDS = {"foregroundServiceType": 0x01010599}

CHUNK_START_ELEMENT = 0x0102
CHUNK_END_ELEMENT = 0x0103
CHUNK_RESOURCE_MAP = 0x0180
VAL_STRING = 0x03
VAL_INT_HEX = 0x11
VAL_BOOLEAN = 0x12
NO_INDEX = 0xFFFFFFFF


def fail(msg, code=1):
    sys.stderr.write("ERROR: " + msg + "\n")
    raise SystemExit(code)


class Pool:
    def __init__(self, data, sp, size, count, flags, str_start):
        self.data = data
        self.sp = sp
        self.size = size
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
    argv = [a for a in sys.argv[1:] if a != "--dry-run"]
    dry = "--dry-run" in sys.argv[1:]
    if len(argv) != 2:
        fail("usage: patch_ywt_binary_manifest.py <in.xml> <out.xml> [--dry-run]", 2)

    data = bytearray(open(argv[0], "rb").read())
    if data[:4] != b"\x03\x00\x08\x00":
        fail("不是二进制 AXML（头 %s）；apktool 解出的文本 XML 不能直接喂本脚本" % data[:4].hex(), 2)

    sp = 8
    sp_type, _hdrsz, sp_size = struct.unpack_from("<HHI", data, sp)
    if sp_type != 0x0001:
        fail("chunk@8 type=0x%X，期望 string pool" % sp_type)
    count, style_count, flags, str_start, _ss = struct.unpack_from("<IIIII", data, sp + 8)
    if style_count:
        fail("string pool 带 styles，不支持")
    if flags & 0x100:
        fail("UTF-8 池，本脚本按 UTF-16 实现")
    pool = Pool(data, sp, sp_size, count, flags, str_start)
    strings = [pool.read(i) for i in range(count)]
    print("[pool] %d strings, %d B" % (count, sp_size))

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
    idx_fgst = ensure("foregroundServiceType")
    idx_special = ensure("specialUse")
    idx_service = ensure("service")
    idx_provider = ensure("provider")
    idx_property = ensure("property")
    idx_usesperm = ensure("uses-permission")
    idx_application = ensure("application")
    idx_svc_name = ensure(MENU_SERVICE)
    idx_prov_name = ensure(INIT_PROVIDER)
    idx_authority = ensure(AUTHORITY)
    idx_prop_name = ensure("android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE")
    idx_prop_value = ensure("MOD menu overlay")
    idx_factory = ensure("appComponentFactory")
    idx_perm = [ensure(p) for p in PERMS_NEED]
    perm_idx = dict(zip(PERMS_NEED, idx_perm))

    # ---- 字符串池重建（旧索引不变，新串追加）-------------------------------
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
    print("[pool] rebuilt %d->%d strings（chunk %d->%d B）" % (count, len(strings), sp_size, len(new_pool)))

    # ---- resource map 追加 -------------------------------------------------
    p = sp + len(new_pool)
    t, _hs, sz = struct.unpack_from("<HHI", data, p)
    if t != CHUNK_RESOURCE_MAP:
        fail("pool 之后不是 resource map（type=0x%X）" % t)
    # resource map 头是 8 B（type u16 + headerSize u16 + size u32）；用 16 会少数 2 条 ⇒
    # 追加起点错位 ⇒ 属性 res id 整体偏移（aapt2 仍能复读，平台却读到 0）。
    # 本包实测：size=184 ⇒ 44 entries（343 条串里前 44 条带 res id）。
    n_ids = (sz - 8) // 4
    extra = [ATTR_RES_IDS.get(strings[i], 0) for i in range(n_ids, len(strings))]
    if any(extra):
        chunk = b"".join(struct.pack("<I", v) for v in extra)
        data[p + sz:p + sz] = chunk
        struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))
        struct.pack_into("<I", data, p + 4, sz + len(chunk))
        named = {strings[n_ids + k]: hex(v) for k, v in enumerate(extra) if v}
        print("[map] resource map +%d entries: %s" % (len(extra), named))
    else:
        print("[map] 新增串都不带 res id，map 不动")

    xml_start = sp + len(new_pool)

    def elem_attr_count(pos):
        return struct.unpack_from("<H", data, pos + 16 + 12)[0]

    def elem_attrs(pos):
        cnt = elem_attr_count(pos)
        attr_start, attr_size = struct.unpack_from("<HH", data, pos + 16 + 8)
        arr = pos + 16 + attr_start
        out = []
        for i in range(cnt):
            ns_i, name_i, raw_i, _s, _r, dtype, ddata = struct.unpack_from("<IIIHBBI", data, arr + i * attr_size)
            out.append((ns_i, name_i, raw_i, dtype, ddata, arr + i * attr_size))
        return out

    def find_string_attr(pos, name_idx):
        for _ns, an, raw_i, dtype, _d, _o in elem_attrs(pos):
            if an == name_idx:
                if dtype != VAL_STRING:
                    return None
                return strings[raw_i] if raw_i < len(strings) else None
        return None

    def walk():
        pos = xml_start
        while pos + 8 <= len(data):
            t, _h, s = struct.unpack_from("<HHI", data, pos)
            if s <= 0:
                break
            if t in (CHUNK_START_ELEMENT, CHUNK_END_ELEMENT):
                _ns, en = struct.unpack_from("<II", data, pos + 16)
                yield pos, t, en
            pos += s

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
            fail("找不到 <application>")
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
        fail("找不到 </application>")

    def insert_bytes(pos, chunk):
        data[pos:pos] = chunk
        struct.pack_into("<I", data, 4, struct.unpack_from("<I", data, 4)[0] + len(chunk))

    def start_elem(name_idx, attrs):
        size = 16 + 20 + 20 * len(attrs)
        chunk = bytearray(struct.pack("<HHIIIIIHHHHHH", CHUNK_START_ELEMENT, 16, size,
                                      1, NO_INDEX, NO_INDEX, name_idx,
                                      20, 20, len(attrs), 0, 0, 0))
        for an, raw_i, dtype, ddata in attrs:
            chunk += struct.pack("<IIIHBBI", idx_ns, an, raw_i, 8, 0, dtype, ddata)
        return bytes(chunk)

    def end_elem(name_idx):
        return struct.pack("<HHIIIII", CHUNK_END_ELEMENT, 16, 24, 1, NO_INDEX, NO_INDEX, name_idx)

    # ---- 锚点断言：application 必须存在且 name 是本作那个 -------------------
    app_start = find_element(idx_application)
    if app_start is None:
        fail("<application> 缺失（换包了？重取锚点）")
    cur_name = find_string_attr(app_start, idx_name)
    cur_factory = find_string_attr(app_start, idx_factory)
    print("[axml] application@name=%s（保持不改）" % cur_name)
    print("[axml] appComponentFactory=%s（保持不改）" % cur_factory)
    if cur_name != APP_NAME_EXPECT:
        fail("application@name 是 %r，期望 %r —— 包结构变了，禁止盲改" % (cur_name, APP_NAME_EXPECT))

    # ---- 现状盘点（幂等判据）---------------------------------------------
    existing_providers = []
    existing_services = []
    existing_perms = set()
    for pos, t, en in walk():
        if t != CHUNK_START_ELEMENT:
            continue
        if en == idx_provider:
            existing_providers.append(find_string_attr(pos, idx_name))
        elif en == idx_service:
            existing_services.append(find_string_attr(pos, idx_name))
        elif en == idx_usesperm:
            v = find_string_attr(pos, idx_name)
            if v:
                existing_perms.add(v)
    have_provider = INIT_PROVIDER in existing_providers
    have_service = MENU_SERVICE in existing_services
    missing_perms = [p for p in PERMS_NEED if p not in existing_perms]
    print("[axml] 现有 provider %d 个（首个=%s）；MenuService=%s；缺权限=%s"
          % (len(existing_providers), existing_providers[0] if existing_providers else None,
             "有" if have_service else "无", missing_perms or "无"))

    provider_xml = start_elem(idx_provider, [
        (idx_name, idx_prov_name, VAL_STRING, idx_prov_name),
        (idx_authorities, idx_authority, VAL_STRING, idx_authority),
        (idx_exported, NO_INDEX, VAL_BOOLEAN, 0),
    ]) + end_elem(idx_provider)
    service_xml = start_elem(idx_service, [
        (idx_name, idx_svc_name, VAL_STRING, idx_svc_name),
        (idx_enabled, NO_INDEX, VAL_BOOLEAN, 1),
        (idx_exported, NO_INDEX, VAL_BOOLEAN, 0),
        (idx_fgst, NO_INDEX, VAL_INT_HEX, FGS_TYPE_DATA),
    ]) + start_elem(idx_property, [
        (idx_name, idx_prop_name, VAL_STRING, idx_prop_name),
        (idx_value, idx_prop_value, VAL_STRING, idx_prop_value),
    ]) + end_elem(idx_property) + end_elem(idx_service)

    plan = []
    if have_provider:
        plan.append(("skip", "provider 已存在（幂等跳过）", ""))
    else:
        plan.append(("add", "provider（插到 application 首个子元素位）",
                     '<provider android:name="%s" android:authorities="%s" android:exported="false"/>'
                     % (INIT_PROVIDER, AUTHORITY)))
    if have_service:
        plan.append(("skip", "MenuService 已存在（幂等跳过）", ""))
    else:
        plan.append(("add", "service（插到 </application> 前）",
                     '<service android:name="%s" android:enabled="true" android:exported="false" '
                     'android:foregroundServiceType="specialUse">'
                     '<property android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE" '
                     'android:value="MOD menu overlay"/></service>' % MENU_SERVICE))
    if missing_perms:
        for p_ in missing_perms:
            plan.append(("add", "uses-permission（插到 <application> 前）",
                         '<uses-permission android:name="%s"/>' % p_))
    else:
        plan.append(("skip", "3 条权限均已存在（幂等跳过）", ""))

    print("[plan] %d 项：" % len(plan))
    for kind, what, xml in plan:
        print("   - [%s] %s" % (kind, what))
        if xml:
            print("       " + xml)


    # ---- 应用（先插权限，再插 provider，最后 service）---------------------
    for p_ in missing_perms:
        pi = perm_idx[p_]
        insert_bytes(find_element(idx_application), start_elem(idx_usesperm, [
            (idx_name, pi, VAL_STRING, pi)]) + end_elem(idx_usesperm))
    if not have_provider:
        app_start = find_element(idx_application)
        hdr_sz = struct.unpack_from("<H", data, app_start + 2)[0]
        insert_at = app_start + hdr_sz + 20 + 20 * elem_attr_count(app_start)
        insert_bytes(insert_at, provider_xml)
    if not have_service:
        insert_bytes(app_end_pos(), service_xml)

    # ---- 自检：结构完整性 + 语义 -----------------------------------------
    errs = []
    pos = xml_start
    while pos + 8 <= len(data):
        t, _h, s = struct.unpack_from("<HHI", data, pos)
        if s < 8 or pos + s > len(data):
            errs.append("chunk@%d 尺寸非法（size=%d）" % (pos, s))
            break
        pos += s
    if pos != len(data):
        errs.append("chunk 未走到文件尾（%d != %d）" % (pos, len(data)))
    maxidx = len(strings)
    for ppos, t, en in walk():
        if t != CHUNK_START_ELEMENT:      # END_ELEMENT 只有 24 B，读属性数组会越界
            continue
        if en >= maxidx:
            errs.append("元素名索引越界 @%d" % ppos)
        for _ns, an, raw_i, dtype, _d, _o in elem_attrs(ppos):
            if an >= maxidx:
                errs.append("属性名索引越界 @%d" % ppos)
            if dtype == VAL_STRING and raw_i != NO_INDEX and raw_i >= maxidx:
                errs.append("字符串属性索引越界 @%d" % ppos)

    app_now = find_element(idx_application)
    if app_now is None:
        errs.append("<application> 丢失")
    else:
        if find_string_attr(app_now, idx_name) != APP_NAME_EXPECT:
            errs.append("application@name 被改动")
        if find_string_attr(app_now, idx_factory) != cur_factory:
            errs.append("appComponentFactory 被改动")

    provs = [p2 for p2, t, en in walk()
             if t == CHUNK_START_ELEMENT and en == idx_provider
             and find_string_attr(p2, idx_name) == INIT_PROVIDER]
    if not provs:
        errs.append("StarryInitProvider 缺失")
    else:
        if find_string_attr(provs[0], idx_authorities) != AUTHORITY:
            errs.append("authorities 不等于 " + AUTHORITY)
        first_prov = next((p2 for p2, t, en in walk()
                           if t == CHUNK_START_ELEMENT and en == idx_provider), None)
        if first_prov != provs[0]:
            errs.append("StarryInitProvider 不是第一个 provider（顺序铁律）")

    svcs = [p2 for p2, t, en in walk()
            if t == CHUNK_START_ELEMENT and en == idx_service
            and find_string_attr(p2, idx_name) == MENU_SERVICE]
    if not svcs:
        errs.append("MenuService 缺失")
    else:
        names = [strings[an] for _ns, an, *_ in elem_attrs(svcs[0])]
        if "foregroundServiceType" not in names:
            errs.append("MenuService 缺 foregroundServiceType")
        if not any(t == CHUNK_START_ELEMENT and en == idx_property for _p, t, en in walk()):
            errs.append("MenuService 缺 property 子元素")

    perms_now = set()
    for p2, t, en in walk():
        if t == CHUNK_START_ELEMENT and en == idx_usesperm:
            v = find_string_attr(p2, idx_name)
            if v:
                perms_now.add(v)
    for p_ in PERMS_NEED:
        if p_ not in perms_now:
            errs.append("权限缺失：" + p_)

    if errs:
        for e in errs:
            print("  FAIL " + e)
        fail("%d 处自检失败，未写出文件" % len(errs))

    if dry:
        print("[dry-run] 已在内存跑完整链路并自检通过，未写文件；产物将是 %d B（原 %d B）"
              % (len(data), len(open(argv[0], "rb").read())))
        return 0
    open(argv[1], "wb").write(data)
    print("[selfcheck] application@name/factory 未改；Provider 首位 + Service + 权限齐备；chunk 走到文件尾")
    print("[done] %s  %d B（原 %d B）" % (argv[1], len(data), len(open(argv[0], "rb").read())))
    return 0



if __name__ == "__main__":
    sys.exit(main())
