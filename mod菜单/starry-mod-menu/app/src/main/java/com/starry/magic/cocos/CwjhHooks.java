package com.starry.magic.cocos;

import android.util.Log;

/**
 * 超维进化：供 JS jsb.reflection / smali 查询菜单开关。
 * getRet 索引（0 基，与菜单下标一致）：
 *   0=超级回血 1=超级回蓝 2=高暴击率 3=高暴击伤害
 *   4=高防御 5=高抗性 6=高闪避 7=免广告 8=秒杀（封号） 9=无敌（封号）
 */
public final class CwjhHooks {
    private static final String TAG = "Cwjh";
    private static final boolean[] RET = new boolean[10];
    private static volatile boolean noAd = false;

    private CwjhHooks() {}

    public static void setRet(int index, boolean on) {
        if (index >= 0 && index < RET.length) {
            RET[index] = on;
        }
    }

    /** jsb / AppActivity.get_ret → 菜单布尔 */
    public static boolean getRet(int index) {
        if (index >= 0 && index < RET.length) {
            return RET[index];
        }
        return false;
    }

    public static void setNoAd(boolean v) {
        noAd = v;
    }

    /** 预留：smali 侧广告门控（本游戏免广告走 JS adsPlay） */
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
    }
}
