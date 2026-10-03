import json, sys, re, os
sys.stdout.reconfigure(encoding='utf-8')
BASE = r'E:\nixiang\07_patched\敢达争锋对决\dump_out'
RVAS = [0x3d9cf54, 0x3db99cc, 0x67be180, 0x67be458]
print('target RVAs dec:', [hex(r) for r in RVAS], [r for r in RVAS])

# --- script.json ---
data = json.load(open(os.path.join(BASE, 'script.json'), encoding='utf-8'))
print('script.json keys:', list(data.keys()))
sm = data.get('ScriptMethod', [])
print('ScriptMethod count:', len(sm))
byaddr = {}
for e in sm:
    byaddr.setdefault(e['Address'], []).append(e)
for r in RVAS:
    for delta in (0, -0x4000, 0x4000):
        if r + delta in byaddr:
            for e in byaddr[r + delta]:
                print('HIT rva=0x%x (delta %+#x) addr=%d name=%s' % (r, delta, e['Address'], e['Name']))
            break
    else:
        print('no exact hit for 0x%x' % r)
