package com.starry.magic.games.kyzt;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * 旷野征途（{@code com.xf.kyzt}，game_id = {@code kyzt}）—— TapTap 改签包强制内嵌网页登录。
 *
 * <p><b>为什么需要这个类</b>：本作 2.2/vc22 底包内 TapSDK 为 <b>4.10.9 新版路由</b>
 * （{@code com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity} →
 * {@code fragmentProvider.provide(request)} → {@code DefaultTapLoginFragmentProvider}
 * 按 {@code LoginRequest.loginType} 分流），客户端已装 TapTap 时默认走
 * {@code ClientLoginFragment}（客户端 SSO）。星光包重签后 SSO 路径的应用身份与后台登记不一致，
 * 必须把登录钉死在内嵌 WebView（官方 {@code WebLoginFragment}）。
 *
 * <p><b>证据（2.2 底包 {@code 02_apks/旷野征途/旷野征途_2.2_taptap_official.apk}，
 * sha256 8cb087ee…3e023，classes.dex 只读核对）</b>：新版路由类全部明文可搜
 * （{@code DefaultTapLoginFragmentProvider}=2、{@code TapTapLoginActivity}=3、
 * {@code WebLoginFragment}=8、{@code ClientLoginUtils}=2，旧版 {@code ClientLoginHandler}=0）；
 * {@link TapLoginForceWeb} 依赖的反射目标全部命中
 * （{@code com_taptap_sdk_login_request}=1、{@code com_taptap_sdk_login_fragment}=1、
 * {@code embedded_web}=1、{@code fallbackToWeb}=1）⇒ 走「包内 TapSDK 可达」分支的运行时强制网页，
 * 不需要静态改 {@code classes.dex} 的登录入口。
 *
 * <p><b>写在哪里</b>（2026-11-11 框架收口）：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。这是唯一允许出现单游戏逻辑的地方 ——
 * 不碰 {@code StarryApp} / {@code StarryInitProvider}，也不存在任何包名白名单
 * （{@code TapLoginForceWeb} 里的白名单已于 2026-11-11 整体删除）。
 *
 * <p><b>不做的事</b>（2026-11-14 本轮边界）：不做签名伪装、不注入签名 killer、
 * 不篡改发往 TapTap 后端的包证书身份或认证结果。若网页链仍因「重签后应用身份未在开发者后台登记」
 * 被拒，只记录返回码与配置缺口，不用伪造官方身份绕过。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}（两个文件均已具备，无需改动）。
 * 缺任一个，R8 会把本类改名成 {@code com.starry.magic.o/a}，反射装载必然 ClassNotFound、补丁静默失效。
 * {@code check_framework_isolation.sh} 会校验两个规则文件一致。
 */
public final class GameHookImpl implements GameHook {

    /** 旷野征途 TapTap 渠道包名（2.2/vc22 实测 manifest package）。 */
    private static final String PKG = "com.xf.kyzt";

    /**
     * {@code Activity.onCreate} 之前 —— {@code TapTapLoginActivity} 首建时
     * {@code fragmentProvider.provide(request)} 尚未执行，窗口正好：
     * 把 intent extra 里的 {@code LoginRequest.loginType} 改成 {@code embedded_web}，
     * 首建直接走 {@code WebLoginFragment}，不拉起 TapTap 客户端。
     */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapLoginForceWeb.onPreCreated(activity);
    }

    /**
     * {@code Activity.onCreate} 之后 —— 兜底：若当前 fragment 不是 Web 登录，
     * 反射调 SDK 自带的 {@code fallbackToWeb(request)}；并处理 API&lt;29 的旧实现清洗。
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
     * <p>{@link TapLoginForceWeb} 本身已无白名单，且内部再按目标 Activity 类名二次判空
     * （非 {@code TapTapLoginActivity} 直接 return），故这里只需判包名。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
