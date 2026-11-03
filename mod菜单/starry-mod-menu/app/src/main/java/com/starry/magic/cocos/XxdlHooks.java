package com.starry.magic.cocos;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;

/**
 * 修仙大陆模拟器（com.sky.xxdl.dlt 1.0.5）—— JS 功能层通道。
 *
 * <p>四项功能全在游戏 JS 层实现（assets/main.js 追加的 window.__sst），本类只把
 * 菜单动作翻译成一次 evalString。语义统一为「点一下加一次」（开关型控件复用为动作按钮），
 * 不做周期性重放：重复推送会把加值变成按帧累加。
 */
public final class XxdlHooks {
    private static final String TAG = "StarryXxdl";

    // 与 XxdlEntry.cpp featureList 下标一致
    private static final int F_RES      = 1;
    private static final int F_LINGSHI  = 2;
    private static final int F_XIANJING = 3;
    private static final int F_TILI     = 4;
    private static final int F_NOAD     = 5;

    private static final Handler UI = new Handler(Looper.getMainLooper());
    /** 每次点击一个唯一序号：JS 侧按序号去重，补推不会重复加值 */
    private static final java.util.concurrent.atomic.AtomicLong SEQ =
            new java.util.concurrent.atomic.AtomicLong(0);

    private XxdlHooks() {}

    public static void setFeature(int index, boolean on) {
        // 免广告是状态型：开/关都要下发；其余四项是按钮型，只处理开
        if (index != F_NOAD && !on) return;
        String key;
        switch (index) {
            case F_RES:      key = "res";      break;
            case F_LINGSHI:  key = "lingshi";  break;
            case F_XIANJING: key = "xianjing"; break;
            case F_TILI:     key = "tili";     break;
            case F_NOAD:     key = "noad";     break;
            default: return;
        }
        final long seq = SEQ.incrementAndGet();
        final String js = "window.__sst&&window.__sst('" + key + "'," + (on ? 1 : 0) + "," + seq + ")";
        // 首次 + 两次补推：游戏 JS 可能在菜单点下时尚未加载完（早推丢包）
        post(js, 0);
        post(js, 1200);
        post(js, 3000);
        Log.i(TAG, "feature " + key + " -> " + js);
    }

    public static void syncFeature(int index, boolean on) {
        setFeature(index, on);
    }

    public static void setValue(int index, int value) {
        // 本作无滑块型功能
    }

    public static void syncValue(int index, int value) {
        setValue(index, value);
    }

    private static void post(final String js, long delayMs) {
        Runnable r = new Runnable() {
            @Override public void run() {
                if (!CocosJsBridge.isCocosEvalSafe()) return;
                CocosJsBridge.evalOnGLThread(js);
            }
        };
        if (delayMs <= 0) UI.post(r);
        else UI.postDelayed(r, delayMs);
    }
}
