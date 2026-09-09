package com.starry.magic.cocos;

import android.util.Log;

/**
 * 造梦西游：供 JS jsb.reflection / AppActivity.get_ret 查询菜单开关。
 * 1百倍伤害 2无敌 3无限技能
 */
public final class ZmxyHooks {
    private static final String TAG = "Zmxy";
    private static final boolean[] RET = new boolean[8];

    private ZmxyHooks() {}

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
