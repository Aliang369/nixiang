// 上下文探针 + 新增 BSJB 镜像倾倒
'use strict';
const PAT = {
    hotfix: '68 6f 74 66 69 78',        // "hotfix"
    base_: '5f 62 61 73 65 2f',          // "_base/"
    yyzy:   '79 7a 7a 79',               // "yyzy"
};

function rangesAll() {
    const out = [];
    for (const p of ['rw-', 'r--']) {
        try { for (const r of Process.enumerateRanges({ protection: p, coalesce: false })) out.push(r); } catch (e) {}
    }
    return out;
}

function scanRange(r, pat, onHit, done) {
    const CH = 64 * 1024 * 1024;
    let off = 0;
    function step() {
        if (off >= r.size) { done(); return; }
        const sz = Math.min(CH, r.size - off);
        try {
            Memory.scan(r.base.add(off), sz, pat, {
                onMatch: (a) => { onHit(a); },
                onComplete: () => { off += sz; setTimeout(step, 0); }
            });
        } catch (e) { off += sz; setTimeout(step, 0); }
    }
    step();
}

function ctxOf(addr, n) {
    try {
        const back = 64;
        const buf = new Uint8Array(addr.sub(back).readByteArray(n));
        let s = '';
        for (let i = 0; i < buf.length; i++) {
            const c = buf[i];
            s += (c >= 0x20 && c < 0x7f) ? String.fromCharCode(c) : (c === 0 ? '·' : '？');
        }
        return s;
    } catch (e) { return '<unreadable>'; }
}

const rs = rangesAll();

// --- 1. hotfix / _base/ 上下文 (各限 60 处) ---
function ctxPass(patName, cap) {
    return new Promise(resolve => {
        const hits = [];
        let i = 0;
        function next() {
            if (i >= rs.length || hits.length >= cap) { resolve(); return; }
            const r = rs[i++];
            scanRange(r, PAT[patName], a => {
                if (hits.length < cap) hits.push({ a: a.toString(), c: ctxOf(a, 160) });
            }, () => setTimeout(next, 0));
        }
        next();
    });
}

// --- 2. BSJB 镜像倾倒 ---
function dumpImages() {
    const seen = {}; let n = 0;
    return new Promise(resolve => {
        let i = 0;
        function nextR() {
            if (i >= rs.length) { resolve(n); return; }
            const r = rs[i++];
            // 同步扫 BSJB (r 内) — r 可能大, 但 BSJB 扫描与深扫同量级; 改为分块异步
            scanRange(r, '42 53 4a 42', b => {
                try {
                    const back = 0x10000;
                    const mzHits = Memory.scanSync(b.sub(back), back, '4d 5a 90 00');
                    if (mzHits.length === 0) return;
                    const mz = mzHits[mzHits.length - 1].address;
                    const lfanew = mz.add(0x3c).readU32();
                    if (lfanew === 0 || lfanew > 0x4000) return;
                    const pe = mz.add(lfanew);
                    if (pe.readU16() !== 0x4550) return;
                    const opt = pe.add(24);
                    const magic = opt.readU16();
                    if (magic !== 0x20b && magic !== 0x10b) return;
                    let sizeImg = opt.add(56).readU32();
                    if (sizeImg === 0 || sizeImg > 0x4000000) sizeImg = 8 * 1024 * 1024;
                    const head = new Uint8Array(mz.readByteArray(512));
                    let sum = 0; for (let k = 0; k < 512; k++) sum = (sum + head[k] * (k + 7)) >>> 0;
                    const key = sizeImg + ':' + sum;
                    if (seen[key]) return;
                    seen[key] = 1;
                    send({ type: 'dump', at: mz.toString(), size: sizeImg, n: n }, mz.readByteArray(sizeImg));
                    n++;
                } catch (e) {}
            }, () => setTimeout(nextR, 0));
        }
        nextR();
    });
}

(async () => {
    for (const p of ['hotfix', 'base_']) {
        const hits = [];
        // 重新走一遍拿上下文
        await new Promise(resolve => {
            let i = 0;
            function next() {
                if (i >= rs.length) { resolve(); return; }
                const r = rs[i++];
                scanRange(r, PAT[p], a => { if (hits.length < 60) hits.push({ a: a.toString(), c: ctxOf(a, 160) }); }, () => setTimeout(next, 0));
            }
            next();
        });
        send({ type: 'ctx', pat: p, n: hits.length, hits: hits });
    }
    const n = await dumpImages();
    send({ type: 'done', imgs: n });
})();
