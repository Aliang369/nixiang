// 月圆之夜 MHP 被动侦察：hotfix icall 真实地址 / metadata 打开者 / 运行时字符串 / libcybrex 自解密检查
// 纯被动（不修改进程行为），适配 frida 17：动态查找导出，静态 API 一律 Process.getModuleByName(...).findExportByName
'use strict';

const LOG = (o) => send(o);

function modInfo(name) {
    try {
        const m = Process.getModuleByName(name);
        return { name: m.name, base: m.base, size: m.size, path: m.path };
    } catch (e) {
        return null;
    }
}

function addrLoc(addr) {
    const mods = Process.enumerateModulesSync();
    for (const m of mods) {
        const b = m.base;
        if (addr.compare(b) >= 0 && addr.compare(b.add(m.size)) < 0) {
            return m.name + '+0x' + addr.sub(b).toString(16);
        }
    }
    return 'anon:' + addr.toString();
}

function scanRanges(patterns, tag) {
    // 只扫 libil2cpp / libcybrex / libmain 的映射范围 + rw- 匿名大段（跳过超大堆，避免卡死）
    const mods = ['libil2cpp.so', 'libcybrex.so', 'libmain.so'];
    const ranges = [];
    for (const mn of mods) {
        try {
            const m = Process.getModuleByName(mn);
            for (const r of m.enumerateRanges('r--')) ranges.push({ r, tag: mn });
            for (const r of m.enumerateRanges('rw-')) ranges.push({ r, tag: mn });
        } catch (e) {}
    }
    for (const pat of patterns) {
        const hits = [];
        for (const { r, tag: mt } of ranges) {
            try {
                const found = Memory.scanSync(r.base, r.size, pat);
                for (const f of found) {
                    hits.push({ where: addrLoc(f.address), snip: hexHead(f.address, 48) });
                    if (hits.length > 40) break;
                }
            } catch (e) {}
            if (hits.length > 40) break;
        }
        LOG({ type: 'strings', tag: tag, pat: pat, hits: hits });
    }
}

function hexHead(addr, n) {
    try {
        const b = addr.readByteArray(n);
        return Array.from(new Uint8Array(b)).map(x => ('0' + x.toString(16)).slice(-2)).join('');
    } catch (e) { return 'unreadable'; }
}

// ---------- 阶段 1：尽早装 hook（libil2cpp 一出现就装） ----------
function installEarlyHooks() {
    const il = modInfo('libil2cpp.so');
    if (!il) return false;

    // 1) thunk br x2 站点：0x3ce9640 (LoadMetadataForAOTAssembly 懒解析 thunk 的尾跳)
    try {
        Interceptor.attach(il.base.add(0x3ce9640), {
            onEnter(args) {
                const fn = this.context.x2;
                LOG({ type: 'x2', where: addrLoc(fn), raw: fn.toString() });
            }
        });
        LOG({ type: 'info', msg: 'thunk br x2 hook installed @0x3ce9640' });
    } catch (e) { LOG({ type: 'err', msg: 'br x2 hook: ' + e }); }

    // 2) thunk 入口 0x3ce95f0：记录入参（dllBytes 对象指针 / mode）
    try {
        Interceptor.attach(il.base.add(0x3ce95f0), {
            onEnter(args) {
                LOG({ type: 'thunk_enter', x0: this.context.x0.toString(), w1: this.context.x1.toString() });
                // il2cpp Array 布局: klass/monitor/bounds/max_length(0x18)/data(0x20)
                try {
                    const arr = this.context.x0;
                    const len = arr.add(0x18).readU32();
                    const data = arr.add(0x20).readPointer();
                    LOG({ type: 'thunk_arr', len: len, head: hexHead(data, 32) });
                    if (len > 0 && len < 64 * 1024 * 1024) {
                        send({ type: 'arrdump', len: len, off: 0 }, data.readByteArray(len));
                    }
                } catch (e) { LOG({ type: 'err', msg: 'thunk_arr: ' + e }); }
            }
        });
    } catch (e) { LOG({ type: 'err', msg: 'thunk hook: ' + e }); }

    // 3) libc open/openat：谁打开 metadata
    try {
        const openat = Process.getModuleByName('libc.so').findExportByName('openat');
        const open = Process.getModuleByName('libc.so').findExportByName('open');
        const hookOpen = (pathArgIdx, fn, name) => {
            Interceptor.attach(fn, {
                onEnter(args) {
                    try {
                        const p = args[pathArgIdx].readCString();
                        if (p && (p.indexOf('metadata') >= 0 || p.indexOf('GG_') >= 0 ||
                                  p.indexOf('hotfix') >= 0 || p.indexOf('il2cpp') >= 0)) {
                            const bt = Thread.backtrace(this.context, Backtracer.FUZZY)
                                .slice(0, 8).map(a => addrLoc(a));
                            LOG({ type: 'open', fn: name, path: p, bt: bt });
                        }
                    } catch (e) {}
                }
            });
        };
        if (openat) hookOpen(1, openat, 'openat');
        if (open) hookOpen(0, open, 'open');
        LOG({ type: 'info', msg: 'open hooks installed' });
    } catch (e) { LOG({ type: 'err', msg: 'open hook: ' + e }); }
    return true;
}

