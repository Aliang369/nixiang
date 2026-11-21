package com.starry.magic.games.dwdlm;

import android.util.Log;

/**
 * 动物大联盟：JS 补丁读取的 native 功能开关桥。
 *
 * <p><b>2026-11-11 隔离搬迁</b>：原 {@code com.starry.magic.cocos.DwdlmHooks}。类名与 TAG
 * （{@code Dwdlm}）跟 {@code games/dwdlm/} 之外的任何东西都无关，摆在共用 cocos 包里等于让框架层
 * 常驻一个单游戏标识串（{@code check_framework_isolation.sh} 第 8 项会抓）。现搬进本游戏自己的包，
 * 接口与方法行为一字未改。
 *
 * <p><b>包名变化的外部契约（改 Java 必须同步，缺一个就静默失效）</b>：
 * <ul>
 *   <li>{@code app/src/main/jni/Games/DwdlmMod.cpp}：{@code LoadAppClass} / {@code FindClass}
 *       的 {@code "com.starry.magic.cocos.DwdlmHooks"} → {@code com.starry.magic.games.dwdlm.DwdlmHooks}；</li>
 *   <li>{@code 04_scripts/动物大联盟/}（menu_dex_build.json 的 keep 名单、build_menu_dex_dwdlm.sh /
 *       surgical_dwdlm_1.0.25.sh / shellless_menu_verify_1.0.25.py 的 smali 路径断言）
 *       —— 那几条属脚本执行者 scope，见本次交接报告的「脚本待改清单」。</li>
 * </ul>
 * R8 侧由两个规则文件的 {@code -keepnames class com.starry.magic.games.**} 覆盖（第 6 项校验）。 */
public final class DwdlmHooks {
    private static final String TAG = "Dwdlm";
    private static final boolean[] RET = new boolean[8];
    private static final int[] VAL = new int[8];

    private DwdlmHooks() {}

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
