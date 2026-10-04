package com.starry.magic.cocos;

import android.app.Activity;
import android.util.Log;

import java.lang.reflect.Method;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * 仙逆：本尊（com.qqsy.xnbz）Cocos Creator 3.8 JSB 注入诊断壳。
 *
 * 目的：定位 evalString 注入失败点（CocosJsBridge.isCocosEvalSafe() 静默 return /
 * 类加载器 / runOnGameThread 队列未 flush）。只做投递与日志，不保存业务状态；
 * 日志 tag = StarryTrace（stages.md 白名单）。
 */
public final class XnbzHooks {
    private static final String TAG = "StarryTrace";
    private static final AtomicInteger sSeq = new AtomicInteger();
    private static volatile String sLastJsMsg = "";

    private XnbzHooks() {}

    /** JS → Java 回传通道（游戏内 jsb.reflection.callStaticMethod 调用）。 */
    public static void logStatus(String msg) {
        if (msg != null) sLastJsMsg = msg;
        Log.i(TAG, "xnbz-js " + msg);
    }

    /**
     * native 侧查询：JS 补丁是否已装上（Tick 用它把「补丁确认」打一次日志，
     * 不依赖 logcat 里人工比对，避免误报「未见 XnbzPatch 日志」）。
     */
    public static boolean isPatched() {
        return sLastJsMsg != null && sLastJsMsg.contains("patch v5 installed");
    }

    /** 最近一条 JS 回传（诊断用）。 */
    public static String lastMsg() {
        return sLastJsMsg;
    }

    /** native 每次催抓都走这里：先取 Activity/安全判定，再投递到游戏线程执行 evalString。 */
    public static void pump(String js) {
        if (js == null || js.isEmpty()) return;
        final int seq = sSeq.incrementAndGet();
        final boolean verbose = (seq <= 5) || (seq % 50 == 0);
        try {
            Activity act = appActivity();
            String cn = (act == null) ? "null" : act.getClass().getName();
            boolean safe = false;
            String safeErr = "";
            try {
                safe = CocosJsBridge.isCocosEvalSafe();
            } catch (Throwable t) {
                safeErr = String.valueOf(t);
            }
            if (verbose) {
                Log.i(TAG, "pump#" + seq + " act=" + cn + " safe=" + safe
                        + (safeErr.isEmpty() ? "" : (" safeErr=" + safeErr)));
            }

            final Method eval = evalStringMethod();
            if (eval == null) {
                Log.w(TAG, "pump#" + seq + " evalString 方法未找到（类加载器？）");
                return;
            }

            Runnable task = new Runnable() {
                @Override
                public void run() {
                    try {
                        Object r = eval.invoke(null, js);
                        if (verbose || seq == 1) Log.i(TAG, "pump#" + seq + " evalString 执行完成 ret=" + r);
                    } catch (Throwable t) {
                        Log.w(TAG, "pump#" + seq + " evalString 抛异常: " + t);
                    }
                }
            };

            // 1) 前台队列（引擎前台每帧 flush）
            if (postTo("com.cocos.lib.CocosHelper", "runOnGameThreadAtForeground", task)) {
                if (verbose) Log.i(TAG, "pump#" + seq + " 已投递 foreground 队列");
                return;
            }
            // 2) 常规游戏线程队列
            if (postTo("com.cocos.lib.CocosHelper", "runOnGameThread", task)) {
                if (verbose) Log.i(TAG, "pump#" + seq + " 已投递 game 队列");
                return;
            }
            // 3) 兜底：CocosJsBridge（内部还有直接执行兜底）
            if (verbose) Log.w(TAG, "pump#" + seq + " 队列不可用，回落 CocosJsBridge.evalOnGLThread");
            CocosJsBridge.evalOnGLThread(js);
        } catch (Throwable t) {
            Log.w(TAG, "pump#" + seq + " 失败: " + t);
        }
    }

    private static boolean postTo(String cls, String method, Runnable task) {
        try {
            Class<?> c = Class.forName(cls);
            Method m = c.getMethod(method, Runnable.class);
            m.invoke(null, task);
            return true;
        } catch (Throwable t) {
            return false;
        }
    }

    private static Method evalStringMethod() {
        String[] names = {
                "com.cocos.lib.CocosJavascriptJavaBridge",
                "org.cocos2dx.lib.Cocos2dxJavascriptJavaBridge",
        };
        for (String n : names) {
            try {
                return Class.forName(n).getMethod("evalString", String.class);
            } catch (Throwable ignored) {
            }
        }
        return null;
    }

    /** 优先用游戏自带公开 API 取当前 Activity（绕开 hidden-api 对 mActivities 的限制）。 */
    private static Activity appActivity() {
        try {
            Class<?> go = Class.forName("com.cocos.lib.GlobalObject");
            Object a = go.getMethod("getActivity").invoke(null);
            if (a instanceof Activity) return (Activity) a;
        } catch (Throwable ignored) {
        }
        return null;
    }
}
