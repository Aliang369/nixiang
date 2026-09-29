package com.starry.magic.cocos;

import android.util.Log;

/**
 * 造梦西游：JS jsb.reflection（经 AppActivity.get_ret / get_val）读取的菜单状态。
 *
 * RET[1] = 伤害倍率>1（仅状态标记，实际倍率走 getVal）
 * RET[2] = 无敌 / RET[3] = 无限技能；VAL[1] = 伤害倍率 1..100
 */
public final class ZmxyHooks {
    private static final String TAG = "Zmxy";
    private static final boolean[] RET = new boolean[8];
    private static final int[] VAL = new int[8];

    static {
        VAL[1] = 1;  // 1 = 原版伤害（滑块关）
    }

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

    public static void setVal(int index, int value) {
        if (index < 0 || index >= VAL.length) {
            return;
        }
        if (value < 1) value = 1;
        if (value > 100) value = 100;
        VAL[index] = value;
    }

    public static int getVal(int index) {
        if (index >= 0 && index < VAL.length) {
            return VAL[index];
        }
        return 0;
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
            VAL[i] = 0;
        }
        VAL[1] = 1;
    }
}
