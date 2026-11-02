package com.starry.magic.cocos;

import android.content.ContentValues;
import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import java.io.File;

/**
 * 异世界当欧皇（com.cxgame.ysjdoh）：功能状态桥。
 *
 * 通道：游戏内 cc.sys.localStorage 落 databases/jsb.sqlite（表 data(key TEXT PRIMARY KEY, value TEXT)），
 * 这里写同一个库同一张表，key=starry_ysjdoh，游戏内补丁脚本每 3s 轮询。
 * JSON：{"mult":2..100,"inv":0/1,"spd":0/1,"noad":0/1}（mult<=1 视为关闭）
 *
 * 为什么不用 evalString / SharedPreferences：
 *  - Cocos 的 localStorage 不落 SharedPreferences（落 sqlite）；
 *  - native evalString 注入在部分机型/houdini 转译层 SIGSEGV（先例实测）；
 *  - 本包逻辑补丁已直接进 assets/assets/scripts/index.jsc，不需要运行时注入。
 */
public final class YsjdohHooks {
    private static final String TAG = "Ysjdoh";
    private static final String KEY = "starry_ysjdoh";
    private static final String DB = "jsb.sqlite";

    private static volatile int sMult = 1;
    private static volatile boolean sInv = false;
    private static volatile boolean sSpd = false;
    private static volatile boolean sNoad = false;

    private YsjdohHooks() {}

    /** 菜单点击：更新并落库。 */
    public static void setFeature(int index, boolean on) {
        switch (index) {
            case 1: sMult = on ? 2 : 1; break;
            case 2: sInv = on; break;
            case 3: sSpd = on; break;
            case 4: sNoad = on; break;
            default: return;
        }
        writeCfg();
    }

    /** 启动期状态同步：只更新内存，不落库（避免框架默认值覆盖玩家选择）。 */
    public static void syncFeature(int index, boolean on) {
        switch (index) {
            case 1: sMult = on ? 2 : 1; break;
            case 2: sInv = on; break;
            case 3: sSpd = on; break;
            case 4: sNoad = on; break;
            default: return;
        }
    }

    public static void setValue(int index, int value) {
        if (index == 1) sMult = clamp(value);
        writeCfg();
    }

    public static void syncValue(int index, int value) {
        if (index == 1) sMult = clamp(value);
    }

    private static int clamp(int v) {
        if (v < 1) return 1;
        if (v > 100) return 100;
        return v;
    }

    public static void clearAll() {
        sMult = 1; sInv = false; sSpd = false; sNoad = false;
        writeCfg();
    }

    public static void writeCfg() {
        SQLiteDatabase db = null;
        try {
            Object app = Class.forName("android.app.ActivityThread")
                    .getMethod("currentApplication").invoke(null);
            if (app == null) return;
            Context ctx = (Context) app;
            File f = ctx.getDatabasePath(DB);
            if (f == null || !f.exists()) {
                Log.w(TAG, "jsb.sqlite not found yet");
                return;
            }
            db = SQLiteDatabase.openDatabase(f.getPath(), null, SQLiteDatabase.OPEN_READWRITE);
            if (!db.isReadOnly()) {
                try {
                    db.execSQL("CREATE TABLE IF NOT EXISTS data(key TEXT PRIMARY KEY, value TEXT)");
                } catch (Throwable ignored) {}
            }
            String json = "{\"mult\":" + sMult + ",\"inv\":" + (sInv ? 1 : 0)
                    + ",\"spd\":" + (sSpd ? 1 : 0) + ",\"noad\":" + (sNoad ? 1 : 0) + "}";
            ContentValues cv = new ContentValues();
            cv.put("key", KEY);
            cv.put("value", json);
            long r = db.insertWithOnConflict("data", null, cv, SQLiteDatabase.CONFLICT_REPLACE);
            Log.i(TAG, "cfg -> " + json + " row=" + r);
        } catch (Throwable t) {
            Log.w(TAG, "writeCfg failed: " + t);
        } finally {
            if (db != null) try { db.close(); } catch (Throwable ignored) {}
        }
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }
}
