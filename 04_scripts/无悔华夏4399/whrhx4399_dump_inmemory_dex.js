// 4399 无悔华夏：按 InMemoryDexFile cookie 精确 dump 真实 DEX。
// 运行方式（phantom-frida + frida CLI）：
//   frida -D <serial> -p <pid> -q -l whrhx4399_dump_inmemory_dex.js
// 输出先写入应用私有目录，随后由本地脚本 root pull；脚本末尾不删除文件。
Java.perform(function () {
    var outDir = "/data/user/0/com.yhhy.whrhx.m4399/files/whrhx4399_dump";
    var mkdir = Java.use("java.io.File").$new(outDir).mkdirs();
    console.log("[whrhx4399] outDir=" + outDir + " mkdir=" + mkdir);

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
        for (var off = 8; off < 0x80; off += 8) {
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
        if (dexPtr === null) return;

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
        console.log("[whrhx4399] wrote " + name + " base=" + dexPtr + " size=" + size);
    }

    var loaders = [];
    Java.enumerateClassLoaders({
        onMatch: function (loader) {
            loaders.push(loader);
        },
        onComplete: function () {
            var dexFileClass = Java.use("dalvik.system.DexFile");
            var cookieField = dexFileClass.class.getDeclaredField("mCookie");
            cookieField.setAccessible(true);
            var Arrays = Java.use("java.util.Arrays");

            for (var li = 0; li < loaders.length; li++) {
                var loader = loaders[li];
                try {
                    loader.loadClass("com.tpf.sdk.TPFApplication");
                } catch (e) {
                    continue;
                }

                var baseLoader;
                try {
                    baseLoader = Java.cast(loader, Java.use("dalvik.system.BaseDexClassLoader"));
                } catch (e) {
                    continue;
                }

                var elements = baseLoader.pathList.value.dexElements.value;
                for (var ei = 0; ei < elements.length; ei++) {
                    var dexFile = elements[ei].dexFile.value;
                    var label = String(dexFile);
                    if (label.indexOf("InMemoryDexFile") !== 0) continue;
                    var cookie = cookieField.get(dexFile);
                    var text = Arrays.toString.overload("[J").call(Arrays, cookie);
                    var values = text.replace(/\[|\]/g, "").split(",");
                    if (values.length !== 2) continue;
                    dumpOne(li, ei, values[1].trim());
                }
            }
            console.log("[whrhx4399] dumped_count=" + count);
        }
    });
});
