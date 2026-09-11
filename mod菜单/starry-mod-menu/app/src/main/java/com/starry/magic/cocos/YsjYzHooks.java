package com.starry.magic.cocos;

import android.util.Log;

/**
 * 异世界勇者（com.mudamuda.idlewow）：JS jsb.reflection / AppActivity.get_ret | get_val 桥。
 *
 * 菜单下标（与 Games/YsjYzEntry.cpp 的 featureList 一一对应）：
 *  1  免广告            2  广告CD清零        3  无限技能·零CD    4  技能不耗蓝
 *  5  自动战斗·自动技能  6  无敌              10 伤害上报还原     15 快速挂机免费
 *  7  伤害倍率(值)       8  减伤百分比(值)
 *  12 移速倍率(值)      13 攻速倍率(值)      14 游戏倍速(值)
 *
 * JS 侧每次轮询读回本表缓存；native 每 Tick 由 YsjYzMod 同步一次。
 */
public final class YsjYzHooks {
    private static final String TAG = "YsjYz";
    private static final boolean[] RET = new boolean[48];
    private static final int[] VAL = new int[48];

    private YsjYzHooks() {}

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
        if (index >= 0 && index < VAL.length) {
            VAL[index] = value;
        }
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
    }
}
