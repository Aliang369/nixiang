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
        patchPluginHelperApp(getContext());
        try { System.loadLibrary("luster"); Log.i(TAG, "nx: luster preloaded"); }
        catch (Throwable t) { Log.w(TAG, "nx: luster preload failed", t); }
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
            // 签名校验过检（B 轨：MT ApkSignatureKillerEx 形态；包内无 killer dex 时只打一行日志）
            com.starry.magic.sign.SignatureKillerBootstrap.install(application);
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

    // 1.0.25 壳：真实 Application 未被创建 ⇒ PluginHelper.app 为 null ⇒ nativeInit 里
    // CCAppInfo.getBingoAppId → PluginHelper.getAppInfo() 抛 NPE ⇒ 进程 8 s 后静默 exit(0)。
    // 这里在 Provider 阶段起一个后台线程，等真实 dex 可由 Class.forName 看到后，把 Application 补上。
    private static void patchPluginHelperApp(final Context ctx) {
        final Application app = (Application) ctx.getApplicationContext();
        if (app == null) {
            return;
        }
        // 只对本游戏的 APKSecure 壳生效：别的游戏没有 com.bingo.sdklibs，免得白起一个 ~60 s 重试线程
        try {
            if (!"com.fightingstars.games.animal".equals(app.getPackageName())) {
                return;
            }
        } catch (Throwable ignore) {
            return;
        }
        Thread t = new Thread(new Runnable() {
            @Override
            public void run() {
                // 壳在 [secure] phase app-ready 之后才把真实 dex 换进 PathClassLoader，因此：
                // ① 必须用 Application 当前的 ClassLoader 取类（调用方 ClassLoader 是我们注入的 dex，
                //    看不到壳动态加载的真实 dex）；② 窗口要覆盖首启解密（实测首启到 app-ready 可 > 20 s）。
                for (int i = 0; i < 400; i++) {
                    try {
                        ClassLoader cl = app.getClassLoader();
                        Class<?> c = null;
                        if (cl != null) {
                            try {
                                c = Class.forName("com.bingo.sdklibs.utils.PluginHelper", false, cl);
                            } catch (Throwable ignore) { }
                        }
                        if (c == null) {
                            c = Class.forName("com.bingo.sdklibs.utils.PluginHelper");
                        }
                        java.lang.reflect.Field f = c.getDeclaredField("app");
                        f.setAccessible(true);
                        if (f.get(null) == null) {
                            c.getMethod("initWithApplication", Application.class).invoke(null, app);
                            Log.i(TAG, "PluginHelper.app filled at try=" + i + " loader=" + c.getClassLoader());
                        } else {
                            Log.i(TAG, "PluginHelper.app already set, skip");
                        }
                        return;
                    } catch (Throwable e) {
                        // 前 5 s 密集（100 × 50 ms），之后降到 5 Hz，最长 ~60 s
                        try { Thread.sleep(i < 100 ? 50 : 200); } catch (InterruptedException ignore) { }
                    }
                }
                Log.w(TAG, "PluginHelper.app fill failed after 400 tries");
            }
        }, "starry-ph");
        t.setDaemon(true);
        t.start();
    }
}
