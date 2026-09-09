package com.starry.magic.cocos;

import android.util.Log;

/**
 * 生存33天：供 JS jsb.reflection / smali 查询菜单开关。
 * getRet 对齐斌哥 JMYBTActivity.get_ret：1=百倍 2=无敌 3=全屏秒杀 4=免广告(Java) 5=攻速
 */
public final class Sc33Hooks {
    private static final String TAG = "Sc33";
    private static final boolean[] RET = new boolean[8];
    private static volatile boolean noAd = false;

    private Sc33Hooks() {}

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

    public static void setNoAd(boolean v) {
        noAd = v;
    }

    public static boolean shouldSkipAd() {
        return noAd;
    }

    public static void logStatus(String msg) {
        if (msg != null) Log.i(TAG, msg);
    }

    public static void clearAll() {
        for (int i = 0; i < RET.length; i++) {
            RET[i] = false;
        }
        noAd = false;
        atkMul = 1;
    }

    /** 攻速滑条：1=关，2..10 = 倍率 */
    private static volatile int atkMul = 1;

    public static void setAtkMul(int mul) {
        if (mul <= 1) {
            atkMul = 1;
        } else if (mul > 10) {
            atkMul = 10;
        } else {
            atkMul = mul;
        }
    }

    public static int getAtkMul() {
        return atkMul;
    }
}
