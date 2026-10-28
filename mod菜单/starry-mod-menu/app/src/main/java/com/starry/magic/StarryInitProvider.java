package com.starry.magic;

import android.app.Application;
import android.content.ContentProvider;
import android.content.Context;
import android.content.ContentValues;
import android.database.Cursor;
import android.net.Uri;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.starry.magic.menu.MenuService;
import com.starry.magic.ota.StarryNativeLoad;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * 星光无 Application 替换注入：终末地（ACE/TSS）实测 Manifest 的
 * android:name Application 被替换为非预期类时，AceApplication.initialize
 * 会在 native 信号量上毒丸挂起（黑屏 Splash + 后续击杀）。
 *
 * 因此不改 Manifest 的 Application，改由本 Provider 在应用进程启动时
 * 自动初始化（ContentProvider.onCreate 早于 Activity.onCreate）。
 *
 * 时刻注意：本 onCreate 晚于 ACE initialize 完成，属安全窗口。
 */
public class StarryInitProvider extends ContentProvider {
    private static final String TAG = "StarryInitProvider";
    private static final Handler MAIN = new Handler(Looper.getMainLooper());
    private static volatile boolean sMenuStartScheduled;

    private static boolean isMainProcess(Context ctx) {
        return StarryApp.isMainProcess(ctx);
    }

    private static void scheduleMenuStart(Context ctx, String reason) {
        if (ctx == null || sMenuStartScheduled) return;
        if (!isMainProcess(ctx)) {
            Log.i(TAG, "MenuService.start skipped (" + reason + ", proc=" + StarryApp.currentProcessName() + ")");
            return;
        }
        sMenuStartScheduled = true;
        Log.i(TAG, "MenuService.start scheduled (" + reason + ")");
        MAIN.post(() -> {
            try {
                MenuService.start(ctx);
                Log.i(TAG, "MenuService.start ok (" + reason + ")");
            } catch (Throwable t) {
                sMenuStartScheduled = false;
                Log.e(TAG, "MenuService.start failed (" + reason + ")", t);
            }
        });
    }

    @Override
    public boolean onCreate() {
        try {
            Context holder = getContext();
            Context app = holder == null ? null : holder.getApplicationContext();
            if (!(app instanceof Application)) {
                Log.i(TAG, "no application context, skip");
                return true;
            }
            final Application application = (Application) app;
            if (!isMainProcess(application)) {
                Log.i(TAG, "skip native (proc=" + StarryApp.currentProcessName() + ")");
                return true;
            }
            // 2026-10-28：运行时签名伪装（SignSpoof）已整体删除；TapTap 验签/登录改走
            // 「改包法」（静态改游戏 dex 的门闩与登录入口，见 _templates/经验_TapTap改包法_…_20261028.md）。
            boolean ok = StarryNativeLoad.ensureLoaded(application);
            Log.i(TAG, "native ensureLoaded=" + ok + " path=" + StarryNativeLoad.loadedPath());
            // P1-6：把自身 APK 路径交给 native，立刻跑一次自完整性校验
            com.starry.magic.bridge.ModBridge.setApkPath(application);
            // TapTap 强制网页登录（旧机制，仅未迁移改包法的老包仍用）
            application.registerActivityLifecycleCallbacks(new Application.ActivityLifecycleCallbacks() {
                @Override
                public void onActivityResumed(android.app.Activity activity) {
                    scheduleMenuStart(application, "activity-resumed");
                }

                @Override
                public void onActivityPreCreated(android.app.Activity a, android.os.Bundle s) {
                    // TapTap 强制内嵌网页登录：onCreate 前 handler 遍历未发生，窗口正好
                    // ★r24 防闪退：生命周期回调裸调用 ⇒ 任何异常都是「打开游戏闪退」，逐个兜底
                    try {
                        TapLoginForceWeb.onPreCreated(a);
                    } catch (Throwable t) {
                        Log.e(TAG, "TapLoginForceWeb.onPreCreated failed", t);
                    }
                }

                @Override
                public void onActivityCreated(android.app.Activity a, android.os.Bundle s) {
                    try {
                        TapLoginForceWeb.onCreated(a);
                    } catch (Throwable t) {
                        Log.e(TAG, "TapLoginForceWeb.onCreated failed", t);
                    }
                }

                @Override public void onActivityStarted(android.app.Activity a) {}
                @Override public void onActivityPaused(android.app.Activity a) {}
                @Override public void onActivityStopped(android.app.Activity a) {}
                @Override public void onActivitySaveInstanceState(android.app.Activity a, android.os.Bundle o) {}
                @Override public void onActivityDestroyed(android.app.Activity a) {}
            });
            MAIN.postDelayed(() -> scheduleMenuStart(application, "fallback-5s"), 5000L);
        } catch (Throwable t) {
            Log.e(TAG, "onCreate failed", t);
        }
        return true;
    }

    @Override public Cursor query(Uri uri, String[] p, String s, String[] a, String o) { return null; }
    @Override public String getType(Uri uri) { return null; }
    @Override public Uri insert(Uri uri, ContentValues values) { return null; }
    @Override public int delete(Uri uri, String s, String[] a) { return 0; }
    @Override public int update(Uri uri, ContentValues values, String s, String[] a) { return 0; }
}
