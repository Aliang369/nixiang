package com.starry.magic.cocos;

import android.util.Log;

/**
 * 勇士之心：JS 自愈注入体读取的星光菜单开关桥。
 *
 * 容量必须 >= 菜单项数：2026-10-07 新增「解锁金卡 / 自动重复挑战 / 自动出售 /
 * 自动分解 / 自动穿戴」后，菜单位置最大到 13（见 Games/YsxzEntry.cpp::kYsxz_Features）。
 * 数组太小不会崩（越界返回 false/0），但新开关会【静默失效】—— 读到的永远是 false。
 */
public final class YsxzHooks {
    private static final String TAG = "Ysxz";
    /**
     * 构建标记：出包脚本用它挡住【容量还是 8 的旧 dex】。
     * 旧 dex 不会报错，只会让菜单 10..13 号开关永远读到 false（静默失效）。
     * 改容量时务必同步改这个串，否则门禁失去意义。
     */
    public static final String CAP_MARK = "ysxz_hooks_cap16_20261007";
    private static final int CAP = 16;
    private static final boolean[] RET = new boolean[CAP];
    private static final int[] VAL = new int[CAP];

    private YsxzHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < RET.length) {
            RET[index] = on;
        }
    }

    public static boolean getRet(int index) {
        return index >= 0 && index < RET.length && RET[index];
    }

    public static void setVal(int index, int value) {
        if (index >= 0 && index < VAL.length) {
            VAL[index] = value;
        }
    }

    public static int getVal(int index) {
        return index >= 0 && index < VAL.length ? VAL[index] : 0;
    }

    public static void logStatus(String msg) {
        if (msg != null) {
            Log.i(TAG, msg);
        }
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
            VAL[i] = 0;
        }
    }
}
