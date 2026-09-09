package com.starry.magic.cocos;

import android.util.Log;

import java.lang.reflect.Field;
import java.lang.reflect.Method;

/**
 * 雷霆战机：JS get_ret + TopOn 激励视频旁路。
 * 1百倍伤害 2无敌 3免广告
 */
public final class LtzjHooks {
    private static final String TAG = "Ltzj";
    private static final boolean[] RET = new boolean[8];
    private static volatile boolean noAd = false;

    private LtzjHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < RET.length) {
            RET[index] = on;
        }
        if (index == 3) {
            noAd = on;
        }
    }

    public static boolean getRet(int index) {
        if (index >= 0 && index < RET.length) {
            return RET[index];
        }
        return false;
    }

    public static void setNoAd(boolean v) {
        noAd = v;
        RET[3] = v;
    }

    public static boolean shouldSkipAd() {
        return noAd || RET[3];
    }

    /**
     * TopOn ATRewardedVideoJSBridge.show 旁路：直接发 Reward + Close 回调。
     */
    public static void fakeRewardShow(String placementId) {
        if (placementId == null) placementId = "";
        try {
            Class<?> bridge = Class.forName("com.anythink.cocosjs.ATRewardedVideoJSBridge");
            Method getHelper = bridge.getDeclaredMethod("getHelper", String.class);
            getHelper.setAccessible(true);
            Object helper = getHelper.invoke(null, placementId);
            if (helper == null) {
                Log.w(TAG, "fakeReward: helper null pid=" + placementId);
                return;
            }
            Method getCb = helper.getClass().getMethod("getCallbackName", String.class);
            String reward = String.valueOf(getCb.invoke(helper, "RewardedVideoReward"));
            String close = String.valueOf(getCb.invoke(helper, "RewardedVideoClose"));
            String adInfo = "{}";
            try {
                Field f = helper.getClass().getDeclaredField("mPlacementId");
                f.setAccessible(true);
                Object cur = f.get(helper);
                if (cur instanceof String && !((String) cur).isEmpty()) {
                    placementId = (String) cur;
                } else {
                    f.set(helper, placementId);
                }
            } catch (Throwable ignored) {
            }
            evalJs(reward + "('" + escape(placementId) + "','" + adInfo + "');");
            evalJs(close + "('" + escape(placementId) + "','" + adInfo + "');");
            Log.i(TAG, "fakeReward ok pid=" + placementId);
        } catch (Throwable t) {
            Log.e(TAG, "fakeReward failed", t);
        }
    }

    private static String escape(String s) {
        return s == null ? "" : s.replace("\\", "\\\\").replace("'", "\\'");
    }

    private static void evalJs(String js) {
        try {
            Class<?> cls = Class.forName("com.cocos.lib.CocosJavascriptJavaBridge");
            Method m = cls.getMethod("evalString", String.class);
            m.invoke(null, js);
        } catch (Throwable t) {
            try {
                CocosJsBridge.evalOnGLThread(js);
            } catch (Throwable t2) {
                Log.w(TAG, "evalJs failed: " + t2.getMessage());
            }
        }
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
        noAd = false;
    }
}
