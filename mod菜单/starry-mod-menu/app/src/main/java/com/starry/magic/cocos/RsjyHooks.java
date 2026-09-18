package com.starry.magic.cocos;

import android.util.Log;

/** 燃烧纪元：JS 启动补丁读取的 native 功能开关/滑块桥。 */
public final class RsjyHooks {
    private static final String TAG = "Rsjy";
    private static final boolean[] RET = new boolean[16];
    private static final int[] VAL = new int[16];

    private RsjyHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < RET.length) RET[index] = on;
    }

    public static boolean getRet(int index) {
        return index >= 0 && index < RET.length && RET[index];
    }

    public static void setVal(int index, int value) {
        if (index >= 0 && index < VAL.length) VAL[index] = value;
    }

    public static int getVal(int index) {
        return index >= 0 && index < VAL.length ? VAL[index] : 0;
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) RET[i] = false;
        for (int i = 0; i < VAL.length; i++) VAL[i] = 0;
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }
}
