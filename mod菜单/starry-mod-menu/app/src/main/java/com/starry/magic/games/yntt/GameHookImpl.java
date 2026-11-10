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
 * <p><b>写在哪里</b>（2026-11-11 框架收口）：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。这是唯一允许出现单游戏逻辑的地方 ——
 * 不碰 {@code StarryApp} / {@code StarryInitProvider}，也不存在任何包名白名单
 * （{@code TapLoginForceWeb} 里的白名单已于 2026-11-11 整体删除）。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}。缺任一个，R8 会把本类改名成
 * {@code com.starry.magic.o/a}，反射装载必然 ClassNotFound、补丁静默失效
 * （784 轮实测踩过：dex 里 {@code games/} 命中 0、{@code magic/o/} 命中 154）。
 * {@code check_framework_isolation.sh} 会校验两个规则文件一致。
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

    /** {@code Activity.onCreate} 之后 —— 新版 {@code fallbackToWeb} 兜底与 API&lt;29 清洗。 */
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
     * <p>这是<b>唯一</b>的生效判据：{@link TapLoginForceWeb} 本身已无白名单。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
