package com.starry.magic.wgdzz;

import android.app.Activity;
import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.starry.magic.menu.Preferences;
import com.starry.magic.ota.PlayOtaPaths;
import com.starry.magic.sign.SignSpoof;

import java.io.File;
import java.io.IOException;
import java.lang.reflect.Array;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * 装机后 swap 官方 base.apk 时，classes5 不在 APK 内；由 native 从
 * {@code /data/local/tmp/wgdzz_classes5.dex} 侧载并注入 PathClassLoader。
 */
public final class DexInstaller {
    private static final String TAG = "WgdzzDex";

    private DexInstaller() {}

    public static void boot(Context ctx, String dexPath) throws Exception {
        try {
            SignSpoof.install();
        } catch (Throwable t) {
            Log.w(TAG, "SignSpoof.install: " + t.getMessage());
        }
        File dex = new File(dexPath);
        if (!dex.isFile()) {
            throw new IllegalArgumentException("dex missing: " + dexPath);
        }
        ClassLoader pathCl = ctx.getClassLoader();
        install(pathCl, dex, ctx.getCodeCacheDir());
        Log.i(TAG, "dex injected, schedule menu");
        if (dexPath.contains("wgdzz_classes5")) {
            scheduleSwapDirectMenu(pathCl);
        } else {
            scheduleMenuStart(ctx, pathCl);
        }
        if (!dexPath.contains("wgdzz_classes5")) {
            ensureNativeLinked(ctx);
        } else {
            Log.i(TAG, "wgdzz sidecar: skip ensureNativeLinked (KwProtect dlopen path)");
        }
    }

    /** swap 官方 base 后 Manifest 无 MenuService，直接挂 Activity 原生菜单。 */
    private static void scheduleSwapDirectMenu(ClassLoader pathCl) {
        Thread t = new Thread(() -> {
            for (int i = 1; i <= 240; i++) {
                try {
                    Activity act = findResumedActivity();
                    if (act != null) {
                        final Activity host = act;
                        postSwapMenuBoot(pathCl, host, i);
                        return;
                    }
                    if (i % 20 == 0) {
                        Log.i(TAG, "swap menu waiting Activity (try " + i + ")");
                    }
                    Thread.sleep(500);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                    return;
                } catch (Throwable e) {
                    Log.w(TAG, "swap menu wait try " + i + ": " + e.getMessage());
                    try {
                        Thread.sleep(500);
                    } catch (InterruptedException ie) {
                        Thread.currentThread().interrupt();
                        return;
                    }
                }
            }
            Log.e(TAG, "swap direct menu gave up (no Activity)");
        }, "WgdzzSwapMenu");
        t.setDaemon(true);
        t.start();
    }

    private static void postSwapMenuBoot(ClassLoader pathCl, Activity host, int attempt) {
        Handler main = new Handler(Looper.getMainLooper());
        main.post(() -> {
            if (bootSwapDirectMenu(pathCl, host)) {
                Log.i(TAG, "swap direct menu ok host=" + host.getClass().getSimpleName());
                return;
            }
            main.postDelayed(() -> {
                Activity retry = findResumedActivity();
                if (retry != null && bootSwapDirectMenu(pathCl, retry)) {
                    Log.i(TAG, "swap direct menu ok (retry) host=" + retry.getClass().getSimpleName());
                } else {
                    Log.w(TAG, "swap direct menu retry failed (attempt " + attempt + ")");
                }
            }, 2000);
        });
    }

    private static boolean bootSwapDirectMenu(ClassLoader pathCl, Activity host) {
        try {
            Class<?> authCls = Class.forName("com.starry.magic.auth.AuthClient", true, pathCl);
            authCls.getMethod("init", Context.class).invoke(null, host);
            Preferences.with(host);
            Class<?> bridge = Class.forName("com.starry.magic.bridge.ModBridge", true, pathCl);
            bridge.getMethod("nativeInit", Context.class).invoke(null, host);
            bridge.getMethod("syncAuthGameIdFromNative").invoke(null);
            Log.i(TAG, "auth game_id synced (swap direct)");
            Class<?> menuCls = Class.forName("com.starry.magic.menu.NativeMenuController", true, pathCl);
            Object menu = menuCls.getConstructor(Context.class).newInstance(host);
            Object ok = menuCls.getMethod("show", Activity.class).invoke(menu, host);
            return Boolean.TRUE.equals(ok);
        } catch (Throwable e) {
            Log.e(TAG, "swap direct menu failed: " + e.getMessage());
            return false;
        }
    }

