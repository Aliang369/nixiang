// 我的勇者 (com.rsg.wdyz) — 被动验证: RVA 落点核对 + il2cpp 结构确认
// 用法: frida -U -p <pid> -l verify_wdyz.js
"use strict";

const RVAS = [
    ["HeroCore.SettleDamage", 0x45ee170],
    ["HeroGroup.AddFlag", 0x45f1e64],
    ["HeroGroup.subFlag", 0x45f1e94],
    ["MonsterCore.SettleDamage", 0x45f8524],
    ["SKillBase.TryEnterCd", 0x4668ec8],
    ["SKillBase.FixedCDCurtail", 0x4669640],
    ["HeroCore.settleMp", 0x45ee2b4],
    ["BattleManager.GetMyHero", 0x45f9a3c],
    ["TestDungeonManager.OnInvincible", 0x364e0fc],
    ["BattleObjectCore.SettleDamage", 0x45ae26c],
];

function main() {
    let m;
    try {
        m = Process.getModuleByName("libil2cpp.so");
    } catch (e) {
        console.log("[!] libil2cpp.so not loaded yet: " + e);
        return;
    }
    console.log("[*] libil2cpp base=" + m.base + " size=" + m.size);

    // 1) RVA 落点指令核对
    for (const [name, rva] of RVAS) {
        try {
            const addr = m.base.add(rva);
            const b = new Uint8Array(addr.readByteArray(16));
            let hex = "";
            for (let i = 0; i < 16; i++) hex += ("0" + b[i].toString(16)).slice(-2) + " ";
            // 粗判 prologue: stp x29,x30 [..-..] = a9 xx 7b fd; 或 pacibsp d503233f; 或 sub sp d10...
            const w = (b[0] | b[1] << 8 | b[2] << 16 | b[3] << 24) >>> 0;
            let tag = "";
            if (w === 0xd503233f) tag = "pacibsp";
            else if ((w & 0xffff) === 0x7bfd) tag = "stp x29,x30";
            else if ((w & 0xffe00000) === 0xd1000000) tag = "sub sp";
            else if (w === 0xd65f03c0) tag = "ret(?)";
            console.log("[rva] " + name + " @" + addr + " : " + hex + (tag ? "  <" + tag + ">" : ""));
        } catch (e) {
            console.log("[rva] " + name + " @0x" + rva.toString(16) + " READ FAIL: " + e);
        }
    }

    // 2) il2cpp 导出与域枚举
    const ex = (n) => {
        try { return Process.getModuleByName("libil2cpp.so").findExportByName(n); } catch (e) { return null; }
    };
    const names = ["il2cpp_domain_get", "il2cpp_domain_get_assemblies", "il2cpp_assembly_get_image",
                   "il2cpp_image_get_name", "il2cpp_class_from_name", "il2cpp_string_new", "il2cpp_init"];
    for (const n of names) console.log("[exp] " + n + " = " + ex(n));

    const dg = ex("il2cpp_domain_get");
    const ga = ex("il2cpp_domain_get_assemblies");
    const gi = ex("il2cpp_assembly_get_image");
    const gn = ex("il2cpp_image_get_name");
    if (dg && ga && gi && gn) {
        try {
            const domain = new NativeFunction(dg, "pointer", [])();
            const szp = Memory.alloc(8);
            const arr = new NativeFunction(ga, "pointer", ["pointer", "pointer"])(domain, szp);
            const count = szp.readULong();
            const getName = new NativeFunction(gn, "pointer", ["pointer"]);
            const getImg = new NativeFunction(gi, "pointer", ["pointer"]);
            console.log("[*] domain=" + domain + " assemblies=" + count);
            const imgNames = [];
            for (let i = 0; i < count; i++) {
                const asm = arr.add(i * Process.pointerSize).readPointer();
                if (asm.isNull()) continue;
                const img = getImg(asm);
                if (img.isNull()) continue;
                const nm = getName(img);
                if (!nm.isNull()) imgNames.push(nm.readUtf8String());
            }
            console.log("[*] images(" + imgNames.length + "): " + imgNames.slice(0, 60).join(", "));
        } catch (e) {
            console.log("[!] domain enum err: " + e);
        }
    }
    console.log("[*] DONE");
}

setImmediate(main);
