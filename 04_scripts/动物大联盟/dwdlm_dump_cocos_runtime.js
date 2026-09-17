// 动物大联盟：只读导出运行时已解密的 libcocos2djs.so 可读段。
// 用法：frida -D f9a116a2 -p <pid> -q -t 12 -l 04_scripts/动物大联盟/dwdlm_dump_cocos_runtime.js
Java.perform(function () {
    var outDir = "/sdcard/Android/data/com.fightingstars.games.animal/files/dwdlm_dump/cocos";
    Java.use("java.io.File").$new(outDir).mkdirs();

    var mod = Process.findModuleByName("libcocos2djs.so");
    if (mod === null) {
        console.log("[dwdlm] libcocos2djs.so not found");
        return;
    }

    var records = [];
    var all = Process.enumerateRanges({ protection: "r--", coalesce: false });
    var ranges = [];
    for (var i = 0; i < all.length; i++) {
        var r = all[i];
        var rel = r.base.sub(mod.base);
        if (rel.compare(0) >= 0 && rel.compare(mod.size) < 0) ranges.push(r);
    }
    console.log("[dwdlm] module base=" + mod.base + " size=" + mod.size + " ranges=" + ranges.length);
    for (var i = 0; i < ranges.length; i++) {
        var r = ranges[i];
        var rel = r.base.sub(mod.base);
        var data;
        try {
            data = r.base.readByteArray(r.size);
        } catch (e) {
            console.log("[dwdlm] range read failed " + r.base + " size=" + r.size + ": " + e);
            continue;
        }
        var name = "range_" + rel.toString().replace("0x", "") + "_" + r.size.toString(16) + "_" + r.protection.replace(/-/g, "") + ".bin";
        var f = new File(outDir + "/" + name, "wb");
        f.write(data);
        f.close();
        records.push({ base: r.base.toString(), size: r.size, offset: rel.toString(), protection: r.protection, file: name });
    }

    var meta = new File(outDir + "/module.json", "w");
    meta.write(JSON.stringify({ base: mod.base.toString(), size: mod.size, ranges: records }));
    meta.close();
    console.log("[dwdlm] cocos ranges dumped=" + records.length + " dir=" + outDir);
});
