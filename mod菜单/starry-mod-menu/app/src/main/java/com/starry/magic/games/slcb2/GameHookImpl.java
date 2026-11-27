package com.starry.magic.games.slcb2;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * 失落城堡2（{@code com.leiting.slcb2}，game_id = {@code slcb2}）—— TapTap 改签包强制内嵌网页登录。
 *
 * <p><b>为什么需要这个类</b>：本作是雷霆渠道包，包内自带 TapTap SDK，但 dex 由易盾壳在运行时
 * 以 InMemoryDexFile 解密，签名侧只能用 MT killer（官方证书烘焙）过 TapSDK 的签名校验。
 * 星光包已用 {@code CN=StarryMagic} 重签，客户端 SSO 仍会因签名不符被拒，
 * 必须把登录钉死在内嵌 WebView。
 *
 * <p><b>写在哪里</b>（2026-11-11 框架收口）：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。这是唯一允许出现单游戏逻辑的地方 ——
 * 不碰 {@code StarryApp} / {@code StarryInitProvider}，也不存在任何包名白名单
 * （{@code TapLoginForceWeb} 里的白名单已于 2026-11-11 整体删除）。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}，否则 R8 改名后反射装载必失败。
 * {@code check_framework_isolation.sh} 会校验两个规则文件一致。
 */
public final class GameHookImpl implements GameHook {

    /** 失落城堡2 TapTap 渠道包名。 */
    private static final String PKG = "com.leiting.slcb2";

    /** {@code Activity.onCreate} 之前 —— handler 遍历尚未发生，窗口正好。 */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapLoginForceWeb.onPreCreated(activity);
    }

    /** {@code Activity.onCreate} 之后 —— 新版 fallbackToWeb 兜底与 API&lt;29 清洗。 */
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
