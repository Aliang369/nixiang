"""Step 11 (final): regenerate hotupdate_methods.txt with fixed sig decode,
build hotupdate_hits.txt (method hits + TypeDef hits), hotupdate_ad_hits.txt
(type/method ad-reward scan + #US user-string scan)."""
# -*- coding: utf-8 -*-
import os, sys, re, hashlib, struct, io
sys.path.insert(0, r'E:\nixiang\06_dumped\冒险与征途\aot')
from _mdread import Meta

AOT = r'E:\nixiang\06_dumped\冒险与征途\aot'
DLL = os.path.join(AOT, 'Assembly-CSharp.dll')
ASM = 'Assembly-CSharp'

m = Meta(DLL)
n_td = len(m.tbl['TypeDef'])
n_md = len(m.tbl['MethodDef'])
mlist = [r[5] for r in m.tbl['TypeDef']]           # MethodList rid
classes = []
for td in range(1, n_td + 1):
    start = mlist[td - 1]
    end = mlist[td] if td < n_td else n_md + 1
    classes.append((m.td_fullname(td), start, end))

TARGETS = ['Damage', 'Character', 'DoAttack', 'StarsCollider',
           'AddProjectile', 'ShowWin', 'MW_GameNotice']

def sig_of(rid):
    r = m.tbl['MethodDef'][rid - 1]
    name = m.s(r[3])
    blob, _ = m.blob(r[4])
    try:
        ret, params, gen = m.sig_blob(blob)
    except Exception as e:
        ret, params, gen = ('<ERR %s>' % e, [], [])
    return name, ret, params, gen

def sig_str(name, ret, params, gen):
    g = '<%s>' % ','.join(gen) if gen else ''
    return '%s%s(%s) -> %s' % (name, g, ', '.join(params), ret)

# ---------------- ad keyword machinery ----------------
KW_STRONG = ['AdReward', 'RewardAd', 'RewardVideo', 'VideoAd', 'AdManager', 'AdHelper',
             'NativeAd', 'Interstitial', 'Banner', 'Pangle', 'TTAd', 'CSJ', 'IronSource',
             'TopOn', 'AdSDK', 'AdsSDK', 'AppLovin', 'Mintegral', 'Gromore', 'ADN',
             'AdUnit', 'AdPlace', 'AdCallback', 'AdStatus', 'AdInfo', 'AdData', 'AdConfig',
             'ShowAd', 'LoadAd', 'PlayAd', 'AdShow', 'AdLoad', 'AdClick', 'AdOpen',
             'Rewarded', 'Adv', 'Advertising', 'Advertise', 'Advert']
KW_MID = ['Reward', 'SDK', 'Video', 'Ad', 'Ads']
CN = ['广告', '激励', '观看']

def tokens(name):
    t = re.split(r'[^0-9A-Za-z\u4e00-\u9fff]+', name)
    out = []
    for s in t:
        out += [x for x in re.findall(r'[A-Z]+(?![a-z])|[A-Z][a-z0-9]*|[a-z0-9]+|[\u4e00-\u9fff]+', s) if x]
    return out

AD_BLACK = ('add', 'addr', 'adap', 'adj', 'adia', 'admi', 'adul')

def _ad_token(tk):
    lt = tk.lower()
    if lt in ('ad', 'ads'):
        return True
    if lt.startswith('ad') and len(lt) >= 4 and not lt.startswith(AD_BLACK):
        return True
    return False

def score(name):
    """3=强相关(Ad/Reward/Video/Pangle... 词元级) 2=中(词元级 Reward/SDK/Video) 1=弱(子串) 0=无关"""
    tk = tokens(name)
    tks = set(x.lower() for x in tk)
    low = name.lower()
    for k in KW_STRONG:
        kl = k.lower()
        if kl in tks:
            return 3
        if len(kl) >= 4 and kl in low:
            return 3
    for c in CN:
        if c in name:
            return 3
    if any(_ad_token(x) for x in tk):
        return 3
    if 'reward' in tks or 'sdk' in tks or 'video' in tks:
        return 2
    if 'reward' in low or 'video' in low or 'sdk' in low:
        return 1
    if 'max' in tks:
        return 1
    return 0

# ---------------- pass 1: methods file + collect hits ----------------
target_hits = {t: [] for t in TARGETS}
ad_types = {}      # full -> score
ad_methods = []    # (score, full, rid, sigline)
cls_methods = {}   # full -> list[rid]

