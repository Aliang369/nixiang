// il2cpp 头部内存 dump + cybrex RW 指针表 + 匿名 rw- 区 metadata 字符串搜索
'use strict';
function hexHead(addr, n) {
    try {
        const b = addr.readByteArray(n);
        return Array.from(new Uint8Array(b)).map(x => ('0' + x.toString(16)).slice(-2)).join('');
    } catch (e) { return 'unreadable'; }
}
const il = Process.getModuleByName('libil2cpp.so');
send({ type: 'info', msg: 'il2cpp base=' + il.base + ' size=0x' + il.size.toString(16) });

// 1) il2cpp 头 8MB
const CH = 512 * 1024, TOTAL = 8 * 1024 * 1024;
for (let o = 0; o < TOTAL; o += CH) {
    try {
        send({ type: 'il2head', off: o }, il.base.add(o).readByteArray(CH));
    } catch (e) { send({ type: 'info', msg: 'read fail @0x' + o.toString(16) + ': ' + e }); break; }
}

// 2) cybrex RW 区 (0x402050-0x40ebc0 file) 的运行时指针上下文
try {
    const cb = Process.getModuleByName('libcybrex.so');
    send({ type: 'cbrw', off: 0x402000 }, cb.base.add(0x402000).readByteArray(0x4000));
} catch (e) { send({ type: 'info', msg: 'cbrw fail: ' + e }); }

// 3) 匿名 rw- 大段搜 metadata 字符串 (≤256MB 的段)
const pats = ['67 6c 6f 62 61 6c 2d 6d 65 74 61 64 61 74 61', '47 47 5f', '68 6f 74 66 69 78'];
const ranges = Process.enumerateRangesSync({ protection: 'rw-', coalesce: false });
let scanned = 0;
for (const pat of pats) {
    const hits = [];
    for (const r of ranges) {
        if (r.size > 256 * 1024 * 1024) continue;
        try {
            for (const f of Memory.scanSync(r.base, r.size, pat)) {
                hits.push({ addr: f.address.toString(), snip: hexHead(f.address.sub(8), 64) });
                if (hits.length > 20) break;
            }
        } catch (e) {}
        if (hits.length > 20) break;
    }
    scanned++;
    send({ type: 'anonstr', pat: pat, hits: hits });
}
send({ type: 'info', msg: 'anon patterns done=' + scanned + ' ranges=' + ranges.length });
send({ type: 'done' });
