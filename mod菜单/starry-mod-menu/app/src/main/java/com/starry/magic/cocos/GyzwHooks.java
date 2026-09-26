package com.starry.magic.cocos;

import android.util.Log;

import java.util.concurrent.atomic.AtomicIntegerArray;

/**
 * 光阴之外（com.shbq.gyzw.ppyx）— 供 JS jsb.reflection / smali 查询菜单开关。
 *
 * getRet 索引对齐 GyzwEntry.cpp 的 featureList 下标：
 *   1=百倍伤害  2=超级无敌  3=战斗加速(滑块 1..10)  5=免广告
 * setNoAd/shouldSkipAd 供 TapAdModImpl.getRewardVideo 的 smali 门控读取。
 */
public final class GyzwHooks {

    private static final String TAG = "Gyzw";
    private static final int N = 16;

    private static final AtomicIntegerArray RET = new AtomicIntegerArray(N);
    private static final AtomicIntegerArray VAL = new AtomicIntegerArray(N);
    private static volatile boolean noAd = false;

    private GyzwHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < N) RET.set(index, on ? 1 : 0);
    }

    public static boolean getRet(int index) {
        return index >= 0 && index < N && RET.get(index) != 0;
    }

    public static void setVal(int index, int v) {
        if (index >= 0 && index < N) VAL.set(index, v);
    }

    public static int getVal(int index) {
        if (index < 0 || index >= N) return 1;
        int v = VAL.get(index);
        return v > 0 ? v : 1;
    }

    public static void setNoAd(boolean v) {
        noAd = v;
    }

    public static boolean shouldSkipAd() {
        return noAd;
    }

    public static void logStatus(String msg) {
        Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < N; i++) {
            RET.set(i, 0);
            VAL.set(i, 0);
        }
        noAd = false;
    }
}
