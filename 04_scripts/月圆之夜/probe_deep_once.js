// 深度探针: 全 r--/rw- (含>512MB 大区, 异步扫描防卡) 找 yyzy_hotfix + 统计 BSJB/MZ
'use strict';
const PAT_YH = '79 7a 7a 79 5f 68 6f 74 66 69 78';

function rangesAll() {
    const out = [];
    for (const p of ['rw-', 'r--']) {
        let rs = [];
        try { rs = Process.enumerateRanges({ protection: p, coalesce: false }); }
        catch (e) { continue; }
        for (const r of rs) out.push(r);
    }
    return out;
}

const yhHits = [];
const bsjbCount = [0];
const mzCount = [0];

function scanRange(r, pat, onHit, done) {
    // 分块异步扫描, 每块 64MB
    const CH = 64 * 1024 * 1024;
    let off = 0;
    function step() {
        if (off >= r.size) { done(); return; }
        const sz = Math.min(CH, r.size - off);
        try {
            Memory.scan(r.base.add(off), sz, pat, {
                onMatch: (a, s) => { onHit(a); },
                onComplete: () => { off += sz; setTimeout(step, 0); }
            });
        } catch (e) { off += sz; setTimeout(step, 0); }
    }
    step();
}

const rs = rangesAll();
let total = 0;
for (const r of rs) total += r.size;
send({ type: 'info', msg: 'ranges=' + rs.length + ' total=' + (total / 1048576).toFixed(0) + 'MB' });

let idx = 0;
function next() {
    if (idx >= rs.length) {
        send({ type: 'result', yh: yhHits.map(a => a.toString()), bsjb: bsjbCount[0], mz: mzCount[0] });
        return;
    }
    const r = rs[idx++];
    scanRange(r, PAT_YH, a => yhHits.push(a), () => {
        scanRange(r, '42 53 4a 42', () => bsjbCount[0]++, () => {
            scanRange(r, '4d 5a 90 00', () => mzCount[0]++, () => {
                send({ type: 'prog', done: idx, of: rs.length });
                setTimeout(next, 0);
            });
        });
    });
}
next();
