// 动物大联盟：短时记录启动链函数进入/返回，只读，不修改内存。
Java.perform(function () {
    var installed = false;
    var timer = setInterval(function () {
        if (installed) return;
        var mod = Process.findModuleByName("libcocos2djs.so");
        if (mod === null) return;
        installed = true;
        clearInterval(timer);
        console.log("[dwdlm-trace] module base=" + mod.base);
        var offsets = {
            0x1fcd5c: "onGameStart",
            0x237750: "getVersionInfo",
            0x23882c: "afterGetVersion",
            0x23ec48: "checkFiles",
            0x23f6dc: "checkFilesCallback",
            0x30c6a8: "invokeCallbacks",
            0x279e0c: "nativeInit",
            0x279f38: "bootEval",
            0x2edb00: "loadScriptFile",
            0x30c79c: "evalString"
        };
        Object.keys(offsets).forEach(function (key) {
            var off = parseInt(key, 16);
            var name = offsets[key];
            try {
                Interceptor.attach(mod.base.add(off), {
                    onEnter: function () { console.log("[dwdlm-trace] enter " + name + " @" + this.returnAddress); },
                    onLeave: function (retval) { console.log("[dwdlm-trace] leave " + name + " ret=" + retval); }
                });
                console.log("[dwdlm-trace] hooked " + name + " off=" + key);
            } catch (e) {
                console.log("[dwdlm-trace] hook failed " + name + " " + e);
            }
        });
    }, 20);
});
