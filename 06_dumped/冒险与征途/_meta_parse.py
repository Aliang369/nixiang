#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
_meta_parse.py - 「冒险与征途」com.mxyzt.gzmy 1.5.9
IL2CPP global-metadata.dat (v29) 解析 + libil2cpp.so 方法 RVA 对照

纯标准库。所有表布局都用「自校验」确定，不硬抄某个版本的结构：
  * 头字段：两套候选（含/不含 stringLiteralData），按 ranges/modulo/字符串表打分
  * 方法表：token 字段位置由 hi-byte 直方图自动定位（MethodDef=0x06）
  * 类型表：method_count/methodStart 偏移由「与方法表交叉统计」自动定位
  * images：由 sum(typeCount)==类型总数 校验
  * CodeGenModule：由 .rela.dyn 的 R_AARCH64_RELATIVE addend 反查（moduleName 字符串地址）
                    + 原始指针全文件扫描兜底；再用 methodPointerCount 与 metadata
                    推出的每 image 方法数做交叉校验

产物：metadata_methods.txt / metadata_hits.txt / metadata_cn_hits.txt
"""
import bisect
import collections
import mmap
import os
import re
import struct
import sys

try:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
except Exception:
    pass

HERE = os.path.dirname(os.path.abspath(__file__))
META_PATH = os.path.join(HERE, "apk_extract", "assets", "bin", "Data", "Managed", "Metadata", "global-metadata.dat")
SO_PATH = os.path.join(HERE, "apk_extract", "lib", "arm64-v8a", "libil2cpp.so")
if len(sys.argv) > 1:
    META_PATH = sys.argv[1]
if len(sys.argv) > 2:
    SO_PATH = sys.argv[2]

OUT_METHODS = os.path.join(HERE, "metadata_methods.txt")
OUT_HITS = os.path.join(HERE, "metadata_hits.txt")
OUT_CN = os.path.join(HERE, "metadata_cn_hits.txt")

TARGETS = [0x0207DCCC, 0x020B1688, 0x020B16A0, 0x020B0E78]

KEYWORDS = ("Damage|Hurt|Hp|Health|Invincible|Immortal|God|Notice|Announce|Bulletin|Ad|"
            "Reward|Drop|Kill|Monster|Enemy|Bullet|Projectile|Skill|Attack|Hit|Dead|Die|"
            "Win|Battle|Level|Stage|Wave").split("|")

CN_KEYWORDS = ["公告", "伤害", "生命", "无敌", "奖励", "广告", "秒杀", "挂机", "倍攻"]

REPORT = []


def out(s=""):
    print(s)
    REPORT.append(s)


def u16(d, o):
    return struct.unpack_from("<H", d, o)[0]


def u32(d, o):
    return struct.unpack_from("<I", d, o)[0]


def i32(d, o):
    return struct.unpack_from("<i", d, o)[0]


def u64(d, o):
    return struct.unpack_from("<Q", d, o)[0]


# ------------------------------------------------------------------ metadata header
FIELDS_BASE = ["string", "events", "properties", "methods", "parameterDefaultValues",
               "fieldDefaultValues", "fieldAndParameterDefaultValueData", "fieldMarshaledSizes",
               "parameters", "fields", "genericParameters", "genericParameterConstraints",
               "genericContainers", "nestedTypes", "interfaces", "vtableMethods",
               "interfaceOffsets", "typeDefinitions", "images", "assemblies", "fieldRefs",
               "referencedAssemblies", "attributeData", "attributeDataRange",
               "unresolvedVirtualCallParameterTypes", "unresolvedVirtualCallParameterRanges",
               "windowsRuntimeTypeNames", "windowsRuntimeStrings", "exportedTypeDefinitions"]


def parse_header(data):
    size = len(data)
    best = None
    for has_lit in (True, False):
        fields = ["stringLiteral"] + (["stringLiteralData"] if has_lit else []) + FIELDS_BASE
        nints = 2 + 2 * len(fields)
        vals = struct.unpack_from("<%di" % nints, data, 0)
        H = {"sanity": vals[0], "version": vals[1]}
        for i, n in enumerate(fields):
            H[n + "Offset"] = vals[2 + 2 * i]
            H[n + "Size"] = vals[3 + 2 * i]
        score, notes = 0, []
        if (H["sanity"] & 0xFFFFFFFF) == 0xFAB11BAF:
            score += 1
        o, s = H["stringOffset"], H["stringSize"]
        if 0 < o and 0 < s and o + s <= size:
            score += 2
            if b"Assembly-CSharp" in data[o:o + s]:
                score += 1
                notes.append("string table ok (contains Assembly-CSharp)")
        if H["methodsSize"] > 0 and H["methodsSize"] % 32 == 0:
            score += 2
            notes.append("methods%32=0")
        for cand in (88, 92):
            if H["typeDefinitionsSize"] > 0 and H["typeDefinitionsSize"] % cand == 0:
                score += 2
                notes.append("typeDefinitions%%%d=0" % cand)
                break
        if H["imagesSize"] > 0 and H["imagesSize"] % 40 == 0:
            score += 2
            notes.append("images%40=0")
        bad = sum(1 for n in fields
                  if H[n + "Offset"] < 0 or H[n + "Size"] < 0 or H[n + "Offset"] + H[n + "Size"] > size)
        if bad == 0:
            score += 2
        notes.append("bad_ranges=%d" % bad)
        if best is None or score > best[0]:
            best = (score, has_lit, H, notes)
    return best


def main():
    data = open(META_PATH, "rb").read()
    size = len(data)
    out("=" * 74)
    out("metadata: %d bytes" % size)
    score, has_lit, H, notes = parse_header(data)
    out("header: %s  (score=%d; %s)"
        % ("v29 layout (has stringLiteralData)" if has_lit else "v24.0 layout (no stringLiteralData)",
           score, "; ".join(notes)))
    out("sanity=0x%08X  version=%d" % (H["sanity"] & 0xFFFFFFFF, H["version"]))
    if (H["sanity"] & 0xFFFFFFFF) != 0xFAB11BAF:
        out("!! sanity 不符，中止")
        return 1

    # ---------------- strings
    s_off, s_size = H["stringOffset"], H["stringSize"]
    str_blob = data[s_off:s_off + s_size]

    def S(idx):
        if idx is None or idx < 0 or idx >= len(str_blob):
            return None
        e = str_blob.find(b"\0", idx)
        return None if e < 0 else str_blob[idx:e].decode("utf-8", "replace")

    # ---------------- method definitions
    md_off, md_size = H["methodsOffset"], H["methodsSize"]
    mdsize = next((c for c in (32, 40, 36, 28) if md_size % c == 0 and md_size // c > 0), 32)
    nmethods = md_size // mdsize
    tok_best, tok_hit, n_samp = None, -1, 0
    for k in range(0, mdsize - 3, 4):
        hit = n = 0
        for i in range(0, nmethods, max(1, nmethods // 4000)):
            n += 1
            if (u32(data, md_off + i * mdsize + k) >> 24) == 0x06:
                hit += 1
        if n and hit > tok_hit:
            tok_best, tok_hit, n_samp = k, hit, n
    out("methods: off=0x%X size=%d elem=%d count=%d  tokenField=+%d (%d/%d sampled)"
        % (md_off, md_size, mdsize, nmethods, tok_best, tok_hit, n_samp))

    methods = []  # (name_i, declaringType, token)
    for i in range(nmethods):
        o = md_off + i * mdsize
        methods.append((i32(data, o), i32(data, o + 4), u32(data, o + tok_best)))

    # ---------------- type definitions（布局自校验）
    td_off, td_size = H["typeDefinitionsOffset"], H["typeDefinitionsSize"]
    tdsize = next((c for c in (88, 92, 96, 84, 104) if td_size % c == 0), 88)
    ntypes = td_size // tdsize

    cnt_of_type = [0] * ntypes
    first_of_type = [-1] * ntypes
    bad_decl = 0
    for i, (_, decl, _tok) in enumerate(methods):
        if 0 <= decl < ntypes:
            cnt_of_type[decl] += 1
            if first_of_type[decl] < 0:
                first_of_type[decl] = i
        else:
            bad_decl += 1
    out("typeDefinitions: off=0x%X size=%d elem=%d count=%d ; methods.declaringType 越界=%d"
        % (td_off, td_size, tdsize, ntypes, bad_decl))

    def best_off(width, truth, lo, hi):
        best, bh = None, -1
        for off in range(lo, hi, width):
            hit = tot = 0
            for t in range(ntypes):
                w = truth[t]
                if w < 0:
                    continue
                tot += 1
                val = (u16 if width == 2 else u32)(data, td_off + t * tdsize + off)
                if val == w:
                    hit += 1
            if tot and hit > bh:
                best, bh = off, hit
        return best, bh

    coff_off, coff_hit = best_off(2, cnt_of_type, 40, tdsize - 18)
    soff_off, soff_hit = best_off(4, first_of_type, 16, 64)
    out("  布局探测: method_count@+%d (%d/%d 命中)  methodStart@+%d (%d/%d 命中)"
        % (coff_off, coff_hit, ntypes, soff_off, soff_hit, sum(1 for x in first_of_type if x >= 0)))

    csz = coff_off   # counts 块起始
    types = []
    for t in range(ntypes):
        o = td_off + t * tdsize
        types.append(dict(name_i=i32(data, o), ns_i=i32(data, o + 4),
                          nest_start=i32(data, o + 48), nest_cnt=u16(data, o + csz + 8),
                          flag=i32(data, o + 28)))

    # ---------------- images（sum(typeCount) 校验）
    im_off, im_size = H["imagesOffset"], H["imagesSize"]
    images = []
    for cand in (40, 44, 36, 48, 32):
        if im_size % cand:
            continue
        tmp, ok = [], True
        for i in range(im_size // cand):
            o = im_off + i * cand
            nm, asm, ts, tc = i32(data, o), i32(data, o + 4), i32(data, o + 8), i32(data, o + 12)
            if nm < 0 or nm >= s_size or ts < 0 or tc < 0 or ts + tc > ntypes + 8:
                ok = False
                break
            tmp.append((nm, asm, ts, tc))
        if ok and sum(x[3] for x in tmp) == ntypes:
            images = tmp
            out("images: off=0x%X size=%d elem=%d count=%d (sum typeCount=%d == ntypes 校验通过)"
                % (im_off, im_size, cand, len(tmp), ntypes))
            break
    if not images:
        out("!! images 表校验失败")
        return 1

    type2image = [0] * ntypes
    for ii, (_, _, ts, tc) in enumerate(images):
        for k in range(ts, min(ts + tc, ntypes)):
            type2image[k] = ii

    methods_per_image = [0] * len(images)
    for _, decl, _ in methods:
        if 0 <= decl < ntypes:
            methods_per_image[type2image[decl]] += 1
    out("assemblies: off=0x%X size=%d (elem≈%d)  stringLiterals=%d  stringTable entries=%d"
        % (H["assembliesOffset"], H["assembliesSize"],
           H["assembliesSize"] // max(1, max((im[1] for im in images), default=0) + 1),
           H["stringLiteralSize"] // 8, str_blob.count(b"\0")))

    # ---------------- ELF
    out("-" * 74)
    fh = open(SO_PATH, "rb")
    mm = mmap.mmap(fh.fileno(), 0, access=mmap.ACCESS_READ)
    e_phoff, e_phentsize, e_phnum = struct.unpack_from("<Q", mm, 0x20)[0], u16(mm, 0x36), u16(mm, 0x38)
    e_shoff, e_shentsize, e_shnum = struct.unpack_from("<Q", mm, 0x28)[0], u16(mm, 0x3A), u16(mm, 0x3C)
    loads = []
    for i in range(e_phnum):
        o = e_phoff + i * e_phentsize
        if u32(mm, o) != 1:
            continue
        flags = u32(mm, o + 4)
        loads.append((struct.unpack_from("<Q", mm, o + 8)[0], struct.unpack_from("<Q", mm, o + 16)[0],
                      struct.unpack_from("<Q", mm, o + 32)[0], flags))
    for po, pv, pf, fl in loads:
        out("  LOAD off=0x%08X vaddr=0x%08X filesz=0x%08X flags=%s"
            % (po, pv, pf, "".join(c for b, c in ((4, "R"), (2, "W"), (1, "X")) if fl & b)))

    def v2o(v):
        for po, pv, pf, _ in loads:
            if pv <= v < pv + pf:
                return po + (v - pv)
        return None

    def o2v(o):
        for po, pv, pf, _ in loads:
            if po <= o < po + pf:
                return pv + (o - po)
        return None

    # ---------------- .rela.dyn：磁盘上被清零的指针都在这里
    rela = collections.defaultdict(list)   # addend(=目标地址) -> [被写入的位置]
    rel_by_off = {}                        # 被写入的位置 -> addend
    nrel = 0
    for i in range(e_shnum):
        o = e_shoff + i * e_shentsize
        if u32(mm, o + 4) != 4:
            continue
        roff, rsz, rsz2 = struct.unpack_from("<Q", mm, o + 24)[0], struct.unpack_from("<Q", mm, o + 32)[0], struct.unpack_from("<Q", mm, o + 56)[0]
        if not rsz:
            continue
        step = rsz2 or 24
        for k in range(rsz // step):
            p = roff + k * step
            if u32(mm, p + 8) != 1027:      # R_AARCH64_RELATIVE
                continue
            nrel += 1
            off_, add_ = u64(mm, p), u64(mm, p + 16)
            rela[add_].append(off_)
            rel_by_off[off_] = add_
    out("  .rela.dyn: R_AARCH64_RELATIVE %d 条（正反向索引完成）" % nrel)

    # ---------------- 定位 Il2CppCodeGenModule
    # 结构（实测）：+0 moduleName(char*) +8 methodPointerCount +16 methodPointers
    #               +24 adjustorThunkCount +32 adjustorThunks +40 invokerIndices ...
    out("-" * 74)
    modmap = {}
    name_cache = {}
    located = 0
    for ii, (nm_i, _asm, ts, tc) in enumerate(images):
        iname = S(nm_i)
        if not iname:
            continue
        want = methods_per_image[ii]
        variants = [iname, iname[:-4] if iname.endswith(".dll") else iname]
        cand = []
        for v in variants:
            if v in name_cache:
                sv = name_cache[v]
            else:
                pat = v.encode() + b"\0"
                p = mm.find(pat)
                while p >= 0 and p > 0 and mm[p - 1] != 0:
                    p = mm.find(pat, p + 1)
                sv = o2v(p) if p >= 0 else None
                name_cache[v] = sv
            if sv is not None:
                cand += rela.get(sv, [])
        pick = None
        for lv in sorted(set(cand)):
            lo = v2o(lv)
            if lo is None or lo + 128 > len(mm):
                continue
            cnt = u32(mm, lo + 8)
            if cnt != want:
                continue
            ptr = rel_by_off.get(lv + 16) or u64(mm, lo + 16)
            po = v2o(ptr)
            if not ptr or po is None or po + cnt * 8 > len(mm):
                continue
            pick = (lv, cnt, ptr, po, iname)
            break
        if pick:
            modmap[ii] = pick
            located += 1
        if ii < 5 or not pick:
            out("  image[%2d] %-44s methods=%-6d %s"
                % (ii, iname, want,
                   ("CodeGenModule@0x%08X cnt=%d methodPointers=0x%08X"
                    % (pick[0], pick[1], pick[2])) if pick else "未定位"))
    out("  CodeGenModule 定位成功: %d / %d 个 image（methodPointerCount 与 metadata 计数交叉校验一致）"
        % (located, len(images)))

    # ---------------- 方法 -> RVA
    # 磁盘上 methodPointers[] 元素被清零，真实地址在 .rela.dyn 的 addend 里
    rows = []
    unresolved_img = 0
    nz = 0
    for i, (name_i, decl, tok) in enumerate(methods):
        if not (0 <= decl < ntypes):
            continue
        ii = type2image[decl]
        if ii not in modmap:
            unresolved_img += 1
            continue
        lv, cnt, ptr, po, iname = modmap[ii]
        rid = tok & 0xFFFFFF
        if not (1 <= rid <= cnt):
            continue
        a = ptr + (rid - 1) * 8
        rva = rel_by_off.get(a)
        if rva is None:
            ao = v2o(a)
            if ao is None:
                continue
            rva = u64(mm, ao)
            if rva:
                nz += 1
        if not rva:
            continue
        nm = S(name_i) or "?"
        rows.append((rva, "%s::%s" % (full_type(decl, types, ntypes, S), nm), iname, i))
    rows.sort()
    out("方法 RVA 解析成功: %d / %d（image 未定位跳过 %d；其中从磁盘原始值解析 %d）"
        % (len(rows), nmethods, unresolved_img, nz))

    with open(OUT_METHODS, "w", encoding="utf-8") as f:
        for rva, full, _i, _x in rows:
            f.write("0x%08X\t%s\n" % (rva, full))

    # ---------------- 目标 RVA 反查
    out("=" * 74)
    rvas = [r[0] for r in rows]
    rvaset = set(rvas)
    for t in TARGETS:
        p = bisect.bisect_left(rvas, t)
        exact = sorted(set(r[1] for r in rows if r[0] == t))
        if exact:
            out("RVA 0x%08X: 精确命中方法入口 -> %s" % (t, "; ".join(exact[:4])))
        else:
            prev = rows[p - 1] if p > 0 else None
            nxt = rows[p] if p < len(rows) else None
            s = "RVA 0x%08X: " % t
            if prev:
                s += "方法内部 -> %s + 0x%X" % (prev[1], t - prev[0])
            else:
                s += "无前驱方法"
            if nxt:
                s += "  | 下一方法 %s @0x%08X (+0x%X)" % (nxt[1], nxt[0], nxt[0] - t)
            out(s)
    out("  目标是否本身是 methodPointer: " +
        ", ".join("%s=%s" % (hex(t), "是" if t in rvaset else "否") for t in TARGETS))

    fo = v2o(TARGETS[0])
    out("  0x%08X fileoff=0x%X 前后 0x40 字节:" % (TARGETS[0], fo))
    for k in range(0, 0x80, 16):
        va = TARGETS[0] - 0x40 + k
        chunk = mm[fo - 0x40 + k:fo - 0x40 + k + 16]
        mark = " <== 目标" if k == 0x40 else ""
        out("    %08X  %s  %s%s" % (va,
            " ".join("%02x" % b for b in chunk[:8]),
            " ".join("%02x" % b for b in chunk[8:16]), mark))

    # ---------------- 段表定位（.text / il2cpp / .eh_frame / .dynsym）
    shstr_off, secs = 0, []
    for i in range(e_shnum):
        o = e_shoff + i * e_shentsize
        secs.append((u32(mm, o), struct.unpack_from("<Q", mm, o + 8)[0],
                     struct.unpack_from("<Q", mm, o + 16)[0],
                     struct.unpack_from("<Q", mm, o + 24)[0],
                     struct.unpack_from("<Q", mm, o + 32)[0],
                     struct.unpack_from("<Q", mm, o + 56)[0]))
    if secs:
        shstr_off = secs[u16(mm, 0x3E)][3]
    def shname(no):
        e = mm.find(b"\0", shstr_off + no)
        return mm[shstr_off + no:e].decode("latin1")
    names = {shname(s[0]): s for s in secs}
    for nm in (".text", "il2cpp", ".plt", ".eh_frame", ".dynsym", ".dynstr", ".data.rel.ro"):
        if nm in names:
            t, fl, va, of, sz, es = names[nm]
            out("  SEC %-13s vaddr=0x%08X off=0x%08X size=0x%X entsize=%d flags=%d" % (nm, va, of, sz, es, fl))
    text_v = names[".text"][2] if ".text" in names else 0
    text_e = text_v + (names[".text"][4] if ".text" in names else 0)
    il2_v = names["il2cpp"][2] if "il2cpp" in names else 0
    il2_e = il2_v + (names["il2cpp"][4] if "il2cpp" in names else 0)

    # ---------------- .eh_frame：函数边界权威（FDE）
    fdes, cie = [], 0
    if ".eh_frame" in names:
        ehs, ehsz = names[".eh_frame"][3], names[".eh_frame"][4]
        p = ehs
        while p + 8 <= ehs + ehsz:
            ln = u32(mm, p)
            if ln in (0, 0xFFFFFFFF):
                break
            if u32(mm, p + 4) == 0:
                cie += 1
            else:
                fld = p + 8
                ini = fld + struct.unpack_from("<i", mm, fld)[0]
                rng = u32(mm, p + 12)
                if rng and ini:
                    fdes.append((ini, rng))
            p += 4 + ln
        fdes.sort()
    fst = [x[0] for x in fdes]
    def fde_of(t):
        i = bisect.bisect_right(fst, t) - 1
        if i < 0:
            return None
        a, r = fdes[i]
        return (a, r) if a <= t < a + r else None
    out("=" * 74)
    out(".eh_frame FDE %d 条（CIE %d）—— 函数边界权威" % (len(fdes), cie))
    tgt_fde = {}
    for t in TARGETS:
        c = fde_of(t)
        tgt_fde[t] = c
        if c:
            out("  0x%08X -> 所属函数 [0x%08X,0x%08X) size=0x%X，偏移 +0x%X %s"
                % (t, c[0], c[0] + c[1], c[1], t - c[0],
                   "（= 函数入口）" if t == c[0] else "（函数内部，非入口）"))
        else:
            out("  0x%08X -> 无 FDE 覆盖（非函数入口，可能是数据/跳转表）" % t)

    # ---------------- .dynsym：目标是否就是导出符号（壳自解析导出表用的那批）
    dymap = {}
    if ".dynsym" in names and ".dynstr" in names:
        t_, fl_, va_, of_, sz_, es_ = names[".dynsym"]
        strbase = names[".dynstr"][3]
        n = sz_ // (es_ or 24)
        for k in range(n):
            p = of_ + k * (es_ or 24)
            no, info, other, shndx = u32(mm, p), mm[p + 4], mm[p + 5], u16(mm, p + 6)
            val, size2 = struct.unpack_from("<Q", mm, p + 8)[0], struct.unpack_from("<Q", mm, p + 16)[0]
            if (info & 0xF) != 2 or not val:          # STT_FUNC 且地址非 0
                continue
            e = mm.find(b"\0", strbase + no)
            nmx = mm[strbase + no:e].decode("latin1")
            dymap[val] = (nmx, size2)
        ntext = sum(1 for v in dymap if text_v <= v < text_e)
        nil2 = sum(1 for v in dymap if il2_v <= v < il2_e)
        tramp = sum(1 for v in dymap if text_v <= v < text_e and dymap[v][1] == 4)
        out("  .dynsym %d 条 FUNC：落在 .text %d 条（其中 st_size==4 的 b 跳板 %d 条），落在 il2cpp 节 %d 条"
            % (len(dymap), ntext, tramp, nil2))
        for t in TARGETS:
            if t in dymap:
                nmx, size2 = dymap[t]
                s = "  0x%08X == 导出符号 %s (st_size=%d" % (t, nmx, size2)
                if size2 == 4 and v2o(t):
                    w = u32(mm, v2o(t))
                    imm = w & 0x3FFFFFF
                    if imm & 0x2000000:
                        imm -= 0x4000000
                    tgt = t + imm * 4
                    s += "，指令 0x%08X = b 0x%08X" % (w, tgt)
                    c = fde_of(tgt)
                    if c:
                        s += "，真实现 [0x%08X,0x%08X) size=0x%X" % (c[0], c[0] + c[1], c[1])
                s += ")"
                out(s)
            else:
                out("  0x%08X != 任何导出符号（.text 内静态函数，无符号名）" % t)

    # ---------------- 方法 RVA 落在哪个节（判定 metadata 能否命名目标）
    nin_txt = sum(1 for r in rows if text_v <= r[0] < text_e)
    nin_il2 = sum(1 for r in rows if il2_v <= r[0] < il2_e)
    out("  metadata methodPointer 分布: .text 段 %d 条, il2cpp(AOT) 段 %d 条 ⇒ %s"
        % (nin_txt, nin_il2, "metadata 只能命名 il2cpp 段的方法，目标 RVA 全在 .text 故无法命名"
           if nin_il2 > nin_txt else "存在 .text 内方法"))

    # ---------------- 关键词命中（游戏程序集优先 + 短词词边界去噪）
    GAME_ASM = ("Launch.dll", "Assembly-CSharp-firstpass.dll", "HelpAssemble.dll",
                "DestMath.dll", "CString.dll", "MeshFusionPro.Runtime.dll", "spine-unity.dll",
                "DOTween.dll", "Unity.Mathematics.dll")
    SHORT = {"hp", "ad", "god", "hit", "die", "win", "dead", "hurt"}
    def is_word_start(s, i):
        return i == 0 or not s[i - 1].isalnum()
    def bound_ok(short, kl):
        i = 0
        while True:
            i = short.find(kl, i)
            if i < 0:
                return False
            j = i + len(kl)
            if (is_word_start(short, i) or (kl[0].isupper() and short[i - 1].islower())
                    or (kl[0].isupper() and short[i - 1].isdigit())):
                if j >= len(short) or not short[j].isalpha() or short[j].isupper():
                    return True
            i += 1
    groups = collections.OrderedDict()
    low = [(k.lower(), k) for k in KEYWORDS]
    for rva, full, asm, _x in rows:
        short = full.split("::")[-1]
        slow = short.lower()
        tag = (0 if asm in GAME_ASM else 1, asm, rva)
        for kl, k in low:
            if kl not in slow:
                continue
            if kl in SHORT and not bound_ok(short, k):
                continue
            groups.setdefault(k, []).append((tag, rva, full, asm))
    with open(OUT_HITS, "w", encoding="utf-8") as f:
        f.write("# metadata 方法名关键词命中（每关键词 <=60 行；游戏程序集优先：%s）\n\n"
                % ", ".join(GAME_ASM))
        for k in KEYWORDS:
            v = sorted(groups.get(k, []))
            f.write("# %s —— 命中 %d 条（游戏程序集 %d 条）\n"
                    % (k, len(v), sum(1 for x in v if x[3] in GAME_ASM)))
            for _tag, rva, full, asm in v[:60]:
                f.write("0x%08X\t%-32s\t%s\n" % (rva, asm, full))
            f.write("\n")
    out("关键词命中: %d 组, 合计 %d 条（其中游戏程序集 %d 条）"
        % (len(groups), sum(len(v) for v in groups.values()),
           sum(1 for v in groups.values() for x in v if x[3] in GAME_ASM)))

    # ---------------- 中文串命中
    cn_re = re.compile("|".join(CN_KEYWORDS))
    cjk_re = re.compile(r"[\u4e00-\u9fff]")
    cn_rows = []
    pos = 0
    while pos < len(str_blob) and len(cn_rows) < 100:
        e = str_blob.find(b"\0", pos)
        if e < 0:
            break
        raw = str_blob[pos:e]
        pos = e + 1
        if raw:
            txt = raw.decode("utf-8", "replace")
            if cjk_re.search(txt) and cn_re.search(txt):
                cn_rows.append(("string", txt))
    lo_, ls_ = H["stringLiteralDataOffset"], H["stringLiteralDataSize"]
    if len(cn_rows) < 40 and ls_ > 0 and lo_ + ls_ <= size:
        lb = data[lo_:lo_ + ls_]
        pos = 0
        while pos < len(lb) and len(cn_rows) < 100:
            e = lb.find(b"\0", pos)
            if e < 0:
                break
            raw = lb[pos:e]
            pos = e + 1
            if raw:
                txt = raw.decode("utf-8", "replace")
                if cjk_re.search(txt) and cn_re.search(txt):
                    cn_rows.append(("lit", txt))
    with open(OUT_CN, "w", encoding="utf-8") as f:
        for src, txt in cn_rows:
            f.write("[%s]\t%s\n" % (src, txt))
    out("中文串命中: %d 条（string 表 + stringLiteral 区；该游戏中文文案不在 metadata 内）" % len(cn_rows))
    cjk_all = 0
    for m in re.finditer(rb"[\xe4-\xe9][\x80-\xbf][\x80-\xbf]", data):
        if m.start() > 1024:
            cjk_all += 1
    out("  全文件 3 字节 UTF-8 CJK 序列共 %d 处（几乎全为二进制巧合字节，非可读中文串）" % cjk_all)

    out("=" * 74)
    order = ["Notice", "Announce", "Bulletin", "Damage", "Hurt", "Hp", "Health",
             "Invincible", "Immortal", "God", "Reward", "Ad", "Drop", "Kill", "Monster",
             "Enemy", "Skill", "Attack", "Hit", "Win", "Battle"]
    top_rows, seen = [], set()
    for k in order:
        for _tag, rva, full, asm in sorted(groups.get(k, [])):
            if asm not in GAME_ASM or full in seen:
                continue
            seen.add(full)
            top_rows.append((k, rva, full, asm))
    for k in order:                                  # 游戏程序集不够 20 条时用其余补足
        for _tag, rva, full, asm in sorted(groups.get(k, [])):
            if full in seen:
                continue
            seen.add(full)
            top_rows.append((k, rva, full, asm))
    with open(OUT_HITS, "a", encoding="utf-8") as f:
        f.write("# TOP（游戏程序集优先，去重）\n")
        for k, rva, full, asm in top_rows[:60]:
            f.write("0x%08X\t%-32s\t%-14s\t%s\n" % (rva, asm, k, full))
        f.write("\n")
    for k, rva, full, asm in top_rows[:20]:
        out("TOP\t%-9s 0x%08X\t%-30s\t%s" % (k, rva, asm, full))
    for src, txt in cn_rows[:10]:
        out("CNTOP\t[%s]\t%s" % (src, txt))

    mm.close()
    fh.close()
    with open(os.path.join(HERE, "_meta_parse.log"), "w", encoding="utf-8") as f:
        f.write("\n".join(REPORT) + "\n")
    return 0


def full_type(ti, types, ntypes, S, _cache={}):
    if ti in _cache:
        return _cache[ti]
    if not (0 <= ti < ntypes):
        return "?"
    t = types[ti]
    nm = S(t["name_i"]) or "?"
    ns = S(t["ns_i"]) or ""
    r = (ns + "." + nm) if ns else nm
    _cache[ti] = r
    return r


if __name__ == "__main__":
    sys.exit(main())
