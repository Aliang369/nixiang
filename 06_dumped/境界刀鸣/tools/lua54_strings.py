import struct,sys

def parse(data):
    p=0
    assert data[:4]==b'\x1bLua', 'not lua'
    p=5
    p+=1
    p+=6
    p+=2
    p+=1
    p+=1
    p+=8
    p+=8
    strings=[]; nums=[]
    def loadInt():
        nonlocal p
        x=0
        while True:
            b=data[p]; p+=1
            x=(x<<7)|(b&0x7f)
            if b&0x80==0: break
        return x
    def loadString():
        nonlocal p
        n=loadInt()
        if n==0: return None
        s=data[p:p+n-1]; p+=n
        return s
    def loadNumber():
        nonlocal p
        v=struct.unpack('<d',data[p:p+8])[0]; p+=8
        return v
    def proto(depth=0):
        nonlocal p
        p+=1
        src=loadString()
        if src: strings.append(src)
        loadInt(); loadInt()
        p+=3
        ncode=loadInt(); p+=ncode*4
        nk=loadInt()
        for _ in range(nk):
            t=data[p]; p+=1
            if t in (0,1,2): pass
            elif t==3: nums.append(loadNumber())
            elif t==4: nums.append(loadInt())
            elif t==5 or t==(5|0x80):
                s=loadString()
                if s is not None: strings.append(s)
            else: raise ValueError(f'bad const type {t} @{p}')
        nups=loadInt(); p+=nups*3
        nprotos=loadInt()
        for _ in range(nprotos): proto(depth+1)
        n=loadInt(); p+=n
        n=loadInt(); p+=n*4
        n=loadInt()
        for _ in range(n):
            loadString(); loadInt(); loadInt()
        n=loadInt()
        for _ in range(n): loadString()
    proto()
    return strings,nums

for f in sys.argv[1:]:
    try:
        d=open(f,'rb').read()
        s,n=parse(d)
        seen=set(); out=[]
        for k in s:
            try: ks=k.decode('utf-8')
            except: ks=repr(k)
            if ks not in seen:
                seen.add(ks); out.append(ks)
        print(f'### {f.split("/")[-1]}  ({len(d)}B, str={len(s)}, num={len(n)})')
        print('  K: ' + ' | '.join(out[:150]))
        print('  N: ' + ' | '.join(str(x) for x in n[:40]))
    except Exception as e:
        print(f'### {f} PARSE FAIL: {type(e).__name__}: {e}')
