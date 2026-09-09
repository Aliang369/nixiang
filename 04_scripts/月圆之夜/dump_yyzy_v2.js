// 月圆之夜 v2 — BSJB 直接扫托管映像 + ver24 metadata 特征 + il2cpp 程序集枚举
// 用法: frida -U -p <pid> -l dump_yyzy_v2.js
"use strict";

const OUT_DIR = "/data/local/tmp/yyzy_dump";

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
        if (fp.isNull()) { console.log("[!] fopen fail(app uid): " + name); return; }
        fwrite(addr, 1, len, fp);
        fclose(fp);
        console.log("[+] " + name + " (" + len + "B)");
    } catch (e) { console.log("[!] write " + name + ": " + e); }
}

function main() {
    const ranges = Process.enumerateRanges("r--").concat(Process.enumerateRanges("rw-"));
    console.log("[*] ranges=" + ranges.length);

    // 1) ver24 metadata 特征 (解密副本)
    let v24 = 0;
    for (const r of ranges) {
        if (r.size > 128 * 1024 * 1024) continue;
        let hits;
        try { hits = Memory.scanSync(r.base, r.size, "af 1b b1 fa 18 00 00 00"); } catch (e) { continue; }
        for (const h of hits) {
            v24++;
            console.log("[meta24] @" + h.address + " range=" + r.base + " size=" + r.size + " prot=" + r.protection + " file=" + (r.file ? r.file.path : "anon"));
            writeFile("meta24_" + h.address + ".bin", h.address, Math.min(r.size - Number(h.address.sub(r.base)), 32 * 1024 * 1024));
        }
    }
    console.log("[*] ver24 hits: " + v24);

    // 2) BSJB 扫描 → 回溯 MZ → 解析 PE 大小
    let bsjb = 0, dumped = 0;
    const seenRange = new Set();
    for (const r of ranges) {
        if (r.size < 100 * 1024 || r.size > 48 * 1024 * 1024) continue;
        if (r.file && r.file.path && (r.file.path.indexOf(".so") >= 0 || r.file.path.indexOf(".apk") >= 0 || r.file.path.indexOf(".dat") >= 0)) continue;
        let hits;
        try { hits = Memory.scanSync(r.base, r.size, "42 53 4a 42"); } catch (e) { continue; }
        if (!hits.length) continue;
        for (const h of hits) {
            bsjb++;
            try {
                // 回溯找 MZ (最多 4KB)
                let start = null;
                const back = Math.min(Number(h.address.sub(r.base)), 4096);
                const buf = new Uint8Array(h.address.sub(back).readByteArray(back + 256));
                for (let i = buf.length - 260; i >= 0; i--) {
                    if (buf[i] === 0x4d && buf[i + 1] === 0x5a) { start = h.address.sub(back - i); break; }
                }
                if (!start) { console.log("[bsjb] no MZ before @" + h.address); continue; }
                if (seenRange.has(start.toString())) continue;
                seenRange.add(start.toString());
                // PE header
                const peOff = start.add(0x3c).readU32();
                if (peOff <= 0 || peOff > 0x1000) continue;
                if (start.add(peOff).readU32() !== 0x00004550) continue;
                const nsec = start.add(peOff + 6).readU16();
                const optSize = start.add(peOff + 20).readU16();
                const secBase = peOff + 24 + optSize;
                let fileSize = 0;
                for (let s = 0; s < nsec; s++) {
                    const se = start.add(secBase + s * 40);
                    const rawSz = se.add(16).readU32();
                    const rawPtr = se.add(20).readU32();
                    if (rawSz && rawPtr && rawPtr + rawSz > fileSize) fileSize = rawPtr + rawSz;
                }
                if (fileSize <= 0 || fileSize > 48 * 1024 * 1024) fileSize = r.size - Number(start.sub(r.base));
                dumped++;
                console.log("[dll] MZ=" + start + " peOff=" + peOff + " nsec=" + nsec + " size=" + fileSize + " bsjbOff=" + Number(h.address.sub(start)));
                writeFile("dll_" + start + "_" + fileSize + ".bin", start, fileSize);
            } catch (e) { console.log("[!] bsjb parse @" + h.address + ": " + e); }
        }
    }
    console.log("[*] bsjb hits=" + bsjb + " dumped=" + dumped);

    // 3) il2cpp 程序集枚举 (导出 API)
    try {
        const il2cpp = Process.getModuleByName("libil2cpp.so");
        const names = ["il2cpp_domain_get", "il2cpp_domain_get_assemblies", "il2cpp_domain_assembly_open",
                       "il2cpp_assembly_get_image", "il2cpp_image_get_name", "il2cpp_image_get_assembly_count"];
        const fns = {};
        for (const n of names) {
            const p = Module.findExportByName("libil2cpp.so", n);
            fns[n] = p ? new NativeFunction(p, "pointer", []) : null;
        }
        console.log("[*] il2cpp exports present: " + names.filter(n => fns[n.replace(/assemblies|assembly_open|assembly_get_image|image_get_name|image_get_assembly_count/, m => m)]).length);
        // 简化: 只试 domain_get + assemblies
        const has = (n) => !!Module.findExportByName("libil2cpp.so", n);
        console.log("[*] domain_get=" + has("il2cpp_domain_get") + " get_assemblies=" + has("il2cpp_domain_get_assemblies") +
                    " assembly_open=" + has("il2cpp_domain_assembly_open") + " image_get_name=" + has("il2cpp_image_get_name"));
        if (has("il2cpp_domain_get") && has("il2cpp_domain_get_assemblies")) {
            const dg = new NativeFunction(Module.findExportByName("libil2cpp.so", "il2cpp_domain_get"), "pointer", []);
            const ga = new NativeFunction(Module.findExportByName("libil2cpp.so", "il2cpp_domain_get_assemblies"), "pointer", ["pointer", "pointer"]);
            const szp = Memory.alloc(8);
            const domain = dg();
            console.log("[*] domain=" + domain);
            if (!domain.isNull()) {
                const arr = ga(domain, szp);
                const count = szp.readUSize ? szp.readUSize() : szp.readULong();
                console.log("[*] assemblies count=" + count + " arr=" + arr);
                const countFn = Module.findExportByName("libil2cpp.so", "il2cpp_assembly_get_image");
                const getImg = countFn ? new NativeFunction(countFn, "pointer", ["pointer"]) : null;
                const getName = Module.findExportByName("libil2cpp.so", "il2cpp_image_get_name") ? new NativeFunction(Module.findExportByName("libil2cpp.so", "il2cpp_image_get_name"), "pointer", ["pointer"]) : null;
                let list = "";
                for (let i = 0; i < count && i < 200; i++) {
                    const asm = arr.add(i * Process.pointerSize).readPointer();
                    if (asm.isNull() || !getImg) continue;
                    const img = getImg(asm);
                    if (img.isNull() || !getName) continue;
                    const nm = getName(img).readUtf8String();
                    list += nm + "\n";
                }
                if (list) console.log("[*] images:\n" + list);
                writeFile("assemblies.txt", Memory.allocUtf8String(list), list.length);
            }
        }
    } catch (e) { console.log("[!] il2cpp enum err: " + e); }

    console.log("[*] DONE v2");
}

setImmediate(main);
