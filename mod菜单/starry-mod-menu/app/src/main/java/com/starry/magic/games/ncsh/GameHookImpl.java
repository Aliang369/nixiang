package com.starry.magic.games.ncsh;

import com.starry.magic.core.GameHook;

/**
 * 农场守护者（game_id = {@code ncsh}）—— 框架 SPI 落地的<b>参考实现</b>。
 *
 * <p>本类是新游戏接入的样板：新游戏只需要
 * <ol>
 *   <li>建包 {@code com.starry.magic.games.<game_id>}，放一个同名 {@code GameHookImpl}；</li>
 *   <li>按需覆写下面几个回调（都用不到就整个类不写 —— 框架会退回空实现）；</li>
 *   <li><b>不要</b>去改 {@code StarryApp} / {@code StarryInitProvider} / {@code MenuService}。</li>
 * </ol>
 *
 * <p>农场守护者目前<b>不需要</b>任何 Java 侧补丁：
 * <ul>
 *   <li>无 TapTap 门闩（官方原版直装，未改签）⇒ 不需要 {@code TapLoginForceWeb}；</li>
 *   <li>无 APKSecure 壳 ⇒ 不需要 {@code PluginHelper} 回填；</li>
 *   <li>无 IAP 校验需要绕过 ⇒ 不需要 {@code DwctIapGate}。</li>
 * </ul>
 * 所以这里只有 {@link #targetPackage()}（独立助手形态下告诉框架目标是谁）。
 * 它的存在本身就是示范：<b>「什么都不做」也应该有一个显式的、属于本游戏的类</b>，
 * 而不是让框架替它猜。
 */
public final class GameHookImpl implements GameHook {

    /** 农场守护者（TapTap 官方 2.1.21）。 */
    private static final String PKG = "com.qld.ncsh.xiyou";

    @Override
    public String targetPackage() {
        return PKG;
    }
}
