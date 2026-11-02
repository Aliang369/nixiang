package com.starry.magic.cocos;

import android.content.ContentValues;
import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.util.Log;

import java.io.File;

/**
 * 不一样的修仙宗门2：功能状态桥。
 *
 * 通道：游戏内 cc.sys.localStorage 在 Android 上落在 databases/jsb.sqlite
 * （表 data(key TEXT PRIMARY KEY, value TEXT)），这里直接写同一个库同一张表，
 * 游戏内补丁脚本每 3s 读一次 key=starry_byydxxzm2 的 JSON。
 *
 * 为什么不用 evalString / SharedPreferences / fileUtils：
 *  - 本包只有 arm64 so，雷电靠 houdini 转译，Java 反射调 native evalString 会 SIGSEGV；
 *  - Cocos 的 localStorage 不落 SharedPreferences（落 sqlite）；
 *  - jsb.fileUtils 在引擎早期调用不稳。
 */
public final class Byydxxzm2Hooks {
    private static final String TAG = "Byydxxzm2";
    private static final String KEY = "starry_byydxxzm2";
    private static final String DB = "jsb.sqlite";

    private static volatile boolean sYu = true;
    private static volatile boolean sLing = true;
    private static volatile boolean sNoAd = true;

    private Byydxxzm2Hooks() {}

    /** 菜单点击：更新并落库，游戏内 3s 内生效。 */
    public static void setFeature(int index, boolean on) {
        switch (index) {
            case 1: sYu = on; break;
            case 2: sLing = on; break;
            case 3: sNoAd = on; break;
            default: return;
        }
        writeCfg();
    }

    /** 启动期状态同步：只更新内存，不落库（避免框架默认值覆盖玩家选择）。 */
    public static void syncFeature(int index, boolean on) {
        switch (index) {
            case 1: sYu = on; break;
            case 2: sLing = on; break;
            case 3: sNoAd = on; break;
            default: return;
        }
    }

    public static boolean getFeature(int index) {
        switch (index) {
            case 1: return sYu;
            case 2: return sLing;
            case 3: return sNoAd;
            default: return false;
        }
    }

    public static void clearAll() {
        sYu = false; sLing = false; sNoAd = false;
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
            String json = "{\"yu\":" + (sYu ? 1 : 0) + ",\"ling\":" + (sLing ? 1 : 0)
                    + ",\"noad\":" + (sNoAd ? 1 : 0) + "}";
            db = SQLiteDatabase.openDatabase(f.getPath(), null, SQLiteDatabase.OPEN_READWRITE);
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
