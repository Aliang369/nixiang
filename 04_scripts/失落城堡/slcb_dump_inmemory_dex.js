// 失落城堡 com.leiting.lostcastle.leiting：按 InMemoryDexFile cookie 精确 dump 真实 DEX。
// 运行： frida -D <serial> -p <pid> -q -l slcb_dump_inmemory_dex.js
// 输出写入应用私有目录 /data/user/0/<pkg>/files/slcb_dump，随后 root pull。
Java.perform(function () {
    var PKG = "com.leiting.lostcastle.leiting";
    var outDir = "/data/user/0/" + PKG + "/files/slcb_dump";
    Java.use("java.io.File").$new(outDir).mkdirs();
    console.log("[slcb] outDir=" + outDir);

    var seen = {};
    var count = 0;

    function hex64(v) {
        return "0x" + BigInt.asUintN(64, BigInt(v)).toString(16);
    }

    function writeBytes(name, data) {
        var f = new File(outDir + "/" + name, "wb");
        f.write(data);
        f.close();
    }

    function readCookiePtr(cookieValue) {
        var base = ptr(hex64(cookieValue));
        for (var off = 8; off < 0x80; off += 8) {
            var p;
            try { p = base.add(off).readPointer(); } catch (e) { continue; }
            try {
                var h = new Uint8Array(p.readByteArray(8));
                // 只锁 "dex\n" + "03x"（本机为 037），版本号不作固定
                if (h[0] === 0x64 && h[1] === 0x65 && h[2] === 0x78 && h[3] === 0x0a &&
                    h[4] === 0x30 && h[5] === 0x33) {
                    var end = String.fromCharCode(h[6], h[7]);
                    if (end === "5\u0000" || end === "6\u0000" || end === "7\u0000" || end === "8\u0000" || end === "9\u0000") return p;
                }
            } catch (e) {}
        }
        return null;
    }

    function dumpOne(li, ei, cookieValue) {
        var dexPtr = readCookiePtr(cookieValue);
        if (dexPtr === null) {
            console.log("[slcb] L" + li + " e" + ei + " no dex begin_");
            return;
        }
        var header = new Uint8Array(dexPtr.readByteArray(0x40));
        var size = new DataView(header.buffer).getUint32(0x20, true);
        if (!size || size < 0x70 || size > 128 * 1024 * 1024) return;
        var key = dexPtr.toString();
        if (seen[key]) return;
        seen[key] = true;
        var name = "inmemory_L" + li + "_e" + ei + "_" + size + ".dex";
        writeBytes(name, dexPtr.readByteArray(size));
        count++;
        console.log("[slcb] wrote " + name + " base=" + dexPtr + " size=" + size);
    }

    var loaders = [];
    Java.enumerateClassLoaders({
        onMatch: function (l) { loaders.push(l); },
        onComplete: function () {
            console.log("[slcb] loaders=" + loaders.length);
            var cookieField = Java.use("dalvik.system.DexFile").class.getDeclaredField("mCookie");
            cookieField.setAccessible(true);
            var Arrays = Java.use("java.util.Arrays");
            for (var li = 0; li < loaders.length; li++) {
                var loader = loaders[li];
                var baseLoader;
                try { baseLoader = Java.cast(loader, Java.use("dalvik.system.BaseDexClassLoader")); }
                catch (e) { continue; }
                var elements;
                try { elements = baseLoader.pathList.value.dexElements.value; }
                catch (e) { continue; }
                console.log("[slcb] loader[" + li + "] " + loader + " elements=" + elements.length);
                for (var ei = 0; ei < elements.length; ei++) {
                    var dexFile;
                    try { dexFile = elements[ei].dexFile.value; } catch (e) { continue; }
                    var label = String(dexFile);
                    var cookie, text, values;
                    try {
                        cookie = cookieField.get(dexFile);
                        text = Arrays.toString.overload("[J").call(Arrays, cookie);
                        values = text.replace(/\[|\]/g, "").split(",");
                    } catch (e) { continue; }
                    var extra = label.indexOf("InMemoryDexFile") === 0 ? "" : " (ondisk?)";
                    console.log("[slcb]   [" + ei + "] " + label + extra + " cookie=" + text);
                    if (values.length === 2) dumpOne(li, ei, values[1].trim());
                }
            }
            console.log("[slcb] dumped_count=" + count);
        }
    });
});
