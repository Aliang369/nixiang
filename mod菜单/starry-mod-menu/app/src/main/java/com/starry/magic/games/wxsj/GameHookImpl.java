package com.starry.magic.games.wxsj;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.core.TapTapWebLogin;

/**
 * 无限升级（{@code com.xingzuodaoshi.infinityUpgrade}，game_id = {@code wxsj}）—— TapTap 改签包强制内嵌网页登录。
 *
 * <p><b>为什么需要这个类</b>：本作是 TapTap 渠道包，TapSDK 的 Java 层（{@code com.taptap.sdk.*}、
 * {@code GatekeeperRepository}、{@code TapSignKit}、登录 Activity）<b>全部在网易易盾壳的 payload 里</b>，
 * APK 内没有可静态修改的 dex。星光包用 {@code CN=StarryMagic} 重签后，TapTap 客户端 SSO 的应用身份
 * 与后台登记不一致，必须把登录钉死在内嵌 WebView（官方 {@code WebLoginFragment}）。
 *
 * <p><b>可达性实测（13.101 / vc1650 底包 {@code 02_apks/无限升级_13.101.apk}，
 * 124,890,743 B / md5 23c3b6e9be50cc579de81f5c5a944e3b，2026-11-18）</b>：
 * 包内只有 <b>1 个</b> {@code classes.dex}（59,596 B / md5 4cfd813f1fcd2f5bf9076cf6c38a85cb），
 * 其类表只有 17 个游戏类且全是 {@code com/netease/nis/wrapper/*}（易盾壳）；
 * 在该 dex 上 {@code grep -a -c} 结果：{@code Lcom/taptap/sdk}=0、{@code GatekeeperRepository}=0、
 * {@code TapSignKit}=0、{@code InitializerServiceImpl}=0、{@code LoginActivityImpl}=0、
 * {@code DefaultTapLoginFragmentProvider}=0，连大小写不敏感的 {@code taptap}=0。
 * 旁证：{@code assets/nedata.db}（10,606,465 B）整段密文、无 dex magic；{@code assets/shellinfo} 记
 * 易盾 {@code shellversion=4.23.201.3}；{@code global-metadata.dat} 与 {@code libil2cpp.so} 内也搜不到
 * {@code taptap} 明文 ⇒ 真实 dex/元数据都在壳 payload，<b>改包法没有落点</b>，只能走运行时反射。
 *
 * <p><b>写在哪里</b>（2026-11-11 框架收口）：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。这是唯一允许出现单游戏逻辑的地方 ——
 * 不碰 {@code StarryApp} / {@code StarryInitProvider}，也不存在任何包名白名单
 * （{@code TapTapWebLogin} 里的白名单已于 2026-11-11 整体删除）。
 *
 * <p><b>为什么用 core.TapTapWebLogin 而不是旧的 sign.TapLoginForceWeb</b>（2026-11-18 lead 裁决）：
 * {@code core.TapTapWebLogin} 是 2026-11-11 框架收口后的<b>平台级能力</b>（文件头自述「框架级平台能力，零白名单，
 * 生效判据唯一＝谁的 GameHookImpl 调了本类」），且走<b>通用反射 + 双代路由</b>（3.x loginHandlers 数组 /
 * 4.x fragmentProvider 分流），<b>不依赖任何单游戏 request 类</b>；{@code sign.TapLoginForceWeb} 文件头自述
 * 「2026-10-28 状态：本类属旧机制，只服务还没迁移改包法的老包」。先例：{@code games/qxzxcz/GameHookImpl}
 * 明确写「新接入点不得再用」。
 *
 * <p><b>不做的事</b>：不复活 {@code SignSpoof}、不注入签名 killer、不篡改发往 TapTap 后端的包证书身份。
 * 签名侧由本作的既有链路 {@code SigReplace}（运行时 {@code PackageInfo} 签名替换，独立于菜单 SPI）负责。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}（两文件均已具备）。
 * 缺任一个，R8 会把本类改名成 {@code com.starry.magic.o/a}，反射装载必然 ClassNotFound、补丁静默失效。
 * 判据：logcat {@code game hook 已装载: com.starry.magic.games.wxsj.GameHookImpl}。
 */
public final class GameHookImpl implements GameHook {

    /** 无限升级 TapTap 渠道包名（13.101/vc1650 实测 manifest package）。 */
    private static final String PKG = "com.xingzuodaoshi.infinityUpgrade";

    /**
     * {@code Activity.onCreate} 之前 —— TapSDK 登录 Activity 首建时
     * {@code fragmentProvider.provide(request)}（4.x）或 handler 遍历（3.x）尚未执行，窗口正好：
     * 4.x 把 intent extra 里 {@code LoginRequest.loginType} 改成 {@code embedded_web}；
     * 3.x 把 handler 数组里的 {@code ClientLoginHandler} 换掉 ⇒ 都不拉起 TapTap 客户端。
     */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onPreCreated(activity);
    }

    /** {@code Activity.onCreate} 之后 —— 兜底：反射调 SDK 自带 {@code fallbackToWeb(request)}。 */
    @Override
    public void onActivityCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onCreated(activity);
    }

    /** 独立助手形态下告诉框架目标是哪个包（注入形态用不到，但保持与 SPI 契约一致）。 */
    @Override
    public String targetPackage() {
        return PKG;
    }

    /**
     * 自己的包自己判断 —— SPI 契约要求实现类能承受「不该被调到」的调用
     * （别的游戏也加载本类时不得产生副作用）。
     * <p>{@link TapTapWebLogin} 本身已无白名单，且内部再按目标 Activity 类名二次判空
     * （非 TapTap 登录 Activity 直接 return），故这里只需判包名。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
