package com.starry.magic.games.dwdlm;

import android.app.Application;
import android.util.Log;

import com.starry.magic.core.GameHook;

/**
 * 动物大联盟 —— APKSecure 壳的 {@code PluginHelper.app} 回填
 * （原写死在共用 {@code StarryInitProvider} 里，现在只在本游戏装载）。
 *
 * <p>为什么需要：1.0.25 壳下真实 Application 未被创建 ⇒ {@code PluginHelper.app} 为 null ⇒
 * nativeInit 里 {@code CCAppInfo.getBingoAppId → PluginHelper.getAppInfo()} 抛 NPE ⇒
 * 进程 8 秒后静默 exit(0)。
 *
 * <p>为什么放后台线程重试：壳在 {@code [secure] phase app-ready} 之后才把真实 dex 换进
 * PathClassLoader，所以 ① 必须用 Application 当前的 ClassLoader 取类（注入 dex 的 CL 看不到壳
 * 动态加载的真实 dex）；② 时间窗要覆盖首启解密（实测首启到 app-ready 可 > 20 s）。
 */
public final class GameHookImpl implements GameHook {

    private static final String TAG = "DwdlmHook";
    /** 本游戏的宿主包名（壳固定这个值，改名渠道需同步改）。 */
    private static final String HOST_PKG = "com.fightingstars.games.animal";

    @Override
    public void onApplicationCreate(Application app) {
        patchPluginHelperApp(app);
    }

    @Override
    public String targetPackage() {
        return HOST_PKG;
    }

    private static void patchPluginHelperApp(final Application app) {
        if (app == null) return;
        // 双保险：万一其它游戏也装到了本实现（多 game_id 混装 / 助手形态），直接退出
        try {
            if (!HOST_PKG.equals(app.getPackageName())) return;
        } catch (Throwable ignore) {
            return;
        }
        Thread t = new Thread(new Runnable() {
            @Override
            public void run() {
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
