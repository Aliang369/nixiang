package com.starry.magic.games.qxzxcz;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.core.TapTapWebLogin;

/**
 * 群侠传：幸存者（{@code com.nenlystudio.qunxiasurvivor.cn}，game_id = {@code qxzxcz}）——
 * TapTap 渠道包强制内嵌网页登录。
 *
 * <p><b>为什么需要这个类</b>：本作 0.2.3.10 包内 TapSDK 4.11.0 为<b>新版路由</b>
 * （{@code com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity} →
 * {@code fragmentProvider.provide(request)} → {@code DefaultTapLoginFragmentProvider} 按
 * {@code LoginRequest.loginType} 分流）。设备装了 TapTap 客户端时默认走
 * {@code ClientLoginFragment}（客户端 SSO）；星光包用 {@code CN=StarryMagic} 重签后，
 * 客户端 SSO 带回的应用身份与开发者后台登记不一致，必然被拒 ⇒ 必须把登录钉死在内嵌
 * WebView（官方 {@code WebLoginFragment}），不拉起 TapTap 客户端。
 *
 * <p><b>证据（0.2.3.10 官包 {@code 02_apks/群侠传：幸存者/群侠传：幸存者_0.2.3.10_official_from_device.apk}，
 * classes.dex 8,016,148 B / md5 {@code f88f4f3f37500d46a1a59eb7c7de0849}，只读核对）</b>：
 * 新路由锚点全在（{@code TapTapLoginActivity}=1、{@code DefaultTapLoginFragmentProvider}=3、
 * {@code WebLoginFragment}=21、{@code ClientLoginFragment}=11），旧路由
 * {@code com.taptap.sdk.ui.TapTapActivity}=0、{@code ClientLoginHandler}=0；
 * {@link TapTapWebLogin} 依赖的反射目标也全部命中
 * （{@code com_taptap_sdk_login_request}=1、{@code com_taptap_sdk_login_fragment}=1、
 * {@code embedded_web}=1、{@code fallbackToWeb}=1）⇒ 走「运行时强制网页」这条路可行，
 * 不需要静态改游戏 dex 的登录入口。
 *
 * <p><b>为什么是 {@link TapTapWebLogin} 而不是旧的 {@code sign.TapLoginForceWeb}</b>：
 * {@code core.TapTapWebLogin} 是 2026-11-11 框架收口后的<b>平台级能力</b>，住在 {@code core/}、
 * <b>零包名白名单</b>，生效判据唯一＝「谁的 {@code games/<game_id>/GameHookImpl} 调了它」；
 * {@code sign.TapLoginForceWeb} 是收口前的旧机制（自带单游戏 request 类常量、注释里已写明
 * 「只服务还没迁移改包法的老包」），新接入点不得再用。
 *
 * <p><b>本类只解决「登录路由」</b>：签名校验<b>不在</b>本类，而在游戏 dex 侧 ——
 * {@code com.taptap.sdk.kit.internal.utils.TapSignKit.getSign} 回灌官方 cert.der 指纹
 * （{@code MD5 6b013f42541e222db7f2153a138ab301}）、
 * {@code GatekeeperRepository}/{@code TapTapSdkInitializer} 的验签门闩恒放行，
 * 见 {@code 10_docs/群侠传：幸存者/登录改包方案.md} §3 四步锚点。
 * 本类<b>不做</b>运行时签名伪装、不复用已删除的 {@code SignSpoof}、不伪造官方身份。
 *
 * <p><b>写在哪里</b>（2026-11-11 框架收口）：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。这是唯一允许出现单游戏逻辑的地方 ——
 * 不碰 {@code StarryApp} / {@code StarryInitProvider}，也不存在任何包名白名单。
 *
 * <p><b>覆盖范围（重要，别夸大）</b>：{@link TapTapWebLogin} 内部每一步都先比对
 * {@code activity.getClass().getName()} 是否为 TapTap 的登录 Activity，类名不匹配即空转；
 * 因此本类<b>只对 TapTap SDK 自己的登录 Activity 生效</b>。游戏 GDScript 侧若另有独立登录/校验
 * （{@code assets/scripts/autoload/platform/taptap_android_backend.gd}），本类覆盖不到；
 * SDK 若升级换掉登录 Activity 类名，本类同样静默失效（改 {@code core.TapTapWebLogin} 的类名常量，
 * <b>不要</b>在本类里加第二套判据）。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}（两个文件均已具备）。
 * 缺任一个，R8 会把本类改名成 {@code com.starry.magic.o/a}，反射装载 ClassNotFound、补丁静默失效。
 * {@code check_framework_isolation.sh} 会校验两个规则文件一致。
 */
public final class GameHookImpl implements GameHook {

    /** 群侠传：幸存者 TapTap 中国渠道包名（0.2.3.10 官包 manifest 实测）。 */
    private static final String PKG = "com.nenlystudio.qunxiasurvivor.cn";

    /**
     * {@code Activity.onCreate} 之前 —— {@code TapTapLoginActivity} 首建时
     * {@code fragmentProvider.provide(request)} 尚未执行，窗口正好：
     * 把 intent extra 里 {@code LoginRequest.loginType} 改成 {@code embedded_web}，
     * 首建直接走 {@code WebLoginFragment}，不拉起 TapTap 客户端。
     * <p>用单参重载：本作无旧版路由（{@code com.taptap.sdk.ui.TapTapActivity}=0），
     * 不需要任何单游戏 request 类。
     */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onPreCreated(activity);
    }

    /**
     * {@code Activity.onCreate} 之后 —— 兜底：若当前 fragment 已是 Web 登录则跳过，
     * 否则反射调 SDK 自带的 {@code fallbackToWeb(request)}；
     * API&lt;29 的旧实现清洗一并由平台类处理（本作不适用）。
     */
    @Override
    public void onActivityCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onCreated(activity);
    }

    /** 独立助手形态下告诉框架目标是哪个包（注入形态用不到，但保持 SPI 契约一致）。 */
    @Override
    public String targetPackage() {
        return PKG;
    }

    /**
     * 自己的包自己判断 —— SPI 契约要求实现类能承受「不该被调到」的调用
     * （别的游戏也加载本类时不得产生副作用）。
     * <p>{@link TapTapWebLogin} 自身已无白名单，这里是<b>唯一</b>的生效判据。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
