// libil2cpp 全 r-x/r-- 区哈希 + 差异块回传 + 匿名区字符串搜索（frida17 API）
'use strict';
const MOD = 'libil2cpp.so';
const CH = 1024 * 1024;
const il = Process.getModuleByName(MOD);

function fnv2(buf) {
    // 双 FNV-1a 32bit — 全部用 Math.imul 保证 32 位精度
    const u8 = new Uint8Array(buf);
    let h1 = 0x811c9dc5, h2 = 0x811c9dc5;
    for (let i = 0; i < u8.length; i++) {
        h1 = Math.imul(h1 ^ u8[i], 16777619) >>> 0;
        h2 = ((h2 + u8[i]) >>> 0);
        h2 = Math.imul(h2, 2246822519) >>> 0;
    }
    return [h1, h2];
}

let ranges = [];
try { ranges = il.enumerateRanges('r-x'); } catch (e) { send({ type: 'err', msg: 'erx:' + e }); }
try { ranges = ranges.concat(il.enumerateRanges('r--')); } catch (e) { send({ type: 'err', msg: 'err:' + e }); }
// 按 rel 去重（取第一个出现的段，r-x 优先于 r--）
{
    const seen = new Set();
    const filtered = [];
    for (const r of ranges) {
        const rel = r.base.sub(il.base).toInt32();
        const key = Math.floor(rel / 65536);
        if (seen.has(key)) continue;
        seen.add(key);
        filtered.push(r);
    }
    ranges = filtered;
}
send({ type: 'info', msg: 'ranges=' + ranges.length + ' total=' + ranges.reduce((s, r) => s + r.size, 0) });

function hashPass() {
    const out = [];
    for (const r of ranges) {
        const relBase = r.base.sub(il.base).toInt32();
        for (let o = 0; o < r.size; o += CH) {
            const len = Math.min(CH, r.size - o);
            let h;
            try {
                h = fnv2(r.base.add(o).readByteArray(len));
            } catch (e) {
                out.push({ rel: relBase + o, len: len, h: 'unreadable' });
                continue;
            }
            out.push({ rel: relBase + o, len: len, h: h });
        }
    }
    return out;
}

function dumpPass(list) {
    let n = 0, bytes = 0;
    for (const item of list) {
        try {
            send({ type: 'diffdump', rel: item.rel, len: item.len }, il.base.add(item.rel).readByteArray(item.len));
            n++; bytes += item.len;
        } catch (e) { send({ type: 'err', msg: 'dump fail rel=0x' + item.rel.toString(16) + ': ' + e }); }
    }
    send({ type: 'info', msg: 'diff chunks sent=' + n + ' bytes=' + bytes });
}

// RPC 接口
rpc.exports = {
    hash: hashPass,
    dumpdiff: dumpPass
};

function anonScan() {
    let ranges2;
    try {
        ranges2 = Process.enumerateRanges({ protection: 'rw-', coalesce: false });
    } catch (e) {
        try { ranges2 = Process.enumerateRangesSync('rw-'); } catch (e2) { send({ type: 'err', msg: 'anon:' + e2 }); return; }
    }
    const pats = ['67 6c 6f 62 61 6c 2d 6d 65 74 61 64 61 74 61', '47 47 5f', '68 6f 74 66 69 78'];
    for (const pat of pats) {
        const hits = [];
        for (const r of ranges2) {
            if (r.size > 256 * 1024 * 1024) continue;
            try {
                for (const f of Memory.scanSync(r.base, r.size, pat)) {
                    hits.push({ addr: f.address.toString(), snip: Array.from(new Uint8Array(f.address.sub(8).readByteArray(64))).map(x => ('0' + x.toString(16)).slice(-2)).join('') });
                    if (hits.length >= 20) break;
                }
            } catch (e) {}
            if (hits.length >= 20) break;
        }
        send({ type: 'anonstr', pat: pat, hits: hits });
    }
}

// RPC 接口: 主机编排 hash → 比对 → dumpdiff → anonScan
rpc.exports = {
    hash: hashPass,
    dumpdiff: dumpPass,
    anonScan: anonScan
};
