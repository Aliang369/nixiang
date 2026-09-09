package com.starry.magic.ota;

import android.content.Context;
import android.util.Log;

import java.io.File;
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

    private StarryNativeLoad() {}

    public static boolean isDone() {
        return DONE.get();
    }

    public static String loadedPath() {
        return loadedPath;
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
                    // 有 OTA 文件却 load 失败时，不要静默退回 APK（否则差异探针永远看不到）
                    return false;
                }
            }
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
