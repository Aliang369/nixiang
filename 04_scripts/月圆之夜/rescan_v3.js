// BSJB 扫描 v3: 内容特征排除已知 AOT; 无效 PE 头的候选也抓 (MHP 打花头)
'use strict';
const KNOWN_KEYS = new Set(global.__KNOWN_KEYS__ || []);

function fnv1a(addr, n) {
    try {
        const b = new Uint8Array(addr.readByteArray(n));
        let h = 0;
        for (let i = 0; i < b.length; i++) h = (Math.imul(h ^ b[i], 16777619) >>> 0);
        return h.toString(16);
    } catch (e) { return 'err'; }
}

function scan() {
    let ranges;
    try { ranges = Process.enumerateRanges({ protection: 'rw-', coalesce: false }); }
    catch (e) { ranges = Process.enumerateRangesSync({ protection: 'rw-', coalesce: false }); }
    const seen = {};
    let dumped = 0;
    for (const r of ranges) {
        if (r.size > 512 * 1024 * 1024) continue;
        let hits = [];
        try { hits = Memory.scanSync(r.base, r.size, '42 53 4a 42'); } catch (e) { continue; }
        for (const h of hits) {
            const back = Math.min(0x400000, h.address.sub(r.base).toInt32()); // 放宽到 4MB 回溯
            let mz = null;
            try {
                const u8 = new Uint8Array(h.address.sub(back).readByteArray(back));
                for (let i = back - 2; i >= 0; i--) {
                    if (u8[i] === 0x4d && u8[i + 1] === 0x5a) { mz = h.address.sub(back - i); break; }
                }
            } catch (e) { continue; }
            if (!mz) continue;
            const k = fnv1a(mz, 4096);
            if (KNOWN_KEYS.has(k)) continue;   // 已知 AOT 内容, 跳过
            if (seen[k]) continue;
            seen[k] = 1;
            const avail = r.base.add(r.size).sub(mz).toInt32();
            const n = Math.min(4 * 1024 * 1024, avail);
            try {
                send({ type: 'pe', mz: mz.toString(), n: n, key: k }, mz.readByteArray(n));
                dumped++;
            } catch (e) {}
            if (dumped >= 8) break;
        }
        if (dumped >= 8) break;
    }
    send({ type: 'info', msg: 'scan done, dumped=' + dumped });
}

scan();
let rounds = 1;
const t = setInterval(() => {
    rounds++;
    scan();
    if (rounds >= 24) clearInterval(t);
}, 15000);
