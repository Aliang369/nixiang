package com.starry.magic.ltzj;

import android.content.Context;
import android.util.Log;

import com.starry.magic.menu.Preferences;
import com.starry.magic.ota.PlayOtaPaths;

import java.io.File;
import java.io.IOException;
import java.lang.reflect.Array;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

/**
 * 把 starry.dex 挂进应用 PathClassLoader，使 Manifest 声明的 MenuService
 * 能被系统 instantiateService 找到（仅 DexClassLoader 不够）。
 */
public final class DexInstaller {
    private static final String TAG = "LtzjDex";

    private DexInstaller() {}

    /**
     * 零 Root 目标入口：不依赖 JSB 传 Context。
     * 前置：本类已在 PathCL（通常由 so JNI_OnLoad / 晚载注入），且
     * {@code files/starry_dex/starry.dex}、{@code files/starry_native/*.so} 已由 JS 从 assets 落盘。
     * JSB：{@code callStaticMethod(".../DexInstaller", "bootZeroRoot", "()Z")}
     */
    public static boolean bootZeroRoot() {
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (!(app instanceof Context)) {
                Log.e(TAG, "bootZeroRoot: currentApplication null");
                return false;
            }
            Context ctx = ((Context) app).getApplicationContext();
            File dex = new File(ctx.getFilesDir(), "starry_dex/starry.dex");
            if (!dex.isFile()) {
                Log.e(TAG, "bootZeroRoot: dex missing " + dex.getAbsolutePath());
                return false;
            }
            boot(ctx, dex.getAbsolutePath());
            return true;
        } catch (Throwable t) {
            Log.e(TAG, "bootZeroRoot failed", t);
            return false;
        }
    }

    public static void boot(Context ctx, String dexPath) throws Exception {
        File dex = new File(dexPath);
        if (!dex.isFile()) {
            throw new IllegalArgumentException("dex missing: " + dexPath);
        }
        ClassLoader pathCl = ctx.getClassLoader();
        install(pathCl, dex, ctx.getCodeCacheDir());
        // 必须绑到 PathClassLoader：System.load 会绑到「调用方」ClassLoader（此处常是
        // 加载 DexInstaller 的 DexClassLoader），导致正式 MenuService/ModBridge（PathCL）
        // 出现 already opened / No implementation found。
        ensureNativeLinked(ctx);
        Log.i(TAG, "dex injected into PathClassLoader, starting MenuService");
        // 反射 + PathCL：避免 DexInstaller(DexCL) 静态引用把 MenuService 加载进 DexCL
        Class<?> ms = Class.forName("com.starry.magic.menu.MenuService", true, pathCl);
        Method start = ms.getMethod("start", Context.class);
        start.invoke(null, ctx);
    }

    /** 用应用 PathClassLoader 关联 libstarry_magic（绝对路径优先，兼容晚加载）。 */
    public static void ensureNativeLinked(Context ctx) {
        ClassLoader pathCl = ctx.getClassLoader();
        Preferences.with(ctx);
        String[] cxx = new String[] {
                new File(ctx.getFilesDir(), "starry_native/libc++_shared.so").getAbsolutePath(),
                new File(ctx.getApplicationInfo().nativeLibraryDir, "libc++_shared.so").getAbsolutePath(),
                "/data/local/tmp/ltzj_st/libc++_shared.so",
        };
        for (String path : cxx) {
            if (!new File(path).isFile()) continue;
            if (loadInto(pathCl, path, "cxx")) break;
        }
        // 优先 OTA so；若 OTA 存在则不再 load 包内 nativeLibraryDir / loadLibrary
        java.util.ArrayList<String> candidates = new java.util.ArrayList<>();
        File ota = PlayOtaPaths.resolveOtaSo(ctx);
        if (ota == null) ota = com.starry.magic.ota.StarryNativeLoad.findOtaSo(ctx);
        if (ota != null) {
            candidates.add(ota.getAbsolutePath());
        } else {
            candidates.add(new File(ctx.getFilesDir(), "starry_native/libstarry_magic.so").getAbsolutePath());
            candidates.add(new File(ctx.getApplicationInfo().nativeLibraryDir, "libstarry_magic.so").getAbsolutePath());
            candidates.add("/data/local/tmp/ltzj_st/libstarry_magic.so");
        }
        for (String path : candidates) {
            if (!new File(path).isFile()) continue;
            if (loadInto(pathCl, path, "magic")) {
                Log.i(TAG, "System.load ok: " + path);
                return;
            }
        }
        if (ota != null) {
            Log.e(TAG, "OTA so present but load failed, skip apk loadLibrary");
            return;
        }
        try {
            System.loadLibrary("starry_magic");
            Log.i(TAG, "System.loadLibrary(starry_magic) ok");
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "native still unlinked: " + e.getMessage());
        }
    }

    /** 经 PathCL 加载 NativeLinker 再 System.load，避免绑到 DexCL。 */
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
