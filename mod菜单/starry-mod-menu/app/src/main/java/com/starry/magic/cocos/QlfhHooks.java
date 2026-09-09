package com.starry.magic.cocos;

import android.util.Log;

/**
 * 青鸾繁华录：供 JS jsb.reflection / AppActivity.get_ret 查询菜单开关。
 * 1 免广告直领  2 真广告改本地  3 本地广告秒过；speed = 游戏加速倍率（1=关）
 */
public final class QlfhHooks {
    private static final String TAG = "Qlfh";
    private static final boolean[] RET = new boolean[8];
    private static volatile int SPEED = 1;

    private QlfhHooks() {}

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

    public static void setSpeed(int speed) {
        SPEED = Math.max(1, Math.min(10, speed));
    }

    public static int getSpeed() {
        return SPEED;
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
        SPEED = 1;
    }
}
