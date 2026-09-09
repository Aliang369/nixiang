// 探测 YooAsset buildin 判定链 (月圆之夜, frida 17 API)
//   1) 找 YooAsset.HostPlayModeImpl, 枚举字段名+偏移
//   2) 挂 IsBuildinPackageBundle: 记录判定返回 + 动态发现 QueryStreamingAssets 实现类
//   3) 挂实现的 QueryStreamingAssets: (pkg, fileName) -> bool
// 用法: frida -D <dev> -f com.ztgame.yyzy -l probe_buildin.js
'use strict';
const LIB = 'libil2cpp.so';
const FN = {};

function cstr(p) { return p.isNull() ? 'null' : Memory.readUtf8String(p); }
function allocS(s) { return Memory.allocUtf8String(s); }
function alloc(sz) { return Memory.alloc(sz || 8); }

function classFullName(k) {
    const ns = cstr(FN.il2cpp_class_get_namespace(k));
    const nm = cstr(FN.il2cpp_class_get_name(k));
    return (ns && ns !== 'null' ? ns + '.' : '') + nm;
}

function findClassAllImages(nsName, clsName) {
    const sizePtr = alloc();
    const arr = FN.il2cpp_domain_get_assemblies(FN.il2cpp_domain_get(), sizePtr);
    const cnt = Memory.readS32(sizePtr);
    for (let i = 0; i < cnt; i++) {
        const asm = Memory.readPointer(arr.add(i * Process.pointerSize));
        const img = FN.il2cpp_assembly_get_image(asm);
        const klass = FN.il2cpp_class_from_name(img, allocS(nsName), allocS(clsName));
        if (!klass.isNull()) return klass;
    }
    return null;
}

function waitForModule(cb, tries) {
    const mod = Process.findModuleByName(LIB);
    if (mod) return cb(mod);
    if (tries > 600) { console.log('[fatal] 等待', LIB, '超时'); return; }
    setTimeout(() => waitForModule(cb, tries + 1), 100);
}

function setup(mod) {
    const defs = [
        ['il2cpp_domain_get', 'pointer', []],
        ['il2cpp_domain_get_assemblies', 'pointer', ['pointer', 'pointer']],
        ['il2cpp_assembly_get_image', 'pointer', ['pointer']],
        ['il2cpp_class_from_name', 'pointer', ['pointer', 'pointer', 'pointer']],
        ['il2cpp_class_get_method_from_name', 'pointer', ['pointer', 'pointer', 'int']],
        ['il2cpp_class_get_name', 'pointer', ['pointer']],
        ['il2cpp_class_get_namespace', 'pointer', ['pointer']],
        ['il2cpp_object_get_class', 'pointer', ['pointer']],
        ['il2cpp_string_chars', 'pointer', ['pointer']],
        ['il2cpp_thread_attach', 'pointer', ['pointer']],
    ];
    FN.defs = defs;
    FN.mod = mod;
    tryResolveExports(0);
}

// il2cpp 符号可见性随 MHP 装载变化, 轮询直到 domain_get 可解析
function tryResolveExports(tries) {
    if (tries > 900) { console.log('[fatal] 导出解析超时'); return; }
    let ok = 0;
    FN.defs = FN.defs.filter(([nm, ret, args]) => {
        if (FN[nm]) { ok++; return false; }
        const a = FN.mod.findExportByName(nm);
        if (a) { FN[nm] = new NativeFunction(a, ret, args); ok++; return false; }
        return true;
    });
    if (FN.il2cpp_domain_get) {
        console.log('[st] exports ok:', ok, '(剩余未解析:', FN.defs.length, ')');
        waitDomain(0);
        return;
    }
    setTimeout(() => tryResolveExports(tries + 1), 300);
}

function waitDomain(tries) {
    let d = null;
    try { d = FN.il2cpp_domain_get(); } catch (e) { d = null; }
    if (d && !d.isNull()) return main(d);
    if (tries > 900) { console.log('[fatal] domain 超时'); return; }
    setTimeout(() => waitDomain(tries + 1), 100);
}

function main(domain) {
    FN.il2cpp_thread_attach(domain);
    const k = findClassAllImages('YooAsset', 'HostPlayModeImpl');
    if (!k) { console.log('[fatal] HostPlayModeImpl 未找到'); return; }
    console.log('[+] HostPlayModeImpl', k);

    const mm = FN.il2cpp_class_get_method_from_name(k, allocS('IsBuildinPackageBundle'), 1);
    if (mm.isNull()) { console.log('[fatal] IsBuildinPackageBundle 未找到'); return; }
    const mptr = Memory.readPointer(mm.add(Process.pointerSize));
    console.log('[+] IsBuildinPackageBundle ptr', mptr);

    // 尝试用盲扫定位 services 对象: IL 显示 4 个引用字段连续 (packageName/buildin/delivery/remote)
    function probeInstance(self) {
        const found = [];
        for (let off = 0x08; off <= 0x48; off += 8) {
            try {
                const p = Memory.readPointer(self.add(off));
                if (p.isNull() || p.compare(0x10000) < 0) continue;
                const cls = FN.il2cpp_object_get_class(p);
                if (cls.isNull()) continue;
                const nm = classFullName(cls);
                if (nm === 'null' || nm.length === 0 || nm.length > 120) continue;
                found.push([off, nm, cls]);
            } catch (e) { /* 跳过非法指针 */ }
        }
        return found;
    }

    let phase = 0;
    Interceptor.attach(mptr, {
        onEnter: function (args) { this.self = args[0]; },
        onLeave: function (ret) {
            console.log('[IsBuildinPackageBundle] ->', ret.toInt32());
            if (phase !== 0) return;
            phase = 1;
            try {
                const fields = probeInstance(this.self);
                for (const [off, nm, cls] of fields)
                    console.log('    inst ptr @' + off.toString(16) + ' =', nm);
                // 找有 QueryStreamingAssets 方法的字段
                for (const [off, nm, cls] of fields) {
                    const qm = FN.il2cpp_class_get_method_from_name(cls, allocS('QueryStreamingAssets'), 2);
                    if (!qm.isNull()) {
                        const qptr = Memory.readPointer(qm.add(Process.pointerSize));
                        console.log('[+] impl @' + off.toString(16), nm, 'QueryStreamingAssets ptr', qptr);
                        Interceptor.attach(qptr, {
                            onEnter: function (a2) {
                                this.pkg = cstr(FN.il2cpp_string_chars(a2[1]));
                                this.fn = cstr(FN.il2cpp_string_chars(a2[2]));
                            },
                            onLeave: function (r2) {
                                console.log(`[QueryStreamingAssets] pkg=${this.pkg} file=${this.fn} -> ${r2.toInt32()}`);
                            }
                        });
                    }
                }
            } catch (e) { console.log('[err]', e); }
        }
    });
    console.log('[ok] hook 挂好, 等待判定调用...');
}

waitForModule(setup, 0);
