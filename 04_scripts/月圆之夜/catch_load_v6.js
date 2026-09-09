// v6: HybridCLR 装载路径动态观察
// H1=0x1497b18 装载错误路径  H2=0x1497c84 init函数入口  H3=0x1497c8c init重入
// THUNK=0x3ce95f4 LoadMetadataForAOTAssembly icall thunk (x0=Array, data@+0x20)
// LOGS=0x125c2b8/0x125c934/0x125c8c8 il2cpp 日志助手(过滤关键词)
// + BSJB 每10s轮询, 新镜像自动倾倒
'use strict';
const BASE = Module.getBaseUrl ? null : null; // placeholder
const mod = Process.getModuleByName('libil2cpp.so');
const B = mod.base;
send({ type: 'info', msg: 'libil2cpp base=' + B });

const VA = {
    h1: 0x1497b18, h2: 0x1497c84, h3: 0x1497c8c,
    thunk: 0x3ce95f4,
    log1: 0x125c2b8, log2: 0x125c934, log3: 0x125c8c8,
};
function a(name) { return B.add(VA[name]); }

function safeStr(p, n) {
    try { const s = p.readCString(n || 200); return s.length ? s : '<empty>'; } catch (e) { return '<unreadable>'; }
}

const KW = ['mage', 'ybrid', 'otfix', 'ssembl', 'nterpret', 'hotupdate', 'HotUpdate'];

// --- H1: 错误路径执行点 ---
try {
    Interceptor.attach(a('h1'), {
        onEnter() {
            send({ type: 'h1', x0: this.context.x0.toString(), x1: this.context.x1.toString(),
                   fmt: safeStr(this.context.x2, 80), w3: this.context.x3.toInt32() });
        }
    });
    send({ type: 'info', msg: 'hook h1 ok' });
} catch (e) { send({ type: 'info', msg: 'h1 fail: ' + e }); }

// --- H2: init 函数入口 ---
try {
    Interceptor.attach(a('h2'), {
        onEnter() {
            send({ type: 'h2', x0: this.context.x0.toString(), x1: this.context.x1.toString(),
                   s0: safeStr(this.context.x0, 60) });
        }
    });
    send({ type: 'info', msg: 'hook h2 ok' });
} catch (e) { send({ type: 'info', msg: 'h2 fail: ' + e }); }

// --- H3: init 重入 ---
try {
    Interceptor.attach(a('h3'), {
        onEnter() { send({ type: 'h3' }); }
    });
    send({ type: 'info', msg: 'hook h3 ok' });
} catch (e) { send({ type: 'info', msg: 'h3 fail: ' + e }); }

// --- THUNK: LoadMetadataForAOTAssembly ---
let thunkN = 0;
try {
    Interceptor.attach(a('thunk'), {
        onEnter() {
            thunkN++;
            try {
                const arr = this.context.x0;
                const len = arr.add(0x18).readU32();
                const head = new Uint8Array(arr.add(0x20).readByteArray(2));
                const isMZ = head[0] === 0x4d && head[1] === 0x5a;
                send({ type: 'thunk', n: thunkN, len: len, mz: isMZ, at: arr.add(0x20).toString() });
                if (isMZ && len > 0 && len < 0x4000000) {
                    send({ type: 'dump', tag: 'thunk' + thunkN, at: arr.add(0x20).toString(), size: len },
                         arr.add(0x20).readByteArray(len));
                }
            } catch (e) { send({ type: 'err', msg: 'thunk read: ' + e }); }
        }
    });
    send({ type: 'info', msg: 'hook thunk ok' });
} catch (e) { send({ type: 'info', msg: 'thunk fail: ' + e }); }

// --- LOG 助手 ---
for (const ln of ['log1', 'log2', 'log3']) {
    try {
        Interceptor.attach(a(ln), {
            onEnter() {
                const s = safeStr(this.context.x0, 260);
                for (const k of KW) {
                    if (s.indexOf(k) >= 0) { send({ type: 'log', via: ln, msg: s }); break; }
                }
            }
        });
        send({ type: 'info', msg: 'hook ' + ln + ' ok' });
    } catch (e) { send({ type: 'info', msg: ln + ' fail: ' + e }); }
}

// --- BSJB 轮询 ---
const seen = {};
function rangesRW() {
    try { return Process.enumerateRanges({ protection: 'rw-', coalesce: false }); } catch (e) { return []; }
}
function scanR(r, pat, cb) {
    try { for (const f of Memory.scanSync(r.base, r.size, pat)) cb(f.address); } catch (e) {}
}
function pollBSJB() {
    let n = 0;
    for (const r of rangesRW()) {
        if (r.size > 512 * 1024 * 1024) continue;
        scanR(r, '42 53 4a 42', b => {
            if (n >= 6) return;
            try {
                const back = 0x10000;
                const lo = b.size !== undefined ? b.address.sub(back) : b.sub(back);
                const mzHits = Memory.scanSync(lo, back, '4d 5a 90 00');
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
                send({ type: 'dump', tag: 'bsjb', at: mz.toString(), size: sizeImg }, mz.readByteArray(sizeImg));
                n++;
            } catch (e) {}
        });
    }
    send({ type: 'poll', new: n });
}
const pt = setInterval(pollBSJB, 10000);
setTimeout(pollBSJB, 2000);
send({ type: 'info', msg: 'v6 armed' });
