#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""无悔华夏 RVA 自动重标（P0-② 的解析/生成内核，由 whrhx_rebaseline.sh 调用）

为什么要有它：本轮实测装钩分布是 **按名 46 : 按 RVA 2** —— 手写 RVA 表在乐变热更后
基本全漂移（`rva miss` 221 条、同一符号反复重试 72 次），每次都要人肉对表。
这里把「dump.cs 的方法地址 → 本机运行时 so 的首指令」这条链自动化：
  * 站点来源 = WhrhxMod.cpp 里所有 `HookRvaOrIl2Cpp("tag","ns","cls","method",argcs,N,RVA_X,...)`
    （类名/方法名/参数个数是真源，比手写注释可靠）；
  * 地址来源 = dump.cs 的 `// RVA: 0x… Offset: 0x…` + 紧随其后的方法签名；
  * 序言来源 = 目标 so 在 RVA 处的首 4 字节小端（本 so p_offset==p_vaddr==0 ⇒ 文件偏移即 RVA）。

三种模式：
  --emit <header>   生成 `WhrhxRvaGen.h`（WHRHX_GEN_RVA_* + kExpectGen[] + 来源 sha256）
  --check           只报告：站点解析率 / 无法解析的站点 / 与 WhrhxMod.cpp 枚举值的漂移清单
  --apply           把 --check 认定的漂移值写回 WhrhxMod.cpp 的 RVA_* 枚举（会先备份 .bak）

用法（一般不用直接调，走 whrhx_rebaseline.sh）：
  python3 whrhx_rebaseline.py --dump <dump.cs> --so <libil2cpp.so> --cpp <WhrhxMod.cpp> \
          [--emit <WhrhxRvaGen.h>] [--check] [--apply]

