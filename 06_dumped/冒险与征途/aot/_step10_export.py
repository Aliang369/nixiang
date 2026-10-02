"""Step 10: build all deliverables from the metadata."""
# -*- coding: utf-8 -*-
import os, sys, re, hashlib, struct
sys.path.insert(0, r'E:\nixiang\06_dumped\冒险与征途\aot')
from _mdread import Meta, TABLES

OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
ASM = 'Assembly-CSharp'
m = Meta(os.path.join(OUT, 'Assembly-CSharp.dll'))
print('assembly:', m.asm_name, 'types:', len(m.tbl['TypeDef']), 'methods:', len(m.tbl['MethodDef']))

n_td = len(m.tbl['TypeDef'])
n_md = len(m.tbl['MethodDef'])
n_pd = len(m.tbl['Param'])

# TypeDef -> [method rid range]
mlist = [r[5] for r in m.tbl['TypeDef']]          # MethodList (rid):
# TypeDef cols = Flags(0) Name(1) Namespace(2) Extends(3) FieldList(4) MethodList(5)
plist = [r[5] for r in m.tbl['MethodDef']]        # ParamList (rid)

# params: rid -> sequence/name
param_rows = m.tbl['Param']

# method signature cache
def method_info(rid):
    r = m.tbl['MethodDef'][rid - 1]
    name = m.s(r[3])
    blob, _ = m.blob(r[4])
    try:
        ret, params, gen = m.sig_blob(blob)
    except Exception as e:
        ret, params, gen = ('<ERR %s>' % e, [], [])
    return name, ret, params, gen

_parse_sig = None

def _parse_sig(blob):
    # wrap the blob in a buffer by temporarily reusing parse_type via a shim object
    class S:
        pass
    saved = m.data
    dummy = struct.pack('<I', 0)
    m.data = blob
    try:
        cc = m.data[0]
        p = 1
        gen = []
        if cc & 0x10:
            n, p = m.cint(p)
            gen = ['!%d' % i for i in range(n)]
        n, p = m.cint(p)
        ret, p = m.parse_type(p)
        params = []
        for _ in range(n):
            if m.data[p] == 0x41:
                p += 1
            t, p = m.parse_type(p); params.append(t)
    finally:
        m.data = saved
    return ret, params, gen

# ---- build class -> methods ----
classes = []
for td in range(1, n_td + 1):
    start = mlist[td - 1]
    end = mlist[td] if td < n_td else n_md + 1
    classes.append((m.td_fullname(td), start, end))

print('classes:', len(classes))

# ---------- 1) hotupdate_methods.txt ----------
lines = []
sig_cache = {}
for full, start, end in classes:
    if end - start == 0:
        continue
    lines.append('=== %s  [%d methods]' % (full, end - start))
    for rid in range(start, end):
        if rid in sig_cache:
            nm, ret, params, gen = sig_cache[rid]
        else:
            nm, ret, params, gen = method_info(rid)
            sig_cache[rid] = (nm, ret, params, gen)
        g = '<%s>' % ','.join(gen) if gen else ''
        lines.append('%s :: %s%s(%s) -> %s' % (ASM, nm, g, ', '.join(params), ret))
p1 = os.path.join(OUT, 'hotupdate_methods.txt')
with open(p1, 'w', encoding='utf-8') as f:
    f.write('# Assembly-CSharp.dll 全量 类::方法 清单\n')
    f.write('# 每行格式: 程序集 :: 方法名(参数类型...) -> 返回类型\n')
    f.write('# 类分组头: "=== 类全名  [N methods]"\n')
    f.write('# TypeDef=%d MethodDef=%d\n\n' % (n_td, n_md))
    f.write('\n'.join(lines))
print('wrote', p1, len(lines), 'lines')

# ---------- 2) hotupdate_hits.txt ----------
TARGETS = ['Damage', 'Character', 'DoAttack', 'StarsCollider', 'AddProjectile', 'ShowWin', 'MW_GameNotice']
BATTLE = re.compile(r'Character|Player|Battle|Enemy|Monster|Level|Projectile|Hero|Unit|Skill|Fight|Combat|Weapon|Bullet|Scene', re.I)
hit_out = []
hit_out.append('# 目标方法名精确匹配（区分大小写）结果')
hit_out.append('# 程序集: %s   文件: Assembly-CSharp.dll' % ASM)
hit_out.append('# 格式: 类全名 :: 方法名(参数) -> 返回  |  token=0x06XXXXXX | 程序集 | 战斗逻辑标记')
hit_out.append('')
summary = {}
for t in TARGETS:
    found = []
    for full, start, end in classes:
        for rid in range(start, end):
            if rid in sig_cache:
                nm, ret, params, gen = sig_cache[rid]
            else:
                nm, ret, params, gen = method_info(rid)
                sig_cache[rid] = (nm, ret, params, gen)
            if nm == t:
                found.append((full, rid, nm, ret, params, gen))
    summary[t] = found
    hit_out.append('#' * 70)
    hit_out.append('## %s  命中 %d 处' % (t, len(found)))
    hit_out.append('#' * 70)
    if not found:
        hit_out.append('  (无精确命中)')
    for full, rid, nm, ret, params, gen in found:
        g = '<%s>' % ','.join(gen) if gen else ''
        flag = 'BATTLE-LIKE' if BATTLE.search(full) else ('GAME-REWARD' if re.search(r'Reward|Gift', full, re.I) else '-')
        hit_out.append('  %s :: %s%s(%s) -> %s' % (full, nm, g, ', '.join(params), ret))
        hit_out.append('      token=0x%08X (rid=%d) | 程序集=%s | 类名特征=%s' % (0x06000000 | rid, rid, ASM, flag))
    hit_out.append('')
p2 = os.path.join(OUT, 'hotupdate_hits.txt')
open(p2, 'w', encoding='utf-8').write('\n'.join(hit_out))
print('wrote', p2)

for t in TARGETS:
    print('  %-14s hits=%d  %s' % (t, len(summary[t]), [x[0] for x in summary[t]][:6]))
