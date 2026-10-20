package com.starry.magic;

import android.app.Activity;
import android.app.Application;
import android.content.Context;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.starry.magic.menu.MenuService;
import com.starry.magic.ota.StarryNativeLoad;
import com.starry.magic.sign.SignSpoof;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * 星光入口 Application（外科包 smali 常改 .super 为官方/壳 App）。
 * <p>
 * 无悔华夏（九游+乐变）：
 * <ul>
 *   <li>勿在 {@code super.onCreate()} 后立刻起 Service——壳占满主线程 + ServiceProxy，
 *       易 5s {@code ForegroundServiceDidNotStartInTimeException}。</li>
 *   <li>乐变会起 {@code :lebian.dns/:lebian.base/:pushservice} 等子进程；子进程里
 *       {@code StarryApp} 也会跑，若再 {@code startForegroundService} 必崩。菜单只在
 *       <b>主进程</b>（进程名=包名）启动。</li>
 * </ul>
 */
public class StarryApp extends Application {
    private static final String TAG = "StarryApp";
    private static final Handler MAIN = new Handler(Looper.getMainLooper());
    private static volatile boolean sMenuStartScheduled;

    /** 主进程=包名；{@code pkg:xxx} 子进程一律 false。 */
    public static boolean isMainProcess(Context ctx) {
        if (ctx == null) return false;
        String pkg = ctx.getPackageName();
        if (pkg == null || pkg.isEmpty()) return false;
        String proc = currentProcessName();
        return pkg.equals(proc);
    }

    public static String currentProcessName() {
        if (Build.VERSION.SDK_INT >= 28) {
            try {
                return Application.getProcessName();
            } catch (Throwable ignored) {
            }
        }
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object n = at.getDeclaredMethod("currentProcessName").invoke(null);
            if (n instanceof String) return (String) n;
        } catch (Throwable ignored) {
        }
        return "";
    }

    @Override
    protected void attachBaseContext(Context base) {
        try {
            SignSpoof.install();
        } catch (Throwable t) {
            Log.e(TAG, "SignSpoof.install failed", t);
        }
        super.attachBaseContext(base);
        // 必须用 this（已 attach）：传 base 时 getApplicationContext() 仍可能为 null → Preferences 崩
        try {
            com.starry.magic.cocos.DwctIapGate.apply(this);
        } catch (Throwable t) {
            Log.e(TAG, "DwctIapGate.apply failed", t);
        }
        // 子进程不加载 native（省事 + 避免误起菜单依赖）
        if (!isMainProcess(this)) {
            Log.i(TAG, "attachBaseContext skip native (proc=" + currentProcessName() + ")");
            return;
        }
        // 优先 OTA，避免后续 MenuService/ModBridge 静态块先 loadLibrary 钉死包内 so
        boolean ok = StarryNativeLoad.ensureLoaded(this);
        Log.i(TAG, "native ensureLoaded=" + ok + " path=" + StarryNativeLoad.loadedPath());
    }

    @Override
    public void onCreate() {
        super.onCreate();
        com.starry.magic.crash.CrashHandler.init(this);  // 崩溃日志：自测壳
        String proc = currentProcessName();
        Log.i(TAG, "onCreate proc=" + proc);
        // 乐变 overlay 的应用接管（LEBIAN_APPLICATION → StarryApp）实测只回调 onCreate、
        // 不回调 attachBaseContext（2026-10-21 无悔华夏 B 站包：主进程无 "native ensureLoaded" 日志、
        // /proc/<pid>/maps 无 libstarry_magic）。这里补一次幂等加载，attachBaseContext 正常时 isDone() 已 true。
        if (isMainProcess(this) && !StarryNativeLoad.isDone()) {
            boolean loaded = StarryNativeLoad.ensureLoaded(this);
            Log.i(TAG, "onCreate fallback native ensureLoaded=" + loaded
                    + " path=" + StarryNativeLoad.loadedPath());
        }
        if (!isMainProcess(this)) {
            Log.i(TAG, "onCreate skip MenuService (secondary process)");
            return;
        }
        registerActivityLifecycleCallbacks(new ActivityLifecycleCallbacks() {
            @Override
            public void onActivityResumed(Activity activity) {
                scheduleMenuStart(activity, "activity-resumed");
            }

            @Override
            public void onActivityPreCreated(Activity a, Bundle s) {
                // TapTap 强制内嵌网页登录：onCreate 前 handler 遍历未发生，窗口正好
                try {
                    TapLoginForceWeb.onPreCreated(a);
                } catch (Throwable ignored) {
                }
            }

            @Override
            public void onActivityCreated(Activity a, Bundle s) {
                try {
                    TapLoginForceWeb.onCreated(a);
                } catch (Throwable ignored) {
                }
            }
            @Override public void onActivityStarted(Activity a) {}
            @Override public void onActivityPaused(Activity a) {}
            @Override public void onActivityStopped(Activity a) {}
            @Override public void onActivitySaveInstanceState(Activity a, Bundle o) {}
            @Override public void onActivityDestroyed(Activity a) {}
        });
        // 兜底：极端无 Activity resume 时仍尝试（延迟，避开壳 onCreate 高峰）
        MAIN.postDelayed(() -> scheduleMenuStart(StarryApp.this, "fallback-5s"), 5000L);
    }

    private static void scheduleMenuStart(Context ctx, String reason) {
        if (ctx == null || sMenuStartScheduled) return;
        if (!isMainProcess(ctx)) {
            Log.i(TAG, "MenuService.start skipped (" + reason + ", proc=" + currentProcessName() + ")");
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
}
