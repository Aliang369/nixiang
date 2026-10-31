package com.starry.magic.cocos;

import android.util.Log;

/**
 * 迷雾大陆：JS jsb.reflection
 * get_ret：2无敌 4无限技能 5自动挂机 6全图拾取
 * get_val：1伤害倍率（1=原伤害）3移动速度倍率（1=关，&gt;1 生效）
 */
public final class MwdlHooks {
    private static final String TAG = "Mwdl";
    private static final boolean[] RET = new boolean[16];
    private static final float[] VAL = new float[16];

    static {
        for (int i = 0; i < VAL.length; i++) {
            VAL[i] = 1f;
        }
    }

    private MwdlHooks() {}

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

    public static void setVal(int index, float v) {
        if (index >= 0 && index < VAL.length) {
            VAL[index] = v;
        }
    }

    public static float getVal(int index) {
        if (index >= 0 && index < VAL.length) {
            return VAL[index];
        }
        return 1f;
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
        for (int i = 0; i < VAL.length; i++) {
            VAL[i] = 1f;
        }
    }
}
