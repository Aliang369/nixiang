package com.starry.magic.cocos;

import android.util.Log;

/**
 * 无尽探险队：JS get_ret 桥。
 * 1超级无敌 2超级移速 3免广告（对齐斌哥 get_ret 1..2 + 星光扩展）
 */
public final class WjtxdHooks {
    private static final String TAG = "Wjtxd";
    private static final boolean[] RET = new boolean[8];

    private WjtxdHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < RET.length) {
            RET[index] = on;
        }
    }

    public static boolean getRet(int index) {
        if (index >= 0 && index < RET.length) {
            return RET[index];
        }
        return false;
    }

    public static void logStatus(String msg) {
        if (msg != null) android.util.Log.i(TAG, msg);
    }

    /**
     * 免广告：直接回调 JS onAdResult=success（等价看完整激励视频拿奖励）。
     * JS 链：RewardVideoAd → addCallBackAd 注册 onNative → sendToNative("adVideo")
     * → Play800Bridge.adVideo → 真广告 onRewarded → JsbBridge.sendToScript("onAdResult","success")。
     * 星光 fake：不触发真广告，直接回发 success → 原 JS 发奖逻辑不变。
     */
    public static void fakeAd() {
        try {
            Class<?> jb = Class.forName("com.cocos.lib.JsbBridge");
            java.lang.reflect.Method m = jb.getMethod("sendToScript", String.class, String.class);
            m.invoke(null, "onAdResult", "success");
            Log.i(TAG, "fakeAd -> onAdResult success");
        } catch (Throwable t) {
            Log.e(TAG, "fakeAd failed", t);
        }
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
    }
}