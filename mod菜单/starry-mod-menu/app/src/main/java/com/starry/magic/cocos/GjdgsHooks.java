package com.starry.magic.cocos;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;

/**
 * 挂机打怪兽（org.cocos2d.guajidaguaishou.tap 1.1.65）—— JS 功能层开关通道。
 *
 * <p>资源类功能从「输入框累加」改为「开关锁定满值」：输入框在 Cocos 2.x 这条链路上
 * 取值时机不稳定（点确认才回调、且各菜单版本行为不一致），开关语义简单且可复检。
 *
 * <p>功能实现全在游戏 JS 层（assets/main.js 追加的 window.__sst），本类只把菜单动作
 * 翻译成 evalString。
 */
public final class GjdgsHooks {
    private static final String TAG = "StarryGjdgs";

    // 与 GjdgsEntry.cpp featureList 下标一致
    private static final int F_GOLD = 1;
    private static final int F_GEM = 2;
    private static final int F_SHARD = 3;
    private static final int F_SPD = 5;
    private static final int F_GOD = 6;
    private static final int F_DMG = 7;
    private static final int F_NOAD = 8;

    private static volatile boolean sGod = false;
    private static volatile boolean sNoAd = false;
    private static volatile int sSpd = 1;
    private static volatile int sDmg = 1;

    private static final Handler UI = new Handler(Looper.getMainLooper());

    private GjdgsHooks() {}

    /** 开关型：幂等，可重放（JS 未加载完时补推） */
    public static void setFeature(int index, boolean on) {
        String key;
        switch (index) {
            case F_GOLD:  key = "lock_gold";  break;
            case F_GEM:   key = "lock_gem";   break;
            case F_SHARD: key = "lock_shard"; break;
            case F_GOD:   key = "god";  sGod = on;  break;
            case F_NOAD:  key = "noad"; sNoAd = on; break;
            default: return;
        }
        pushIdempotent(key, on ? 1 : 0);
    }

    public static void syncFeature(int index, boolean on) {
        setFeature(index, on);
    }

    /** 滑块型（只有加速与伤害倍率） */
    public static void setValue(int index, int value) {
        switch (index) {
            case F_SPD:
                sSpd = clamp(value, 1, 10);
                pushIdempotent("spd", sSpd);
                break;
            case F_DMG:
                sDmg = clamp(value, 1, 100);
                pushIdempotent("dmg", sDmg);
                break;
            default:
                break;
        }
    }

    public static void syncValue(int index, int value) {
        setValue(index, value);
    }

    /** 供改包后的广告适配器查询：免广告开关是否开启 */
    public static boolean isNoAd() {
        return sNoAd;
    }

    public static String pullState() {
        return "{\"god\":" + (sGod ? 1 : 0) + ",\"noad\":" + (sNoAd ? 1 : 0)
                + ",\"spd\":" + sSpd + ",\"dmg\":" + sDmg + "}";
    }

    private static void pushIdempotent(final String key, final int value) {
        final String js = "window.__sst&&window.__sst('" + key + "'," + value + ")";
        CocosJsBridge.evalOnGLThread(js);
        for (int i = 1; i <= 3; i++) {
            UI.postDelayed(new Runnable() {
                @Override public void run() {
                    if (!CocosJsBridge.isCocosEvalSafe()) return;
                    CocosJsBridge.evalOnGLThread(js);
                }
            }, 800L * i);
        }
    }

    private static int clamp(int v, int lo, int hi) {
        if (v < lo) return lo;
        if (v > hi) return hi;
        return v;
    }

    public static void logStatus(String msg) {
        Log.i(TAG, msg + " god=" + sGod + " noad=" + sNoAd + " spd=" + sSpd + " dmg=" + sDmg);
    }
}
