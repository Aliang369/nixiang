// 一次性交叉验证: 统计 rw- 范围 + 搜 yyzy_hotfix / hotfixMainScriptName
'use strict';
function ranges() {
    try { return Process.enumerateRanges({ protection: 'rw-', coalesce: false }); }
    catch (e) { return Process.enumerateRangesSync('rw-'); }
}
const rs = ranges();
let total = 0, skipped = 0, skippedSize = 0;
for (const r of rs) { if (r.size > 512*1024*1024) { skipped++; skippedSize += r.size; } else total += r.size; }
send({type:'info', msg:`ranges=${rs.length} scanned=${(total/1048576).toFixed(0)}MB skipped=${skipped}(${(skippedSize/1048576).toFixed(0)}MB)`});

function find(pat) {
    const out = [];
    for (const r of rs) {
        if (r.size > 512*1024*1024) continue;
        try { for (const f of Memory.scanSync(r.base, r.size, pat)) out.push(f.address.toString()); } catch (e) {}
    }
    return out;
}
const yh = find('79 7a 7a 79 5f 68 6f 74 66 69 78'); // yyzy_hotfix
const hm = find('68 6f 74 66 69 78 4d 61 69 6e'); // hotfixMain
const bsjb = find('42 53 4a 42'); // BSJB
send({type:'result', yyzy_hotfix: yh, hotfixMain: hm.length, bsjb: bsjb.length, bsjb_first: bsjb.slice(0,5)});
