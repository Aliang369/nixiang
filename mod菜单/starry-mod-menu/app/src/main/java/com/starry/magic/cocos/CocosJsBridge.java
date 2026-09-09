package com.starry.magic.cocos;

import android.app.Activity;
import android.util.Log;

import java.lang.reflect.Field;
import java.lang.reflect.Method;

/**
 * Cocos Creator JSB：在游戏线程执行 evalString。
 * 反射调用，兼容：
 * - 旧栈 org.cocos2dx（动物餐厅 / libcocos2djs）
 * - 新栈 com.cocos.lib（堡垒之战 Creator 3.8 / libcocos）
 */
public final class CocosJsBridge {
    private static final String TAG = "CocosJsBridge";

    private CocosJsBridge() {}

  /**
     * Cocos GL 是否可安全 eval。37 支付页 {@code com.host.PluginActivity} 顶栈时
     * evalString 会 SIGSEGV（见 tombstone / Sc33 支付重启）。
     */
    public static boolean isCocosEvalSafe() {
        Activity cur = getCurrentActivity();
        if (cur == null) return false;
        String cn = cur.getClass().getName();
        if (cn.contains("PluginActivity") || cn.startsWith("com.host.")) {
            return false;
        }
        if (cn.contains("pay") || cn.contains("Pay") || cn.contains("WebView")) {
            return false;
        }
        try {
            Class<?> cocosAct = Class.forName("org.cocos2dx.lib.Cocos2dxActivity");
            if (cocosAct.isInstance(cur)) return true;
        } catch (Throwable ignored) {
        }
        // Creator 3.x（com.cocos.lib.CocosActivity，如遗弃之地 / 堡垒之战）
        try {
            Class<?> cocos3 = Class.forName("com.cocos.lib.CocosActivity");
            return cocos3.isInstance(cur);
        } catch (Throwable ignored) {
            return false;
        }
    }

    /** 在 Cocos 游戏线程执行一段 JS；失败仅打日志 */
    public static void evalOnGLThread(final String js) {
        if (js == null || js.isEmpty()) return;
        if (!isCocosEvalSafe()) return;
        try {
            final Method evalString = findEvalString();
            if (evalString == null) {
                Log.w(TAG, "evalString bridge not found");
                return;
            }
            Runnable task = () -> {
                try {
                    evalString.invoke(null, js);
                } catch (Throwable t) {
                    Log.w(TAG, "evalString failed: " + t.getMessage());
                }
            };
            if (runOnGameThread(task)) return;
            // 兜底：直接 eval（部分时机可能已在游戏线程）
            task.run();
        } catch (Throwable t) {
            Log.w(TAG, "evalOnGLThread failed: " + t.getMessage());
        }
    }

    private static Method findEvalString() {
        String[] classes = {
                "com.cocos.lib.CocosJavascriptJavaBridge",
                "org.cocos2dx.lib.Cocos2dxJavascriptJavaBridge",
        };
        for (String name : classes) {
            try {
                return Class.forName(name).getMethod("evalString", String.class);
            } catch (Throwable ignored) {
            }
        }
        return null;
    }

    /** @return true 若已成功投递到游戏线程 */
    private static boolean runOnGameThread(Runnable task) {
        // Creator 3.x
        try {
            Class<?> helper = Class.forName("com.cocos.lib.CocosHelper");
            Method m = helper.getMethod("runOnGameThread", Runnable.class);
            m.invoke(null, task);
            return true;
        } catch (Throwable ignored) {
        }
        // 旧 cocos2d-jsb：Activity.runOnGLThread
        try {
            Object activity = findLegacyAppActivity();
            if (activity != null) {
                Method runOnGL = activity.getClass().getMethod("runOnGLThread", Runnable.class);
                runOnGL.invoke(activity, task);
                return true;
            }
        } catch (Throwable ignored) {
        }
        return false;
    }

    private static Object findLegacyAppActivity() throws Exception {
        // 动物餐厅等：org.cocos2dx.javascript.AppActivity.app
        try {
            Class<?> actCls = Class.forName("org.cocos2dx.javascript.AppActivity");
            Field appField = actCls.getField("app");
            Object app = appField.get(null);
            if (app != null) return app;
            Activity a = getCurrentActivity();
            if (a != null && actCls.isInstance(a)) return a;
        } catch (Throwable ignored) {
        }
        // 镇邪人等：JMYBTActivity extends Cocos2dxActivity（无 AppActivity）
        Activity cur = getCurrentActivity();
        if (cur != null) {
            try {
                Class<?> cocosAct = Class.forName("org.cocos2dx.lib.Cocos2dxActivity");
                if (cocosAct.isInstance(cur)) return cur;
            } catch (Throwable ignored) {
            }
        }
        return null;
    }

    private static Activity getCurrentActivity() {
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object thread = at.getMethod("currentActivityThread").invoke(null);
            Field f = at.getDeclaredField("mActivities");
            f.setAccessible(true);
            Object mActivities = f.get(thread);
            if (mActivities instanceof java.util.Map) {
                for (Object rh : ((java.util.Map<?, ?>) mActivities).values()) {
                    Class<?> rhCls = rh.getClass();
                    Field paused = rhCls.getDeclaredField("paused");
                    paused.setAccessible(true);
                    if (paused.getBoolean(rh)) continue;
                    Field activity = rhCls.getDeclaredField("activity");
                    activity.setAccessible(true);
                    Object act = activity.get(rh);
                    if (act instanceof Activity) return (Activity) act;
                }
            }
        } catch (Throwable ignored) {
        }
        return null;
    }
}
