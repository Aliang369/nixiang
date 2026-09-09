// 轮询等 yyzy_hotfix 出现 → 命中后倾倒周围 ±2MB (自动, 检测到即停)
'use strict';
const PAT = '79 7a 7a 79 5f 68 6f 74 66 69 78'; // yyzy_hotfix
let done = false;

function scanDump() {
    if (done) return;
    let ranges;
    try { ranges = Process.enumerateRanges({ protection: 'rw-', coalesce: false }); }
    catch (e) { ranges = Process.enumerateRangesSync('rw-'); }
    for (const r of ranges) {
        if (r.size > 512 * 1024 * 1024) continue;
        let hits = [];
        try { hits = Memory.scanSync(r.base, r.size, PAT); } catch (e) { continue; }
        for (const h of hits) {
            done = true;
            // 倾倒命中点前后各 2MB (受 range 边界限制)
            const back = Math.min(2 * 1024 * 1024, h.address.sub(r.base).toInt32());
            const fwd = Math.min(2 * 1024 * 1024, r.base.add(r.size).sub(h.address).toInt32());
            const start = h.address.sub(back);
            const total = back + fwd;
            try {
                send({ type: 'dump', at: h.address.toString(), back: back, fwd: fwd, n: total }, start.readByteArray(total));
            } catch (e) { send({ type: 'err', msg: 'dump: ' + e }); }
            send({ type: 'info', msg: 'captured @' + h.address + ' (+' + back + '/-' + fwd + ')' });
        }
        if (done) break;
    }
    if (!done) send({ type: 'tick' });
}

scanDump();
let n = 0;
const t = setInterval(() => {
    n++;
    if (done || n > 30) { clearInterval(t); if (!done) send({ type: 'info', msg: '超时未命中' }); return; }
    scanDump();
}, 15000);
