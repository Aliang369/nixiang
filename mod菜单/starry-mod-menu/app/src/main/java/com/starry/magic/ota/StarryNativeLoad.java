package com.starry.magic.ota;

import android.content.Context;
import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.util.concurrent.atomic.AtomicBoolean;

/**
 * 统一加载 libstarry_magic：有 OTA 文件则只 load OTA，禁止再 loadLibrary 钉死包内旧 so。
 * <p>
 * 覆盖安装新 APK 时会先走 {@link PlayOtaManager#invalidateOtaIfApkUpdated}，
 * 避免旧 {@code files/starry_ota/} 挡住包内新 so。
 */
public final class StarryNativeLoad {
    private static final String TAG = "StarryNativeLoad";
    private static final AtomicBoolean DONE = new AtomicBoolean(false);
    private static volatile String loadedPath = "";
    /** 每个 classloader 独占的副本名前缀/槽位（乐变 overlay 二次实例化 Application 时用）。 */
    private static final String COPY_PREFIX = "libstarry_magic_ld_";
    private static final String COPY_SLOT =
            Long.toHexString(new java.util.Random().nextLong() & 0x7fffffffffffffffL);

    private StarryNativeLoad() {}

    private static String copyName() {
        return COPY_PREFIX + COPY_SLOT + ".so";
    }

    /**
     * 同一进程内乐变 overlay 会把 Application/Service 二次实例化到另一个 classloader，
     * 而 Android nativeloader 禁止同一个 so <b>文件</b>被两个 classloader 打开
     * （{@code UnsatisfiedLinkError: Shared library ... already opened by ClassLoader ...}）。
     * 这里给本 classloader 复制一份独占副本再 load —— 内容仍是 OTA so，
     * 不退回包内旧 so（差异探针语义不变）。
     */
    private static File copyForThisClassLoader(File ota) {
        try {
            File dir = ota.getParentFile();
            if (dir == null || !dir.isDirectory()) return null;
            File[] kids = dir.listFiles();
            if (kids != null) {
                long now = System.currentTimeMillis();
                for (File f : kids) {
                    String n = f.getName();
                    if (!n.startsWith(COPY_PREFIX) || n.equals(copyName())) continue;
                    // 已 dlopen 的副本删掉也安全（inode 仍在）；只清 60s 前的，避开并发加载
                    if (now - f.lastModified() > 60000L) {
                        //noinspection ResultOfMethodCallIgnored
                        f.delete();
                    }
                }
            }
            File dst = new File(dir, copyName());
            FileInputStream in = new FileInputStream(ota);
            FileOutputStream out = new FileOutputStream(dst);
            try {
                byte[] buf = new byte[64 * 1024];
                int n;
                while ((n = in.read(buf)) > 0) out.write(buf, 0, n);
            } finally {
                try { in.close(); } catch (Throwable ignored) { }
                try { out.close(); } catch (Throwable ignored) { }
            }
            if (!dst.isFile() || dst.length() != ota.length()) return null;
            return dst;
        } catch (Throwable t) {
            Log.w(TAG, "copy OTA so for classloader failed: " + t);
            return null;
        }
    }

    public static boolean isDone() {
        return DONE.get();
    }

    public static String loadedPath() {
        return loadedPath;
    }

    /**
     * SecShell APK cannot accept a new libstarry_magic.so entry. The surgical
     * package stores the statically-linked library under an existing x86-only
     * native entry instead; arm64 devices never load that entry through the
     * normal game path, so this alias keeps the APK single-file.
     */
    private static boolean tryLoadPackagedAlias(Context ctx) {
        if (ctx == null || !"com.qxjc.hxwjmlq".equals(ctx.getPackageName())) {
            return false;
        }
        try {
            System.loadLibrary("SecShell-x86");
            loadedPath = "loadLibrary(SecShell-x86)";
            DONE.set(true);
            Log.i(TAG, "loaded packaged alias so via loadLibrary");
            try {
                com.starry.magic.bridge.ModBridge.runBridgeSelfTest();
            } catch (Throwable t) {
                Log.w(TAG, "bridge self test failed", t);
            }
            return true;
        } catch (UnsatisfiedLinkError e) {
            Log.w(TAG, "loadLibrary SecShell-x86 failed", e);
            return false;
        }
    }

