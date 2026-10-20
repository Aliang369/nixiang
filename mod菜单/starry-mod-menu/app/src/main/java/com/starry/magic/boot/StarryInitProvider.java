package com.starry.magic.boot;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;

import com.starry.magic.StarryApp;
import com.starry.magic.menu.MenuService;
import com.starry.magic.ota.StarryNativeLoad;

/**
 * 进程级早期初始化入口（无 UI）。
 * <p>
 * 为什么需要它：乐变 overlay 接管应用时不一定回调 {@code StarryApp.attachBaseContext/onCreate}
 * （2026-10-21 无悔华夏 B 站包实测：主进程没有 StarryApp 日志，但 Service 侧日志正常）。
 * ContentProvider 由 AMS 在 Application 之后、首个组件之前稳定创建，是唯一与「谁当 Application」
 * 无关的早期钩子；4399 同构包里也用它兜住乐变 overlay 的时序。
 * <p>
 * 语义：只做三件事 —— 主进程加载 native、按 StarryApp 既有节奏排一次 MenuService 启动兜底。
 * 全部幂等（StarryNativeLoad.isDone / MenuService.start 已有 running 判重）。
 */
public final class StarryInitProvider extends ContentProvider {
    private static final String TAG = "StarryInit";
    private static final long MENU_START_DELAY_MS = 4000L;

    private static void note(Context ctx, String msg) {
        try {
            File dir = ctx.getFilesDir();
            if (dir == null) return;
            File f = new File(dir, "starry_init.log");
            FileWriter w = new FileWriter(f, true);
            try {
                w.write((System.currentTimeMillis() / 1000L) + " " + msg + "\n");
            } finally {
                w.close();
            }
        } catch (Throwable ignored) {
        }
    }

    @Override
    public boolean onCreate() {
        Context ctx = getContext();
        try {
            Log.i(TAG, "provider onCreate proc=" + StarryApp.currentProcessName());
            if (ctx == null) return true;
            if (!StarryApp.isMainProcess(ctx)) {
                Log.i(TAG, "provider skip (secondary process)");
                return true;
            }
            note(ctx, "provider start proc=" + StarryApp.currentProcessName());
            if (!StarryNativeLoad.isDone()) {
                boolean loaded = StarryNativeLoad.ensureLoaded(ctx);
                Log.i(TAG, "provider ensureLoaded=" + loaded
                        + " path=" + StarryNativeLoad.loadedPath());
                note(ctx, "ensureLoaded=" + loaded + " path=" + StarryNativeLoad.loadedPath());
                if (!loaded) {
                    String dir = null;
                    String soPath = null;
                    try {
                        dir = ctx.getApplicationInfo().nativeLibraryDir;
                        File so = new File(dir, "libstarry_magic.so");
                        soPath = so.getAbsolutePath();
                        note(ctx, "nativeLibraryDir=" + dir + " soExists=" + so.isFile()
                                + " size=" + so.length());
                        System.load(soPath);
                        note(ctx, "manual System.load OK " + soPath);
                    } catch (Throwable t) {
                        note(ctx, "manual load failed dir=" + dir + " err=" + t);
                    }
                }
            } else {
                note(ctx, "already done path=" + StarryNativeLoad.loadedPath());
            }
            final Context app = ctx.getApplicationContext();
            if (app != null) {
                new Handler(Looper.getMainLooper()).postDelayed(() -> {
                    try {
                        MenuService.start(app);
                        Log.i(TAG, "provider MenuService.start ok");
                    } catch (Throwable t) {
                        Log.w(TAG, "provider MenuService.start failed: " + t);
                    }
                }, MENU_START_DELAY_MS);
            }
        } catch (Throwable t) {
            Log.e(TAG, "provider init failed", t);
        }
        return true;
    }

    @Override
    public Cursor query(Uri uri, String[] projection, String selection,
                        String[] selectionArgs, String sortOrder) {
        return null;
    }

    @Override
    public String getType(Uri uri) {
        return null;
    }

    @Override
    public Uri insert(Uri uri, ContentValues values) {
        return null;
    }

    @Override
    public int delete(Uri uri, String selection, String[] selectionArgs) {
        return 0;
    }

    @Override
    public int update(Uri uri, ContentValues values, String selection, String[] selectionArgs) {
        return 0;
    }
}
