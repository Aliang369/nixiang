// 探针：libil2cpp.so 运行时内存 vs 文件内容，判断易盾是否对 SO 做加密/内存解密。
// 输出：各采样 offset 的内存字节与哈希，配合本地脚本与文件比对。
Java.perform(function () {
    var m = Process.findModuleByName("libil2cpp.so");
    if (m === null) { console.log("[slcb] libil2cpp.so not loaded"); return; }
    console.log("[slcb] libil2cpp base=" + m.base + " size=" + m.size + " path=" + m.path);
    var offs = [0x0, 0x1c8, 0x1000, 0x100000, 0x400000, 0x800000, 0xc00000, 0x1000000,
                0x1400000, 0x2000000, 0x3000000, 0x3993000, 0x3aa6a50, 0x3acb350];
    offs.forEach(function (o) {
        var p = m.base.add(o);
        var b;
        try { b = new Uint8Array(p.readByteArray(32)); } catch (e) { console.log("  " + o + " unreadable"); return; }
        var hex = "";
        for (var i = 0; i < b.length; i++) hex += ("0" + b[i].toString(16)).slice(-2);
        console.log("  mem[0x" + o.toString(16) + "] " + hex);
    });
});
