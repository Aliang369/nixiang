"""Step 12: ad / rewarded-video related types & methods + #US user-string scan."""
# -*- coding: utf-8 -*-
import os, sys, re, io
sys.path.insert(0, r'E:\nixiang\06_dumped\冒险与征途\aot')
from _mdread import Meta

AOT = r'E:\nixiang\06_dumped\冒险与征途\aot'
m = Meta(os.path.join(AOT, 'Assembly-CSharp.dll'))
n_td = len(m.tbl['TypeDef']); n_md = len(m.tbl['MethodDef'])
mlist = [r[5] for r in m.tbl['TypeDef']]
classes = []
for td in range(1, n_td + 1):
    classes.append((m.td_fullname(td), mlist[td - 1], mlist[td] if td < n_td else n_md + 1))

def sig_of(rid):
    r = m.tbl['MethodDef'][rid - 1]
    blob, _ = m.blob(r[4])
    try:
        ret, params, gen = m.sig_blob(blob)
    except Exception as e:
        ret, params, gen = ('<ERR %s>' % e, [], [])
    return m.s(r[3]), ret, params, gen

def sigs(nm, ret, params, gen):
    g = '<%s>' % ','.join(gen) if gen else ''
    return '%s%s(%s) -> %s' % (nm, g, ', '.join(params), ret)

KW_STRONG = ['AdReward', 'RewardAd', 'RewardVideo', 'VideoAd', 'AdManager', 'AdHelper',
             'NativeAd', 'Interstitial', 'Banner', 'Pangle', 'TTAd', 'CSJ', 'IronSource',
             'TopOn', 'AdSDK', 'AdsSDK', 'AppLovin', 'Mintegral', 'Gromore', 'ADN',
             'AdUnit', 'AdPlace', 'AdCallback', 'AdStatus', 'AdInfo', 'AdData', 'AdConfig',
             'ShowAd', 'LoadAd', 'PlayAd', 'AdShow', 'AdLoad', 'AdClick', 'AdOpen',
             'Rewarded', 'Advertising', 'Advertise', 'Advert']
CN = ['广告', '激励', '观看']
AD_BLACK = ('add', 'addr', 'adap', 'adj', 'adia', 'admi', 'adul')

def tokens(name):
    out = []
    for s in re.split(r'[^0-9A-Za-z\u4e00-\u9fff]+', name):
        out += re.findall(r'[A-Z]+(?![a-z])|[A-Z][a-z0-9]*|[a-z0-9]+|[\u4e00-\u9fff]+', s)
    return out

def _subseq(seq, sub):
    n = len(sub)
    if n == 0 or n > len(seq):
        return False
    for i in range(len(seq) - n + 1):
        if seq[i:i + n] == sub:
            return True
    return False

def hits_of(name):
    """只在词元边界上匹配，杜绝 LoadManager 命中 AdManager、Adventure 命中 Advert 这类误报"""
    tk = [t.lower() for t in tokens(name)]
    tks = set(tk)
    hits = []
    for k in KW_STRONG:
        kl = k.lower(); kt = [t.lower() for t in tokens(k)]
        if kl in tks or (len(kt) > 1 and _subseq(tk, kt)):
            if k not in hits:
                hits.append(k)
        elif len(kl) >= 5 and any(t.startswith(kl) for t in tk):
            if k not in hits:
                hits.append(k)
    for c in CN:
        if c in name and c not in hits:
            hits.append(c)
    if 'reward' in tks and 'Reward' not in hits:
        hits.append('Reward')
    if 'video' in tks and 'Video' not in hits:
        hits.append('Video')
    if 'sdk' in tks and 'SDK' not in hits:
        hits.append('SDK')
    return hits

def score(name):
    """3=强相关(广告词元) 2=中(Reward/SDK/Video 词元) 1=弱(子串) 0=无关"""
    h = hits_of(name)
    if any(x in KW_STRONG or x in CN for x in h):
        return 3
    tk = [t.lower() for t in tokens(name)]
    tks = set(tk)
    if 'reward' in tks or 'sdk' in tks or 'video' in tks:
        return 2
    low = name.lower()
    if 'reward' in low or 'video' in low or 'sdk' in low or 'max' in tks:
        return 1
    return 0

