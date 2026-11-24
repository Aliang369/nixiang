package com.starry.magic.games.wdyz;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.core.TapTapWebLogin;

/**
 * 我的勇者（{@code com.rsg.wdyz}，game_id = {@code wdyz}）—— TapTap 渠道包强制内嵌网页登录。
 *
 * <p><b>B 轨定位（本类只做一件事）</b>：把 TapTap 登录钉死在 SDK 自带的内嵌 WebView
 * （{@code WebLoginFragment}），不拉起 TapTap 客户端 SSO。签名/验签门闩<b>不在本类</b>，
 * 由 A 轨（killer dex / 静态改包）负责；本类<b>不含</b>任何签名伪装、不复活已删除的
 * {@code SignSpoof}、不伪造官方身份。
 *
 * <p><b>为什么走 SPI 而不是改包法</b>：本作 7.6.5 底包是网易易盾（NetEase NIS）壳，
 * 包内只有 1 个 {@code classes.dex}，manifest 引用的 {@code com.taptap.*} 类在 dex 里
 * <b>不存在</b>（运行时解密装载，见 {@code 10_docs/我的勇者/分析_战斗_20260906.md} §P1
 * 「manifest 引用的 alipay/anythink/TapTap 类在 DEX 中不存在」）⇒ 静态改 dex 的登录入口不可行，
 * 按 {@code skill/re-analyze/login/taptap-web-login.md} 顶部「现行口径」二选一表，落到运行时 SPI。
 *
 * <p><b>选哪条 core 路径（7.6.5 manifest 实测）</b>：底包
 * {@code 02_apks/我的勇者/我的勇者_7.6.5_base.apk} 的 AndroidManifest.xml 里登录 Activity 名是
 * {@code com.taptap.sdk.login.ui.TapTapLoginActivity}，即 {@link TapTapWebLogin} 的
 * {@code OLD_LOGIN_ACTIVITY} 常量（{@code core/TapTapWebLogin.java:50}），<b>不是</b>新版
 * {@code impl.ui.activity.TapTapLoginActivity}（同文件 L48）。因此：
 * <ul>
 *   <li>生效手段＝{@code sanitize()} 把 {@code impl.loginHandlers} 里的
 *       {@code ClientLoginHandler}/{@code PhoneLoginHandler} 换成 canHandle 恒 false 的代理
 *       → onCreate 遍历全不命中 → 落 SDK 自带 fallbackToWeb（WebLoginFragment）；</li>
 *   <li>单参重载足够：{@code forceOldImplWebEarly} 那条「换旧版 request 类」的路需要一款游戏
 *       自己的 Parcelable 类，本作没有也不需要。日志
 *       {@code StarryTapLogin: old-impl no legacy request class supplied, skip} 是<b>预期</b>，
 *       不是缺陷。</li>
 * </ul>
 *
 * <p><b>历史实测（7.6.1 / vc203，同一渠道包，设备 OnePlus OPD2404 Android 14）</b>：
 * {@code logcat -s StarryTapLogin} 出现 {@code TapTapLoginActivity sanitized=true noopProxy=true}，
 * 随后 {@code AuthClient /v1/auth/login → 200}；同批日志里
 * {@code forceOldImplWeb failed (NoSuchField request)} 属预期（该 SDK 无 {@code request} 字段）。
 * 见 {@code 10_docs/我的勇者/evidence_20260920/README.md} 第 8 项。
 *
 * <p><b>覆盖范围（别夸大）</b>：{@link TapTapWebLogin} 每一步先比对
 * {@code activity.getClass().getName()}，类名不匹配即空转 ⇒ 本类只对 TapTap SDK 自己的登录
 * Activity 生效；游戏侧另有的登录/授权链路覆盖不到。SDK 换掉登录 Activity 类名后本类静默失效
 * （那时改 {@code core.TapTapWebLogin} 的类名常量，<b>不要</b>在本类里加第二套判据）。
 *
 * <p><b>边界</b>：{@code TapTapWebLogin} 已无包名白名单（2026-11-11 删），生效判据唯一＝
 * 「谁的 {@code games/<game_id>/GameHookImpl} 调了它」⇒ 目标判断写在本类
 * {@link #isTarget(Activity)} 里。不碰 {@code StarryApp} / {@code StarryInitProvider}，
 * 不新增任何白名单。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}（两文件均已具备，L80/L79）。
 * 缺任一个，R8 会把本类改名成 {@code com.starry.magic.o/a}，反射装载 ClassNotFound、补丁静默失效。
 * 两文件一致性由 {@code 04_scripts/_common/check_framework_isolation.sh} 第 6 项机器校验。
 */
public final class GameHookImpl implements GameHook {

    /** 我的勇者 TapTap 渠道包名（7.6.5 底包 manifest 与 {@code GameConfig.h} STARRY_GAME_WDYZ 一致）。 */
    private static final String PKG = "com.rsg.wdyz";

    /**
     * {@code Activity.onCreate} 之前 —— 旧版 TapTapActivity 的 handler 遍历与
     * {@code LoginClient.sendLoginRequest} 都还没发生，窗口正好：由平台类把
     * {@code loginHandlers} 清洗成「一个都不 canHandle」，首建直接落 fallbackToWeb。
     */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onPreCreated(activity);
    }

    /**
     * {@code Activity.onCreate} 之后 —— 兜底：当前 fragment 仍非 {@code WebLoginFragment} 时
     * 反射调 {@code impl.fallbackToWeb(activity, request)}。
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
     * （别的游戏也加载本类时不得产生副作用）。这是<b>唯一</b>的生效判据。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
