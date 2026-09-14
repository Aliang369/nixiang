package com.starry.magic.cocos;

import android.util.Log;

/** 正中靶心2：JS 启动补丁读取的 native 功能开关桥。 */
public final class Zzbx2Hooks {
    private static final String TAG = "Zzbx2";
    private static final boolean[] RET = new boolean[8];
    private static final int[] VAL = new int[8];

    private Zzbx2Hooks() {}

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
