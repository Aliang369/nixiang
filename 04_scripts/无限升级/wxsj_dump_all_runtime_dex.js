// Dump every ART-loaded DEX reachable from Java ClassLoaders.
//
// Frida 17's Python binding can select V8 without actually exposing the Java
// bridge on this host. Run this through the Frida CLI instead:
//
//   frida -D <serial> -p <pid> --runtime=v8 -q -t 20 \
//     -l wxsj_dump_all_runtime_dex.js
//
// Files are written to the app's private files directory and pulled by root.

Java.perform(function () {
    var pkg = "com.xingzuodaoshi.infinityUpgrade";
    var outDir = "/data/user/0/" + pkg + "/files/wxsj_fulldex";
    Java.use("java.io.File").$new(outDir).mkdirs();

    var seenPointers = {};
    var records = [];
    var loadersSeen = 0;
    var elementsSeen = 0;

    function writeBytes(name, data) {
        var file = new File(outDir + "/" + name, "wb");
        file.write(data);
        file.close();
    }

    function writeText(name, text) {
        var file = new File(outDir + "/" + name, "w");
        file.write(text);
        file.close();
    }

    function hex64(value) {
        return "0x" + BigInt.asUintN(64, BigInt(value)).toString(16);
    }

    function findDexPointer(cookiePointer) {
        var cookieBase = ptr(cookiePointer);
        for (var offset = 8; offset < 0x80; offset += 8) {
            var candidate;
            try {
                candidate = cookieBase.add(offset).readPointer();
            } catch (e) {
                continue;
            }
            try {
                var magic = new Uint8Array(candidate.readByteArray(8));
                if (magic[0] === 0x64 && magic[1] === 0x65 &&
                    magic[2] === 0x78 && magic[3] === 0x0a &&
                    magic[4] === 0x30 && magic[5] === 0x33 &&
                    magic[6] === 0x35 && magic[7] === 0x00) {
                    return candidate;
                }
            } catch (e) {}
        }
        return null;
    }

    function dumpDex(loaderIndex, elementIndex, label, cookieValue) {
        var dexPointer = findDexPointer(hex64(cookieValue));
        if (dexPointer === null) {
            return false;
        }

        var pointerKey = dexPointer.toString();
        if (seenPointers[pointerKey]) {
            return false;
        }
        seenPointers[pointerKey] = true;

        var header = new Uint8Array(dexPointer.readByteArray(0x40));
        var size = new DataView(header.buffer).getUint32(0x20, true);
        if (!size || size < 0x70 || size > 256 * 1024 * 1024) {
            console.log("[wxsj-dump] bad size " + size + " at " + pointerKey);
            return false;
        }

        var ordinal = records.length + 1;
        var name = "runtime_" + ordinal + "_L" + loaderIndex +
            "_e" + elementIndex + "_" + size + ".dex";
        writeBytes(name, dexPointer.readByteArray(size));
        records.push({
            file: name,
            bytes: size,
            loader: loaderIndex,
            element: elementIndex,
            label: label,
            base: pointerKey
        });
        console.log("[wxsj-dump] wrote " + name + " base=" + pointerKey);
        return true;
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
            var BaseDex = Java.use("dalvik.system.BaseDexClassLoader");

            for (var li = 0; li < loaders.length; li++) {
                var baseLoader;
                try {
                    baseLoader = Java.cast(loaders[li], BaseDex);
                } catch (e) {
                    continue;
                }
                loadersSeen++;

                var elements;
                try {
                    elements = baseLoader.pathList.value.dexElements.value;
                } catch (e) {
                    continue;
                }

                for (var ei = 0; ei < elements.length; ei++) {
                    var dexFile;
                    try {
                        dexFile = elements[ei].dexFile.value;
                    } catch (e) {
                        continue;
                    }
                    if (dexFile === null) {
                        continue;
                    }
                    elementsSeen++;

                    var label;
                    try {
                        label = String(dexFile);
                    } catch (e) {
                        label = "<unprintable>";
                    }

                    var cookie;
                    try {
                        cookie = cookieField.get(dexFile);
                    } catch (e) {
                        continue;
                    }

                    var text;
                    try {
                        text = Arrays.toString.overload("[J").call(Arrays, cookie);
                    } catch (e) {
                        continue;
                    }
                    var values = text.replace(/\[|\]/g, "").split(",");
                    if (values.length !== 2) {
                        continue;
                    }
                    dumpDex(li, ei, label, values[1].trim());
                }
            }

            writeText("runtime_dex.json", JSON.stringify({
                package: pkg,
                loaders_seen: loadersSeen,
                elements_seen: elementsSeen,
                dex: records
            }, null, 2) + "\n");
            console.log("[wxsj-dump] done dex_files=" + records.length +
                " loaders=" + loadersSeen + " elements=" + elementsSeen);
        }
    });
});
