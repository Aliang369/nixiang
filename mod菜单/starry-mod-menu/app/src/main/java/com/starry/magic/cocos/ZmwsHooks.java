package com.starry.magic.cocos;

import android.util.Log;

/**
 * 造梦无双：JS jsb.reflection / AppActivity.get_ret
 * 1无限技能 2法宝技能 3超级秒杀 4超级无敌
 */
public final class ZmwsHooks {
    private static final String TAG = "Zmws";
    private static final boolean[] RET = new boolean[8];

    private ZmwsHooks() {}

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
