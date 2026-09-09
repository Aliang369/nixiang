// 月圆之夜 v4 — 全范围 BSJB 扫描 (含 GC 大段) + ver24 metadata
// 用法: frida -U -p <pid> -l dump_yyzy_v4.js
"use strict";

const OUT_DIR = "/data/data/com.ztgame.yyzy/files/yyzy_dump";

function getExport(name) {
    try { return Process.getModuleByName("libc.so").findExportByName(name); } catch (e) {}
    try { return Module.getGlobalExportByName(name); } catch (e) {}
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
        console.log("[+] " + name + " (" + len + "B)");
    } catch (e) { console.log("[!] write " + name + ": " + e); }
}

function main() {
    const ranges = Process.enumerateRanges("r--").concat(Process.enumerateRanges("rw-"));
    console.log("[*] ranges=" + ranges.length);

    // 1) ver24 metadata 特征
    let v24 = 0;
    for (const r of ranges) {
        let hits;
        try { hits = Memory.scanSync(r.base, r.size, "af 1b b1 fa 18 00 00 00"); } catch (e) { continue; }
        for (const h of hits) {
            v24++;
            console.log("[meta24] @" + h.address + " range=" + r.base + "/" + r.size + " prot=" + r.protection + " file=" + (r.file ? r.file.path : "anon"));
            writeFile("meta24_" + h.address + ".bin", h.address, Math.min(r.size - Number(h.address.sub(r.base)), 32 * 1024 * 1024));
        }
    }
    console.log("[*] ver24 hits: " + v24);

    // 2) BSJB 全范围
    let bsjb = 0, dumped = 0;
    const seenStart = new Set();
    for (const r of ranges) {
        if (r.size < 50 * 1024) continue;
        let hits;
        try { hits = Memory.scanSync(r.base, r.size, "42 53 4a 42"); } catch (e) { continue; }
        for (const h of hits) {
            bsjb++;
            if (bsjb <= 60) {
                console.log("[bsjb#" + bsjb + "] @" + h.address + " range=" + r.base + "/" + r.size + " prot=" + r.protection + " file=" + (r.file ? r.file.path : "anon"));
            }
            try {
                // 回溯 MZ
                let start = null;
                const back = Math.min(Number(h.address.sub(r.base)), 4096);
                const buf = new Uint8Array(h.address.sub(back).readByteArray(back + 256));
                for (let i = buf.length - 260; i >= 0; i--) {
                    if (buf[i] === 0x4d && buf[i + 1] === 0x5a) { start = h.address.sub(back - i); break; }
                }
                if (!start || seenStart.has(start.toString())) continue;
                seenStart.add(start.toString());
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
                if (fileSize <= 0 || fileSize > 256 * 1024 * 1024) continue;
                dumped++;
                console.log("  [dll] MZ=" + start + " pe=" + peOff + " nsec=" + nsec + " size=" + fileSize);
                writeFile("dll_" + start + "_" + fileSize + ".bin", start, fileSize);
            } catch (e) { if (bsjb <= 60) console.log("  parse err: " + e); }
        }
    }
    console.log("[*] bsjb hits=" + bsjb + " dumped=" + dumped);
    console.log("[*] DONE v4");
}

setImmediate(main);