// ---------- 阶段 2：init 完成后的被动采集 ----------
function latePass() {
    // 0x54bc070 槽位现值
    try {
        const il = Process.getModuleByName('libil2cpp.so');
        const slot = il.base.add(0x54bc070).readPointer();
        LOG({ type: 'slot', val: slot.toString(), where: slot.isNull() ? 'null' : addrLoc(slot) });
    } catch (e) { LOG({ type: 'err', msg: 'slot: ' + e }); }

    // 字符串扫描
    const pats = [
        '67 6c 6f 62 61 6c 2d 6d 65 74 61 64 61 74 61 2e 64 61 74', // "global-metadata.dat"
        '67 6c 6f 62 61 6c 2d 6d 65 74 61 64 61 74 61',             // "global-metadata"
        '47 47 5f',                                                  // "GG_"
        '68 6f 74 66 69 78',                                         // "hotfix"
        '48 79 62 72 69 64 43 4c 52'                                 // "HybridCLR"
    ];
    scanRanges(pats, 'late');

    // FAB11BAF 元数据映射头（全部命中 + ver 字段 + 头部 dump）
    try {
        const hits = [];
        const mods = ['libil2cpp.so', 'libcybrex.so', 'libmain.so'];
        for (const mn of mods) {
            const m = Process.getModuleByName(mn);
            for (const r of m.enumerateRanges('r--')) {
                try {
                    for (const f of Memory.scanSync(r.base, r.size, 'fa 1b b1 fa')) {
                        const ver = f.address.add(4).readU32();
                        hits.push({ where: addrLoc(f.address), ver: ver, head: hexHead(f.address, 96) });
                    }
                } catch (e) {}
            }
        }
        LOG({ type: 'metahits', hits: hits.slice(0, 6) });
    } catch (e) { LOG({ type: 'err', msg: 'metahits: ' + e }); }

    // libcybrex 内存镜像（检查自解密）
    try {
        const cb = Process.getModuleByName('libcybrex.so');
        let idx = 0;
        for (const r of cb.enumerateRanges('r--').concat(cb.enumerateRanges('rw-'))) {
            const off = r.base.sub(cb.base).toInt32();
            const size = r.size;
            const CH = 512 * 1024;
            for (let o = 0; o < size; o += CH) {
                const n = Math.min(CH, size - o);
                try {
                    send({ type: 'cxdump', off: off + o, total: size, idx: idx++ }, r.base.add(o).readByteArray(n));
                } catch (e) {}
            }
        }
        LOG({ type: 'info', msg: 'cybrex dump sent, chunks=' + idx });
    } catch (e) { LOG({ type: 'err', msg: 'cxdump: ' + e }); }

    LOG({ type: 'done' });
}

let earlyTries = 0;
const earlyTimer = setInterval(() => {
    earlyTries++;
    if (installEarlyHooks()) {
        clearInterval(earlyTimer);
        // libil2cpp 已加载 → 等 init 走完再做 late pass
        setTimeout(latePass, 20000);
        setTimeout(latePass2, 40000);
    } else if (earlyTries > 240) {
        clearInterval(earlyTimer);
        LOG({ type: 'err', msg: 'libil2cpp 未出现' });
    }
}, 500);

function latePass2() {
    // 二次槽位检查（有些游戏热更加载晚）
    try {
        const il = Process.getModuleByName('libil2cpp.so');
        const slot = il.base.add(0x54bc070).readPointer();
        LOG({ type: 'slot2', val: slot.toString(), where: slot.isNull() ? 'null' : addrLoc(slot) });
    } catch (e) {}
}