def why(name):
    h = hits_of(name)
    if h:
        return ','.join(h)
    tk = [t.lower() for t in tokens(name)]
    for k in ('reward', 'video', 'sdk', 'max'):
        if k in tk:
            return k
    low = name.lower()
    for k in ('reward', 'video', 'sdk'):
        if k in low:
            return 'loose:' + k
    return '-'

# ---------------- scan types + methods ----------------
t3, t2, t1 = [], [], []
m3, m2, m1 = [], [], []
for full, start, end in classes:
    sc = score(full)
    if sc == 3: t3.append(full)
    elif sc == 2: t2.append(full)
    elif sc == 1: t1.append(full)
    for rid in range(start, end):
        nm = m.s(m.tbl['MethodDef'][rid - 1][3])
        s = score(nm)
        if s == 3: m3.append((full, rid, nm))
        elif s == 2: m2.append((full, rid, nm))
        elif s == 1: m1.append((full, rid, nm))

# property / field names (bonus, cheap)
props = [(m.s(r[1])) for r in m.tbl.get('Property', [])]
fields = [(m.s(r[1])) for r in m.tbl.get('Field', [])]
p3 = [x for x in props if score(x) == 3]
f3 = [x for x in fields if score(x) == 3]
print('types 3/2/1 =', len(t3), len(t2), len(t1))
print('methods 3/2/1 =', len(m3), len(m2), len(m1))
print('prop3/field3 =', len(p3), len(f3))

o = []
o.append('# hotupdate_ad_hits.txt — 广告 / 激励视频 相关类型与方法（Assembly-CSharp.dll，HybridCLR 热更）')
o.append('# 匹配规则：标识符按 camelCase/下划线切成词元，词元级命中 Ad/Ads/AdReward/Rewarded/Video/Pangle/')
o.append('#           TTAd/CSJ/IronSource/TopOn/Banner/Interstitial/NativeAd 等，或含中文 广告/激励/观看 → 强相关(3)')
o.append('#           Reward/SDK/Video 词元级 → 中相关(2)；子串命中(Reward/Video/SDK/Max) → 弱相关(1)')
o.append('# 排序：强 → 中 → 弱；同行给出命中关键词 why')
o.append('')

def dump_methods(fh, tlist, title, limit=None):
    fh.append('#' * 78); fh.append('## ' + title); fh.append('#' * 78)
    n = 0
    for full in tlist:
        fh.append('--- 类型 %s   [命中: %s]' % (full, why(full)))
        st = en = None
        for f, s, e in classes:
            if f == full:
                st, en = s, e; break
        if st is None:
            continue
        for rid in range(st, en):
            if limit and n >= limit:
                fh.append('  ... (截断)'); return
            nm, ret, params, gen = sig_of(rid)
            w = why(nm)
            mark = '  <<< ' if score(nm) >= 2 else '      '
            fh.append('%s%s | token=0x%08X%s' % (mark, sigs(nm, ret, params, gen), 0x06000000 | rid,
                                                 ('  [method-hit:%s]' % w) if w != '-' else ''))
            n += 1

dump_methods(o, t3, 'A. 强相关(score=3) 类型 — 列出该类型全部方法')
o.append('')
o.append('#' * 78)
o.append('## B. 强相关(score=3) 方法（类::方法）')
o.append('#' * 78)
for full, rid, nm in m3:
    n2, ret, params, gen = sig_of(rid)
    o.append('  %s :: %s | token=0x%08X | 命中=%s' % (full, sigs(n2, ret, params, gen), 0x06000000 | rid, why(nm)))
o.append('')
o.append('#' * 78)
o.append('## C. 中相关(score=2) Reward/SDK/Video 词元级 — 类型与方法')
o.append('#' * 78)
o.append('### C1. 类型（共 %d 个，只列名单 + 命中词）' % len(t2))
for full in t2:
    o.append('  %s   [命中: %s]' % (full, why(full)))
o.append('### C2. 方法（共 %d 个；★ = 类名也相关）' % len(m2))
for full, rid, nm in m2:
    star = '★' if score(full) >= 2 else ' '
    o.append(' %s%s :: %s | token=0x%08X' % (star, full, nm, 0x06000000 | rid))
