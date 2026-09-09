// 我的勇者 v4 — 枚举 class fields 核对偏移
"use strict";

const NULLPTR = ptr(0);

function main() {
    const mod = Process.getModuleByName("libil2cpp.so");
    const ex = (n) => { try { return mod.findExportByName(n); } catch (e) { return null; } };
    const f = {};
    for (const [k, n, ret, args] of [
        ["domain_get", "il2cpp_domain_get", "pointer", []],
        ["domain_get_assemblies", "il2cpp_domain_get_assemblies", "pointer", ["pointer", "pointer"]],
        ["assembly_get_image", "il2cpp_assembly_get_image", "pointer", ["pointer"]],
        ["image_get_name", "il2cpp_image_get_name", "pointer", ["pointer"]],
        ["class_from_name", "il2cpp_class_from_name", "pointer", ["pointer", "pointer", "pointer"]],
        ["class_get_fields", "il2cpp_class_get_fields", "pointer", ["pointer", "pointer"]],
        ["field_get_name", "il2cpp_field_get_name", "pointer", ["pointer"]],
        ["field_get_offset", "il2cpp_field_get_offset", "int", ["pointer"]],
    ]) {
        const p = ex(n);
        if (!p) { console.log("[!] missing export " + n); return; }
        f[k] = new NativeFunction(p, ret, args);
    }

    const szp = Memory.alloc(8);
    const arr = f.domain_get_assemblies(f.domain_get(), szp);
    const count = szp.readULong();
    let targetImg = null;
    for (let i = 0; i < count; i++) {
        const asm = arr.add(i * Process.pointerSize).readPointer();
        const img = f.assembly_get_image(asm);
        if (f.image_get_name(img).readUtf8String() === "Assembly-CSharp.dll") { targetImg = img; break; }
    }
    const S = (s) => Memory.allocUtf8String(s);

    const plan = [
        ["Battle", "HeroCore", ["hp", "_verifyAttr", "hpSyncDirty"]],
        ["Battle", "HeroGroup", ["isMyHero"]],
        ["Battle", "BattleObjectCore", ["BattleGroup"]],
        ["Battle.Skill", "SKillBase", ["maxCdTime", "CDTime", "cding", "_canUseCount", "BanUseByBuff"]],
        ["", "MBullet", ["damageRatio", "_damageRatio", "bulletId"]],
    ];
    for (const [ns, name, wanted] of plan) {
        const c = f.class_from_name(targetImg, S(ns), S(name));
        if (c.isNull()) { console.log("[cls] " + name + " NOT FOUND"); continue; }
        const iter = Memory.alloc(8);
        iter.writePointer(NULLPTR);
        const wantedSet = {};
        for (const w of wanted) wantedSet[w] = true;
        let total = 0;
        let fl = f.class_get_fields(c, iter);
        while (!fl.isNull()) {
            total++;
            const nm = f.field_get_name(fl).readUtf8String();
            if (wantedSet[nm] !== undefined) {
                const off = f.field_get_offset(fl);
                console.log("[f] " + name + "." + nm + " offset=0x" + (off >>> 0).toString(16));
            }
            fl = f.class_get_fields(c, iter);
        }
        console.log("[*] " + name + " total fields=" + total);
    }
    console.log("[*] DONE v4");
}
setImmediate(main);