    private static void scheduleMenuStart(Context ctx, ClassLoader pathCl) {
        final Context appCtx = ctx.getApplicationContext() != null ? ctx.getApplicationContext() : ctx;
        Thread t = new Thread(() -> {
            for (int i = 1; i <= 240; i++) {
                try {
                    Activity act = findResumedActivity();
                    Context host = act != null ? act : (i >= 6 ? appCtx : null);
                    if (host == null) {
                        Thread.sleep(500);
                        continue;
                    }
                    if (tryStartMenuService(pathCl, host)) {
                        return;
                    }
                    final Context startCtx = host;
                    new Handler(Looper.getMainLooper()).post(() -> tryStartMenuService(pathCl, startCtx));
                    return;
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                    return;
                } catch (Throwable e) {
                    Log.w(TAG, "MenuService wait try " + i + ": " + e.getMessage());
                    try {
                        Thread.sleep(500);
                    } catch (InterruptedException ie) {
                        Thread.currentThread().interrupt();
                        return;
                    }
                }
            }
            Log.e(TAG, "MenuService.start gave up");
        }, "WgdzzMenuWait");
        t.setDaemon(true);
        t.start();
    }

    private static boolean tryStartMenuService(ClassLoader pathCl, Context ctx) {
        try {
            Class<?> ms = Class.forName("com.starry.magic.menu.MenuService", true, pathCl);
            Method start = ms.getMethod("start", Context.class);
            start.invoke(null, ctx);
            Log.i(TAG, "MenuService.start ok host=" + ctx.getClass().getSimpleName());
            return true;
        } catch (Throwable e) {
            Log.e(TAG, "MenuService.start failed: " + e.getMessage());
            return false;
        }
    }

    private static Activity findResumedActivity() {
        Activity a = findActivityInThread(false);
        return a != null ? a : findActivityInThread(true);
    }

