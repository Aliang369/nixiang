// H路线 v5: 轮询 yyzy_hotfix 字符串 → BSJB 扫描 → MZ 回溯 → 按 SizeOfImage 整像倾倒
'use strict';
const PAT_YH = '79 7a 7a 79 5f 68 6f 74 66 69 78'; // "yyzy_hotfix"
const PAT_BSJB = '42 53 4a 42'; // "BSJB"
const PAT_MZ = '4d 5a 90 00'; // MZ\0\0 后随 e_cblp=0x90 → 典型 PE

function ranges() {
    try { return Process.enumerateRanges({ protection: 'rw-', coalesce: false }); }
    catch (e) { return Process.enumerateRangesSync('rw-'); }
}

function findHits(pat) {
    const out = [];
    for (const r of ranges()) {
        if (r.size > 512 * 1024 * 1024) continue;
        try { for (const f of Memory.scanSync(r.base, r.size, pat)) out.push(f.address); } catch (e) {}
    }
    return out;
}

// 从 BSJB 命中回溯 ≤64KB 找 MZ，解析 SizeOfImage 整像倾倒
function dumpImages() {
    const seen = {};
    let n = 0;
    const bsjbHits = findHits(PAT_BSJB);
    send({ type: 'info', msg: 'BSJB hits: ' + bsjbHits.length });
    for (const b of bsjbHits) {
        try {
            const back = 0x10000;
            const lo = b.sub(back);
            let mzHits = [];
            try { mzHits = Memory.scanSync(lo, back, PAT_MZ); } catch (e) { continue; }
            if (mzHits.length === 0) continue;
            const mz = mzHits[mzHits.length - 1].address;
            const lfanew = mz.add(0x3c).readU32();
            if (lfanew === 0 || lfanew > 0x4000) continue;
            const pe = mz.add(lfanew);
            if (pe.readU16() !== 0x4550) continue; // 'PE\0\0' 低16位
            const opt = pe.add(24);
            const magic = opt.readU16();
            if (magic !== 0x20b && magic !== 0x10b) continue;
            let sizeImg = opt.add(56).readU32();
            if (sizeImg === 0 || sizeImg > 0x4000000) sizeImg = 8 * 1024 * 1024;
            // 去重: 大小 + 头部 512B 粗校验和
            const head = mz.readByteArray(512);
            const hv = new Uint8Array(head);
            let sum = 0; for (let i = 0; i < 512; i++) sum = (sum + hv[i] * (i + 7)) >>> 0;
            const key = sizeImg + ':' + sum;
            if (seen[key]) { continue; }
            seen[key] = 1;
            const data = mz.readByteArray(sizeImg);
            send({ type: 'dump', at: mz.toString(), size: sizeImg, n: n }, data);
            n++;
            if (n >= 24) { send({ type: 'info', msg: '达到上限 24, 停止' }); break; }
        } catch (e) { send({ type: 'err', msg: 'img@' + b + ': ' + e }); }
    }
    return n;
}

// 兜底: 无 BSJB 镜像时, 围绕 yyzy_hotfix 字符串 ±2MB 倾倒私有容器
function dumpAroundStr() {
    const hits = findHits(PAT_YH);
    send({ type: 'info', msg: 'yyzy_hotfix string hits: ' + hits.length });
    let n = 0;
    for (const h of hits.slice(0, 4)) {
        try {
            const r = ranges().find(r => h.compare(r.base) >= 0 && h.compare(r.base.add(r.size)) < 0);
            if (!r) continue;
            const back = Math.min(2 * 1024 * 1024, h.sub(r.base).toInt32());
            const fwd = Math.min(2 * 1024 * 1024, r.base.add(r.size).sub(h).toInt32());
            const data = h.sub(back).readByteArray(back + fwd);
            send({ type: 'dump', at: h.toString(), size: back + fwd, ctx: 1, n: n }, data);
            n++;
        } catch (e) { send({ type: 'err', msg: 'str@' + h + ': ' + e }); }
    }
    return n;
}

let phase = 0; // 0=等字符串 1=已dump第一轮 2=完成
let pass2 = false;

function tick() {
    if (phase === 2) return;
    const hits = findHits(PAT_YH);
    if (hits.length === 0) { send({ type: 'tick' }); return; }
    if (phase === 0) {
        send({ type: 'info', msg: 'yyzy_hotfix 出现 ' + hits.length + ' 处, 3 秒后整像扫描' });
        phase = 1;
        setTimeout(() => {
            const n = dumpImages();
            if (n === 0) {
                send({ type: 'info', msg: '无 BSJB 镜像, 走字符串兜底' });
                dumpAroundStr();
            }
            setTimeout(() => {
                const n2 = dumpImages();
                send({ type: 'done', imgs: n2 });
                phase = 2;
            }, 15000);
        }, 3000);
    }
}

send({ type: 'info', msg: 'v5 loaded, 轮询中' });
tick();
const t = setInterval(tick, 15000);
setTimeout(() => { clearInterval(t); if (phase !== 2) send({ type: 'done', imgs: -1, msg: '窗口超时' }); }, 570000);
