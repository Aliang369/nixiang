// 动物大联盟：只读打印应用 SharedPreferences，确认 isFirstStart 实际状态。
Java.perform(function () {
    var ActivityThread = Java.use("android.app.ActivityThread");
    var app = ActivityThread.currentApplication();
    if (app === null) {
        console.log("[dwdlm-prefs] application null");
        return;
    }
    var ctx = app.getApplicationContext();
    var File = Java.use("java.io.File");
    var parent = ctx.getFilesDir().getParentFile();
    var prefsDir = File.$new(parent, "shared_prefs");
    var names = prefsDir.list();
    if (names === null) {
        console.log("[dwdlm-prefs] no shared_prefs dir");
        return;
    }
    for (var i = 0; i < names.length; i++) {
        var file = String(names[i]);
        var name = file.replace(/\.xml$/, "");
        try {
            var prefs = ctx.getSharedPreferences(name, 0);
            var all = prefs.getAll();
            var it = all.keySet().iterator();
            while (it.hasNext()) {
                var key = it.next();
                console.log("[dwdlm-prefs] " + file + " " + key + "=" + all.get(key));
            }
        } catch (e) {
            console.log("[dwdlm-prefs] read failed " + file + ": " + e);
        }
    }
});