    private static Activity findActivityInThread(boolean allowPaused) {
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object thread = at.getMethod("currentActivityThread").invoke(null);
            Field f = at.getDeclaredField("mActivities");
            f.setAccessible(true);
            Object mActivities = f.get(thread);
            if (mActivities == null) return null;
            Class<?> mapCls = mActivities.getClass();
            Method sizeM = mapCls.getMethod("size");
            Method valueAtM = mapCls.getMethod("valueAt", int.class);
            int n = (Integer) sizeM.invoke(mActivities);
            for (int i = 0; i < n; i++) {
                Object rh = valueAtM.invoke(mActivities, i);
                if (rh == null) continue;
                Class<?> rhCls = rh.getClass();
                if (!allowPaused) {
                    Field paused = rhCls.getDeclaredField("paused");
                    paused.setAccessible(true);
                    if (paused.getBoolean(rh)) continue;
                }
                Field activity = rhCls.getDeclaredField("activity");
                activity.setAccessible(true);
                Object act = activity.get(rh);
                if (!(act instanceof Activity)) continue;
                Activity a = (Activity) act;
                if (a.isFinishing()) continue;
                return a;
            }
        } catch (Throwable t) {
            Log.w(TAG, "findActivityInThread: " + t.getMessage());
        }
        return null;
    }

    public static void ensureNativeLinked(Context ctx) {
        ClassLoader pathCl = ctx.getClassLoader();
        Preferences.with(ctx);
        String[] cxx = new String[] {
                new File(ctx.getFilesDir(), "starry_native/libc++_shared.so").getAbsolutePath(),
                new File(ctx.getApplicationInfo().nativeLibraryDir, "libc++_shared.so").getAbsolutePath(),
        };
        for (String path : cxx) {
            if (!new File(path).isFile()) continue;
            if (loadInto(pathCl, path, "cxx")) break;
        }
        ArrayList<String> candidates = new ArrayList<>();
        File ota = PlayOtaPaths.resolveOtaSo(ctx);
        if (ota == null) ota = com.starry.magic.ota.StarryNativeLoad.findOtaSo(ctx);
        if (ota != null) {
            candidates.add(ota.getAbsolutePath());
        } else {
            candidates.add(new File(ctx.getFilesDir(), "starry_native/libstarry_magic.so").getAbsolutePath());
            candidates.add(new File(ctx.getApplicationInfo().nativeLibraryDir, "libstarry_magic.so").getAbsolutePath());
            candidates.add("/data/local/tmp/wgdzz_libstarry_magic.so");
        }
        for (String path : candidates) {
            if (!new File(path).isFile()) continue;
            if (loadInto(pathCl, path, "magic")) {
                Log.i(TAG, "native linked: " + path);
                return;
            }
        }
        try {
            System.loadLibrary("starry_magic");
            Log.i(TAG, "System.loadLibrary(starry_magic) ok");
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "native still unlinked: " + e.getMessage());
        }
    }

    static boolean loadInto(ClassLoader pathCl, String path, String tag) {
        try {
            Class<?> linker = Class.forName("com.starry.magic.ltzj.NativeLinker", true, pathCl);
            linker.getMethod("load", String.class).invoke(null, path);
            Log.i(TAG, "System.load " + tag + " ok: " + path);
            return true;
        } catch (Throwable t) {
            Throwable c = t.getCause() != null ? t.getCause() : t;
            String msg = c.getMessage() != null ? c.getMessage() : String.valueOf(c);
            if (msg.contains("already opened") && msg.contains("PathClassLoader")) {
                Log.i(TAG, "System.load " + tag + " already in PathCL: " + path);
                return true;
            }
            Log.w(TAG, "System.load " + tag + " failed: " + path + " → " + msg);
            return false;
        }
    }

    public static void install(ClassLoader loader, File dexFile, File optDir) throws Exception {
        Object pathList = getField(loader, "pathList");
        Object[] oldElements = (Object[]) getField(pathList, "dexElements");
        Object[] extra = makeDexElements(pathList, dexFile, optDir);
        Object[] combined = (Object[]) Array.newInstance(
                oldElements.getClass().getComponentType(),
                oldElements.length + extra.length);
        System.arraycopy(extra, 0, combined, 0, extra.length);
        System.arraycopy(oldElements, 0, combined, extra.length, oldElements.length);
        setField(pathList, "dexElements", combined);
        Log.i(TAG, "dexElements " + oldElements.length + " → " + combined.length);
    }

    @SuppressWarnings({"unchecked", "rawtypes"})
    private static Object[] makeDexElements(Object pathList, File dexFile, File optDir)
            throws Exception {
        ArrayList<File> files = new ArrayList<>();
        files.add(dexFile);
        ArrayList<IOException> suppressed = new ArrayList<>();
        try {
            Method m = pathList.getClass().getDeclaredMethod(
                    "makePathElements", List.class, File.class, List.class);
            m.setAccessible(true);
            return (Object[]) m.invoke(pathList, files, optDir, suppressed);
        } catch (NoSuchMethodException ignored) {
        }
        try {
            Method m = pathList.getClass().getDeclaredMethod(
                    "makeDexElements", ArrayList.class, File.class, ArrayList.class, ClassLoader.class);
            m.setAccessible(true);
            return (Object[]) m.invoke(pathList, files, optDir, suppressed, pathList.getClass().getClassLoader());
        } catch (NoSuchMethodException ignored) {
        }
        Method m = pathList.getClass().getDeclaredMethod(
                "makeDexElements", ArrayList.class, File.class, ArrayList.class);
        m.setAccessible(true);
        return (Object[]) m.invoke(pathList, files, optDir, suppressed);
    }

    private static Object getField(Object obj, String name) throws Exception {
        Class<?> c = obj.getClass();
        while (c != null) {
            try {
                Field f = c.getDeclaredField(name);
                f.setAccessible(true);
                return f.get(obj);
            } catch (NoSuchFieldException e) {
                c = c.getSuperclass();
            }
        }
        throw new NoSuchFieldException(name);
    }

    private static void setField(Object obj, String name, Object value) throws Exception {
        Class<?> c = obj.getClass();
        while (c != null) {
            try {
                Field f = c.getDeclaredField(name);
                f.setAccessible(true);
                f.set(obj, value);
                return;
            } catch (NoSuchFieldException e) {
                c = c.getSuperclass();
            }
        }
        throw new NoSuchFieldException(name);
    }
}
