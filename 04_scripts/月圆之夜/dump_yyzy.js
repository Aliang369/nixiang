// 月圆之夜 (com.ztgame.yyzy) — 运行时内存扫描 dump
// 目标: ① 解密后的 global-metadata (magic FAB11BAF, 内存中真实版本应为 24.x)
//      ② HybridCLR 热更 DLL (MZ/PE + BSJB)
// 用法: frida -U -p <pid> -l dump_yyzy.js
// 产物: /data/local/tmp/yyzy_dump/

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
        if (fp.isNull()) { console.log("[!] fopen fail: " + name); return false; }
        fwrite(addr, 1, len, fp);
        fclose(fp);
        console.log("[+] dumped " + name + " (" + len + " bytes from " + addr + ")");
        return true;
    } catch (e) { console.log("[!] writeFile " + name + " err: " + e); return false; }
}

function main() {
    try {
        const mkdir = new NativeFunction(getExport("mkdir"), "int", ["pointer", "int"]);
        mkdir(Memory.allocUtf8String(OUT_DIR), 0x1ff);
    } catch (e) {}

    let il2cppBase = "n/a";
    try {
        const m = Process.getModuleByName("libil2cpp.so");
        il2cppBase = m.base + " size=" + m.size;
    } catch (e) {}
    console.log("[*] libil2cpp: " + il2cppBase + " pid=" + Process.id);

    // ---------- 1) metadata 扫描 ----------
    const ranges = Process.enumerateRanges("r--").concat(Process.enumerateRanges("rw-"));
    console.log("[*] total ranges: " + ranges.length);
    let metaHits = 0;
    for (const r of ranges) {
        // metadata mmap 通常 8MB~64MB
        if (r.size < 4 * 1024 * 1024 || r.size > 128 * 1024 * 1024) continue;
        let hits;
        try {
            hits = Memory.scanSync(r.base, Math.min(r.size, 64 * 1024 * 1024), "af 1b b1 fa");
        } catch (e) { continue; }
        for (const h of hits) {
            let ver = -1;
            try { ver = h.address.add(4).readU32(); } catch (e) {}
            if (ver < 20 || ver > 40) { console.log("[meta] magic@" + h.address + " ver=" + ver + " (skip)"); continue; }
            metaHits++;
            const name = "metadata_v" + ver + "_" + h.address + ".bin";
            const len = Math.min(r.size - Number(h.address.sub(r.base)), 64 * 1024 * 1024);
            console.log("[meta] FOUND magic@" + h.address + " ver=" + ver + " range=" + r.base + " size=" + r.size + " prot=" + r.protection + " file=" + (r.file ? r.file.path : "anon"));
            writeFile(name, h.address, len);
        }
    }
    console.log("[*] metadata hits: " + metaHits);

    // ---------- 2) 热更 DLL 扫描 (MZ + PE + BSJB) ----------
    let dllHits = 0;
    const dumped = new Set();
    for (const r of ranges) {
        if (r.size < 200 * 1024 || r.size > 32 * 1024 * 1024) continue;
        // 跳过已映射的 so 文件段
        if (r.file && r.file.path && r.file.path.indexOf(".so") >= 0) continue;
        let hits;
        try {
            hits = Memory.scanSync(r.base, r.size, "4d 5a 90 00"); // MZ\x90\x00 标准 DOS 头
        } catch (e) { continue; }
        for (const h of hits) {
            try {
                const peOff = h.address.add(0x3c).readU32();
                if (peOff <= 0 || peOff > 0x1000) continue;
                const peSig = h.address.add(peOff).readU32();
                if (peSig !== 0x00004550) continue; // PE\0\0
                // 找 BSJB (.NET 元数据) 在前 8KB 内
                let hasBsjb = false;
                const head = h.address.readByteArray(Math.min(8192, r.size - Number(h.address.sub(r.base))));
                const u8 = new Uint8Array(head);
                for (let i = 0; i + 4 <= u8.length; i++) {
                    if (u8[i] === 0x42 && u8[i + 1] === 0x53 && u8[i + 2] === 0x4a && u8[i + 3] === 0x42) { hasBsjb = true; break; }
                }
                if (!hasBsjb) continue;
                // PE 大小估算: 取 range 剩余或找 fat sections —— 简化: dump range 内从 MZ 到 range 末尾
                const dumpLen = r.size - Number(h.address.sub(r.base));
                const key = h.address.toString();
                if (dumped.has(key)) continue;
                dumped.add(key);
                dllHits++;
                const name = "dll_" + h.address + "_" + dumpLen + ".bin";
                console.log("[dll] FOUND PE@" + h.address + " len~" + dumpLen + " range=" + r.base + " prot=" + r.protection);
                writeFile(name, h.address, dumpLen);
            } catch (e) { continue; }
        }
    }
    console.log("[*] dll hits: " + dllHits);
    console.log("[*] DONE. ls " + OUT_DIR);
}

setImmediate(main);
