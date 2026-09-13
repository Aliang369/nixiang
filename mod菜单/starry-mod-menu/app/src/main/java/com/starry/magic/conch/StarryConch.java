package com.starry.magic.conch;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.starry.magic.menu.Preferences;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.lang.reflect.Method;

/**
 * LayaAir/Conch runtime bridge for 地城狩猎.
 *
 * <p>The game is a Conch H5 runtime, not a WebView and not Unity. The APK already
 * exposes the native static method {@code layaair.game.browser.ConchJNI.RunJS(String)},
 * which is also used by the game's own Java-to-JS callback path. This bridge injects
 * an idempotent JavaScript patch into that runtime and refreshes the menu state in a
 * global object before each injection.</p>
 *
 * <p>The daemon is intentionally periodic: the DCC system may reload the JavaScript
 * runtime, which clears injected globals. Re-executing the guarded patch is cheap and
 * restores the hook after a reload.</p>
 */
public final class StarryConch {
    private static final String TAG = "StarryConch";
    private static final String PATCH_ASSET = "starry/dcsl_patch.js";
    private static final long FIRST_DELAY_MS = 1200L;
    private static final long INJECT_INTERVAL_MS = 3000L;
    private static final int FEAT_SKILL = 1;

    private static final Handler MAIN = new Handler(Looper.getMainLooper());
    private static volatile Context sAppContext;
    private static volatile String sPatchSource;
    private static volatile boolean sStarted;
    private static volatile int sInjectCount;

    private StarryConch() {}

    /** Ack from the injected Conch patch, visible in logcat as StarryConch. */
    public static void ack(String detail) {
        Log.i(TAG, "patch ack: " + (detail == null ? "" : detail));
    }

    public static void start(Context context) {
        if (context == null) {
            return;
        }
        Context app = context.getApplicationContext();
        sAppContext = app != null ? app : context;
        try {
            Preferences.with(sAppContext);
        } catch (Throwable t) {
            Log.w(TAG, "Preferences init failed: " + t);
        }
        if (sStarted) {
            return;
        }
        synchronized (StarryConch.class) {
            if (sStarted) {
                return;
            }
            sStarted = true;
            MAIN.postDelayed(new Runnable() {
                @Override
                public void run() {
                    tick();
                }
            }, FIRST_DELAY_MS);
        }
        Log.i(TAG, "Conch JS daemon started");
    }

    private static void tick() {
        try {
            inject();
        } catch (Throwable t) {
            Log.w(TAG, "inject failed: " + t);
        } finally {
            MAIN.postDelayed(new Runnable() {
                @Override
                public void run() {
                    tick();
                }
            }, INJECT_INTERVAL_MS);
        }
    }

    private static void inject() throws Exception {
        Context ctx = sAppContext;
        if (ctx == null) {
            return;
        }
        String patch = patchSource(ctx);
        if (patch == null || patch.isEmpty()) {
            return;
        }

        String script = "window.__starryDcslState=" + stateJson() + ";\n" + patch;
        Class<?> conch = Class.forName("layaair.game.browser.ConchJNI");
        Method runJs = conch.getDeclaredMethod("RunJS", String.class);
        runJs.setAccessible(true);
        runJs.invoke(null, script);

        int n = ++sInjectCount;
        if (n <= 5 || n % 40 == 0) {
            Log.i(TAG, "RunJS inject #" + n + " (" + script.length() + " chars)");
        }
    }

    private static String stateJson() {
        boolean live = liveSession();
        boolean skill = live && Preferences.loadPrefBool(FEAT_SKILL, false);
        return "{\"live\":" + (live ? 1 : 0)
                + ",\"skill\":" + (skill ? 1 : 0)
                + "}";
    }

    private static boolean liveSession() {
        try {
            if (!Preferences.isMenuEnabled()
                    || !Preferences.isServerFeatureEnabled(FEAT_SKILL)) {
                return false;
            }
            Boolean nativeLive = nativeSessionLive();
            if (nativeLive != null) {
                return nativeLive.booleanValue();
            }
            return Preferences.isFeaturesAllowed();
        } catch (Throwable t) {
            Log.w(TAG, "liveSession failed closed: " + t);
            return false;
        }
    }

    private static Boolean nativeSessionLive() {
        try {
            Class<?> bridge = Class.forName("com.starry.magic.bridge.ModBridge");
            Method method = bridge.getDeclaredMethod("nativeHasLiveSession");
            method.setAccessible(true);
            Object result = method.invoke(null);
            if (result instanceof Boolean) {
                return (Boolean) result;
            }
        } catch (Throwable t) {
            Log.w(TAG, "nativeHasLiveSession unavailable: " + t);
        }
        return null;
    }

    private static String patchSource(Context ctx) {
        String cached = sPatchSource;
        if (cached != null) {
            return cached;
        }
        InputStream in = null;
        try {
            in = ctx.getAssets().open(PATCH_ASSET);
            ByteArrayOutputStream out = new ByteArrayOutputStream(16384);
            byte[] buf = new byte[8192];
            int n;
            while ((n = in.read(buf)) > 0) {
                out.write(buf, 0, n);
            }
            String src = new String(out.toByteArray(), "UTF-8");
            sPatchSource = src;
            Log.i(TAG, "patch loaded: " + PATCH_ASSET + " (" + src.length() + " chars)");
            return src;
        } catch (Throwable t) {
            Log.e(TAG, "patch asset missing: " + PATCH_ASSET, t);
            return null;
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (Throwable ignored) {
                }
            }
        }
    }
}
