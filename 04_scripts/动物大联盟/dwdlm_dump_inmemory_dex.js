// 动物大联盟 1.0.23：只读导出 APKSecure InMemoryDexFile，供更新器逻辑分析。
// 用法：frida -D f9a116a2 -p <pid> -q -l 04_scripts/动物大联盟/dwdlm_dump_inmemory_dex.js
Java.perform(function () {
    var outDir = "/sdcard/Android/data/com.fightingstars.games.animal/files/dwdlm_dump";
    Java.use("java.io.File").$new(outDir).mkdirs();

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

    function dumpOne(loaderIndex, elementIndex, cookieValue) {
        var cookieBase = ptr(hex64(cookieValue));
        var dexPtr = null;
        try {
            if (new Uint8Array(cookieBase.readByteArray(8))[0] === 0x64) dexPtr = cookieBase;
        } catch (e) {}
        for (var off = 0; dexPtr === null && off < 0x100; off += 8) {
            var p;
            try {
                p = cookieBase.add(off).readPointer();
            } catch (e) {
                continue;
            }
            try {
                var h = new Uint8Array(p.readByteArray(8));
                if (h[0] === 0x64 && h[1] === 0x65 && h[2] === 0x78 && h[3] === 0x0a &&
                    h[4] === 0x30 && h[5] === 0x33 && h[6] === 0x35 && h[7] === 0x00) {
                    dexPtr = p;
                    break;
                }
            } catch (e) {}
        }
        if (dexPtr === null) {
            console.log("[dwdlm] no dex header for cookie=" + cookieBase);
            return;
        }

        var header = new Uint8Array(dexPtr.readByteArray(0x40));
        var view = new DataView(header.buffer);
        var size = view.getUint32(0x20, true);
        if (!size || size < 0x70 || size > 64 * 1024 * 1024) return;

        var key = dexPtr.toString();
        if (seen[key]) return;
        seen[key] = true;

        var name = "inmemory_L" + loaderIndex + "_e" + elementIndex + "_" + size + ".dex";
        writeBytes(name, dexPtr.readByteArray(size));
        count++;
        console.log("[dwdlm] wrote " + name + " base=" + dexPtr + " size=" + size);
    }

    var loaders = [];
    Java.enumerateClassLoaders({
        onMatch: function (loader) {
            loaders.push(loader);
        },
        onComplete: function () {
            var dl = Java.use("dalvik.system.DexFile");
            var f = dl.class.getDeclaredField("mCookie");
            f.setAccessible(true);
            var Arrays = Java.use("java.util.Arrays");

            for (var li = 0; li < loaders.length; li++) {
                var loader = loaders[li];
                try {
                    loader.loadClass("com.bingo.cocos.bridge.BingoApplication");
                } catch (e) {
                    continue;
                }
                console.log("[dwdlm] business loader[" + li + "]=" + loader);
                var baseLoader = Java.cast(loader, Java.use("dalvik.system.BaseDexClassLoader"));
                var elements = baseLoader.pathList.value.dexElements.value;
                for (var ei = 0; ei < elements.length; ei++) {
                    var dexFile = elements[ei].dexFile.value;
                    var label = String(dexFile);
                    console.log("[dwdlm]   element[" + ei + "]=" + label);
                    if (label.indexOf("InMemoryDexFile") !== 0) continue;
                    var cookie = f.get(dexFile);
                    var text = Arrays.toString.overload("[J").call(Arrays, cookie);
                    var values = text.replace(/\[|\]/g, "").split(",");
                    console.log("[dwdlm]   cookie=" + text);
                    for (var ci = 0; ci < values.length; ci++) {
                        dumpOne(li, ei, values[ci].trim());
                    }
                }
            }
            console.log("[dwdlm] dumped_count=" + count);
        }
    });
});
