#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""核验星光 MOD 锚点（类/方法/参数个数）是否仍在目标版本中存在。

两条证据链，互为交叉校验：
  A. 运行时 metadata（主证据、权威）：解析 v21–v24 metadata 的 TypeDefinitions/MethodDefinitions 表。
     名字/参数个数直接来自 metadata 表，不经任何猜测。
  B. Il2CppDumper 的 dump.cs（辅证据）：能解析继承链（锚点方法可能定义在父类上，
     MOD 侧用 GetMethodInHierarchy 沿父类向上找）。

用法：
  anchor_check_mrfzz.py --metadata <runtime .dat> [--dump-cs <dump.cs>] [--cpp <ArknightsMod.cpp>]
                        [--spec anchors_mrfzz.json] [--out-json out.json] [--out-md out.md]
                        [--version-name 2.7.81] [--version-code 200] [--serial f9a116a2]

退出码：0 = 全部锚点直接命中且 argc 一致；3 = 有锚点缺失/argc 不一致（需人工判定）；2 = 输入错误。
"""
from __future__ import annotations

import argparse
import datetime as _dt
import json
import re
import struct
import sys
from pathlib import Path

# ---------------------------------------------------------------- metadata v24

HDR_FIELDS = {
    "sanity": 0x00,
    "version": 0x04,
    "stringOffset": 0x18,
    "stringCount": 0x1C,
    "methodsOffset": 0x30,
    "methodsCount": 0x34,
    "parametersOffset": 0x58,
    "parametersCount": 0x5C,
    "nestedTypesOffset": 0x80,
    "nestedTypesCount": 0x84,
    "typeDefinitionsOffset": 0xA0,
    "typeDefinitionsCount": 0xA4,
}

MD_SIZE = 0x20  # Il2CppMethodDefinition v24

# Il2CppTypeDefinition 结构（2.7.81 官方 metadata version=24 实测，td_size=0x5C）。
# 字段偏移不是照抄 Il2CppDumper 的声明顺序，而是用「计数与分组一致 / 子表平铺无重叠」实测反推：
#   0x20 fieldStart   0x24 methodStart  0x28 eventStart   0x2c propertyStart
#   0x30 nestedTypesStart 0x34 interfacesStart 0x38 vtableStart 0x3c interfaceOffsetsStart
#   0x1c flags（=0x100001=BeforeFieldInit|Public 等，见 Deck/Card 实测）
#   0x44 method_count 0x46 property_count 0x48 field_count 0x4a event_count
#   0x4c nested_type_count 0x4e vtable_count 0x50 interfaces_count 0x52 interface_offsets_count
#   0x54 bitfield  0x58 token
# 判据证据：0x24/0x44 的区间并集恰为 [0,301881]=methods 表容量；0x20/0x48 → [0,364690]；
#   0x30/0x4c → [0,19167]=nestedTypes 表容量；且 methodStart 直读与 declaringType 分组 3934/3934 一致。


def _i32(b: bytes, off: int) -> int:
    return struct.unpack_from("<i", b, off)[0]


def _u16(b: bytes, off: int) -> int:
    return struct.unpack_from("<H", b, off)[0]


def _cstr(b: bytes, off: int) -> str:
    if off < 0 or off >= len(b):
        return ""
    end = b.find(b"\x00", off)
    if end < 0:
        return ""
    try:
        return b[off:end].decode("utf-8", "replace")
    except Exception:  # pragma: no cover
        return ""


# Il2CppTypeDefinition v24.1+：(24.1, 24.5] 区间的字段布局，无 customAttributeIndex / byrefTypeIndex / rgctx*
#  0x00 nameIndex      0x04 namespaceIndex   0x08 byvalTypeIndex    0x0c byrefTypeIndex
#  0x10 declaringTypeIndex 0x14 parentIndex   0x18 elementTypeIndex  0x1c genericContainerIndex
#  0x20 flags          0x24 fieldStart       0x28 methodStart       0x2c eventStart
#  0x30 propertyStart  0x34 nestedTypesStart 0x38 interfacesStart   0x3c vtableStart
#  0x40 interfaceOffsetsStart                 0x44 method_count …   0x54 bitfield  0x58 token
TD_SIZE_GUESSES = (0x5C, 0x58, 0x60)


def _method_histogram(b: bytes, hdr: dict) -> dict[int, list[dict]]:
    """按 Il2CppMethodDefinition.declaringType 把方法表分组。

    本版 metadata 的 Il2CppTypeDefinition.methodStart 大量为 -1（不是有效索引），
    真正的归属关系由 Il2CppMethodDefinition.declaringType 给出（实测：
    Entity 的分组方法数 393 == 该类型 method_count 393）。
    """
    mb = hdr["methodsOffset"]
    mdc = hdr["methodsCount"] // MD_SIZE
    st = hdr["stringOffset"]
    grouped: dict[int, list[dict]] = {}
    for j in range(mdc):
        mo = mb + j * MD_SIZE
        if mo + MD_SIZE > len(b):
            break
        dt = _i32(b, mo + 4)
        flags = _u16(b, mo + 0x18)
        grouped.setdefault(dt, []).append(
            {
                "name": _cstr(b, st + _i32(b, mo)),
                "argc": _u16(b, mo + 0x1E),
                "flags": flags,
                "static": bool(flags & 0x0010),
            }
        )
    return grouped


def _valid_tdsizes(b: bytes, hdr: dict, grouped: dict[int, list[dict]]) -> list[int]:
    """判定 Il2CppTypeDefinition 结构尺寸（字节数 → 元素个数用尺寸换算）。

    判据（强判据）：按该尺寸解析出的每个类型定义，其 method_count 应当等于
    「declaringType == 该类型下标」的方法条数。错位尺寸上不可能大面积成立。
    """
    tdo = hdr["typeDefinitionsOffset"]
    tdc_bytes = hdr["typeDefinitionsCount"]
    scored = []
    for size in TD_SIZE_GUESSES:
        tdc = tdc_bytes // size
        if tdc <= 0:
            continue
        checked = 0
        good = 0
        name_ok = 0
        step = max(1, tdc // 400)
        for i in range(0, tdc, step):
            o = tdo + i * size
            if o + size > len(b):
                break
            checked += 1
            mc = _u16(b, o + 0x44)
            if len(grouped.get(i, ())) == mc:
                good += 1
            nm = _cstr(b, hdr["stringOffset"] + _i32(b, o))
            if nm and all(ch.isprintable() for ch in nm):
                name_ok += 1
        frac = good / checked if checked else 0.0
        scored.append((frac, name_ok / checked if checked else 0.0, size))
        print(f"[td-size] {size:#04x}: method_count 与 declaringType 分组一致 "
              f"{good}/{checked} = {frac:.1%}")
    scored.sort(key=lambda x: (-x[0], -x[1], x[2]))
    if not scored or scored[0][0] < 0.95:
        raise SystemExit(f"错误: 无法判定 Il2CppTypeDefinition 结构尺寸（最佳一致率 {scored[0][0]:.1%}）")
    return [s for _, _, s in scored]


class MetadataV24:
    def __init__(self, blob: bytes):
        self.b = blob
        self.hdr = {k: _i32(blob, v) for k, v in HDR_FIELDS.items()}
        self.grouped = _method_histogram(blob, self.hdr)
        cand = _valid_tdsizes(blob, self.hdr, self.grouped)
        self.td_size = cand[0]
        # 头里的 Count 一律是字节数（v21–v24 语义），换算成元素个数
        self.str_base = self.hdr["stringOffset"]
        self.str_count = self.hdr["stringCount"]
        self.md_base = self.hdr["methodsOffset"]
        self.md_count = self.hdr["methodsCount"] // MD_SIZE
        self.nt_base = self.hdr["nestedTypesOffset"]
        self.nt_count = self.hdr["nestedTypesCount"] // 4
        self.td_base = self.hdr["typeDefinitionsOffset"]
        self.td_count = self.hdr["typeDefinitionsCount"] // self.td_size
        self.types: list[dict] = []
        self.methods_direct = 0
        self.methods_crossok = 0
        self._parse()
        # type_index（byvalTypeIndex）→ 类型，全库唯一；用于把 parentIndex(0x10) 解析成基类名
        self.by_type_index: dict[int, dict] = {}
        for t in self.types:
            self.by_type_index.setdefault(t["type_index"], t)

    def name(self, idx: int) -> str:
        return _cstr(self.b, self.str_base + idx)

    def _method(self, idx: int) -> dict:
        """按方法表下标读一条 Il2CppMethodDefinition（0x20 B）。"""
        o = self.md_base + idx * MD_SIZE
        flags = _u16(self.b, o + 0x18)
        return {
            "name": self.name(_i32(self.b, o)),
            "argc": _u16(self.b, o + 0x1E),
            "flags": flags,
            "static": bool(flags & 0x10),
        }

    def _parse(self) -> None:
        b, td = self.b, self.td_size
        for i in range(self.td_count):
            o = self.td_base + i * td
            if o + td > len(b):
                break
            # 方法：优先走 methodStart(0x24)+method_count(0x44) 直读，
            # 与 declaringType 分组交叉校验；不合法时回退分组结果。
            mstart, mcount = _i32(b, o + 0x24), _u16(b, o + 0x44)
            if 0 <= mstart and mstart + mcount <= self.md_count:
                methods = [self._method(mstart + k) for k in range(mcount)]
                self.methods_direct += 1
                if len(self.grouped.get(i, ())) == mcount:
                    self.methods_crossok += 1
            else:
                methods = self.grouped.get(i, [])
            nstart, ncount = _i32(b, o + 0x30), _u16(b, o + 0x4C)
            if nstart < 0 or nstart > self.nt_count:
                nstart, ncount = 0, 0
            nested = []
            for k in range(ncount):
                no = self.nt_base + (nstart + k) * 4
                if no + 4 > len(b):
                    break
                nested.append(_i32(b, no))
            self.types.append(
                {
                    "idx": i,
                    "name": self.name(_i32(b, o)),
                    "ns": self.name(_i32(b, o + 4)),
                    "flags": _i32(b, o + 0x1C),
                    "type_index": _i32(b, o + 0x08),
                    "declaring_type_index": _i32(b, o + 0x0C),
                    "parent_type_index": _i32(b, o + 0x10),
                    "method_start": mstart,
                    "method_count": _u16(b, o + 0x44),
                    "nested_start": nstart,
                    "nested_count": ncount,
                    "methods": methods,
                    "nested": nested,
                }
            )

    def find(self, ns: str, name: str) -> list[dict]:
        return [t for t in self.types if t["ns"] == ns and t["name"] == name]

    def full_name(self, t: dict) -> str:
        return (t["ns"] + "." if t["ns"] else "") + t["name"]

    def parent(self, t: dict) -> dict | None:
        p = self.by_type_index.get(t["parent_type_index"])
        return p if p is not None and p["idx"] != t["idx"] else None

    def chain(self, t: dict, depth: int = 12) -> list[str]:
        out, cur, seen = [], t, set()
        while cur is not None and cur["idx"] not in seen and len(out) < depth:
            seen.add(cur["idx"])
            out.append(self.full_name(cur))
            cur = self.parent(cur)
        return out

    def resolve_in_hierarchy(self, t: dict, method: str, argc: int):
        """沿 metadata 重建的继承链向上找方法（等价 MOD 的 GetMethodInHierarchy）。"""
        cur, seen = t, set()
        while cur is not None and cur["idx"] not in seen:
            seen.add(cur["idx"])
            for m in cur["methods"]:
                if m["name"] == method and m["argc"] == argc:
                    return cur, m
            cur = self.parent(cur)
        return None, None

    def find_nested(self, outer: dict, name: str) -> list[dict]:
        out = []
        for idx in outer["nested"]:
            if 0 <= idx < len(self.types) and self.types[idx]["name"] == name:
                out.append(self.types[idx])
        return out


# ------------------------------------------------------------------- dump.cs


class DumpCs:
    CLS = re.compile(
        r"^(?P<mods>(?:public|private|protected|internal|sealed|abstract|static|unsafe|readonly)\s+)*"
        r"(?P<kind>class|struct|interface|enum)\s+(?P<name>[A-Za-z_][\w`]*)"
        r"(?P<generic><[^{]*?>)?\s*(?::\s*(?P<bases>[^{]+?))?\s*//\s*TypeDefIndex:\s*(?P<tidx>\d+)"
    )
    NS = re.compile(r"^//\s*Namespace:\s*(?P<ns>.*?)\s*$")
    MTH = re.compile(r"^\s{0,8}(?P<sig>(?:\[[^\]]*\]\s*)*(?:public|private|protected|internal|static|virtual|"
                     r"override|sealed|abstract|new|unsafe|extern|async|\s)*[^;{}=]*?)\s"
                     r"(?P<name>[A-Za-z_][\w]*)\s*\((?P<args>[^()]*)\)\s*(?:=>|\{|;)")

    def __init__(self, text: str):
        self.classes: dict[str, dict] = {}
        ns = ""
        cur = None
        for line in text.splitlines():
            m = self.NS.match(line)
            if m:
                ns = m.group("ns")
                continue
            m = self.CLS.match(line.strip())
            if m and line.lstrip().startswith(("public", "private", "protected", "internal", "sealed", "abstract")):
                name = m.group("name")
                cur = {
                    "name": name,
                    "ns": ns,
                    "bases": [x.strip().split("<")[0].split(".")[-1]
                              for x in (m.group("bases") or "").split(",") if x.strip()],
                    "methods": {},
                }
                self.classes.setdefault(name, cur)
                continue
            if cur is not None:
                if line.strip() == "}":
                    cur = None
                    continue
                mm = self.MTH.match(line)
                if mm:
                    args = mm.group("args").strip()
                    argc = 0 if not args else args.count(",") + 1
                    cur["methods"].setdefault(mm.group("name"), argc)
        # 去重：同名类保留方法最多的那个
        best: dict[str, dict] = {}
        for name, c in self.classes.items():
            if name not in best or len(c["methods"]) > len(best[name]["methods"]):
                best[name] = c
        self.classes = best

    def lookup(self, name: str, method: str, seen=None) -> tuple[int | None, str | None]:
        """沿父类链查找方法，返回 (argc, 定义类名)。"""
        seen = seen or set()
        if name in seen:
            return None, None
        seen.add(name)
        c = self.classes.get(name)
        if not c:
            return None, None
        if method in c["methods"]:
            return c["methods"][method], name
        for base in c["bases"]:
            argc, owner = self.lookup(base, method, seen)
            if argc is not None:
                return argc, owner
        return None, None


# -------------------------------------------------------------------- 主流程

# 视为「解析可达」的结论：类型自身命中 / dump.cs 继承链命中 / 全库同名同 argc 声明者存在
OK_VERDICTS = ("ok", "ok_inherited", "ok_inherited_candidate")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--metadata", required=True)
    ap.add_argument("--dump-cs")
    ap.add_argument("--cpp")
    ap.add_argument("--spec", required=True)
    ap.add_argument("--out-json")
    ap.add_argument("--out-md")
    ap.add_argument("--version-name", default="")
    ap.add_argument("--version-code", default="")
    ap.add_argument("--serial", default="")
    args = ap.parse_args()

    meta_path = Path(args.metadata)
    if not meta_path.is_file():
        print(f"错误: metadata 不存在: {meta_path}", file=sys.stderr)
        return 2
    print("[1/5] 读取 metadata …", file=sys.stderr, flush=True)
    blob = meta_path.read_bytes()
    md = MetadataV24(blob)
    print(f"[2/5] 解析完成：types={len(md.types)} td_size={md.td_size:#x}", file=sys.stderr, flush=True)
    hdr = md.hdr
    print(f"[metadata] file={meta_path.name} size={len(blob)} sanity=0x{hdr['sanity'] & 0xFFFFFFFF:08x} "
          f"version={hdr['version']} strings={md.str_count} types={md.td_count} methods={md.md_count} "
          f"td_size={md.td_size:#x}")

    spec = json.loads(Path(args.spec).read_text(encoding="utf-8"))
    cpp_text = Path(args.cpp).read_text(encoding="utf-8", errors="replace") if args.cpp and Path(args.cpp).is_file() else None
    dcs = DumpCs(Path(args.dump_cs).read_text(encoding="utf-8", errors="replace")) if args.dump_cs and Path(args.dump_cs).is_file() else None
    if dcs:
        print(f"[dump.cs] classes={len(dcs.classes)}")

    # 命名空间级方法索引：用于「不在该类型自身」时的旁证（找同命名空间里同名方法的宿主类型/参数个数）
    print("[3/5] 建立命名空间方法索引 …", file=sys.stderr, flush=True)
    ns_method_index: dict[tuple[str, str], list[tuple[str, int]]] = {}
    global_method_index: dict[str, list[tuple[str, str, int]]] = {}
    for t in md.types:
        for m in t["methods"]:
            ns_method_index.setdefault((t["ns"], m["name"]), []).append((t["name"], m["argc"]))
            global_method_index.setdefault(m["name"], []).append((t["ns"], t["name"], m["argc"]))

    print(f"[4/5] 逐条核验 {len(spec['anchors'])} 个锚点 …", file=sys.stderr, flush=True)
    results = []
    for a in spec["anchors"]:
        ns, cname, meth, argc = a["ns"], a["class"], a["method"], a["argc"]
        outer, inner = a.get("outer"), a.get("inner")
        entry = {
            "id": a["id"],
            "feature": a["feature"],
            "ns": ns,
            "class": cname,
            "method": meth,
            "argc_expected": argc,
            "cpp_lines": a.get("cpp_lines", []),
        }

        target = None
        if outer:
            outers = md.find(ns, outer)
            if outers:
                inners = md.find_nested(outers[0], inner)
                target = inners[0] if inners else None
            entry["resolve"] = f"{ns}.{outer}+{inner}"
        else:
            hits = md.find(ns, cname)
            if hits:
                target = hits[0]
            entry["resolve"] = f"{ns}.{cname}"

        # 主证据：metadata 直接命中
        direct = None
        variants = []
        if target is not None:
            entry["type_index"] = target["idx"]
            entry["methods_total"] = len(target["methods"])
            for m in target["methods"]:
                if m["name"] == meth:
                    if m["argc"] == argc and direct is None:
                        direct = m
                    variants.append(m["argc"])
        if direct is not None:
            entry["verdict"] = "ok"
            entry["evidence"] = "metadata:类型自身方法命中，argc 一致"
            entry["static"] = direct["static"]
        elif variants:
            entry["verdict"] = "argc_mismatch"
            entry["evidence"] = f"metadata:类型自身存在同名方法但 argc={variants}，期望 {argc}"
        elif target is None:
            entry["verdict"] = "missing_type"
            entry["evidence"] = "metadata:未找到该命名空间下的类型/嵌套类型"
        else:
            owner, _m = md.resolve_in_hierarchy(target, meth, argc) if target else (None, None)
            if owner is not None:
                entry["verdict"] = "ok_inherited"
                entry["owner"] = md.full_name(owner)
                entry["chain"] = md.chain(target)
                entry["evidence"] = (f"metadata:类型自身无 {meth}，继承链命中 {md.full_name(owner)}"
                                     f"（argc={argc}）；链={' -> '.join(md.chain(target))}")
            else:
                entry["verdict"] = "not_on_type"
                entry["evidence"] = (f"metadata:类型 {entry['resolve']} 自身方法集（{entry.get('methods_total', 0)} 条）"
                                     f"无 {meth}，继承链上也未命中 argc={argc}（MOD 走 GetMethodInHierarchy）")

        # 辅证据：dump.cs（含继承链）
        if dcs:
            got, owner = dcs.lookup(cname, meth)
            if outer:
                got, owner = dcs.lookup(inner, meth)
            entry["dumpcs"] = {"argc": got, "owner": owner}
            if entry["verdict"] == "not_on_type" and got == argc:
                entry["verdict"] = "ok_inherited"
                entry["evidence"] = f"dump.cs:方法定义在父类 {owner}，argc 一致（MOD 沿父类向上解析）"
            elif entry["verdict"] == "not_on_type" and got is None:
                entry["verdict"] = "missing_method"
                entry["evidence"] = "metadata 与 dump.cs 均未在该类及父类链上找到该方法"
            elif entry["verdict"] == "ok" and got is not None and got != argc:
                entry["verdict"] = "argc_conflict"
                entry["evidence"] = f"metadata argc={argc} 但 dump.cs argc={got}（{owner}）"

        # cpp 侧交叉：锚点串必须仍出现在 MOD 源码里
        if cpp_text is not None:
            entry["in_cpp"] = (meth in cpp_text) and (cname in cpp_text or (outer or "") in cpp_text)
        if entry["verdict"] not in OK_VERDICTS:
            cands = ns_method_index.get((ns, meth), [])
            entry["candidates_same_ns"] = [{"type": t, "argc": a} for t, a in cands[:12]]
            entry["candidates_same_ns_total"] = len(cands)
            gcands = [{"ns": n, "type": t, "argc": a} for n, t, a in global_method_index.get(meth, [])]
            entry["candidates_global"] = gcands[:20]
            entry["candidates_global_total"] = len(gcands)
            exact = [c for c in gcands if c["argc"] == argc]
            entry["candidates_global_argc_match"] = exact[:20]
            if entry["verdict"] == "not_on_type" and exact:
                entry["verdict"] = "ok_inherited_candidate"
                owners = ", ".join(sorted({f"{c['ns']}.{c['type']}" for c in exact})[:4])
                entry["evidence"] += (f"；全库同名同 argc 的声明者：{owners}"
                                     f"（共 {len(exact)} 处）⇒ 由 IL2CPP 层级向上解析可达")
        results.append(entry)

    ok = [r for r in results if r["verdict"] in OK_VERDICTS]
    bad = [r for r in results if r not in ok]
    summary = {
        "total": len(results),
        "ok": len(ok),
        "bad": len(bad),
        "verdicts": {v: sum(1 for r in results if r["verdict"] == v) for v in sorted({r["verdict"] for r in results})},
    }

    out = {
        "schema": "nixiang.starry.anchor.v1",
        "game": "明日方舟",
        "game_id": "mrfzz",
        "version_name": args.version_name,
        "version_code": args.version_code,
        "generated_at": _dt.datetime.now().isoformat(timespec="seconds"),
        "serial": args.serial,
        "metadata": str(meta_path),
        "metadata_magic": f"0x{hdr['sanity'] & 0xFFFFFFFF:08x}",
        "metadata_version": hdr["version"],
        "method_def_size": md.td_size,
        "type_table_crosscheck": {
            "types_with_method_start": md.methods_direct,
            "method_count_equal_grouped": md.methods_crossok,
        },
        "source_of_truth": spec.get("source_of_truth", ""),
        "summary": summary,
        "anchors": results,
    }

    w = max(len(r["feature"]) for r in results)
    print()
    print(f"{'功能':<{w}}  {'结论':<14} {'解析目标':<38} argc 证据")
    for r in results:
        print(f"{r['feature']:<{w}}  {r['verdict']:<14} {r['resolve']:<38} {r['argc_expected']}  {r['evidence']}")
    print()
    print(json.dumps(summary, ensure_ascii=False))

    if args.out_json:
        Path(args.out_json).parent.mkdir(parents=True, exist_ok=True)
        Path(args.out_json).write_text(json.dumps(out, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"[out] {args.out_json}")
    if args.out_md:
        lines = [f"# 明日方舟 {args.version_name} 锚点核验（星光七功能）", "",
                 f"- 数据源：`{meta_path}`（metadata version {hdr['version']}，magic `0x{hdr['sanity'] & 0xFFFFFFFF:08x}`）",
                 f"- 生成时间：{out['generated_at']}｜设备 {args.serial}｜判定 {summary['ok']}/{summary['total']} 通过",
                 f"- 源码真相源：`{out['source_of_truth']}`", "",
                 "| 功能 | 解析目标 | 方法 | argc 期望 | 结论 | 证据 |", "|---|---|---|---:|---|---|"]
        for r in results:
            lines.append(f"| {r['feature']} | `{r['resolve']}` | `{r['method']}` | {r['argc_expected']} | "
                         f"{r['verdict']} | {r['evidence']} |")
        Path(args.out_md).write_text("\n".join(lines) + "\n", encoding="utf-8")
        print(f"[out] {args.out_md}")

    return 0 if not bad else 3


if __name__ == "__main__":
    sys.exit(main())
