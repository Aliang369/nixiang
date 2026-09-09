package com.starry.magic.cocos;

import android.util.Log;

/**
 * 山海幻境：JS jsb.reflection / AppActivity.get_ret
 * get_ret：2 无敌  3 百倍伤害（菜单 feat 1/2 映射到此）
 */
public final class ShhjHooks {
    private static final String TAG = "Shhj";
    private static final boolean[] RET = new boolean[8];

    private ShhjHooks() {}

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
        if (msg != null) Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
    }
}
