// 动物大联盟：将 GAME_FIRST_START.FIRST_START 恢复为 true，其余偏好不动。
Java.perform(function () {
    var ActivityThread = Java.use("android.app.ActivityThread");
    var app = ActivityThread.currentApplication();
    if (app === null) {
        console.log("[dwdlm-first-start] application null");
        return;
    }
    var prefs = app.getApplicationContext().getSharedPreferences("GAME_FIRST_START", 0);
    var ok = prefs.edit().putBoolean("FIRST_START", true).commit();
    console.log("[dwdlm-first-start] set FIRST_START=true commit=" + ok);
});
