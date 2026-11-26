package com.starry.magic.cocos;

import android.util.Log;

import com.starry.magic.menu.Preferences;

/**
 * 魔法奇录（com.shw.mfqlm1）：供 JS 侧 jsb.reflection 查询菜单开关。
 *
 * <p>索引对齐 {@code Games/MfqlEntry.cpp} 的 featureList：1 = 免广告。
 * 判据三层（与框架口径一致，缺一不放行）：后台允许功能 &gt; 服务器 botostrap 单项门控 &gt; 本地开关。
 * 不读票、不验签的行为是错的：那等于「装上就能用」，绕过了 native 的票门禁。
 *
 * <p><b>R8 必须 keep 全名</b>（proguard-rules.pro / inject-dex-rules.pro 各一行）：
 * JS 侧按 {@code "com.starry.magic.cocos.MfqlHooks"} 全名反射调用，被改名即静默读 false。
 */
public final class MfqlHooks {
    private static final String TAG = "Mfql";

    /** featureList 下标（与 MfqlEntry.cpp 严格一致） */
    public static final int FEAT_FREE_AD = 1;

    private static volatile boolean freeAdOverride = false;
    private static volatile boolean hasOverride = false;

    private MfqlHooks() {}

    /** JS 侧主入口：免广告开关是否生效 */
    public static boolean getFreeAd() {
        return getRet(FEAT_FREE_AD);
    }

    /** 与 ZxrHooks 同名的通用入口：JS 侧 __starryMfqlOn(i) 直接用它 */
    public static boolean getRet(int index) {
        if (hasOverride) {
            return freeAdOverride;
        }
        return featEnabled(index);
    }

    /** native / 菜单侧推状态（可选；未推送时以 prefs 为准） */
    public static void setRet(int index, boolean on) {
        if (index == FEAT_FREE_AD) {
            freeAdOverride = on;
            hasOverride = true;
        }
    }

    /** 菜单被后台判「未授权/已停用」时清掉本地覆盖，回到 prefs 判定 */
    public static void clearAll() {
        freeAdOverride = false;
        hasOverride = false;
    }

    public static void logStatus(String msg) {
        if (msg != null) {
            Log.i(TAG, msg);
        }
    }

    private static boolean featEnabled(int featNum) {
        if (featNum <= 0) return false;
        try {
            if (!Preferences.isFeaturesAllowed()) return false;
            if (!Preferences.isServerFeatureEnabled(featNum)) return false;
            return Preferences.loadPrefBool(featNum, false);
        } catch (Throwable t) {
            Log.w(TAG, "feat#" + featNum + " read failed: " + t);
            return false;
        }
    }
}
