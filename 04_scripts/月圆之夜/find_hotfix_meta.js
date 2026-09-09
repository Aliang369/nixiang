// 定位热更镜像元数据堆: 搜 'yyzy_hotfix' ASCII → 倾倒周围区域
'use strict';
function scanStr(pat) {
    let ranges;
    try { ranges = Process.enumerateRanges({ protection: 'rw-', coalesce: false }); }
    catch (e) { ranges = Process.enumerateRangesSync('rw-'); }
    const out = [];
    for (const r of ranges) {
        if (r.size > 512 * 1024 * 1024) continue;
        try {
            for (const f of Memory.scanSync(r.base, r.size, pat)) {
                out.push({ addr: f.address.toString(), rangeBase: r.base.toString(), rangeSize: r.size });
            }
        } catch (e) {}
    }
    return out;
}

send({ type: 'hits_yyzy', hits: scanStr('79 7a 7a 79 5f 68 6f 74 66 69 78') }); // yyzy_hotfix
send({ type: 'hits_us', hits: scanStr('23 55 53') }); // '#US' 元数据流名
send({ type: 'done' });
