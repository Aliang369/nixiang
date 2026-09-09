// GOT 审计: 扫全部模块 rw- 段的 8 字节指针, 指向 cybrex 的 = shadowhook 落点
'use strict';
const cb = Process.getModuleByName('libcybrex.so');
const cbLo = cb.base, cbHi = cb.base.add(cb.size);
send({ type: 'info', msg: 'cybrex base=' + cb.base + ' size=0x' + cb.size.toString(16) });

const targets = ['libil2cpp.so', 'libunity.so', 'libmain.so', 'libxlua.so'];
for (const mn of targets) {
    let m;
    try { m = Process.getModuleByName(mn); } catch (e) { send({ type: 'info', msg: mn + ' 不存在' }); continue; }
    let ranges = [];
    try { ranges = m.enumerateRanges('rw-'); } catch (e) { continue; }
    let hits = 0;
    for (const r of ranges) {
        const rel0 = r.base.sub(m.base).toInt32();
        const n = Math.floor(r.size / 8);
        for (let i = 0; i < n; i++) {
            let v;
            try { v = r.base.add(i * 8).readPointer(); } catch (e) { break; }
            if (v.compare(cbLo) >= 0 && v.compare(cbHi) < 0) {
                const off = rel0 + i * 8;
                const into = v.sub(cbLo).toInt32();
                send({ type: 'got', mod: mn, entry: '0x' + off.toString(16), target: 'cybrex+0x' + into.toString(16) });
                hits++;
                if (hits > 60) break;
            }
        }
        if (hits > 60) break;
    }
    send({ type: 'info', msg: mn + ': cybrex 指针条目=' + hits });
}
send({ type: 'done' });
