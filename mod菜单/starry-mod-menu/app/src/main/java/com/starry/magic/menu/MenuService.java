package com.starry.magic.menu;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.Application;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ServiceInfo;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.os.SystemClock;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.View;
import android.view.ViewTreeObserver;
import android.view.WindowManager;
import android.widget.Toast;

import com.starry.magic.bridge.ModBridge;

/**
 * 悬浮窗菜单服务
 *
 * 全原生 UI：{@link NativeMenuController}「星光魔法阵」。
 * - 主路径：当前 Activity + TYPE_APPLICATION（无悬浮窗权限）
 * - 备用：TYPE_APPLICATION_OVERLAY（已授权时）
 */
public class MenuService extends Service {

    private static final String TAG = "MenuService";
    private static final int NOTIFICATION_ID = 1001;
    private static final String CHANNEL_ID = "starry_magic_menu";
    private static final long STARTUP_GRACE_MS = 10000L;
    /** 心跳自检间隔 */
    private static final long HEARTBEAT_INTERVAL_MS = 4000L;
    /** 摇一摇恢复：加速度突变阈值 + 冷却 */
    private static final float SHAKE_ACCEL_THRESHOLD = 13f;
    private static final long SHAKE_COOLDOWN_MS = 1200L;
    private static MenuService instance;
    /** 最近 resumed 的 Activity（弱引用），findResumedHint 用 */
    private static java.lang.ref.WeakReference<Activity> lastResumedActivity;

    static {
        // Application 已 ensureLoaded 时跳过，避免 startService 触发类加载时再扫盘拖慢 onCreate
        if (!com.starry.magic.ota.StarryNativeLoad.isDone()) {
            try {
                Class<?> at = Class.forName("android.app.ActivityThread");
                Object app = at.getMethod("currentApplication").invoke(null);
                if (app instanceof android.content.Context) {
                    com.starry.magic.ota.StarryNativeLoad.ensureLoaded((android.content.Context) app);
                } else {
                    loadStarryNativeLibraryFallback();
                }
            } catch (Throwable t) {
                loadStarryNativeLibraryFallback();
            }
        }
    }

