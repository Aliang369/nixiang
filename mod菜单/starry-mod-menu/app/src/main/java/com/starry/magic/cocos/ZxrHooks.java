package com.starry.magic.cocos;

import android.util.Log;

/**
 * 镇邪人：供 JS jsb.reflection / smali 查询菜单开关。
 * getRet 索引对齐斌哥 JMYBTActivity.get_ret：1=秒杀 2=无敌 3=攻速 4=全屏秒杀
 */
public final class ZxrHooks {
    private static final String TAG = "Zxr";
    private static final boolean[] RET = new boolean[8];
    private static volatile boolean noAd = false;

    private ZxrHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < RET.length) {
            RET[index] = on;
        }
    }

    /** jsb / JMYBTActivity.get_ret → 菜单布尔 */
    public static boolean getRet(int index) {
        if (index >= 0 && index < RET.length) {
            return RET[index];
        }
        return false;
    }

    public static void setNoAd(boolean v) {
        noAd = v;
    }

    /** smali SdkManager.SdkShowAdReward 门控 */
    public static boolean shouldSkipAd() {
        return noAd;
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
