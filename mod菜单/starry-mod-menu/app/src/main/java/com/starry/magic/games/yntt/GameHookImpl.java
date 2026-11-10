package com.starry.magic.games.yntt;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * 一念通天（{@code com.qinglv.Immortal}，game_id = {@code yntt}）—— TapTap 改签包强制内嵌网页登录。
 *
 * <p><b>为什么需要这个类</b>：本作是 TapSDK 3.30.2 旧版路由
 * （{@code com.taptap.sdk.ui.TapTapActivity} → {@code LoginClient.sendLoginRequest} →
 * 已装 TapTap 客户端时先走 ActivityHandler）。星光包用 {@code CN=StarryMagic} 重签，
 * 客户端 SSO 会因签名不符被拒，必须把登录钉死在内嵌 WebView。
 *
 * <p><b>为什么写在这里而不是别处</b>（2026-11-11 框架收口，见 {@code core/GameHook} 类头）：
 * <ul>
 *   <li>不能改 {@code StarryApp} / {@code StarryInitProvider}：框架层不许出现单游戏逻辑，
 *       {@code check_framework_isolation.sh} 会挡。</li>
 *   <li>不能往 {@code TapLoginForceWeb.PKG_WHITELIST} 加包名：那份白名单**只减不增**，
 *       基线 47 条由同一脚本校验，变多即 FAIL。</li>
 *   <li>不能塞 {@code core/legacy/LegacyTapTapGames.GAME_IDS}：那是存量兼容垫片，
 *       同样只减不增（基线 8）。</li>
 * </ul>
 * 规范路径就是本类：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。
 *
 * <p><b>依赖的前置条件</b>：{@code app/proguard-rules.pro} 必须有
 * {@code -keepnames class com.starry.magic.games.**}，否则 R8 会把本类改名，
 * 反射装载永远 ClassNotFound、补丁静默失效（784 轮实测踩过）。
 *
 * <p><b>内部白名单仍在生效</b>：{@link TapLoginForceWeb} 的 5 个入口各自还会按
 * {@code PKG_WHITELIST.contains(getPackageName())} 自检；{@code com.qinglv.Immortal}
 * 在该表内（历史遗留，属 47 条基线内），所以调用链是通的。等框架收口把白名单整体删掉时，
 * 这里要同步改成本类自行完成反射改写。
 */
public final class GameHookImpl implements GameHook {

    /** 一念通天 TapTap 渠道包名。 */
    private static final String PKG = "com.qinglv.Immortal";

    /**
     * {@code Activity.onCreate} 之前 —— TapTapActivity 的 handler 遍历尚未发生，窗口正好。
     * 旧版路径在这里把 intent extra 的 request 换成 {@code YnttLoginRequest}，
     * 其 {@code getRequestCode()} 抛异常，逼 {@code LoginClient} 回退到官方 {@code WebViewHandler}。
     */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapLoginForceWeb.onPreCreated(activity);
    }

    /**
     * {@code Activity.onCreate} 之后 —— 新版 {@code fallbackToWeb} 兜底与 API&lt;29 清洗。
     */
    @Override
    public void onActivityCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapLoginForceWeb.onCreated(activity);
    }

    /** 独立助手形态下告诉框架目标是哪个包（注入形态用不到，但保持与 SPI 契约一致）。 */
    @Override
    public String targetPackage() {
        return PKG;
    }

    /**
     * 自己的包自己判断 —— SPI 契约要求实现类能承受「不该被调到」的调用
     * （别的游戏也加载本类时不得产生副作用）。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
