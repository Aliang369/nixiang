package com.starry.magic.games.hxwj;

import com.starry.magic.core.GameHook;

/**
 * 幻想武将模拟器（game_id = {@code hxwj}）——SecShell 别名 so 形态。
 *
 * <p>原逻辑写死在 {@code ota/StarryNativeLoad.tryLoadPackagedAlias()} 里：
 * {@code "com.qxjc.hxwjmlq".equals(ctx.getPackageName())}。那条路径之所以存在，是因为
 * SecShell 壳的 APK 不接受新增 {@code libstarry_magic.so} 条目，只能把 so 塞进包内一个
 * 既有 x86-only 的 native 槽位，再让 arm64 设备永远走不到的正常路径去别名加载。
 *
 * <p>2026-11-11 迁到这里：框架不再知道任何具体游戏，只问当前游戏「你要不要别名加载」。
 */
public final class GameHookImpl implements GameHook {

    private static final String PKG = "com.qxjc.hxwjmlq";

    @Override
    public String targetPackage() {
        return PKG;
    }

    @Override
    public boolean usesPackagedSoAlias() {
        return true;
    }
}
