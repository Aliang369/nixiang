#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""一念通天 RVA 重定位提取器（换包必跑）。

一次扫描 dump.cs，打印/落盘固定挂点集的「类::方法 + Offset(file offset) + VA + 序言首指令 + 字段偏移」，
供 YnttMod.cpp 的 kRva*/kIns*/kOff* 回填。序言从同一份 libil2cpp.so 的 **file offset** 实测。
注意（2026-10-30 实测，768 起）：本作 `.text` 段 p_offset≠p_vaddr（768 差 0x4000），
dump.cs 的「Offset / RVA / VA」三列不再相等；`HookAtRva` 走 `FileOffToVa()`（按 file offset
查 /proc/self/maps 换算 VA），因此 **kRva* 必须填 Offset 列**（json 里的 `file_offset`/`rva`）。

用法:
  python3 04_scripts/一念通天/yntt_rva_extract.py \
      --dump 06_dumped/一念通天/official_747/static/il2cpp_dump/dump.cs \
      --so   06_dumped/一念通天/official_747/static/libil2cpp.so \
      --version 747 \
      --out  06_dumped/一念通天/official_747/rva_targets.json \
      [--prev 06_dumped/一念通天/official_739/rva_targets.json]
"""
import argparse
import json
import re
import struct
import sys

# 固定挂点集（与 10_docs/一念通天/星空落地_739_20260930.md「功能与挂点」一一对应）
TARGETS = [
    ("MonsterReduceHP",    "monsterStatus",     "ReduceHP",              "秒杀：伤害 ×100"),
    ("NpcReduceHP",        "NpcStatus",         "ReduceHP",              "秒杀：仅 type==2(ENEMY) 的 Npc 伤害 ×100"),
    ("MonsterOnDamageBuff", "monsterStatus",    "OnDamageBuff",          "秒杀：持续伤害层数 ×100"),
    ("NpcOnDamageBuff",    "NpcStatus",         "OnDamageBuff",          "秒杀：Npc 持续伤害层数 ×100（仅 ENEMY）"),
    ("charStatus.onDamage", "charStatus",       "onDamage",              "无敌：正伤害参数置 0 后再调原函数"),
    ("Time.set_timeScale", "UnityEngine.Time",  "set_timeScale",         "战斗加速：滑条 2..10 倍率；1=关"),
    ("GameRuning.Update",  "GameRuning",        "Update",                "无限体力/无限货币：orig 后回写字段（只上调不回降）"),
    ("GameRuning.ShowAD",  "GameRuning",        "ShowAD",                "免广告：写 adRequest 后经 il2cpp_runtime_invoke 调官方 onADSuccess"),
    ("GameRuning.onADSuccess", "GameRuning",    "onADSuccess",           "免广告：runtime_invoke 目标（不直接 hook）"),
]
# 字段偏移集（Class::Field）
FIELDS = [
    ("GameRuning", "gas"), ("GameRuning", "tili"), ("GameRuning", "lingshi"),
    ("GameRuning", "money"), ("GameRuning", "NPCData"), ("GameRuning", "fumodata"),
    ("GameRuning", "adRequest"), ("fumoData", "tili"), ("fumoData", "tili_zq"),
    ("fumoData", "tili_guoqin"), ("npcData", "ZLDGTili"), ("NpcStatus", "type"),
]

CLASS_RE = re.compile(r'^(?:public|internal|private)\s+(?:sealed\s+|abstract\s+)?(?:class|struct|enum)\s+([A-Za-z_][\w.]*)')
RVA_RE = re.compile(r'//\s*RVA:\s*(0x[0-9A-Fa-f]+)\s+Offset:\s*(0x[0-9A-Fa-f]+)\s+VA:\s*(0x[0-9A-Fa-f]+)')
FIELD_RE = re.compile(r'^\s*(?:public|private|protected|internal)?\s*(?:static\s+)?[\w\.\<\>\[\]\*,\s]+?\s([A-Za-z_]\w*)\s*;\s*//\s*(0x[0-9A-Fa-f]+)')
METHOD_RE = re.compile(r'^\s*(?:public|private|protected|internal)\s+(?:static\s+|virtual\s+|override\s+|unsafe\s+)*[\w\.\<\>\[\]\*,\s]+\s(\w+)\s*\(')
NAMESPACE_RE = re.compile(r'^//\s*Namespace:\s*(.*)$')


def read_prologue(so_path, off, n=4):
    with open(so_path, "rb") as f:
        f.seek(off)
        b = f.read(n)
    if len(b) < n:
        return None, None
    return int.from_bytes(b, "little"), b.hex(" ")


def parse_dump(path):
    """→ {class: {'ns':str,'line':int,'fields':{name:(off,line)},'methods':{name:((rva,off,va,line),sig)}}}"""
    want_cls = {c for _, c, _, _ in TARGETS} | {c for c, _ in FIELDS}
    want_m = {}
    for _, c, m, _ in TARGETS:
        want_m.setdefault(c, set()).add(m)
    want_f = {}
    for c, f in FIELDS:
        want_f.setdefault(c, set()).add(f)

    out = {}
    cur = None
    ns = ""
    pending_rva = None
    with open(path, "r", encoding="utf-8", errors="replace") as f:
        for lineno, line in enumerate(f, 1):
            m = NAMESPACE_RE.match(line)
            if m:
                ns = m.group(1).strip()
                continue
            c = CLASS_RE.match(line)
            if c:
                name = c.group(1)
                qname = ("%s.%s" % (ns, name)) if ns else name
                key = qname if qname in want_cls else (name if name in want_cls else None)
                cur = out.setdefault(qname, {"ns": ns, "line": lineno, "fields": {}, "methods": {}}) if key else None
                if cur is not None:
                    cur["ns"] = ns
                    cur["line"] = lineno
                    cur["_key"] = key
                pending_rva = None
                continue
            r = RVA_RE.search(line)
            if r:
                pending_rva = (r.group(1), r.group(2), r.group(3), lineno)
                continue
            if cur is None:
                continue
            # <X>k__BackingField 无空格前缀，先归一化成普通字段名再匹配
            norm = re.sub(r'<(\w+)>k__BackingField', r'\1', line)
            fld = FIELD_RE.match(norm)
            if fld:
                fname = fld.group(1)
                if fname.startswith("_"):
                    fname = fname[1:]
                cname = _clsname(out, cur)
                if fname in want_f.get(cname, set()):
                    cur["fields"][fname] = (fld.group(2), lineno)
                    continue
            met = METHOD_RE.match(line)
            if met:
                nm = met.group(1)
                cname = _clsname(out, cur)
                if nm in want_m.get(cname, set()) and pending_rva:
                    # 整条注释三元组原样带出：(dump RVA, dump Offset, dump VA, dump 行号)
                    cur["methods"][nm] = (pending_rva, line.strip())
                pending_rva = None
    return out


def _clsname(out, obj):
    for k, v in out.items():
        if v is obj:
            return k
    return ""


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dump", required=True)
    ap.add_argument("--so", required=True)
    ap.add_argument("--version", required=True)
    ap.add_argument("--out")
    ap.add_argument("--prev", help="上一版本 rva_targets.json，用于打印漂移对照")
    a = ap.parse_args()

    classes = parse_dump(a.dump)
    prev = json.load(open(a.prev, encoding="utf-8")) if a.prev else None
    prev_t = {t["tag"]: t for t in prev["targets"]} if prev else {}
    prev_f = prev.get("field_offsets", {}) if prev else {}

    targets = []
    for tag, cls, meth, role in TARGETS:
        c = classes.get(cls)
        if not c or meth not in c["methods"]:
            print("[MISS] %s :: %s.%s" % (tag, cls, meth), file=sys.stderr)
            targets.append({"tag": tag, "class": cls, "method": meth, "role": role, "status": "MISSING"})
            continue
        rva_comment, sig = c["methods"][meth]
        dump_rva_s, dump_off_s, dump_va_s = rva_comment[0], rva_comment[1], rva_comment[2]
        lineno = rva_comment[3]
        # 关键：YnttMod.cpp 的 HookAtRva 用 FileOffToVa()（/proc/self/maps 的 file offset → VA），
        # 所以 kRva* 必须填 **dump.cs 的 Offset 列**，不是 RVA/VA 列。768 起 Offset≠VA（.text 段
        # p_offset 0x1bbeb9c / p_vaddr 0x1bc2b9c，差 0x4000），填错会 0x4000 偏移挂空页。
        off = int(dump_off_s, 16)
        va = int(dump_va_s, 16)
        pro_le, pro_bytes = read_prologue(a.so, off)
        argc = sig.count(",") + 1 if "(" in sig and sig.split("(", 1)[1].strip().startswith((")",)) is False else 0
        if "(" in sig:
            inner = sig.split("(", 1)[1].rsplit(")", 1)[0].strip()
            argc = 0 if inner == "" else len(inner.split(","))
        item = {
            "tag": tag, "class": cls, "method": meth, "argc": argc,
            "managed_sig": sig,
            "file_offset": dump_off_s,   # = YnttMod.cpp kRva*（钩子真实取值）
            "rva": dump_off_s,           # 历史字段名，语义同 file_offset（勿与 dump_rva 混用）
            "dump_rva": dump_rva_s,      # dump.cs「RVA」列 = 运行时 VA
            "va": dump_va_s,             # dump.cs「VA」列 = so 基址 + va
            "seg_delta": "0x%X" % (va - off),
            "prologue_le": ("0x%08X" % pro_le) if pro_le is not None else None,
            "prologue_bytes": pro_bytes, "role": role, "dump_line": lineno,
        }
        p = prev_t.get(tag)
        if p:
            item["prev_rva"] = p.get("rva")
            item["prev_prologue_le"] = p.get("prologue_le")
            item["drift"] = (p.get("rva") != dump_off_s) or (p.get("prologue_le") != item["prologue_le"])
        targets.append(item)

    fields = {}
    for cls, fld in FIELDS:
        c = classes.get(cls)
        if c and fld in c["fields"]:
            fields["%s.%s" % (cls, fld)] = c["fields"][fld][0]
        else:
            fields["%s.%s" % (cls, fld)] = "MISSING"

    data = {
        "game": "一念通天", "package": "com.qinglv.Immortal",
        "version": a.version, "game_id": "yntt",
        "note": "锚点摘录；整份 dump.cs 可再生（dump_static_official_yntt_%s.sh）。" % a.version,
        "targets": targets, "field_offsets": fields, "verified": False,
    }
    if a.out:
        with open(a.out, "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
            f.write("\n")
        print("written: %s" % a.out)

    print("\n%-24s %-12s %-12s %-6s %-12s %-8s %s" % ("tag", "kRva(=Offset)", "va", "argc", "prologue", "drift", "prev_kRva"))
    deltas = set()
    for t in targets:
        if t.get("status") == "MISSING":
            print("%-24s MISSING" % t["tag"])
            continue
        deltas.add(t["seg_delta"])
        print("%-24s %-12s %-12s %-6s %-12s %-8s %s" % (
            t["tag"], t["file_offset"], t["va"], t.get("argc", "-"), t.get("prologue_le", "-"),
            ("YES" if t.get("drift") else "no") if "drift" in t else "-",
            t.get("prev_rva", "")))
    if deltas != {"0x0"}:
        print("\n[警告] dump Offset 与 VA 不等（seg_delta=%s）：kRva* 填 **file_offset** 列"
              "（HookAtRva 走 FileOffToVa 按 file offset 解析），填 va 会整体偏移挂空页。"
              % ",".join(sorted(deltas)))
    print("\nfield_offsets (new / prev):")
    for k, v in fields.items():
        pv = prev_f.get(k)
        pv = pv.split("（")[0] if isinstance(pv, str) else pv
        mark = "" if v == pv else "  <-- CHANGE"
        print("  %-22s %-8s prev=%s%s" % (k, v, pv, mark))


if __name__ == "__main__":
    main()