o.append('')
o.append('#' * 78)
o.append('## D. 弱相关/待排除(score=1) — 前 400 条方法（含 Max 子串，多为误报，仅备查）')
o.append('#' * 78)
o.append('# 类型共 %d 个' % len(t1))
for full, rid, nm in m1[:400]:
    o.append('  %s :: %s | token=0x%08X | 命中=%s' % (full, nm, 0x06000000 | rid, why(nm)))
o.append('')
o.append('#' * 78)
o.append('## E. 属性/字段名 中含强相关词（score=3）')
o.append('#' * 78)
o.append('### E1. Property（%d 个）' % len(p3))
for x in sorted(set(p3)):
    o.append('  P ' + x)
o.append('### E2. Field（%d 个）' % len(f3))
for x in sorted(set(f3)):
    o.append('  F ' + x)
o.append('')

# ---------------- F. #US user-string scan ----------------
off, size = m.streams['#US']
d = m._file
US_KEYS = [('广告', False), ('激励', False), ('观看', False), ('reward', True), ('ad_', True), ('double', True)]
buckets = {k: [] for k, _ in US_KEYS}
seen = {k: set() for k, _ in US_KEYS}
p = off
total = 0
while p < off + size:
    try:
        n, q = m.cint(p, d)
    except Exception:
        break
    if n == 0:
        p += 1
        continue
    blob = d[q:q + n]
    if len(blob) < n:
        break
    # #US blob: 无前置 flag 字节；长度为奇数时最后一个字节是「特殊字符」flag
    body = blob[:-1] if (n & 1) else blob
    try:
        s = body.decode('utf-16-le', 'replace')
    except Exception:
        s = ''
    total += 1
    low = s.lower()
    for k, ci in US_KEYS:
        if (k.lower() in low) if ci else (k in s):
            if s not in seen[k] and len(buckets[k]) < 60:
                seen[k].add(s)
                buckets[k].append((q, s))
    p = q + n
print('#US entries scanned:', total)

o.append('#' * 78)
o.append('## F. #US（用户字符串堆）扫描 — 每个关键词最多 60 条（堆内偏移 + UTF-16LE 解码原文）')
o.append('#' * 78)
o.append('# 堆范围: #US off=0x%X size=%d, 解析出字符串 %d 条' % (off, size, total))
for k, ci in US_KEYS:
    o.append('### F[%s]  命中 %d 条' % (k, len(buckets[k])))
    for q, s in buckets[k]:
        s1 = s.replace('\r', '\\r').replace('\n', '\\n')
        if len(s1) > 200:
            s1 = s1[:200] + '…'
        o.append('  0x%X  %s' % (q, s1))
    o.append('')
# ---------------- G. 补充扫描（判定「看广告得奖励」的上下文串） ----------------
EXTRA = [('视频', False), ('双倍', False), ('免广告', False), ('月卡', False),
         ('watch_ad', True), ('watchad', True), ('videoad', True), ('rewardvideo', True)]
xb = {k: [] for k, _ in EXTRA}
p = off
while p < off + size:
    try:
        n, q = m.cint(p, d)
    except Exception:
        break
    if n == 0:
        p += 1
        continue
    blob = d[q:q + n]
    if len(blob) < n:
        break
    body = blob[:-1] if (n & 1) else blob
    try:
        s = body.decode('utf-16-le', 'replace')
    except Exception:
        s = ''
    low = s.lower()
    for k, ci in EXTRA:
        if ((k.lower() in low) if ci else (k in s)) and len(xb[k]) < 60:
            xb[k].append((q, s))
    p = q + n

o.append('#' * 78)
o.append('## G. 补充扫描（超出台面关键词，用于判定「看广告得奖励」是否存在）')
o.append('#' * 78)
for k, ci in EXTRA:
    o.append('### G[%s]  命中 %d 条' % (k, len(xb[k])))
    for q, s in xb[k]:
        s1 = s.replace('\r', '\\r').replace('\n', '\\n')
        if len(s1) > 200:
            s1 = s1[:200] + '…'
        o.append('  0x%X  %s' % (q, s1))
    o.append('')
open(os.path.join(AOT, 'hotupdate_ad_hits.txt'), 'w', encoding='utf-8').write('\n'.join(o))
print('ad_hits lines:', len(o))
