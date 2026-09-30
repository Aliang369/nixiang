// 诊断：打印 InMemoryDexFile cookie 指向的内存布局，判断真实 DEX 指针位置 /
// 是否为 CDEX 格式（Android 12+ InMemoryDexFile 可能用 cdex001）。
Java.perform(function () {
    function hex64(v) { return "0x" + BigInt.asUintN(64, BigInt(v)).toString(16); }
    function peek(p, n) {
        try { return new Uint8Array(p.readByteArray(n)); } catch (e) { return null; }
    }
    function ascii(b) {
        var s = "";
        for (var i = 0; i < b.length; i++) s += (b[i] >= 32 && b[i] < 127) ? String.fromCharCode(b[i]) : ".";
        return s;
    }
    function scanFor(p, span) {
        var b = peek(p, span);
        if (!b) return null;
        var pats = [[0x64,0x65,0x78,0x0a],[0x63,0x64,0x65,0x78],[0x64,0x65,0x79,0x0a]];
        for (var i = 0; i + 4 <= b.length; i++) {
            for (var k = 0; k < pats.length; k++) {
                var m = pats[k], ok = true;
                for (var j = 0; j < 4; j++) if (b[i + j] !== m[j]) { ok = false; break; }
                if (ok) return i;
            }
        }
        return -1;
    }

    var loaders = [];
    Java.enumerateClassLoaders({
        onMatch: function (l) { loaders.push(l); },
        onComplete: function () {
            var cookieField = Java.use("dalvik.system.DexFile").class.getDeclaredField("mCookie");
            cookieField.setAccessible(true);
            var Arrays = Java.use("java.util.Arrays");
            for (var li = 0; li < loaders.length; li++) {
                var baseLoader;
                try { baseLoader = Java.cast(loaders[li], Java.use("dalvik.system.BaseDexClassLoader")); } catch (e) { continue; }
                var elements = baseLoader.pathList.value.dexElements.value;
                for (var ei = 0; ei < elements.length; ei++) {
                    var dexFile = elements[ei].dexFile.value;
                    var label = String(dexFile);
                    if (label.indexOf("InMemoryDexFile") !== 0) continue;
                    var cookie = cookieField.get(dexFile);
                    var text = Arrays.toString.overload("[J").call(Arrays, cookie);
                    var vals = text.replace(/\[|\]/g, "").split(",");
                    console.log("===== L" + li + " e" + ei + " cookie=" + text);
                    for (var vi = 0; vi < vals.length; vi++) {
                        var v = BigInt(vals[vi].trim());
                        var p = ptr(hex64(v));
                        if (p.isNull()) { console.log("  [" + vi + "] null"); continue; }
                        console.log("  [" + vi + "] " + p);
                        var head = peek(p, 64);
                        if (head) console.log("       head: " + ascii(head));
                        // 跟着第一级指针走
                        for (var off = 0; off < 0x60; off += 8) {
                            var q;
                            try { q = p.add(off).readPointer(); } catch (e) { continue; }
                            if (q.isNull() || q.compare(ptr("0x1000")) < 0) continue;
                            var m = scanFor(q, 0x40);
                            if (m !== -1) {
                                var hb = peek(q, 16);
                                console.log("       +0x" + off.toString(16) + " -> " + q + " magic@" + m + " '" + ascii(hb) + "'");
                            }
                        }
                    }
                }
            }
        }
    });
});