f = io.open(os.path.join(AOT, 'hotupdate_methods.txt'), 'w', encoding='utf-8')
f.write('# Assembly-CSharp.dll 全量 类::方法 清单（HybridCLR 热更程序集）\n')
f.write('# 行格式: 程序集 :: 方法名(参数类型...) -> 返回类型     [泛型方法形如 Name<T>\n')
f.write('# 类分组头: "=== 类全名  [N methods]"\n')
f.write('# TypeDef=%d  MethodDef=%d  程序集名=%s\n\n' % (n_td, n_md, ASM))

lines_written = 0
for full, start, end in classes:
    if end - start == 0:
        cls_methods.setdefault(full, [])
        continue
    sc = score(full)
    if sc:
        ad_types[full] = max(ad_types.get(full, 0), sc)
    f.write('=== %s  [%d methods]\n' % (full, end - start))
    lines_written += 1
    ids = []
    for rid in range(start, end):
        name, ret, params, gen = sig_of(rid)
        s = sig_str(name, ret, params, gen)
        f.write('%s :: %s\n' % (ASM, s))
        lines_written += 1
        ids.append(rid)
        if name in target_hits:
            target_hits[name].append((full, rid, s))
        if sc < 3:
            ms = score(name)
            if ms:
                ad_methods.append((ms, full, rid, s))
    cls_methods[full] = ids
f.close()
print('methods.txt lines:', lines_written)
print('target method hits:', {k: len(v) for k, v in target_hits.items()})
print('ad_types:', len(ad_types), 'ad_methods:', len(ad_methods))

# ---------------- 2) hotupdate_hits.txt ----------------
BATTLE = re.compile(r'Character|Player|Battle|Enemy|Monster|Level|Projectile|Hero|Unit|Skill|Fight|Combat|Weapon|Bullet|Attack|Collider|GameObject', re.I)
REWARD = re.compile(r'Reward|Gift|Award|Prize', re.I)
RELOAD = re.compile(r'Weapon|Gun|Bullet|Fire|Shoot|Launcher|Tower|Turret', re.I)

def tag(fullname):
    if BATTLE.search(fullname):
        return 'BATTLE-LIKE'
    if REWARD.search(fullname):
        return 'GAME-REWARD'
    if RELOAD.search(fullname):
        return 'WEAPON/PROJ'
    return '-'

o = []
o.append('# hotupdate_hits.txt — 目标名字（精确匹配，区分大小写）在 Assembly-CSharp.dll 中的归属')
o.append('# 说明：7 个名字里另含 2 个"类名"（Character / StarsCollider / MW_GameNotice 见下），')
o.append('#       凡方法名 0 命中的名字，会在「类名命中」小节给出该 TypeDef 及其方法清单。')
o.append('# 程序集：Assembly-CSharp（HybridCLR 热更 DLL，非 AOT）')
o.append('# 每条：类全名 :: 签名 | MethodDef token=0x06XXXXXX | 特征标签')
o.append('')
for t in TARGETS:
    o.append('=' * 78)
    o.append('## 目标名 `%s`' % t)
    o.append('=' * 78)
    hits = target_hits[t]
    o.append('--- 方法名精确命中：%d 处 ---' % len(hits))
    for full, rid, s in hits:
        o.append('  %s :: %s' % (full, s))
        o.append('      token=0x%08X (rid=%d) | 程序集=%s | 标签=%s' % (0x06000000 | rid, rid, ASM, tag(full)))
    if not hits:
        o.append('  (无同名的具体方法)')
    # class-name match
    cms = [(full, cls_methods.get(full, [])) for full, _s, _e in classes if full == t or full.endswith('.' + t)]
    if cms:
        o.append('--- 同名/后缀同名【类】命中：%d 个 ---' % len(cms))
        for full, ids in cms:
            o.append('  类型 %s  [%d methods]  标签=%s' % (full, len(ids), tag(full)))
            for rid in ids:
                nm, ret, params, gen = sig_of(rid)
                o.append('      %s :: %s | token=0x%08X' % (full, sig_str(nm, ret, params, gen), 0x06000000 | rid))
    else:
        o.append('--- 同名【类】命中：0 个 ---')
    o.append('')

# 重载说明
o.append('#' * 78)
o.append('# 附加说明：AddProjectile 的重载/多类情况')
o.append('#' * 78)
for full, rid, s in target_hits['AddProjectile']:
    o.append('  %s :: %s  (token=0x%08X, 标签=%s)' % (full, s, 0x06000000 | rid, tag(full)))
o.append('')
open(os.path.join(AOT, 'hotupdate_hits.txt'), 'w', encoding='utf-8').write('\n'.join(o))
print('hits.txt lines:', len(o))
