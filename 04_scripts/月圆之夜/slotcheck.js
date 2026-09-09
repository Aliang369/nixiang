// 最小实验: thunk 入口/br x2 hook + 槽位轮询 (无扫描, 避免上轮杂音)
'use strict';
const il = () => Process.getModuleByName('libil2cpp.so');
let slotReported = false;
let callCount = 0;
const aotRanges = [];
let peScanned = false;

function inAot(addr) {
    for (const r of aotRanges) {
        if (addr.compare(r.start) >= 0 && addr.compare(r.start.add(r.len)) < 0) return true;
    }
    return false;
}

// 窗口期 PE(BSJB) 扫描: 抓 Assembly.Load 的明文热更 DLL
function scanPE() {
    let ranges;
    try { ranges = Process.enumerateRanges({ protection: 'rw-', coalesce: false }); }
    catch (e) {
        try { ranges = Process.enumerateRangesSync({ protection: 'rw-', coalesce: false }); }
        catch (e2) { send({ type: 'err', msg: 'scanPE ranges: ' + e2 }); return; }
    }
    const seen = [];
    let dumped = 0;
    for (const r of ranges) {
        if (r.size > 512 * 1024 * 1024 || dumped >= 6) continue;
        let hits = [];
        try { hits = Memory.scanSync(r.base, r.size, '42 53 4a 42'); } catch (e) { continue; }
        for (const h of hits) {
            if (inAot(h.address)) continue;
            // 回溯 MZ (最多 1MB)
            const back = Math.min(0x100000, h.address.sub(r.base).toInt32());
            let mz = null;
            try {
                const u8 = new Uint8Array(h.address.sub(back).readByteArray(back));
                for (let i = back - 2; i >= 0; i--) {
                    if (u8[i] === 0x4d && u8[i + 1] === 0x5a) { mz = h.address.sub(back - i); break; }
                }
            } catch (e) { continue; }
            if (!mz) continue;
            const key = mz.toString();
            if (seen.indexOf(key) >= 0) continue;
            seen.push(key);
            const avail = r.base.add(r.size).sub(mz).toInt32();
            const n = Math.min(4 * 1024 * 1024, avail);
            try {
                send({ type: 'pe', mz: key, bsjb: h.address.toString(), n: n }, mz.readByteArray(n));
                dumped++;
            } catch (e) {}
            if (dumped >= 6) break;
        }
        if (dumped >= 6) break;
    }
    send({ type: 'info', msg: 'scanPE done, PE dumps=' + dumped });
}

function hexHead(addr, n) {
    try {
        return Array.from(new Uint8Array(addr.readByteArray(n))).map(x => ('0' + x.toString(16)).slice(-2)).join('');
    } catch (e) { return 'unreadable'; }
}

function install() {
    let m;
    try { m = il(); } catch (e) { return false; }
    send({ type: 'info', msg: 'il2cpp base=' + m.base });
    // thunk 真实入口 0x3ce95f4 (0x3ce95f0 是上一函数的 ret): 抓 dllBytes 数组 + 调用栈
    try {
        Interceptor.attach(m.base.add(0x3ce95f4), {
            onEnter(args) {
                callCount++;
                try {
                    const arr = this.context.x0;
                    const len = arr.add(0x18).readU32();
                    const data = arr.add(0x20); // il2cpp Array 数据内联在 0x20 (klass/monitor/bounds/max_length 之后)
                    send({ type: 'thunk', mode: this.context.x1.toInt32(), len: len, head: hexHead(data, 32) });
                    if (len > 0 && len < 128 * 1024 * 1024) {
                        aotRanges.push({ start: data, len: len });
                        send({ type: 'arrdump', len: len }, data.readByteArray(Math.min(len, 32 * 1024 * 1024)));
                    }
                } catch (e) { send({ type: 'err', msg: 'thunk body: ' + e }); }
                if (callCount >= 20 && !peScanned) {
                    peScanned = true;
                    // 持续周期扫描: 等用户登录触发热更加载 (每 15s, 共 40 轮 ≈ 10 分钟)
                    for (let i = 0; i < 40; i++) {
                        setTimeout(scanPE, 2500 + i * 15000);
                    }
                }
                try {
                    const bt = Thread.backtrace(this.context, Backtracer.FUZZY).slice(0, 14).map(a => {
                        const s = a.toString(16);
                        if (a.compare(m.base) >= 0 && a.compare(m.base.add(m.size)) < 0) {
                            return 'il2cpp+0x' + a.sub(m.base).toString(16);
                        }
                        return s;
                    });
                    send({ type: 'bt', frames: bt });
                } catch (e) { send({ type: 'err', msg: 'bt: ' + e }); }
            }
        });
        send({ type: 'info', msg: 'thunk hook ok' });
    } catch (e) { send({ type: 'err', msg: 'thunk hook: ' + e }); }
    // br x2 站点 0x3ce9640: 真实 icall 实现地址
    try {
        Interceptor.attach(m.base.add(0x3ce9640), {
            onEnter(args) {
                send({ type: 'fnaddr', val: this.context.x2.toString() });
            }
        });
        send({ type: 'info', msg: 'br x2 hook ok' });
    } catch (e) { send({ type: 'err', msg: 'brx2: ' + e }); }
    return true;
}

let tries = 0;
const t = setInterval(() => {
    tries++;
    if (install()) {
        clearInterval(t);
        // 槽位轮询: 每 10s
        let n = 0;
        const s = setInterval(() => {
            n++;
            try {
                const v = il().base.add(0x54bc070).readPointer();
                send({ type: 'slot', n: n, val: v.toString() });
                if (!v.isNull()) { clearInterval(s); }
            } catch (e) {
                send({ type: 'err', msg: 'slot: ' + e });
                clearInterval(s);
            }
            if (n > 12) clearInterval(s);
        }, 10000);
    } else if (tries > 300) {
        clearInterval(t);
        send({ type: 'err', msg: 'il2cpp 未加载' });
    }
}, 200);
