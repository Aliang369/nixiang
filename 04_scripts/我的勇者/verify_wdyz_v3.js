// 我的勇者 v3 — 按真实命名空间解析战斗类 (Battle / Battle.Skill)
"use strict";

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
        ["field_from_name", "il2cpp_class_get_field_from_name", "pointer", ["pointer", "pointer"]],
        ["field_get_offset", "il2cpp_field_get_offset", "int", ["pointer"]],
        ["method_from_name", "il2cpp_class_get_method_from_name", "pointer", ["pointer", "pointer", "int"]],
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
        const nm = f.image_get_name(img).readUtf8String();
        if (nm === "Assembly-CSharp.dll") { targetImg = img; break; }
    }
    console.log("[*] Assembly-CSharp img=" + targetImg);

    const S = (s) => Memory.allocUtf8String(s);
    const clsName = (ns, name) => f.class_from_name(targetImg, S(ns), S(name));

    const plan = [
        ["HeroCore", "Battle", ["SettleDamage", "settleMp"], ["hp", "_verifyAttr"]],
        ["HeroGroup", "Battle", ["AddFlag", "subFlag"], ["isMyHero"]],
        ["MonsterCore", "Battle", ["SettleDamage", "GoDeath"], []],
        ["BattleObjectCore", "Battle", ["SettleDamage", "settleMp"], ["BattleGroup"]],
        ["BattleManager", "Battle", ["GetMyHero"], []],
        ["SKillBase", "Battle.Skill", ["TryEnterCd", "FixedCDCurtail"], ["CDTime", "cding", "maxCdTime"]],
        ["MBullet", "", [], ["damageRatio", "bulletId"]],
        ["TestDungeonManager", "", ["OnInvincible"], []],
        ["LHeroHitProcess", "Battle", ["CheckDeath"], []],
        ["LMonsterHitProcess", "Battle", ["OnHpEmpty"], []],
        ["DamageLimitStep", "Battle", [], []],
        ["BaseLHitProcess", "Battle", ["ProcessExpendMp"], []],
    ];
    for (const [name, ns, methods, fields] of plan) {
        let c = clsName(ns, name);
        if (c.isNull() && ns) c = clsName("", name);
        console.log("[cls] " + ns + "." + name + " = " + c + (c.isNull() ? " (NOT FOUND)" : " OK"));
        if (c.isNull()) continue;
        for (const mname of methods) {
            try {
                const mp = f.method_from_name(c, mname, -1);
                if (!mp.isNull()) {
                    const p = mp.readPointer();
                    const rva = Number(p.sub(mod.base));
                    console.log("    [m] " + name + "::" + mname + " fnRva=0x" + rva.toString(16));
                }
            } catch (e) {}
        }
        for (const fname of fields) {
            try {
                const fp = f.field_from_name(c, fname);
                if (!fp.isNull()) {
                    const off = f.field_get_offset(fp);
                    console.log("    [f] " + name + "." + fname + " offset=0x" + (off >>> 0).toString(16));
                }
            } catch (e) {}
        }
    }
    console.log("[*] DONE v3");
}

setImmediate(main);