    /** 无 Application 时的兜底（仍优先 OTA 扫描）。 */
    private static void loadStarryNativeLibraryFallback() {
        ClassLoader pathCl = resolveAppClassLoader();
        // 尝试从常见 files 根扫 OTA
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof android.content.Context
                    && com.starry.magic.ota.StarryNativeLoad.ensureLoaded((android.content.Context) app)) {
                return;
            }
        } catch (Throwable ignored) {
        }
        String filesNative = null;
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof android.content.Context) {
                filesNative = new java.io.File(
                        ((android.content.Context) app).getFilesDir(), "starry_native")
                        .getAbsolutePath();
            }
        } catch (Throwable ignored) {
        }
        String[] cxx = filesNative != null
                ? new String[] { filesNative + "/libc++_shared.so", "/data/local/tmp/ltzj_st/libc++_shared.so" }
                : new String[] { "/data/local/tmp/ltzj_st/libc++_shared.so" };
        for (String path : cxx) {
            if (!new java.io.File(path).isFile()) continue;
            if (loadIntoClassLoader(pathCl, path)) {
                Log.i(TAG, "libc++_shared loaded via System.load: " + path);
                break;
            }
        }
        // 有 OTA 目录时禁止 loadLibrary
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof android.content.Context) {
                java.io.File ota = com.starry.magic.ota.StarryNativeLoad.findOtaSo((android.content.Context) app);
                if (ota != null) {
                    if (loadIntoClassLoader(pathCl, ota.getAbsolutePath())) {
                        Log.i(TAG, "libstarry_magic OTA loaded: " + ota.getAbsolutePath());
                        return;
                    }
                    Log.e(TAG, "OTA so exists but load failed, skip apk loadLibrary");
                    return;
                }
            }
        } catch (Throwable ignored) {
        }
        String[] abs = filesNative != null
                ? new String[] { filesNative + "/libstarry_magic.so", "/data/local/tmp/ltzj_st/libstarry_magic.so" }
                : new String[] { "/data/local/tmp/ltzj_st/libstarry_magic.so" };
        for (String path : abs) {
            if (!new java.io.File(path).isFile()) continue;
            if (loadIntoClassLoader(pathCl, path)) {
                Log.i(TAG, "libstarry_magic loaded via System.load: " + path);
                return;
            }
        }
        try {
            System.loadLibrary("starry_magic");
            Log.i(TAG, "libstarry_magic loaded");
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "loadLibrary starry_magic failed", e);
        }
    }

    private static ClassLoader resolveAppClassLoader() {
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof android.content.Context) {
                return ((android.content.Context) app).getClassLoader();
            }
        } catch (Throwable ignored) {
        }
        return MenuService.class.getClassLoader();
    }

    private static boolean loadIntoClassLoader(ClassLoader cl, String path) {
        try {
            Class<?> linker = Class.forName("com.starry.magic.ltzj.NativeLinker", true, cl);
            linker.getMethod("load", String.class).invoke(null, path);
            return true;
        } catch (Throwable t) {
            Throwable c = t.getCause() != null ? t.getCause() : t;
            String msg = c.getMessage() != null ? c.getMessage() : String.valueOf(c);
            if (msg.contains("already opened") && msg.contains("PathClassLoader")) {
                return true;
            }
            Log.w(TAG, "System.load failed: " + path + " → " + msg);
            return false;
        }
    }

    /**
     * 独立助手模式（无 root 直装）：星光助手以自己包名单独安装，
     * MenuService 跑在 {@link ModTarget#HELPER_PKG} 进程里，拿不到游戏进程的宿主 Activity。
     * 此模式只能走 TYPE_APPLICATION_OVERLAY 常驻，禁止绑本 App 自己的 Activity
     * （一旦绑上，切到游戏后本 App onStop 就会把菜单一起摘掉），
     * 同时禁止任何「宿主生命周期驱动的藏窗」。
     *
     * <p>包名取自 {@link ModTarget}，不再硬编码；换游戏只改那一个文件。
     */
    public static boolean isStandaloneMode(Context ctx) {
        try {
            if (ctx == null) {
                return false;
            }
            String pkg = ctx.getPackageName();
            // 双保险：两个包名被组装脚本改成一样时（例如游戏本身就叫 com.starry.magic），
            // 不能把游戏进程误判成独立助手 —— 那会让游戏内的菜单丢掉宿主、只剩悬浮窗。
            if (ModTarget.HELPER_PKG.equals(ModTarget.GAME_PKG)) {
                Log.w(TAG, "isStandaloneMode: HELPER_PKG == GAME_PKG ("
                        + ModTarget.GAME_PKG + "), 判定为注入形态");
                return false;
            }
            return ModTarget.HELPER_PKG.equals(pkg) && !ModTarget.GAME_PKG.equals(pkg);
        } catch (Throwable t) {
            return false;
        }
    }

    private boolean standalone() {
        return isStandaloneMode(this);
    }

    private WindowManager windowManager;
    private NativeMenuController nativeMenu;
    private boolean isMenuVisible = false;
    /** nativeMenu 是否已 attach 且当前视为「在屏上」 */
    private boolean overlayAttached = false;
    private boolean nativeBootstrapped = false;
    private final Handler mainHandler = new MenuSafeHandler(TAG);
    private Application.ActivityLifecycleCallbacks resumeWatcher;
    /** 进程内已 started 的 Activity 数；为 0 视为进后台 */
    private int startedActivityCount = 0;
    /** resumed 计数：用于 onPause 立刻藏窗（不等 ColorOS 拖到 onStop） */
    private int resumedActivityCount = 0;
    /**
     * Service 常在 Activity.onCreate 里启动，register 时已错过 onResume，
     * 计数会一直为 0 → 误判后台并 removeView，桌面冷启动（Splash 链）尤其明显。
     */
    private boolean hostLifecyclePrimed = false;
    private long menuCreatedAtElapsed = 0L;
    /** start(Activity) 时暂存，onCreate 里补登记生命周期 */
    private static java.lang.ref.WeakReference<Activity> pendingHostActivity;
    private Activity focusedHost;
    /** 菜单输入框要焦点（弹 IME）时宿主会丢 window focus，不能当成划后台 */
    private boolean overlayInputFocus = false;
    /** 失焦/进浮窗后一段时间内禁止再 addView，避免自由浮窗动画期又挂上全屏 overlay */
    private long suppressShowUntilElapsed = 0L;
    /** 用户设置页「隐藏菜单」：仅本局，重启后默认恢复 */
    private boolean menuUserHidden = false;
    private SensorManager shakeSensorManager;
    private SensorEventListener shakeListener;
    private long lastShakeAtElapsed = 0L;
    private final Runnable hideOverlayIfBackground = () -> {
        if (!hostLifecyclePrimed) {
            Log.i(TAG, "app background ignored (lifecycle not primed)");
            return;
        }
        // ACTIVITY 模式：Activity 进后台时 View 自然不可见，不需要主动 removeView
        if (nativeMenu != null && nativeMenu.isAttached()
                && nativeMenu.getAttachMode() == NativeMenuController.AttachMode.ACTIVITY) {
            Log.i(TAG, "app background ignored (ACTIVITY mode, view auto-hidden)");
            return;
        }
        if (startedActivityCount > 0 || resumedActivityCount > 0) {
            Log.i(TAG, "app background ignored (host already resumed)");
            return;
        }
        if (startedActivityCount <= 0 && resumedActivityCount <= 0) {
            hideOverlayNow("app background");
        }
    };
    private final Runnable focusGainedSyncRunnable =
            () -> syncOverlayForHost("host window focus gained (deferred)");
    private final ViewTreeObserver.OnWindowFocusChangeListener hostFocusListener = hasFocus -> {
        if (!hasFocus) {
            if (overlayInputFocus) {
                Log.i(TAG, "host focus lost ignored (overlay IME)");
                return;
            }
            mainHandler.removeCallbacks(hideOverlayIfBackground);
            mainHandler.removeCallbacks(focusGainedSyncRunnable);
            mainHandler.post(() -> {
                if (overlayInputFocus) {
                    Log.i(TAG, "host focus lost ignored (overlay IME, deferred)");
                    return;
                }
                // ACTIVITY 模式不因失焦藏窗：挂在 Activity 自己的 WM 上，不会盖桌面
                if (nativeMenu != null && nativeMenu.isAttached()
                        && nativeMenu.getAttachMode() == NativeMenuController.AttachMode.ACTIVITY) {
                    Log.i(TAG, "host focus lost ignored (ACTIVITY mode)");
                    return;
                }
                hideOverlayNow("host window focus lost");
            });
        } else {
            mainHandler.removeCallbacks(hideOverlayIfBackground);
            mainHandler.removeCallbacks(focusGainedSyncRunnable);
            mainHandler.postDelayed(focusGainedSyncRunnable, 450);
        }
    };

    private final Runnable suppressExpiredSyncRunnable =
            () -> syncOverlayForHost("suppress expired");

    /** 心跳自检：菜单掉了就补回来；宿主死了也强制重绑 */
    private final Runnable heartbeatRunnable = new Runnable() {
        @Override
        public void run() {
            try {
            if (instance == null) return;
            if (menuUserHidden) {
                return;
            }
            Activity host = resolveMenuHost(focusedHost);
            if (nativeMenu != null && nativeMenu.isAttached()
                    && (!nativeMenu.isBoundHostAlive()
                    || (host != null
                    && nativeMenu.getAttachMode() == NativeMenuController.AttachMode.ACTIVITY
                    && nativeMenu.getBoundActivity() != host))) {
                Log.i(TAG, "heartbeat: host stale → ensureAliveOrRebind");
                if (host != null && !host.isFinishing()) {
                    nativeMenu.ensureAliveOrRebind(host);
                    syncOverlayForHost("heartbeat-stale");
                } else {
                    nativeMenu.ensureAliveOrRebind(null);
                }
            } else if (nativeMenu == null || !nativeMenu.isAttached()) {
                if (host != null && !host.isFinishing()) {
                    Log.i(TAG, "heartbeat: menu lost, re-attaching");
                    ensureNativeMenu("heartbeat", host);
                    syncOverlayForHost("heartbeat");
                }
            }
            } catch (Throwable t) {
                Log.e(TAG, "heartbeat exception isolated", t);
            } finally {
                mainHandler.postDelayed(this, HEARTBEAT_INTERVAL_MS);
            }
        }
    };

    private final Runnable hostLayoutSyncRunnable =
            () -> syncOverlayForHost("host layout changed");
    private final ViewTreeObserver.OnGlobalLayoutListener hostLayoutListener = () -> {
        mainHandler.removeCallbacks(hostLayoutSyncRunnable);
        mainHandler.postDelayed(hostLayoutSyncRunnable, 50);
    };

    private boolean inStartupGrace() {
        return menuCreatedAtElapsed > 0L
                && android.os.SystemClock.uptimeMillis() - menuCreatedAtElapsed < STARTUP_GRACE_MS;
    }

    private boolean isNativeReady() {
        return nativeMenu != null && nativeMenu.isAttached();
    }

    private void hideOverlayNow(String reason) {
        if (standalone()) {
            Log.i(TAG, reason + " → skip hide (standalone overlay)");
            return;
        }
        if (!hostLifecyclePrimed && inStartupGrace()) {
            Log.i(TAG, reason + " → skip hide (startup grace)");
            return;
        }
        // ACTIVITY 模式：挂在 Activity 自己的 WM 上，不会盖桌面，不需要失焦藏窗
        if (nativeMenu != null && nativeMenu.isAttached()
                && nativeMenu.getAttachMode() == NativeMenuController.AttachMode.ACTIVITY) {
            Log.i(TAG, reason + " → skip hide (ACTIVITY mode, safe)");
            return;
        }
        overlayInputFocus = false;
        suppressShowUntilElapsed = android.os.SystemClock.uptimeMillis() + 1800L;
        mainHandler.removeCallbacks(focusGainedSyncRunnable);
        mainHandler.removeCallbacks(suppressExpiredSyncRunnable);
        mainHandler.postDelayed(suppressExpiredSyncRunnable, 1850);
        Log.i(TAG, reason + " → hide overlay");
        setOverlayVisible(false);
    }

    /** 补登记宿主：解决 Service 晚于 Activity.onResume 注册导致的计数为 0 */
    private void primeHostLifecycle(Activity a, String reason) {
        if (a == null || a.isFinishing()) return;
        hostLifecyclePrimed = true;
        if (resumedActivityCount <= 0) resumedActivityCount = 1;
        if (startedActivityCount <= 0) startedActivityCount = 1;
        attachHostFocusWatcher(a);
        Log.i(TAG, "host lifecycle primed (" + reason + ") started="
                + startedActivityCount + " resumed=" + resumedActivityCount);
        mainHandler.removeCallbacks(hideOverlayIfBackground);
        mainHandler.post(() -> {
            ensureMenuAfterPermissionGranted("prime:" + reason);
            syncOverlayForHost("prime:" + reason);
        });
    }

    /** AppActivity.onResume 注入调用，确保桌面冷启动 Splash 链后也能稳住悬浮窗 */
    public static void notifyHostResumed(Activity activity) {
        MenuService s = instance;
        if (s == null) {
            if (activity != null) {
                pendingHostActivity = new java.lang.ref.WeakReference<>(activity);
            }
            return;
        }
        if (activity == null) return;
        s.mainHandler.post(() -> s.primeHostLifecycle(activity, "notifyHostResumed"));
    }

    /** ColorOS CompactWindow：Decor 仍是全屏分辨率，必须用 Task/WindowConfig 真实 bounds */
    @SuppressWarnings("deprecation")
    private float hostScreenAreaRatio(Activity a) {
        if (a == null) return 1f;
        DisplayMetrics dm = new DisplayMetrics();
        try {
            (windowManager != null ? windowManager : a.getWindowManager())
                    .getDefaultDisplay().getRealMetrics(dm);
        } catch (Throwable t) {
            return 1f;
        }
        int sw = Math.max(dm.widthPixels, 1);
        int sh = Math.max(dm.heightPixels, 1);
        float screenArea = sw * (float) sh;

        android.graphics.Rect taskBounds = null;
        try {
            Configuration cfg = a.getResources().getConfiguration();
            Object wc = Configuration.class.getMethod("getWindowConfiguration").invoke(cfg);
            Object b = wc.getClass().getMethod("getBounds").invoke(wc);
            if (b instanceof android.graphics.Rect && !((android.graphics.Rect) b).isEmpty()) {
                taskBounds = new android.graphics.Rect((android.graphics.Rect) b);
            }
        } catch (Throwable ignored) {}
        if (taskBounds == null && Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            try {
                taskBounds = new android.graphics.Rect(a.getWindowManager().getCurrentWindowMetrics().getBounds());
            } catch (Throwable ignored) {}
        }
        if (taskBounds == null) {
            try {
                ActivityManager am = (ActivityManager) a.getSystemService(Context.ACTIVITY_SERVICE);
                if (am != null) {
                    for (ActivityManager.AppTask appTask : am.getAppTasks()) {
                        ActivityManager.RecentTaskInfo info = appTask.getTaskInfo();
                        if (info == null || info.taskId != a.getTaskId()) continue;
                        Object cfg = null;
                        try {
                            cfg = info.getClass().getField("configuration").get(info);
                        } catch (Throwable ignored) {}
                        if (cfg != null) {
                            Object wc = cfg.getClass().getMethod("getWindowConfiguration").invoke(cfg);
                            Object b = wc.getClass().getMethod("getBounds").invoke(wc);
                            if (b instanceof android.graphics.Rect && !((android.graphics.Rect) b).isEmpty()) {
                                taskBounds = new android.graphics.Rect((android.graphics.Rect) b);
                            }
                        }
                        if (taskBounds == null) {
                            try {
                                Object b = info.getClass().getField("taskBounds").get(info);
                                if (b instanceof android.graphics.Rect && !((android.graphics.Rect) b).isEmpty()) {
                                    taskBounds = new android.graphics.Rect((android.graphics.Rect) b);
                                }
                            } catch (Throwable ignored) {}
                        }
                        break;
                    }
                }
            } catch (Throwable ignored) {}
        }

        if (taskBounds == null || taskBounds.isEmpty()) {
            if (a.getWindow() == null) return 1f;
            View decor = a.getWindow().getDecorView();
            int vw = decor.getWidth();
            int vh = decor.getHeight();
            if (vw <= 0 || vh <= 0) return 1f;
            return (vw * (float) vh) / screenArea;
        }
        float ratio = (taskBounds.width() * (float) taskBounds.height()) / screenArea;
        Log.i(TAG, "hostTaskBounds=" + taskBounds + " ratio=" + ratio);
        return ratio;
    }

    private boolean isHostNearFullscreen(Activity a) {
        return hostScreenAreaRatio(a) >= 0.75f;
    }

    /**
     * ColorOS 自由浮窗/分屏：Activity 仍 resumed 甚至有焦点，但全屏 OVERLAY 会盖住桌面，
     * 触发「正在当前页面悬浮显示，可能会造成部分操作无响应」。
     */
    private boolean shouldShowOverlayForHost(Activity a) {
        // 独立助手模式：菜单是常驻悬浮球，与「同进程宿主是否全屏」无关
        if (standalone()) return true;
        if (a == null || a.isFinishing()) return false;
        if (android.os.SystemClock.uptimeMillis() < suppressShowUntilElapsed) {
            return false;
        }
        if (!isNativeReady()) return false;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N && a.isInMultiWindowMode()) {
            return false;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O && a.isInPictureInPictureMode()) {
            return false;
        }
        if (!isHostNearFullscreen(a)) {
            return false;
        }
        return a.hasWindowFocus() || overlayInputFocus;
    }

    /** 仅在全屏且宿主有焦点时露菜单；否则藏窗避免系统警告（OVERLAY 挂载时） */
    private void syncOverlayForHost(String reason) {
        if (nativeMenu == null) return;
        if (menuUserHidden) {
            Log.i(TAG, reason + " → skip show (user hidden)");
            return;
        }
        Activity host = focusedHost;
        if (host == null || host.isFinishing()) {
            host = resolveMenuHost(null);
        }
        // ACTIVITY 模式：hide 只是 removeView，isAttached 仍为 true。
        // 必须显式 addView；失败则整窗重绑。宿主死/切 Splash→Game 必须 ensureAliveOrRebind。
        if (nativeMenu.isAttached()
                && nativeMenu.getAttachMode() == NativeMenuController.AttachMode.ACTIVITY) {
            if (host != null && !host.isFinishing()
                    && (resumedActivityCount > 0 || host.hasWindowFocus()
                    || !nativeMenu.isBoundHostAlive())) {
                suppressShowUntilElapsed = 0L;
                if (startedActivityCount <= 0) startedActivityCount = 1;
                if (resumedActivityCount <= 0) resumedActivityCount = 1;
                nativeMenu.ensureAliveOrRebind(host);
                setOverlayVisible(true);
                Log.i(TAG, reason + " → show ACTIVITY overlay"
                        + " host=" + host.getClass().getSimpleName()
                        + " alive=" + nativeMenu.isBoundHostAlive());
            } else if (!nativeMenu.isBoundHostAlive()) {
                Log.w(TAG, reason + " → ACTIVITY host dead, detach");
                nativeMenu.ensureAliveOrRebind(null);
            }
            return;
        }
        if (shouldShowOverlayForHost(host)) {
            setOverlayVisible(true);
            Log.i(TAG, reason + " → show overlay");
        } else if (overlayAttached || isMenuVisible) {
            hideOverlayNow(reason + " (not fullscreen focus)");
        }
    }

    public static MenuService getInstance() {
        return instance;
    }

    /** 供 NativeMenuController.findResumedHint 使用 */
    static Activity getLastResumedActivity() {
        // 独立助手模式：本 App 的 Activity 不能当宿主（否则菜单随本 App 进后台被摘）
        MenuService s = instance;
        if (s != null && s.standalone()) return null;
        if (lastResumedActivity == null) return null;
        Activity a = lastResumedActivity.get();
        return (a != null && !a.isFinishing()) ? a : null;
    }

    /**
     * MenuService 晚于 GameActivity.onResume 启动时，lifecycle 回调会错过首帧。
     * 从 ActivityThread.mActivities 补捞未 paused 的 Activity（TLGD 等未注入 onResume 的游戏）。
     */
    private static Activity findResumedActivityViaThread() {
        MenuService s0 = instance;
        if (s0 != null && s0.standalone()) return null;
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object thread = at.getMethod("currentActivityThread").invoke(null);
            java.lang.reflect.Field f = at.getDeclaredField("mActivities");
            f.setAccessible(true);
            Object mActivities = f.get(thread);
            if (!(mActivities instanceof java.util.Map)) return null;
            for (Object rh : ((java.util.Map<?, ?>) mActivities).values()) {
                if (rh == null) continue;
                Class<?> rhCls = rh.getClass();
                java.lang.reflect.Field paused = rhCls.getDeclaredField("paused");
                paused.setAccessible(true);
                if (paused.getBoolean(rh)) continue;
                java.lang.reflect.Field activity = rhCls.getDeclaredField("activity");
                activity.setAccessible(true);
                Object act = activity.get(rh);
                if (!(act instanceof Activity)) continue;
                Activity a = (Activity) act;
                if (a.isFinishing()) continue;
                String pkg = a.getPackageName();
                if (pkg == null) continue;
                MenuService s = instance;
                if (s != null && !pkg.equals(s.getPackageName())) continue;
                lastResumedActivity = new java.lang.ref.WeakReference<>(a);
                return a;
            }
            return null;
        } catch (Throwable t) {
            Log.w(TAG, "findResumedActivityViaThread: " + t.getMessage());
            return null;
        }
    }

    private Activity resolveMenuHost(Activity preferred) {
        // 独立助手模式：绝不拿本 App 的 Activity 当宿主，强制 OVERLAY
        if (standalone()) return null;
        if (preferred != null && !preferred.isFinishing()) return preferred;
        Activity a = getLastResumedActivity();
        if (a != null) return a;
        if (focusedHost != null && !focusedHost.isFinishing()) return focusedHost;
        if (pendingHostActivity != null) {
            a = pendingHostActivity.get();
            if (a != null && !a.isFinishing()) return a;
        }
        return findResumedActivityViaThread();
    }

    public boolean isMenuVisible() {
        return isMenuVisible;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        instance = this;
        com.starry.magic.crash.CrashHandler.init(this);  // 崩溃日志：游戏进程内尽早挂
        // Android 8+：startForegroundService 后须尽快 startForeground；勿在 native/Preferences 之后
        //（无悔华夏等重 Application 机型会 ForegroundServiceDidNotStartInTimeException）
        try {
            startForegroundCompat();
        } catch (Throwable t) {
            Log.e(TAG, "onCreate startForeground failed", t);
        }
        menuCreatedAtElapsed = android.os.SystemClock.uptimeMillis();
        windowManager = (WindowManager) getSystemService(WINDOW_SERVICE);
        Log.i(TAG, "MenuService created");
        // 兜底加载（幂等）：attachBaseContext 在乐变 overlay 下可能未被回调，Service 一定会跑。
        if (!com.starry.magic.ota.StarryNativeLoad.isDone()) {
            boolean loaded = com.starry.magic.ota.StarryNativeLoad.ensureLoaded(this);
            Log.i(TAG, "onCreate native ensureLoaded=" + loaded
                    + " path=" + com.starry.magic.ota.StarryNativeLoad.loadedPath());
        }
        try {
            Preferences.with(this);
            ModBridge.syncAuthGameIdFromNative();
            Log.i(TAG, "auth game_id=" + Preferences.getAuthGameId());
        } catch (Throwable t) {
            Log.e(TAG, "post-fg init failed (menu may still show)", t);
        }
        // ★r24 防闪退：以下四段原先都是 onCreate 裸段 —— 任一抛出都会让游戏进程当场死
        //   （这就是「打开游戏就闪退」最典型的一类）。分段兜底，失败的段只降级不致命。
        try {
            registerResumeWatcher();
        } catch (Throwable t) {
            Log.e(TAG, "onCreate registerResumeWatcher failed", t);
        }
        Activity pending = pendingHostActivity != null ? pendingHostActivity.get() : null;
        pendingHostActivity = null;
        try {
            if (pending != null) {
                primeHostLifecycle(pending, "start-context");
            } else {
                Activity live = findResumedActivityViaThread();
                if (live != null) {
                    primeHostLifecycle(live, "start-thread-scan");
                }
            }
        } catch (Throwable t) {
            Log.e(TAG, "onCreate primeHostLifecycle failed", t);
        }
        try {
            tryShowMenuWithRetry(0);
        } catch (Throwable t) {
            Log.e(TAG, "onCreate tryShowMenuWithRetry failed", t);
        }
        if (standalone()) {
            Log.i(TAG, "standalone overlay mode (pkg=" + ModTarget.HELPER_PKG
                    + ", target=" + ModTarget.GAME_PKG + ")");
            mainHandler.postDelayed(standaloneAttachRunnable, 1500L);
        }
        mainHandler.postDelayed(heartbeatRunnable, HEARTBEAT_INTERVAL_MS);
    }

    /** 独立助手：常驻悬浮球自愈（授权后 / 被系统回收后自动重挂） */
    private final Runnable standaloneAttachRunnable = new Runnable() {
        @Override
        public void run() {
            try {
                if (menuUserHidden) return;
                if (nativeMenu == null || !nativeMenu.isAttached()) {
                    Log.i(TAG, "standalone: (re)attach overlay");
                    showMenuNative(null);
                } else if (nativeMenu.getAttachMode() != NativeMenuController.AttachMode.OVERLAY) {
                    Log.i(TAG, "standalone: force OVERLAY (was "
                            + nativeMenu.getAttachMode() + ")");
                    nativeMenu.detachWindowOnly();
                    nativeMenu = null;
                    showMenuNative(null);
                } else if (!nativeMenu.canOverlay()) {
                    Log.w(TAG, "standalone: overlay permission missing");
                }
                if (nativeMenu != null && nativeMenu.isAttached()) {
                    setOverlayVisible(true);
                }
            } catch (Throwable t) {
                Log.e(TAG, "standaloneAttach isolated", t);
            } finally {
                if (standalone()) mainHandler.postDelayed(this, 2500L);
            }
        }
    };

    public static void notifyOverlayInputFocus(boolean focused) {
        MenuService s = instance;
        if (s == null) return;
        s.overlayInputFocus = focused;
    }

    /**
     * 宿主即将离开到桌面/多任务（Activity.onUserLeaveHint）。
     * 上划手势时 onPause 很晚，全屏悬浮窗会盖在系统动画上几秒；此处尽早藏窗。
     */
    public static void notifyHostLeaving() {
        MenuService s = instance;
        if (s == null) return;
        s.mainHandler.post(() -> {
            s.mainHandler.removeCallbacks(s.hideOverlayIfBackground);
            s.hideOverlayNow("host onUserLeaveHint");
        });
    }

    private void registerResumeWatcher() {
        if (resumeWatcher != null) return;
        resumeWatcher = new Application.ActivityLifecycleCallbacks() {
            @Override public void onActivityCreated(Activity a, Bundle s) {}

            @Override
            public void onActivityStarted(Activity a) {
                startedActivityCount++;
                mainHandler.removeCallbacks(hideOverlayIfBackground);
                mainHandler.removeCallbacks(zombieWatchdog);
            }

            @Override
            public void onActivityResumed(Activity a) {
                hostLifecyclePrimed = true;
                resumedActivityCount++;
                lastResumedActivity = new java.lang.ref.WeakReference<>(a);
                mainHandler.removeCallbacks(hideOverlayIfBackground);
                // 取消「误藏」抑制窗，立刻允许再显示
                suppressShowUntilElapsed = 0L;
                attachHostFocusWatcher(a);
                mainHandler.post(() -> {
                    ensureNativeMenu("activity-resumed:" + a.getClass().getSimpleName(), a);
                    syncOverlayForHost("activity-resumed:" + a.getClass().getSimpleName());
                    // 截图进相册再回：焦点/WM token 常滞后，多补几次
                    mainHandler.postDelayed(
                            () -> syncOverlayForHost("resume delayed 400"), 400);
                    mainHandler.postDelayed(
                            () -> syncOverlayForHost("resume delayed 1200"), 1200);
                });
            }

            @Override
            public void onActivityPaused(Activity a) {
                resumedActivityCount = Math.max(0, resumedActivityCount - 1);
                detachHostFocusWatcher(a);
                if (resumedActivityCount == 0) {
                    mainHandler.removeCallbacks(hideOverlayIfBackground);
                    // 拉长防抖：挂机/ColorOS 常在数毫秒内 Stop→Start
                    mainHandler.postDelayed(hideOverlayIfBackground, 600);
                }
            }

            @Override
            public void onActivityStopped(Activity a) {
                startedActivityCount = Math.max(0, startedActivityCount - 1);
                if (startedActivityCount == 0 && resumedActivityCount == 0) {
                    mainHandler.removeCallbacks(hideOverlayIfBackground);
                    mainHandler.postDelayed(hideOverlayIfBackground, 600);
                }
            }

            @Override public void onActivitySaveInstanceState(Activity a, Bundle o) {}

            @Override
            public void onActivityDestroyed(Activity a) {
                try {
                if (nativeMenu == null || a == null) return;
                if (nativeMenu.getAttachMode() != NativeMenuController.AttachMode.ACTIVITY) return;
                if (nativeMenu.getBoundActivity() != a) return;
                Log.i(TAG, "bound host destroyed: " + a.getClass().getSimpleName()
                        + " → schedule rebind");
                mainHandler.post(() -> {
                    if (menuUserHidden || nativeMenu == null) return;
                    Activity next = resolveMenuHost(focusedHost);
                    if (next != null && next != a && !next.isFinishing()) {
                        nativeMenu.ensureAliveOrRebind(next);
                        syncOverlayForHost("host-destroyed→" + next.getClass().getSimpleName());
                    } else {
                        // 暂无新宿主：先摘掉死窗，等下次 resume/heartbeat
                        nativeMenu.ensureAliveOrRebind(null);
                    }
                });
                } catch (Throwable t) {
                    Log.e(TAG, "onActivityDestroyed rebind failed", t);
                }
            }
        };
        // ★r24 防闪退：getApplication() 在极少数机型/进程形态下可能抛异常或返回 null，
        //   而本方法是 onCreate 裸段调用 ⇒ 异常会直接冒泡杀进程。失败不致命：resume 补挂仍在。
        try {
            Application app = getApplication();
            if (app == null) {
                Log.w(TAG, "lifecycleWatcher skipped: getApplication()==null");
                resumeWatcher = null;
                return;
            }
            app.registerActivityLifecycleCallbacks(resumeWatcher);
            Log.i(TAG, "lifecycleWatcher registered (resume + background hide)");
        } catch (Throwable t) {
            resumeWatcher = null;
            Log.e(TAG, "lifecycleWatcher register failed", t);
        }
    }

    private void unregisterResumeWatcher() {
        if (resumeWatcher == null) return;
        mainHandler.removeCallbacks(hideOverlayIfBackground);
        try {
            getApplication().unregisterActivityLifecycleCallbacks(resumeWatcher);
        } catch (Exception ignored) {}
        resumeWatcher = null;
    }

    private void attachHostFocusWatcher(Activity a) {
        // ★r24 防闪退：本方法从 onActivityResumed 里裸调用，任何装饰视图异常（宿主机型差异、
        //   Window 已销毁）都会冒泡成「打开游戏闪退」⇒ 整体纳入 Throwable 兜底。
        try {
            attachHostFocusWatcher0(a);
        } catch (Throwable t) {
            Log.e(TAG, "attachHostFocusWatcher failed", t);
        }
    }

    private void attachHostFocusWatcher0(Activity a) {
        if (a == null || a.getWindow() == null) return;
        View decor = a.getWindow().getDecorView();
        if (focusedHost != null && focusedHost != a) {
            detachHostFocusWatcher(focusedHost);
        }
        focusedHost = a;
        ViewTreeObserver obs = decor.getViewTreeObserver();
        obs.removeOnWindowFocusChangeListener(hostFocusListener);
        obs.addOnWindowFocusChangeListener(hostFocusListener);
        obs.removeOnGlobalLayoutListener(hostLayoutListener);
        obs.addOnGlobalLayoutListener(hostLayoutListener);
    }

    private void detachHostFocusWatcher(Activity a) {
        if (a == null || a.getWindow() == null) return;
        try {
            ViewTreeObserver obs = a.getWindow().getDecorView().getViewTreeObserver();
            obs.removeOnWindowFocusChangeListener(hostFocusListener);
            obs.removeOnGlobalLayoutListener(hostLayoutListener);
        } catch (Exception ignored) {}
        if (focusedHost == a) focusedHost = null;
    }

    /** 菜单未起来时补一次显示 */
    private void ensureMenuAfterPermissionGranted(String reason) {
        Activity host = focusedHost;
        if (host == null && pendingHostActivity != null) {
            host = pendingHostActivity.get();
        }
        ensureNativeMenu(reason, host);
    }

    private void ensureNativeMenu(String reason, Activity host) {
        if (menuUserHidden) {
            Log.i(TAG, "ensure native menu skipped (user hidden): " + reason);
            return;
        }
        if (nativeMenu != null && nativeMenu.isAttached()) {
            if (host != null) nativeMenu.ensureAliveOrRebind(host);
            return;
        }
        Log.i(TAG, "ensure native menu (" + reason + ")");
        try {
            showMenuNative(host);
        } catch (Throwable e) {   // ★r24：Throwable（Error 族同样会让进程闪退）
            Log.e(TAG, "ensure native failed: " + e.getMessage());
            tryShowMenuWithRetry(0);
        }
    }

    private void tryShowMenuWithRetry(int attempt) {
        Activity host = resolveMenuHost(focusedHost);
        try {
            if (showMenuNative(host)) return;
        } catch (Throwable e) {   // ★r24：Throwable（NoClassDefFoundError/UnsatisfiedLinkError 都在内）
            Log.e(TAG, "showMenuNative failed (attempt " + attempt + "): " + e.getMessage());
        }
        if (attempt < 20) {
            mainHandler.postDelayed(() -> tryShowMenuWithRetry(attempt + 1), 1500);
        } else {
            Log.w(TAG, "native menu wait exhausted; will retry on activity resume");
        }
    }

    public static void start(Context context) {
        if (context == null) return;
        // 乐变子进程（:lebian.dns 等）也会加载 StarryApp；在那里起 FGS → 必 5s 杀进程
        if (!com.starry.magic.StarryApp.isMainProcess(context)) {
            Log.i(TAG, "start skipped: secondary proc=" + com.starry.magic.StarryApp.currentProcessName());
            return;
        }
        if (instance != null) {
            Log.i(TAG, "start skipped: already running");
            return;
        }
        if (context instanceof Activity) {
            pendingHostActivity = new java.lang.ref.WeakReference<>((Activity) context);
        }
        // 等长 Manifest 劫持：部分游戏新版没有 33 字符广告 Service 槽，
        // 由外科脚本把 34 字符槽替换成 MenuService2（MenuService 空子类）。
        // 仅当 MenuService2 真的被声明在当前包 Manifest 里才切别名，模板自测不受影响。
        Class<?> svc = MenuService.class;
        try {
            Intent probe = new Intent()
                    .setClassName(context.getPackageName(), "com.starry.magic.menu.MenuService2");
            if (context.getPackageManager().resolveService(probe, 0) != null) {
                svc = Class.forName("com.starry.magic.menu.MenuService2");
                Log.i(TAG, "start via MenuService2 alias (manifest slot)");
            }
        } catch (Throwable ignored) {
        }
        Intent intent = new Intent(context, svc);
        // 乐变 ServiceProxy：禁止 startForegroundService（onCreate 迟到必崩）。
        // 仅 plain startService；onCreate 里仍 startForeground 保通知。
        try {
            context.startService(intent);
            Log.i(TAG, "startService (plain, no-FGS)");
        } catch (Throwable t) {
            Log.e(TAG, "MenuService start failed", t);
        }
    }

    public void showMenu() {
        // ★r24 防闪退：这里原先是**裸调用**，任何异常都会顺着调用方（菜单开关/通知点击）
        //   冒泡到主线程。现在失败只记日志并排一次重试，玩家最多「这一次没弹出」。
        try {
            Activity host = focusedHost;
            if (host == null && pendingHostActivity != null) {
                host = pendingHostActivity.get();
            }
            showMenuNative(host);
        } catch (Throwable t) {
            Log.e(TAG, "showMenu failed → 排一次重试", t);
            try {
                tryShowMenuWithRetry(0);
            } catch (Throwable ignored) {
            }
        }
    }

    private void bootstrapNativeIfNeeded() {
        if (!nativeBootstrapped) {
            nativeBootstrapped = true;
            // 乐变 overlay 接管 Application（LEBIAN_APPLICATION）时实测既不回调 attachBaseContext、
            // 也不回调 StarryApp.onCreate（2026-10-21 无悔华夏 B 站包）⇒ 由真正会跑的 Service 兜底加载 so。
            // 正常路径下 attachBaseContext 已加载（isDone=true），这里是幂等 no-op。
            if (!com.starry.magic.ota.StarryNativeLoad.isDone()) {
                boolean loaded = com.starry.magic.ota.StarryNativeLoad.ensureLoaded(this);
                Log.i(TAG, "bootstrap native ensureLoaded=" + loaded
                        + " path=" + com.starry.magic.ota.StarryNativeLoad.loadedPath());
            }
            try {
                ModBridge.nativeInit(this);
            } catch (UnsatisfiedLinkError e) {
                Log.w(TAG, "nativeInit failed: " + e.getMessage());
            }
        }
        // 每次开菜单都尝试应用本地 patch（幂等）；避免只跑一次时序错过
        try {
            com.starry.magic.ota.PlayOtaManager.applyLocalPatchIfAny(this);
        } catch (Throwable t) {
            Log.w(TAG, "applyLocalPatch: " + t.getMessage());
        }
    }

    private boolean showMenuNative(Activity host) {
        // 独立助手模式：强制走 OVERLAY（host=null → tryAttachActivity 直接 false → tryAttachOverlay）
        if (standalone()) host = null;
        com.starry.magic.auth.AuthClient.init(this);
        Preferences.with(this);
        ModBridge.syncAuthGameIdFromNative();
        if (nativeMenu == null) {
            nativeMenu = new NativeMenuController(this);
        }
        boolean ok = nativeMenu.show(host);
        if (ok) {
            bootstrapNativeIfNeeded();
            isMenuVisible = true;
            overlayAttached = true;
            Log.i(TAG, "native menu shown mode=" + nativeMenu.getAttachMode());
        }
        return ok;
    }

    /**
     * 隐藏菜单（完全移除）
     */
    public void hideMenu() {
        if (nativeMenu != null) {
            nativeMenu.hide();
            nativeMenu = null;
        }
        isMenuVisible = false;
        overlayAttached = false;
        nativeBootstrapped = false;
        Log.i(TAG, "Native menu hidden");
    }

    /** 设置页「隐藏菜单」：摘掉悬浮 UI，功能与服务继续；摇一摇恢复。 */
    public void hideMenuFromUser() {
        mainHandler.post(() -> {
            if (menuUserHidden) return;
            menuUserHidden = true;
            if (nativeMenu != null) {
                nativeMenu.collapse();
                nativeMenu.dismissTransientUi();
            }
            Toast.makeText(this, "菜单已隐藏，摇一摇可恢复", Toast.LENGTH_SHORT).show();
            setOverlayVisible(false);
            startShakeListener();
            Log.i(TAG, "menu hidden by user (shake to restore)");
        });
    }

  /** 摇一摇等触发：恢复用户隐藏的菜单。 */
    public void restoreMenuFromUserHidden(String reason) {
        mainHandler.post(() -> {
            if (!menuUserHidden) return;
            menuUserHidden = false;
            stopShakeListener();
            suppressShowUntilElapsed = 0L;
            Activity host = resolveMenuHost(focusedHost);
            if (host != null && nativeMenu != null && nativeMenu.isAttached()) {
                nativeMenu.rebind(host);
            }
            syncOverlayForHost("restore:" + reason);
            if (nativeMenu != null) {
                nativeMenu.showToast("菜单已恢复");
            }
            Log.i(TAG, "menu restored (" + reason + ")");
        });
    }

    public boolean isMenuUserHidden() {
        return menuUserHidden;
    }

    private void startShakeListener() {
        if (shakeSensorManager == null) {
            shakeSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        }
        if (shakeSensorManager == null) return;
        if (shakeListener != null) return;
        shakeListener = new SensorEventListener() {
            private float lastX, lastY, lastZ;
            private boolean hasLast = false;

            @Override
            public void onSensorChanged(SensorEvent event) {
                if (!menuUserHidden || event.sensor.getType() != Sensor.TYPE_ACCELEROMETER) return;
                float x = event.values[0];
                float y = event.values[1];
                float z = event.values[2];
                if (!hasLast) {
                    lastX = x;
                    lastY = y;
                    lastZ = z;
                    hasLast = true;
                    return;
                }
                float dx = x - lastX;
                float dy = y - lastY;
                float dz = z - lastZ;
                lastX = x;
                lastY = y;
                lastZ = z;
                float delta = (float) Math.sqrt(dx * dx + dy * dy + dz * dz);
                if (delta < SHAKE_ACCEL_THRESHOLD) return;
                long now = SystemClock.uptimeMillis();
                if (now - lastShakeAtElapsed < SHAKE_COOLDOWN_MS) return;
                lastShakeAtElapsed = now;
                restoreMenuFromUserHidden("shake");
            }

            @Override
            public void onAccuracyChanged(Sensor sensor, int accuracy) {}
        };
        Sensor accel = shakeSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        if (accel != null) {
            shakeSensorManager.registerListener(shakeListener, accel, SensorManager.SENSOR_DELAY_UI);
            Log.i(TAG, "shake listener started");
        } else {
            Log.w(TAG, "shake listener: no accelerometer");
        }
    }

    private void stopShakeListener() {
        if (shakeSensorManager != null && shakeListener != null) {
            shakeSensorManager.unregisterListener(shakeListener);
        }
        shakeListener = null;
        lastShakeAtElapsed = 0L;
    }

    public void collapseToBall() {
        if (nativeMenu != null) {
            nativeMenu.collapse();
        }
    }

    public void expandFromBall() {
        if (nativeMenu != null) {
            nativeMenu.expand();
        }
    }

    public void showToast(final String text) {
        showToast(text, "info");
    }

    public void showToast(final String text, final String type) {
        if (nativeMenu != null) {
            nativeMenu.showToast(text);
            return;
        }
        mainHandler.post(() -> Toast.makeText(this, text != null ? text : "", Toast.LENGTH_SHORT).show());
    }

    /** 公告由 NativeMenuController bootstrap 拉取；保留空实现供旧 JNI/注入兼容 */
    public void showAnnouncement(String title, String content) {
        Log.d(TAG, "showAnnouncement ignored (native bootstrap handles announcements)");
    }

    public void hideAnnouncement() {
        Log.d(TAG, "hideAnnouncement ignored (native bootstrap handles announcements)");
    }

    public void setVisibility(int visibility) {
        setOverlayVisible(visibility == View.VISIBLE);
    }

    /**
     * 显示/隐藏整个悬浮层。
     * OVERLAY 模式：removeView 避免 ColorOS「正在当前页面悬浮显示…」警告。
     */
    public void setOverlayVisible(boolean visible) {
        if (nativeMenu != null) {
            nativeMenu.setVisible(visible);
            overlayAttached = visible && nativeMenu.isAttached();
            Log.i(TAG, "overlayVisible=" + visible + " attached=" + overlayAttached);
        }
    }

    // ===== Legacy HTML overlay stubs (ModBridge / 旧注入兼容) =====

    public void setOverlayDragActive(boolean active) {
        // legacy HTML overlay drag; no-op in native UI
    }

    public void setOverlayPopupFullscreen(boolean full) {
        // legacy HTML overlay fullscreen; no-op in native UI
    }

    public void exitPopupFullscreenToFrame(int x, int y, int w, int h) {
        // legacy HTML overlay frame; no-op in native UI
    }

    public boolean isOverlayPopupFullscreen() {
        return true;
    }

    public void queueOverlayFrame(int x, int y, int w, int h) {
        // legacy HTML overlay frame; no-op in native UI
    }

    public void setOverlayFrame(int x, int y, int w, int h) {
        // legacy HTML overlay frame; no-op in native UI
    }

    public void beginOverlayLocalDrag(int x, int y, int w, int h) {
        // legacy HTML overlay drag; no-op in native UI
    }

    public void endOverlayLocalDrag() {
        // legacy HTML overlay drag; no-op in native UI
    }

    public String getOverlayTouchRawJSON() {
        DisplayMetrics dm = getResources().getDisplayMetrics();
        int sw = Math.max(dm.widthPixels, 1);
        int sh = Math.max(dm.heightPixels, 1);
        int cw = Math.max(1, Math.round(sw / dm.density));
        int ch = Math.max(1, Math.round(sh / dm.density));
        return "{\"x\":0,\"y\":0,\"sw\":" + sw + ",\"sh\":" + sh
                + ",\"cw\":" + cw + ",\"ch\":" + ch + ",\"t\":0}";
    }

    public float getTouchRawX() { return 0f; }
    public float getTouchRawY() { return 0f; }

    public int getScreenW() {
        return getResources().getDisplayMetrics().widthPixels;
    }

    public int getScreenH() {
        return getResources().getDisplayMetrics().heightPixels;
    }

    public void setFocusable(boolean focusable) {
        overlayInputFocus = focusable;
    }

    public void setMenuPanelExpanded(boolean expanded) {
        // legacy HTML overlay alpha; native panel handles its own layout
    }

    public void evaluateJs(final String script) {
        // legacy HTML bridge; no-op in native UI
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        stopShakeListener();
        mainHandler.removeCallbacks(heartbeatRunnable);
        unregisterResumeWatcher();
        hideMenu();
        instance = null;
        Log.i(TAG, "MenuService destroyed");
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        // 部分 ROM 在 onCreate 与 onStartCommand 之间仍计 5s 超时；此处再保一次
        try {
            startForegroundCompat();
        } catch (Throwable t) {
            Log.e(TAG, "onStartCommand startForeground failed", t);
        }
        if (standalone()) {
            // 独立助手（com.starry.magic）：进程里永远没有游戏 Activity，「无宿主」
            // 是常态而非异常 ⇒ 不设丧尸看门狗、START_STICKY 保活，让悬浮球在
            // 「划掉最近任务 / 系统回收内存」之后被系统自动重建，用户不必再开 App。
            Log.i(TAG, "standalone overlay service → 常驻模式（START_STICKY，无宿主看门狗停用）");
            mainHandler.post(() -> ensureMenuAfterPermissionGranted("onStartCommand(standalone)"));
            return START_STICKY;
        }
        if (startedActivityCount > 0 || resumedActivityCount > 0) {
            // 进程里有活的游戏 Activity：正常路径，立即挂菜单
            mainHandler.post(() -> ensureMenuAfterPermissionGranted("onStartCommand"));
        } else {
            // 无活宿主（AMS 崩溃重启 / Application.onCreate 早于 Activity）：
            // 先不挂菜单，等 Activity 起来（onActivityResumed 会重新 ensure）；
            // 25s 内 Activity 没来 → 僵尸服务进程，自杀。
            Log.i(TAG, "no host alive at onStartCommand → defer menu, 25s watchdog");
            mainHandler.removeCallbacks(zombieWatchdog);
            mainHandler.postDelayed(zombieWatchdog, 25_000);
        }
        // START_NOT_STICKY：游戏进程死亡（崩溃/被杀）后系统不得重建本服务——
        // START_STICKY 会复活出一个只有菜单壳、没有游戏的僵尸进程，悬浮球和
        // 远程公告会一直挂在屏幕上（战魂铭人 14:01 崩溃后实测复现）。
        return START_NOT_STICKY;
    }

    /** 僵尸服务看门狗：AMS 崩溃重启出的进程里只有服务、没有游戏 Activity，
     *  宽限期满仍无宿主则自杀（MuMu 上滑杀进程实测：AMS 按 start-requested
     *  重启服务 → Application.onCreate 注入点重新调 start() → OVERLAY 模式
     *  菜单挂上僵尸进程。挂载前必须有活宿主）。 */
    private final Runnable zombieWatchdog = new Runnable() {
        @Override public void run() {
            if (standalone()) {
                // 独立助手没有游戏宿主，这里永远为 0 ⇒ 绝不能自杀
                Log.i(TAG, "standalone → 看门狗豁免（无宿主是常态）");
                return;
            }
            if (startedActivityCount == 0 && resumedActivityCount == 0) {
                Log.i(TAG, "no host within 25s → zombie service, stopSelf");
                stopSelf();
            }
        }
    };

    @Override
    public void onTaskRemoved(Intent rootIntent) {
        // 上滑清任务时服务跟着结束：配合 Manifest stopWithTask="true"。
        // 部分模拟器 ROM（MuMu 实测）上滑走的是「直接杀进程」路径，本回调
        // 可能不触发；AMS 若按 start-requested 重启服务，由 onStartCommand
        // 的活宿主检查 + 看门狗兜底。stopSelf 覆盖「移除任务」路径。
        if (standalone()) {
            // 独立助手是玩家唯一的功能开关入口（游戏是官方原包，菜单进不去）
            // ⇒ 划掉最近任务不能让悬浮球消失。语义：服务常驻，不随行退出。
            Log.i(TAG, "standalone → task removed 但保持常驻（悬浮球继续）");
            super.onTaskRemoved(rootIntent);
            return;
        }
        Log.i(TAG, "task removed → stopSelf（游戏关闭，菜单服务随行）");
        stopSelf();
        super.onTaskRemoved(rootIntent);
    }


    /** 通知点一下就回助手主页（用户动作 ⇒ 允许重新拉起前台服务；国产 ROM 里这是
     *  「球被系统杀了之后最省事的一条自救路径」）。 */
    private android.app.PendingIntent reopenIntent() {
        Intent open = new Intent(this, com.starry.magic.app.MainActivity.class);
        open.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP);
        int flags = android.app.PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            flags |= android.app.PendingIntent.FLAG_IMMUTABLE;
        }
        return android.app.PendingIntent.getActivity(this, NOTIFICATION_ID, open, flags);
    }

    @SuppressWarnings("DEPRECATION")
    private void startForegroundCompat() {
        // Split APK 的 uses-permission 不会并入部分 ROM 的 requested permissions。
        // MenuService 由普通 startService 启动，没有 5 秒前台化义务；无权限时保持
        // 普通 Service 即可，避免 SecurityException 污染启动日志。
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M
                && checkSelfPermission(android.Manifest.permission.FOREGROUND_SERVICE)
                != PackageManager.PERMISSION_GRANTED) {
            Log.w(TAG, "FOREGROUND_SERVICE not granted; menu continues as plain service");
            return;
        }
        NotificationManager nm = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
        Notification notification;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "菜单服务",
                    NotificationManager.IMPORTANCE_LOW
            );
            channel.setDescription("MOD 菜单前台服务");
            channel.setShowBadge(false);
            if (nm != null) {
                nm.createNotificationChannel(channel);
            }
            Notification.Builder builder = new Notification.Builder(this, CHANNEL_ID)
                    .setContentTitle("星光助手")
                    .setContentText("菜单运行中 · 点此恢复菜单球")
                    .setSmallIcon(android.R.drawable.ic_menu_info_details)
                    .setContentIntent(reopenIntent())
                    .setOngoing(true);
            notification = builder.build();
        } else {
            notification = new Notification.Builder(this)
                    .setContentTitle("星光助手")
                    .setContentText("菜单运行中 · 点此恢复菜单球")
                    .setSmallIcon(android.R.drawable.ic_menu_info_details)
                    .setContentIntent(reopenIntent())
                    .setOngoing(true)
                    .build();
        }

        // 外科 zip 若只等长劫持官方 Service 名，Manifest 常无 foregroundServiceType；
        // Android 14+ SPECIAL_USE 会 IllegalArgumentException，必须回退无 type。
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE) {
            try {
                startForeground(NOTIFICATION_ID, notification, ServiceInfo.FOREGROUND_SERVICE_TYPE_SPECIAL_USE);
                return;
            } catch (IllegalArgumentException e) {
                Log.w(TAG, "SPECIAL_USE not in Manifest, fallback plain startForeground", e);
            }
        }
        startForeground(NOTIFICATION_ID, notification);
    }
}
