import re,sys
dis=open(sys.argv[1]).read().split("\n")
names=sys.argv[2:]
# 顶层块: "-- 子函数 N "
blocks={}
cur=None
for i,l in enumerate(dis):
    m=re.match(r'^-- 子函数 (\d+) ', l)
    if m:
        cur=int(m.group(1)); blocks[cur]=[]
    elif cur is not None:
        blocks[cur].append(l)
# 找 proto 编号: CLOSURE 行 -> proto#N，其后面 4 行内出现 SETTABLE "...name"
targets={}
for i,l in enumerate(dis):
    m=re.search(r'CLOSURE\s+R\d+\s+-> proto#(\d+)', l)
    if m:
        pn=int(m.group(1))
        for l2 in dis[i+1:i+2]:
            for nm in names:
                if re.search(r'"%s"'%re.escape(nm), l2):
                    targets.setdefault(nm,pn)
for nm in names:
    if nm in targets:
        pn=targets[nm]
        print("=== %s -> proto#%d ==="%(nm,pn))
        print("\n".join(blocks.get(pn,["<missing>"])))
    else:
        print("=== %s -> 未找到注册点 ==="%nm)
