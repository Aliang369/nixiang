package com.starry.magic.cocos;

import android.util.Log;

/**
 * 我在神界刷装备：JS 菜单桥。
 *
 * RET[1] 仅用于记录伤害倍率是否大于 1；实际倍率由 getAtkMul() 读取。
 * RET[2] 无敌 / RET[3] 免广告 / RET[4] 游戏加速。
 */
public final class SjzzHooks {
    private static final String TAG = "Sjzz";
    private static final boolean[] RET = new boolean[8];
    private static volatile int ATK_MUL = 1;

    private SjzzHooks() {}

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

    public static void setAtkMul(int value) {
        if (value < 1) value = 1;
        if (value > 100) value = 100;
        ATK_MUL = value;
    }

    public static int getAtkMul() {
        return ATK_MUL;
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
        ATK_MUL = 1;
        Log.i(TAG, "clearAll");
    }
}
