// 月圆之夜 v3 — 早挂载: 钩 il2cpp_add_internal_call 抓 HybridCLR/AOT 解密后 DLL 字节
// 需在 il2cpp 初始化前 attach (重启游戏 + attach 重试循环)
// 产物: /data/data/com.ztgame.yyzy/files/yyzy_dump/
"use strict";

const OUT_DIR = "/data/data/com.ztgame.yyzy/files/yyzy_dump";
let dumpCount = 0;

function getExport(name) {
    try { return Module.getGlobalExportByName(name); } catch (e) {}
    try { return Module.getExportByName("libc.so", name); } catch (e) {}
    return null;
}
const fopen = new NativeFunction(getExport("fopen"), "pointer", ["pointer", "pointer"]);
const fwrite = new NativeFunction(getExport("fwrite"), "ulong", ["pointer", "ulong", "ulong", "pointer"]);
const fclose = new NativeFunction(getExport("fclose"), "int", ["pointer"]);

function writeFile(name, addr, len) {
    try {
        const fp = fopen(Memory.allocUtf8String(OUT_DIR + "/" + name + "\0"), Memory.allocUtf8String("wb\0"));
        if (fp.isNull()) { console.log("[!] fopen fail: " + name); return; }
        fwrite(addr, 1, len, fp);
        fclose(fp);
        dumpCount++;
        console.log("[+] dumped " + name + " (" + len + "B)");
    } catch (e) { console.log("[!] write " + name + ": " + e); }
}

function findExport(mod, name) {
    try { return Process.getModuleByName(mod).findExportByName(name); } catch (e) {}
    try { return Module.getGlobalExportByName(name); } catch (e) {}
    return null;
}

function main() {
    const addIcall = findExport("libil2cpp.so", "il2cpp_add_internal_call");
    console.log("[*] il2cpp_add_internal_call = " + addIcall);
    if (!addIcall) {
        console.log("[!] export missing — il2cpp already initialized or stripped");
        return;
    }
    Interceptor.attach(addIcall, {
        onEnter(args) {
            try {
                const name = args[0].readUtf8String();
                const ptr_ = args[1];
                if (name && /HybridCLR|Assembly::Load|MetadataForAOT|Assembly::LoadFrom/i.test(name)) {
                    console.log("[icall] " + name + " -> " + ptr_);
                    hookLoader(name, ptr_);
                }
            } catch (e) {}
        }
    });
    console.log("[*] il2cpp_add_internal_call hooked, waiting for icall registrations...");
}

const hooked = new Set();
function hookLoader(name, fnPtr) {
    if (hooked.has(fnPtr.toString())) return;
    hooked.add(fnPtr.toString());

    Interceptor.attach(fnPtr, {
        onEnter(args) {
            this.arr = args[0];
            try {
                // Il2CppArray 64bit: max_length@0x18, data@0x20
                const len = this.arr.add(0x18).readU64 ? this.arr.add(0x18).readU64().toNumber() : this.arr.add(0x18).readULong();
                this.len = len;
                this.data = this.arr.add(0x20);
                let head = "";
                try {
                    const h = new Uint8Array(this.data.readByteArray(8));
                    for (let i = 0; i < 8; i++) head += ("0" + h[i].toString(16)).slice(-2) + " ";
                } catch (e) {}
                console.log("[loader] " + name + " arr=" + this.arr + " len=" + len + " head: " + head);
            } catch (e) {
                console.log("[!] loader arg parse err: " + e + " (arg0 可能不是 Il2CppArray)");
                this.arr = null;
            }
        },
        onLeave(ret) {
            if (!this.arr) return;
            try {
                const len = this.len;
                if (len > 1024 && len < 64 * 1024 * 1024) {
                    dumpCount = dumpCount; // noop
                    const fname = "dll_" + Date.now() + "_" + len + ".bin";
                    writeFile(fname, this.data, len);
                    // 判定是否 PE
                    try {
                        const m0 = this.data.readU8();
                        const m1 = this.data.add(1).readU8();
                        console.log("    magic=" + m0.toString(16) + m1.toString(16) + (m0 === 0x4d && m1 === 0x5a ? " (PE!)" : ""));
                    } catch (e) {}
                }
            } catch (e) { console.log("[!] onLeave dump err: " + e); }
        }
    });
}

// 兜底: 若 attach 晚了, il2cpp_add_internal_call 可能已被全部调用
// 此时改为直接扫导出表/内存找 LoadFromBlob 不现实 — 打印提示
setImmediate(main);
