package com.starry.magic.games.dwct;

import android.app.Application;

import com.starry.magic.cocos.DwctIapGate;
import com.starry.magic.core.GameHook;

/**
 * 动物餐厅 —— IAP 闸口补丁（原在 {@code StarryApp.attachBaseContext} 里无条件执行）。
 *
 * <p>行为不变：{@link DwctIapGate#apply(Application)} 内部仍按包名
 * {@code inland.twgame.restaurant} 自检；这里只负责「什么时候调」，不再让
 * 每个游戏的启动路径都去碰它。
 */
public final class GameHookImpl implements GameHook {

    /** 动物餐厅（TapTap 原版）。仅独立助手形态用到。 */
    private static final String PKG = "inland.twgame.restaurant";

    @Override
    public void onAttachBaseContext(Application app) {
        DwctIapGate.apply(app);
    }

    @Override
    public String targetPackage() {
        return PKG;
    }
}