退出码：0 全解析；1 有未解析站点（--emit/--check）；2 参数/文件错误。
"""
import argparse
import hashlib
import os
import re
import sys

AP = argparse.ArgumentParser()
AP.add_argument('--dump', required=True)
AP.add_argument('--so', required=True)
AP.add_argument('--cpp', required=True)
AP.add_argument('--expect', default='',
                help='手写 kExpect 的迁移存档（常量名/声明序言/tag）；默认 04_scripts/无悔华夏/whrhx_expect_legacy.txt')
AP.add_argument('--emit', default='')
AP.add_argument('--check', action='store_true')
AP.add_argument('--apply', action='store_true')
AP.add_argument('--max-report', type=int, default=40)
A = AP.parse_args()

for p in (A.dump, A.so, A.cpp):
    if not os.path.isfile(p):
        print('ERROR: missing file: %s' % p)
        sys.exit(2)


def sha256(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(1 << 20), b''):
            h.update(chunk)
    return h.hexdigest()


# ---------- 1) dump.cs → (cls, method, argc) -> rva ----------
RVA_RE = re.compile(r'^\s*// RVA: (0x[0-9a-fA-F]+) Offset:')
NS_RE = re.compile(r'^\s*// Namespace: (.*)$')
# 注意：dump.cs 里**嵌套类**写成 `public class CountryData.CountryArmyData`（点号全名），
# 而 il2cpp 按名查类用的是**短名**（`CountryArmyData`，命名空间为空），cpp 的 HookRvaOrIl2Cpp
# 站点也一律用短名 ⇒ 必须保留点号全名后再取最后一段作为索引键（否则嵌套类里的方法全落空）。
CLASS_RE = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:public|private|internal|protected)?\s*'
                      r'(?:sealed\s+|abstract\s+|static\s+|unsafe\s+|partial\s+)*class\s+'
                      r'([A-Za-z_][A-Za-z0-9_`]*(?:\.[A-Za-z_][A-Za-z0-9_`]*)*)')
# dump.cs 的签名行形如：`\tpublic long GetMaxCanSendNum(int targetCity, bool b = False) { }`
# 注意结尾是 `{ }`（空体），早期版本写成 `) {` ⇒ 必须允许 `{?}?` 两种收尾。
METHOD_RE = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:public|private|protected|internal|static|virtual|'
                       r'override|sealed|abstract|extern|unsafe|new|readonly)\s+'
                       r'[^=;{}()]*?([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)\s*\{?\s*\}?\s*$')


def count_params(argstr):
    argstr = argstr.strip()
    if not argstr:
        return 0
    depth = 0
    n = 1
    for ch in argstr:
        if ch in '<[(':
            depth += 1
        elif ch in '>])':
            depth -= 1
        elif ch == ',' and depth == 0:
            n += 1
    return n


methods = {}          # (短cls名, method, argc) -> rva
methods_full = {}     # (点号全名, method, argc) -> rva（兜底）
by_name = {}          # (method, argc) -> set(短cls名)
cur_cls = ''
cur_full_cls = ''
pend_rva = None
ns_last = ''
with open(A.dump, encoding='utf-8', errors='replace') as f:
    for line in f:
        if NS_RE.match(line):          # dump.cs 里是 tab 缩进 ⇒ 不能用 startswith
            ns_last = line.split(':', 1)[1].strip()
            continue
        if line.lstrip().startswith('// RVA:'):
            m = RVA_RE.match(line)
            pend_rva = int(m.group(1), 16) if m else None
            continue
        if line.lstrip().startswith('//'):
            continue
        mc = CLASS_RE.match(line)
        if mc:
            cur_full = mc.group(1)          # 如 CountryData.CountryArmyData
            cur_cls = cur_full.split('.')[-1]   # 如 CountryArmyData（il2cpp 按名口径）
            cur_full_cls = cur_full
            pend_rva = None
            continue
        if pend_rva is not None:
            mm = METHOD_RE.match(line)
            if mm:
                name, args = mm.group(1), mm.group(2)
                argc = count_params(args)
                methods.setdefault((cur_cls, name, argc), pend_rva)
                if cur_full_cls != cur_cls:
                    methods_full.setdefault((cur_full_cls, name, argc), pend_rva)
                by_name.setdefault((name, argc), set()).add(cur_cls)
            pend_rva = None

print('[dump] 方法索引 %d 条（类=%d 个命名空间末值=%r）' % (len(methods), len({k[0] for k in methods}), ns_last))

# ---------- 2) 站点：HookRvaOrIl2Cpp 调用 + argc 数组 ----------
cpp = open(A.cpp, encoding='utf-8').read()
# 解析用副本：把 `// …` 注释（含行尾注释）替换成等长空格 —— 这样「参数列表中间插注释」的站点
# 仍能被 SITE_RE 吞下，且**字符偏移不变**（arr_at 依赖偏移就近查找）。
# 反例（真实踩过）：在 HookRvaOrIl2Cpp(...) 的参数中间插了一段 ★ 注释，站点数立刻 42→41，
# 那一条钩被静默漏掉且报告不报错。
cpp_scan = re.sub(r'//[^\n]*', lambda m: ' ' * len(m.group(0)), cpp)

# argc 数组多为**函数局部** `static const int kUpdateArgcs[] = {...}`，不同作用域会重名
# （例如 kUpdateArgcs 在 Battle 臂里是 {0}、在 Policy 臂里是 {3}）⇒ 不能按「最后一次定义」
# 全局覆盖，必须按**就近在前**解析：站点只认自己之前最近的那个同名定义。
arr_defs = []          # (offset, name, vals)
for m in re.finditer(r'static const int (\w+)\[\]\s*=\s*\{([^}]*)\}', cpp_scan, re.S):
    vals = [int(x) for x in re.findall(r'\d+', m.group(2))]
    arr_defs.append((m.start(), m.group(1), vals))
arr_defs.sort()
arr_names = {n for _, n, _ in arr_defs}


def arr_at(name, pos):
    best = None
    for off, n, vals in arr_defs:
        if n == name and off < pos:
            best = vals          # 列表已排序 ⇒ 最后一个即最近
    return best or []

SITE_RE = re.compile(
    r'HookRvaOrIl2Cpp\(\s*"([^"]+)"\s*,\s*"([^"]*)"\s*,\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*'
    r'([A-Za-z_]\w*)\s*,\s*(\d+)\s*,\s*(RVA_[A-Z0-9_]+|0)\s*,', re.S)
sites = []
for m in SITE_RE.finditer(cpp_scan):
    tag, ns, cls, method, arr, nargcs, rva_const = m.groups()
    argcs = arr_at(arr, m.start())
    argcs = argcs[:int(nargcs)] if argcs else []
    sites.append(dict(tag=tag, ns=ns, cls=cls, method=method, arr=arr, n=int(nargcs),
                      rva_const=rva_const, argcs=argcs))

consts = {}
for m in re.finditer(r'\b(RVA_[A-Z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+)', cpp_scan):
    consts.setdefault(m.group(1), int(m.group(2), 16))

print('[site] HookRvaOrIl2Cpp 站点 %d 个（argc 数组 %d 个，RVA 常量 %d 个）'
      % (len(sites), len(arr_names), len(consts)))


def first_insn(path, rva):
    with open(path, 'rb') as f:
        f.seek(rva)
        b = f.read(4)
    return int.from_bytes(b, 'little') if len(b) == 4 else None


resolved = {}     # rva_const -> (rva, insn, tag)
unresolved = []
for s in sites:
    cands = []
    for argc in s['argcs']:
        rva = methods.get((s['cls'], s['method'], argc))
        if rva is not None:
            cands.append((argc, rva))
    if len(cands) != 1:
        ns_cands = []
        for argc in s['argcs']:
            for c in by_name.get((s['method'], argc), ()):
                ns_cands.append((c, argc, methods.get((c, s['method'], argc))))
        unresolved.append((s, cands, ns_cands))
        continue
    argc, rva = cands[0]
    insn = first_insn(A.so, rva)
    if insn is None or insn in (0, 0xFFFFFFFF):
        unresolved.append((s, cands, []))
        continue
    prev = resolved.get(s['rva_const'])
    if prev and prev[0] != rva:
        unresolved.append((s, cands, [('CONFLICT', prev[1], prev[0])]))
        continue
    resolved[s['rva_const']] = (rva, insn, s['tag'])

print('[resolve] 唯一解析 %d / 未解析 %d' % (len(resolved), len(unresolved)))
shown = 0
for s, cands, ns_cands in unresolved:
    if shown >= A.max_report:
        print('   …（其余 %d 条省略）' % (len(unresolved) - shown))
        break
    shown += 1
    hint = ''
    if not cands:
        alt = sorted({('%s.%s' % (c, m)) for c, m in
                      [k[:2] for k in by_name if k[0] == s['method'] and k[1] in s['argcs']]})
        hint = ' 同名候选=%s' % (alt[:4] if alt else '无')
    print('   UNRESOLVED %-26s %s.%s argc=%s%s' % (s['tag'], s['cls'], s['method'], s['argcs'], hint))

# ---------- 2b) 手写 kExpect 行 → 纳入生成表（tag 作为输入，首指令从 so 现读） ----------
# 动机：只按 HookRvaOrIl2Cpp 站点建表的时代，手写 kExpect 里那些「不经站点、直接 HookAtRva
# 用的常量」（RVA_IS_PLAYER_LEB / RVA_POLICY_GET_DATA / RVA_ON_BATTLE_END_LEB …）没有生成
# 条目 ⇒ 它们的序言校验只能落在手写表上，而手写表会随热更漂移（实测 50 条里 21 条失配）。
# 现在把「常量名 + tag」当作输入，首指令一律从本机运行时 so 现读 ⇒ 单一事实来源。
HAND_RE = re.compile(r'\{\s*(RVA_[A-Z0-9_]+)\s*,\s*(0x[0-9A-Fa-f]{8})\s*,\s*"([^"]*)"\s*\}')
hand_rows = [(m.group(1), int(m.group(2), 16), m.group(3)) for m in HAND_RE.finditer(cpp_scan)]
# ★ 手写 kExpect[] 已从 cpp 删除（P0-②）⇒ 映射信息改由外部存档提供；cpp 内嵌表若还在则优先沿用。
EXPECT_TXT = A.expect or os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                      'whrhx_expect_legacy.txt')
if os.path.isfile(EXPECT_TXT):
    txt_rows = []
    for ln in open(EXPECT_TXT, encoding='utf-8'):
        ln = ln.strip()
        if not ln or ln.startswith('#'):
            continue
        parts = ln.split(None, 2)
        if len(parts) < 2 or not parts[0].startswith('RVA_'):
            continue
        try:
            insn = int(parts[1], 16)
        except ValueError:
            continue
        txt_rows.append((parts[0], insn, parts[2] if len(parts) > 2 else parts[0]))
    if txt_rows:
        hand_rows = txt_rows
        print('[expect] %s：%d 行（覆盖 cpp 内嵌表）' % (os.path.basename(EXPECT_TXT), len(txt_rows)))
gen_consts = dict(resolved)          # const -> (rva, insn, tag)，站点派生
hand_undef = []
hand_drift = []
stale_consts = {}     # const -> (rva, tag)：RVA 已漂移（该地址上不是这个函数）⇒ 显式拒用
for cname, declared, tag in hand_rows:
    if cname not in consts:
        hand_undef.append((cname, tag))
        continue
    rva = consts[cname]
    insn = first_insn(A.so, rva)
    if declared != insn:
        hand_drift.append((cname, tag, rva, declared, insn))
    if cname in gen_consts:
        continue                      # 已由站点（dump.cs 实证）覆盖 ⇒ 可信
    if declared == insn:
        gen_consts[cname] = (rva, insn, tag)   # 声明与实测一致 ⇒ 仍然可信，纳入
    else:
        # ★ 不能把「在漂移地址上读到的字节」当成期望序言写进接受表 —— 那等于给错地址盖章。
        #   进拒用表：ResolveRva() 命中即直接放弃该 RVA，退回按名路径。
        stale_consts[cname] = (rva, tag)
print('[hand] kExpect 行 %d 条：常量未定义 %d；声明序言与实测不一致 %d%s'
      % (len(hand_rows), len(hand_undef), len(hand_drift),
         '（这些行已由生成表接管，手写表可删）' if hand_drift else ''))
for cname, tag, rva, declared, insn in hand_drift[:A.max_report]:
    print('   HAND_DRIFT %-30s %-34s RVA=0x%08X 声明=0x%08X 实测=0x%08X'
          % (cname, tag, rva, declared, insn))

# ---------- 3) 与 WhrhxMod.cpp 枚举的漂移 ----------
stale = []
for cname, (rva, insn, tag) in sorted(resolved.items()):
    old = consts.get(cname)
    if old is None:
        stale.append((cname, None, rva, insn, tag))
    elif old != rva:
        stale.append((cname, old, rva, insn, tag))
print('[drift] 与 cpp 枚举不一致 %d 条（含未定义）' % len(stale))
for cname, old, rva, insn, tag in stale[:A.max_report]:
    o = '未定义' if old is None else '0x%08X' % old
    print('   STALE %-34s cpp=%-10s dump=0x%08X insn=0x%08X  %s' % (cname, o, rva, insn, tag))

if A.apply:
    if not stale:
        print('[apply] 无需修改')
    else:
        new_cpp = cpp
        n = 0
        for cname, old, rva, insn, tag in stale:
            if old is None:
                continue
            pat = re.compile(r'(\b%s\s*=\s*)0x[0-9A-Fa-f]+' % re.escape(cname))
            new_cpp, k = pat.subn(lambda m: '%s0x%X' % (m.group(1), rva), new_cpp, count=1)
            n += k
        if n:
            bak = A.cpp + '.bak'
            if not os.path.exists(bak):
                open(bak, 'w', encoding='utf-8').write(cpp)
            open(A.cpp, 'w', encoding='utf-8').write(new_cpp)
            print('[apply] 已重写 %d 个枚举值（备份 %s）' % (n, bak))
        else:
            print('[apply] 没有可重写的枚举值（全部未定义）')

if A.emit:
    guard = 'WHRHX_RVA_GEN_H'
    # ★ 命名口径：宏名按 **RVA_* 常量名**（RVA_ADD_RES_LEB → WHRHX_GEN_RVA_ADD_RES_LEB），
    #   不按站点 tag —— tag 会重复（AddRes 同时挂在 RVA_KB_ADD_RES / RVA_ADD_RES_LEB 上），
    #   早期按 tag 命名会生成同名 `#define`（ADDRES 两次）且丢失常量对应关系。
    lines = [
        '/* 自动生成，请勿手改 —— 由 04_scripts/无悔华夏/whrhx_rebaseline.sh 产出（P0-②）。',
        ' * 生成时间(UTC): %s' % __import__('datetime').datetime.now(
            __import__('datetime').timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
        ' * 来源 dump.cs : %s' % os.path.basename(A.dump),
        ' *   sha256      : %s' % sha256(A.dump),
        ' * 来源 runtime so: %s' % os.path.basename(A.so),
        ' *   sha256      : %s' % sha256(A.so),
        ' * 站点解析：唯一 %d / 未解析 %d；枚举漂移 %d；常量条目 %d（含 kExpect 行并入 %d 条）。' % (
            len(resolved), len(unresolved), len(stale), len(gen_consts),
            len(hand_rows)),
        ' *',
        ' * 本 so 的 PT_LOAD#0 是 p_offset=0/p_vaddr=0 ⇒ 文件偏移 == RVA == VA，',
        ' * 所以 kExpectGen[].firstInsn 就是「直接 seek 到该 RVA 读 4 字节小端」的结果。',
        ' * WhrhxMod.cpp 的 ResolveRva() 优先用 kExpectGen 做序言校验（比手写 kExpect 可信），',
        ' * 手写 kExpect 表已删除：它的常量名+tag 被当作**输入**，首指令一律从 so 现读。',
        ' * RVA 在按名之后，属「可选加速」；kExpectStale[] 是明确拒用名单。',
        ' */',
        '#ifndef %s' % guard,
        '#define %s' % guard,
        '',
        'struct WhrhxGenExpect {',
        '    uintptr_t rva;',
        '    uint32_t firstInsn;',
        '    const char* tag;',
        '};',
        '',
    ]
    for cname, (rva, insn, tag) in sorted(gen_consts.items()):
        was = ('0x%X' % consts[cname]) if cname in consts else 'undef'
        same = ' (same)' if cname in consts and consts[cname] == rva else ''
        # ★ 宏名必须加前缀：直接 `#define RVA_X` 会和 WhrhxMod.cpp 里的同名枚举常量撞车
        #   （实测症状：编译报 20 条 `error: expected identifier`，第 46 行起枚举全废）。
        lines.append('#define %-46s 0x%X  /* %s, was %s%s */'
                     % ('WHRHX_GEN_' + cname, rva, tag, was, same))
    # kExpectGen 按 rva 去重（多个常量可指向同一方法：RVA_KB_* 与 *_LEB）。
    by_rva = {}
    for cname, (rva, insn, tag) in sorted(gen_consts.items()):
        by_rva.setdefault(rva, (insn, tag))
    lines += ['', 'static const WhrhxGenExpect kExpectGen[] = {']
    for rva in sorted(by_rva):
        insn, tag = by_rva[rva]
        lines.append('    {0x%X, 0x%08X, "%s"},' % (rva, insn, tag))
    lines += [
        '};',
        'static const size_t kExpectGenCount = sizeof(kExpectGen) / sizeof(kExpectGen[0]);',
        '',
        '// 按 rva 查自动生成的期望序言（命中即代表这份 RVA 对本机运行时 so 有效）。',
        'static inline const WhrhxGenExpect* FindExpectGen(uintptr_t rva) {',
        '    for (size_t i = 0; i < kExpectGenCount; ++i) {',
        '        if (kExpectGen[i].rva == rva) return &kExpectGen[i];',
        '    }',
        '    return nullptr;',
        '}',
        '',
        '// RVA 已漂移的常量（手写表声明的序言与本机运行时 so 该地址的实际字节不符）：',
        '// 这些地址上已经不是目标函数，绝不能当「加速路径」用 —— 显式列出便于 ResolveRva 拒绝，',
        '// 并让日志一眼看出「这条 RVA 表已过期」，而不是静默钩到别的函数上。',
        'static const WhrhxGenExpect kExpectStale[] = {',
    ]
    # 按 rva 去重（RVA_BATTLE_SET_END / RVA_SET_END_LEB 这类同址多常量只留一条，标出常量名）
    _seen_stale = set()
    for cname, (rva, tag) in sorted(stale_consts.items(), key=lambda kv: kv[1][0]):
        if rva in _seen_stale:
            continue
        _seen_stale.add(rva)
        lines.append('    {0x%X, 0x00000000, "%s (%s)"},' % (rva, tag, cname))
    lines += [
        '};',
        'static const size_t kExpectStaleCount = sizeof(kExpectStale) / sizeof(kExpectStale[0]);',
        '',
        'static inline const WhrhxGenExpect* FindStaleGen(uintptr_t rva) {',
        '    for (size_t i = 0; i < kExpectStaleCount; ++i) {',
        '        if (kExpectStale[i].rva == rva) return &kExpectStale[i];',
        '    }',
        '    return nullptr;',
        '}',
        '',
        '#endif  // %s' % guard,
        '',
    ]
    open(A.emit, 'w', encoding='utf-8').write('\n'.join(lines))
    print('[emit] 已写 %s（%d 行，常量 %d 个，kExpectGen %d 条）'
          % (A.emit, len(lines), len(gen_consts), len(by_rva)))

sys.exit(1 if unresolved else 0)