    /** Scan starry_ota game dirs for libstarry_magic.so without prefs. */
    public static File findOtaSo(Context ctx) {
        if (ctx == null) return null;
        File fromPrefs = PlayOtaPaths.resolveOtaSo(ctx);
        if (fromPrefs != null && fromPrefs.isFile()) return fromPrefs;
        File root = new File(ctx.getFilesDir(), PlayOtaPaths.DIR_NAME);
        if (!root.isDirectory()) return null;
        File[] kids = root.listFiles();
        if (kids == null) return null;
        File best = null;
        long bestM = -1;
        for (File d : kids) {
            if (!d.isDirectory()) continue;
            File so = new File(d, "libstarry_magic.so");
            if (!so.isFile()) continue;
            long m = so.lastModified();
            if (m >= bestM) {
                bestM = m;
                best = so;
            }
        }
        return best;
    }

    /**
     * @return true 已加载（本次或此前）
     */
    public static boolean ensureLoaded(Context ctx) {
        if (DONE.get()) return true;
        synchronized (StarryNativeLoad.class) {
            if (DONE.get()) return true;
            // 覆盖安装新 APK 后清掉旧 OTA，否则会一直 load 旧补丁 so
            try {
                if (PlayOtaManager.invalidateOtaIfApkUpdated(ctx)) {
                    Log.i(TAG, "cleared stale OTA after APK update");
                }
            } catch (Throwable t) {
                Log.w(TAG, "invalidateOtaIfApkUpdated: " + t.getMessage());
            }
            File ota = findOtaSo(ctx);
            if (ota != null) {
                try {
                    System.load(ota.getAbsolutePath());
                    loadedPath = ota.getAbsolutePath();
                    DONE.set(true);
                    Log.i(TAG, "loaded OTA so: " + loadedPath);
                    return true;
                } catch (Throwable t) {
                    Log.e(TAG, "load OTA so failed: " + ota.getAbsolutePath(), t);
                    // 多 classloader 冲突（乐变 overlay 二次实例化 Application/Service）：
                    // 同一 so 文件不能被两个 classloader 打开 → 复制本 classloader 独占副本再 load。
                    File copy = copyForThisClassLoader(ota);
                    if (copy != null) {
                        try {
                            System.load(copy.getAbsolutePath());
                            loadedPath = copy.getAbsolutePath();
                            DONE.set(true);
                            Log.i(TAG, "loaded OTA so (classloader-private copy): " + loadedPath);
                            return true;
                        } catch (Throwable t2) {
                            Log.e(TAG, "load OTA so copy failed: " + copy.getAbsolutePath(), t2);
                        }
                    }
                    // 最后兜底：loadLibrary 本 classloader 自带的那份（包内 / 乐变负载内）。
                    // 宁可菜单可用，也不要在这个 classloader 里留一个没 native 的死菜单。
                    if (tryLoadPackagedAlias(ctx)) return true;
                    try {
                        System.loadLibrary("starry_magic");
                        loadedPath = "loadLibrary(starry_magic)";
                        DONE.set(true);
                        Log.w(TAG, "OTA so unusable in this classloader; loaded local so via loadLibrary");
                        return true;
                    } catch (UnsatisfiedLinkError e) {
                        Log.e(TAG, "loadLibrary starry_magic failed", e);
                    }
                    return false;
                }
            }
            if (tryLoadPackagedAlias(ctx)) return true;
            try {
                System.loadLibrary("starry_magic");
                loadedPath = "loadLibrary(starry_magic)";
                DONE.set(true);
                Log.i(TAG, "loaded apk so via loadLibrary");
                // 桥接自检：so 加载成功即跑（RegisterNatives 迁移配套，自测壳验证用）
                try {
                    com.starry.magic.bridge.ModBridge.runBridgeSelfTest();
                } catch (Throwable t) {
                    Log.w(TAG, "bridge self test failed", t);
                }
                return true;
            } catch (UnsatisfiedLinkError e) {
                Log.e(TAG, "loadLibrary starry_magic failed", e);
                return false;
            }
        }
    }
}
