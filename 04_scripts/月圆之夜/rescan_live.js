// 对已登录的活进程重扫 BSJB: 按前 4KB md5 特征去重, 找 AOT 尺寸之外的新 PE
'use strict';
const il = Process.getModuleByName('libil2cpp.so');
const KNOWN_AOT_SIZES = new Set([212480,770560,174592,31232,444416,20992,317952,487424,1178624,7168,5632,7680,491520,113152,64512,21504,794624,11264,232448,144384,2591232,391680,55296]);

function key(addr) {
    try {
        const b = new Uint8Array(addr.readByteArray(4096));
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
            const back = Math.min(0x100000, h.address.sub(r.base).toInt32());
            let mz = null;
            try {
                const u8 = new Uint8Array(h.address.sub(back).readByteArray(back));
                for (let i = back - 2; i >= 0; i--) {
                    if (u8[i] === 0x4d && u8[i + 1] === 0x5a) { mz = h.address.sub(back - i); break; }
                }
            } catch (e) { continue; }
            if (!mz) continue;
            const avail = r.base.add(r.size).sub(mz).toInt32();
            const n = Math.min(4 * 1024 * 1024, avail);
            // 过滤: MZ→PE 头里读 SizeOfRawData 最大节 = 实际大小, 若是已知 AOT 尺寸则跳过
            try {
                const lfa = mz.add(0x3c).readU32();
                if (lfa > n || lfa > 0x1000) { mz = null; continue; }
                const secOff = lfa + 24 + 224; // PE32
                const nsec = mz.add(lfa + 6).readU16();
                if (nsec === 0 || nsec > 8) { mz = null; continue; }
                let fsize = 0;
                for (let s = 0; s < nsec; s++) {
                    const rsz = mz.add(secOff + s * 40 + 16).readU32();
                    const ro = mz.add(secOff + s * 40 + 20).readU32();
                    fsize = Math.max(fsize, ro + rsz);
                }
                if (KNOWN_AOT_SIZES.has(fsize)) continue;
                const k = key(mz) + '_' + fsize;
                if (seen[k]) continue;
                seen[k] = 1;
                send({ type: 'pe', mz: mz.toString(), fsize: fsize, n: Math.min(n, fsize + 4096) }, mz.readByteArray(Math.min(n, fsize + 4096)));
                dumped++;
            } catch (e) { continue; }
            if (dumped >= 10) break;
        }
        if (dumped >= 10) break;
    }
    send({ type: 'info', msg: 'scan done, new PEs=' + dumped });
}

scan();
let rounds = 1;
const t = setInterval(() => {
    rounds++;
    scan();
    if (rounds >= 18) clearInterval(t);
}, 20000);
