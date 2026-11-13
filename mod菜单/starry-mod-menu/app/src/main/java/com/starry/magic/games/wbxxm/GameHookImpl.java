package com.starry.magic.games.wbxxm;

import com.starry.magic.core.GameHook;

/**
 * 吾辈小熊猫（game_id = {@code wbxxm}，com.wbxxm.wbxxm 5.0 TapTap 渠道）。
 *
 * <p>本作的 Java 侧差异**全部落在静态改包**里，不在运行时拦截，所以这里只登记目标包名：
 * <ul>
 *   <li><b>签名</b>：B 轨 killer dex（{@code classes8.dex} 的
 *       {@code bin.mt.signature.KillerApplication193}）—— 宿主的
 *       {@code com.xipu.msdk.XiPuSDKApplication} 父类在静态 smali 里改成了 killer，
 *       运行期 {@code PackageInfo.CREATOR} 层回灌官方证书；TapTap 的 {@code TapSignKit}
 *       与喜扑的 {@code SOPackageUtil.MarketP} 两个消费者一起归零。
 *       ⇒ 不需要 {@code TapLoginForceWeb} 的包名白名单，也<b>不得</b>复活 {@code SignSpoof}。</li>
 *   <li><b>验签门闩</b>：{@code GatekeeperRepository.isGatekeeperLoadedSuccessfully()} 恒 true、
 *       {@code showErrorToast}/{@code showGatekeeperError} 首行 return-void、
 *       {@code InitializerServiceImpl.checkInitialize(Z)} 恒 true（静态 smali）。</li>
 *   <li><b>强制网页登录</b>：{@code ClientLoginUtils.isClientLogin(Activity, Actions)} 恒 false
 *       （静态 smali，2026-10-28 现行口径「改包法」）。</li>
 * </ul>
 *
 * <p>为什么不用 {@code TapLoginForceWeb}：那是白名单+运行时伪装时代的产物，
 * 白名单会随游戏越加越长（正是 2026-11-11 框架收口要消灭的形态）；
 * 本作 killer 已在 Application 层接管，静态改包是更窄的那条路。
 *
 * <p>本类存在还有一个作用：框架 {@code GameHooks.get()} 用
 * {@code Class.forName("com.starry.magic.games.wbxxm.GameHookImpl")} 反射装载，
 * 类在就说明「本游戏的 Java 侧确实被注入脚本拷进来了」，缺了会静默退回空实现。
 */
public final class GameHookImpl implements GameHook {

    /** 吾辈小熊猫（TapTap 官包 5.0，versionCode 5）。 */
    private static final String PKG = "com.wbxxm.wbxxm";

    @Override
    public String targetPackage() {
        return PKG;
    }
}
